using System.Globalization;

namespace NCoreUtils.Videos;

internal sealed class JobArguments(Uri source, Uri destination, ResizeOptions options)
{
    public Uri Source { get; } = source;

    public Uri Destination { get; } = destination;

    public ResizeOptions Options { get; } = options;
}

internal static class OptionsParser
{
    private struct JobArgumentsBuilder
    {
        public Uri? Source { get; set; }

        public Uri? Destination { get; set; }

        /// <summary>
        /// Desired output audio type. Defaults to the input audio type when not set.
        /// </summary>
        public string? AudioType { get; set; }

        /// <summary>
        /// Desired output video type. Defaults to the input video type when not set.
        /// </summary>
        public VideoSettings? VideoType { get; set; }

        /// <summary>
        /// Desired output video width. Defaults to the input video width when not set.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// Desired output video height. Defaults to the input video height when not set.
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// Defines resizing mode. Defaults to <c>none</c>.
        /// </summary>
        public string? ResizeMode { get; set; }

        /// <summary>
        /// Desired quality of the output video. Server dependent default value is used when not set.
        /// </summary>
        public int? Quality { get; set; }

        /// <summary>
        /// Whether to perform any optimization on the output video. Server dependent default value is used when not
        /// set.
        /// </summary>
        public bool? Optimize { get; set; }

        /// <summary>
        /// Optional X coordinate of the weight point of the video.
        /// </summary>
        public int? WeightX { get; set; }

        /// <summary>
        /// Optional Y coordinate of the weight point of the video.
        /// </summary>
        public int? WeightY { get; set; }

        public JobArguments Build()
        {
            if (Source is null)
            {
                throw new InvalidOperationException("No source provided.");
            }
            if (Destination is null)
            {
                throw new InvalidOperationException("No destination provided.");
            }
            return new(
                Source,
                Destination,
                new ResizeOptions(
                    audioType: AudioType,
                    videoType: VideoType,
                    width: Width,
                    height: Height,
                    resizeMode: ResizeMode,
                    quality: Quality,
                    optimize: Optimize,
                    weightX: WeightX,
                    weightY: WeightY
                )
            );
        }
    }

    public static JobArguments ParseArguments(string[] args)
    {
        var builder = new JobArgumentsBuilder();
        foreach (var arg in args)
        {
            ReadOnlySpan<char> span = arg;
            if (span.Length < 3 || span[0] != '-')
            {
                throw new InvalidOperationException($"Invalid argument: \"{arg}\".");
            }
            if (span[1] == '-')
            {
                var raw = span[2..];
                var eqIndex = raw.IndexOf('=');
                var name = eqIndex == -1 ? raw : raw[..eqIndex];
                var value = eqIndex == -1 ? null : new string(raw[(eqIndex + 1)..]);
                if (name.Equals("source", StringComparison.Ordinal) && value is not null)
                {
                    if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
                    {
                        throw new InvalidOperationException($"\"{value}\" is not a valid option for \"source\".");
                    }
                    builder.Source = uri;
                }
                else if (name.Equals("destination", StringComparison.Ordinal) && value is not null)
                {
                    if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
                    {
                        throw new InvalidOperationException($"\"{value}\" is not a valid option for \"destination\".");
                    }
                    builder.Destination = uri;
                }
                else if (name.Equals("audio", StringComparison.Ordinal) && value is not null)
                {
                    builder.AudioType = value;
                }
                else if (name.Equals("video", StringComparison.Ordinal) && value is not null)
                {
                    if (!VideoSettings.TryParse(value, default, out var settings))
                    {
                        throw new InvalidOperationException($"\"{value}\" is not a valid option for \"video\".");
                    }
                    builder.VideoType = settings;
                }
                else if (name.Equals("width", StringComparison.Ordinal) && value is not null)
                {
                    if (!int.TryParse(value, NumberStyles.Number, default, out var num))
                    {
                        throw new InvalidOperationException($"\"{value}\" is not a valid option for \"width\".");
                    }
                    builder.Width = num;
                }
                else if (name.Equals("height", StringComparison.Ordinal) && value is not null)
                {
                    if (!int.TryParse(value, NumberStyles.Number, default, out var num))
                    {
                        throw new InvalidOperationException($"\"{value}\" is not a valid option for \"height\".");
                    }
                    builder.Height = num;
                }
                else if (name.Equals("mode", StringComparison.Ordinal) && value is not null)
                {
                    builder.ResizeMode = value;
                }
                else if (name.Equals("quality", StringComparison.Ordinal) && value is not null)
                {
                    if (!int.TryParse(value, NumberStyles.Number, default, out var num))
                    {
                        throw new InvalidOperationException($"\"{value}\" is not a valid option for \"quality\".");
                    }
                    builder.Quality = num;
                }
                else if (name.Equals("optimize", StringComparison.Ordinal))
                {
                    builder.Optimize = value switch
                    {
                        null or "true" => true,
                        "false" => false,
                        _ => throw new InvalidOperationException($"\"{value}\" is not a valid option for \"optimize\".")
                    };
                }
                else if (name.Equals("wx", StringComparison.Ordinal) && value is not null)
                {
                    if (!int.TryParse(value, NumberStyles.Number, default, out var num))
                    {
                        throw new InvalidOperationException($"\"{value}\" is not a valid option for \"wx\".");
                    }
                    builder.WeightX = num;
                }
                else if (name.Equals("wy", StringComparison.Ordinal) && value is not null)
                {
                    if (!int.TryParse(value, NumberStyles.Number, default, out var num))
                    {
                        throw new InvalidOperationException($"\"{value}\" is not a valid option for \"wy\".");
                    }
                    builder.WeightX = num;
                }
                else
                {
                    throw new InvalidOperationException($"Invalid argument: \"{arg}\".");
                }
            }
            else
            {
                throw new InvalidOperationException($"Invalid argument: \"{arg}\".");
            }
        }
        return builder.Build();
    }
}