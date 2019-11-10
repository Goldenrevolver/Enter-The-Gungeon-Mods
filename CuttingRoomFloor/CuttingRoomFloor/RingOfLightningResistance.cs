using ItemAPI;
using UnityEngine;

namespace CuttingRoomFloor
{
    class RingOfLightningResistance : PassiveItem
    {
        //template BulletStatusEffectItem
        
        public static void Init()
        {
            //The name of the item
            string itemName = "Ring of Lightning Resistance";

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
            string longDesc = "Prevents damage from electricity.\n\nA ring originally worn by Alistair, the Thunderbolt. The gemstone set in the golden band is cracked down the middle.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.D;
        }

        private DamageTypeModifier m_electricityImmunity;

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);

            this.m_electricityImmunity = new DamageTypeModifier();
            this.m_electricityImmunity.damageMultiplier = 0f;
            this.m_electricityImmunity.damageType = CoreDamageTypes.Electric;
            player.healthHaver.damageTypeModifiers.Add(this.m_electricityImmunity);
        }

        public override DebrisObject Drop(PlayerController player)
        {
            DebrisObject debrisObject = base.Drop(player);

            if (player && this.m_electricityImmunity != null)
            {
                player.healthHaver.damageTypeModifiers.Remove(this.m_electricityImmunity);
            }
            return debrisObject;
        }
    }
}
