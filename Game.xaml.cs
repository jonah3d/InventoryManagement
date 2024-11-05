using InventoryManagement.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ObservableCollection<KeyValuePair<Item, int>> CraftingRequirements { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public Game()
        {
            InitializeComponent();
            ResizeView();
            _gameItems = new GameItems();
            CraftingRequirements = new ObservableCollection<KeyValuePair<Item, int>>();
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
                        _selectedItem.AddCraftingRequirement(recipeItem, quantity);

                        // Update the recipes list view after adding the crafting requirement
                        lv_recipeslist.ItemsSource = null; // Reset the source
                        lv_recipeslist.ItemsSource = _selectedItem.CraftingRequirements
                            .Select(req => new { ItemName = req.Key.Name, Quantity = req.Value }) // Anonymous type to bind
                            .ToList();

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

                // Refresh the recipes list view when an item is selected
                lv_recipeslist.ItemsSource = _selectedItem.CraftingRequirements
                    .Select(req => new { ItemName = req.Key.Name, Quantity = req.Value }) // Anonymous type to bind
                    .ToList();
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

            List<Item> selectedItems = selectedSlots
                .Select(slot => ((Image)slot.Children.OfType<Image>().FirstOrDefault())?.DataContext as InventoryItem)
                .Where(inventoryItem => inventoryItem != null)
                .Select(inventoryItem => inventoryItem.Item)
                .ToList();

            // Iterate through each game item and check if the selected items can craft it
            foreach (var gameItem in _gameItems.Items)
            {
                if (CanCraftItem(gameItem, selectedItems))
                {
                    // If we can craft the item, perform the crafting
                    CraftItem(gameItem, selectedItems);
                    UpdateInventoryGrid();
                    selectedSlots.Clear();
                    ShowMessageDialog($"Successfully combined items into {gameItem.Name}.");
                    return; // Exit after successfully crafting
                }
            }

            ShowMessageDialog("The selected items cannot be combined into a valid recipe.");
        }

        private bool CanCraftItem(Item item, List<Item> selectedItems)
        {
            // Check if the selected items fulfill the crafting requirements
            foreach (var requirement in item.CraftingRequirements)
            {
                Item requiredItem = requirement.Key;
                int requiredQuantity = requirement.Value;

                // Count how many of the required item are selected
                int availableQuantity = selectedItems.Count(selected => selected.Name == requiredItem.Name);
                if (availableQuantity < requiredQuantity)
                {
                    return false; // Not enough of the required item
                }
            }

            return true; // All requirements are met
        }

        private void CraftItem(Item craftedItem, List<Item> selectedItems)
        {
            // Remove the required items from the inventory
            foreach (var requirement in craftedItem.CraftingRequirements)
            {
                Item requiredItem = requirement.Key;
                int quantityNeeded = requirement.Value;

                // Use a different variable name for the inner loop
                for (int j = 0; j < quantityNeeded; j++)
                {
                    var inventoryItem = _inventory.Items.FirstOrDefault(i => i.Item.Name == requiredItem.Name);
                    if (inventoryItem != null && inventoryItem.Quantity > 0)
                    {
                        _inventory.RemoveItem(inventoryItem.Item, 1); // Remove one item
                    }
                }
            }

            // Add the crafted item to the inventory
            _inventory.AddItem(craftedItem, 1);
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
