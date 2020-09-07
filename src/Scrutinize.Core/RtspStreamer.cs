// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using RtspClientSharp;
using RtspClientSharp.RawFrames;
using RtspClientSharp.RawFrames.Video;

using Scrutinize.H264;
using Scrutinize.Models;

namespace Scrutinize
{
    public sealed class RtspStreamer
    {
        readonly Action<List<byte[]>?, int, ArraySegment<byte>> _frameReceivedHandler;
        readonly List<byte[]> _spsPpsNaluSegmentBuffers = new List<byte[]>();
        List<byte[]>? _previousPpsNaluSegmentBuffers;
        int _startCodeSize;

        public CameraFeed Feed { get; }

        public RtspStreamer(
            CameraFeed feed,
            Action<List<byte[]>?, int, ArraySegment<byte>> frameReceivedHandler)
        {
            Feed = feed
                ?? throw new ArgumentNullException(nameof(feed));
            _frameReceivedHandler = frameReceivedHandler
                ?? throw new ArgumentNullException(nameof(frameReceivedHandler));
        }

        public async Task StartAsync(
            CancellationToken cancellationToken = default)
        {
            var connectionParameters = new ConnectionParameters(Feed.Uri)
            {
                RtpTransport = Feed.RtpTransportProtocol
            };

            using var rtspClient = new RtspClient(connectionParameters);

            rtspClient.FrameReceived += HandleFrameReceived;

            await rtspClient.ConnectAsync(cancellationToken);
            await rtspClient.ReceiveAsync(cancellationToken);
        }

        static unsafe bool SegmentBuffersEqual(List<byte[]>? a, List<byte[]>? b)
        {
            if (a is null || b is null || a.Count != b.Count)
                return false;

            for (var i = 0; i < a.Count; i++)
            {
                var ai = a[i];
                var bi = b[i];

                if (ai == bi)
                    continue;

                if (ai is null || bi is null || ai.Length != bi.Length)
                    return false;

                fixed (byte* p1 = ai, p2 = bi)
                {
                    byte* x1 = p1, x2 = p2;
                    var length = ai.Length;

                    for (var j = 0; j < length / 8; j++, x1 += 8, x2 += 8)
                    {
                        if (*(long*)x1 != *(long*)x2)
                            return false;
                    }

                    if ((length & 4) != 0)
                    {
                        if (*(int*)x1 != *(int*)x2)
                            return false;

                        x1 += 4;
                        x2 += 4;
                    }

                    if ((length & 2) != 0)
                    {
                        if (*(short*)x1 != *(short*)x2)
                            return false;
                        x1 += 2;
                        x2 += 2;
                    }

                    if ((length & 1) != 0 && *x1 != *x2)
                        return false;
                }
            }

            return true;
        }

        unsafe void HandleFrameReceived(object sender, RawFrame e)
        {
            switch (e)
            {
                case RawH264IFrame iFrame:
                    fixed (byte* spsPpsPtr = &iFrame.SpsPpsSegment.Array[
                        iFrame.SpsPpsSegment.Offset])
                    {
                        Nalu.ParseAnnexBNaluSegments(
                            spsPpsPtr,
                            iFrame.SpsPpsSegment.Count,
                            _spsPpsNaluSegmentBuffers,
                            out _startCodeSize);

                        if (!SegmentBuffersEqual(
                            _spsPpsNaluSegmentBuffers,
                            _previousPpsNaluSegmentBuffers))
                        {
                            _previousPpsNaluSegmentBuffers
                                = _spsPpsNaluSegmentBuffers;
                            _frameReceivedHandler(
                                _spsPpsNaluSegmentBuffers,
                                _startCodeSize,
                                PrepareFrame(
                                    _startCodeSize,
                                    e.FrameSegment));
                            return;
                        }
                    }
                    break;
                case RawH264PFrame pFrame:
                    break;
                default:
                    return;
            }

            _frameReceivedHandler(
                null,
                _startCodeSize,
                PrepareFrame(
                    _startCodeSize,
                    e.FrameSegment));
        }

        unsafe ArraySegment<byte> PrepareFrame(
            int startCodeSize,
            ArraySegment<byte> frameBuffer)
        {
            var lengthWithoutHeader = frameBuffer.Count - startCodeSize;
            fixed (byte* framePtr = &frameBuffer.Array[frameBuffer.Offset])
            {
                *(framePtr + 0) = (byte)(lengthWithoutHeader >> 24);
                *(framePtr + 1) = (byte)(lengthWithoutHeader >> 16);
                *(framePtr + 2) = (byte)(lengthWithoutHeader >> 8);
                *(framePtr + 3) = (byte)(lengthWithoutHeader >> 0);
            }
            return frameBuffer;
        }
    }
}