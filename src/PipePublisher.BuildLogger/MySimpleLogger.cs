namespace PipePublisherBuildLogger
{
    using System;
    using Interface.Data.Logger;
    using Interface.Naming;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using Pipe.Publisher;

    public class ProjectStartedEventArgsDto : EventArgsBaseDto
    {
        public string Message { get; set; }
    }

    public class PipePublisherBuildLogger : Logger, INodeLogger
    {
        private Pipe.Publisher.Publisher? _publisher;
        private IEventSource? _eventSource;

        public void Initialize(IEventSource eventSource, int nodeCount)
        {
            Initialize(eventSource);
        }

        public override void Initialize(IEventSource eventSource)
        {
            if (_publisher == null)
            {
                _publisher = new Publisher(new ConsolePublisherOutput(), GetPipeName());

                _eventSource = eventSource;

                _eventSource.BuildStarted += EventSourceOnBuildStarted;
                _eventSource.ProjectStarted += EventSourceOnProjectStarted;
                // _eventSource.ProjectFinished += EventSourceOnProjectFinished;
            }
        }

        private void EventSourceOnProjectFinished(object sender, ProjectFinishedEventArgs e)
        {
            _publisher?.Send(e, e.GetType().Name);
        }

        private void EventSourceOnBuildStarted(object sender, BuildStartedEventArgs e)
        {
            _publisher?.Send(e, e.GetType().Name);
        }

        private void EventSourceOnProjectStarted(object sender, ProjectStartedEventArgs e)
        {
            var evt = new ProjectStartedEventArgsDto
                {
                    Message = e.Message,
                };
            _publisher?.Send(evt);
        }

        public override void Shutdown()
        {
            IEventSource? es = _eventSource;
            if (es != null)
            {
                // es.BuildStarted -= EventSourceOnBuildStarted;
                // es.ProjectStarted -= EventSourceOnProjectStarted;
                // es.ProjectFinished -= EventSourceOnProjectFinished;
            }

            _publisher?.Dispose();
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

            return "coen";
            throw new Exception("Could not find port to connect to.");
        }
    }
}