namespace utopia.Models
{
    public class TribeResource
    {

        public TribeResource() {}

        public TribeResource(Resource resource, int amount)
        {
            Resource = resource;
            Amount = amount;
        }

        public int Id { get; set; }
        public int ResourceId { get; set; }
        public int TribeId { get; set; }
        public Resource Resource { get; set; }
        public int Amount { get; set; }

    }
}