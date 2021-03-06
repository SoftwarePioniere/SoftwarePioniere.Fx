// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace SoftwarePioniere.Caching
{
    public class CacheOptions
    {
        public bool CachingDisabled { get; set; }
        public int CacheMinutes { get; set; } = 120;
        public int CacheLoadSplitSize { get; set; } = 2000;
        public string CacheScope { get; set; }
        public int CacheLockTimeoutSeconds { get; set; } = 10;
        public bool DisableLocking { get; set; }
        public bool DisableLocking2 { get; set; }
        public bool DisableLocking3 { get; set; }
        public bool ThrowExceptions { get; set; }
    }
}