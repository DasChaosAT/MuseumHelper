using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using MuseumHelper.ItemData;

namespace MuseumHelper
{
    public class ModEntry : Mod
    {

        public override void Entry(IModHelper helper)
        {
            PlayerEvents.InventoryChanged += this.PlayerEvents_InventoryChanged;
        }

        private void PlayerEvents_InventoryChanged(object sender, EventArgsInventoryChanged e)
        {
            if (!Context.IsWorldReady)
                return;

            LibraryMuseum museum = Game1.locations.OfType<LibraryMuseum>().FirstOrDefault();
            if (museum == null)
                return;

            bool donateableItem = false;
            List<string> donateableItemNameList = new List<string>();

            foreach (ItemStackChange itemStack in e.Added)
            {
                if (museum.isItemSuitableForDonation(itemStack.Item))
                    donateableItem = true;
                
                if (donateableItem) 
                    donateableItemNameList.Add(itemStack.Item.Name);
            }
                
            if (!donateableItem)
                return;

            List<int> donatedItemIDList = new List<int>();

            foreach (KeyValuePair<Vector2, int> pair in museum.museumPieces.Pairs)
                donatedItemIDList.Add(pair.Value);

            bool donateItemFound = false;
            foreach (SearchableItem sItem in GetItems(donateableItemNameList.ToArray()))
                if (!donatedItemIDList.Contains(sItem.ID))
                    donateItemFound = true;

            if (donateItemFound)
                Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("message-type.found-museum-item")));

        }

        private readonly ItemRepository Items = new ItemRepository();

        public IEnumerable<SearchableItem> GetItems(string[] searchWords)
        {
            // normalise search term
            searchWords = searchWords?.Where(word => !string.IsNullOrWhiteSpace(word)).ToArray();
            if (searchWords?.Any() == false)
                searchWords = null;

            // find matches
            return (
                from item in this.Items.GetAll()
                let term = $"{item.ID}|{item.Type}|{item.Name}|{item.DisplayName}"
                where searchWords == null || searchWords.All(word => term.IndexOf(word, StringComparison.CurrentCultureIgnoreCase) != -1)
                select item
            );
        }
    }
}