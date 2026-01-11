using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Spans;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Rsse.Tests.Common;

namespace RD.RsseEngine.Benchmarks.Common;

/// <summary>
/// Компонент с Lucene.
/// </summary>
public static class LuceneWrapper
{
    private static readonly RAMDirectory Dir = new();

    /// <summary>
    /// Инициализировать Lucene набором заметок.
    /// </summary>
    public static async Task InitializeAsync()
    {
        const LuceneVersion appLuceneVersion = LuceneVersion.LUCENE_48;
        var analyzer = new StandardAnalyzer(appLuceneVersion);
        using var writer = new IndexWriter(Dir, new IndexWriterConfig(appLuceneVersion, analyzer));

        var dataProvider = new FileDataMultipleProvider();
        await foreach (var entity in dataProvider.GetDataAsync())
        {
            var text = entity.Title + " " + entity.Text;
            var doc = new Document
            {
                new TextField("content", text, Field.Store.YES)
            };
            writer.AddDocument(doc);
        }

        writer.Commit();
    }

    /// <summary>
    /// Найти по тексту список идентификаторов подходящих текстов.
    /// </summary>
    /// <param name="text">Текст для поиска.</param>
    /// <returns>Список идентификаторов найденных текстов.</returns>
    public static List<int> Find(string text)
    {
        var split = text.Split(' ');
        var clauses = new SpanQuery[split.Length];
        for (var i = 0; i < clauses.Length; i++)
        {
            clauses[i] = new SpanMultiTermQueryWrapper<FuzzyQuery>
                (new FuzzyQuery(new Term("content", split[i]), 2));
        }

        var nearQuery = new SpanNearQuery(clauses, 30, false);

        using var reader = DirectoryReader.Open(Dir);
        var searcher = new IndexSearcher(reader);

        var hits = searcher.Search(nearQuery, 10).ScoreDocs;
        var ids = hits.Select(x => x.Doc).ToList();

        return ids;
    }
}
