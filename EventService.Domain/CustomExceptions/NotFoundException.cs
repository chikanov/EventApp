namespace EventService.Domain.CustomExceptions
{
    public class NotFoundException : Exception
    {
        internal NotFoundException(string message) : base(message) { }
    }
}
