namespace Event
{
    public class LockDto
    {
        public string Id { get; set; }
        public string WorkflowId { get; set; }
        //It's expected that LockOwner matches the Id of the EventAddressDto making the lock call.
        public string LockOwner { get; set; }   
    }
}