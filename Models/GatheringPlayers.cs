using System.Collections.Generic;

namespace utopia.Models
{
    public class GatheringPlayers
    {

        public GatheringPlayers()
        {
        }
        public GatheringPlayers(PlayerResource playerResource)
        {
            PlayerResource = playerResource;

        }
        
        public int Id { get; set; }
        public PlayerResource PlayerResource { get; set; }
    }
} 