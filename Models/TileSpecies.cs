using System.Collections.Generic;

namespace utopia.Models
{
    public class TileSpecies
    {
        
        public TileSpecies() { }
        public TileSpecies(Species species, int amount)
        {
            Species = species;
            Amount = amount;
            SpeciesIndividuals = new List<SpeciesIndividual>();
        }

        public int Id { get; set; }
        public int TileId { get; set; }
        public Species Species { get; set; }
        public int SpeciesId { get; set; }
        public int Amount { get; set; }
        public List<SpeciesIndividual> SpeciesIndividuals { get; set; }
        
        
        public override string ToString() => $"(TileId:{TileId}, Species: {Species.SpeciesName})";
    }
}