using ItemAPI;
using UnityEngine;

namespace BeyondThePast
{
    public class WarningItem : PlayerItem
    {
        public static int WarningItemID;

        public static void Register()
        {
            //The name of the item
            string itemName = "Warning Sign";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BeyondThePast/Resources/Warning_Item";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<WarningItem>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Read Me!";
            string longDesc = "You are not supposed to play with the Cultist in single player by using the console or a custom character that does not change the base. It's very buggy. Download the 'Playable Cultist (Improved)' custom character mod instead!";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.None, 0f);
            WarningItemID = item.PickupObjectId;

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.EXCLUDED;
        }
    }
}