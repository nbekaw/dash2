public class RequestDeadlineDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DeadlineGroup Deadlines { get; set; }
}

public class DeadlineGroup
{
    public StatusCounts Overdue { get; set; }
    public StatusCounts Today { get; set; }
    public StatusCounts Tomorrow { get; set; }
    public StatusCounts MoreThan2Days { get; set; }

}

public class StatusCounts
{
    public int Assigned { get; set; }
    public int InProgress { get; set; }

    public int Total => Assigned + InProgress;
}