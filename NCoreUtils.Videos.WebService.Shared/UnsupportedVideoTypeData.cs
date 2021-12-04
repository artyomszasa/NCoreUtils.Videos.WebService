using System;
using System.Text.Json;

namespace NCoreUtils.Videos.WebService
{
    [Serializable]
    public class UnsupportedVideoTypeData : VideoErrorData
    {
        static readonly JsonEncodedText _keyVideoType = JsonEncodedText.Encode(VideoErrorProperties.VideoType);

        public string VideoType { get; }

        public UnsupportedVideoTypeData(string errorCode, string description, string imageType)
            : base(errorCode, description)
            => VideoType = imageType;

        internal override void WriteTo(Utf8JsonWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteString(_keyVideoType, VideoType);
        }
    }
}