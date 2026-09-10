namespace EventService.Domain.CustomExceptions
{
    public class NotFoundEventException : Exception
    {
        public NotFoundEventException(string message) : base(message) { }
    }
}
