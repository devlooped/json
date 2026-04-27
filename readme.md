![JSON Icon](assets/img/json.png) JsonPeek and JsonPoke MSBuild Tasks
============

[![License](https://img.shields.io/github/license/devlooped/json.svg?color=blue)](https://github.com/devlooped/json/blob/main/license.txt)
[![Build](https://github.com/devlooped/json/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/devlooped/json/actions)

![JsonPeek Icon](assets/img/jsonpeek.png) JsonPeek
============

[![Version](https://img.shields.io/nuget/vpre/JsonPeek.svg?color=royalblue)](https://www.nuget.org/packages/JsonPeek)
[![Downloads](https://img.shields.io/nuget/dt/JsonPeek.svg?color=green)](https://www.nuget.org/packages/JsonPeek)

<!-- #JsonPeek -->
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

<!-- #JsonPeek -->

![JsonPoke Icon](assets/img/jsonpoke.png) JsonPoke
============

[![Version](https://img.shields.io/nuget/vpre/JsonPoke.svg?color=royalblue)](https://www.nuget.org/packages/JsonPoke)
[![Downloads](https://img.shields.io/nuget/dt/JsonPoke.svg?color=green)](https://www.nuget.org/packages/JsonPoke)

<!-- #JsonPoke -->
Write values to JSON nodes selected with JSONPath

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

<!-- #JsonPoke -->

# Dogfooding

[![CI Version](https://img.shields.io/endpoint?url=https://shields.kzu.app/vpre/JsonPeek/main&label=nuget.ci&color=brightgreen)](https://pkg.kzu.app/index.json)
[![Build](https://github.com/devlooped/json/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/devlooped/json/actions)

We also produce CI packages from branches and pull requests so you can dogfood builds as quickly as they are produced. 

The CI feed is `https://pkg.kzu.app/index.json`. 

The versioning scheme for packages is:

- PR builds: *42.42.42-pr*`[NUMBER]`
- Branch builds: *42.42.42-*`[BRANCH]`.`[COMMITS]`

<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->
# Sponsors 

<!-- sponsors.md -->
[![Clarius Org](https://avatars.githubusercontent.com/u/71888636?v=4&s=39 "Clarius Org")](https://github.com/clarius)
[![MFB Technologies, Inc.](https://avatars.githubusercontent.com/u/87181630?v=4&s=39 "MFB Technologies, Inc.")](https://github.com/MFB-Technologies-Inc)
[![Khamza Davletov](https://avatars.githubusercontent.com/u/13615108?u=11b0038e255cdf9d1940fbb9ae9d1d57115697ab&v=4&s=39 "Khamza Davletov")](https://github.com/khamza85)
[![SandRock](https://avatars.githubusercontent.com/u/321868?u=99e50a714276c43ae820632f1da88cb71632ec97&v=4&s=39 "SandRock")](https://github.com/sandrock)
[![DRIVE.NET, Inc.](https://avatars.githubusercontent.com/u/15047123?v=4&s=39 "DRIVE.NET, Inc.")](https://github.com/drivenet)
[![Keith Pickford](https://avatars.githubusercontent.com/u/16598898?u=64416b80caf7092a885f60bb31612270bffc9598&v=4&s=39 "Keith Pickford")](https://github.com/Keflon)
[![Thomas Bolon](https://avatars.githubusercontent.com/u/127185?u=7f50babfc888675e37feb80851a4e9708f573386&v=4&s=39 "Thomas Bolon")](https://github.com/tbolon)
[![Kori Francis](https://avatars.githubusercontent.com/u/67574?u=3991fb983e1c399edf39aebc00a9f9cd425703bd&v=4&s=39 "Kori Francis")](https://github.com/kfrancis)
[![Reuben Swartz](https://avatars.githubusercontent.com/u/724704?u=2076fe336f9f6ad678009f1595cbea434b0c5a41&v=4&s=39 "Reuben Swartz")](https://github.com/rbnswartz)
[![Jacob Foshee](https://avatars.githubusercontent.com/u/480334?v=4&s=39 "Jacob Foshee")](https://github.com/jfoshee)
[![](https://avatars.githubusercontent.com/u/33566379?u=bf62e2b46435a267fa246a64537870fd2449410f&v=4&s=39 "")](https://github.com/Mrxx99)
[![Eric Johnson](https://avatars.githubusercontent.com/u/26369281?u=41b560c2bc493149b32d384b960e0948c78767ab&v=4&s=39 "Eric Johnson")](https://github.com/eajhnsn1)
[![Jonathan ](https://avatars.githubusercontent.com/u/5510103?u=98dcfbef3f32de629d30f1f418a095bf09e14891&v=4&s=39 "Jonathan ")](https://github.com/Jonathan-Hickey)
[![Ken Bonny](https://avatars.githubusercontent.com/u/6417376?u=569af445b6f387917029ffb5129e9cf9f6f68421&v=4&s=39 "Ken Bonny")](https://github.com/KenBonny)
[![Simon Cropp](https://avatars.githubusercontent.com/u/122666?v=4&s=39 "Simon Cropp")](https://github.com/SimonCropp)
[![agileworks-eu](https://avatars.githubusercontent.com/u/5989304?v=4&s=39 "agileworks-eu")](https://github.com/agileworks-eu)
[![Zheyu Shen](https://avatars.githubusercontent.com/u/4067473?v=4&s=39 "Zheyu Shen")](https://github.com/arsdragonfly)
[![Vezel](https://avatars.githubusercontent.com/u/87844133?v=4&s=39 "Vezel")](https://github.com/vezel-dev)
[![ChilliCream](https://avatars.githubusercontent.com/u/16239022?v=4&s=39 "ChilliCream")](https://github.com/ChilliCream)
[![4OTC](https://avatars.githubusercontent.com/u/68428092?v=4&s=39 "4OTC")](https://github.com/4OTC)
[![domischell](https://avatars.githubusercontent.com/u/66068846?u=0a5c5e2e7d90f15ea657bc660f175605935c5bea&v=4&s=39 "domischell")](https://github.com/DominicSchell)
[![Adrian Alonso](https://avatars.githubusercontent.com/u/2027083?u=129cf516d99f5cb2fd0f4a0787a069f3446b7522&v=4&s=39 "Adrian Alonso")](https://github.com/adalon)
[![torutek](https://avatars.githubusercontent.com/u/33917059?v=4&s=39 "torutek")](https://github.com/torutek)
[![mccaffers](https://avatars.githubusercontent.com/u/16667079?u=110034edf51097a5ee82cb6a94ae5483568e3469&v=4&s=39 "mccaffers")](https://github.com/mccaffers)
[![Seika Logiciel](https://avatars.githubusercontent.com/u/2564602?v=4&s=39 "Seika Logiciel")](https://github.com/SeikaLogiciel)
[![Andrew Grant](https://avatars.githubusercontent.com/devlooped-user?s=39 "Andrew Grant")](https://github.com/wizardness)
[![eska-gmbh](https://avatars.githubusercontent.com/devlooped-team?s=39 "eska-gmbh")](https://github.com/eska-gmbh)


<!-- sponsors.md -->
[![Sponsor this project](https://avatars.githubusercontent.com/devlooped-sponsor?s=118 "Sponsor this project")](https://github.com/sponsors/devlooped)

[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- https://github.com/devlooped/sponsors/raw/main/footer.md -->
