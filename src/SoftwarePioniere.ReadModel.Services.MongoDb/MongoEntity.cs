namespace SoftwarePioniere.ReadModel
{
    public class MongoEntity<T> where T : Entity
    {
        // ReSharper disable once InconsistentNaming
        public string _id { get; set; }

        public T Entity { get; set; }
    }
}