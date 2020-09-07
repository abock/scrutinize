// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using System;

using AppKit;

using Foundation;

namespace Scrutinize.Mac
{
    [Register(nameof(DocumentViewController))]
    sealed class DocumentViewController : NSViewController
    {
        DocumentViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidAppear()
        {
            base.ViewDidAppear();

            if (View is DocumentView documentView &&
                documentView.Window is NSWindow window &&
                NSDocumentController
                    .SharedDocumentController
                    .DocumentForWindow(window) is Document document &&
                document.WorkspaceController is object)
                documentView.WorkspaceController = document.WorkspaceController;
        }
    }
}