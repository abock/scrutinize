// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using System;

namespace Scrutinize.Models
{
    public sealed class InvalidScrutinizeDocumentException : Exception
    {
        internal InvalidScrutinizeDocumentException(
            string path,
            Exception? innerException = null)
            : base(
                  $"The document {path} is not a valid Scrutinize document.",
                  innerException)
        {
        }
    }
}