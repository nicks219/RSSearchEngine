using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rsse.Api.Services;
using Rsse.Domain.Service.Contracts;
using Rsse.Infrastructure.Context;
using Rsse.Tests.Common;
using Rsse.Tests.Integration.FakeDb.Api;
using Rsse.Tests.Integration.FakeDb.Extensions;
using RsseEngine.Service;
using static Rsse.Domain.Service.Configuration.RouteConstants;

namespace Rsse.Tests.Integration.FakeDb;

/// <summary>
/// Тестирование поисковых метрик по всем алгоритмам на актуальном объеме данных.
/// </summary>
[TestClass]
public class ComplianceGeneralTests
{
    private static readonly Uri BaseAddress = new("http://localhost:5000/");
    private static readonly WebApplicationFactoryClientOptions Options = new() { BaseAddress = BaseAddress };
    private static readonly CancellationToken NoneToken = CancellationToken.None;
    private static CustomWebAppFactory<SqliteStartup>? _factory;

    [ClassInitialize]
    public static async Task Initialize(TestContext _)
    {
        _factory = new CustomWebAppFactory<SqliteStartup>();
        using var client = _factory.CreateClient(Options);
        // Cигнатура выбрана из-за конфликта в перегрузках `Microsoft.Testing`.
        await using var context = (NpgsqlCatalogContext)_factory.Services.GetService(typeof(NpgsqlCatalogContext))!;
        var db = context.Database;

        await using (var connection = db.GetDbConnection())
        {
            await connection.OpenAsync();

            await using (var cmd = connection.CreateCommand())
            {
                // Удаляем всё из таблицы потому тк DatabaseInitializer добавляет один документ.
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "DELETE FROM [Note]";
                await cmd.ExecuteNonQueryAsync();
            }

            var fileDataProvider = new FileDataOnceProvider();
            await foreach (var noteEntity in fileDataProvider.GetDataAsync())
            {
                await using var cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "INSERT INTO [Note] ([NoteId], [Title], [Text]) VALUES (@param1, @param2, @param3)";

                var parameterFirst = cmd.CreateParameter();
                parameterFirst.ParameterName = "param1";
                parameterFirst.Value = noteEntity.NoteId;
                parameterFirst.DbType = DbType.Int32;
                cmd.Parameters.Add(parameterFirst);

                var parameterSecond = cmd.CreateParameter();
                parameterSecond.ParameterName = "param2";
                parameterSecond.Value = noteEntity.Title;
                parameterSecond.DbType = DbType.String;
                cmd.Parameters.Add(parameterSecond);

                var parameterThird = cmd.CreateParameter();
                parameterThird.ParameterName = "param3";
                parameterThird.Value = noteEntity.Text;
                parameterThird.DbType = DbType.String;
                cmd.Parameters.Add(parameterThird);

                var result = await cmd.ExecuteNonQueryAsync();
                if (result != 1)
                {
                    throw new Exception($"[{nameof(Initialize)}] insert failed | " +
                                        $"'{parameterFirst.Value}' '{parameterSecond.Value}' '{parameterThird.Value}'");
                }
            }
        }

        // Необходимо заново инициализировать токенайзер тк содержание бд изменилось.
        var tokenizerApiClient = (ITokenizerApiClient)_factory?.Services.GetService(typeof(ITokenizerApiClient))!;
        var dataProvider = (DbDataProvider)_factory?.Services.GetService(typeof(DbDataProvider))!;
        await tokenizerApiClient.Initialize(dataProvider, NoneToken);
    }

    [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
    public static void ClassCleanup() => _factory?.Dispose();

    // прокидываем коллекцию тк она должна принадлежать тесту
    public static IEnumerable<object?[]> ComplianceTestData => TestData.ComplianceTestData;

    [TestMethod]
    [DynamicData(nameof(ComplianceTestData))]
    public async Task ComplianceRequest_ShouldReturn_ExpectedMetrics(string request, string expected)
    {
        foreach (var extendedSearchType in TestData.ExtendedSearchTypes)
        {
            foreach (var reducedSearchTypes in TestData.ReducedSearchTypes)
            {
                // arrange:
                using var client = _factory?.CreateClient(Options);
                client.EnsureNotNull();
                var uri = new Uri($"{ComplianceIndicesGetUrl}?text={request}", UriKind.Relative);
                var tokenizerApiClient = (ITokenizerApiClient)_factory?.Services.GetService(typeof(ITokenizerApiClient))!;
                var tokenizerServiceCore = (TokenizerServiceCore)tokenizerApiClient.GetTokenizerServiceCore();
                tokenizerServiceCore.ConfigureSearchEngine(
                    extendedSearchType: extendedSearchType,
                    reducedSearchType: reducedSearchTypes);

                // act:
                using var response = await client.GetAsync(uri, NoneToken);
                var complianceResponse = await response.Content.ReadAsStringAsync(NoneToken);

                // assert:
                complianceResponse.Should().Be(expected, $"[ExtendedSearchType.{extendedSearchType} ReducedSearchType.{reducedSearchTypes}]");
            }
        }
    }

    /// <summary>
    /// Посмотреть содержимое таблицы.
    /// </summary>
    /// <param name="connection">Соединения с бд.</param>
    private static async Task LookupTable(DbConnection connection)
    {
        await using var cmd = connection.CreateCommand();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = "SELECT * FROM [Note]";

        var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var resultFirst = reader.GetInt64(0);
            var resultSecond = reader.GetString(1);
            var resultThird = reader.GetString(2);

            Console.WriteLine($"\t{resultFirst}\t{resultSecond}\t{resultThird}");
        }

        await reader.CloseAsync();
    }
}
