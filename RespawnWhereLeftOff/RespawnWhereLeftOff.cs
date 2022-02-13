namespace RespawnWhereLeftOff;
public class RespawnWhereLeftOff : Mod, ILocalSettings<SaveSettings>
{
    internal static RespawnWhereLeftOff Instance;
    private static bool shouldIgnoreNormalRespawn = false;
    
    public static SaveSettings saveSettings { get; set; } = new SaveSettings();
    public void OnLoadLocal(SaveSettings s) => saveSettings = s;
    

    public SaveSettings OnSaveLocal() => saveSettings;

    public override string GetVersion() => AssemblyUtils.GetAssemblyVersionHash();

    public override void Initialize()
    {
        Instance ??= this;
        On.HeroController.Respawn += RespawnWhereLeftOffPlease;
        ModHooks.SavegameLoadHook += NewGameLoading;
        ModHooks.BeforeSavegameSaveHook += SavePosition;
        IL.GameManager.ReadyForRespawn += ChangeRespawnScene;
    }

    private void ChangeRespawnScene(ILContext il)
    {
        ILCursor cursor = new ILCursor(il).Goto(0);
        
        if (cursor.TryGotoNext(MoveType.After, 
                i => i.MatchCallvirt<PlayerData>("GetString")))
        {
            cursor.EmitDelegate<Func<string,string>>((oldScene) =>
            {
                Log(oldScene + " " + saveSettings.respawnScene);
                if (shouldIgnoreNormalRespawn && saveSettings.RespawnPoint != Vector3.zero && saveSettings.respawnScene != null)
                {
                    return saveSettings.respawnScene;
                }

                return oldScene;
            });
        }
    }


    private void SavePosition(SaveGameData saveGameData)
    {
        saveSettings.respawnScene = GameManager.instance.GetSceneNameString();
        saveSettings.RespawnPoint = Ref.HC.hero_state is ActorStates.running or ActorStates.idle ?
            HeroController.instance.transform.position : 
            PlayerData.instance.hazardRespawnLocation;
    }

    private void NewGameLoading(int obj)
    {
        shouldIgnoreNormalRespawn = true;
    }

    private static readonly FastReflectionDelegate ResetMotion = typeof(HeroController).GetMethod("ResetMotion", BindingFlags.Instance | BindingFlags.NonPublic).GetFastDelegate();
    private static readonly FastReflectionDelegate ResetAttacks = typeof(HeroController).GetMethod("ResetAttacks", BindingFlags.Instance | BindingFlags.NonPublic).GetFastDelegate();
    private static readonly FastReflectionDelegate ResetInput = typeof(HeroController).GetMethod("ResetInput", BindingFlags.Instance | BindingFlags.NonPublic).GetFastDelegate();
    private static readonly FastReflectionDelegate FinishedEnteringScene = typeof(HeroController).GetMethod("FinishedEnteringScene", BindingFlags.Instance | BindingFlags.NonPublic).GetFastDelegate();
    private IEnumerator RespawnWhereLeftOffPlease(On.HeroController.orig_Respawn orig, HeroController self)
    {
        if (shouldIgnoreNormalRespawn)
        {
            shouldIgnoreNormalRespawn = false;
            if (saveSettings.RespawnPoint != Vector3.zero && saveSettings.respawnScene != null)
            {
                var renderer = ReflectionHelper.GetField<HeroController, MeshRenderer>(self, "renderer");
                var rb2d = ReflectionHelper.GetField<HeroController, Rigidbody2D>(self, "rb2d");
                var animCtrl = ReflectionHelper.GetField<HeroController, HeroAnimationController>(self, "animCtrl");
                self.playerData = PlayerData.instance;
                self.playerData.disablePause = true;
                self.gameObject.layer = 9;
                renderer.enabled = true;
                rb2d.isKinematic = false;
                self.cState.dead = false;
                self.cState.onGround = true;
                self.cState.hazardDeath = false;
                self.cState.recoiling = false;
                ReflectionHelper.SetField(self, "enteringVertically", false);
                ReflectionHelper.SetField(self, "airDashed", false);
                ReflectionHelper.SetField(self, "doubleJumped", false);
                self.CharmUpdate();
                self.MaxHealth();
                self.ClearMP();
                ResetMotion(self);
                self.ResetHardLandingTimer();
                ResetAttacks(self);
                ResetInput(self);
                self.CharmUpdate();
                
                ReflectionHelper.SetField(GameManager.instance.cameraCtrl, "isGameplayScene", true);
                GameManager.instance.cameraCtrl.PositionToHero(false);
                self.transform.SetPosition2D(self.FindGroundPoint(saveSettings.RespawnPoint));

                GameCameras.instance.cameraFadeFSM.SendEvent("RESPAWN");

                self.isHeroInPosition = true;

                float clipDuration = animCtrl.GetClipDuration("Wake Up Ground");
                animCtrl.PlayClip("Wake Up Ground");
                self.StopAnimationControl();
                self.controlReqlinquished = true;
                yield return new WaitForSeconds(clipDuration);
                self.StartAnimationControl();
                self.controlReqlinquished = false;
                self.proxyFSM.SendEvent("HeroCtrl-Respawned");
                FinishedEnteringScene(self);
                self.playerData.disablePause = false;
                self.playerData.isInvincible = false;
                yield return new WaitForSeconds(0.5f);
                Ref.PD.disablePause = false;
                yield break;
            }
        }

        yield return orig(self);
      
    }
}