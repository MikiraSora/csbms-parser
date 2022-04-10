using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSBMSParser
{
    internal static class MethodExtensions
    {
        public static IEnumerable<KeyValuePair<T, D>> descendingMap<T, D>(this IDictionary<T, D> self)
        {
            return self.Keys.OrderByDescending(x => x).Select(x => new KeyValuePair<T, D>(x, self[x]));
        }

        public static KeyValuePair<T, D> lowerEntry<T, D>(this IDictionary<T, D> self, T key)
        {
            var cmp = Comparer<T>.Default;
            var x = self.Keys.OrderByDescending(x => x).FirstOrDefault(x => cmp.Compare(x, key) < 0);
            return self.TryGetValue(x, out var d) ? new KeyValuePair<T, D>(x, d) : default;
        }

        public static IDictionary<T, D> subMap<T, D>(this IDictionary<T, D> self, T fromKey, bool fromInclusive, T toKey, bool toInclusive)
        {
            var cmp = Comparer<T>.Default;
            var map = new Dictionary<T, D>();
            foreach (var x in self.Keys.Where(x =>
            {
                var cmpValue = cmp.Compare(fromKey, x);
                if (cmpValue < 0 || (cmpValue == 0 && !fromInclusive))
                    return false;
                cmpValue = cmp.Compare(x, toKey);
                if (cmpValue < 0 || (cmpValue == 0 && !toInclusive))
                    return false;
                return true;
            }))
                map[x] = self[x];
            return map;
        }

        public static string substring(this string s, int start) => s.substring(start, s.Length);
        public static string substring(this string s, int start, int end) => s.Substring(start, end - start);
    }
}
