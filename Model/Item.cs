using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Model
{
    public class Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }

        // Dictionary where each key is an Item needed to craft this item, and value is the quantity needed
        public Dictionary<Item, int> CraftingRequirements { get; set; } = new Dictionary<Item, int>();

        public Item(string name, string description, string imagePath)
        {
            Name = name;
            Description = description;
            ImagePath = imagePath;
        }

        // Method to add an item and quantity needed for crafting
        public void AddCraftingRequirement(Item item, int quantity)
        {
            CraftingRequirements[item] = quantity;
        }
    }



}
