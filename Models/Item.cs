using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace utopia.Models
{
    public class Item
    {
        public Item() {}

        public Item(Resource resource, string name, string desc, int itemEfficiency)
        {
            Resource = resource;
            ItemName = name;
            ItemDesc = desc;
            ItemEfficiency = itemEfficiency;
        }

        public int Id { get; set; }
        public string ItemName { get; set; }
        public string ItemDesc { get; set; }
        public int ItemEfficiency { get; set; }
        public Resource Resource { get; set; }
    }
}