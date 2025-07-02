using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rsse.Api.Services;
using Rsse.Domain.Service.Contracts;
using Rsse.Infrastructure.Context;
using Rsse.Tests.Integration.FakeDb.Api;
using Rsse.Tests.Integration.FakeDb.Extensions;
using Rsse.Tests.Units;
using RsseEngine.Service;
using static Rsse.Domain.Service.Configuration.RouteConstants;

namespace Rsse.Tests.Integration.FakeDb;

/// <summary>
/// Тестирование поисковых метрик на всех алгоритмах на актуальном объеме данных.
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
        await using var context = (NpgsqlCatalogContext)_factory?.Services.GetService(typeof(NpgsqlCatalogContext))!;
        var db = context.Database;
        // метрики будут соответствовать тесту ReadOnlyIntegrationTests.ComplianceRequest_ShouldReturn_ExpectedDocumentWeights если убрать \r\n
        // await db.ExecuteSqlRawAsync(ExampleWithoutLineEnds2, NoneToken);
        await db.ExecuteSqlRawAsync(Example10, NoneToken);
        await db.ExecuteSqlRawAsync(Example444, NoneToken);
        await db.ExecuteSqlRawAsync(Example243, NoneToken);

        // Для поиска необходимо заново инициализировать токенайзер.
        var tokenizer = (ITokenizerApiClient)_factory?.Services.GetService(typeof(ITokenizerApiClient))!;
        var dataProvider = (DbDataProvider)_factory?.Services.GetService(typeof(DbDataProvider))!;
        await tokenizer.Initialize(dataProvider, NoneToken);
    }

    [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
    public static void ClassCleanup() => _factory?.Dispose();

    [TestMethod]
    [DataRow("чёрт с ними за столом", """{"res":{"1":59.523809523809526},"error":null}""")]
    [DataRow("поём на", """{"res":{"1":23.80952380952381},"error":null}""")]
    [DataRow("с ними за столом чёрт", """{"res":{"1":4.761904761904762},"error":null}""")]
    [DataRow("чорт з ным зо сталом", """{"res":{"1":0.4878048780487805},"error":null}""")]

    [DataRow("ты шла по палубе в молчаний", """{"res":{"10":0.0641025641025641},"error":null}""")]
    [DataRow("оно шла по палубе в молчаний", """{"res":{"10":0.0641025641025641},"error":null}""")]

    [DataRow("преключиться вдруг верный друг", """{"res":{"444":0.38095238095238093,"243":0.02112676056338028},"error":null}""")]
    [DataRow("приключится вдруг верный друг", """{"res":{"444":37.38317757009346},"error":null}""")]

    // Ошибочные поисковые запросы.
    [DataRow("пляшем на", """{"res":null,"error":null}""")]
    [DataRow("123 456 иии", """{"res":null,"error":null}""")]
    [DataRow("aa bb cc dd .,/#", """{"res":null,"error":null}""")]
    [DataRow(" |", """{"res":null,"error":null}""")]
    public async Task ComplianceRequest_ShouldReturn_ExpectedMetrics(string request, string expected)
    {
        foreach (var extendedSearchType in ComplianceTests.ExtendedSearchTypes)
        {
            foreach (var reducedSearchTypes in ComplianceTests.ReducedSearchTypes)
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
                // Console.WriteLine(request + " " + complianceResponse);
                complianceResponse.Should().Be(expected);
            }
        }
    }

    private const string ExampleWithoutLineEnds2 =
        """
        INSERT INTO [Note] ([NoteId], [Title], [Text]) VALUES (2,'Розенбаум - Вечерняя застольная','Чёрт с ними! ' ||
                                                       'За столом сидим, поём, пляшем… Поднимем эту чашу за детей наших И скинем с головы иней, ' ||
                                                       'Поднимем, поднимем. За утро и за свежий из полей ветер, За друга, не дожившего до дней этих, ' ||
                                                       'За память, что живёт с нами, Затянем, затянем. Бог в помощь всем живущим на Земле людям, ' ||
                                                       'Мир дому, где собак и лошадей любят. За силу, что несут волны, ' ||
                                                       'По полной, по полной. Родные, нас живых ещё не так мало, Поднимем за удачу на тропе шалой, ' ||
                                                       'Чтоб ворон да не по нам каркал, По чарке, по чарке…');
        """;

    private const string Example10 =
        """
        INSERT INTO [Note] ([NoteId], [Title], [Text]) VALUES (10,'Антонов Юрий - Белый теплоход','Я засмотрелся на тебя,\n' ||
                                                       'Ты шла по палубе в молчании,\nИ тихо белый теплоход\nОт шумной пристани отчалил,\n' ||
                                                       'И закружил меня он вдруг,\nМеня он закачал,\nА за кормою уплывал\nВеселый морвокзал.\n\n' ||
                                                       'Припев:\nАх, белый теплоход, гудка тревожный бас,\nКрик чаек за кормой, сиянье синих глаз,\n' ||
                                                       'Ах, белый теплоход, бегущая вода,\nУносишь ты меня, скажи, куда?\n\nА теплоход по морю плыл,\n' ||
                                                       'Бежали волны за кормой,\nИ ветер ласковый морской,\nРазвеселясь, играл с тобой.\n' ||
                                                       'И засмотревшись на тебя,\nНе зная, почему,\nЯ в этот миг, как никогда,\nЗавидовал ему.\n\n' ||
                                                       'Припев 2 раза (тот же)');
        """;

    private const string Example243 =
        """
        INSERT INTO [Note] ([NoteId], [Title], [Text]) VALUES (243,'Харатьян Дмитрий - Песня о любви','Как год без весны, весна без листвы,\n' ||
                                                       'Листва без грозы и гроза без молний,\nТак годы скучны без права любви:\n' ||
                                                       'Лететь на призыв или стон безмолвный твой.\nТак годы скучны без права любви:\n' ||
                                                       'Лететь на призыв или стон безмолвный твой.\n\nУвы, не предскажешь беду.\n' ||
                                                       'Зови, я удар отведу.\nПусть голову сам за это отдам,\nГадать о цене - не по мне, любимая.\n\n ' ||
                                                       'Дороги любви для нас нелегки,\nЗато к нам добры белый мох и клевер.\n' ||
                                                       'Полны соловьи счастливой тоски,\nИ весны щедры, возвратясь на север к нам.\n' ||
                                                       'Полны соловьи щемящей тоски,\nИ весны щедры, возвратясь на север к нам.\n\n' ||
                                                       'Земля, где так много разлук,\nСама повенчает нас вдруг.\nЗа то, что верны мы птицам весны,\n' ||
                                                       'Они и зимой нам слышны, любимая.\n\nЗемля, где так много разлук,\nСама повенчает нас вдруг.\n' ||
                                                       'За то, что верны мы птицам весны,\nОни и зимой нам слышны, любимая.\n' ||
                                                       'За то, что верны мы птицам весны,\nОни и зимой нам слышны, любимая.');
        """;

    private const string Example444 =
        """
        INSERT INTO [Note] ([NoteId], [Title], [Text]) VALUES (444,'Детские - Настоящий друг','Дружба крепкая не сломается,\n' ||
                                                       'Не расклеится от дождей и вьюг.\nДруг в беде не бросит, лишнего не спросит,\n' ||
                                                       'Вот что значит настоящий верный друг.\nДруг в беде не бросит, лишнего не спросит,\n' ||
                                                       'Вот что значит настоящий верный друг.\n\nМы поссоримся и помиримся,\n\' ||
                                                       '"Не разлить водой\" - шутят все вокруг.\nВ полдень или в полночь друг придет на помощь,\n' ||
                                                       'Вот что значит настоящий верный друг.\nВ полдень или в полночь друг придет на помощь,\n' ||
                                                       'Вот что значит настоящий верный друг.\n\nДруг всегда меня сможет выручить,\n' ||
                                                       'Если что-нибудь приключится вдруг.\nНужным быть кому-то в трудную минуту -\n' ||
                                                       'Вот что значит настоящий верный друг.\nНужным быть кому-то в трудную минуту -\n' ||
                                                       'Вот что значит настоящий верный друг.');
        """;
}
