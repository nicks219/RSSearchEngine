using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using SearchEngine.Configuration;
using SearchEngine.Data;
using SearchEngine.Infrastructure.Cache;
using SearchEngine.Tests.Infrastructure;
using SearchEngine.Tests.Infrastructure.DAL;

namespace SearchEngine.Tests;

[TestClass]
public class CacheTests
{
    // списки токенов соответствуют текстам из TestDataRepository:

    private readonly List<int> _testDef1 = new()
    {
        1040119440, 33759, 1030767639, 1063641, 2041410332, 1999758047, 1034259014,
        1796253404, 1201652179, 33583602, 1041276484, 1063641, 1911513819, -2036958882, 2001222215, 397889902,
        -242918757
    };

    private readonly List<int> _testUndef1 = new()
    {
        33551703, 33759, 1075359, 33449, 1034441666, 33361239, 1075421, 1034822160, 2003716344, 33790, 1087201,
        33449, 1080846, 33648454, 1993560527, 1035518482, 2031583174
    };

    private readonly List<int> _testDef2 = new()
    {
        -143480386, 1540588859, 1009732761, -143480386, 33434461, 33418, 1089433, 1932589633, 1745272967, -143480386
    };

    private readonly List<int> _testUndef2 = new()
    {
        33307888, 1720827301, 1032391667, 33307888, 1081435, 33418, 33294, 1039272458, 1032768782, 33307888
    };

    private IServiceScopeFactory _factory = Substitute.For<IServiceScopeFactory>();

    [TestInitialize]
    public void Initialize()
    {
        var host = new TestServiceProvider<CacheRepository>(true);
        _factory = new TestServiceScopeFactory(host.ServiceProvider);
    }

    [TestMethod]
    public void CacheRepository_ShouldInit_CacheLines_Correctly()
    {
        // arrange:
        var options = Substitute.For<IOptions<CommonBaseOptions>>();
        options.Value.Returns(new CommonBaseOptions { TokenizerIsEnable = true });
        var cache = new CacheRepository(_factory, options, new TestLogger<CacheRepository>());
        var def = cache.GetDefinedCache();
        var undef = cache.GetUndefinedCache();

        // act:
        def.Should().NotBeNull();
        undef.Should().NotBeNull();
        def.Count.Should().Be(1);
        undef.Count.Should().Be(1);

        // assert:
        def.ElementAt(0)
            .Value
            .Should()
            .BeEquivalentTo(_testDef1);

        undef.ElementAt(0)
            .Value
            .Should()
            .BeEquivalentTo(_testUndef1);
    }

    [TestMethod]
    public void CacheRepository_ShouldUpdate_CacheLines()
    {
        // arrange:
        var options = Substitute.For<IOptions<CommonBaseOptions>>();
        options.Value.Returns(new CommonBaseOptions { TokenizerIsEnable = true });
        var cache = new CacheRepository(_factory, options, new TestLogger<CacheRepository>());
        var def = cache.GetDefinedCache();
        var undef = cache.GetUndefinedCache();

        // act:
        cache.Update(1, new TextEntity { Title = TestDataRepository.SecondSongTitle, Song = TestDataRepository.SecondSongText });

        // assert:
        def.ElementAt(0)
            .Value
            .Should()
            .BeEquivalentTo(_testDef2);

        undef.ElementAt(0)
            .Value
            .Should()
            .BeEquivalentTo(_testUndef2);
    }

    [TestMethod]
    public void CacheRepository_ShouldCreate_CacheLines()
    {
        // arrange:
        var options = Substitute.For<IOptions<CommonBaseOptions>>();
        options.Value.Returns(new CommonBaseOptions { TokenizerIsEnable = true });
        var cache = new CacheRepository(_factory, options, new TestLogger<CacheRepository>());
        var def = cache.GetDefinedCache();
        var undef = cache.GetUndefinedCache();

        // act:
        cache.Create(2, new TextEntity
        {
            Title = TestDataRepository.SecondSongTitle,
            Song = TestDataRepository.SecondSongText
        });

        // assert:
        def.ElementAt(1)
            .Value
            .Should()
            .BeEquivalentTo(_testDef2);

        undef.ElementAt(1)
            .Value
            .Should()
            .BeEquivalentTo(_testUndef2);
    }

    [TestMethod]
    public void CacheRepository_ShouldDelete_CacheLines()
    {
        // arrange:
        var options = Substitute.For<IOptions<CommonBaseOptions>>();
        options.Value.Returns(new CommonBaseOptions { TokenizerIsEnable = true });
        var cache = new CacheRepository(_factory, options, new TestLogger<CacheRepository>());
        var def = cache.GetDefinedCache();
        var undef = cache.GetUndefinedCache();
        var defCountBefore = def.Count;
        var undefCountBefore = undef.Count;

        // act:
        cache.Delete(1);

        // assert:
        defCountBefore.Should().Be(1);
        undefCountBefore.Should().Be(1);
        def.Count.Should().Be(0);
        undef.Count.Should().Be(0);
    }

    [TestMethod]
    public void CacheRepository_WhenTokenizerDisabled_ShouldDoNothing()
    {
        // arrange:
        var options = Substitute.For<IOptions<CommonBaseOptions>>();
        options.Value.Returns(new CommonBaseOptions { TokenizerIsEnable = false });
        var cache = new CacheRepository(_factory, options, new TestLogger<CacheRepository>());
        var def = cache.GetDefinedCache();
        var undef = cache.GetUndefinedCache();

        // init asserts:
        CachesShouldBeEmpty();

        // create act & asserts:
        cache.Create(2, new TextEntity { Title = TestDataRepository.SecondSongTitle, Song = TestDataRepository.SecondSongText });
        CachesShouldBeEmpty();

        // update act & asserts:
        cache.Update(2, new TextEntity { Title = TestDataRepository.SecondSongTitle, Song = TestDataRepository.SecondSongText });
        CachesShouldBeEmpty();

        // delete act & asserts:
        cache.Delete(1);
        CachesShouldBeEmpty();

        return;

        void CachesShouldBeEmpty()
        {
            def.Should().NotBeNull();
            undef.Should().NotBeNull();
            def?.Count.Should().Be(0);
            undef?.Count.Should().Be(0);
        }
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }
}
