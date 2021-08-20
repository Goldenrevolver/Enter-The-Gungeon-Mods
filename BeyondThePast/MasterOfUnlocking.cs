using ItemAPI;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeyondThePast
{
    public class MasterOfUnlocking : SpawnItemOnRoomClearItem
    {
        public static int MasterOfUnlockingID;

        public static void Register()
        {
            //The name of the item
            string itemName = "Master of Unlocking";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BeyondThePast/Resources/Master_of_Unlocking";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<MasterOfUnlocking>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Play Well, Get Keys";
            string longDesc = "Increases the chance of gaining a key upon clearing a room. Increases projectile speed by 5% for every key you are holding.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");
            MasterOfUnlockingID = item.PickupObjectId;

            foreach (var synergy in GameManager.Instance.SynergyManager.synergies)
            {
                if (synergy != null && synergy.NameKey == "#MASTEROFUNLOCKING")
                {
                    synergy.MandatoryItemIDs = new List<int>() { 356 };
                    synergy.OptionalItemIDs = new List<int>() { 140, item.PickupObjectId };
                    synergy.RequiresAtLeastOneGunAndOneItem = false;
                }
            }

            var original = PickupObjectDatabase.GetById(140) as SpawnItemOnRoomClearItem;

            foreach (var publicField in typeof(SpawnItemOnRoomClearItem).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
            {
                publicField.SetValue(item, publicField.GetValue(original));
            }

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.EXCLUDED;
        }

        private StatModifier statBuff;
        public static Action<int> OnKeysChanged = delegate (int keys) { };

        public override void Pickup(PlayerController player)
        {
            if (this.m_pickedUp)
            {
                return;
            }

            statBuff = new StatModifier
            {
                statToBoost = PlayerStats.StatType.ProjectileSpeed,
                modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE,
                amount = 1f
            };

            player.ownerlessStatModifiers.Add(statBuff);
            player.stats.RecalculateStats(player, false, false);

            OnKeysChanged += OnChangeEvent;

            base.Pickup(player);
        }

        public static void SetupHook()
        {
            new Hook(typeof(PlayerConsumables).GetProperty("KeyBullets").GetSetMethod(), typeof(MasterOfUnlocking).GetMethod(nameof(MasterOfUnlocking.CheckForKeys)));
        }

        public static void CheckForKeys(Action<PlayerConsumables, int> orig, PlayerConsumables self, int value)
        {
            orig(self, value);

            OnKeysChanged(value);
        }

        private void OnChangeEvent(int newValue)
        {
            if (Owner)
            {
                statBuff.amount = 1 + (0.05f * newValue);
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
                OnKeysChanged -= OnChangeEvent;

                player.ownerlessStatModifiers.Remove(statBuff);
                statBuff = null;
                player.stats.RecalculateStats(player, false, false);
            }
        }
    }
}