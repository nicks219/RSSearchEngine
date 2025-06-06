using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Spans;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace SearchEngine.Benchmarks.Common;

public static class LuceneTokenizer
{
    private static readonly RAMDirectory Dir = new();

    public static async Task InitializeLucene()
    {
        var dataProvider = new FileDataProvider();
        await foreach (var entity in dataProvider.GetDataAsync())
        {
            var text = entity.Title + " " + entity.Text;
            Add(text);
        }
    }

    private static void Add(string text)
    {
        const LuceneVersion appLuceneVersion = LuceneVersion.LUCENE_48;
        var analyzer = new StandardAnalyzer(appLuceneVersion);

        using var writer = new IndexWriter(Dir, new IndexWriterConfig(appLuceneVersion, analyzer));
        var doc = new Document
        {
            new TextField("content", text, Field.Store.YES)
        };
        writer.AddDocument(doc);
        writer.Commit();
    }

    public static IEnumerable<string> Find(string text)
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
        foreach (var hit in hits)
        {
            var foundDoc = searcher.Doc(hit.Doc);
            yield return foundDoc.Get("content");
        }
    }
}
