using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Sets values as specified by a JSONPath query into a JSON file.
/// </summary>
public class JsonPoke : Task
{
    /// <summary>
    /// Specifies the JSON input as a file path.
    /// </summary>
    [Required]
    public ITaskItem? JsonInputPath { get; set; }

    /// <summary>
    /// Specifies the JSONPath query.
    /// </summary>
    [Required]
    public string Query { get; set; } = "$";

    /// <summary>
    /// Specifies the value to be inserted into the specified path.
    /// </summary>
    public ITaskItem[] Value { get; set; } = Array.Empty<ITaskItem>();

    /// <summary>
    /// Specifies the raw (JSON) value to be inserted into the specified path.
    /// </summary>
    public string? RawValue { get; set; }

    /// <summary>
    /// Property names to set for a JSON object value from matching 
    /// item metadata values from <see cref="Value"/> item(s).
    /// </summary>
    public ITaskItem[] Properties { get; set; } = Array.Empty<ITaskItem>();

    /// <summary>
    /// Locates nodes matching the <see cref="Query"/> and replaces their contents 
    /// with the provided <see cref="Value"/>.
    /// </summary>
    public override bool Execute()
    {
        if (JsonInputPath == null)
            return Log.Error("JPO01", $"{nameof(JsonInputPath)} is required.");

        var filePath = JsonInputPath.GetMetadata("FullPath");

        if (!File.Exists(filePath))
            return Log.Error("JPO02", $"Specified {nameof(JsonInputPath)} not found at {filePath}.");

        var content = File.ReadAllText(filePath);

        if (string.IsNullOrEmpty(content))
            return Log.Error("JPO03", $"Empty JSON content.");

        if (Value.Length == 0 && RawValue == null)
            return Log.Warn("JPO04", $"No value(s) specified.", true);

        if (Value.Length > 0 && RawValue != null)
            return Log.Error("JPO05", $"Cannot specify both {nameof(Value)} and {nameof(RawValue)}.");

        var jvalue = new Lazy<JToken>(GetValue);

        var json = JObject.Parse(content!);
        foreach (var node in json.SelectTokens(Query))
        {
            if (Value.Length > 1 || Properties.Length != 0 || RawValue != null)
            {
                // We'll be doing complex object replacement, 
                // so just replace the whole thing in one shot, 
                // no smarts for target-type selection.
                node.Replace(jvalue.Value);
                continue;
            }

            // If the Value.Length == 1 OR Properties.Length == 0 (meaning 
            // we're not entering above condition for either arrays or complex 
            // objects), it's a single value, which we can target-type to the
            // native node type being replaced. This allows us to preserve the
            // native JSON type whenever possible.

            switch (node.Type)
            {
                case JTokenType.String:
                    node.Replace(new JValue(Value[0].ItemSpec));
                    break;
                case JTokenType.Array:
                    node.Replace(new JArray(jvalue.Value));
                    break;
                case JTokenType.Integer when long.TryParse(Value[0].ItemSpec, out var typed):
                    node.Replace(new JValue(typed));
                    break;
                case JTokenType.Float when decimal.TryParse(Value[0].ItemSpec, out var typed):
                    node.Replace(new JValue(typed));
                    break;
                case JTokenType.Boolean when bool.TryParse(Value[0].ItemSpec, out var typed):
                    node.Replace(new JValue(typed));
                    break;
                case JTokenType.Date when DateTime.TryParseExact(Value[0].ItemSpec, "O", CultureInfo.CurrentCulture, DateTimeStyles.RoundtripKind, out var typed):
                    node.Replace(new JValue(typed));
                    break;
                case JTokenType.Date when DateTime.TryParse(Value[0].ItemSpec, out var typed):
                    node.Replace(new JValue(typed));
                    break;
                case JTokenType.Guid when Guid.TryParse(Value[0].ItemSpec, out var typed):
                    node.Replace(new JValue(typed));
                    break;
                case JTokenType.Uri when Uri.TryCreate(Value[0].ItemSpec, UriKind.RelativeOrAbsolute, out var typed):
                    node.Replace(new JValue(typed));
                    break;
                case JTokenType.TimeSpan when TimeSpan.TryParse(Value[0].ItemSpec, out var typed):
                    node.Replace(new JValue(typed));
                    break;
                default:
                    // Default to just replacing with whatever value we calculated.
                    node.Replace(jvalue.Value);
                    break;
            }
        }

        File.WriteAllText(filePath, json.ToString(Formatting.Indented));

        return true;
    }

    JToken GetValue()
    {
        if (Value.Length == 1)
            return GetValue(Value[0]);
        else if (RawValue != null)
            return JToken.Parse(RawValue);

        return new JArray(Value.Select(GetValue).ToArray());
    }

    JToken GetValue(ITaskItem item)
    {
        if (Properties.Length == 0)
            return GetTypedValue(item.ItemSpec);

        var value = new JObject();
        foreach (var prop in Properties)
            value[prop.ItemSpec] = GetTypedValue(item.GetMetadata(prop.ItemSpec));

        return value;
    }

    JToken GetTypedValue(string itemSpec)
    {
        if ((itemSpec.StartsWith("\"") && itemSpec.EndsWith("\"")) ||
            (itemSpec.StartsWith("'") && itemSpec.EndsWith("'")))
            return new JValue(itemSpec.Trim('\'', '"'));

        try
        {
            return JToken.Parse(itemSpec);
        }
        catch
        {
            return new JValue(itemSpec);
        }
    }
}
