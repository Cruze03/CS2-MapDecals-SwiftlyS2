using System.Data;
using FluentMigrator.Runner;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Npgsql;

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
    public static void RunMigrations(IDbConnection dbConnection)
    {
        var serviceProvider = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb =>
            {
                ConfigureDatabase(rb, dbConnection);
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
    private static void ConfigureDatabase(IMigrationRunnerBuilder rb, IDbConnection dbConnection)
    {
        switch (dbConnection)
        {
            case MySqlConnection:
                rb.AddMySql5();
                break;
            case NpgsqlConnection:
                rb.AddPostgres();
                break;
            case System.Data.SQLite.SQLiteConnection:
                rb.AddSQLite();
                break;
            default:
                throw new NotSupportedException($"Unsupported database connection type: {dbConnection.GetType().Name}");
        }

        rb.WithGlobalConnectionString(dbConnection.ConnectionString);
    }
}