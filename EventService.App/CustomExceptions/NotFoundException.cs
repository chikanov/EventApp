namespace EventApp.CustomExceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException() : base("Resource not found.") { }

        public NotFoundException(string message) : base(message) { }

        public NotFoundException(string message, Exception inner) : base(message, inner) { }

        protected NotFoundException(System.Runtime.Serialization.SerializationInfo si, System.Runtime.Serialization.StreamingContext sc) : base(si, sc) { }
    }
}
