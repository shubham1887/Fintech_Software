namespace FintechApp.Entities
{
    public class BaseEntity
    {
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
