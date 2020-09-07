// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using AppKit;
using Foundation;

namespace Scrutinize.Mac
{
    [Register(nameof(AppDelegate))]
    public class AppDelegate : NSApplicationDelegate
    {
        public override bool ApplicationShouldOpenUntitledFile(
            NSApplication sender)
            => false;
    }
}