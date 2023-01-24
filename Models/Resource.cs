namespace utopia.Models
{
    public class Resource
    {
        public Resource() {}
        
        public Resource(int id, string resourceName, string resourceDesc, int resourceDropness)
        {
            Id = id;
            ResourceName = resourceName;
            ResourceDesc = resourceDesc;
            ResourceDropness = resourceDropness;
        }
        
        public int Id { get; set; }
        public string ResourceName { get; set; }
        public string ResourceDesc { get; set; }
        public int ResourceDropness { get; set; }

    }
}