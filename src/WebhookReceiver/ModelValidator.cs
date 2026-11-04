using System.ComponentModel.DataAnnotations;

namespace WebhookReceiver;

public static class ModelValidator
{
    // Runs the data annotation attributes on a model and groups errors by field,
    // in the format Results.ValidationProblem expects.
    public static bool TryValidate(object model, out Dictionary<string, string[]> errors)
    {
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);

        errors = results
            .SelectMany(r => (r.MemberNames.Any() ? r.MemberNames : new[] { "" })
                .Select(member => (member, message: r.ErrorMessage ?? "Invalid value")))
            .GroupBy(e => e.member)
            .ToDictionary(g => g.Key, g => g.Select(e => e.message).ToArray());

        return isValid;
    }
}
