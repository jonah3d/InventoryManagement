using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Model
{
    public class Recipe
    {
        public string Name { get; set; }
        public List<RecipeIngredient> Ingredients { get; set; }

        public Recipe(string name)
        {
            Name = name;
            Ingredients = new List<RecipeIngredient>();
        }

        public void AddIngredient(Item ingredient, int quantity)
        {
            Ingredients.Add(new RecipeIngredient(ingredient, quantity));
        }
    }


}
