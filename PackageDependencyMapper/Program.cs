using System;
using System.IO;
using System.Linq;
using Json = System.Text.Json;

namespace PackageDependencyMapper
{
    class Program
    {
        static bool TryLoadOptionFromFile(ref Options opt, string configFile)
        {
            try
            {
                FileStream fs = File.OpenRead(configFile);
                var doc = Json.JsonDocument.Parse(fs);
                var inputFolder = doc.RootElement.GetProperty("InputFolder");
                var folderCount = inputFolder.GetArrayLength();
                foreach (var f in inputFolder.EnumerateArray())
                {
                    var dir = f.GetString();
                    opt.SearchDirectories.Add(dir);
                    if (opt.Verbose) Console.WriteLine($"add search folder {dir}");
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while parsing config file");
                Console.WriteLine(e.Message);
                return false;
            }
        }
        static void PrintHelp()
        {
            Console.WriteLine("Unity Package Dependency Mapper.");
            Console.WriteLine("This tool will search for Unity packages and map their dependencies into a graph that can be outputted in various ways.");
            Console.WriteLine("Options:");
            Console.WriteLine("\t-config <filename.json>");
            Console.WriteLine("\t\tload configuration from json files.");
            Console.WriteLine("\t\tex:");
            Console.WriteLine("\t\t\t{");
            Console.WriteLine("\t\t\t  \"InputFolder\": [");
            Console.WriteLine("\t\t\t    \"C:/folder/\"");
            Console.WriteLine("\t\t\t  ]");
            Console.WriteLine("\t\t\t}");
            Console.WriteLine("\t\tIf none specified, will search current directory.");
            Console.WriteLine("\t-verbose");
            Console.WriteLine("\t\t print extra information about the mapping process.");
            Console.WriteLine("\t-help");
            Console.WriteLine("\t\t print this help.");
            Console.WriteLine("\t-treedown <name of package to output|all>");
            Console.WriteLine("\t\t output a tree of all sub packages and version that depends on the given package name.");

        }
        static bool TryLoadOptionFromArgs(ref Options opt, string[] args)
        {
            bool hasConfig = false;
            for (int i = 0; i != args.Length; ++i)
            {
                if (args[i] == "-help")
                {
                    PrintHelp();
                    return true;
                }
                if (args[i] == "-verbose")
                {
                    opt.Verbose = true;
                }
                else if (args[i] == "-treedown")
                {

                    ++i;
                    if (i >= args.Length)
                    {
                        Console.WriteLine("Invalid argument for option -treedown");
                        Console.WriteLine("Usage: -treedown <name of package to output|all>");
                        return false;
                    }
                    opt.OutputPackageTreeDown = args[i];
                }
                else if (args[i] == "-config")
                {
                    ++i;
                    if (i >= args.Length)
                    {
                        Console.WriteLine("Invalid argument for option -config");
                        Console.WriteLine("Usage: -config <MyConfig.json>");
                        return false;
                    }

                    if (!TryLoadOptionFromFile(ref opt, args[i]))
                    {
                        return false;
                    }
                    hasConfig = true;
                }
            }


            if (!hasConfig)
            {
                var curDir = Directory.GetCurrentDirectory();
                opt.SearchDirectories.Add(curDir);
                if (opt.Verbose) Console.WriteLine($"add search folder {curDir}");
            }

            return true;
        }
        static void PrintTreeDown(TreePrintFrame tf, Package pack)
        {
            
            Console.WriteLine(tf.Indent(pack.Name + (pack.Known ? "" : " (Unknown)")));
            foreach (var tfc in tf.VisitChildren(pack.Children.AsEnumerable()))
            {
                PrintTreeDown(tfc.Item1, tfc.Item2.Child);
            }
        }
        
        static void PrintVersionTreeDown(TreePrintFrame tf, Package pack, VersionInfo version)
        {
            Console.WriteLine(
                tf.Indent(
                    version.Version.Name +
                    (version.Known ?
                          $" (Unity {version.UnityVersion})"
                        : "  (json not found)"
                    )
                )
            );
                
            foreach (var tfc in tf.VisitChildren(pack.ChildrenOfVersion(version.Version)))
            {
                PrintVersionTreeDown(tfc.Item1, tfc.Item2.Child);
            }
            
        }
        static void PrintVersionTreeDown(TreePrintFrame tf, Package pack)
        {
            Console.WriteLine(tf.Indent(pack.Name + (pack.Known ? "" : " (Unknown)")));
            //print from newest version to oldest
            foreach (var tfc in tf.VisitChildren(pack.Version.Reverse(), "|*"))
            {
                PrintVersionTreeDown(tfc.Item1, pack, tfc.Item2.Value);
            }
        }


        static int Main(string[] args)
        {
            if(args.Length == 0)
            {
                PrintHelp();
                return 0;
            }
            Options opt = new Options();
            if (!TryLoadOptionFromArgs(ref opt, args))
            {
                return 1;
            }
            PackageDependencyGraph graph = new PackageDependencyGraph();
            Searcher mapper = new Searcher(graph, opt);
            mapper.Search();

            if (!String.IsNullOrEmpty(opt.OutputPackageTreeDown))
            {
                if(opt.OutputPackageTreeDown == "all")
                {
                    foreach (var p in graph.AllRootPackage)
                    {
                        PrintVersionTreeDown(new TreePrintFrame(), p);
                    }
                }
                if(graph.AllPackage.TryGetValue(opt.OutputPackageTreeDown, out var pack))
                {
                    PrintVersionTreeDown(new TreePrintFrame(), pack);
                }

            }
            return 0;
        }
    }
}
