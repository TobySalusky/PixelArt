using System.Collections.Generic;
using System.Linq;

namespace PixelArt {
    public static class StringUtil {
        public static DelimPair searchPairs(this string str, string open, string close, int searchIndex) {
            return DelimPair.genPairDict(str, open, close)[searchIndex];
        }
        
        public static Dictionary<(string, string), int> nestAmounts(this string str, (int, int) rangeInclusive, params (string, string)[] delimTypes) {
            var dict = DelimPair.searchAll(str, delimTypes);

            (int start, int end) = rangeInclusive;
            
            var nestDict = new Dictionary<(string, string), int>();
            foreach (var key in dict.Keys) {
                nestDict[key] = 0;
                foreach (var pair in dict[key]) {
                    if (pair.openIndex < start && pair.closeIndex > end) nestDict[key]++;
                }
            }
            return nestDict;
        }
    }
}