namespace utopia.Models
{
    public class TileResource
    {

        public TileResource() {}

        public TileResource(Resource resource, int amount, int resCap)
        {
            Resource = resource;
            Amount = amount;
            ResourceCap = resCap;
        }

        public int Id { get; set; }
        public int ResourceId { get; set; }
        public int TileId { get; set; }
        public int ResourceCap { get; set; }
        public Resource Resource { get; set; }
        public int Amount { get; set; }

    }
}