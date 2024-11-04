using InventoryManagement.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace InventoryManagement
{
    public sealed partial class Game : Page, INotifyPropertyChanged
    {
        private const int WinWidth = 1024;
        private const int WinHeight = 768;
        private GameItems _gameItems;
        private Item _selectedItem;
        private ImageSource _selectedImageSource;
        private Inventory _inventory = new Inventory();
        private HashSet<RelativePanel> selectedSlots = new HashSet<RelativePanel>();


        public event PropertyChangedEventHandler PropertyChanged;

        public Game()
        {
            InitializeComponent();
            ResizeView();
            _gameItems = new GameItems();
        }

        private void ResizeView()
        {
            var view = ApplicationView.GetForCurrentView();
            var desiredSize = new Windows.Foundation.Size(WinWidth, WinHeight);
            view.TryResizeView(desiredSize);
        }

        private void btn_backbtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            ClearInputFields();
            lv_itemslist.SelectedItem = null;
            lv_recipeslist.SelectedItem = null;
        }

        private async void btn_createitm_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedItem != null)
            {
                UpdateSelectedItem();
            }
            else
            {
                await CreateNewItemAsync();
            }

            ClearInputFields();
        }

        private void btn_itemDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteSelectedItem();
        }

        private void btn_recipeAdd_Click(object sender, RoutedEventArgs e)
        {
            string recipeName = txtbx_recipeName.Text;

            if (_selectedItem == null)
            {
                ShowMessageDialog("Please select an item to add a recipe to.");
                return;
            }

          
            if (cmbx_recipeItemsQt.SelectedItem is ComboBoxItem selectedItem)
            {
              
                if (int.TryParse(selectedItem.Content.ToString(), out int quantity) && quantity > 0)
                {
                    Item recipeItem = _gameItems.Items.FirstOrDefault(item => item.Name.Equals(recipeName, StringComparison.OrdinalIgnoreCase));

                    if (recipeItem != null)
                    {
                       
                        Recipe recipe = new Recipe(recipeItem.Name);
                        recipe.AddIngredient(recipeItem, quantity);

                     
                        _selectedItem.AddRecipe(recipe);

                      
                        lv_recipeslist.ItemsSource = null;
                        lv_recipeslist.ItemsSource = _selectedItem.Recipes;

                        ShowMessageDialog($"Recipe '{recipeItem.Name}' with quantity '{quantity}' added to '{_selectedItem.Name}'.");
                    }
                    else
                    {
                        ShowMessageDialog($"No item found with the name '{recipeName}'.");
                    }
                }
                else
                {
                    ShowMessageDialog("Please select a valid quantity.");
                }
            }
            else
            {
                ShowMessageDialog("Please select a quantity.");
            }

            txtbx_recipeName.Text = string.Empty; 
            cmbx_recipeItemsQt.SelectedIndex = -1; 
        }





        private async void ShowMessageDialog(string message)
        {
            var dialog = new MessageDialog(message);
            await dialog.ShowAsync();
        }

        private async void lv_itemslist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedItem = lv_itemslist.SelectedItem as Item;
            if (_selectedItem != null)
            {
                LoadSelectedItemDetails();
                await LoadAndDisplayImageAsync();
            }
        }

        private void ClearInputFields()
        {
            txtbx_itemName.Text = string.Empty;
            txtbx_itemDesc.Text = string.Empty;
            txtbx_itemImg.Text = string.Empty;
        }

        private void UpdateSelectedItem()
        {
            _selectedItem.Name = txtbx_itemName.Text;
            _selectedItem.Description = txtbx_itemDesc.Text;
            _selectedItem.ImagePath = txtbx_itemImg.Text;
            RefreshItemList();
            _selectedItem = null;
            lv_itemslist.SelectedItem = null;
        }

        private async Task CreateNewItemAsync()
        {
            var newItem = new Item(txtbx_itemName.Text, txtbx_itemDesc.Text, txtbx_itemImg.Text);
            await LoadAndDisplayImageAsync();
            _gameItems.AddItem(newItem);
            RefreshItemList();
        }

        private void DeleteSelectedItem()
        {
            if (lv_itemslist.SelectedItem is Item selectedItem)
            {
                _gameItems.RemoveItem(selectedItem);
                RefreshItemList();
                ClearInputFields();
                lv_itemslist.SelectedItem = null;
            }
        }

        private void RefreshItemList()
        {
            lv_itemslist.ItemsSource = null;
            lv_itemslist.ItemsSource = _gameItems.Items;
        }

        private void LoadSelectedItemDetails()
        {
            txtbx_itemName.Text = _selectedItem.Name;
            txtbx_itemDesc.Text = _selectedItem.Description;
            txtbx_itemImg.Text = _selectedItem.ImagePath;
        }

        public ImageSource SelectedImageSource
        {
            get => _selectedImageSource;
            set
            {
                _selectedImageSource = value;
                OnPropertyChanged(nameof(SelectedImageSource));
            }
        }

        private async Task LoadAndDisplayImageAsync()
        {
            string imageName = txtbx_itemImg.Text;
            if (string.IsNullOrWhiteSpace(imageName))
            {
                SelectedImageSource = null;
                return;
            }

            try
            {

                StorageFolder assetsFolder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
                StorageFile imageFile = await assetsFolder.GetFileAsync(imageName);


                using (IRandomAccessStream stream = await imageFile.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(stream);
                    SelectedImageSource = bitmapImage;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading image: {ex.Message}");
                SelectedImageSource = null;
            }
        }


        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void btn_addToInv_Click(object sender, RoutedEventArgs e)
        {
            if (cmbx_itemInv.SelectedItem is Item selectedItem && cmbx_itemInvQt.SelectedItem is ComboBoxItem quantityItem)
            {
                int quantity = int.Parse((string)quantityItem.Content);
                _inventory.AddItem(selectedItem, quantity);
                UpdateInventoryGrid();
            }
        }

        private void UpdateInventoryGrid()
        {
            for (int i = 0; i < _inventory.Items.Count; i++)
            {
                InventoryItem inventoryItem = _inventory.Items[i];
                string imageSourcePath = $"ms-appx:///Assets/{inventoryItem.Item.ImagePath}";

                var imageControl = (Image)FindName($"img_invSlth{i:D2}");
                var quantityText = (TextBlock)FindName($"txtb_invSlth{i:D2}");

                if (imageControl != null && quantityText != null)
                {
                    imageControl.Source = new BitmapImage(new Uri(imageSourcePath));
                    quantityText.Text = inventoryItem.Quantity.ToString();
                }
            }
        }

        private void btn_Combine_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSlots.Count != 2)
            {
                ShowMessageDialog("Please select exactly two items to combine.");
                return;
            }

            List<Item> selectedItems = new List<Item>();
            foreach (var slot in selectedSlots)
            {
                var imageControl = slot.Children.OfType<Image>().FirstOrDefault();
                if (imageControl?.DataContext is InventoryItem inventoryItem)
                {
                    selectedItems.Add(inventoryItem.Item);
                }
            }

            
            foreach (var item in _gameItems.Items)
            {
                if (item.Recipes == null || !item.Recipes.Any())
                    continue; 

                bool recipeMatch = true;
                var recipeIngredients = item.Recipes.GroupBy(r => r.Name)
                                                     .ToDictionary(g => g.Key, g => g.Count());

           
                foreach (var recipeIngredient in recipeIngredients)
                {
                    int selectedCount = selectedItems.Count(i => i.Name == recipeIngredient.Key);

                    if (selectedCount < recipeIngredient.Value)
                    {
                        recipeMatch = false;
                        break; 
                    }
                }

              
                if (recipeMatch)
                {
                    CraftItem(item, recipeIngredients, selectedItems);
                    UpdateInventoryGrid();
                    selectedSlots.Clear();
                    return; 
                }
            }

            ShowMessageDialog("The selected items cannot be combined into a valid recipe.");
        }


        private void CraftItem(Item craftedItem, Dictionary<string, int> recipeIngredients, List<Item> selectedItems)
        {
            
            foreach (var recipeIngredient in recipeIngredients)
            {
                int quantityNeeded = recipeIngredient.Value;
                foreach (var selectedItem in selectedItems.ToList())
                {
                    if (selectedItem.Name == recipeIngredient.Key && quantityNeeded > 0)
                    {
                        var inventoryItem = _inventory.Items.FirstOrDefault(i => i.Item.Name == selectedItem.Name);
                        if (inventoryItem != null)
                        {
                            int quantityToRemove = Math.Min(quantityNeeded, inventoryItem.Quantity);
                            _inventory.RemoveItem(inventoryItem.Item, quantityToRemove);
                            quantityNeeded -= quantityToRemove;

                            if (quantityToRemove > 0)
                            {
                                selectedItems.Remove(selectedItem);
                            }

                            if (quantityNeeded <= 0)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            _inventory.AddItem(craftedItem, 1);
        }

        private object GetSlotIndex(RelativePanel slot)
        {
            var name = slot.Name;
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Slot name cannot be null or empty.");
            }


            if (!name.StartsWith("slot"))
            {
                throw new ArgumentException("Invalid slot name format.");
            }

            var indexString = name.Substring(4);
            if (!int.TryParse(indexString, out var index))
            {
                throw new ArgumentException("Invalid slot index in name.");
            }

            return index - 1;
        }


        private void CraftItem(Item recipe, List<Item> selectedItems)
        {

            var craftedItem = new Item(recipe.Name, recipe.Description, recipe.ImagePath);


            foreach (var uniqueIngredient in recipe.Recipes.Distinct())
            {
                int requiredQuantity = recipe.Recipes.Count(item => item.Name == uniqueIngredient.Name);
                int quantityToRemove = requiredQuantity;

                foreach (var selectedItem in selectedItems.ToList())
                {
                    if (selectedItem.Name == uniqueIngredient.Name && quantityToRemove > 0)
                    {

                        var inventoryItem = _inventory.Items.FirstOrDefault(i => i.Item.Name == selectedItem.Name);
                        if (inventoryItem != null)
                        {

                            int quantityRemoved = Math.Min(quantityToRemove, inventoryItem.Quantity);
                            _inventory.RemoveItem(inventoryItem.Item, quantityRemoved);
                            quantityToRemove -= quantityRemoved;


                            if (quantityToRemove <= 0)
                            {
                                break;
                            }
                        }
                    }
                }
            }


            _inventory.AddItem(craftedItem, 1);

            foreach (var slot in InventoryGrid.Children.OfType<RelativePanel>())
            {
                var imageControl = (Image)slot.Children.OfType<Image>().FirstOrDefault();
                if (imageControl == null || imageControl.DataContext == null)
                {

                    imageControl = new Image
                    {
                        Source = new BitmapImage(new Uri($"ms-appx:///Assets/{craftedItem.ImagePath}")),
                        DataContext = new InventoryItem { Item = craftedItem }
                    };
                    slot.Children.Add(imageControl);

                    var quantityText = (TextBlock)slot.Children.OfType<TextBlock>().FirstOrDefault();
                    if (quantityText != null)
                    {
                        quantityText.Text = "1";
                    }
                    break;
                }
            }
        }

        private bool CanCraftItem(Item recipe, List<Item> selectedItems)
        {
            foreach (var uniqueIngredient in recipe.Recipes.Distinct())
            {

                int requiredQuantity = recipe.Recipes.Count(item => item.Name == uniqueIngredient.Name);


                int selectedQuantity = selectedItems.Count(item => item.Name == uniqueIngredient.Name);

                if (selectedQuantity < requiredQuantity)
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsRecipeMatch(List<Item> recipe, List<InventoryItem> selectedInventoryItems)
        {
            foreach (var recipeItem in recipe.Distinct())
            {
                int requiredQuantity = recipe.Count(item => item.Name == recipeItem.Name);
                int selectedQuantity = selectedInventoryItems.Count(invItem => invItem.Item.Name == recipeItem.Name);

                if (selectedQuantity < requiredQuantity)
                {
                    return false; 
                }
            }
            return true; 
        }

       
        private void DeductInventoryQuantities(List<Item> recipe, List<InventoryItem> selectedInventoryItems)
        {
            foreach (var recipeItem in recipe.Distinct())
            {
                int requiredQuantity = recipe.Count(item => item.Name == recipeItem.Name);

                foreach (var selectedInventoryItem in selectedInventoryItems.Where(invItem => invItem.Item.Name == recipeItem.Name).ToList())
                {
                    int quantityToRemove = Math.Min(requiredQuantity, selectedInventoryItem.Quantity);
                    _inventory.RemoveItem(selectedInventoryItem.Item, quantityToRemove);
                    requiredQuantity -= quantityToRemove;

                    if (requiredQuantity <= 0) break; 
                }
            }
        }


        private void RelativePanel_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var slot = (RelativePanel)sender;

            if (selectedSlots.Contains(slot))
            {

                selectedSlots.Remove(slot);
                slot.BorderBrush = null;
                slot.BorderThickness = new Thickness(0);
            }
            else
            {

                selectedSlots.Add(slot);
                slot.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Yellow);
                slot.BorderThickness = new Thickness(2);
            }
        }

    }
}