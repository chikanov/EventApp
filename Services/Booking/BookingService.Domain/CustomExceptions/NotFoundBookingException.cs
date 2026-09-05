namespace BookingService.Domain.CustomExceptions
{
    public class NotFoundBookingException : Exception
    {
        public NotFoundBookingException(string message) : base(message) { }
    }
}
