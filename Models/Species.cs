using System.Collections.Generic;

namespace utopia.Models
{
    public class Species
    {
        public Species() {}
        
        public Species(string speciesName, string speciesDesc, int speciesDropness, bool speciesType, int hpLow, int hpHigh, int attackLow, int attackHigh)
        {
            SpeciesName = speciesName;
            SpeciesDesc = speciesDesc;
            SpeciesDropness = speciesDropness;
            SpeciesType = speciesType;
            HpLow = hpLow;
            HpHigh = hpHigh;
            AttackLow = attackLow;
            AttackHigh = attackHigh;
        }
        
        public int Id { get; set; }
        public string SpeciesName { get; set; }
        public string SpeciesDesc { get; set; }
        public int SpeciesDropness { get; set; }
        public bool SpeciesType { get; set; }
        public int SpeciesPopulation { get; set; }
        public int HpLow { get; set; }
        public int HpHigh { get; set; }
        public int AttackLow { get; set; }
        public int AttackHigh { get; set; }
    }
}