using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using SwiftlyS2.Shared.Database;

namespace MapDecals.Database.Migrations;

/// <summary>
/// FluentMigrator runner for database schema migrations
/// Supports MySQL/MariaDB, PostgreSQL, and SQLite
/// </summary>
public static class MigrationRunner
{
    /// <summary>
    /// Run all pending migrations
    /// </summary>
    /// <param name="dbConnection">Database connection</param>
    public static void RunMigrations(IDatabaseService dbService, string connectionName)
    {
        using var dbConnection = dbService.GetConnection(connectionName);
        var protocol = dbService.GetConnectionInfo(connectionName).Driver;

        var serviceProvider = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb =>
            {
                ConfigureDatabase(rb, protocol, dbConnection.ConnectionString);
                rb.ScanIn(typeof(MigrationRunner).Assembly).For.Migrations();
            })
            .AddLogging(lb => lb.AddFluentMigratorConsole())
            .BuildServiceProvider(false);

        using var scope = serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    /// <summary>
    /// Configure the FluentMigrator runner for the appropriate database type
    /// </summary>
    private static void ConfigureDatabase(IMigrationRunnerBuilder rb, string dbConnectionDriver, string connectionString)
    {
        switch (dbConnectionDriver)
        {
            case "mysql":
                rb.AddMySql5();
                break;
            case "postgresql":
                rb.AddPostgres();
                break;
            case "sqlite":
                rb.AddSQLite();
                break;
            default:
                throw new NotSupportedException($"Unsupported database connection type: {dbConnectionDriver}");
        }

        rb.WithGlobalConnectionString(connectionString);
    }
}