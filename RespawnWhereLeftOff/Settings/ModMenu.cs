namespace RespawnWhereLeftOff;

public static class ModMenu
{
    private static Menu MenuRef;

    public static MenuScreen CreateMenuScreen(MenuScreen modListMenu)
    {
        MenuRef = new Menu("Mod Menu", new Element[]
        {
			//TODO: Implement Menu
        });
        
        return MenuRef.GetMenuScreen(modListMenu);
    }
}