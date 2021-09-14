using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Xunit;
using Xunit.Abstractions;

public record Tests(ITestOutputHelper Output)
{
    static HashSet<string> Restored = new();

    [Theory]
    [MemberData(nameof(GetTargets))]
    public void Run(string file, string name, bool failure = false, string? code = null)
    {
        //if (!Restored.Contains(file))
        //{
        //    var log = new TestLogger();
        //    Assert.Equal(BuildResultCode.Success, BuildManager.DefaultBuildManager.Build(
        //        new BuildParameters
        //        {
        //            Loggers = new ILogger[] { log },
        //            ResetCaches = true,
        //        },
        //        new BuildRequestData(
        //            Path.Combine(Directory.GetCurrentDirectory(), file),
        //            new Dictionary<string, string>(), null, new[] { "Restore" }, null))
        //        .OverallResult);

        //    Restored.Add(file);
        //}

        var logger = new TestLogger();
        var result = BuildManager.DefaultBuildManager.Build(
            new BuildParameters
            {
                ResetCaches = true,
                Loggers = new ILogger[] { logger }
            },
            new BuildRequestData(
                Path.Combine(Directory.GetCurrentDirectory(), file), 
                new Dictionary<string, string>(), null, new[] { name }, null));

        if (failure)
        {
            if (result.OverallResult != BuildResultCode.Failure)
                Output.WriteLine(string.Join(Environment.NewLine, logger.Events
                    .Select(e => e.Message)));

            Assert.Equal(BuildResultCode.Failure, result.OverallResult);
            Assert.Contains(code, logger.Errors);
        }
        else
        {
            if (result.OverallResult != BuildResultCode.Success) 
                Output.WriteLine(string.Join(Environment.NewLine, logger.Events
                    .Select(e => e.Message)));

            Assert.Equal(BuildResultCode.Success, result.OverallResult);
            if (code != null)
                Assert.Contains(code, logger.Warnings);
        }
    }

    public static IEnumerable<object?[]> GetTargets()
    {
        foreach (var file in Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.targets"))
        {
            foreach (var target in XDocument.Load(file).Root!.Elements("Target"))
            {
                var label = target.Attribute("Label")?.Value;
                var name = target.Attribute("Name")!.Value;
                if (label == null)
                {
                    yield return new object?[] { Path.GetFileName(file), name, };
                    continue;
                }

                var parts = label.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    continue;

                yield return new object[] { Path.GetFileName(file), name, parts[0] == "Error", parts[1] };
            }
        }
    }
}