// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using System;

using Newtonsoft.Json;

using RtspClientSharp;

namespace Scrutinize.Models
{
    public sealed class CameraFeed
    {
        public string Name { get; }
        public Uri Uri { get; }
        public RtpTransportProtocol RtpTransportProtocol { get; }

        [JsonConstructor]
        public CameraFeed(
            string name,
            Uri uri,
            RtpTransportProtocol rtpTransportProtocol)
        {
            Name = name;
            Uri = uri;
            RtpTransportProtocol = rtpTransportProtocol;
        }
    }
}