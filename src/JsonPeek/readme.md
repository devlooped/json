[![Version](https://img.shields.io/nuget/vpre/JsonPeek.svg?color=royalblue)](https://www.nuget.org/packages/JsonPeek)
[![Downloads](https://img.shields.io/nuget/dt/JsonPeek.svg?color=green)](https://www.nuget.org/packages/JsonPeek)
[![License](https://img.shields.io/github/license/devlooped/json.svg?color=blue)](https://github.com/devlooped/json/blob/main/license.txt)
[![Build](https://github.com/devlooped/json/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/json/actions)

Usage:

```xml
  <JsonPeek ContentPath="[JSON_FILE]" Query="[JSONPath]">
    <Output TaskParameter="Result" PropertyName="Value" />
  </JsonPeek>
  <JsonPeek Content="[JSON]" Query="[JSONPath]">
    <Output TaskParameter="Result" ItemName="Values" />
  </JsonPeek>
```


Parameters:

| Parameter   | Description                                                                                                    |
| ----------- | -------------------------------------------------------------------------------------------------------------- |
| Content     | Optional `string` parameter. Input/Output.<br/>Specifies the JSON input as a string.                           |
| ContentPath | Optional `ITaskItem` parameter.<br/>Specifies the JSON input as a file path.                                   |
| Query       | Required `string` parameter.<br/>Specifies the [JSONPath](https://goessner.net/articles/JsonPath/) expression. |
| Result      | Output `ITaskItem[]` parameter.<br/>Contains the results that are returned by the task.                        |

You can either provide the path to a JSON file via `ContentPath` or 
provide the straight JSON content to `Content`. The `Query` is a 
[JSONPath](https://goessner.net/articles/JsonPath/) expression that is evaluated 
and returned via the `Result` task parameter. You can assign the resulting 
value to either a property (i.e. for a single value) or an item name (i.e. 
for multiple results).

JSON object properties are automatically projected as item metadata when 
assigning the resulting value to an item. For example, given the following JSON:

```JSON
{
    "http": {
        "host": "localhost",
        "port": 80,
        "ssl": true
    }
}
```

You can read the entire `http` value as an item with each property as a metadata 
value with:

```xml
<JsonPeek ContentPath="host.json" Query="$.http">
    <Output TaskParameter="Result" ItemName="Http" />
</JsonPeek>
```

The `Http` item will have the following values (if it were declared in MSBuild):

```xml
<ItemGroup>
    <Http Include="[item raw json]">
        <host>localhost</host>
        <port>80</port>
        <ssl>true</ssl>
    </Http>
</ItemGroup>
```

These item metadata values could be read as MSBuild properties as follows, for example:

```xml
<PropertyGroup>
    <Host>@(Http -> '%(host)')</Host>
    <Port>@(Http -> '%(port)')</Port>
    <Ssl>@(Http -> '%(ssl)')</Ssl>
</PropertyGroup>
```


## Sponsors

[![sponsored](https://raw.githubusercontent.com/devlooped/oss/main/assets/images/sponsors.svg)](https://github.com/sponsors/devlooped) [![clarius](https://raw.githubusercontent.com/clarius/branding/main/logo/byclarius.svg)](https://github.com/clarius)[![clarius](https://raw.githubusercontent.com/clarius/branding/main/logo/logo.svg)](https://github.com/clarius)

*[get mentioned here too](https://github.com/sponsors/devlooped)!*
