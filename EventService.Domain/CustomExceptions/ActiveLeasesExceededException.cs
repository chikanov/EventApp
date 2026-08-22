namespace EventService.Domain.CustomExceptions
{
    public class ActiveLeasesExceededException : Exception
    {
        public ActiveLeasesExceededException() : base("The limit of active armor has been reached.") { }

        public ActiveLeasesExceededException(string message) : base(message) { }
    }
}
