using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;
using System.Linq;
using SObject = StardewValley.Object;

namespace MuseumHelper
{
    public class ModEntry : Mod
    {
        private Dictionary<int, Item> itemDic;
        private bool itemDicGenerated = false;

        public override void Entry(IModHelper helper)
        {
            GenerateItemList();
            PlayerEvents.InventoryChanged += this.PlayerEvents_InventoryChanged;
        }

        private void PlayerEvents_InventoryChanged(object sender, EventArgsInventoryChanged e)
        {
            if (!Context.IsWorldReady)
                return;

            if (!itemDicGenerated)
                return;

            LibraryMuseum museum = Game1.locations.OfType<LibraryMuseum>().FirstOrDefault();
            if (museum == null)
                return;

            Dictionary<int, Item> donateableItemDic = new Dictionary<int, Item>();
            foreach (ItemStackChange itemStack in e.Added)
                if (museum.isItemSuitableForDonation(itemStack.Item))
                    if (itemDic.Any(x => x.Value.Name.Equals(itemStack.Item.Name)))
                        donateableItemDic.Add(itemDic.FirstOrDefault(x => x.Value.Equals(itemStack.Item)).Key, itemStack.Item);
                    else
                        this.Monitor.Log($"{Game1.player.Name} - Donateable Item: Didn't found ID for {itemStack.Item}.", LogLevel.Error);

            if (donateableItemDic.Count == 0)
                return;

            List<int> donatedItemList = new List<int>();
            foreach (KeyValuePair<Vector2, int> pair in museum.museumPieces.Pairs)
                donatedItemList.Add(pair.Value);

            bool ItemsToDonate = false;
            foreach (KeyValuePair<int, Item> donateableItem in donateableItemDic)
                if (!donatedItemList.Contains(donateableItem.Key))
                    ItemsToDonate = true;

            if (ItemsToDonate)
                Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("message-type.found-museum-item")));
        }

        private void GenerateItemList()
        {
            itemDic = new Dictionary<int, Item>();

            foreach (int id in Game1.objectInformation.Keys)
            {
                SObject item = new SObject(id, 1);
                itemDic.Add(id, item);
            }

            itemDicGenerated = true;
        }
    }
}