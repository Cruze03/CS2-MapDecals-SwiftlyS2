using MapDecals.Menus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Plugins;

using Cookies.Contract;

namespace MapDecals;

[PluginMetadata(
    Id = "cc.mapdecals",
    Version = "1.0.1",
    Name = "Map Decals",
    Author = "Cruze",
    Description = "Place map decals inside map"
// MinimumAPIVersion = "1.1.5-beta55"
)]
public sealed partial class Plugin(ISwiftlyCore core) : BasePlugin(core)
{
    /* ==================== Static Access ==================== */
    public static new ISwiftlyCore Core { get; private set; } = null!;

    /* ==================== Configurations ==================== */
    internal static IOptionsMonitor<PluginConfig> Config { get; private set; } = null!;

    /* ==================== Services ==================== */
    internal static DatabaseService Database { get; private set; } = null!;
    internal static MenuManager MenuManager { get; private set; } = null!;

    /* ==================== Shared API ==================== */
    internal IPlayerCookiesAPIv1? PlayerCookiesAPIv1 { get; private set; } = null;
    internal const string MapDecalCookieKey = "cc_mapdecals.v1";

    public override void Load(bool hotReload)
    {
        Core = base.Core;

        InitializeConfigs();
        RegisterCommands();
        RegisterGameEvents();

        Database = new DatabaseService("cc_mapdecals", hotReload);

        MenuManager = new MenuManager(Database, Config.CurrentValue);
    }

    /* ==================== Configuration Loading ==================== */
    private void InitializeConfigs()
    {
        Config = BuildConfigService<PluginConfig>("config.json", "MapDecals");
    }

    private IOptionsMonitor<T> BuildConfigService<T>(string fileName, string sectionName) where T : class, new()
    {
        Core.Configuration
            .InitializeJsonWithModel<T>(fileName, sectionName)
            .Configure(builder =>
            {
                builder.AddJsonFile(Core.Configuration.GetConfigPath(fileName), optional: false, reloadOnChange: true);
            });

        ServiceCollection services = new();
        services.AddSwiftly(Core)
            .AddOptionsWithValidateOnStart<T>()
            .BindConfiguration(sectionName);

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IOptionsMonitor<T>>();
    }

    /* ==================== Shared API ==================== */
    public override void UseSharedInterface(IInterfaceManager interfaceManager)
    {
        if (interfaceManager.HasSharedInterface("Cookies.Player.v1"))
        {
            PlayerCookiesAPIv1 = interfaceManager.GetSharedInterface<IPlayerCookiesAPIv1>("Cookies.Player.v1");

            foreach (var player in Core.PlayerManager.GetAllPlayers())
            {
                if (player.IsFakeClient) continue;

                LoadPlayerPrefs(player);
            }
        }
    }

    /* ==================== Unload ==================== */
    public override void Unload() { }
}