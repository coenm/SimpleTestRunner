namespace Interface.Data.Logger;

// AutoMapper
public class TestRunMessageEventArgsDto : EventArgsBaseDto
{
    public string Message { get; set; }

    public TestMessageLevel Level { get; set; }
}

public abstract class EventArgsBaseDto
{
}