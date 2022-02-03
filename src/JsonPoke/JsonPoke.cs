using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Sets values as specified by a JSONPath query into a JSON file.
/// </summary>
public class JsonPoke : Task
{
    static readonly Regex JPathExpr = new(@"(?<property>\['(?<name>[^\]]+)'\])|(?<property>\.(?<name>[^\.\[]+))|(?<array>\[(?<end>\^)?(?<index>\d+)\])", RegexOptions.Compiled);

    /// <summary>
    /// Specifies the JSON input as a string.
    /// </summary>
    [Output]
    public string? Content { get; set; }

    /// <summary>
    /// Specifies the JSON input as a file path.
    /// </summary>
    public ITaskItem? ContentPath { get; set; }

    /// <summary>
    /// Specifies the JSONPath expression.
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
    /// Contains the updated JSON nodes.
    /// </summary>
    [Output]
    public ITaskItem[] Result { get; private set; } = new ITaskItem[0];

    /// <summary>
    /// Locates nodes matching the <see cref="Query"/> and replaces their contents 
    /// with the provided <see cref="Value"/>.
    /// </summary>
    public override bool Execute()
    {
        if (Content == null && ContentPath == null)
            return Log.Error("JPO01", $"Either {nameof(Content)} or {nameof(ContentPath)} must be provided.");

        if (ContentPath != null && !File.Exists(ContentPath.GetMetadata("FullPath")))
            return Log.Error("JPO02", $"Specified {nameof(ContentPath)} not found at {ContentPath.GetMetadata("FullPath")}.");

        if (Content != null && ContentPath != null)
            return Log.Error("JPO03", $"Cannot specify both {nameof(Content)} and {nameof(ContentPath)}.");

        var content = ContentPath != null ?
            File.ReadAllText(ContentPath.GetMetadata("FullPath")) : Content;

        if (string.IsNullOrEmpty(content))
            return Log.Error("JPO04", $"Empty JSON content.");

        if (Value.Length == 0 && RawValue == null)
            return Log.Warn("JPO05", $"No value(s) specified.", true);

        if (Value.Length > 0 && RawValue != null)
            return Log.Error("JPO06", $"Cannot specify both {nameof(Value)} and {nameof(RawValue)}.");

        var jvalue = new Lazy<JToken>(GetValue);

        var json = JObject.Parse(content!);
        var matches = JPathExpr.Matches(Query).Cast<Match>().ToList();
        // If we have any part of teh expression using our own "from end" syntax for array indexing, 
        // we know we'll be inserting a *new* 
        var nodes = matches.Any(m => m.Groups["end"].Success) ? new() : json.SelectTokens(Query).ToList();
        var result = new List<ITaskItem>();
        var owned = new HashSet<string>();

        // We couldn't match anything to Query, so attempt to 
        // create an object at the specified path.
        if (nodes.Count == 0)
        {
            // If we can't match anything, we can't create any objects.
            if (matches.Count == 0)
                return true;

            // Split paths, try to match sequentially, can't do wildcards as final segment
            for (var midx = 0; midx < matches.Count; midx++)
            {
                var match = matches[midx];
                var parent = json.SelectToken(Query[..match.Index]);
                var token = match.Groups["end"].Success ? null : json.SelectToken(Query[..(match.Index + match.Length)]);
                if (token != null)
                    continue;

                if (parent == null)
                    return Log.Error("JPO07", $"Could not find parent node for {Query[..match.Index]}.");

                // For arrays, if we don't find the element, create/add it if we can
                if (match.Groups["array"].Success)
                {
                    if (parent is not JArray array)
                    {
                        // We own the node (meaning we created it previously), so we can replace it with an array instead.
                        if (owned.Contains(Query[..match.Index]))
                        {
                            array = new JArray();
                            parent.Replace(array);
                        }
                        else
                        {
                            return Log.Warn("JPO08", $"JSONPath segment '{Query[..match.Index]}' didn't match a JSON array.");
                        }
                    }

                    if (match.Groups["index"].Success)
                    {
                        var index = int.Parse(match.Groups["index"].Value, CultureInfo.InvariantCulture);
                        if (match.Groups["end"].Success)
                        {
                            index = array.Count + 1 - index;
                            // In this case, we'll be changing the index value, so we'll need 
                            // to replace the query and matches for subsequent segments.
                            Query = Query[..match.Index] + "[" + index + "]" + Query[(match.Index + match.Length)..];
                            matches = JPathExpr.Matches(Query).Cast<Match>().ToList();
                            match = matches[midx];
                        }

                        if (index >= 0 && index <= array.Count)
                        {
                            // We can simply add the new object at the given index.
                            token = new JObject();
                            array.Insert(index, token);
                            owned.Add(Query[..(match.Index + match.Length)]);
                        }
                        else
                        {
                            return Log.Warn("JPO09", $"JSONPath index {index} out of range.");
                        }
                    }
                    else
                    {
                        return Log.Warn("JPO10", $"JSONPath array index not specified.");
                    }
                }
                else
                {
                    // If parent is not an object, we can't set a property on it.
                    if (parent is not JObject obj)
                        return Log.Warn("JPO11", $"JSONPath segment '{Query[..match.Index]}' didn't match a JSON object.");

                    token = new JProperty(match.Groups["name"].Value, new JObject());
                    owned.Add(Query[..(match.Index + match.Length)]);
                    obj.Add(token);
                }

                if (token == null)
                    return false;
            }
        }

        nodes = json.SelectTokens(Query).ToList();

        void AddResult(JToken node, int index)
        {
            if (nodes.Count == 1)
            {
                result.Add(new TaskItem(Query, new Dictionary<string, string>
                {
                    { "Value", node.AsString() }
                }));
            }
            else
            {
                result.Add(new TaskItem(Query + "[" + index + "]", new Dictionary<string, string>
                {
                    { "Value", node.AsString() }
                }));
            }
        }

        for (var i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];

            if (Value.Length > 1 || Properties.Length != 0 || RawValue != null)
            {
                // We'll be doing complex object replacement, 
                // so just replace the whole thing in one shot, 
                // no smarts for target-type selection.
                node.Replace(jvalue.Value);
                AddResult(jvalue.Value, i);
                continue;
            }

            // If the Value.Length == 1 OR Properties.Length == 0 (meaning 
            // we're not entering above condition for either arrays or complex 
            // objects), it's a single value, which we can target-type to the
            // native node type being replaced. This allows us to preserve the
            // native JSON type whenever possible.

            var value = node.Type switch
            {
                JTokenType.String => new JValue(Value[0].ItemSpec),
                JTokenType.Array => new JArray(jvalue.Value),
                JTokenType.Integer when long.TryParse(Value[0].ItemSpec, out var typed) => new JValue(typed),
                JTokenType.Float when decimal.TryParse(Value[0].ItemSpec, out var typed) => new JValue(typed),
                JTokenType.Boolean when bool.TryParse(Value[0].ItemSpec, out var typed) => new JValue(typed),
                JTokenType.Date when DateTime.TryParseExact(Value[0].ItemSpec, "O", CultureInfo.CurrentCulture, DateTimeStyles.RoundtripKind, out var typed) => new JValue(typed),
                JTokenType.Date when DateTime.TryParse(Value[0].ItemSpec, out var typed) => new JValue(typed),
                JTokenType.Guid when Guid.TryParse(Value[0].ItemSpec, out var typed) => new JValue(typed),
                JTokenType.Uri when Uri.TryCreate(Value[0].ItemSpec, UriKind.RelativeOrAbsolute, out var typed) => new JValue(typed),
                JTokenType.TimeSpan when TimeSpan.TryParse(Value[0].ItemSpec, out var typed) => new JValue(typed),
                _ => jvalue.Value,
            };

            node.Replace(value);
            AddResult(value, i);
        }

        Content = json.ToString(Formatting.Indented);
        if (ContentPath != null)
            File.WriteAllText(ContentPath.GetMetadata("FullPath"), Content);

        Result = result.ToArray();

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
