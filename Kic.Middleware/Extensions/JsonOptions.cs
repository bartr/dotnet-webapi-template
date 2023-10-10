// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kic.Middleware;

public static class JsonOptions
{
    public static JsonSerializerOptions GetDefault()
    {
        // default serialization options
        // ignore nulls in json
        JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            ReadCommentHandling = JsonCommentHandling.Skip,
            IgnoreReadOnlyProperties = false,
            AllowTrailingCommas = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };

        // serialize enums as strings
        jsonOptions.Converters.Add(new JsonStringEnumConverter());

        return jsonOptions;
    }
}
