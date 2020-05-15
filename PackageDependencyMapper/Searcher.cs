using System;
using Json = System.Text.Json;
using System.Collections.Generic;
using System.IO;

namespace PackageDependencyMapper
{
    public class Options
    {
        public List<string> SearchDirectories = new List<string>();

        public string OutputFilename;
        public bool Verbose = false;

        public string OutputPackageTreeDown = null;

    }
    public class Searcher
    {
        public static bool TryGetProperty(Json.JsonElement e, string name, out string value)
        {
            if (e.TryGetProperty(name, out var v))
            {
                value = v.GetString();
                return true;
            }
            value = default;
            return false;
        }
        public PackageDependencyGraph Graph;
        public Options SearchOptions;
        public Searcher(PackageDependencyGraph graph, Options options)
        {
            Graph = graph;
            SearchOptions = options;
        }
        public void Search()
        {
            foreach (var dir in SearchOptions.SearchDirectories)
            {
                DirectoryInfo di = new DirectoryInfo(dir);
                foreach (var fi in di.EnumerateFiles("package.json", SearchOption.AllDirectories))
                {
                    LoadPackage(fi);
                }
            }
            
        }

        public void ProcessDependency(Package child, VersionInfo childVersionInfo, string parentName, PackageVersion parentVersion)
        {

            if (!Graph.AllPackage.TryGetValue(parentName, out var parent))
            {
                parent = new Package { Name = parentName };
                Graph.AllPackage.Add(parentName, parent);
                if (SearchOptions.Verbose) Console.WriteLine($"Add unknown package {parent.Name}");
            }

            if (!parent.Version.TryGetValue(parentVersion, out var parentVersionInfo))
            {
                parentVersionInfo = new VersionInfo() { Version = parentVersion };
                parent.Version.Add(parentVersionInfo.Version, parentVersionInfo);
            }

            Dependency dep = new Dependency()
            {
                Parent = parent,
                Child = child,
                ParentVersion = parentVersionInfo.Version,
                ChildVersion = childVersionInfo.Version
            };
            child.Parent.Add(dep);
            parent.Children.Add(dep);
            if (SearchOptions.Verbose) Console.WriteLine($"add dependency {child.Name} -> {parent.Name}");

        }
        public void LoadPackage(FileInfo file)
        {

            FileStream fs = file.OpenRead();
            var doc = Json.JsonDocument.Parse(fs);


            if (!TryGetProperty(doc.RootElement, "name", out var name))
            {
                return;
            }
            if (String.IsNullOrEmpty(name))
            {
                return;
            }

            Package package;
            if (!Graph.AllPackage.TryGetValue(name, out package))
            {
                package = new Package { Name = name };
                Graph.AllPackage.Add(name, package);
            }
            if (SearchOptions.Verbose) Console.WriteLine($"Found package {package.Name} in '{file.FullName}'");
            if (!package.Known)
            {
                TryGetProperty(doc.RootElement, "displayName", out package.DisplayName);
                package.Known = true;
            }

            if (!TryGetProperty(doc.RootElement, "version", out var versionStr))
            {
                return;
            }
            if (!PackageVersion.TryParse(versionStr, out var version))
            {
                return;
            }

            if (!package.Version.TryGetValue(version, out var versionInfo))
            {
                versionInfo = new VersionInfo
                {
                    Version = version
                };
                package.Version.Add(versionInfo.Version, versionInfo);
            }

            if (!versionInfo.Known)
            {
                if (TryGetProperty(doc.RootElement, "unity", out var unityVerStr))
                {
                    if (!UnityVersion.TryParse(unityVerStr, out versionInfo.UnityVersion))
                    {
                        Console.WriteLine($"Could not parse Unity version {unityVerStr} from package '{file.FullName}'");
                        versionInfo.UnityVersion = new UnityVersion();
                    }
                }
                versionInfo.Known = true;
            }


            if (doc.RootElement.TryGetProperty("dependencies", out var dependencies))
            {
                foreach (var d in dependencies.EnumerateObject())
                {
                    if (!PackageVersion.TryParse(d.Value.GetString(), out var versionDep))
                    {
                        return;
                    }
                    ProcessDependency(package, versionInfo, d.Name, versionDep);
                }
            }
        }

    }
}