using System;
using System.Linq;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace ConApp
{
    static class RemoveThrow
    {
        internal static void GO()
        {
            var compilation = GetCompilation();
            int total = 0, hasChanged = 0;
            var opt = new CSharpParseOptions();
            foreach (var tree in compilation.SyntaxTrees)
            {
                ++total;
                var problems = tree.GetDiagnostics();
                if (problems.Any())
                {
                    foreach (var item in problems) Console.WriteLine(item);
                    continue;
                }
                var model = compilation.GetSemanticModel(tree);
                var rewriter = new ThrowRewriter(model);
                var oldNode = tree.GetRoot();
                var newNode = rewriter.Visit(oldNode);
                if (newNode == oldNode) continue;
                ++hasChanged;
                var path = tree.FilePath.Replace("RemoveThrow/in/", "RemoveThrow/out/");
                File.WriteAllText(path, newNode.ToFullString());
                var newTree = tree.WithRootAndOptions(newNode, opt);
                compilation.ReplaceSyntaxTree(tree, newTree);
            }
            Console.WriteLine($"Rewrite: {hasChanged}/ Total: {total}.");
            {
                using var ms = new MemoryStream();
                var emitResult = compilation.Emit(ms);
                if (!emitResult.Success)
                    foreach (var problem in emitResult.Diagnostics)
                        Console.WriteLine(problem);
            }
        }
        private static CSharpCompilation GetCompilation()
        {
            var file = typeof(int).GetTypeInfo().Assembly.Location;
            var dir = Path.GetDirectoryName(file);
            var refs = new[]
            {
                MetadataReference.CreateFromFile(file),
                MetadataReference.CreateFromFile(typeof(System.Console).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(dir, "System.Runtime.dll")),
            };
            var compilation = CSharpCompilation.Create("Qwe",
                syntaxTrees: Directory.EnumerateFiles("src/RemoveThrow/in").Select(q =>
                {
                    var w = File.ReadAllText(q);
                    return CSharpSyntaxTree.ParseText(w).WithFilePath(q);
                }),
                references: refs,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            return compilation;
        }
    }
}