using System;
using System.Collections.Generic;

namespace SoftwarePioniere.Messaging
{
    /// <summary>
    /// Basis für ein Command
    /// </summary>
    public abstract class CommandBase : MessageBase, ICommand
    {
        public const string RequestIdKey = "RequestId";

        public const string TraceIdentifierKey = "TraceIdentifier";

        protected CommandBase(Guid id, DateTime timeStampUtc, string userId, int originalVersion, string objectType, string objectId, IDictionary<string, string> properties = null) : base(id, timeStampUtc, userId)
        {
            OriginalVersion = originalVersion;
            ObjectType = objectType;
            ObjectId = objectId;
            Properties = properties;
        }

        /// <summary>
        /// Die EventVersion des Original Objekts
        /// </summary>
        public int OriginalVersion { get; }

        public string ObjectType { get; }

        public string ObjectId { get; }

        public IDictionary<string, string> Properties { get; }
    }

    public static class CommandBaseExtensions
    {
        public static void AddProperty(this ICommand cmd, string key, string value)
        {
            if (cmd.Properties.ContainsKey(key))
                cmd.Properties.Remove(key);

            cmd.Properties.Add(key, value);
        }

        public static void SetTraceIdentifier(this ICommand cmd, string value)
        {
            cmd.AddProperty(CommandBase.TraceIdentifierKey, value);
        }

        public static void SetRequestId(this ICommand cmd, string value)
        {
            cmd.AddProperty(CommandBase.RequestIdKey, value);
        }

        public static string GetTraceIdentifier(this ICommand cmd)
        {
            if (cmd == null)
                return null;

            if (cmd.Properties.ContainsKey(CommandBase.TraceIdentifierKey))
            {
                return cmd.Properties[CommandBase.TraceIdentifierKey];
            }

            return null;
        }

        public static string GetRequestId(this ICommand cmd)
        {
            if (cmd == null)
                return null;

            if (cmd.Properties.ContainsKey(CommandBase.RequestIdKey))
            {
                return cmd.Properties[CommandBase.RequestIdKey];
            }

            return null;
        }
    }
}
