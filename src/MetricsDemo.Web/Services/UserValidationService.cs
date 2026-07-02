using MetricsDemo.Web.Models;

namespace MetricsDemo.Web.Services;

/// <summary>
/// Guard-clause validation — early returns keep nesting and cyclomatic complexity low.
/// </summary>
public sealed class UserValidationService
{
    public (bool Ok, List<string> Errors) ValidateProfile(UserProfileDto profile)
    {
        var errors = new List<string>();

        if (profile is null)
        {
            errors.Add("profile_null");
            return (false, errors);
        }

        ValidateEmail(profile, errors);
        ValidateAge(profile, errors);
        ValidatePhone(profile, errors);
        ValidateDisplayName(profile, errors);

        return (errors.Count == 0, errors);
    }

    private static void ValidateEmail(UserProfileDto profile, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(profile.Email))
            errors.Add("email_required");
        else if (!profile.Email.Contains('@', StringComparison.Ordinal))
            errors.Add("email_format");
    }

    private static void ValidateAge(UserProfileDto profile, List<string> errors)
    {
        if (profile.Age < 0)
            errors.Add("age_negative");
        else if (profile.Age is > 0 and < 13 && profile.AcceptsMarketing)
            errors.Add("minor_marketing");
        else if (profile.Age > 120)
            errors.Add("age_unrealistic");
    }

    private static void ValidatePhone(UserProfileDto profile, List<string> errors)
    {
        if (!string.IsNullOrEmpty(profile.Phone) && profile.Phone.Count(char.IsDigit) < 8)
            errors.Add("phone_short");
    }

    private static void ValidateDisplayName(UserProfileDto profile, List<string> errors)
    {
        var name = profile.DisplayName ?? string.Empty;
        if (name.Length > 200)
            errors.Add("display_name_long");
    }
}
