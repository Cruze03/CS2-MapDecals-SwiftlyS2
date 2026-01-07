using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Translation;

namespace MapDecals.Menus;

// =========================================
// =           PLACE DECAL MENU
// =========================================

/// <summary>place decal menu</summary>
public sealed partial class MenuManager
{
    internal static class EditDecalsMenu
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
            var steamid = player.SteamID;

            var menuBuilder = Plugin.Core.MenusAPI
                .CreateBuilder()
                .Design.SetMenuTitle(localizer["cc.menu.editdecals.title"])
                .Design.SetMenuTitleVisible(true)
                .Design.SetMenuFooterVisible(true)
                .Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
                .SetPlayerFrozen(false);

            foreach (var decal in Plugin._mapDecals)
            {
                menuBuilder.AddOption(new SubmenuMenuOption(
                    decal.DecalName,
                    () => EditDecalMenu.Build(menuManager, player, localizer, decal)
                ));
            }

            return menuBuilder.Build();
        }
    }
}