using System;
using System.Runtime.Serialization;

namespace NCoreUtils.Videos
{
    [Serializable]
    public class UnsupportedVideoTypeException : VideoException
    {
        public string VideoType { get; set; } = string.Empty;

        protected UnsupportedVideoTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
            => VideoType = info.GetString(nameof(VideoType)) ?? string.Empty;

        public UnsupportedVideoTypeException(string videoType, string descripton)
            : base(ErrorCodes.UnsupportedVideoType, descripton)
            => VideoType = videoType ?? string.Empty;

        public UnsupportedVideoTypeException(string videoType, string description, Exception innerException)
            : base(ErrorCodes.UnsupportedVideoType, description, innerException)
            => VideoType = videoType ?? string.Empty;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(VideoType), VideoType);
        }
    }
}