using System;
using System.ComponentModel.DataAnnotations;

namespace Glow.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EmailIfNullOrEmpty : ValidationAttribute
    {

        private string[] PropertyName { get; set; }

        public EmailIfNullOrEmpty(params string[] properties)
        {
            PropertyName = properties;
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            var instance = context.ObjectInstance;
            Type type = instance.GetType();

            var valueString = value?.ToString();
            foreach (var v in PropertyName)
            {
                var result = string.IsNullOrEmpty(type.GetProperty(v).GetValue(instance)?.ToString());

                // TODO use regex
                if (result && valueString != null && !valueString.Contains("@"))
                {
                    return new ValidationResult(ErrorMessage);
                }
            }


            return ValidationResult.Success;
        }
    }

}
