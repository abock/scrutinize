// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using AppKit;
using CoreGraphics;

using Scrutinize.Models;

namespace Scrutinize.Mac
{
    sealed class CameraGroupView : NSView
    {
        Workspace? _workspace;
        CameraGroup? _cameraGroup;
        CameraGroupLayout<CameraView>? _cameraGroupLayout;

        public void Reload(
            Workspace? workspace,
            CameraGroup? cameraGroup)
        {
            _workspace = workspace;
            _cameraGroup = cameraGroup;
            _cameraGroupLayout = new CameraGroupLayout<CameraView>(
                _workspace,
                _cameraGroup,
                camera =>
                {
                    var cameraView = new CameraView(camera);
                    AddSubview(cameraView);
                    return cameraView;
                },
                (cameraView, frame) =>
                {
                    cameraView.Frame = new CGRect(
                        frame.X,
                        frame.Y,
                        frame.Width,
                        frame.Height);
                },
                cameraView =>
                {
                    cameraView.RemoveFromSuperview();
                    cameraView.Dispose();
                });

            NeedsLayout = true;
        }

        public override void Layout()
        {
            _cameraGroupLayout?.Layout(
                Frame.Width,
                Frame.Height,
                !IsFlipped);

            base.Layout();
        }
    }
}