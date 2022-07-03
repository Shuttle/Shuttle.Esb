namespace Shuttle.Esb
{
    public class ExceptionHandling
    {
        public bool ShouldRetry { get; private set; } = true;
        public bool ShouldBlock { get; private set; }

        public ExceptionHandling Unrecoverable()
        {
            ShouldRetry = false;

            return this;
        }

        public ExceptionHandling Block()
        {
            ShouldBlock = true;

            return this;
        }
    }
}