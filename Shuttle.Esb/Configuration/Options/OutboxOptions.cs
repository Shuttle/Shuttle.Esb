namespace Shuttle.Esb;

public class OutboxOptions : ProcessorOptions
{
    public OutboxOptions()
    {
        ThreadCount = 1;
    }
}