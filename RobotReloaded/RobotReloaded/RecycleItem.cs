using ItemAPI;
using UnityEngine;

namespace RobotReloaded
{
    public class RecycleItem : PassiveItem
    {
        public static void Init()
        {
            //The name of the item
            string itemName = "Recycle-inator";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "RobotReloaded/Resources/recycle_item";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<RecycleItem>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "For a good cause";
            string longDesc = "Even the clueless scientist who made it agrees on the importance of the cause.\n\nPermanently increases damage by 5% for every junk you ever picked up. Destroyed chests are guaranteed to drop junk.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.D;
        }
    }
}
