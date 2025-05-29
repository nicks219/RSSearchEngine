using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rsse.Generator;

/// <summary>
/// Генератор констант с роутами API для клиента.
/// </summary>
// NB: Если переделать на publish, то запуск из папки scr: dotnet publish Rsse.Generator -c Release -o ../src/Rsse.Service/tools
[ExcludeFromCodeCoverage]
internal abstract class Generator
{
    private static async Task<int> Main(string[] args)
    {
        if (args.Length != 3)
        {
            await Console.Error.WriteLineAsync("Usage: Rsse.Generator <dllPath> <className> <outputFile>");
            return 1;
        }

        var dllPath = args[0];
        var constantsClass = args[1];
        var outputFile = args[2];

        var isTsx = outputFile.EndsWith(".tsx");
        var isJson = outputFile.EndsWith(".json");
        try
        {
            var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
            var assemblies = Directory.GetFiles(runtimeDir, "*.dll");
            var resolver = new PathAssemblyResolver(assemblies);
            using var metadataLoadContext = new MetadataLoadContext(resolver, "System.Private.CoreLib");
            var assembly = metadataLoadContext.LoadFromAssemblyPath(dllPath);

            var type = assembly.GetType(constantsClass) ?? throw new Exception($"Class {constantsClass} not found");

            var fields = type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f is { IsLiteral: true, IsInitOnly: false })
                .ToDictionary(f => f.Name, f => f.GetRawConstantValue());

            await using var sw = new StreamWriter(outputFile);
            if (isJson)
            {
                await sw.WriteLineAsync("{");
                var last = fields.Last();
                foreach (var pair in fields)
                {
                    var comma = pair.Key == last.Key ? "" : ",";
                    await sw.WriteLineAsync($"  \"{char.ToLower(pair.Key[0])}{pair.Key[1..]}\": \"{pair.Value}\"{comma}");
                }

                await sw.WriteLineAsync("}");
                return 1;
            }

            if (!isTsx) return 0;

            await sw.WriteLineAsync("export const RouteConstants = {");
            var lastKvp = fields.Last();
            foreach (var pair in fields)
            {
                var comma = pair.Key == lastKvp.Key ? "" : ",";
                await sw.WriteLineAsync($"  {char.ToLower(pair.Key[0])}{pair.Key[1..]}: \"{pair.Value}\"{comma}");
            }

            await sw.WriteLineAsync("} as const;");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("{0} crashed: {1}", nameof(Rsse.Generator), ex);
            return 1;
        }
    }
}
