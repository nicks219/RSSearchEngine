using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rsse.Api.Startup;
using Rsse.Domain.Data.Dto;
using Rsse.Domain.Service.ApiModels;
using Rsse.Domain.Service.Configuration;
using Rsse.Tests.Common;
using Rsse.Tests.Integration.RealDb.Api;
using Rsse.Tests.Integration.RealDb.Extensions;
using Rsse.Tests.Integration.RealDb.Infra;

namespace Rsse.Tests.Integration.RealDb;

[TestClass]
public sealed class ReadOnlyIntegrationTests : TestBase, IDisposable
{
    private readonly IntegrationWebAppFactory<Startup> _factory = new();

    [ClassInitialize]
    public static async Task InitializeTests(TestContext context)
    {
        // Однократная накатка дампа на Postgres.
        var token = CancellationToken.None;
        await using var factory = new IntegrationWebAppFactory<Startup>();
        using var client = factory.CreateClient(Options);

        await client.GetAsync(RouteConstants.SystemWaitWarmUpGetUrl, token);
        await TestHelper.CleanUpDatabases(factory, Token);
        await client.TryAuthorizeToService("1@2", "12", ct: Token);
        await client.GetAsync($"{RouteConstants.MigrationRestoreGetUrl}?databaseType=MySql", token);
        await client.GetAsync(RouteConstants.MigrationCopyGetUrl, token);
    }

    /// <summary>
    /// Закрываем фабрику средствами шарпа.
    /// </summary>
    public void Dispose()
    {
        _factory.Dispose();
    }

    /// <summary>
    /// Тест оценивает распределение запросов на получение случайной заметки.
    /// </summary>
    [TestMethod]
    public async Task ElectionRequests_ShouldHasExpectedResponsesDistribution_WithRandomElection()
    {
        var token = CancellationToken.None;

        const int electionTestNotesCount = 900;
        const int electionTestTagsCount = 44;
        const double expectedCoefficient = 0.7D;
        var requestCount = 250;

        using var client = _factory.CreateClient(Options);

        await client.GetAsync(RouteConstants.SystemWaitWarmUpGetUrl, token);

        var tempCount = requestCount;

        var expectedNotesCount = Math.Min(electionTestNotesCount, requestCount) * expectedCoefficient;

        var checkedTags = Enumerable.Range(1, electionTestTagsCount).ToList();
        var noteRequestDto = new NoteRequest
        (
            CheckedTags: checkedTags,
            Title: default,
            Text: default,
            NoteIdExchange: default
        );

        var idStorage = new Dictionary<int, int>();

        while (requestCount-- > 0)
        {
            var uri = new Uri(RouteConstants.ReadNotePostUrl, UriKind.Relative);
            using var content = new StringContent(JsonSerializer.Serialize(noteRequestDto), Encoding.UTF8, "application/json");
            using var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = content;
            using var message = await client.SendAsync(request, cancellationToken: token);
            message.EnsureSuccessStatusCode();
            var resp = await message.Content.ReadFromJsonAsync<NoteResponse>(token);
            var noteId = resp.EnsureNotNull().NoteIdExchange.EnsureNotNull().Value;
            List<string> empty = [];
            var noteResultDto = new NoteResultDto(empty, noteId);

            var id = noteResultDto.NoteIdExchange;

            if (!idStorage.TryAdd(id, 1))
            {
                idStorage[id] += 1;
            }
        }

        using var _ = new AssertionScope();

        const int buckets = 10;

        const int evaluatedBucket = 2;

        var bucket = new int[buckets];

        // assert:
        idStorage.Count
            .Should()
            .BeGreaterOrEqualTo((int)expectedNotesCount);

        Console.WriteLine("[get different '{0}' ids from '{1}' notes | by '{2}' calls]", idStorage.Count, electionTestNotesCount, tempCount);
        Console.Write("[with greater or equals to '{0}' repeats] ", evaluatedBucket);

        var evaluatedCounter = 0;
        foreach (var (_, value) in idStorage)
        {
            if (value >= evaluatedBucket)
            {
                evaluatedCounter++;
            }

            for (var i = 0; i < bucket.Length; i++)
            {
                if (value > i)
                {
                    bucket[i] += 1;
                }
            }
        }

        Console.Write($"{evaluatedCounter} unique notes");
        Console.WriteLine("\n[repeat histogram: 1 - {0}]", buckets);

        foreach (var i in bucket)
        {
            Console.Write(i + " ");
        }
    }

    // прокидываем коллекцию тк она должна принадлежать тесту
    public static IEnumerable<object?[]> ComplianceTestData => TestData.ComplianceGeneralTestData;

    [TestMethod]
    [DynamicData(nameof(ComplianceTestData))]
    public async Task ComplianceRequest_ShouldReturn_ExpectedDocumentWeights(string text, string expected)
    {
        // arrange:
        var token = CancellationToken.None;
        using var client = _factory.CreateClient(Options);
        await client.GetAsync(RouteConstants.SystemWaitWarmUpGetUrl, token);

        // act:
        var httpEncoded = HttpUtility.UrlEncode(text);
        var uriString = $"{RouteConstants.ComplianceIndicesGetUrl}?text={httpEncoded}";
        var uri = new Uri(uriString, UriKind.Relative);
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        using var message = await client.SendAsync(request, cancellationToken: token);
        message.EnsureSuccessStatusCode();
        var complianceResponse = await message.Content.ReadAsStringAsync(token);

        // assert:
        complianceResponse.Should().Be(expected);
    }
}
