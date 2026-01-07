using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Translation;

namespace MapDecals.Menus;

// =========================================
// =           DECAL MAIN MENU
// =========================================

/// <summary>Decal main menu</summary>
public sealed partial class MenuManager
{
    internal static class DecalMenu
    {
        internal static void Show(MenuManager menuManager, IPlayer player, ILocalizer localizer)
        {
            var menu = Build(menuManager, player, localizer);
            Plugin.Core.MenusAPI.OpenMenuForPlayer(player, menu);
        }

        // =========================================
        // =           BUILD MENU
        // =========================================

        internal static IMenuAPI Build(MenuManager menuManager, IPlayer player, ILocalizer localizer)
        {
            var menuBuilder = Plugin.Core.MenusAPI
                .CreateBuilder()
                .Design.SetMenuTitle(localizer["cc.menu.mainmenu.title"])
                .Design.SetMenuTitleVisible(true)
                .Design.SetMenuFooterVisible(true)
                .Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
                .SetPlayerFrozen(false);

            menuBuilder.AddOption(new SubmenuMenuOption(
                localizer["cc.menu.placedecal.option"],
                () => PlaceDecalMenu.Build(menuManager, player, localizer)
            ));
            if (Plugin._mapDecals.Count() > 0)
                menuBuilder.AddOption(new SubmenuMenuOption(
                    localizer["cc.menu.editdecal.option"],
                    () => EditDecalsMenu.Build(menuManager, player, localizer)
                ));

            return menuBuilder.Build();
        }
    }
}