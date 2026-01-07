using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Translation;

namespace MapDecals.Menus;

// =========================================
// =           EDIT DECAL MENU
// =========================================

/// <summary>edit decal menu</summary>
public sealed partial class MenuManager
{
    internal static class EditDecalMenu
    {
        internal static void Show(MenuManager menuManager, IPlayer player, ILocalizer localizer, MapDecal decal)
        {
            var menu = Build(menuManager, player, localizer, decal);
            Plugin.Core.MenusAPI.OpenMenuForPlayer(player, menu);
        }

        // =========================================
        // =           BUILD MENU
        // =========================================

        internal static IMenuAPI Build(MenuManager menuManager, IPlayer player, ILocalizer localizer, MapDecal decal)
        {
            var menuBuilder = Plugin.Core.MenusAPI
                .CreateBuilder()
                .Design.SetMenuTitle(localizer["cc.menu.editdecal.title", decal.DecalName])
                .Design.SetMenuTitleVisible(true)
                .Design.SetMenuFooterVisible(true)
                .Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
                .SetPlayerFrozen(false);

            var decalToggle = new ToggleMenuOption(localizer["cc.menu.editdecal.isactive"], decal.IsActive);
            decalToggle.ValueChanged += async (_, e) =>
            {
                decal.IsActive = e.NewValue;
                decal.ToggleEntity();
                var msg = e.NewValue ? localizer["cc.menu.editdecal.isactive.on"] : localizer["cc.menu.editdecal.isactive.off"];
                await menuManager._database.SaveDecal(decal);
                _ = e.Player.SendChatAsync($"{localizer["cc.decals.prefix"]} {msg}");
            };
            menuBuilder.AddOption(decalToggle);

            var decalDelete = new InputMenuOption(localizer["cc.menu.editdecal.delete"], defaultValue: decal.DecalName, hintMessage: localizer["cc.menu.editdecal.delete.hint"],
            validator: (value) =>
            {
                if (value.Length < 3 || string.IsNullOrWhiteSpace(value))
                {
                    return false;
                }

                return true;
            });
            decalDelete.ValueChanged += async (_, e) =>
            {
                if (e.NewValue.ToLower().Equals("yes"))
                {
                    await menuManager._database.DeleteDecal(decal.Id);
                    Plugin._mapDecals.Remove(decal);
                    decal.DeleteEntity();
                    Plugin.Core.Scheduler.NextWorldUpdate(() =>
                    {
                        var menu = Plugin.Core.MenusAPI.GetCurrentMenu(player);
                        if (menu != null)
                            Plugin.Core.MenusAPI.CloseMenuForPlayer(player, menu);

                        if (Plugin._mapDecals.Count > 0)
                            EditDecalsMenu.Show(menuManager, player, localizer);
                    });
                }
            };
            menuBuilder.AddOption(decalDelete);

            var decalName = new InputMenuOption(localizer["cc.menu.editdecal.name"], defaultValue: decal.DecalName, hintMessage: localizer["cc.menu.editdecal.name.hint"],
            validator: (value) =>
            {
                if (value.Length < 3 || string.IsNullOrWhiteSpace(value) || Plugin._mapDecals.Any(d => d.DecalName == value))
                {
                    return false;
                }

                return true;
            });
            decalName.SetValue(player, decal.DecalName);
            decalName.ValueChanged += async (_, e) =>
            {
                decal.DecalName = e.NewValue;
                await menuManager._database.SaveDecal(decal);
            };

            menuBuilder.AddOption(decalName);

            menuBuilder.AddOption(new SubmenuMenuOption(
                localizer["cc.menu.editdecal.position"],
                () =>
                {
                    var menuBuilder = Plugin.Core.MenusAPI
                        .CreateBuilder()
                        .Design.SetMenuTitle(localizer["cc.menu.editdecal.position"])
                        .Design.SetMenuTitleVisible(true)
                        .Design.SetMenuFooterVisible(true)
                        .Design.SetGlobalScrollStyle(MenuOptionScrollStyle.LinearScroll)
                        .SetPlayerFrozen(false);

                    if (!Vector.TryDeserialize(decal.Position, out var position) || !QAngle.TryDeserialize(decal.Angles, out var angle))
                    {
                        return menuBuilder.Build();
                    }

                    var positionX = position.X.ToString();
                    var positionY = position.Y.ToString();
                    var positionZ = position.Z.ToString();
                    var angleX = angle.X.ToString();
                    var angleY = angle.Y.ToString();
                    var angleZ = angle.Z.ToString();

                    var positionXInput = new InputMenuOption(
                        text: localizer["cc.menu.editdecal.position.x"],
                        hintMessage: localizer["cc.menu.editdecal.position.hint"],
                        defaultValue: positionX,
                        validator: (value) =>
                        {
                            if (string.IsNullOrWhiteSpace(value) || positionX == value || !float.TryParse(value, out _))
                            {
                                return false;
                            }

                            return true;
                        });
                    positionXInput.SetValue(player, positionX);
                    positionXInput.ValueChanged += async (_, e) =>
                    {
                        decal.Position = $"{e.NewValue} {positionY} {positionZ}";
                        positionX = e.NewValue;
                        positionXInput.SetValue(player, positionX);
                        decal.PositionUpdated();
                        await menuManager._database.SaveDecal(decal);
                    };

                    var positionYInput = new InputMenuOption(
                        text: localizer["cc.menu.editdecal.position.y"],
                        hintMessage: localizer["cc.menu.editdecal.position.hint"],
                        defaultValue: positionY,
                        validator: (value) =>
                        {
                            if (string.IsNullOrWhiteSpace(value) || positionY == value || !float.TryParse(value, out _))
                            {
                                return false;
                            }

                            return true;
                        });
                    positionYInput.SetValue(player, positionY);
                    positionYInput.ValueChanged += async (_, e) =>
                    {
                        decal.Position = $"{positionX} {e.NewValue} {positionZ}";
                        positionY = e.NewValue;
                        positionYInput.SetValue(player, positionY);
                        decal.PositionUpdated();
                        await menuManager._database.SaveDecal(decal);
                    };

                    var positionZInput = new InputMenuOption(
                        text: localizer["cc.menu.editdecal.position.z"],
                        hintMessage: localizer["cc.menu.editdecal.position.hint"],
                        defaultValue: positionZ,
                        validator: (value) =>
                        {
                            if (string.IsNullOrWhiteSpace(value) || positionZ == value || !float.TryParse(value, out _))
                            {
                                return false;
                            }

                            return true;
                        });
                    positionZInput.SetValue(player, positionY);
                    positionZInput.ValueChanged += async (_, e) =>
                    {
                        decal.Position = $"{positionX} {positionY} {e.NewValue}";
                        positionZ = e.NewValue;
                        positionZInput.SetValue(player, positionZ);
                        decal.PositionUpdated();
                        await menuManager._database.SaveDecal(decal);
                    };

                    var angleXInput = new InputMenuOption(
                        text: localizer["cc.menu.editdecal.angle.x"],
                        hintMessage: localizer["cc.menu.editdecal.position.hint"],
                        defaultValue: angleX,
                        validator: (value) =>
                        {
                            if (string.IsNullOrWhiteSpace(value) || angleX == value || !float.TryParse(value, out _))
                            {
                                return false;
                            }

                            return true;
                        });
                    angleXInput.SetValue(player, angleX);
                    angleXInput.ValueChanged += async (_, e) =>
                    {
                        decal.Angles = $"{e.NewValue} {angleY} {angleZ}";
                        angleX = e.NewValue;
                        angleXInput.SetValue(player, angleX);
                        decal.PositionUpdated();
                        await menuManager._database.SaveDecal(decal);
                    };

                    var angleYInput = new InputMenuOption(
                        text: localizer["cc.menu.editdecal.angle.y"],
                        hintMessage: localizer["cc.menu.editdecal.position.hint"],
                        defaultValue: angleY,
                        validator: (value) =>
                        {
                            if (string.IsNullOrWhiteSpace(value) || angleY == value || !float.TryParse(value, out _))
                            {
                                return false;
                            }

                            return true;
                        });
                    angleYInput.SetValue(player, angleY);
                    angleYInput.ValueChanged += async (_, e) =>
                    {
                        decal.Angles = $"{angleX} {e.NewValue} {angleZ}";
                        angleY = e.NewValue;
                        angleYInput.SetValue(player, angleY);
                        decal.PositionUpdated();
                        await menuManager._database.SaveDecal(decal);
                    };

                    var angleZInput = new InputMenuOption(
                        text: localizer["cc.menu.editdecal.angle.z"],
                        hintMessage: localizer["cc.menu.editdecal.position.hint"],
                        defaultValue: angleZ,
                        validator: (value) =>
                        {
                            if (string.IsNullOrWhiteSpace(value) || angleZ == value || !float.TryParse(value, out _))
                            {
                                return false;
                            }

                            return true;
                        });
                    angleZInput.SetValue(player, angleZ);
                    angleZInput.ValueChanged += async (_, e) =>
                    {
                        decal.Angles = $"{angleX} {angleY} {e.NewValue}";
                        angleZ = e.NewValue;
                        angleZInput.SetValue(player, angleZ);
                        decal.PositionUpdated();
                        await menuManager._database.SaveDecal(decal);
                    };


                    menuBuilder.AddOption(positionXInput);
                    menuBuilder.AddOption(positionYInput);
                    menuBuilder.AddOption(positionZInput);
                    menuBuilder.AddOption(angleXInput);
                    menuBuilder.AddOption(angleYInput);
                    menuBuilder.AddOption(angleZInput);

                    return menuBuilder.Build();
                }
            ));

            var depthSlider = new SliderMenuOption(
                text: localizer["cc.menu.editdecal.depth"],
                min: 0,
                max: 100,
                defaultValue: decal.Depth,
                step: 2
            );
            depthSlider.ValueChanged += async (_, e) =>
            {
                decal.Depth = (int)e.NewValue;
                decal.DepthUpdated();
                await menuManager._database.SaveDecal(decal);
            };
            menuBuilder.AddOption(depthSlider);

            var widthSlider = new SliderMenuOption(
                text: localizer["cc.menu.editdecal.width"],
                min: 64,
                max: 256,
                defaultValue: decal.Width,
                step: 4
            );
            widthSlider.ValueChanged += async (_, e) =>
            {
                decal.Width = (int)e.NewValue;
                decal.WidthUpdated();
                await menuManager._database.SaveDecal(decal);
            };
            menuBuilder.AddOption(widthSlider);

            var heightSlider = new SliderMenuOption(
                text: localizer["cc.menu.editdecal.height"],
                min: 64,
                max: 1024,
                defaultValue: decal.Height,
                step: 4
            );
            heightSlider.ValueChanged += async (_, e) =>
            {
                decal.Height = (int)e.NewValue;
                decal.HeightUpdated();
                await menuManager._database.SaveDecal(decal);
            };
            menuBuilder.AddOption(heightSlider);

            var vipToggle = new ToggleMenuOption(localizer["cc.menu.editdecal.forceonvip"], decal.ForceOnVIP);
            vipToggle.ValueChanged += async (_, e) =>
            {
                decal.ForceOnVIP = e.NewValue;
                decal.ForceOnVIPUpdated();
                await menuManager._database.SaveDecal(decal);
                var msg = e.NewValue ? localizer["cc.menu.editdecal.forceonvip.on"] : localizer["cc.menu.editdecal.forceonvip.off"];
                await e.Player.SendChatAsync($"{localizer["cc.decals.prefix"]} {msg}");
            };
            menuBuilder.AddOption(vipToggle);

            return menuBuilder.Build();
        }
    }
}