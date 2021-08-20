using ItemAPI;
using UnityEngine;

namespace CuttingRoomFloor
{
    internal class RingOfLightningResistance : PassiveItem
    {
        //template BulletStatusEffectItem

        public static void Init()
        {
            //The name of the item
            string itemName = "Ring of Lightning";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "CuttingRoomFloor/Resources/ring_of_lightning_resistance";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<RingOfLightningResistance>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Grounded";
            string longDesc = "Prevents damage from electricity. 25% increased projectile speed.\n\nA ring originally worn by Alistair, the Thunderbolt. The gemstone set in the golden band is cracked down the middle.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");
            item.AddPassiveStatModifier(PlayerStats.StatType.ProjectileSpeed, 1.25f, StatModifier.ModifyMethod.MULTIPLICATIVE);

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.D;
        }

        private DamageTypeModifier electricityImmunity;

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);

            this.electricityImmunity = new DamageTypeModifier
            {
                damageMultiplier = 0f,
                damageType = CoreDamageTypes.Electric
            };

            player.healthHaver.damageTypeModifiers.Add(this.electricityImmunity);
        }

        public override DebrisObject Drop(PlayerController player)
        {
            DebrisObject debrisObject = base.Drop(player);

            Cleanup(player);

            return debrisObject;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Cleanup(Owner);
        }

        private void Cleanup(PlayerController player)
        {
            if (player && this.electricityImmunity != null)
            {
                player.healthHaver.damageTypeModifiers.Remove(this.electricityImmunity);
                electricityImmunity = null;
            }
        }
    }
}