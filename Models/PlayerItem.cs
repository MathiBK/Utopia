using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace utopia.Models
{
    public class PlayerItem
    {
        public PlayerItem() { }

        public PlayerItem(Item item, int quantity, Player player)
        {
            Item = item;
            ItemQuantity = quantity;
            Player = player;
        }
        
        public int Id { get; set; }
        public int ItemId { get; set; }
        public int ItemQuantity { get; set; }
        public Item Item { get; set; }
        public Player Player { get; set; }
    }
}