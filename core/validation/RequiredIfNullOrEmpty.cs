using System;
using System.ComponentModel.DataAnnotations;

namespace Glow.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredIfNullOrEmpty : RequiredAttribute
    {

        private string[] PropertyName { get; set; }

        public RequiredIfNullOrEmpty(params string[] properties)
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

                if (result && string.IsNullOrWhiteSpace(valueString))
                {
                    return new ValidationResult(ErrorMessage);
                }
            }


            return ValidationResult.Success;
        }
    }
}
