namespace Interface.Server;

using System;
using Interface.Data.Logger;

public interface ITestMonitor
{
    IObservable<EventArgsBaseDto> Events { get; }
}