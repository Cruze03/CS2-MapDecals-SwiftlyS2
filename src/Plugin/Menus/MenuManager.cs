using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Translation;

namespace MapDecals.Menus;

/// <summary>
/// Central menu manager for Map Decals
/// Handles all menu navigation and display
/// Split into partial classes for organization
/// </summary>
public sealed partial class MenuManager
{
    // =========================================
    // =           FIELDS
    // =========================================

    internal readonly Plugin.DatabaseService _database;
    internal readonly PluginConfig _config;

    // =========================================
    // =           CONSTRUCTOR
    // =========================================

    public MenuManager(Plugin.DatabaseService database, PluginConfig config)
    {
        _database = database;
        _config = config;
    }

    // =========================================
    // =           PUBLIC API
    // =========================================

    /// <summary>Opens the main menu for decal placement</summary>
    public void OpenMainMenu(IPlayer player, ILocalizer localizer)
    {
        DecalMenu.Show(this, player, localizer);
    }

    /// <summary>Opens decal menu to edit decal placement</summary>
    public void OpenDecalMenu(IPlayer player, ILocalizer localizer, MapDecal decal)
    {
        EditDecalMenu.Show(this, player, localizer, decal);
    }
}