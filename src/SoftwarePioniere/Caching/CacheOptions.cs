namespace SoftwarePioniere.Caching
{
    public class CacheOptions
    {
        public int CacheMinutes { get; set; } = 120;
        public int CacheLoadSplitSize { get; set; } = 500;
    }
}