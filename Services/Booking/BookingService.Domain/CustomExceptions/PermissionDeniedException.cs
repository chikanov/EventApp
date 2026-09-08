namespace BookingService.Domain.CustomExceptions
{
    public class PermissionDeniedException : Exception
    {
        public PermissionDeniedException() : base("The user does not have the rights to perform this operation.") { }

        public PermissionDeniedException(string message) : base(message) { }
    }
}
