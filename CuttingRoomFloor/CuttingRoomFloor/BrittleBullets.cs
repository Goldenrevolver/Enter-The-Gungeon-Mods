using ItemAPI;
using UnityEngine;

namespace CuttingRoomFloor
{
    class BrittleBullets : PassiveItem
    {
        //based on FragileGunItem and FragileGunItemPiece

        public static void Init()
        {
            //The name of the item
            string itemName = "Brittle Bullets";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "CuttingRoomFloor/Resources/brittle_bullets";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<BrittleBullets>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Risk/Reward";
            string longDesc = "Greatly increases firepower. All equipped ammunition will shatter upon receiving damage.\n\nIt takes an idiot to do cool things. That's why it's cool!";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.Damage, 0.75f, StatModifier.ModifyMethod.ADDITIVE);

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.C;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.OnReceivedDamage += loseAmmo;
        }

        public override DebrisObject Drop(PlayerController player)
        {
            var drop = base.Drop(player);
            player.OnReceivedDamage -= loseAmmo;
            return drop;
        }

        private void loseAmmo(PlayerController player)
        {
            if (player && player.CurrentGun && !player.CurrentGun.InfiniteAmmo)
            {
                player.CurrentGun.ammo = 0;
            }
        }
    }
}