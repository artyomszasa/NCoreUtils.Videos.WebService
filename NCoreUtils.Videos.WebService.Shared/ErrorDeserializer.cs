using System.Text;

namespace NCoreUtils.Videos.WebService
{
    internal class ErrorDeserializer
    {
        protected static readonly byte[] _keyDescription = Encoding.ASCII.GetBytes(VideoErrorProperties.Description);

        protected string ErrorCode { get; }

        protected string Description { get; set; } = string.Empty;

        public ErrorDeserializer(string errorCode)
            => ErrorCode = errorCode;
    }
}