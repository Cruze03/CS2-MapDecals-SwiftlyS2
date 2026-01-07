using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared.EntitySystem;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace MapDecals;

public sealed partial class Plugin
{
    public const float DecalWidth = 128;
    public const float DecalHeight = 128;
    public const int DecalDepth = 12;

    private static Dictionary<int, bool> _adDecalStatus = new();
    public static List<MapDecal> _mapDecals = new();
    public static ConcurrentDictionary<ulong, ConfigDecals?> _applyingDecal = new();
    public static bool _warnErrorDecal = false;

    private void CreateDecalOnPing(CCSPlayerPawn pawn, Vector position, ConfigDecals decal)
    {
        if (pawn == null)
            return;

        try
        {
            var eyeAngles = pawn.EyeAngles;

            // Flip yaw to face the player
            float flippedYaw = (eyeAngles.Y + 180.0f) % 360.0f;
            float yawRad = flippedYaw * MathF.PI / 180f;

            // Backward direction (already unit length in XY plane)
            Vector backward = new(
                -MathF.Cos(yawRad),
                -MathF.Sin(yawRad),
                0f
            );

            Vector decalPos = position + backward * 2f;

            float pitchRad = eyeAngles.X * MathF.PI / 180f;
            float eyeZ = -MathF.Sin(pitchRad);

            var angle = new QAngle(0f, flippedYaw, 0f);

            CHandle<CEnvDecal>? entityHandle;

            // If looking down steeply, push decal slightly up
            if (eyeZ < -0.90f)
            {
                decalPos.Z += 1f;
                entityHandle = CreateDecal(
                    decalPos,
                    angle,
                    decal,
                    DecalDepth,
                    DecalWidth,
                    DecalHeight,
                    false
                );
                if (entityHandle == null) return;
                SaveDecalPosition(pawn, decal, decalPos, angle, DecalDepth, DecalWidth, DecalHeight, (CHandle<CEnvDecal>)entityHandle);
                return;
            }

            // Default wall decal
            angle.X = 90.0f;
            entityHandle = CreateDecal(
                decalPos,
                angle,
                decal,
                DecalDepth,
                DecalWidth,
                DecalHeight,
                false
            );
            if (entityHandle == null) return;
            SaveDecalPosition(pawn, decal, decalPos, angle, DecalDepth, DecalWidth, DecalHeight, (CHandle<CEnvDecal>)entityHandle);
        }
        catch (Exception ex)
        {
            Core.Logger.LogError($"Error while creating decal: {ex}");
        }
    }

    private static void SaveDecalPosition(CCSPlayerPawn pawn, ConfigDecals decal, Vector position, QAngle angle, int depth, float width, float height, CHandle<CEnvDecal> entityHandle)
    {
        if (!entityHandle.IsValid) return;

        var player = pawn.ToPlayer()!;

        var localizer = Core.Translation.GetPlayerLocalizer(player);

        var mapdecal = new MapDecal
        {
            Map = Core.Engine.GlobalVars.MapName,
            DecalId = decal.UniqId,

            DecalName = $"{pawn.LastPlaceName}_{Core.Engine.GlobalVars.TickCount}",

            Position = $"{position.X} {position.Y} {position.Z}",
            Angles = $"{angle.X} {angle.Y} {angle.Z}",

            Depth = depth,
            Width = width,
            Height = height,

            ForceOnVIP = false,
            IsActive = true,
            Entity = entityHandle
        };

        Task.Run(async () =>
        {
            var id = await Database.SaveDecal(mapdecal);

            if (id != null)
            {
                mapdecal.Id = (ulong)id;
            }
            Core.Scheduler.NextWorldUpdate(() =>
            {
                if (!_mapDecals.Contains(mapdecal))
                    _mapDecals.Add(mapdecal);
                MenuManager.OpenDecalMenu(player, localizer, mapdecal);
            });
        });
    }

    public static CHandle<CEnvDecal>? CreateDecal(string position, string ang, string uniqueid, int depth, float width, float height, bool forceOnVip, MapDecal mapdecal)
    {
        var decal = Config.CurrentValue.Props.FirstOrDefault(d => d.UniqId == uniqueid);
        if (!Vector.TryDeserialize(position, out var cords) || !QAngle.TryDeserialize(ang, out var angle)) return null;

        if (decal == null)
        {
            if (!_warnErrorDecal)
            {
                Core.Logger.LogError($"Failed to spawn {mapdecal.DecalName} because unable to find any decal with uniqueid `{uniqueid}` in config.json.");
                _warnErrorDecal = true;
            }
            return null;
        }

        return CreateDecal(
            cords,
            angle,
            decal,
            depth,
            width,
            height,
            forceOnVip
        );
    }

