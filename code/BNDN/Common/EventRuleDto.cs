namespace Common
{
    //TODO: Might not be used
    public class EventRuleDto
    {
        public string Id { get; set; }
        public bool Condition { get; set; }
        public bool Exclusion { get; set; }
        public bool Response { get; set; }
        public bool Inclusion { get; set; }
    }
}
