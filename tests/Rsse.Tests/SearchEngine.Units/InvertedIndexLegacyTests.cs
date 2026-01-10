using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleEngine.Dto.Common;
using SimpleEngine.Indexes;

namespace Rsse.Tests.SearchEngine.Units;

[TestClass]
public class InvertedIndexLegacyTests
{
    // токены в документах дублируются
    private static readonly TokenVector VectorFirst =  new([10,20,20,30,40,10]);
    private static readonly TokenVector VectorSecond =  new([50,60,60,70,80,50]);
    private static readonly TokenVector VectorThird =  new([20,30,30,40,90,20]);

    [TestMethod]
    public void Index_WhenAddIdenticalTokensToTwoDocuments_ShouldContainTwoIdAndAllTokens()
    {
        // arrange
        var idFirst = new DocumentId(1);
        var idSecond = new DocumentId(2);
        var index = new InvertedIndexLegacy();

        // act
        index.TryAddDocument(idFirst, VectorFirst);
        index.TryAddDocument(idFirst, VectorFirst);
        index.TryAddDocument(idSecond, VectorFirst);

        // assert
        index.TokenCount.Should().Be(VectorFirst.DistinctAndGet().Count);
        foreach (var token in VectorFirst)
        {
            index.TryGetIds(token, out var ids).Should().BeTrue();
            ids.Should().NotBeNull();
            ids.Count.Should().Be(2);
        }
    }

    [TestMethod]
    public void Index_WhenDeleteOneOfTwoDocument_ShouldRemainOneId()
    {
        // arrange
        var idFirst = new DocumentId(1);
        var idSecond = new DocumentId(2);
        var index = new InvertedIndexLegacy();

        // act
        index.TryAddDocument(idFirst, VectorFirst);
        index.TryAddDocument(idFirst, VectorFirst);
        index.TryAddDocument(idSecond, VectorFirst);
        index.TryRemoveDocument(idFirst, VectorFirst);

        // assert
        index.TokenCount.Should().Be(VectorFirst.DistinctAndGet().Count);
        foreach (var token in VectorFirst)
        {
            index.TryGetIds(token, out var ids);
            ids.Should().NotBeNull();
            ids.Count.Should().Be(1);
            ids.First().Should().Be(idSecond);
        }
    }

    [TestMethod]
    public void Index_WhenClear_ShouldRemainEmpty()
    {
        // arrange
        var idFirst = new DocumentId(1);
        var idSecond = new DocumentId(2);
        var index = new InvertedIndexLegacy();

        // act
        index.TryAddDocument(idFirst, VectorFirst);
        index.TryAddDocument(idFirst, VectorFirst);
        index.TryAddDocument(idSecond, VectorFirst);
        index.Clear();

        // assert
        index.TokenCount.Should().Be(0);
        foreach (var token in VectorFirst)
        {
            index.TryGetIds(token, out var ids);
            ids.Should().BeNull();
        }
    }

    [TestMethod]
    public void Index_WhenRemoveToken_ShouldContainRemainingTokens()
    {
        // arrange
        var idFirst = new DocumentId(1);
        var idSecond = new DocumentId(2);
        var index = new InvertedIndexLegacy();
        var removedToken = VectorFirst.ElementAt(0);

        // act
        index.TryAddDocument(idFirst, VectorFirst);
        index.TryAddDocument(idFirst, VectorFirst);
        index.TryAddDocument(idSecond, VectorFirst);
        var isRemoved = index.TryRemoveToken(removedToken, out var removedIds);

        // assert
        isRemoved.Should().BeTrue();
        removedIds.Should().NotBeNull();
        removedIds.Count.Should().Be(2);
        index.TokenCount.Should().Be(VectorFirst.DistinctAndGet().Count - 1);
        index.TryGetIds(removedToken, out var notPresentIds);
        notPresentIds.Should().BeNull();
    }

    //////////////////
    // update tests //
    //////////////////

    [TestMethod]
    public void Index_WhenUpdateDocumentToIdentical_ShouldNotChanged()
    {
        // апдейт на аналогичный вектор

        // arrange
        var idFirst = new DocumentId(1);
        var idSecond = new DocumentId(2);
        var index = new InvertedIndexLegacy();

        // act
        index.TryAddDocument(idFirst, VectorFirst);
        index.TryAddDocument(idFirst, VectorFirst);
        index.TryAddDocument(idSecond, VectorFirst);
        index.TryUpdateDocument(documentId: idFirst, newTokenVector: VectorFirst, oldTokenVector: VectorFirst);

        // assert
        index.TokenCount.Should().Be(VectorFirst.DistinctAndGet().Count);
        foreach (var token in VectorFirst)
        {
            index.TryGetIds(token, out var ids).Should().BeTrue();
            ids.Should().NotBeNull();
            ids.Count.Should().Be(2);
        }
    }

