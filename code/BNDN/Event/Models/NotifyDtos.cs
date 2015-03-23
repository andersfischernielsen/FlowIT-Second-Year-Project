namespace Event.Models
{
    public class NotifyDto
    {
        public string Id { get; set; }
    }

    public class ExcludeDto : NotifyDto
    {
    }

    public class IncludeDto : NotifyDto
    {
    }

    public class PendingDto : NotifyDto
    {
    }
}