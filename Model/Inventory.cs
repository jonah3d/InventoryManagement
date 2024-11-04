using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Model
{
    public class Inventory
    {
        public List<InventoryItem> Items { get; } = new List<InventoryItem>();

        public void AddItem(Item item, int quantity)
        {
            var existingItem = Items.FirstOrDefault(i => i.Item.Name == item.Name);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                Items.Add(new InventoryItem { Item = item, Quantity = quantity });
            }
        }

        public void RemoveItem(Item item, int quantity)
        {
            var existingItem = Items.FirstOrDefault(i => i.Item.Name == item.Name);
            if (existingItem != null)
            {
                existingItem.Quantity -= quantity;
                if (existingItem.Quantity <= 0)
                {
                    Items.Remove(existingItem);
                }
            }
        }

        public int GetItemQuantity(Item item)
        {
            var existingItem = Items.FirstOrDefault(i => i.Item.Name == item.Name);
            return existingItem?.Quantity ?? 0;
        }
    }
}
