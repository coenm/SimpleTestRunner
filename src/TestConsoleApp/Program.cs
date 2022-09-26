namespace TestConsoleApp
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Interface;
    using Interface.Data.Collector;
    using Interface.Data.Collector.Inner;
    using Serialization;


    internal class Program
    {
        private static IMapper _mapper;

        static async Task<int> Main(string[] args)
        {
            var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(InterfaceProject).Assembly));
            _mapper = config.CreateMapper();


            foreach (var item in args)
            {
                Console.WriteLine($"args: {item}");
            }

            string arg = string.Empty;
            if (args.Length > 0)
            {
                arg = args[0];
            }
            
            Console.WriteLine("test --" + arg);
            await Task.Delay(2000);
            for (int i = 1; i < 100; i++)
                Console.WriteLine($"test {i}" + arg);
            await Task.Delay(10);

            var evt = new TestCaseStartEventArgsDto
                {
                    IsChildTestCase = true,
                    SessionId = "ss",
                    TestCaseId = Guid.Empty,
                    TestCaseName = "aaa",
                    TestElement = new TestCaseDto
                        {
                            CodeFilePath = "d",
                            DisplayName = "dd",
                        },
                };

            var s = new Serialization();
            var ss = s.Serialize(evt);
            Console.WriteLine(ss);
            var obj = s.Deserialize(ss) as TestCaseStartEventArgsDto;
            //
            //
            // var bytes = JsonSerializer.SerializeToUtf8Bytes(evt, new JsonSerializerOptions
            //     {
            //         WriteIndented = false,
            //         AllowTrailingCommas = true,
            //     });
            // var s = Z85Extended.Encode(bytes);
            //
            // Console.WriteLine($"##coenm:{s}");

            if (arg.Equals("throw", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("Thrown by app");
            }

            return 9;
        }
    }
}