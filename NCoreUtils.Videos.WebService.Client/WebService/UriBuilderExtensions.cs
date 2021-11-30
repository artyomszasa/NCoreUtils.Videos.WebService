using System;

namespace NCoreUtils.Videos.WebService
{
    static class UriBuilderExtensions
    {
        public static UriBuilder AppendPathSegment(this UriBuilder builder, string segment)
        {
            if (string.IsNullOrEmpty(builder.Path))
            {
                builder.Path = segment;
            }
            else
            {
                if (builder.Path.EndsWith("/"))
                {
                    builder.Path += segment;
                }
                else
                {
                    builder.Path += "/" + segment;
                }
            }
            return builder;
        }
    }
}