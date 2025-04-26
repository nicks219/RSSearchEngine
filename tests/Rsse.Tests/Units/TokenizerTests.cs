using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using SearchEngine.Common.Configuration;
using SearchEngine.Data.Entities;
using SearchEngine.Data.Repository.Contracts;
using SearchEngine.Engine.Tokenizer;
using SearchEngine.Tests.Units.Mocks;
using SearchEngine.Tests.Units.Mocks.DatabaseRepo;

namespace SearchEngine.Tests.Units;

[TestClass]
public class TokenizerTests
{
    // векторы соответствуют заметкам из TestDataRepository:

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

    private IServiceScopeFactory _factory = Substitute.For<IServiceScopeFactory>();

    [TestInitialize]
    public void Initialize()
    {
        var host = new CustomProviderWithLogger<TokenizerService>();
        _factory = new CustomScopeFactory(host.Provider);
        var repo = (TestCatalogRepository)host.Provider.GetRequiredService<IDataRepository>();
        repo.RemoveStubData(400);
    }

    [TestMethod]
    public void Tokenizer_ShouldInitExtendedAndReducedLines_Correctly()
    {
        // arrange:
        var options = Substitute.For<IOptions<CommonBaseOptions>>();
        options.Value.Returns(new CommonBaseOptions { TokenizerIsEnable = true });
        var tokenizer = new TokenizerService(_factory, options, new NoopLogger<TokenizerService>());
        CreateTestNote(tokenizer);
        var extended = tokenizer.GetExtendedLines();
        var reduced = tokenizer.GetReducedLines();

        // act:
        extended.Should().NotBeNull();
        reduced.Should().NotBeNull();
        extended.Count.Should().Be(1);
        reduced.Count.Should().Be(1);

        // assert:
        extended.ElementAt(0)
            .Value
            .Should()
            .BeEquivalentTo(_extendedFirst);

        reduced.ElementAt(0)
            .Value
            .Should()
            .BeEquivalentTo(_reducedFirst);
    }

    [TestMethod]
    public void Tokenizer_ShouldUpdate_ExtendedAndReducedLines()
    {
        // arrange:
        var options = Substitute.For<IOptions<CommonBaseOptions>>();
        options.Value.Returns(new CommonBaseOptions { TokenizerIsEnable = true });
        var tokenizer = new TokenizerService(_factory, options, new NoopLogger<TokenizerService>());
        CreateTestNote(tokenizer);
        //var extended = tokenizer.GetExtendedLines();
        //var reduced = tokenizer.GetReducedLines();

        // act:
        tokenizer.Update(1, new NoteEntity { Title = TestCatalogRepository.SecondNoteTitle, Text = TestCatalogRepository.SecondNoteText });
        var extended = tokenizer.GetExtendedLines();
        var reduced = tokenizer.GetReducedLines();

        // assert:
        extended.First()
            .Value
            .Should()
            .BeEquivalentTo(_extendedSecond);

        reduced.First()
            .Value
            .Should()
            .BeEquivalentTo(_reducedSecond);
    }

    [TestMethod]
    public void Tokenizer_ShouldCreate_ExtendedAndReducedLines()
    {
        // arrange:
        var options = Substitute.For<IOptions<CommonBaseOptions>>();
        options.Value.Returns(new CommonBaseOptions { TokenizerIsEnable = true });
        var tokenizer = new TokenizerService(_factory, options, new NoopLogger<TokenizerService>());
        var extended = tokenizer.GetExtendedLines();
        var reduced = tokenizer.GetReducedLines();

        // act:
        tokenizer.Create(2, new NoteEntity
        {
            Title = TestCatalogRepository.SecondNoteTitle,
            Text = TestCatalogRepository.SecondNoteText
        });

        // assert:
        extended.Last()
            .Value
            .Should()
            .BeEquivalentTo(_extendedSecond);

        reduced.Last()
            .Value
            .Should()
            .BeEquivalentTo(_reducedSecond);
    }

    [TestMethod]
    public void Tokenizer_ShouldDelete_ExtendedAndReducedLines()
    {
        // arrange:
        var options = Substitute.For<IOptions<CommonBaseOptions>>();
        options.Value.Returns(new CommonBaseOptions { TokenizerIsEnable = true });
        var tokenizer = new TokenizerService(_factory, options, new NoopLogger<TokenizerService>());
        CreateTestNote(tokenizer);
        var extended = tokenizer.GetExtendedLines();
        var reduced = tokenizer.GetReducedLines();
        var extendedCountBefore = extended.Count;
        var reducedCountBefore = reduced.Count;

        // act:
        tokenizer.Delete(1);

        // assert:
        extendedCountBefore.Should().Be(1);
        reducedCountBefore.Should().Be(1);
        extended.Count.Should().Be(0);
        reduced.Count.Should().Be(0);
    }

    [TestMethod]
    public void Tokenizer_WhenDisabled_ShouldDoNothing()
    {
        // arrange:
        var options = Substitute.For<IOptions<CommonBaseOptions>>();
        options.Value.Returns(new CommonBaseOptions { TokenizerIsEnable = false });
        var tokenizer = new TokenizerService(_factory, options, new NoopLogger<TokenizerService>());
        var extended = tokenizer.GetExtendedLines();
        var reduced = tokenizer.GetReducedLines();

        // init asserts:
        VectorsShouldBeEmpty();

        // create act & asserts:
        tokenizer.Create(2, new NoteEntity { Title = TestCatalogRepository.SecondNoteTitle, Text = TestCatalogRepository.SecondNoteText });
        VectorsShouldBeEmpty();

        // update act & asserts:
        tokenizer.Update(2, new NoteEntity { Title = TestCatalogRepository.SecondNoteTitle, Text = TestCatalogRepository.SecondNoteText });
        VectorsShouldBeEmpty();

        // delete act & asserts:
        tokenizer.Delete(1);
        VectorsShouldBeEmpty();

        return;

        void VectorsShouldBeEmpty()
        {
            extended.Should().NotBeNull();
            reduced.Should().NotBeNull();
            extended.Count.Should().Be(0);
            reduced.Count.Should().Be(0);
        }
    }

    private static void CreateTestNote(TokenizerService tokenizerService) =>
        tokenizerService.Create(1, new NoteEntity
        {
            Title = TestCatalogRepository.FirstNoteTitle,
            Text = TestCatalogRepository.FirstNoteText
        });

    [TestCleanup]
    public void TestCleanup()
    {
    }
}
