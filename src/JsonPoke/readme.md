[![Version](https://img.shields.io/nuget/vpre/JsonPoke.svg?color=royalblue)](https://www.nuget.org/packages/JsonPoke)
[![Downloads](https://img.shields.io/nuget/dt/JsonPoke.svg?color=green)](https://www.nuget.org/packages/JsonPoke)
[![License](https://img.shields.io/github/license/devlooped/json.svg?color=blue)](https://github.com/devlooped/json/blob/main/license.txt)
[![Build](https://github.com/devlooped/json/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/json/actions)

Usage:

```xml
  <JsonPoke JsonInputPath="[JSON_FILE]" Query="[JSONPath]" Value="[VALUE]" />
  <JsonPoke JsonInputPath="[JSON_FILE]" Query="[JSONPath]" RawValue="[JSON]" />
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

  <JsonPoke JsonInputPath="http.json" Query="$.http.ports" Value="@(HttpPort)" />
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

   <JsonPoke JsonInputPath="http.json" Query="$.http" Value="@(Http)" Properties="host;port;ssl" />
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


## Sponsors

[![sponsored](https://raw.githubusercontent.com/devlooped/oss/main/assets/images/sponsors.svg)](https://github.com/sponsors/devlooped) [![clarius](https://raw.githubusercontent.com/clarius/branding/main/logo/byclarius.svg)](https://github.com/clarius)[![clarius](https://raw.githubusercontent.com/clarius/branding/main/logo/logo.svg)](https://github.com/clarius)

*[get mentioned here too](https://github.com/sponsors/devlooped)!*
