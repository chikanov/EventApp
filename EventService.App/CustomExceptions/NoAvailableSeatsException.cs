namespace EventApp.CustomExceptions
{
    public class NoAvailableSeatsException : Exception
    {
        public NoAvailableSeatsException() : base("No available seats for this event.") { }

        public NoAvailableSeatsException(string message) : base(message) { }

        public NoAvailableSeatsException(string message, Exception inner) : base(message, inner) { }

        protected NoAvailableSeatsException(System.Runtime.Serialization.SerializationInfo si, System.Runtime.Serialization.StreamingContext sc) : base(si, sc) { }
    }
}
