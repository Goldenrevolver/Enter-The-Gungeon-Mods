using ItemAPI;
using UnityEngine;

namespace CuttingRoomFloor
{
    class CueBullets
    {
        public static void Init()
        {
            //The name of the item
            string itemName = "Cue Bullets";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "CuttingRoomFloor/Resources/cue_bullets";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<BilliardsStickItem>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Bullet Kin, Corner Pocket";
            string longDesc = "Substantially increases knockback on kills; enemies hit by corpses take damage.\n\nHaving given up on ever killing their pasts, many failed Gungeoneers pass the time by playing dumb games. Maybe one such adventurer missed playing billiards?";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.D;
        }
    }
}