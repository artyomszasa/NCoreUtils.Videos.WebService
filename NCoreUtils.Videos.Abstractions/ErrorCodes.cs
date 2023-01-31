namespace NCoreUtils.Videos;

/// Contains predefined error codes.
public static class ErrorCodes
{
    /// Resize method not supported.
    public const string UnsupportedResizeMode = "unsupported_resize_mode";

    /// Requested output video type not supported.
    public const string UnsupportedVideoType = "unsupported_video_type";

    /// Input video is invalid or not supported.
    public const string InvalidVideo = "invalid_video";

    /// Implementation specific error occured while preforming resize or get-info operation.
    public const string InternalError = "internal_error";

    /// Generic error.
    public const string GenericError = "generic_error";
}