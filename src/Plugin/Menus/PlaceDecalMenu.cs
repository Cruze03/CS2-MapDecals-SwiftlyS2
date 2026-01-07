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
    internal static class PlaceDecalMenu
    {
        // =========================================
        // =           BUILD MENU
        // =========================================

        internal static IMenuAPI Build(MenuManager menuManager, IPlayer player, ILocalizer localizer)
        {
            var steamid = player.SteamID;

            var menuBuilder = Plugin.Core.MenusAPI
                .CreateBuilder()
                .Design.SetMenuTitle(localizer["cc.menu.placedecal.title"])
                .Design.SetMenuTitleVisible(true)
                .Design.SetMenuFooterVisible(true)
                .Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
                .SetPlayerFrozen(false);

            foreach (var prop in menuManager._config.Props)
            {
                var button = new ButtonMenuOption(
                    prop.Name
                );
                button.Click += (sender, args) =>
                {
                    var steamid = player.SteamID;
                    if (!Plugin._applyingDecal.TryAdd(steamid, prop))
                    {
                        Plugin._applyingDecal[steamid] = prop;
                    }
                    CancelPlaceDecalMenu.Show(menuManager, player, localizer);
                    Plugin.Core.Scheduler.NextWorldUpdate(() =>
                    {
                        player.SendChat($"{localizer["cc.decals.prefix"]} {localizer["cc.placedcal.pinghelper"]}");
                        player.SendAlert(localizer["cc.placedcal.pinghelper"]);
                    });
                    return ValueTask.CompletedTask;
                };
                menuBuilder.AddOption(button);
            }

            return menuBuilder.Build();
        }
    }
}