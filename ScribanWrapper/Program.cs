using System;
using System.IO;
using System.Reflection;

internal class Program
{
    private static void Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += static (_, args) =>
        {
            AssemblyName assemblyName = new(args.Name);
            string path = assemblyName.Name + ".dll";
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            if (stream is null)
                return null;

            stream.Position = 0;
            var assemblyRawBytes = new byte[stream.Length];
            stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
            return Assembly.Load(assemblyRawBytes);
        };

        if (args.Length < 1)
        {
            Console.WriteLine("""
                Usage: SourceGen.exe [ScriptFile] > out.txt"
                For details on the text file format, please refer to: https://github.com/scriban/scriban
                """);
            return;
        }

        string scriptFile = args[0];
        if (!File.Exists(scriptFile))
        {
            Console.WriteLine($"\"{scriptFile}\" not found.");
            return;
        }

        string result = GetRenderedText(scriptFile);
        Console.WriteLine(result);
    }

    // If you don't extract method that uses external library, a DLL error will occur.
    private static string GetRenderedText(string scriptFile)
    {
        string sourceScript = File.ReadAllText(scriptFile);
        return Scriban.Template.Parse(sourceScript).Render();
    }
}
