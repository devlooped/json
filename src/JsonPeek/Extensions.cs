using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json.Linq;

static class Extensions
{
    public static bool Error(this TaskLoggingHelper log, string code, string message, bool result = false)
    {
        log.LogError(null, code, null, null, 0, 0, 0, 0, message);
        return result;
    }

    public static bool Warn(this TaskLoggingHelper log, string code, string message, bool result = false)
    {
        log.LogWarning(null, code, null, null, 0, 0, 0, 0, message);
        return result;
    }

    public static IEnumerable<ITaskItem> AsItems(this JToken json) => json switch
    {
        { Type: JTokenType.Object } when json is JObject obj => new ITaskItem[] { obj.AsItem() },
        { Type: JTokenType.Array } when json is JArray arr => arr.SelectMany(AsItems),
        { Type: JTokenType.Null } => Array.Empty<ITaskItem>(),
        _ => new ITaskItem[] { new TaskItem(json.ToString()) },
    };

    static ITaskItem AsItem(this JObject json)
    {
        var item = new TaskItem(json.ToString());
        // Top-level properties turned into metadata for convenience.
        foreach (var prop in json.Properties())
            item.SetMetadata(prop.Name, prop.Value.ToString());

        return item;
    }
}
