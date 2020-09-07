// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Scrutinize.H264
{
    public sealed class NaluParserTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("01")]
        [InlineData("00 01")]
        [InlineData("00 00 01")]
        [InlineData("00 00 00 01")]
        [InlineData("00 00 00 02 00")]
        [InlineData("00 00 00 01 00", "00")]
        [InlineData("00 00 00 01 67", "67")]
        [InlineData("00 00 00 01 67 aa", "67 aa")]
        [InlineData("00 00 00 01 67 aa bb", "67 aa bb")]
        [InlineData("00 00 00 01 67 aa bb cc", "67 aa bb cc")]
        [InlineData("00 00 00 01 67 aa bb cc dd", "67 aa bb cc dd")]
        [InlineData("00 00 00 01 67 aa bb cc dd ee", "67 aa bb cc dd ee")]
        [InlineData("00 00 00 01 67 aa bb cc dd ee ff", "67 aa bb cc dd ee ff")]
        [InlineData("00 00 00 01 67 aa bb 00 00 00 01 68 cc dd",
            "67 aa bb",
            "68 cc dd")]
         [InlineData("00 00 00 01 67 aa bb 00 00 00 01 68 00 00 00 01 69",
            "67 aa bb",
            "68",
            "69")]
         [InlineData("00 00 00 01 67 aa bb 00 00 00 01 68 00 00 00 01 69 cc",
            "67 aa bb",
            "68",
            "69 cc")]
         [InlineData("00 00 00 01 67 aa bb 00 00 00 01 68 00 00 00 01 69 cc 00 00 00 02",
            "67 aa bb",
            "68",
            "69 cc 00 00 00 02")]
         [InlineData("00 00 00 01 67 aa bb 00 00 00 01 68 00 00 00 01 69 cc 00 00 00 01",
            "67 aa bb",
            "68",
            "69 cc 00 00 00 01")]
        public unsafe void ParseValidAnnexBNaluSegments(
            string hexString,
            params string[] expectedHexSegments)
        {
            for (int i = 0; i < 2; i++)
            {
                var buffer = ParseHex(hexString);
                if (buffer.Length == 0)
                {
                    Assert.Equal(0, expectedHexSegments.Length);
                    return;
                }

                var recycleBuffers = new byte[0][];

                if (i == 1)
                {
                    recycleBuffers = new[]
                    {
                        Array.Empty<byte>(),
                        Array.Empty<byte>(),
                        Array.Empty<byte>(),
                        Array.Empty<byte>(),
                        Array.Empty<byte>(),
                        Array.Empty<byte>()
                    };
                };

                fixed (byte* bufferPtr = &buffer[0])
                {
                    var segments = new List<byte[]>(recycleBuffers);

                    Nalu.ParseAnnexBNaluSegments(
                        bufferPtr,
                        buffer.Length,
                        segments,
                        out _);

                    Assert.Equal(
                        expectedHexSegments
                            .Select(s => ParseHex(s))
                            .ToArray(),
                        segments
                            .Select(s => s.ToArray())
                            .ToArray());
                }
            }
        }

        static byte[] ParseHex(string hexString, bool ignoreWhiteSpace = true)
        {
            if (string.IsNullOrEmpty(hexString))
                return Array.Empty<byte>();

            var buffer = new byte[hexString.Length / 2];
            var bufferLength = 0;

            byte highNibble = 0;

            for (int i = 0, j = 0; i < hexString.Length; i++)
            {
                var c = hexString[i];
                if (ignoreWhiteSpace && char.IsWhiteSpace(c))
                    continue;

                byte nibble;
                if (c >= '0' && c <= '9')
                    nibble = (byte)(c - '0');
                else if (c >= 'a' && c <= 'f')
                    nibble = (byte)(c - 'a' + 10);
                else if (c >= 'A' && c <= 'F')
                    nibble = (byte)(c - 'A' + 10);
                else
                    throw new Exception(
                        $"Invalid hex character '{c}' " +
                        $"(0x{(int)c:x2}) at index {i}");

                if (j++ % 2 == 0)
                    highNibble = (byte)(nibble << 4);
                else
                    buffer[bufferLength++] = (byte)(highNibble | nibble);
            }

            Array.Resize(ref buffer, bufferLength);

            return buffer;
        }
    }
}
