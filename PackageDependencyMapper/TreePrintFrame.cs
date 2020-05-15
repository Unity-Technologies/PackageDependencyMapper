
using System.Collections.Generic;
using System.Linq;

namespace PackageDependencyMapper
{

    public class TreePrintFrame
    {
        public string IndentParent = "";
        public string IndentChild = "";
        public string Indent(string strIn)
        {
            return IndentParent + strIn;
        }
        public IEnumerable<(TreePrintFrame, T)> VisitChildren<T>(IEnumerable<T> collection, string parentNewIndent = "|-", string childNewIndent = "| ", string childNewIndentEmpty = "  ")
        {
            TreePrintFrame childFrame = new TreePrintFrame()
            {
                IndentParent = this.IndentChild + parentNewIndent,
                IndentChild = this.IndentChild + childNewIndent
            };

            var last = collection.Count() - 1;
            var cur = 0;
            foreach (var c in collection)
            {
                if (cur == last)
                {
                    childFrame.IndentChild = IndentChild + childNewIndentEmpty;
                }
                yield return (childFrame, c);
                ++cur;
            }
        }
    }
}