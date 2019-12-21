// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace SoftwarePioniere.Caching
{
    public class CacheOptions
    {
        public int CacheMinutes { get; set; } = 120;
        public int CacheLoadSplitSize { get; set; } = 2000;
        public string CacheScope { get; set; }
        public int CacheLockTimeoutSeconds { get; set; } = 10;
        public bool DisableLocking { get; set; }
    }
}