    public static CHandle<CEnvDecal>? CreateDecal(Vector cords, QAngle angle, ConfigDecals decal, int depth, float width, float height, bool forceOnVip)
    {
        using (var keyValues = new CEntityKeyValues())
        {
            var entity = Core.EntitySystem.CreateEntityByDesignerName<CEnvDecal>("env_decal");
            if (entity == null) return null;

            try
            {
                entity.Entity!.Name = $"cc_decal_{Core.Engine.GlobalVars.TickCount}";

                if (forceOnVip)
                {
                    entity.Entity!.Name = $"cc_forcedecal_{Core.Engine.GlobalVars.TickCount}";
                }
                keyValues.SetString("targetname", entity.Entity.Name);
                keyValues.SetString("material", decal.Material);
                entity.Width = width;
                entity.Height = height;
                entity.Depth = depth;
                entity.RenderOrder = 1;
                entity.RenderMode = RenderMode_t.kRenderNormal;

                entity.ProjectOnWorld = true;

                entity.Teleport(cords, angle, null);
                entity.DispatchSpawn(keyValues);

                entity.SetTransmitState(true);

                Core.Scheduler.NextWorldUpdate(() =>
                {
                    foreach (var player in Core.PlayerManager.GetAllPlayers())
                    {
                        if (player.IsFakeClient) continue;

                        UpdateDecalTransmit(player, entity, decal, forceOnVip);
                    }
                });

                return Core.EntitySystem.GetRefEHandle(entity);
            }
            catch (Exception error)
            {
                Core.Logger.LogError($"Error while creating decal: {error}");
                return null;
            }
        }
    }

    public static void UpdateDecalTransmit(IPlayer player, CEnvDecal entity, string uniqueid, bool forceOnVip)
    {
        var cdecal = Config.CurrentValue.Props.FirstOrDefault(d => d.UniqId == uniqueid);

        if (cdecal == null) return;

        UpdateDecalTransmit(player, entity, cdecal, forceOnVip);
    }

    public static void UpdateDecalTransmit(IPlayer player, CEnvDecal? entity = null, ConfigDecals? cdecal = null, bool? forceOnVip = null)
    {
        // TODO: Fix issue where when per player transmit is set, EditDecal on the spot change stops working + enable/disable only works for few seconds after round start
        return;
        if (entity != null && cdecal != null && forceOnVip != null)
        {
            if (forceOnVip == true)
            {
                if (!string.IsNullOrWhiteSpace(cdecal.ShowPermission) && !Core.Permission.PlayerHasPermission(player.SteamID, cdecal.ShowPermission))
                {
                    entity.SetTransmitState(false, player.PlayerID);
                }
                else if (!entity.IsTransmitting(player.PlayerID))
                {
                    entity.SetTransmitState(true, player.PlayerID);
                }
            }
            else if (forceOnVip == false)
            {
                if (!string.IsNullOrWhiteSpace(cdecal.ShowPermission))
                {
                    if (Core.Permission.PlayerHasPermission(player.SteamID, cdecal.ShowPermission))
                    {
                        if (_adDecalStatus.TryGetValue(player.PlayerID, out var status))
                        {
                            // Console.WriteLine($"[1] Found {player.Controller?.PlayerName}'s status: {status}");
                            entity.SetTransmitState(status, player.PlayerID);
                        }
                        else
                        {
                            entity.SetTransmitState(true, player.PlayerID);
                        }
                    }
                    else if (entity.IsTransmitting(player.PlayerID))
                    {
                        entity.SetTransmitState(false, player.PlayerID);
                    }
                }
                else
                {
                    if (_adDecalStatus.TryGetValue(player.PlayerID, out var status))
                    {
                        // Console.WriteLine($"[2] Found {player.Controller?.PlayerName}'s status: {status}");
                        entity.SetTransmitState(status, player.PlayerID);
                    }
                    else
                    {
                        entity.SetTransmitState(true, player.PlayerID);
                    }
                }
            }
            return;
        }

        if (entity == null || cdecal == null || forceOnVip == null)
        {
            foreach (var decal in _mapDecals)
            {
                cdecal = Config.CurrentValue.Props.FirstOrDefault(d => d.UniqId == decal.DecalId);

                if (cdecal == null) continue;

                var ent = decal.GetEntity();

                if (ent == null) continue;

                entity = ent;

                UpdateDecalTransmit(player, entity, cdecal, decal.ForceOnVIP);
            }
        }
    }

    private static void LoadMapDecals(string map)
    {
        _mapDecals.Clear();
        Core.Scheduler.NextWorldUpdate(() =>
        {
            Task.Run(async () =>
            {
                _mapDecals = await Database.GetDecals(map);
            });
        });
    }

    private void LoadPlayerPrefs(IPlayer player)
    {
        if (PlayerCookiesAPIv1 == null) return;

        PlayerCookiesAPIv1.Load(player);

        if (!PlayerCookiesAPIv1.Has(player, MapDecalCookieKey))
        {
            if (!_adDecalStatus.TryAdd(player.PlayerID, true))
            {
                _adDecalStatus[player.PlayerID] = true;
            }
        }
        else
        {
            var status = PlayerCookiesAPIv1.Get<bool>(player, MapDecalCookieKey);
            if (!_adDecalStatus.TryAdd(player.PlayerID, status))
            {
                _adDecalStatus[player.PlayerID] = status;
            }
        }

        UpdateDecalTransmit(player);
    }
}