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

        // Собираем все House IDs
        var allHouseIds = houseGroups.SelectMany(hg => hg.Houses).ToHashSet();

        // Загружаем все запросы сразу
        var allRequests = await _requests
            .Find(r => r.House != null && allHouseIds.Contains(r.House.ID))
            .ToListAsync();

        // Группируем по House ID
        var requestsByHouseId = allRequests
            .GroupBy(r => r.House.ID)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var hg in houseGroups)
        {
            // Это лишний запрос в базу, для каждго ЖК будет делатся такой запрос, надо старатся минимизироват запросы в базу данных
            // для этого лучше подгружать нужные данные заранее и группировать их
            //var requestFilter = Builders<Request>.Filter.In("House.ID", hg.Houses);
            //var requests = await _requests.Find(requestFilter).ToListAsync();

            var groupRequests = hg.Houses
                .Where(requestsByHouseId.ContainsKey)
                .SelectMany(houseId => requestsByHouseId[houseId])
                .ToList();

            var assigned = groupRequests.Count(r => statusLookup.ContainsKey(r.RequestStatusId) && statusLookup[r.RequestStatusId] == "executorAssigned");
            var inProgress = groupRequests.Count(r => statusLookup.ContainsKey(r.RequestStatusId) && statusLookup[r.RequestStatusId] == "work");

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

        var allHouseIds = houseGroups.SelectMany(hg => hg.Houses).ToHashSet();

        // Загружаем все запросы
        var allRequests = await _requests
            .Find(r => r.House != null && allHouseIds.Contains(r.House.ID))
            .ToListAsync();

        // Группируем запросы по ID дома
        var requestsByHouseId = allRequests
            .GroupBy(r => r.House.ID)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var hg in houseGroups)
        {
            // тоже лишний запрос в базу данных
            //var requests = await _requests.Find(r => r.House != null && hg.Houses.Contains(r.House.ID)).ToListAsync();

            var groupRequests = hg.Houses
                .Where(requestsByHouseId.ContainsKey)
                .SelectMany(houseId => requestsByHouseId[houseId])
                .ToList();

            var deadlines = new DeadlineGroup
            {
                Today = CountByStatus(groupRequests.Where(r => r.Deadline.HasValue && r.Deadline.Value.Date == today), statusLookup),
                Tomorrow = CountByStatus(groupRequests.Where(r => r.Deadline.HasValue && r.Deadline.Value.Date == tomorrow), statusLookup),
                Overdue = CountByStatus(groupRequests.Where(r => r.Deadline.HasValue && r.Deadline.Value < today), statusLookup),
                MoreThan2Days = CountByStatus(groupRequests.Where(r => r.Deadline.HasValue && r.Deadline.Value > afterTomorrow), statusLookup)            };
                
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