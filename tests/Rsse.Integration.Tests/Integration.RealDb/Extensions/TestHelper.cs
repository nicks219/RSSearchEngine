using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Npgsql;
using Rsse.Api.Startup;

namespace Rsse.Tests.Integration.RealDb.Extensions;

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
        var mysqlDataSource = factory.Services.GetRequiredService<MySqlDataSource>();

        var command =
                """
                TRUNCATE TABLE "Users" RESTART IDENTITY CASCADE;
                TRUNCATE TABLE "TagsToNotes" RESTART IDENTITY CASCADE;
                TRUNCATE TABLE "Tag" RESTART IDENTITY CASCADE;
                TRUNCATE TABLE "Note" RESTART IDENTITY CASCADE;
                INSERT INTO public."Users" VALUES (1, '1@2', '12');
                """;

        await using var pgCmd = pgDataSource.CreateCommand(command);
        await pgCmd.ExecuteNonQueryAsync(ct);

        command =
            """
            SET FOREIGN_KEY_CHECKS = 0;
            TRUNCATE TABLE `Users`;
            TRUNCATE TABLE `TagsToNotes`;
            TRUNCATE TABLE `Tag`;
            TRUNCATE TABLE `Note`;
            SET FOREIGN_KEY_CHECKS = 1;
            INSERT Users VALUES(1, '1@2', '12');
            """;

        await using var mysqlCmd = mysqlDataSource.CreateCommand(command);
        await mysqlCmd.ExecuteNonQueryAsync(ct);

    }
}
