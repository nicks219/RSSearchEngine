using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using SearchEngine.Api.Services;
using SearchEngine.Data.Contracts;
using SearchEngine.Data.Dto;
using SearchEngine.Service.Configuration;
using SearchEngine.Service.Contracts;
using SearchEngine.Tests.Units.Mocks;
using SearchEngine.Tests.Units.Mocks.Repo;

namespace SearchEngine.Tests.Units;

[TestClass]
public class TokenizerTests
{
    public required IServiceScopeFactory ScopeFactory;
    public required ITokenizerProcessorFactory ProcessorFactory;

    // векторы соответствуют заметкам из FakeCatalogRepository

    private readonly List<int> _extendedFirst =
    [
        1040119440, 33759, 1030767639, 1063641, 2041410332, 1999758047, 1034259014,
        1796253404, 1201652179, 33583602, 1041276484, 1063641, 1911513819, -2036958882, 2001222215, 397889902,
        -242918757
    ];

    private readonly List<int> _reducedFirst =
    [
        33551703, 33759, 1075359, 33449, 1034441666, 33361239, 1075421, 1034822160, 2003716344, 33790, 1087201,
        33449, 1080846, 33648454, 1993560527, 1035518482, 2031583174
    ];

    private readonly List<int> _extendedSecond =
        [-143480386, 1540588859, 1009732761, -143480386, 33434461, 33418, 1089433, 1932589633, 1745272967, -143480386];

    private readonly List<int> _reducedSecond =
        [33307888, 1720827301, 1032391667, 33307888, 1081435, 33418, 33294, 1039272458, 1032768782, 33307888];

    [TestInitialize]
    public void Initialize()
    {
        var host = new ServiceProviderStub();
        ScopeFactory = host.Provider.GetRequiredService<IServiceScopeFactory>();
        ProcessorFactory = host.Provider.GetRequiredService<ITokenizerProcessorFactory>();
        var repo = (FakeCatalogRepository)host.Provider.GetRequiredService<IDataRepository>();
        repo.RemoveStubData(400);
    }

    [TestMethod]
    public async Task Tokenizer_ShouldInit_ExtendedAndReducedLines()
    {
        // arrange:
        var options = Substitute.For<IOptions<CommonBaseOptions>>();
        options.Value.Returns(new CommonBaseOptions { TokenizerIsEnable = true });
        var tokenizer = new TokenizerService(ScopeFactory, ProcessorFactory, options, new NoopLogger<TokenizerService>());
        await CreateTestNote(tokenizer);
        var tokenLines = tokenizer.GetTokenLines();

        // act:
        tokenLines.Should().NotBeNull();
        tokenLines.Count.Should().Be(1);

        // assert:
        tokenLines.ElementAt(0)
            .Value
            .Extended
            .Should()
            .BeEquivalentTo(_extendedFirst);

        tokenLines.ElementAt(0)
            .Value
            .Reduced
            .Should()
            .BeEquivalentTo(_reducedFirst);
    }

    [TestMethod]
    public async Task Tokenizer_ShouldUpdate_ExtendedAndReducedLines()
    {
        // arrange:
        var options = Substitute.For<IOptions<CommonBaseOptions>>();
        options.Value.Returns(new CommonBaseOptions { TokenizerIsEnable = true });
        var tokenizer = new TokenizerService(ScopeFactory, ProcessorFactory, options, new NoopLogger<TokenizerService>());
        await CreateTestNote(tokenizer);

        // act:
        await tokenizer.Update(1,
            new TextRequestDto
            {
                Title = FakeCatalogRepository.SecondNoteTitle,
                Text = FakeCatalogRepository.SecondNoteText
            });
        var tokenLines = tokenizer.GetTokenLines();

        // assert:
        tokenLines.First()
            .Value
            .Extended
            .Should()
            .BeEquivalentTo(_extendedSecond);

        tokenLines.First()
            .Value
            .Reduced
            .Should()
            .BeEquivalentTo(_reducedSecond);
    }

    [TestMethod]
    public async Task Tokenizer_ShouldCreate_ExtendedAndReducedLines()
    {
        // arrange:
        var options = Substitute.For<IOptions<CommonBaseOptions>>();
        options.Value.Returns(new CommonBaseOptions { TokenizerIsEnable = true });
        var tokenizer = new TokenizerService(ScopeFactory, ProcessorFactory, options, new NoopLogger<TokenizerService>());
        var tokenLines = tokenizer.GetTokenLines();

        // act:
        await tokenizer.Create(2, new TextRequestDto
        {
            Title = FakeCatalogRepository.SecondNoteTitle,
            Text = FakeCatalogRepository.SecondNoteText
        });

        // assert:
        tokenLines.Last()
            .Value
            .Extended
            .Should()
            .BeEquivalentTo(_extendedSecond);

        tokenLines.Last()
            .Value
            .Reduced
            .Should()
            .BeEquivalentTo(_reducedSecond);
    }

    [TestMethod]
    public async Task Tokenizer_ShouldDelete_ExtendedAndReducedLines()
    {
        // arrange:
        var options = Substitute.For<IOptions<CommonBaseOptions>>();
        options.Value.Returns(new CommonBaseOptions { TokenizerIsEnable = true });
        var tokenizer = new TokenizerService(ScopeFactory, ProcessorFactory, options, new NoopLogger<TokenizerService>());
        await CreateTestNote(tokenizer);
        var tokenLines = tokenizer.GetTokenLines();
        var countBefore = tokenLines.Count;

        // act:
        await tokenizer.Delete(1);
        var countAfter = tokenLines.Count;

        // assert:
        countBefore.Should().Be(1);
        countAfter.Should().Be(0);
    }

    [TestMethod]
    public async Task Tokenizer_ShouldDoNothing_WhenDisabled()
    {
        // arrange:
        var options = Substitute.For<IOptions<CommonBaseOptions>>();
        options.Value.Returns(new CommonBaseOptions { TokenizerIsEnable = false });
        var tokenizer = new TokenizerService(ScopeFactory, ProcessorFactory, options, new NoopLogger<TokenizerService>());
        var tokenLines = tokenizer.GetTokenLines();

        // init asserts:
        VectorsShouldBeEmpty();

        // create act & asserts:
        await tokenizer.Create(2,
            new TextRequestDto
            {
                Title = FakeCatalogRepository.SecondNoteTitle,
                Text = FakeCatalogRepository.SecondNoteText
            });
        VectorsShouldBeEmpty();

        // update act & asserts:
        await tokenizer.Update(2,
            new TextRequestDto
            {
                Title = FakeCatalogRepository.SecondNoteTitle,
                Text = FakeCatalogRepository.SecondNoteText
            });
        VectorsShouldBeEmpty();

        // delete act & asserts:
        await tokenizer.Delete(1);
        VectorsShouldBeEmpty();

        return;

        void VectorsShouldBeEmpty()
        {
            tokenLines.Should().NotBeNull();
            tokenLines.Count.Should().Be(0);
        }
    }

    private static async Task CreateTestNote(TokenizerService tokenizerService) =>
        await tokenizerService.Create(id: 1, new TextRequestDto
        {
            Title = FakeCatalogRepository.FirstNoteTitle,
            Text = FakeCatalogRepository.FirstNoteText
        });
}
