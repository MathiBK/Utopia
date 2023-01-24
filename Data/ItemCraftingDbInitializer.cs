
﻿using System;
 using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
 using utopia.Helper;
 using utopia.Models;

namespace utopia.Data
{
    public class ItemCraftingDbInitializer
    {

        public static void Initialize(ApplicationDbContext context, bool dev)
        {
            if (dev)
            {
                context.Database.EnsureCreated();
            }
            var resources = context.Resources.ToList();
            var items = context.Items.ToList();
            
            context.ItemCrafting.AddRange(new List<ItemCrafting>
            {
                new ItemCrafting(resources.Single(r => r.ResourceName == "Wood"),2, resources.Single(r => r.ResourceName == "Stone"),2,null,0,items.Single(i => i.ItemName == "Axe")),
                new ItemCrafting(resources.Single(r => r.ResourceName == "Wood"),2, resources.Single(r => r.ResourceName == "Stone"),2,null,0,items.Single(i => i.ItemName == "Pickaxe"))
            });
            context.SaveChanges();
            
        }
    }
}