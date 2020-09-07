// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Scrutinize.H264
{
    public static unsafe class Nalu
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void YieldSegment(
            byte* start,
            byte* end,
            ref int segmentCount,
            List<byte[]> segments)
        {
            start += 4;
            if (start >= end)
                return;

            var length = (int)(end - start);
            byte[] buffer;

            if (segmentCount < segments.Count)
            {
                buffer = segments[segmentCount];
                if (buffer.Length != length)
                {
                    Array.Resize(ref buffer, length);
                    segments[segmentCount] = buffer;
                }
            }
            else
            {
                buffer = new byte[length];
                segments.Add(buffer);
            }

            segmentCount++;

            Marshal.Copy((IntPtr)start, buffer, 0, length);
        }

        public static unsafe void ParseAnnexBNaluSegments(
            byte* buffer,
            int bufferLength,
            List<byte[]> segments,
            out int startCodeSize)
        {
            const int StartCodeSize = 4;

            startCodeSize = StartCodeSize;

            if (bufferLength <= StartCodeSize || *(uint*)buffer != 0x01000000)
            {
                segments.Clear();
                return;
            }

            var segmentCount = 0;
            var p = buffer;
            var end = buffer + bufferLength;
            var naluStart = p;

            while (true)
            {
                if (*(uint*)p == 0x01000000 && p + StartCodeSize < end)
                {
                    if (p > buffer)
                        YieldSegment(
                            naluStart,
                            p,
                            ref segmentCount,
                            segments);
                    naluStart = p;
                    p += StartCodeSize;
                }

                if (p >= end - StartCodeSize)
                {
                    YieldSegment(
                        naluStart,
                        end,
                        ref segmentCount,
                        segments);
                    break;
                }

                p++;
            }

            for (var i = segments.Count - 1; i >= segmentCount; i--)
                segments.RemoveAt(i);
        }
    }
}