using System;
using NCoreUtils.Videos.WebService;

namespace NCoreUtils.Videos.Generic;

public static class ExceptionHelper
{
    public static VideoErrorData GetErrorData(VideoException exn) => exn switch
    {
        UnsupportedResizeModeException e => new UnsupportedResizeModeData(e.ErrorCode, e.Message, e.ResizeMode, e.Width, e.Height),
        UnsupportedVideoTypeException e => new UnsupportedVideoTypeData(e.ErrorCode, e.Message, e.VideoType),
        InternalVideoException e => new InternalVideoErrorData(e.ErrorCode, e.Message, e.InternalCode),
        VideoException e => new VideoErrorData(e.ErrorCode, e.Message)
    };

    public static VideoErrorData GetErrorData(Exception exn) => exn switch
    {
        VideoException vexn => GetErrorData(vexn),
        _ => new VideoErrorData(ErrorCodes.GenericError, exn.Message)
    };
}