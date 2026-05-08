using System.ComponentModel.DataAnnotations;

namespace EventApp.Helper
{
    /// DateGreaterThanAttribute
    [AttributeUsage(AttributeTargets.Property)]
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        /// DateGreaterThanAttribute
        public DateGreaterThanAttribute(string dateToCompareToFieldName)
        {
            DateToCompareToFieldName = dateToCompareToFieldName;
        }

        private string DateToCompareToFieldName { get; set; }

        /// ValidationResult IsValid
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTime earlierDate = (DateTime)value;
            DateTime laterDate = (DateTime)validationContext.ObjectType.GetProperty(DateToCompareToFieldName).GetValue(validationContext.ObjectInstance, null);

            if (laterDate > earlierDate)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult("The end date must be greater than the start date.");
        }
    }
}
