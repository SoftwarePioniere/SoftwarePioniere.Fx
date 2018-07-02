using System;
using Newtonsoft.Json;

namespace SoftwarePioniere.Messaging
{
    public abstract class MessageTestBase<T> where T : class, IMessage
    {
        // ReSharper disable once MemberCanBePrivate.Global
        protected const string FromConstructor = "created from constructor";
        // ReSharper disable once MemberCanBePrivate.Global
        protected const string FromSerialization = "created from serilization";

        protected Guid Id = Guid.NewGuid();
        protected DateTime TimeStamp = DateTime.UtcNow;
        protected const string UserId = "fake user id";

        protected abstract T CreateFromConstructor();

        public virtual void RunTest()
        {
            var obj1 = CreateFromConstructor();
            TestCreatedFromConstructor(obj1);
            TestCreatedFromSerilization(obj1);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        protected void TestCreatedFromConstructor(T o)
        {
            TestIt(o, FromConstructor);
        }


        // ReSharper disable once MemberCanBePrivate.Global
        protected void TestCreatedFromSerilization(T obj1)
        {
            var str = JsonConvert.SerializeObject(obj1);
            var objx = JsonConvert.DeserializeObject<T>(str);

            TestIt(objx, FromSerialization);
        }

        protected abstract void TestIt(T o, string s);
    }
}