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
        public List<Recipe> Recipes { get; set; } = new List<Recipe>(); 

        public Item(string name, string description, string imagePath)
        {
            Name = name;
            Description = description;
            ImagePath = imagePath;
        }

        public void AddRecipe(Recipe recipe)
        {
            Recipes.Add(recipe);
        }
    }



}
