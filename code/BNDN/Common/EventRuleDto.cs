namespace Common
{
    public class EventRuleDto
    {
        public bool Condition { get; set; }
        public bool Exclusion { get; set; }
        public bool Response { get; set; }
        public bool Inclusion { get; set; }
    }
}
