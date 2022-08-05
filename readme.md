![JSON Icon](assets/img/json.png) JsonPeek and JsonPoke MSBuild Tasks
============

[![License](https://img.shields.io/github/license/devlooped/json.svg?color=blue)](https://github.com/devlooped/json/blob/main/license.txt)
[![Build](https://github.com/devlooped/json/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/json/actions)

![JsonPeek Icon](assets/img/jsonpeek.png) JsonPeek
============

[![Version](https://img.shields.io/nuget/vpre/JsonPeek.svg?color=royalblue)](https://www.nuget.org/packages/JsonPeek)
[![Downloads](https://img.shields.io/nuget/dt/JsonPeek.svg?color=green)](https://www.nuget.org/packages/JsonPeek)

Read values from JSON using JSONPath.

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
| Content     | Optional `string` parameter.<br/>Specifies the JSON input as a string.                                         |
| ContentPath | Optional `ITaskItem` parameter.<br/>Specifies the JSON input as a file path.                                   |
| Empty       | Optional `string` parameter.<br/>Value to use as a replacement for empty values matched in JSON.               |
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

In addition to the explicitly opted in object properties, the entire node is available 
as raw JSON via the special `_` (single underscore) metadata item.

If the matched value is empty, no items (because items cannot be constructed with empty 
identity) or property value will be returned. This makes it difficult to distinguish a 
successfully matched empty value from no value matched at all. For these cases, it's 
possible to specify an `Empty` value to stand-in for an empty (but successful) matched 
result instead, which allow to distinguish both scenarios:

```xml
<JsonPeek Content="$(Json)" Empty="$empty" Query="$(Query)">
  <Output TaskParameter="Result" PropertyName="Value" />
</JsonPeek>

<Error Condition="'$(Value)' == '$empty'" Text="The element $(Query) cannot have an empty value." />
```


![JsonPoke Icon](assets/img/jsonpoke.png) JsonPoke
============

Write values to JSON nodes selected with JSONPath

[![Version](https://img.shields.io/nuget/vpre/JsonPoke.svg?color=royalblue)](https://www.nuget.org/packages/JsonPoke)
[![Downloads](https://img.shields.io/nuget/dt/JsonPoke.svg?color=green)](https://www.nuget.org/packages/JsonPoke)

Usage:

```xml
  <JsonPoke ContentPath="[JSON_FILE]" Query="[JSONPath]" Value="[VALUE]" />
  <JsonPoke ContentPath="[JSON_FILE]" Query="[JSONPath]" RawValue="[JSON]" />
  <JsonPoke Content="[JSON]" Query="[JSONPath]" Value="[VALUE]" />
```

Parameters:

| Parameter   | Description                                                                                                                                           |
| ----------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| Content     | Optional `string` input/output parameter.<br/>Specifies the JSON input as a string and contains the updated<br/>JSON after successful task execution. |
| ContentPath | Optional `ITaskItem` parameter.<br/>Specifies the JSON input as a file path.                                                                          |
| Query       | Required `string` parameter.<br/>Specifies the [JSONPath](https://goessner.net/articles/JsonPath/) expression.                                        |
| Value       | Optional `ITaskItem[]` parameter.<br/>Specifies the value(s) to be inserted into the specified path.                                                  |
| RawValue    | Optional `string` parameter.<br/>Specifies the raw (JSON) value to be inserted into the specified path.                                               |

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

> NOTE: The port number was preserved as a number, as is the `ssl` boolean.

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

The task can create entire object hierarchies if any segment of the path expression is 
not found, which makes it very easy to create complex structures by assigning a single 
value. For example, if the `http` section in the examples above didn't exist at all, 
the following task would add it automatically, prior to assigning the `ssl` property to `true`:

```xml
<JsonPoke ContentPath="http.json" Query="$.http.ssl" Value="true" />
```

This also works for indexed queries, such as adding launch profile to 
[launchSettings.json](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-6.0#lsj) 
by simply assigning a value:

```xml
<JsonPoke ContentPath="Properties\launchSettings.json" Query="$.profiles['IIS Express'].commandName" Value="IISExpress" />
```

which would create the following entry:

```json
{
  "profiles": {
    "IIS Express": {
      "commandName": "IISExpress",
    }
  }
}
```

Array index is also supported as part of the query, to modify existing values. If the array is empty 
or non-existent, it's also possible to just use the index `[0]` to denote the new node should be the 
sole element in the new array, like for adding a new watch file value to 
[host.json](https://docs.microsoft.com/en-us/azure/azure-functions/functions-host-json):

```xml
<JsonPoke ContentPath="host.json" Query="$.watchFiles[0]" Value="myFile.txt" />
```

Which results in:

```json
{
  ...
  "watchFiles": [ "myFile.txt" ]
}
```

It's quite common to want to add entries to an existing array, usually at the end of the array. The 
JSONPath syntax supports indexes that start from the end of the array (such as `[-1:]`), but if the 
array had any values already, that would match whichever is the last element, meaning in an *update* 
to that element's value. Since we need a different syntax for *inserting* a new node, starting from 
the end of the list, we leverage the C# syntax `^n` where `n` is the position starting from the end. 
To add a new element at the end of the list, the index `[^1]` can be used. `^2` means prior to last
and so on.

For example, to *add* a new watched file to the array in the example above, we could use:

```xml
<JsonPoke ContentPath="host.json" Query="$.watchFiles[^1]" Value="myOtherFile.txt" />
```

Given an existing `host.json` file like the one above, we would get a new file added like so:

```json
{
  ...
  "watchFiles": [ "myFile.txt", "myOtherFile.txt" ]
}
```

If the `watchFiles` property didn't exit at all or had no elements, the result would be 
the same as if we used `[0]`, but this makes the code more flexible if needed.


The modified JSON nodes can be assigned to an item name using the `Result` task property, 
and will contain the item path (matching the `Query` plus the index if multiple nodes were modified) 
as well as the `Value` item metadata containing the raw JSON that was written.

# Dogfooding

[![CI Version](https://img.shields.io/endpoint?url=https://shields.kzu.io/vpre/JsonPeek/main&label=nuget.ci&color=brightgreen)](https://pkg.kzu.io/index.json)
[![Build](https://github.com/devlooped/json/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/json/actions)

We also produce CI packages from branches and pull requests so you can dogfood builds as quickly as they are produced. 

The CI feed is `https://pkg.kzu.io/index.json`. 

The versioning scheme for packages is:

- PR builds: *42.42.42-pr*`[NUMBER]`
- Branch builds: *42.42.42-*`[BRANCH]`.`[COMMITS]`

<!-- include docs/footer.md -->
# Sponsors 

<!-- sponsors.md -->
[![Kirill Osenkov](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/KirillOsenkov.png "Kirill Osenkov")](https://github.com/KirillOsenkov)
[![C. Augusto Proiete](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/augustoproiete.png "C. Augusto Proiete")](https://github.com/augustoproiete)
[![SandRock](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/sandrock.png "SandRock")](https://github.com/sandrock)
[![Amazon Web Services](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/aws.png "Amazon Web Services")](https://github.com/aws)
[![Christian Findlay](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/MelbourneDeveloper.png "Christian Findlay")](https://github.com/MelbourneDeveloper)
[![Clarius Org](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/clarius.png "Clarius Org")](https://github.com/clarius)
[![MFB Technologies, Inc.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/MFB-Technologies-Inc.png "MFB Technologies, Inc.")](https://github.com/MFB-Technologies-Inc)


<!-- sponsors.md -->

<br>&nbsp;
[![Sponsor this project](https://github.com/devlooped/sponsors/blob/main/sponsor.png "Sponsor this project")](https://github.com/sponsors/devlooped)
<br>&nbsp;
[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- docs/footer.md -->
