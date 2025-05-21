using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace SearchEngine.Tooling.Generators;

/// <summary>
/// Билд-таска для генерации json/tsx по файлу с константами.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class GenerateConstantsJsonTask : Task
{
    // <summary/> Именование класса с константами.
    [Required] public string ConstantsClass { get; set; }

    // <summary/> Файл с результатами, *.tst либо *.json.
    [Required] public string OutputFile { get; set; }

    // <summary/> Путь к сборке.
    [Required] public string DllPath { get; set; }

    /// <summary>
    /// Выполнить билд-таску.
    /// </summary>
    public override bool Execute()
    {
        var isTsx = OutputFile.EndsWith(".tsx");
        var isJson = OutputFile.EndsWith(".json");
        try
        {
            var dllPath = DllPath;
            var asm = Assembly.LoadFrom(dllPath);
            var type = asm.GetType(ConstantsClass) ?? throw new Exception($"Class {ConstantsClass} not found");

            var fields = type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f is { IsLiteral: true, IsInitOnly: false })
                .ToDictionary(f => f.Name, f => f.GetValue(null));

            using var sw = new StreamWriter(OutputFile);
            if (isJson)
            {
                sw.WriteLine("{");
                var last = fields.Last();
                foreach (var pair in fields)
                {
                    var comma = pair.Key == last.Key ? "" : ",";
                    sw.WriteLine($"  \"{char.ToLower(pair.Key[0])}{pair.Key.Substring(1)}\": \"{pair.Value}\"{comma}");
                }

                sw.WriteLine("}");
                return true;
            }

            if (!isTsx) return false;

            sw.WriteLine("export const RouteConstants = {");
            var lastKvp = fields.Last();
            foreach (var pair in fields)
            {
                var comma = pair.Key == lastKvp.Key ? "" : ",";
                sw.WriteLine($"  {char.ToLower(pair.Key[0])}{pair.Key.Substring(1)}: \"{pair.Value}\"{comma}");
            }

            sw.WriteLine("} as const;");
            return true;
        }
        catch (Exception ex)
        {
            Log.LogError("{0} crashed: {1}", nameof(GenerateConstantsJsonTask), ex);
            return false;
        }
    }
}
