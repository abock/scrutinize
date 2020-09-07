// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using System;

namespace Scrutinize.Models
{
    public readonly struct CameraFrame : IEquatable<CameraFrame>
    {
        public Camera Camera { get; }
        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }

        public CameraFrame(
            Camera camera,
            double x,
            double y,
            double width,
            double height)
        {
            Camera = camera;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Equals(CameraFrame other)
            => other.Camera == Camera &&
                other.X == X &&
                other.Y == Y &&
                other.Width == Width &&
                other.Height == Height;

        public override bool Equals(object obj)
            => obj is CameraFrame other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(
                Camera,
                X,
                Y,
                Width,
                Height);

        public override string ToString()
            => $"{Camera.Name} [{Camera.Id}] @ {X},{Y}+{Width}x{Height}";
    }
}