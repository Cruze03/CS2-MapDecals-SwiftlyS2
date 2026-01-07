using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dommel;
using MapDecals;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

// =========================================
// =           DATABASE RECORD
// =========================================

/// <summary>
/// Database record for map decals
/// </summary>
[Table("cc_mapdecals")]
public sealed class MapDecal
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public ulong Id { get; set; }

    [Required]
    [MaxLength(64)]
    [Column("map")]
    public string Map { get; set; } = "";

    [Required]
    [MaxLength(64)]
    [Column("decal_id")]
    public string DecalId { get; set; } = "";

    [Required]
    [MaxLength(64)]
    [Column("decal_name")]
    public string DecalName { get; set; } = "";

    [Column("position")]
    public string Position { get; set; } = "";

    [Column("angles")]
    public string Angles { get; set; } = "";

    [Column("depth")]
    public int Depth { get; set; } = 12;

    [Column("width")]
    public float Width { get; set; } = 128;

    [Column("height")]
    public float Height { get; set; } = 128;

    [Column("force_on_vip")]
    public bool ForceOnVIP { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [Ignore]
    public IPlayer? EditingPlayer { get; set; } = null;

    [Ignore]
    public CHandle<CEnvDecal> Entity { get; set; }

    public void ToggleEntity()
    {
        if (IsActive)
        {
            Plugin.Core.Scheduler.NextWorldUpdate(() =>
            {
                var entityHandle = Plugin.CreateDecal(Position, Angles, DecalId, Depth, Width, Height, ForceOnVIP, this);
                if (entityHandle == null) return;
                Entity = (CHandle<CEnvDecal>)entityHandle;
            });
        }
        else
        {
            DeleteEntity();
        }
    }

    public CEnvDecal? GetEntity()
    {
        if (!Entity.IsValid) return null;

        var entity = Entity.Value;

        if (entity == null) return null;

        return entity;
    }

    public void DeleteEntity()
    {
        Plugin.Core.Scheduler.NextWorldUpdate(() =>
        {
            var entity = GetEntity();

            if (entity == null) return;

            entity.Despawn();
        });
    }

    public void PositionUpdated()
    {
        Plugin.Core.Scheduler.NextWorldUpdate(() =>
        {
            var entity = GetEntity();

            if (entity == null) return;

            if (!Vector.TryDeserialize(Position, out var pos) || !QAngle.TryDeserialize(Angles, out var ang)) return;

            entity.Teleport(pos, ang, null);
        });
    }

    public void WidthUpdated()
    {
        Plugin.Core.Scheduler.NextWorldUpdate(() =>
        {
            var entity = GetEntity();

            if (entity == null) return;

            entity.Width = Width;
            entity.WidthUpdated();
        });
    }

    public void HeightUpdated()
    {
        Plugin.Core.Scheduler.NextWorldUpdate(() =>
        {
            var entity = GetEntity();

            if (entity == null) return;

            entity.Height = Height;
            entity.HeightUpdated();
        });
    }

    public void DepthUpdated()
    {
        Plugin.Core.Scheduler.NextWorldUpdate(() =>
        {
            var entity = GetEntity();

            if (entity == null) return;

            entity.Depth = Depth;
            entity.DepthUpdated();
        });
    }
    public void ForceOnVIPUpdated()
    {
        Plugin.Core.Scheduler.NextWorldUpdate(() =>
        {
            var entity = GetEntity();

            if (entity == null) return;

            foreach (var player in Plugin.Core.PlayerManager.GetAllPlayers())
            {
                if (player.IsFakeClient) continue;

                Plugin.UpdateDecalTransmit(player, entity, DecalId, ForceOnVIP);
            }
        });
    }
}