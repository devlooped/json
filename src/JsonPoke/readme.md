[![Version](https://img.shields.io/nuget/vpre/JsonPoke.svg?color=royalblue)](https://www.nuget.org/packages/JsonPoke)
[![Downloads](https://img.shields.io/nuget/dt/JsonPoke.svg?color=green)](https://www.nuget.org/packages/JsonPoke)
[![License](https://img.shields.io/github/license/devlooped/json.svg?color=blue)](https://github.com/devlooped/json/blob/main/license.txt)

Usage:

```xml
<JsonPoke ContentPath="[JSON_FILE]" Query="[JSONPath]" Value="[VALUE]" />
<JsonPoke ContentPath="[JSON_FILE]" Query="[JSONPath]" RawValue="[JSON]" />
<JsonPoke Content="[JSON]" Query="[JSONPath]" Value="[VALUE]" />
```

You must either provide the path to a JSON file via `ContentPath` or 
raw JSON content via `Content`.

The `Value` can be an item group, and in that case, it will be inserted into the 
JSON node matching the [JSONPath](https://goessner.net/articles/JsonPath/) expression 
`Query` as an array. `RawValue` can be used to provide 
an entire JSON fragment as a string, with no conversion to an MSBuild item at all.

The existing JSON node will determine the data type of the value being written, 
so as to preserve the original document. Numbers, booleans and DateTimes are 
properly parsed before serializing to the node. 

```xml
<PropertyGroup>
    <Json>
{
  "http": {
    "host": "localhost",
    "port": 80,
    "ssl": true
  }
}
    </Json>
</PropertyGroup>

<JsonPoke Content="$(Json)" Query="$.http.host" Value="example.com">
  <Output TaskParameter="Content" PropertyName="Json" />
</JsonPoke>

<JsonPoke Content="$(Json)" Query="$.http.port" Value="80">
  <Output TaskParameter="Content" PropertyName="Json" />
</JsonPoke>

<JsonPoke Content="$(Json)" Query="$.http.ssl" Value="true">
  <Output TaskParameter="Content" PropertyName="Json" />
</JsonPoke>

<Message Importance="high" Text="$(Json)" />
```

Note how we update multiple values and assign the updated content to the 
same `$(Json)` property so it can be used in subsequent updates. The last 
`Message` task will render the following JSON:

```JSON
{
  "http": {
    "host": "example.com",
    "port": 80,
    "ssl": true
  }
}
```

> NOTE: The `port` value is preserved as a number, as is the `ssl` boolean.

To force a value to be interpreted as a string, you can surround it with double or single quotes.
For example, given the following JSON file:

```JSON
{
    "http": {
        "ports": [
            "80"
        ]
    }
}
```

We can replace the `ports` array with string values as follows (without the 
explicit quotes, the values would be interpreted as numbers otherwise):

```xml
<ItemGroup>
  <HttpPort Include="'8080'" />
  <HttpPort Include="'1080'" />
</ItemGroup>

<JsonPoke ContentPath="http.json" Query="$.http.ports" Value="@(HttpPort)" />
```

Result:

```JSON
{
  "http": {
    "ports": [
      "8080", 
      "1080"
    ]
  }
}
```

It's also possible to write a complex object based on MSBuild item metadata: 

```xml
<ItemGroup>
  <Http Include="Value">
    <host>localhost</host>
    <port>80</port>
    <ssl>true</ssl>
  </Value>
</ItemGroup>

<JsonPoke ContentPath="http.json" Query="$.http" Value="@(Http)" Properties="host;port;ssl" />
```

Result:

```JSON
{
  "http": {
    "host": "localhost",
    "port": 80,
    "ssl": true
  }
}
```

Note how the native JSON type was automatically inferred, even though everything is 
basically a string in MSBuild. As noted above, you can surround any of the item metadata 
values in double or single quotes to force them to be written as strings instead.

The modified JSON nodes can be assigned to an item name using the `Result` task property, 
and will contain the item path (matching the `Query` plus the index if multiple nodes were modified) 
as well as the `Value` item metadata containing the raw JSON that was written.

## Parameters

| Parameter   | Description                                                                                                                                            |
| ----------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Content     | Optional `string` input/output parameter.<br/>Specifies the JSON input as a string and contains the updated <br/>JSON after successful task execution. |
| ContentPath | Optional `ITaskItem` parameter.<br/>Specifies the JSON input as a file path.                                                                           |
| Query       | Required `string` parameter.<br/>Specifies the [JSONPath](https://goessner.net/articles/JsonPath/) expression.                                         |
| Value       | Optional `ITaskItem[]` parameter.<br/>Specifies the value(s) to be inserted into the specified path.                                                   |
| RawValue    | Optional `string` parameter.<br/>Specifies the raw (JSON) value to be inserted into the specified path.                                                |

<!-- include ../../docs/footer.md -->
# Sponsors 

<!-- sponsors.md -->
[![Kirill Osenkov](https://github.com/devlooped/sponsors/raw/main/.github/avatars/KirillOsenkov).svg "Kirill Osenkov")](https://github.com/KirillOsenkov)
[![C. Augusto Proiete](https://github.com/devlooped/sponsors/raw/main/.github/avatars/augustoproiete).svg "C. Augusto Proiete")](https://github.com/augustoproiete)
[![SandRock](https://github.com/devlooped/sponsors/raw/main/.github/avatars/sandrock).svg "SandRock")](https://github.com/sandrock)
[![Amazon Web Services](https://github.com/devlooped/sponsors/raw/main/.github/avatars/aws).svg "Amazon Web Services")](https://github.com/aws)
[![Christian Findlay](https://github.com/devlooped/sponsors/raw/main/.github/avatars/MelbourneDeveloper).svg "Christian Findlay")](https://github.com/MelbourneDeveloper)
[![Clarius Org](https://github.com/devlooped/sponsors/raw/main/.github/avatars/clarius).svg "Clarius Org")](https://github.com/clarius)
[![MFB Technologies, Inc.](https://github.com/devlooped/sponsors/raw/main/.github/avatars/MFB-Technologies-Inc).svg "MFB Technologies, Inc.")](https://github.com/MFB-Technologies-Inc)


<!-- sponsors.md -->

<br>&nbsp;
<a href="https://github.com/sponsors/devlooped" title="Sponsor this project">
  <img src="https://github.com/devlooped/sponsors/blob/main/sponsor.png" />
</a>
<br>

[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- ../../docs/footer.md -->
