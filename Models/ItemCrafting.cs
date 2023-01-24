using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace utopia.Models
{
    public class ItemCrafting
    {
        public ItemCrafting() {}

        public ItemCrafting(Resource resource1,int resource1Amount, Resource resource2,int resource2Amount, Resource resource3, int resource3Amount, Item item)
        {
            Resource1 = resource1;
            Resource2 = resource2;
            Resource3 = resource3;
            Item = item;
            Resource1Amount = resource1Amount;
            Resource2Amount = resource2Amount;
            Resource3Amount = resource3Amount;
        }

        public int Id { get; set; }
        public Item Item { get; set; }
        public int Resource1Amount { get; set; }
        public int Resource2Amount { get; set; }
        public int Resource3Amount { get; set; }
        
        public Resource Resource1 { get; set; }
        public Resource Resource2 { get; set; }
        public Resource Resource3 { get; set; }

    }
}