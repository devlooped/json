![Icon](https://raw.githubusercontent.com/devlooped/json/main/assets/img/json.png) JsonPeek and JsonPoke MSBuild Tasks
============

[![License](https://img.shields.io/github/license/devlooped/json.svg?color=blue)](https://github.com/devlooped/json/blob/main/license.txt)
[![Build](https://github.com/devlooped/json/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/json/actions)

# JsonPeek

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

You can either provide the path to a JSON file via `ContentPath` or 
provide the straight JSON content to `Content`. The `Query` is a 
[JSONPath](https://goessner.net/articles/JsonPath/) expression that is evaluated 
and returned via the `Result` task parameter. You can assign the resulting 
value to either a property (i.e. for a single value) or an item name (i.e. 
for multiple results).

JSON object properties are automatically projected as item metadata when 
assigning the resulting value to an item. For example, given the following JSON:

```json
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

# JsonPoke

Write values to JSON nodes selected with JSONPath

[![Version](https://img.shields.io/nuget/vpre/JsonPoke.svg?color=royalblue)](https://www.nuget.org/packages/JsonPoke)
[![Downloads](https://img.shields.io/nuget/dt/JsonPoke.svg?color=green)](https://www.nuget.org/packages/JsonPoke)

Usage:

```xml
  <JsonPoke ContentPath="[JSON_FILE]" Query="[JSONPath]" Value="[VALUE]" />
  <JsonPoke ContentPath="[JSON_FILE]" Query="[JSONPath]" RawValue="[JSON]" />
```

The `Value` can be an item group, and in that case, it will be inserted into the 
JSON node matching the [JSONPath](https://goessner.net/articles/JsonPath/) expression 
`Query` as an array. RawValue can be used to provide 
an entire JSON fragment as a string, with no conversion to an MSBuild item at all.

The existing JSON node will determine the data type of the value being written, 
so as to preserve the original document. Numbers, booleans and DateTimes are 
properly parsed before serializing to the node. To force a value to be interpreted 
as a string, you can surround it with double or single quotes.

For example, given the following JSON file:

```json
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

```json
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

```json
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


# Dogfooding

[![CI Version](https://img.shields.io/endpoint?url=https://shields.kzu.io/vpre/JsonPeek/main&label=nuget.ci&color=brightgreen)](https://pkg.kzu.io/index.json)
[![Build](https://github.com/devlooped/json/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/json/actions)

We also produce CI packages from branches and pull requests so you can dogfood builds as quickly as they are produced. 

The CI feed is `https://pkg.kzu.io/index.json`. 

The versioning scheme for packages is:

- PR builds: *42.42.42-pr*`[NUMBER]`
- Branch builds: *42.42.42-*`[BRANCH]`.`[COMMITS]`



## Sponsors

[![sponsored](https://raw.githubusercontent.com/devlooped/oss/main/assets/images/sponsors.svg)](https://github.com/sponsors/devlooped) [![clarius](https://raw.githubusercontent.com/clarius/branding/main/logo/byclarius.svg)](https://github.com/clarius)[![clarius](https://raw.githubusercontent.com/clarius/branding/main/logo/logo.svg)](https://github.com/clarius)

*[get mentioned here too](https://github.com/sponsors/devlooped)!*
