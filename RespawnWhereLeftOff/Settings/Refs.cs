namespace RespawnWhereLeftOff;
internal static class Ref
{
    private static GameManager _gm;
    private static InputHandler _ih;
    private static HeroController _hc;
    private static GameObject _knight;
    private static PlayerData _pd;
    private static SceneData _sd;

    internal static PlayerData PD => _pd ??= PlayerData.instance;
    internal static SceneData SD => _sd ??= SceneData.instance;
    internal static GameManager GM => _gm ??= GameManager.instance;
    internal static InputHandler IH => _ih ??= InputHandler.Instance;
    internal static HeroController HC => _hc ??= HeroController.instance;
    internal static GameObject Knight => _knight ??= HC.gameObject; 
}