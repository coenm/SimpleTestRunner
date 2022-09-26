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
            var arg = string.Empty;
            if (args.Length > 0)
            {
                arg = args[0];
            }
            
            if (arg.Equals("throw", StringComparison.CurrentCultureIgnoreCase))
            {
                Console.WriteLine("just some text before exception.");
                throw new Exception("Thrown by app");
            }

            var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(InterfaceProject).Assembly));
            _mapper = config.CreateMapper();
            
            foreach (var item in args)
            {
                Console.WriteLine($"args: {item}");
            }
            
            Console.WriteLine("test --" + arg);
            await Task.Delay(2000);

            for (var i = 1; i < 100; i++)
            {
                Console.WriteLine($"test {i}" + arg);
            }

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
            Console.WriteLine(s.Serialize(evt));

            return 9;
        }
    }
}