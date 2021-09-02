using ItemAPI;
using System.Collections.Generic;
using UnityEngine;

namespace BeyondThePast
{
    public class SupplySupport : SupplyDropItem
    {
        public static int SupplySupportID;
        private static readonly string theItemName = "Supply Support";

        public static void Register()
        {
            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BeyondThePast/Resources/Supply_Drop";

            //Create new GameObject
            GameObject obj = new GameObject(theItemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<SupplySupport>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(theItemName, resourceName, obj);

            var supplyDrop = PickupObjectDatabase.GetById(77) as SupplyDropItem;

            //Ammonomicon entry variables
            string shortDesc = "I Need Supplies!";
            string longDesc = "Calls in a supply drop. Can be used once per floor.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");
            SupplySupportID = item.PickupObjectId;

            foreach (var publicField in typeof(SupplyDropItem).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
            {
                publicField.SetValue(item, publicField.GetValue(supplyDrop));
            }

            item.consumable = false;
            item.consumableHandlesOwnDuration = false;
            item.consumableOnActiveUse = false;
            item.consumableOnCooldownUse = false;

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.EXCLUDED;
        }

        public static void HandleSynergy(AdvancedSynergyEntry synergy)
        {
            if (synergy.NameKey == "#SUPPLYDROP")
            {
                if (synergy.OptionalItemIDs == null)
                {
                    synergy.OptionalItemIDs = new List<int>();
                }

                if (!synergy.OptionalItemIDs.Contains(SupplySupportID))
                {
                    synergy.OptionalItemIDs.Add(SupplySupportID);
                }
                else
                {
                    return;
                }

                if (!synergy.OptionalItemIDs.Contains(77))
                {
                    synergy.OptionalItemIDs.Add(77);
                }

                if (synergy.MandatoryItemIDs.Count != 2)
                {
                    ETGModConsole.Log(SynergyHelper.GenerateModEditedSynergyProblem(synergy, "Supply Drop", theItemName));
                }

                synergy.MandatoryItemIDs.RemoveAll((int x) => x == 77);
            }
            else
            {
                if (synergy.OptionalItemIDs != null && synergy.OptionalItemIDs.Contains(77) && !synergy.OptionalItemIDs.Contains(SupplySupportID))
                {
                    synergy.OptionalItemIDs.Add(SupplySupportID);
                }

                if (synergy.MandatoryItemIDs != null && synergy.MandatoryItemIDs.Contains(77))
                {
                    ETGModConsole.Log(SynergyHelper.GenerateModdedSynergyProblem(synergy, "Supply Drop", theItemName));
                }
            }
        }

        [SerializeField]
        private bool hasBeenUsedThisFloor = false;

        public override void Pickup(PlayerController player)
        {
            if (this.m_pickedUp)
            {
                return;
            }

            player.OnNewFloorLoaded += ResetCooldown;
            base.Pickup(player);
        }

        public override bool CanBeUsed(PlayerController user)
        {
            return !hasBeenUsedThisFloor;
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