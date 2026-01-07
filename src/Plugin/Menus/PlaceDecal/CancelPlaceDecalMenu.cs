using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Translation;

namespace MapDecals.Menus;

// =========================================
// =           CANCEL PLACE DECAL MENU
// =========================================

/// <summary>cancel place decal menu</summary>
public sealed partial class MenuManager
{
    internal static class CancelPlaceDecalMenu
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
                .Design.SetMenuTitle(localizer["cc.menu.placedecal.option"])
                .Design.SetMenuTitleVisible(true)
                .Design.SetMenuFooterVisible(true)
                .Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
                .SetPlayerFrozen(false);

            var cancelbutton = new ButtonMenuOption(
                localizer["cc.menu.cancelplacedecal.cancel"]
            );

            cancelbutton.Click += (sender, args) =>
            {
                var steamid = args.Player.SteamID;
                if (!Plugin._applyingDecal.TryAdd(steamid, null))
                {
                    Plugin._applyingDecal[steamid] = null;
                }
                menuManager.OpenMainMenu(player, localizer);
                return ValueTask.CompletedTask;
            };

            menuBuilder.AddOption(cancelbutton);

            return menuBuilder.Build();
        }
    }
}