// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Scrutinize.Models
{
    public sealed class CameraGroup
    {
        public string Name { get; }
        public IReadOnlyList<CameraId> CameraIds { get; }

        [JsonConstructor]
        public CameraGroup(
            string name,
            IReadOnlyList<CameraId> cameraIds)
        {
            Name = name;
            CameraIds = cameraIds ?? Array.Empty<CameraId>();
        }
    }
}