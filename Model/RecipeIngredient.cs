using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Model
{
    public class RecipeIngredient
    {
        public Item Ingredient { get; set; }
        public int Quantity { get; set; }

        public RecipeIngredient(Item ingredient, int quantity)
        {
            Ingredient = ingredient;
            Quantity = quantity;
        }
    }

}
