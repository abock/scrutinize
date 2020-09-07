// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using System;

using Newtonsoft.Json;

namespace Scrutinize.Models
{
    sealed class CameraIdConverter : JsonConverter<CameraId>
    {
        public override CameraId ReadJson(
            JsonReader reader,
            Type objectType,
            CameraId existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
            => new CameraId((string?)reader.Value);

        public override void WriteJson(
            JsonWriter writer,
            CameraId value,
            JsonSerializer serializer)
            => writer.WriteValue(value.ToString());
    }
}