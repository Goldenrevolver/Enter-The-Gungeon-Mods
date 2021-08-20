using ItemAPI;
using System.Collections.Generic;
using UnityEngine;

namespace BeyondThePast
{
    public class SupplySupport : SupplyDropItem
    {
        public static int SupplySupportID;

        public static void Register()
        {
            //The name of the item
            string itemName = "Supply Support";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BeyondThePast/Resources/Supply_Drop";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<SupplySupport>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            var supplyDrop = PickupObjectDatabase.GetById(77) as SupplyDropItem;

            //Ammonomicon entry variables
            string shortDesc = "I Need Supplies!";
            string longDesc = "Calls in a supply drop. Can be used once per floor.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");
            SupplySupportID = item.PickupObjectId;

            foreach (var synergy in GameManager.Instance.SynergyManager.synergies)
            {
                if (synergy != null && synergy.NameKey == "#SUPPLYDROP")
                {
                    synergy.MandatoryItemIDs = new List<int>() { 494 };
                    synergy.OptionalItemIDs = new List<int>() { 77, item.PickupObjectId };
                    synergy.RequiresAtLeastOneGunAndOneItem = false;
                }
            }

            item.consumable = false;
            item.consumableHandlesOwnDuration = false;
            item.consumableOnActiveUse = false;
            item.consumableOnCooldownUse = false;

            foreach (var publicField in typeof(SupplyDropItem).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
            {
                publicField.SetValue(item, publicField.GetValue(supplyDrop));
            }

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.EXCLUDED;
        }

        private bool hasBeenUsedThisFloor = false;

        public override bool CanBeUsed(PlayerController user)
        {
            return !hasBeenUsedThisFloor;
        }

        public override void Pickup(PlayerController player)
        {
            if (this.m_pickedUp)
            {
                return;
            }

            player.OnNewFloorLoaded += ResetCooldown;
            base.Pickup(player);
        }

        protected override void DoEffect(PlayerController user)
        {
            hasBeenUsedThisFloor = true;

            base.DoEffect(user);
        }

        public void ResetCooldown(PlayerController player)
        {
            hasBeenUsedThisFloor = false;
        }

        // no 'On Drop' effect needed
        protected override void OnDestroy()
        {
            if (LastOwner)
            {
                LastOwner.OnNewFloorLoaded -= ResetCooldown;
            }

            base.OnDestroy();
        }
    }
}