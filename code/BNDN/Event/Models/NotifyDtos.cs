namespace Event.Models
{
    public abstract class NotifyDto
    {
        protected NotifyDto(string id)
        {
            Id = id;
        }

        public string Id { get; set; }
    }

    public class ExcludeDto : NotifyDto
    {
        public ExcludeDto(string id) : base(id) { }
    }

    public class IncludeDto : NotifyDto
    {
        public IncludeDto(string id) : base(id) { }
    }

    public class PendingDto : NotifyDto
    {
        public PendingDto(string id) : base(id) { }
    }

    public class ConditionDto : NotifyDto
    {
        public ConditionDto(string id) : base(id) { }
    }
}