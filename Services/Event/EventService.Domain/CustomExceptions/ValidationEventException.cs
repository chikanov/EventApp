namespace EventService.Domain.CustomExceptions
{
    public class ValidationEventException : Exception
    {
        public IDictionary<string, ICollection<string>> Errors { get; } = new Dictionary<string, ICollection<string>>();

        public ValidationEventException(IDictionary<string, ICollection<string>> errors) : base("Validation failed")
        {
            Errors = errors;
        }

        public ValidationEventException(string field, string error) : base("Validation failed")
        {
            Errors = new Dictionary<string, ICollection<string>>
            {
                { field, new[] { error } }
            };
        }
    }
}
