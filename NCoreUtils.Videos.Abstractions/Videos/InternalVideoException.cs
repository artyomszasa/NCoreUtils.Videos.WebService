using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Videos.WebService
{
    [Serializable]
    public class InternalVideoException : VideoException
    {
        public string InternalCode { get; }

        public override string Message => $"{base.Message} [InternalCode = {InternalCode}]";

        protected InternalVideoException(SerializationInfo info, StreamingContext context)
            : base(info, context)
            => InternalCode = info.GetString(nameof(InternalCode)) ?? string.Empty;

        public InternalVideoException(string internalCode, string description)
            : base(internalCode, description)
            => InternalCode = internalCode ?? throw new ArgumentNullException(nameof(internalCode));

        public InternalVideoException(string internalCode, string description, Exception innerException)
            : base(internalCode, description, innerException)
            => InternalCode = internalCode ?? throw new ArgumentNullException(nameof(internalCode));

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(InternalCode), InternalCode);
        }
    }
}