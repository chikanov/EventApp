namespace UserService.Domain.CustomExceptions
{
    public class NotFoundUserException : Exception
    {
        public NotFoundUserException(string message) : base(message) { }
    }
}
