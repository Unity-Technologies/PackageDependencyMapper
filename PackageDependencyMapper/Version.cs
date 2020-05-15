using System;

namespace PackageDependencyMapper
{
    public class VersionMMR : IComparable<VersionMMR>
    {
        public int Major = 0;
        public int Minor = 0;
        public int Revision = 0;

        public int CompareTo(VersionMMR other)
        {
            var majorR = Major.CompareTo(other.Major);
            if (majorR != 0) return majorR;

            var minorR = Minor.CompareTo(other.Minor);
            if (minorR != 0) return minorR;

            var revisionR = Revision.CompareTo(other.Revision);
            if (revisionR != 0) return revisionR;

            return 0;
        }
        public static bool TryParse(string name, out VersionMMR result)
        {
            result = new VersionMMR();

            var verSeg = name.Split(".");
            if (verSeg.Length > 0)
            {
                if (!int.TryParse(verSeg[0], out result.Major)) return false;
            }
            if (verSeg.Length > 1)
            {
                if (!int.TryParse(verSeg[1], out result.Minor)) return false;
            }
            if (verSeg.Length > 2)
            {
                if (!int.TryParse(verSeg[2], out result.Revision)) return false;
            }
            return true;

        }
        public override string ToString()
        {
            return $"{Major}.{Minor}.{Revision}";
        }
    }
    public class PackageVersion : IComparable<PackageVersion>
    {
        public VersionMMR MMR = new VersionMMR();
        public int Preview = -1;

        public bool IsPreview { get { return Preview >= 0; } }
        public string Name { get { return ToString(); } }

        public override string ToString()
        {
            if (IsPreview)
            {
                return $"{MMR}-preview.{Preview}";
            }
            return MMR.ToString();
        }
        public int CompareTo(PackageVersion other)
        {
            var mmrR = MMR.CompareTo(other.MMR);
            if (mmrR != 0) return mmrR;

            var previewR = Preview.CompareTo(other.Preview);
            if (previewR != 0) return previewR;

            return 0;
        }
        public static bool TryParse(string name, out PackageVersion result)
        {
            result = new PackageVersion();
            var segPrev = name.Split("-");
            switch (segPrev.Length)
            {

                case 2:
                    if (!segPrev[1].StartsWith("preview")) return false;
                    // is a preview package
                    var prevSeg = segPrev[1].Split(".");
                    if (prevSeg.Length == 2)
                    {
                        if (!int.TryParse(prevSeg[1], out result.Preview)) return false;
                    }
                    else
                    {
                        result.Preview = 0;
                    }
                    goto case 1; // fall through
                case 1:
                    return VersionMMR.TryParse(segPrev[0], out result.MMR);
                default:
                    return false;
            }
        }
    }
    public class UnityVersion : IComparable<UnityVersion>
    {

        public VersionMMR MMR = new VersionMMR();

        public override string ToString()
        {
            return MMR.ToString();
        }
        public int CompareTo(UnityVersion other)
        {
            var mmrR = MMR.CompareTo(other.MMR);
            if (mmrR != 0) return mmrR;

            return 0;
        }
        public static bool TryParse(string name, out UnityVersion result)
        {
            result = new UnityVersion();
            return VersionMMR.TryParse(name, out result.MMR);
        }
    }
}