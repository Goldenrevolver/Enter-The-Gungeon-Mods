using ItemAPI;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace BeyondThePast
{
    public class RecycleItem : PassiveItem
    {
        public static int RecycleItemID;

        public static void Register()
        {
            //The name of the item
            string itemName = "Recycle-inator";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BeyondThePast/Resources/Recycle_Item";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<RecycleItem>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "For A Good Cause";
            string longDesc = "Permanently increases damage by 5% for every junk you ever picked up. Destroyed chests are guaranteed to drop junk.\n\nEven the clueless scientist who made it agrees on the importance of the cause.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");
            RecycleItemID = item.PickupObjectId;

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.EXCLUDED;
        }

        private Coroutine checkForHandCoroutine;

        public override void Pickup(PlayerController player)
        {
            if (this.m_pickedUp)
            {
                return;
            }

            checkForHandCoroutine = player.StartCoroutine(CheckForHand());

            base.Pickup(player);
        }

        public override DebrisObject Drop(PlayerController player)
        {
            var ret = base.Drop(player);

            Cleanup(player);

            return ret;
        }

        protected override void OnDestroy()
        {
            Cleanup(Owner);

            base.OnDestroy();
        }

        private void Cleanup(PlayerController player)
        {
            if (player)
            {
                player.StopCoroutine(checkForHandCoroutine);
                checkForHandCoroutine = null;
            }
        }

        private IEnumerator CheckForHand()
        {
            while (true)
            {
                if (Owner?.inventory?.AllGuns != null)
                {
                    foreach (var item in Owner.inventory.AllGuns)
                    {
                        if (item.PickupObjectId == 576)
                        {
                            item.PreventStartingOwnerFromDropping = true;
                        }
                    }
                }

                yield return new WaitForSeconds(1);
            }
        }

        public static void SetupHook()
        {
            new Hook(typeof(Chest).GetMethod("OnBroken", BindingFlags.Instance | BindingFlags.NonPublic), typeof(RecycleItem).GetMethod(nameof(RecycleItem.OnBrokenHook)));
        }

        public static void OnBrokenHook(Action<Chest> orig, Chest self)
        {
            bool hasRecycleItem = false;

            foreach (var player in GameManager.Instance.AllPlayers)
            {
                foreach (var item in player.passiveItems)
                {
                    if (item is RecycleItem)
                    {
                        hasRecycleItem = true;
                        break;
                    }
                }
            }

            if (hasRecycleItem)
            {
                float num = GameManager.Instance.RewardManager.ChestDowngradeChance;
                float num2 = GameManager.Instance.RewardManager.ChestHalfHeartChance;
                float num3 = GameManager.Instance.RewardManager.ChestExplosionChance;
                float num4 = GameManager.Instance.RewardManager.ChestJunkChance;
                float num5 = GameManager.Instance.RewardManager.HasKeyJunkMultiplier;
                float num6 = GameManager.Instance.RewardManager.HasJunkanJunkMultiplier;

                GameManager.Instance.RewardManager.ChestDowngradeChance = 0f;
                GameManager.Instance.RewardManager.ChestHalfHeartChance = 0f;
                GameManager.Instance.RewardManager.ChestExplosionChance = 0f;
                GameManager.Instance.RewardManager.ChestJunkChance = 1f;
                GameManager.Instance.RewardManager.HasKeyJunkMultiplier = 1f;
                GameManager.Instance.RewardManager.HasJunkanJunkMultiplier = 1f;
                orig(self);
                GameManager.Instance.RewardManager.ChestDowngradeChance = num;
                GameManager.Instance.RewardManager.ChestHalfHeartChance = num2;
                GameManager.Instance.RewardManager.ChestExplosionChance = num3;
                GameManager.Instance.RewardManager.ChestJunkChance = num4;
                GameManager.Instance.RewardManager.HasKeyJunkMultiplier = num5;
                GameManager.Instance.RewardManager.HasJunkanJunkMultiplier = num6;
            }
            else
            {
                orig(self);
            }
        }
    }
}