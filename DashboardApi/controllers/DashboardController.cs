using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/dashboard/organization/{id}")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("requests-summary")]
    public async Task<IActionResult> GetSummary(string id)
    {
        var data = await _dashboardService.GetRequestSummaryAsync(id);
        return Ok(new { houseGroups = data });
    }

    [HttpGet("requests-deadlines")]
    public async Task<IActionResult> GetDeadlines(string id)
    {
        var data = await _dashboardService.GetRequestDeadlinesAsync(id);
        return Ok(new { houseGroups = data });
    }
}