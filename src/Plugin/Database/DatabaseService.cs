using Dommel;
using MapDecals.Database.Migrations;
using Microsoft.Extensions.Logging;

namespace MapDecals;

public sealed partial class Plugin
{
    public sealed partial class DatabaseService
    {
        /* ==================== Fields ==================== */
        private readonly string _connectionName;

        /* ==================== Properties ==================== */
        public bool IsEnabled { get; private set; }

        /* ==================== Constructor ==================== */
        public DatabaseService(string connectionName, bool hotReload)
        {
            _connectionName = connectionName;

            Task.Run(async () =>
            {
                await InitializeAsync(hotReload);
            });
        }

        /* ==================== Initialization ==================== */
        public async Task InitializeAsync(bool hotReload)
        {
            try
            {
                // Run FluentMigrator migrations
                MigrationRunner.RunMigrations(Core.Database, _connectionName);

                IsEnabled = true;

                if (hotReload)
                    LoadMapDecals(Core.Engine.GlobalVars.MapName);
            }
            catch (Exception ex)
            {
                Core.Logger.LogError(ex, "Failed to initialize database");
                IsEnabled = false;
            }
        }

        public async Task<List<MapDecal>> GetDecals(string map)
        {
            if (!IsEnabled)
                return new();

            try
            {
                using var connection = Core.Database.GetConnection(_connectionName);
                connection.Open();

                return (await connection
                    .SelectAsync<MapDecal>(d => d.Map == map))
                    .ToList();
            }
            catch (Exception ex)
            {
                Core.Logger.LogError(ex, "Failed to load decals for map {Map}", map);
                return new();
            }
        }

        public async Task<ulong?> SaveDecal(MapDecal decal)
        {
            if (!IsEnabled)
                return null;

            try
            {
                using var connection = Core.Database.GetConnection(_connectionName);
                connection.Open();

                var existing = await connection.GetAsync<MapDecal>(decal.Id);
                if (existing != null)
                {
                    await connection.UpdateAsync(decal);
                }
                else
                {
                    object result = await connection.InsertAsync(decal);
                    return Convert.ToUInt64(result);
                }
            }
            catch (Exception ex)
            {
                Core.Logger.LogError(ex, "Failed to save decal for map {Map}", decal.Map);
            }

            return null;
        }

        public async Task<bool> DeleteDecal(ulong id)
        {
            if (!IsEnabled)
                return false;

            try
            {
                using var connection = Core.Database.GetConnection(_connectionName);
                connection.Open();

                await connection.DeleteAsync(new MapDecal { Id = id });
            }
            catch (Exception ex)
            {
                Core.Logger.LogError(ex, "Failed to delete decal for map {Map}", Core.Engine.GlobalVars.MapName);
                return false;
            }

            return true;
        }
    }
}