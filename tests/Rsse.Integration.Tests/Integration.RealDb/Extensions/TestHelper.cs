using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Npgsql;
using SearchEngine.Api.Startup;

namespace SearchEngine.Tests.Integration.RealDb.Extensions;

/// <summary>
/// Контейнер с вспомогательным функционалом для тестов.
/// </summary>
public static class TestHelper
{
    /// <summary>
    /// Очистить таблицы двух баз данных
    /// </summary>
    /// <param name="factory">Хост.</param>
    /// <param name="ct">Токен отмены.</param>
    // todo: перенести в сид если требуется очистка
    internal static async Task CleanUpDatabases(WebApplicationFactory<Startup> factory, CancellationToken ct)
    {
        var pgDataSource = factory.Services.GetRequiredService<NpgsqlDataSource>();
        var mysqlConnectionString = factory.Services
            .GetRequiredService<IConfiguration>()
            .GetConnectionString(Startup.DefaultConnectionKey);

        var commands = new List<string>
        {
            """TRUNCATE TABLE "Users" RESTART IDENTITY CASCADE;""",
            """TRUNCATE TABLE "TagsToNotes" RESTART IDENTITY CASCADE;""",
            """TRUNCATE TABLE "Tag" RESTART IDENTITY CASCADE;""",
            """TRUNCATE TABLE "Note" RESTART IDENTITY CASCADE;""",
            """INSERT INTO public."Users" VALUES (1, '1@2', '12');"""
        };
        foreach (var command in commands)
        {
            await using var cmd = pgDataSource.CreateCommand(command);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        await using var mysqlConnection = new MySqlConnection(mysqlConnectionString);
        await mysqlConnection.OpenAsync(ct);
        commands =
        [
            "SET FOREIGN_KEY_CHECKS = 0;",
            "TRUNCATE TABLE `Users`;",
            "TRUNCATE TABLE `Tag`;",
            "TRUNCATE TABLE `Note`;",
            "TRUNCATE TABLE `TagsToNotes`;",
            "SET FOREIGN_KEY_CHECKS = 1;",
            "INSERT Users VALUES(1, '1@2', '12');"
        ];
        foreach (var command in commands)
        {
            await using var cmd = new MySqlCommand(command, mysqlConnection);
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}
