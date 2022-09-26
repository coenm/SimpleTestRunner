namespace NetMq.Publisher;

using System;
using AutoMapper;
using Interface;
using Interface.Data.Logger;
using Serialization;

public class Publisher : IDisposable
{
    private readonly IMapper _mapper;
    private readonly Serialization _s;

    public Publisher()
    {
        var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(InterfaceProject).Assembly));
        _mapper = config.CreateMapper();
        _s = new Serialization();
    }
    
    public void Send(EventArgs evt, string caller)
    {
        if (_mapper.Map(evt, evt.GetType(), typeof(EventArgsBaseDto)) is EventArgsBaseDto dto)
        {
            Console.WriteLine(_s.Serialize(dto));
        }
    }

    public void Dispose()
    {
    }
}