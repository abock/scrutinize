// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using System;

using AppKit;
using Foundation;

using Scrutinize.Controllers;
using Scrutinize.Models;

namespace Scrutinize.Mac
{
    [Register(nameof(Document))]
    sealed class Document : NSDocument
    {
        public WorkspaceController WorkspaceController { get; }
            = new WorkspaceController();

        Document(IntPtr handle) : base(handle)
        {
        }

        public override void WindowControllerDidLoadNib(
            NSWindowController windowController)
            => base.WindowControllerDidLoadNib(windowController);

        [Export("autosavesInPlace")]
        public static bool AutosaveInPlace() => true;

        public new static bool UsesUbiquitousStorage
        {
            [Export("usesUbiquitousStorage")]
            get => false;
        }

        public override void MakeWindowControllers()
            => AddWindowController((NSWindowController)NSStoryboard
                .FromName("Main", null)
                .InstantiateControllerWithIdentifier(
                    "Document Window Controller"));

        public override bool ReadFromUrl(
            NSUrl url,
            string typeName,
            out NSError? outError)
        {
            WorkspaceController.Workspace = Workspace.FromFile(url.Path);
            outError = null;
            return true;
        }

        public override bool WriteToUrl(
            NSUrl url,
            string typeName,
            out NSError? outError)
        {
            WorkspaceController.Workspace.Write(url.Path);
            outError = null;
            return true;
        }
    }
}