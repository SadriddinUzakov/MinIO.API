using System.Collections.Generic;

namespace minio.Domain.Core.Utilities
{
    public enum SizeUnit
    {
        WIDTH,
        HEIGHT,
        PERCENT
    }

    public static class SizeUnitExtensions
    {
        private static readonly Dictionary<string, SizeUnit> Lookup = new()
        {
            { "w", SizeUnit.WIDTH },
            { "h", SizeUnit.HEIGHT },
            { "p", SizeUnit.PERCENT }
        };

        public static string ToKey(this SizeUnit sizeUnit)
        {
            return sizeUnit switch
            {
                SizeUnit.WIDTH => "w",
                SizeUnit.HEIGHT => "h",
                SizeUnit.PERCENT => "p",
                _ => null
            };
        }

        public static bool EqualsKey(this SizeUnit sizeUnit, string key)
        {
            return sizeUnit.ToKey() == key;
        }

        public static SizeUnit? Get(string key)
        {
            return Lookup.TryGetValue(key, out var sizeUnit) ? sizeUnit : null;
        }
    }
}


