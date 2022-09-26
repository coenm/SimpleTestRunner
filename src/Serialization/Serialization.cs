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
        private const string PREFIX = "#@testrunner";
        private readonly IMapper _mapper;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        private static readonly List<Type> _types = typeof(EventArgsBaseDto).Assembly
                                                                            .GetTypes()
                                                                            .Where(t => t.IsSubclassOf(typeof(EventArgsBaseDto)))
                                                                            .ToList();

        public Serialization()
        {
            var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(InterfaceProject).Assembly));
            _mapper = config.CreateMapper();
            _jsonSerializerOptions = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    AllowTrailingCommas = false,
                };
        }

        public string Serialize(EventArgsBaseDto dto)
        {
            // var dto = _mapper.Map(evt, evt.GetType(), typeof(EventArgsBaseDto)) as EventArgsBaseDto;
            // dto!.SessionId = _session;
            var @type = dto.GetType().Name;

            var bytes = JsonSerializer.SerializeToUtf8Bytes(dto, dto.GetType(), _jsonSerializerOptions);
            
            var z85Encoded = Z85Extended.Encode(bytes);

            return $"{PREFIX}:{@type.Length}:{@type}:{z85Encoded.Length}:{z85Encoded}";
        }

        public EventArgsBaseDto? Deserialize(ReadOnlySpan<char> line)
        {
            if (!IsTestRunnerLine(line))
            {
                return null;
            }

            ReadOnlySpan<char> unprocessed = line[(PREFIX.Length+1)..];

            int index = unprocessed.IndexOf(':');
            ReadOnlySpan<char> lenString = unprocessed[..index];
            if (!int.TryParse(lenString, NumberStyles.None, CultureInfo.CurrentCulture, out int len))
            {
                return null;
            }

            if (len <= 0)
            {
                return null;
            }

            unprocessed = unprocessed[(index + 1)..];
            var msgType = unprocessed[..len].ToString();
            unprocessed = unprocessed[(len + 1)..];

            index = unprocessed.IndexOf(':');
            lenString = unprocessed[..index];
            if (!int.TryParse(lenString, NumberStyles.None, CultureInfo.CurrentCulture, out len))
            {
                return null;
            }

            if (len <= 0)
            {
                return null;
            }

            unprocessed = unprocessed[(index + 1)..];
            ReadOnlySpan<char> payload = unprocessed[..len]; // should be all

            Type @type = _types.Single(x => x.Name.Equals(msgType));

            var bytes = Z85Extended.Decode(payload.ToString());
            var obj = System.Text.Json.JsonSerializer.Deserialize(bytes, @type);
            return obj as EventArgsBaseDto;
        }

        public bool IsTestRunnerLine(ReadOnlySpan<char> line)
        {
            if (line.Length <= PREFIX.Length)
            {
                return false;
            }

            return line[..PREFIX.Length].Equals(PREFIX, StringComparison.InvariantCulture);
        }
    }
}