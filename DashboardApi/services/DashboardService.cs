using MongoDB.Driver;
using System.Linq;

public class DashboardService : IDashboardService
{
    private readonly IMongoCollection<HouseGroup> _houseGroups;
    private readonly IMongoCollection<Request> _requests;
    private readonly IMongoCollection<DictRequestStatus> _requestStatuses;

    public DashboardService(IMongoDatabase db)
    {
        _houseGroups = db.GetCollection<HouseGroup>("HouseGroups");
        _requests = db.GetCollection<Request>("Requests");
        _requestStatuses = db.GetCollection<DictRequestStatus>("DictRequestStatus");
    }

    public async Task<List<RequestSummaryDto>> GetRequestSummaryAsync(string orgId)
    {
        var houseGroups = await _houseGroups.Find(hg => hg.Organization.ID == orgId).ToListAsync();
        var result = new List<RequestSummaryDto>();

        var statuses = await _requestStatuses.Find(_ => true).ToListAsync();
        var statusLookup = statuses.ToDictionary(s => s.Id, s => s.Code);

        foreach (var hg in houseGroups)
        {
            var requestFilter = Builders<Request>.Filter.In("House.ID", hg.Houses);
            var requests = await _requests.Find(requestFilter).ToListAsync();

            var assigned = requests.Count(r => statusLookup.ContainsKey(r.RequestStatusId) && statusLookup[r.RequestStatusId] == "executorAssigned");
            var inProgress = requests.Count(r => statusLookup.ContainsKey(r.RequestStatusId) && statusLookup[r.RequestStatusId] == "work");

            result.Add(new RequestSummaryDto
            {
                Id = hg.Id,
                Name = hg.Name.Ru,
                Requests = new SummaryCounts
                {
                    Assigned = assigned,
                    InProgress = inProgress
                }
            });
        }

        return result;
    }

    public async Task<List<RequestDeadlineDto>> GetRequestDeadlinesAsync(string orgId)
    {
        var houseGroups = await _houseGroups.Find(hg => hg.Organization.ID == orgId).ToListAsync();
        var result = new List<RequestDeadlineDto>();

        var statuses = await _requestStatuses.Find(_ => true).ToListAsync();
        var statusLookup = statuses.ToDictionary(s => s.Id, s => s.Code);

        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var afterTomorrow = today.AddDays(2);

        foreach (var hg in houseGroups)
        {
            var requests = await _requests.Find(r => r.House != null && hg.Houses.Contains(r.House.ID)).ToListAsync();

            var deadlines = new DeadlineGroup
            {
                Today = CountByStatus(requests.Where(r => r.Deadline.HasValue && r.Deadline.Value.Date == today), statusLookup),
                Tomorrow = CountByStatus(requests.Where(r => r.Deadline.HasValue && r.Deadline.Value.Date == tomorrow), statusLookup),
                Overdue = CountByStatus(requests.Where(r => r.Deadline.HasValue && r.Deadline.Value < today), statusLookup),
                MoreThan2Days = CountByStatus(requests.Where(r => r.Deadline.HasValue && r.Deadline.Value > afterTomorrow), statusLookup)            };
                
            result.Add(new RequestDeadlineDto
            {
                Id = hg.Id,
                Name = hg.Name.Ru,
                Deadlines = deadlines
            });
        }

        return result;
    }

    private StatusCounts CountByStatus(IEnumerable<Request> requests, Dictionary<string, string> statusLookup)
    {
        return new StatusCounts
        {
            Assigned = requests.Count(r => statusLookup.ContainsKey(r.RequestStatusId) && statusLookup[r.RequestStatusId] == "executorAssigned"),
            InProgress = requests.Count(r => statusLookup.ContainsKey(r.RequestStatusId) && statusLookup[r.RequestStatusId] == "work")
        };
    }
}