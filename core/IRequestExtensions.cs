using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Glow.AspNetCore.Utils.Module
{
    public static class IRequestExtensions
    {
        public static List<ValidationResult> Validate(this IRequest obj)
        {
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(obj, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(obj, context, validationResults, true);
            return validationResults;
        }

        public static List<ValidationResult> Validate<T>(this IRequest<T> obj)
        {
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(obj, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(obj, context, validationResults, true);
            return validationResults;
        }
    }
}
