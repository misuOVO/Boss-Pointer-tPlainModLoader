using System;
using Microsoft.Xna.Framework.Input;

namespace BossCursor
{
    /// <summary>按键名（Keys 枚举名，如 F8 / B / OemOpenBrackets）解析与缓存。</summary>
    internal static class KeyUtil
    {
        private static string _cachedName;
        private static Keys _cachedKey = Keys.None;

        public static Keys Get(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Keys.None;

            if (name == _cachedName)
                return _cachedKey;

            _cachedName = name;
            _cachedKey = TryParse(name, out Keys key) ? key : Keys.None;
            return _cachedKey;
        }

        public static bool TryParse(string name, out Keys key)
        {
            key = Keys.None;
            if (string.IsNullOrWhiteSpace(name))
                return false;

            try
            {
                return Enum.TryParse(name.Trim(), true, out key) && key != Keys.None;
            }
            catch
            {
                return false;
            }
        }
    }
}
