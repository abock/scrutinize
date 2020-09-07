// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using AppKit;

namespace Scrutinize.Mac
{
    static class Program
    {
        static void Main(string[] args)
        {
            NSApplication.Init();
            NSApplication.Main(args);
        }
    }
}