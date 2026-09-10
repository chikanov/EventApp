namespace BookingService.Domain.CustomExceptions
{
    public class ValidationBookingException : Exception
    {
        public IDictionary<string, ICollection<string>> Errors { get; } = new Dictionary<string, ICollection<string>>();

        public ValidationBookingException(IDictionary<string, ICollection<string>> errors) : base("Validation failed")
        {
            Errors = errors;
        }

        public ValidationBookingException(string field, string error) : base("Validation failed")
        {
            Errors = new Dictionary<string, ICollection<string>>
            {
                { field, new[] { error } }
            };
        }
    }
}
