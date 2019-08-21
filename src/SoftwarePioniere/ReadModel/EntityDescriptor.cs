namespace SoftwarePioniere.ReadModel
{
    /// <summary>
    /// Describing structure for Entities
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public struct EntityDescriptor<TEntity> where TEntity : Entity
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsNew { get; set; }
        public string Id { get; set; }
        public TEntity Entity { get; set; }
        public string EntityId { get; set; }
    }
}