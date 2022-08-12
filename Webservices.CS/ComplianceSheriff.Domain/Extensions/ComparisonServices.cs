using System.Collections.Generic;
using System.Linq;

namespace ComplianceSheriff.Extensions
{
    public static class ComparisonServices
    {
        // Compares both lists and determines if any records have been added or 
        // removed from either list and returns the number of differences
        public static int CompareLists<T>(List<T> listOne, List<T> listTwo)
        {
            return listOne.Except(listTwo).Union(listTwo.Except(listOne)).ToList().Count;
        }
    }
}
