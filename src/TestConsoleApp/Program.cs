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

            if (arg.Equals("color", StringComparison.CurrentCultureIgnoreCase))
            {
                Console.Write("Normal Text");
                ConsoleColor foregroundColor1 = Console.ForegroundColor;
                try
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Red text");
                }
                finally
                {
                    Console.ForegroundColor = foregroundColor1;
                }
                Console.Write("Normal Text");
                return 0;
            }


            if (arg.Equals("black", StringComparison.CurrentCultureIgnoreCase))
            {
                Console.Write("Normal Text");
                Console.Write("Red text");
                Console.Write("Normal Text");
                return 1;
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
                    TestCaseId = Guid.Empty,
                    TestCaseName = "aaa",
                    TestElement = new TestCaseDto
                        {
                            CodeFilePath = "d",
                            DisplayName = "dd",
                        },
                };

            await Console.Error.WriteLineAsync($"Err: {Console.IsErrorRedirected}.");

            var s = new Serialization();
            Console.WriteLine(s.Serialize(evt));

            return 9;
        }
    }
}