using ItemAPI;
using UnityEngine;

namespace CuttingRoomFloor
{
    class BubbleShield
    {
        public static void Init()
        {
            //The name of the item
            string itemName = "Bubble Shield";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "CuttingRoomFloor/Resources/bubble_shield";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<ReflectShieldPlayerItem>();
            item.duration = 5f;

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Don't swallow it";
            string longDesc = "Extremely elastic, severely sturdy and ridiculously regenerative, this magic bubble gum can be both a life saver as well as a pastime while exploring the dungeon.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.PerRoom, 2);

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.D;
        }
    }
}