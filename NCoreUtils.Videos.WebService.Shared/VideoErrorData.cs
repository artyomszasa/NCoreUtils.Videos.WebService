using System;
using System.Text.Json;

namespace NCoreUtils.Videos.WebService
{
    [Serializable]
    public class VideoErrorData
    {
        static readonly JsonEncodedText _keyErrorCode = JsonEncodedText.Encode(VideoErrorProperties.ErrorCode);

        static readonly JsonEncodedText _keyDescription = JsonEncodedText.Encode(VideoErrorProperties.Description);

        public string ErrorCode { get; }

        public string Description { get; }

        public VideoErrorData(string errorCode, string description)
        {
            ErrorCode = errorCode;
            Description = description;
        }

        internal virtual void WriteTo(Utf8JsonWriter writer)
        {
            writer.WriteString(_keyErrorCode, ErrorCode);
            writer.WriteString(_keyDescription, Description);
        }
    }
}