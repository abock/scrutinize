// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using System;

using Newtonsoft.Json;

namespace Scrutinize.Models
{
    public readonly struct CameraId : IEquatable<CameraId>
    {
        readonly string _value;

        [JsonConstructor]
        public CameraId(string? value = null)
            => _value = value is null || string.IsNullOrWhiteSpace(value)
                ? Guid.NewGuid().ToString()
                : value;

        public bool Equals(CameraId other)
            => string.Equals(
                other._value,
                _value,
                StringComparison.Ordinal);

        public override bool Equals(object obj)
            => obj is CameraId other && Equals(other);

        public override int GetHashCode()
            => _value.GetHashCode();

        public override string ToString()
            => _value;

        public static bool operator ==(CameraId left, CameraId right)
            => left.Equals(right);

        public static bool operator !=(CameraId left, CameraId right)
            => !left.Equals(right);
    }
}