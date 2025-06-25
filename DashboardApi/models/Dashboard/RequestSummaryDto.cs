public class RequestSummaryDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public SummaryCounts Requests { get; set; }
}

public class SummaryCounts
{
    public int Assigned { get; set; }
    public int InProgress { get; set; }
    public int Total => Assigned + InProgress;
}