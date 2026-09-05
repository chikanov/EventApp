namespace UserService.Domain.CustomExceptions
{
    public class ValidationUserException : Exception
    {
        public IDictionary<string, ICollection<string>> Errors { get; } = new Dictionary<string, ICollection<string>>();

        public ValidationUserException(IDictionary<string, ICollection<string>> errors) : base("Validation failed")
        {
            Errors = errors;
        }

        public ValidationUserException(string field, string error) : base("Validation failed")
        {
            Errors = new Dictionary<string, ICollection<string>>
            {
                { field, new[] { error } }
            };
        }
    }
}
