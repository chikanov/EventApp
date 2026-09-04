namespace EventService.Domain.CustomExceptions
{
    public class ActiveLeasesExceededException : Exception
    {
        public ActiveLeasesExceededException() : base("The limit of 10 active bookings has been reached.") { }

        public ActiveLeasesExceededException(string message) : base(message) { }
    }
}
