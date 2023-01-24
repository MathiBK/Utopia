
﻿using System;
 using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
 using utopia.Helper;
 using utopia.Models;

namespace utopia.Data
{
    public class ItemDbInitializer
    {

        public static void Initialize(ApplicationDbContext context, bool dev)
        {
            
            if (dev)
            {
                context.Database.EnsureCreated();
            }

            var resources = context.Resources.ToList();
            var items = context.Items.ToList();
            
            context.Items.AddRange(new List<Item>
            {
                new Item(resources.Single(r => r.ResourceName == "Wood"),"Axe","Better at gathering wood",2),
                new Item(resources.Single(r => r.ResourceName == "Stone"),"Pickaxe","Better at gathering stone",2)
                
            });
            context.SaveChanges();
            
        }
    }
}