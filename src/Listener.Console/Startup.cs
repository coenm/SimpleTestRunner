using System.Collections.Generic;
using NetMQ;
using NetMQ.Sockets;

namespace Listener.Console;

static class Program
{
    public static IList<string> allowableCommandLineArgs  = new[] { "TopicA", "TopicB", "All" };

    public static void Main(string[] args)
    {
        if (args.Length != 1 || !allowableCommandLineArgs.Contains(args[0]))
        {
            System.Console.WriteLine("Expected one argument, either " +
                                     "'TopicA', 'TopicB' or 'All'");
            // Environment.Exit(-1);
            args = new[] { "All", };
        }

        string topic = args[0] == "All" ? "" : args[0];
        System.Console.WriteLine("Subscriber started for Topic : {0}", topic);
        using var subSocket = new SubscriberSocket();
        subSocket.Options.ReceiveHighWatermark = 1000;
        subSocket.Bind("tcp://*:12345");
        //subSocket.Connect("tcp://localhost:12345");
        subSocket.Subscribe(topic);
        System.Console.WriteLine("Subscriber socket connecting...");
            
        while (true)
        {
            string incomingTopic = subSocket.ReceiveFrameString();
            string eventName = subSocket.ReceiveFrameString();
            string msgType = subSocket.ReceiveFrameString();
            string payload = subSocket.ReceiveFrameString();
            System.Console.WriteLine(eventName);
            System.Console.WriteLine(msgType);
            System.Console.WriteLine(payload);
            System.Console.WriteLine(new string('-', 100));
        }
    }
}