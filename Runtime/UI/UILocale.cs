using Air.UnityGameCore.Runtime;

namespace Air.UI
{
    public static class UILocale
    {
        public static string Lookup(string key)
        {
            var value = Locale.Lookup(key);
            return string.IsNullOrEmpty(value) ? null : value;
        }
    }
}
