using System;

namespace utopia.Models
{
    public class SpeciesIndividual
    {
        
        public SpeciesIndividual() {}
        
        public SpeciesIndividual(TileSpecies ts, Species species, string individualName, string individualDesc, int individualHp, int individualAttack)
        {
            var rand = new Random();
            Species = species;
            TileSpecies = ts;
            IndividualName = individualName;
            IndividualDesc = individualDesc;
            IndividualHp = individualHp;
            IndividualAttack = individualAttack;
            IndividualChadness = rand.Next(1, 71);
        }
        
        public int Id { get; set; }
        public int SpeciesId { get; set; }
        public TileSpecies TileSpecies { get; set; }
        public int TileSpeciesId { get; set; }
        public Species Species { get; set; }
        public string IndividualName { get; set; }
        public string IndividualDesc { get; set; }
        public int IndividualHp { get; set; }
        public int IndividualAttack { get; set; }
        public int IndividualChadness { get; set; }
        
    }
}