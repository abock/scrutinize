// https://github.com/abock/scrutinize
// Copyright 2020 Aaron Bockover.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Scrutinize.Models
{
    sealed class ScrutinizeJsonSerializer : JsonSerializer
    {
        public ScrutinizeJsonSerializer()
        {
            Formatting = Formatting.Indented;
            NullValueHandling = NullValueHandling.Ignore;
            DefaultValueHandling = DefaultValueHandling.Ignore;

            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            Converters.Add(new CameraIdConverter());
            Converters.Add(new StringEnumConverter());
        }
    }
}