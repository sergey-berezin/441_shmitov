using System;
using System.Collections.Generic;
using System.Linq;

namespace MainLib
{
    public class SummaryClass
    {
        public string Name;
        public IReadOnlyList<string> CatNames;

        public override string ToString()
        {
            string result = Name + " : ";
            foreach (var n in CatNames)
            {
                result += n + " ";
            }
            result += "\n";
            return result;
        }
    }
}
