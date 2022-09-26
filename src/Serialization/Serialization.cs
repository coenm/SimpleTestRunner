namespace Serialization
{
    using System.Globalization;
    using System.Text.Json;
    using AutoMapper;
    using CoenM.Encoding;
    using Interface;
    using Interface.Data.Logger;
    using JsonSerializer = System.Text.Json.JsonSerializer;

    public class Serialization
    {
        private const string PREFIX = "#@testrunner:";
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

        public string Serialize(object dto)
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

            return $"{PREFIX}:{@type.Length}:{@type}:{s.Length}:{s}";
        }

        public EventArgsBaseDto? Deserialize(string evt)
        {
            // todo
            // ReadOnlySpan<char> data = evt;

            if (!evt.StartsWith(PREFIX))
            {
                return null;
            }

            var x1 = evt[(PREFIX.Length+1)..];

            var index = x1.IndexOf(':');
            var lenString = x1[..index];
            if (!int.TryParse(lenString, NumberStyles.None, CultureInfo.CurrentCulture, out int len))
            {
                return null;
            }

            x1 = x1[(index+1)..];
            var msgType = x1[..len];
            x1 = x1[(len + 1)..];

            index = x1.IndexOf(':');
            lenString = x1.Substring(0, index);
            if (!int.TryParse(lenString, NumberStyles.None, CultureInfo.CurrentCulture, out len))
            {
                return null;
            }

            x1 = x1[(index + 1)..];
            var payload = x1[..len]; // should be all

            Type @type = _types.Single(x => x.Name.Equals(msgType));

            var bytes = Z85Extended.Decode(payload);
            var obj = System.Text.Json.JsonSerializer.Deserialize(bytes, @type);
            return obj as EventArgsBaseDto;
        }
    }
}