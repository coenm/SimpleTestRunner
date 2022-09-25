namespace Serialization
{
    using System.Globalization;
    using System.Text.Json;
    using AutoMapper;
    using AutoMapper.Configuration.Annotations;
    using CoenM.Encoding;
    using Interface;
    using Interface.Data.Logger;
    using Newtonsoft.Json;
    using JsonSerializer = System.Text.Json.JsonSerializer;

    public class Serialization
    {
        private readonly IMapper _mapper;
        private static readonly List<Type> _types = typeof(EventArgsBaseDto).Assembly
                                                                            .GetTypes()
                                                                            .Where(t => t.IsSubclassOf(typeof(EventArgsBaseDto)))
                                                                            .ToList();
        public Serialization()
        {
            var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(InterfaceProject).Assembly));
            _mapper = config.CreateMapper();
        }

        public string Serialize(EventArgsBaseDto dto)
        {
            // var dto = _mapper.Map(evt, evt.GetType(), typeof(EventArgsBaseDto)) as EventArgsBaseDto;
            // dto!.SessionId = _session;
            var @type = dto.GetType().Name;

            var bytes = JsonSerializer.SerializeToUtf8Bytes(dto, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    AllowTrailingCommas = false,
                });
            
            var s = Z85Extended.Encode(bytes);

            return $"#@coenm:{@type.Length}:{@type}:{s.Length}:{s}";
        }

        public EventArgsBaseDto? Deserialize(string evt)
        {
            var prefix = "#@coen:";
            if (!evt.StartsWith(prefix))
            {
                return null;
            }

            var x1 = evt[prefix.Length..];

            var index = x1.IndexOf(':');
            var lenString = x1.Substring(0, index);
            if (!int.TryParse(lenString, NumberStyles.None, CultureInfo.CurrentCulture, out int len))
            {
                return null;
            }

            x1 = x1[index..];
            return null;
            // Type @type = _types.Single(x => x.Name.Equals(msgType));
            // evt = JsonConvert.DeserializeObject(payload, @type) as EventArgsBaseDto;
            //
            //
            // var dto = _mapper.Map(evt, evt.GetType(), typeof(EventArgsBaseDto)) as EventArgsBaseDto;
            // // dto!.SessionId = _session;
            // var @type = dto.GetType().Name;
            //
            // var bytes = JsonSerializer.SerializeToUtf8Bytes(dto, new JsonSerializerOptions
            //     {
            //         WriteIndented = false,
            //         AllowTrailingCommas = true,
            //     });
            //
            // var s = Z85Extended.Encode(bytes);
            //
            // return $"#@coenm:{@type.Length}:{@type}:{s.Length}:{s}";
        }

    }
}