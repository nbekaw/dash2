public interface IDashboardService
{
    Task<List<RequestSummaryDto>> GetRequestSummaryAsync(string organizationId);
    Task<List<RequestDeadlineDto>> GetRequestDeadlinesAsync(string organizationId);
}

