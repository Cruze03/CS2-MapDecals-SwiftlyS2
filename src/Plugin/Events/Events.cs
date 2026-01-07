using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace MapDecals;

public sealed partial class Plugin
{
    private void RegisterGameEvents()
    {
        Core.GameEvent.HookPost<EventRoundPoststart>(OnRoundStart);
        Core.GameEvent.HookPost<EventPlayerPing>(OnPlayerPing);
        Core.GameEvent.HookPost<EventPlayerConnectFull>(OnPlayerConnectFull);
        Core.GameEvent.HookPre<EventPlayerDisconnect>(OnPlayerDisconnect);

        Core.Event.OnPrecacheResource += OnPrecacheResource;
        Core.Event.OnMapLoad += OnMapLoad;
    }

    private void OnMapLoad(IOnMapLoadEvent @event)
    {
        LoadMapDecals(@event.MapName);
        _warnErrorDecal = false;
    }

    private void OnPrecacheResource(IOnPrecacheResourceEvent @event)
    {
        var props = Config.CurrentValue.Props;
        if (props.Count() > 0)
        {
            foreach (var prop in props)
            {
                @event.AddItem(prop.Material);
            }
        }
    }

    private HookResult OnRoundStart(EventRoundPoststart @event)
    {
        foreach (var decal in _mapDecals)
        {
            if (!decal.IsActive) continue;

            var entityHandle = CreateDecal(
                decal.Position,
                decal.Angles,
                decal.DecalId,
                decal.Depth,
                decal.Width,
                decal.Height,
                decal.ForceOnVIP,
                decal
            );

            if (entityHandle == null) continue;

            decal.Entity = (CHandle<CEnvDecal>)entityHandle;
        }
        return HookResult.Continue;
    }

    private HookResult OnPlayerPing(EventPlayerPing @event)
    {
        var player = @event.Accessor.GetPlayer("userid");
        if (player == null || !player.IsValid || player.IsFakeClient) return HookResult.Continue;

        var steamid = player.SteamID;

        if (!_applyingDecal.TryGetValue(steamid, out var decal) || decal == null) return HookResult.Continue;

        var pawn = player.PlayerPawn;
        if (pawn == null) return HookResult.Continue;

        _applyingDecal[steamid] = null;

        var menu = Core.MenusAPI.GetCurrentMenu(player);
        if (menu != null)
            Core.MenusAPI.CloseMenuForPlayer(player, menu);

        CreateDecalOnPing(pawn, new(@event.X, @event.Y, @event.Z), decal);
        return HookResult.Continue;
    }

    private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event)
    {
        var player = @event.Accessor.GetPlayer("userid");
        if (player == null || !player.IsValid || player.IsFakeClient) return HookResult.Continue;

        LoadPlayerPrefs(player);

        return HookResult.Continue;
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event)
    {
        var player = @event.Accessor.GetPlayer("userid");
        if (player == null || !player.IsValid || player.IsFakeClient || PlayerCookiesAPIv1 == null) return HookResult.Continue;

        if (_adDecalStatus.TryGetValue(player.PlayerID, out var status))
        {
            PlayerCookiesAPIv1.Set(player, MapDecalCookieKey, status);
            PlayerCookiesAPIv1.Save(player);
            _adDecalStatus.Remove(player.PlayerID);
        }

        return HookResult.Continue;
    }
}