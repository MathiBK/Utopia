using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace utopia.Models
{
    public class TribeItem
    {
        public TribeItem() { }

        public TribeItem(Item item, int quantity)
        {
            Item = item;
            ItemQuantity = quantity;
        }
        
        public int Id { get; set; }
        public int ItemId { get; set; }
        public int ItemQuantity { get; set; }
        public Item Item { get; set; }
    }
}