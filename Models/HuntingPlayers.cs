using System.Collections.Generic;

namespace utopia.Models
{
    public class HuntingPlayers
    {

        public HuntingPlayers()
        {
        }
        public HuntingPlayers(PlayerResource playerResource)
        {
            PlayerResource = playerResource;
            SpeciesIndividual = null;
        }
        
        public int Id { get; set; }
        public PlayerResource PlayerResource { get; set; }
        public SpeciesIndividual SpeciesIndividual { get; set; }
    }
}