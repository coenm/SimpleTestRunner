namespace Interface.Data.Logger;

public class TestRunMessageEventArgsDto : EventArgsBaseDto
{
    public string Message { get; set; }

    public TestMessageLevel Level { get; set; }
}