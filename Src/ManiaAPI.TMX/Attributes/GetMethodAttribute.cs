﻿using System.Text.Json.Serialization;

namespace ManiaAPI.TMX.Attributes;

[AttributeUsage(AttributeTargets.Method)]
class GetMethodAttribute<TJsonContext> : Attribute where TJsonContext : JsonSerializerContext
{
    public string Route { get; }

    public GetMethodAttribute(string route)
    {
        Route = route;
    }
}