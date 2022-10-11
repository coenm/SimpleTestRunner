namespace PipePublisher.BuildLogger
{
    using System.Diagnostics.Tracing;
    using System.Security.Policy;
    using Interface.Naming;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using Microsoft.VisualStudio.TestPlatform.Utilities;
    using Pipe.Publisher;

    //https://learn.microsoft.com/en-us/visualstudio/msbuild/build-loggers?view=vs-2022 
    public class MySimpleLogger : Logger
    {
        private readonly Pipe.Publisher.Publisher _publisher;
        private IEventSource? _eventSource;

        public MySimpleLogger()
        {
            _publisher = new Pipe.Publisher.Publisher(new ConsolePublisherOutput(), GetPipeName());
        }

        public override void Initialize(IEventSource eventSource)
        {
            _eventSource = eventSource;

            //Register for the ProjectStarted, TargetStarted, and ProjectFinished events
            _eventSource.ProjectStarted += EventSourceOnProjectStarted;
            // _eventSource.ProjectStarted += new ProjectStartedEventHandler(eventSource_ProjectStarted);
            _eventSource.TargetStarted += new TargetStartedEventHandler(eventSource_TargetStarted);
            _eventSource.ProjectFinished += new ProjectFinishedEventHandler(eventSource_ProjectFinished);
        }

        private void EventSourceOnProjectStarted(object sender, ProjectStartedEventArgs e)
        {
            // e.ProjectFile
            _publisher.Send(null, "");
        }

        public override void Shutdown()
        {
            // _eventSource.ProjectStarted -= new ProjectStartedEventHandler(eventSource_ProjectStarted);
            // _eventSource.TargetStarted -= new TargetStartedEventHandler(eventSource_TargetStarted);
            // _eventSource.ProjectFinished -= new ProjectFinishedEventHandler(eventSource_ProjectFinished);

            _publisher.Dispose();
            base.Shutdown();
        }

        private static string GetPipeName()
        {
            try
            {
                var value = Environment.GetEnvironmentVariable(EnvironmentVariables.PIPE_NAME);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }
            catch (Exception)
            {
                // do nothing
            }

            throw new Exception("Could not find port to connect to.");
        }

        void eventSource_ProjectFinished(object sender, ProjectFinishedEventArgs e)
        {
            Console.WriteLine("Project Finished: " + e.ProjectFile);
        }
        void eventSource_TargetStarted(object sender, TargetStartedEventArgs e)
        {
            if (Verbosity == LoggerVerbosity.Detailed)
            {
                Console.WriteLine("Target Started: " + e.TargetName);
            }
        }
    }
}