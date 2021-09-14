using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json.Linq;

/// <summary>
/// Returns values as specified by a JSONPath Query from a JSON file.
/// </summary>
public class JsonPeek : Task
{
    /// <summary>
    /// Specifies the JSON input as a string.
    /// </summary>
    public string? JsonContent { get; set; }

    /// <summary>
    /// Specifies the JSON input as a file path.
    /// </summary>
    public ITaskItem? JsonInputPath { get; set; }

    /// <summary>
    /// Specifies the JSONPath query.
    /// </summary>
    [Required]
    public string Query { get; set; } = "$";

    /// <summary>
    /// Contains the results that are returned by the task.
    /// </summary>
    [Output]
    public ITaskItem[] Result { get; private set; } = new ITaskItem[0];

    /// <summary>
    /// Executes the <see cref="Query"/> against either the 
    /// <see cref="JsonContent"/> or <see cref="JsonInputPath"/> JSON.
    /// </summary>
    public override bool Execute()
    {
        if (JsonContent == null && JsonInputPath == null)
            return Log.Error("JPE01", $"Either {nameof(JsonContent)} or {nameof(JsonInputPath)} must be provided.");

        if (JsonInputPath != null && !File.Exists(JsonInputPath.GetMetadata("FullPath")))
            return Log.Error("JPE02", $"Specified {nameof(JsonInputPath)} not found at {JsonInputPath.GetMetadata("FullPath")}.");

        if (JsonContent != null && JsonInputPath != null)
            return Log.Error("JPE03", $"Cannot specify both {nameof(JsonContent)} and {nameof(JsonInputPath)}.");

        var content = JsonInputPath != null ?
            File.ReadAllText(JsonInputPath.GetMetadata("FullPath")) : JsonContent;

        if (string.IsNullOrEmpty(content))
            return Log.Warn("JPE04", $"Empty JSON content.", true);

        var json = JObject.Parse(content!);

        Result = json.SelectTokens(Query).SelectMany(x => x.AsItems()).ToArray();

        return true;
    }
}
