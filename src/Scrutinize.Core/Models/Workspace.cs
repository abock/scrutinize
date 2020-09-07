// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace Scrutinize.Models
{
    public sealed class Workspace : IEquatable<Workspace>
    {
        const string Magic = "// WqHqXr4gZbfj71963vJTi";

        static readonly Encoding s_encoding = new UTF8Encoding(false);

        public static Workspace Empty { get; } = new Workspace(null, null);

        public IReadOnlyList<Camera> Cameras { get; }
        public IReadOnlyList<CameraGroup> CameraGroups { get; }

        [JsonConstructor]
        public Workspace(
            IReadOnlyList<Camera>? cameras,
            IReadOnlyList<CameraGroup>? cameraGroups)
        {
            Cameras = cameras ?? Array.Empty<Camera>();
            CameraGroups = cameraGroups ?? Array.Empty<CameraGroup>();
        }

        public bool Equals(Workspace other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return string.Equals(
                ToString(),
                other.ToString(),
                StringComparison.Ordinal);
        }

        public bool TryGetCamera(CameraId id, out Camera camera)
        {
            camera = Cameras.FirstOrDefault(camera => camera.Id == id);
            return camera is Camera;
        }

        public Camera? GetCamera(CameraId id)
        {
            if (TryGetCamera(id, out var camera))
                return camera;
            return null;
        }

        public IReadOnlyList<Camera> GetCameras(CameraGroup? cameraGroup)
        {
            if (cameraGroup is null)
                return Array.Empty<Camera>();

            return cameraGroup
                .CameraIds
                .Select(id => GetCamera(id))
                .Where(camera => camera is Camera)
                .Cast<Camera>()
                .ToList();
        }

        #region Serialization

        public static Workspace FromFile(string filePath)
        {
            try
            {
                using var fileStream = File.OpenRead(filePath);
                using var textReader = new StreamReader(fileStream);

                var magicLine = textReader.ReadLine().Trim();
                if (magicLine != Magic)
                    throw new InvalidScrutinizeDocumentException(filePath);

                using var jsonReader = new JsonTextReader(textReader);
                var jsonSerializer = new ScrutinizeJsonSerializer();
                return jsonSerializer.Deserialize<Workspace>(jsonReader)
                    ?? Empty;
            }
            catch (Exception e) when (!(e is InvalidScrutinizeDocumentException))
            {
                throw new InvalidScrutinizeDocumentException(filePath, e);
            }
        }

        public void Write(TextWriter textWriter, bool indented = true)
        {
            textWriter.WriteLine(Magic);
            var jsonWriter = new JsonTextWriter(textWriter);
            var jsonSerializer = new ScrutinizeJsonSerializer
            {
                Formatting = indented
                  ? Formatting.Indented
                  : Formatting.None
            };
            jsonSerializer.Serialize(jsonWriter, this);
        }

        public void Write(Stream stream, bool indented = true)
            => Write(
                new StreamWriter(stream, s_encoding, 4096, true)
                {
                    AutoFlush = true
                },
                indented);

        public void Write(string filePath, bool indented = true)
        {
            using var fileStream = File.OpenWrite(filePath);
            Write(fileStream, indented);
            fileStream.Flush();
        }

        public override string ToString()
            => ToString(indented: true);

        public string ToString(bool indented)
        {
            using var writer = new StringWriter();
            Write(writer, indented);
            return writer.ToString();
        }

        #endregion
    }
}