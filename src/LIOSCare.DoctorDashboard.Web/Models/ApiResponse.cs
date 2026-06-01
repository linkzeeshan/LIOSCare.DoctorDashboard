namespace LIOSCare.DoctorDashboard.Web.Models;

internal static class ApiResponse
{
    internal static object Data<T>(T data) => new { data };

    internal static object Paged<T>(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        pageSize = Math.Max(1, pageSize);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new
        {
            data = items,
            meta = new
            {
                pagination = new
                {
                    page,
                    pageSize,
                    totalCount,
                    totalPages,
                    hasNext = page < totalPages,
                    hasPrevious = page > 1
                }
            }
        };
    }

    internal static object Listed<T>(IReadOnlyList<T> items) =>
        new { data = items, meta = new { count = items.Count } };

    internal static object Err(string message, int status) => new { error = message, status };
}
