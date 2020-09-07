// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;

using AppKit;
using Foundation;

using Scrutinize.Controllers;
using Scrutinize.Models;

namespace Scrutinize.Mac
{
    [Register(nameof(DocumentView))]
    sealed class DocumentView : NSView
    {
        readonly CameraGroupView _cameraGroupView;

        WorkspaceController? _workspaceController;
        public WorkspaceController? WorkspaceController
        {
            get => _workspaceController;
            set
            {
                if (_workspaceController is object)
                    _workspaceController.PropertyChanged
                        -= HandleWorkspaceControllerPropertyChanged;

                _workspaceController = value;

                if (_workspaceController is object)
                    _workspaceController.PropertyChanged
                        += HandleWorkspaceControllerPropertyChanged;

                ReloadWorkspace(_workspaceController?.Workspace);
            }
        }

        public Workspace Workspace => WorkspaceController?.Workspace
            ?? Workspace.Empty;

        DocumentView(IntPtr handle) : base(handle)
        {
            _cameraGroupView = new CameraGroupView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            AddSubview(_cameraGroupView);

            _cameraGroupView.LeadingAnchor
                .ConstraintEqualToAnchor(LeadingAnchor).Active = true;

            _cameraGroupView.TrailingAnchor
                .ConstraintEqualToAnchor(TrailingAnchor).Active = true;

            _cameraGroupView.TopAnchor
                .ConstraintEqualToAnchor(TopAnchor).Active = true;

            _cameraGroupView.BottomAnchor
                .ConstraintEqualToAnchor(BottomAnchor).Active = true;
        }

        void HandleWorkspaceControllerPropertyChanged(
            object sender,
            PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WorkspaceController.Workspace))
                ReloadWorkspace(WorkspaceController?.Workspace);
        }

        void ReloadWorkspace(Workspace? workspace)
        {
            if (workspace?.CameraGroups
                is IReadOnlyList<CameraGroup> cameraGroups &&
                cameraGroups.Count > 0)
                _cameraGroupView.Reload(workspace, cameraGroups[0]);
            else
                _cameraGroupView.Reload(workspace, null);
        }
    }
}