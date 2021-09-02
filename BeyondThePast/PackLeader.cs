using ItemAPI;
using System.Collections;
using UnityEngine;

namespace BeyondThePast
{
    public class PackLeader : PassiveItem
    {
        public static int PackLeaderID;

        public static void Register()
        {
            //The name of the item
            string itemName = "Pack Leader";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BeyondThePast/Resources/Pack_Leader";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<PackLeader>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Strength Of The Pack";
            string longDesc = "Increases movement speed by 0.25 for every companion you have.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");
            PackLeaderID = item.PickupObjectId;

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.EXCLUDED;
        }

        // doesn't matter if these are reset on drop

        private int companionCountAtLastUpdate = 0;
        private StatModifier statBuff;
        private Coroutine checkForCompanionCoroutine;
        private Coroutine checkForColtCoroutine;

        public override void Pickup(PlayerController player)
        {
            if (this.m_pickedUp)
            {
                return;
            }

            statBuff = new StatModifier
            {
                statToBoost = PlayerStats.StatType.MovementSpeed,
                modifyType = StatModifier.ModifyMethod.ADDITIVE,
                amount = 0f
            };

            player.ownerlessStatModifiers.Add(statBuff);
            player.stats.RecalculateStats(player, false, false);

            checkForCompanionCoroutine = player.StartCoroutine(CheckForCompanion());

            checkForColtCoroutine = player.StartCoroutine(CheckForColt());

            base.Pickup(player);
        }

        private IEnumerator CheckForColt()
        {
            while (true)
            {
                if (Owner?.inventory?.AllGuns != null)
                {
                    foreach (var item in Owner.inventory.AllGuns)
                    {
                        if (item.PickupObjectId == 62)
                        {
                            if (!item.InfiniteAmmo || !item.PreventStartingOwnerFromDropping)
                            {
                                item.InfiniteAmmo = true;
                                item.PreventStartingOwnerFromDropping = true;
                            }
                        }
                    }
                }

                yield return new WaitForSeconds(1);
            }
        }

        private IEnumerator CheckForCompanion()
        {
            while (true)
            {
                if (Owner && Owner.companions != null)
                {
                    if (Owner.companions.Count != companionCountAtLastUpdate)
                    {
                        companionCountAtLastUpdate = Owner.companions.Count;
                        OnChangeEvent(companionCountAtLastUpdate);
                    }
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        public void OnChangeEvent(int newValue)
        {
            if (Owner)
            {
                statBuff.amount = 0.25f * newValue;
                Owner.stats.RecalculateStats(Owner, false, false);
            }
        }

        public override DebrisObject Drop(PlayerController player)
        {
            var drop = base.Drop(player);

            Cleanup(player);

            return drop;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Cleanup(Owner);
        }

        private void Cleanup(PlayerController player)
        {
            if (player)
            {
                player.StopCoroutine(checkForCompanionCoroutine);
                checkForCompanionCoroutine = null;

                player.StopCoroutine(checkForColtCoroutine);
                checkForColtCoroutine = null;

                player.ownerlessStatModifiers.Remove(statBuff);
                statBuff = null;

                player.stats.RecalculateStats(player, false, false);
            }
        }
    }
}