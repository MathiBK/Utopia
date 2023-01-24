namespace utopia.Models
{
    public class PlayerResource
    {

        public PlayerResource() {}

        public PlayerResource(Resource resource, int amount)
        {
            Resource = resource;
            Amount = amount;
        }

        public int Id { get; set; }
        public int ResourceId { get; set; }
        public Player Player { get; set; }
        public int PlayerId { get; set; }
        public Resource Resource { get; set; }
        public int Amount { get; set; }

    }
}