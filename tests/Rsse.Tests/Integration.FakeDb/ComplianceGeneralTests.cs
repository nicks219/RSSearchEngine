using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rsse.Api.Services;
using Rsse.Domain.Data.Entities;
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
        FileDataProvider fileDataProvider = new FileDataProvider();

        _factory = new CustomWebAppFactory<SqliteStartup>();
        using var client = _factory.CreateClient(Options);
        // Cигнатура выбрана из-за конфликта в перегрузках `Microsoft.Testing`.
        await using var context = (NpgsqlCatalogContext)_factory?.Services.GetService(typeof(NpgsqlCatalogContext))!;
        var db = context.Database;

        // метрики будут соответствовать тесту ReadOnlyIntegrationTests.ComplianceRequest_ShouldReturn_ExpectedDocumentWeights если убрать \r\n

        // await db.ExecuteSqlRawAsync(ExampleWithoutLineEnds2, NoneToken);
        //await db.ExecuteSqlRawAsync(Example10, NoneToken);
        //await db.ExecuteSqlRawAsync(Example444, NoneToken);
        //await db.ExecuteSqlRawAsync(Example243, NoneToken);

        await using (DbConnection connection = db.GetDbConnection())
        {
            await connection.OpenAsync();

            await using (DbCommand cmd = connection.CreateCommand())
            {
                // TODO удаляем всё из таблицы потому что DatabaseInitializer зачем то вставляет одну запись - запись мешается
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "DELETE FROM [Note]";

                var result = await cmd.ExecuteNonQueryAsync();
            }

            await foreach (NoteEntity noteEntity in fileDataProvider.GetDataAsync())
            {
                await using (DbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "INSERT INTO [Note] ([NoteId], [Title], [Text]) VALUES (@param1, @param2, @param3)";

                    DbParameter param1 = cmd.CreateParameter();
                    param1.ParameterName = "param1";
                    param1.Value = noteEntity.NoteId;
                    param1.DbType = DbType.Int32;
                    cmd.Parameters.Add(param1);

                    DbParameter param2 = cmd.CreateParameter();
                    param2.ParameterName = "param2";
                    param2.Value = noteEntity.Title;
                    param2.DbType = DbType.String;
                    cmd.Parameters.Add(param2);

                    DbParameter param3 = cmd.CreateParameter();
                    param3.ParameterName = "param3";
                    param3.Value = noteEntity.Text;
                    param3.DbType = DbType.String;
                    cmd.Parameters.Add(param3);

                    var result = await cmd.ExecuteNonQueryAsync();
                    if (result != 1)
                    {
                        throw new Exception("INSERT не прошёл");
                    }
                }
            }

            // можно проверить что вставили
            /*await using (DbCommand cmd = connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM [Note]";

                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var result0 = reader.GetInt64(0);
                    var result1 = reader.GetString(1);
                    var result2 = reader.GetString(2);

                    Console.WriteLine($"\t{result0}\t{result1}");
                }

                await reader.CloseAsync();
            }*/
        }

        // Для поиска необходимо заново инициализировать токенайзер.
        var tokenizer = (ITokenizerApiClient)_factory?.Services.GetService(typeof(ITokenizerApiClient))!;
        var dataProvider = (DbDataProvider)_factory?.Services.GetService(typeof(DbDataProvider))!;
        await tokenizer.Initialize(dataProvider, NoneToken);
    }

    [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
    public static void ClassCleanup() => _factory?.Dispose();

    [TestMethod]
    // TODO тут добавлены новые метрики помимо ReadOnlyIntegrationTests.ComplianceRequest_ShouldReturn_ExpectedDocumentWeights
    /*
    [DataRow("чёрт с ними за столом", """{"res":{"1":52.631578947368425},"error":null}""")]
    [DataRow("поём на", """{"res":{"456":31.25,"250":28.169014084507044,"1":21.05263157894737,"275":15.748031496062993},"error":null}""")]
    [DataRow("с ними за столом чёрт", """{"res":{"1":4.2105263157894735},"error":null}""")]
    [DataRow("чорт з ным зо сталом", """{"res":{"1":0.43478260869565216},"error":null}""")]

    [DataRow("ты шла по палубе в молчаний", """{"res":{"10":5.154639175257731},"error":null}""")]
    [DataRow("оно шла по палубе в молчаний", """{"res":{"10":0.6818181818181818},"error":null}""")]

    [DataRow("преключиться вдруг верный друг", """{"res":{"444":0.35714285714285715,"243":0.02},"error":null}""")]
    [DataRow("приключится вдруг верный друг", """{"res":{"444":35.08771929824562},"error":null}""")]

    // Ошибочные поисковые запросы.
    [DataRow("пляшем на", """{"res":{"1":21.05263157894737},"error":null}""")]
    [DataRow("123 456 иии", """{"res":null,"error":null}""")]
    [DataRow("aa bb cc dd .,/#", """{"res":null,"error":null}""")]
    [DataRow(" |", """{"res":null,"error":null}""")]
    */

    // TODO Скопировал метрики из ReadOnlyIntegrationTests.ComplianceRequest_ShouldReturn_ExpectedDocumentWeights

    // Валидные поисковые запросы.
    [DataRow("чорт з ным зо сталом", """{"res":{"1":0.43478260869565216},"error":null}""")]
    [DataRow("чёрт с ними за столом", """{"res":{"1":52.631578947368425},"error":null}""")]
    [DataRow("с ними за столом чёрт", """{"res":{"1":4.2105263157894735},"error":null}""")]
    [DataRow("преключиться вдруг верный друг", """{"res":{"444":0.35714285714285715,"243":0.02},"error":null}""")]
    [DataRow("приключится вдруг верный друг", """{"res":{"444":35.08771929824562},"error":null}""")]
    [DataRow("пляшем на", """{"res":{"1":21.05263157894737},"error":null}""")]
    [DataRow("ты шла по палубе в молчаний", """{"res":{"10":5.154639175257731},"error":null}""")]
    [DataRow("оно шла по палубе в молчаний", """{"res":{"10":0.6818181818181818},"error":null}""")]
    // Мусорные поисковые запросы.
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
                complianceResponse.Should().Be(expected, $"[ExtendedSearchType.{extendedSearchType} ReducedSearchType.{reducedSearchTypes}]");
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
