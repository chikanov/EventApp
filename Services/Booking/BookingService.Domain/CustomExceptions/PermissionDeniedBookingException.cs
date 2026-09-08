namespace BookingService.Domain.CustomExceptions
{
    public class PermissionDeniedBookingException : Exception
    {
        public PermissionDeniedBookingException() : base("The user does not have the rights to perform this operation.") { }

        public PermissionDeniedBookingException(string message) : base(message) { }
    }
}
