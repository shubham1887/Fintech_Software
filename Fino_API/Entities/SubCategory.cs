namespace FintechApp.Entities
{
    public class SubCategory : BaseEntity
    {
        public int SubCategoryID { get; set; }
        public int CategoryID { get; set; }
        public string SubCategoryName { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string LongDescription { get; set; } = string.Empty;
        public Category Category { get; set; } = null!;
    }
}
