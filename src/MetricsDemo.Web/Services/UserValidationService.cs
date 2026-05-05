using MetricsDemo.Web.Models;

namespace MetricsDemo.Web.Services;

/// <summary>
/// Deep nesting and mixed control flow to surface cognitive complexity in analyzers.
/// </summary>
public sealed class UserValidationService
{
    public (bool Ok, List<string> Errors) ValidateProfile(UserProfileDto profile)
    {
        var errors = new List<string>();

        try
        {
            if (profile is null)
            {
                errors.Add("profile_null");
                return (false, errors);
            }

            if (string.IsNullOrWhiteSpace(profile.Email))
            {
                errors.Add("email_required");
            }
            else
            {
                if (!profile.Email.Contains('@', StringComparison.Ordinal))
                {
                    errors.Add("email_format");
                }
                else
                {
                    var domain = profile.Email.Split('@').LastOrDefault();
                    if (domain is null || domain.Length < 3)
                    {
                        errors.Add("email_domain_short");
                    }
                    else
                    {
                        foreach (var ch in domain)
                        {
                            if (char.IsWhiteSpace(ch))
                            {
                                errors.Add("email_domain_whitespace");
                                break;
                            }
                        }
                    }
                }
            }

            if (profile.Age < 0)
                errors.Add("age_negative");
            else if (profile.Age > 0 && profile.Age < 13)
            {
                if (profile.AcceptsMarketing)
                    errors.Add("minor_marketing");
                else
                {
                    if (profile.CountryCode == "US")
                        errors.Add("minor_us_extra_check");
                }
            }
            else if (profile.Age > 120)
                errors.Add("age_unrealistic");

            if (!string.IsNullOrEmpty(profile.Phone))
            {
                var digits = profile.Phone.Count(char.IsDigit);
                if (digits < 8)
                {
                    errors.Add("phone_short");
                }
                else
                {
                    if (profile.Phone.StartsWith('+', StringComparison.Ordinal))
                    {
                        if (digits > 15)
                            errors.Add("phone_long");
                    }
                    else if (profile.CountryCode == "IN" && digits < 10)
                        errors.Add("phone_in_length");
                }
            }

            var displayName = profile.DisplayName ?? string.Empty;
            for (var i = 0; i < displayName.Length; i++)
            {
                var c = displayName[i];
                if (c < 32)
                {
                    errors.Add("display_name_control_chars");
                    break;
                }
            }
        }
        catch (Exception)
        {
            errors.Add("validation_exception");
        }

        return (errors.Count == 0, errors);
    }
}
