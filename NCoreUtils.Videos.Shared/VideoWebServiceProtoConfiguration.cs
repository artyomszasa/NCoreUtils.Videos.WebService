using System.Text.Json;
using NCoreUtils.AspNetCore.Proto;

namespace NCoreUtils.Videos.Internal
{
    public static class VideoWebServiceProtoConfiguration
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                ImmutableJsonConverterFactory.GetOrCreate<VideoOptions>(),
                ImmutableJsonConverterFactory.GetOrCreate<Images.ResizeOptions>()
            }
        };

        public static ServiceDescriptorBuilder ApplyVideoWebServiceDefaults(this ServiceDescriptorBuilder builder, string? path)
            => builder
                .SetPath(path)
                .SetNamingPolicy(NamingPolicy.SnakeCase)
                .SetDefaultInputType(InputType.Json(_jsonOptions))
                .SetDefaultOutputType(OutputType.Json(_jsonOptions))
                .SetDefaultErrorType(ErrorType.Json(_jsonOptions));

        public static ServiceDescriptorBuilder ApplyVideoWebServiceDefaults(this ServiceDescriptorBuilder builder)
            => builder.ApplyVideoWebServiceDefaults(string.Empty);
    }
}