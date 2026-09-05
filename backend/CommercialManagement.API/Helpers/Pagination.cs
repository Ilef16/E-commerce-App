using CommercialManagement.API.Constants;

namespace CommercialManagement.API.Helpers;

public static class Pagination
{
    public static string? Validate(int page, int pageSize)
    {
        if (page < 1)
            return "Le paramètre page doit être supérieur ou égal à 1.";

        if (pageSize < 1 || pageSize > BusinessConstants.MaxPageSize)
            return $"Le paramètre pageSize doit être compris entre 1 et {BusinessConstants.MaxPageSize}.";

        return null;
    }
}
