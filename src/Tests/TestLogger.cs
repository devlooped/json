using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

class TestLogger : Logger
{
    public HashSet<string> Warnings { get; } = new();
    public HashSet<string> Errors { get; } = new();
    public List<BuildEventArgs> Events { get; } = new();

    public override void Initialize(IEventSource eventSource)
    {
        eventSource.AnyEventRaised += (_, e) => Events.Add(e);
        eventSource.ErrorRaised += (_, e) => Errors.Add(e.Code);
        eventSource.WarningRaised += (_, e) => Warnings.Add(e.Code);
    }
}