    [TestMethod]
    public void Index_WhenUpdateDocumentToEmpty_ShouldRemainOneId()
    {
        // апдейт на пустой вектор

        // arrange
        var idFirst = new DocumentId(1);
        var idSecond = new DocumentId(2);
        var index = new InvertedIndexLegacy();

        // act
        index.TryAddDocument(idFirst, VectorFirst);
        index.TryAddDocument(idFirst, VectorFirst);
        index.TryAddDocument(idSecond, VectorFirst);
        index.TryUpdateDocument(documentId: idFirst, newTokenVector: TokenVector.Empty, oldTokenVector: VectorFirst);

        // assert
        index.TokenCount.Should().Be(VectorFirst.DistinctAndGet().Count);
        foreach (var token in VectorFirst)
        {
            index.TryGetIds(token, out var ids).Should().BeTrue();
            ids.Should().NotBeNull();
            ids.Count.Should().Be(1);
        }
    }

    [TestMethod]
    public void Index_WhenUpdateOneOfTwoDocumentsToNewCompletely_ShouldContainTwoVectors()
    {
        // апдейт на полностью новый вектор

        // arrange
        var idFirst = new DocumentId(1);
        var idSecond = new DocumentId(2);
        var index = new InvertedIndexLegacy();

        // act
        index.TryAddDocument(idFirst, VectorFirst);
        index.TryAddDocument(idFirst, VectorFirst);
        index.TryAddDocument(idSecond, VectorFirst);
        index.TryUpdateDocument(documentId: idFirst, newTokenVector: VectorSecond, oldTokenVector: VectorFirst);

        // assert
        index.TokenCount.Should().Be(VectorFirst.DistinctAndGet().Count + VectorSecond.DistinctAndGet().Count);
        foreach (var token in VectorFirst)
        {
            index.TryGetIds(token, out var ids).Should().BeTrue();
            ids.Should().NotBeNull();
            ids.Count.Should().Be(1);
            // первый вектор остаётся только для второго документа
            ids.First().Should().Be(idSecond);
        }
        foreach (var token in VectorSecond)
        {
            index.TryGetIds(token, out var ids).Should().BeTrue();
            ids.Should().NotBeNull();
            ids.Count.Should().Be(1);
            ids.First().Should().Be(idFirst);
        }
    }

    [TestMethod]
    public void Index_WhenUpdateDocumentToNewPartially_ShouldContainNewVector()
    {
        // апдейт с добавлением и удалением токенов
        // подумать: если ids токена остаются пустыми, то удаляю токен из индекса
        // [10,20,30,40] -> [20,30,40,90] | токен 10 остаётся с пустым списком

        // arrange
        var idFirst = new DocumentId(1);
        var index = new InvertedIndexLegacy();

        // act
        index.TryAddDocument(idFirst, VectorFirst);
        index.TryAddDocument(idFirst, VectorFirst);
        index.TryUpdateDocument(documentId: idFirst, newTokenVector: VectorThird, oldTokenVector: VectorFirst);

        // assert
        index.TokenCount.Should().Be(VectorThird.DistinctAndGet().Count);
        foreach (var token in VectorThird)
        {
            index.TryGetIds(token, out var ids).Should().BeTrue();
            ids.Should().NotBeNull();
            ids.Count.Should().Be(1);
            ids.First().Should().Be(idFirst);
        }
    }

    [TestMethod]
    public void Index_InconsistentOperations_ShouldNotThrowOrCorruptData()
    {
        // тесты с некорректными данными

        // arrange
        var idFirst = new DocumentId(1);
        var idSecond = new DocumentId(2);
        var index = new InvertedIndexLegacy();

        // act & assert
        // add: пустой вектор не добавляется
        index.TryAddDocument(idFirst, new TokenVector());
        index.TokenCount.Should().Be(0);

        // remove token: отсутствует token
        index
            .TryRemoveToken(new Token(100), out _)
            .Should()
            .BeFalse();
        index.TokenCount.Should().Be(0);

        // update: отсутствует token
        index
            .TryUpdateDocument(documentId: idSecond, newTokenVector: VectorThird, oldTokenVector: VectorFirst)
            .Should()
            .BeFalse();

        // update: отсутствует id
        index.TryAddDocument(idFirst, VectorFirst);
        index
            .TryUpdateDocument(documentId: idSecond, newTokenVector: VectorThird, oldTokenVector: VectorFirst)
            .Should()
            .BeFalse();
        index.TokenCount.Should().Be(VectorFirst.DistinctAndGet().Count);

        // remove id: отсутствует token
        index
            .TryRemoveDocument(idFirst, VectorSecond)
            .Should()
            .BeFalse();
        index.TokenCount.Should().Be(VectorFirst.DistinctAndGet().Count);

        // remove id: отсутствует id
        index
            .TryRemoveDocument(idSecond, VectorFirst)
            .Should()
            .BeFalse();
        index.TokenCount.Should().Be(VectorFirst.DistinctAndGet().Count);

        // assert: неуспешные попытки не изменили добавленные данные
        foreach (var token in VectorFirst)
        {
            index.TryGetIds(token, out var ids).Should().BeTrue();
            ids.Should().NotBeNull();
            ids.Count.Should().Be(1);
            ids.First().Should().Be(idFirst);
        }
    }
}
