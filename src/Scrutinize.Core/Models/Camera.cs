// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Scrutinize.Models
{
    public sealed class Camera
    {
        public CameraId Id { get; }
        public string Name { get; }
        public IReadOnlyList<CameraFeed> Feeds { get; }

        [JsonConstructor]
        public Camera(
            CameraId id,
            string name,
            IReadOnlyList<CameraFeed>? feeds)
        {
            Id = id;
            Name = name;
            Feeds = feeds ?? Array.Empty<CameraFeed>();
        }
    }
}