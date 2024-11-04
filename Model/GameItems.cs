using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Model
{
    public class GameItems
    {

        public List<Item> Items;
        

        public GameItems()
        {
            Items = new List<Item>();
            
        }

        public void AddItem(Item item)
        {
            Items.Add(item);
        }

        public void RemoveItem(Item item)
        {
            Items.Remove(item);
        }

    }
}
