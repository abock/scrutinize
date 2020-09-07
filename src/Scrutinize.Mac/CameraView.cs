// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

using AppKit;
using AVFoundation;
using CoreAnimation;
using CoreGraphics;
using CoreMedia;

using Scrutinize.Models;

namespace Scrutinize.Mac
{
    sealed class CameraView : NSView, ICameraView
    {
        static int s_cameraCount;

        readonly int _cameraId;
        readonly AVSampleBufferDisplayLayer _videoLayer;
        readonly RtspStreamer? _streamer;
        readonly nuint[] _sampleSizes = new nuint[1];

        CMVideoFormatDescription? _videoFormatDescription;

        public Camera Camera { get; }

        public CameraView(Camera camera)
        {
            _cameraId = s_cameraCount++;

            Camera = camera
                ?? throw new ArgumentNullException(nameof(camera));

            WantsLayer = true;

            _videoLayer = new AVSampleBufferDisplayLayer();

            if (Layer is CALayer rootLayer)
                rootLayer.AddSublayer(_videoLayer);

            if (camera.Feeds.Count > 0)
            {
                _streamer = new RtspStreamer(
                    camera.Feeds[0],
                    HandleRtspFrameReceived);

                _streamer.StartAsync().ContinueWith(task => { });
            }

            Notifications.ObserveFrameChanged((sender, e) =>
            {
                CATransaction.Begin();
                CATransaction.DisableActions = true;
                _videoLayer.Frame = new CGRect(default, Frame.Size);
                CATransaction.Commit();
            });
        }

        unsafe void HandleRtspFrameReceived(
            List<byte[]>? spsPpsSegments,
            int startCodeSize,
            ArraySegment<byte> frameBuffer)
        {
            if (spsPpsSegments is object)
            {
                _videoFormatDescription = CMVideoFormatDescription
                    .FromH264ParameterSets(
                        spsPpsSegments,
                        startCodeSize,
                        out var error);

                Console.WriteLine(
                    "[{0}]: SPS PPS Updated: {1}x{2}, {3}.{4}",
                    _cameraId,
                    _videoFormatDescription.Dimensions.Width,
                    _videoFormatDescription.Dimensions.Height,
                    _videoFormatDescription.MediaSubType,
                    _videoFormatDescription.MediaType);
            }

            if (_videoFormatDescription is null)
                return;

            fixed (byte *frameBufferPtr = &frameBuffer.Array[0])
            {
                var frameBufferSize = (nuint)frameBuffer.Count;
                _sampleSizes[0] = frameBufferSize;

                var blockBuffer = CMBlockBuffer.FromMemoryBlock(
                    (IntPtr)frameBufferPtr,
                    frameBufferSize,
                    null,
                    0,
                    frameBufferSize,
                    CMBlockBufferFlags.AlwaysCopyData,
                    out var blockError); ;

                if (blockBuffer is object &&
                    blockError == CMBlockBufferError.None)
                {
                    var sampleBuffer = CMSampleBuffer.CreateReady(
                        blockBuffer,
                        _videoFormatDescription,
                        1,
                        null,
                        _sampleSizes,
                        out var sampleError);

                    if (sampleBuffer is object &&
                        sampleError == CMSampleBufferError.None)
                    {
                        sampleBuffer.GetSampleAttachments(true)[0]
                            .DisplayImmediately = true;

                        if (_videoLayer.ReadyForMoreMediaData)
                            _videoLayer.Enqueue(sampleBuffer);
                    }
                }
            }
        }
    }
}