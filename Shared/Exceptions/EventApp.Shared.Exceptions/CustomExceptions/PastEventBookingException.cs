namespace EventApp.Shared.Exceptions.CustomExceptions
{
    public class PastEventBookingException : Exception
    {
        public PastEventBookingException() : base("You cannot book an event that has already taken place.") { }

        public PastEventBookingException(string message) : base(message) { }
    }
}
