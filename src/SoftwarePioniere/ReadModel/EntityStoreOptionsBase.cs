namespace SoftwarePioniere.ReadModel
{
    /// <summary>
    /// Entity Store Base Options
    /// </summary>
    public abstract class EntityStoreOptionsBase
    {
        ///// <summary>
        ///// Defines an AppId for Filtering  and Appending to the Entity
        ///// TODO: Implementation in Service (the EntityId must Change also)
        ///// </summary>
        //public string AppId { get; set; }

        ///// <summary>
        ///// Use the AppId for Filterung
        ///// </summary>
        //public bool UseAppId { get; set; }

        //public ILoggerFactory LoggerFactory { get; set; }
        //public ICacheClient CacheClient { get; set; }
        public int CacheMinutes { get; set; } = 120;
        public bool CachingDisabled { get; set; }

        public bool ThrowDeveloperError { get; set; }
    }
}
