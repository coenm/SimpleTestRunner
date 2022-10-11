namespace Pipe.Publisher;

public interface IPublisherOutput
{
    void WriteLine(string? message);

    void Write(string? message);
}