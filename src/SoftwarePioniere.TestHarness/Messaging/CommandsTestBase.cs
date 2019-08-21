namespace SoftwarePioniere.Messaging
{
    public abstract class CommandsTestBase<T> : MessageTestBase<T> where T : class, ICommand, IMessage
    {
        protected const string FromRequest = "created from request";

        protected const int OriginalVersion = 123;
        protected const string RequestId = "fake request id";

        public override void RunTest()
        {
            var obj1 = CreateFromConstructor();
            TestCreatedFromConstructor(obj1);
            TestCreatedFromSerilization(obj1);
        }

        protected void TestCreatedFromRequest(T o)
        {
            TestIt(o, FromRequest);
        }
    }
}