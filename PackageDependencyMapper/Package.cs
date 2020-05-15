using System;
using System.Collections.Generic;
using System.IO;

namespace PackageDependencyMapper
{
    public class Dependency
    {
        public Package Parent;
        public Package Child;
        public PackageVersion ParentVersion;
        public PackageVersion ChildVersion;
        public override int GetHashCode()
        {
            return HashCode.Combine(Parent.GetHashCode(), Child.GetHashCode(), ParentVersion.GetHashCode(),
                ChildVersion.GetHashCode());
        }

    }

    public class VersionInfo
    {
        public bool Known = false; //true when the package has been loaded from it's json file. false when it's only a reference from another package
        public PackageVersion Version;
        public UnityVersion UnityVersion;

    }
    public class Package
    {
        public bool Known = false; //true when the package has been loaded from it's json file. false when it's only a reference from another package
        public string Name;
        public string DisplayName;
        public FileInfo File;
        //public Dictionary<string, VersionInfo> Version = new Dictionary<string, VersionInfo>();
        public SortedDictionary<PackageVersion, VersionInfo> Version = new SortedDictionary<PackageVersion, VersionInfo>();

        public HashSet<Dependency> Children = new HashSet<Dependency>();
        public HashSet<Dependency> Parent = new HashSet<Dependency>();
        public IEnumerable<Dependency> ChildrenOfVersion(PackageVersion version)
        {
            foreach (var d in Children)
            {
                if (d.ParentVersion == version)
                {
                    yield return d;
                }
            }
        }

    }
    public class PackageDependencyGraph
    {
        public Dictionary<string, Package> AllPackage = new Dictionary<string, Package>();
        public IEnumerable<Package> AllRootPackage
        {
            get
            {
                foreach (var p in AllPackage)
                {
                    if (p.Value.Parent.Count == 0)
                    {
                        yield return p.Value;
                    }
                }
            }
        }
    }
}