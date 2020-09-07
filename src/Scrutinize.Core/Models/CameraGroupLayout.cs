// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Scrutinize.Models
{
    public sealed class CameraGroupLayout<TCameraView>
        where TCameraView : ICameraView
    {
        readonly Workspace? _workspace;
        readonly CameraGroup? _cameraGroup;
        readonly Func<Camera, TCameraView> _allocateViewHandler;
        readonly Action<TCameraView, CameraFrame> _updateFrameHandler;
        readonly Action<TCameraView> _disposeViewHandler;

        TCameraView[] _activeViews = Array.Empty<TCameraView>();

        public CameraGroupLayout(
            Workspace? workspace,
            CameraGroup? cameraGroup,
            Func<Camera, TCameraView> allocateViewHandler,
            Action<TCameraView, CameraFrame> updateFrameHandler,
            Action<TCameraView> disposeViewHandler)
        {
            _workspace = workspace;
            _cameraGroup = cameraGroup;

            _allocateViewHandler = allocateViewHandler
                ?? throw new ArgumentNullException(nameof(allocateViewHandler));

            _updateFrameHandler = updateFrameHandler
                ?? throw new ArgumentNullException(nameof(updateFrameHandler));

            _disposeViewHandler = disposeViewHandler
                ?? throw new ArgumentNullException(nameof(disposeViewHandler));
        }

        public void Layout(
            double width,
            double height,
            bool flippedGeometry)
        {
            var recycleViews = new List<TCameraView>(_activeViews);

            var cameras = _workspace?.GetCameras(_cameraGroup)
                ?? Array.Empty<Camera>();

            var cameraFrames = Tile(
                cameras,
                width,
                height,
                flippedGeometry);

            Array.Resize(ref _activeViews, cameraFrames.Count);

            for (var i = 0; i < _activeViews.Length; i++)
            {
                TCameraView cameraView;
                var frame = cameraFrames[i];

                var recycleIndex = recycleViews.FindIndex(
                    v => v.Camera == frame.Camera);
                if (recycleIndex >= 0)
                {
                    cameraView = recycleViews[recycleIndex];
                    recycleViews.RemoveAt(recycleIndex);
                }
                else
                {
                    cameraView = _allocateViewHandler(frame.Camera);
                }

                _updateFrameHandler(cameraView, frame);

                _activeViews[i] = cameraView;
            }

            foreach (var staleCameraView in recycleViews)
                _disposeViewHandler(staleCameraView);
        }

        static IReadOnlyList<CameraFrame> Tile(
            IReadOnlyList<Camera> cameras,
            double width,
            double height,
            bool flippedGeometry)
        {
            var frames = new CameraFrame[cameras.Count];

            var sqrt = Math.Sqrt(cameras.Count);
            var rows = Math.Ceiling(sqrt);
            var cols = Math.Ceiling(sqrt);

            var frameWidth = Math.Floor(width / cols);
            var frameHeight = Math.Floor(height / rows);

            var currentX = 0.0;
            var currentY = flippedGeometry
                ? height - frameHeight
                : 0.0;

            for (var i = 0; i < frames.Length; i++)
            {
                if (currentX + frameWidth > width)
                {
                    currentX = 0;
                    currentY += flippedGeometry
                        ? -frameHeight
                        : frameHeight;
                }

                frames[i] = new CameraFrame(
                    cameras[i],
                    currentX,
                    currentY,
                    frameWidth,
                    frameHeight);

                currentX += frameWidth;
            }

            return frames;
        }
    }
}