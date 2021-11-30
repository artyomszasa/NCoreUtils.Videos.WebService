using System;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace NCoreUtils.Videos.WebService
{
    static class Utf8JsonReaderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ReadOrFail(this ref Utf8JsonReader reader)
        {
            if (!reader.Read())
            {
                throw new InvalidOperationException("Unexpected end of json stream");
            }
        }
    }
}