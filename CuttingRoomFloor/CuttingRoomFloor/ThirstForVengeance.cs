using ItemAPI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoomFloor
{
    internal class ThirstForVengeance : PassiveItem
    {
        public static void Init()
        {
            //The name of the item
            string itemName = "Thirst For Vengeance";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "CuttingRoomFloor/Resources/thirst_for_vengeance";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<ThirstForVengeance>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Taking You With Me";
            string longDesc = "Increased rate of fire the closer you are to defeat. Slaying an enemy during your last breath prevents death.\n\nSometimes getting even is all you’ve got left.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.C;
        }

        private float fireRatePerHalfHeart = 0.05f;
        private StatModifier buff = null;

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);

            if (!PassiveItem.ActiveFlagItems.ContainsKey(player))
            {
                PassiveItem.ActiveFlagItems.Add(player, new Dictionary<Type, int>());
            }
            if (!PassiveItem.ActiveFlagItems[player].ContainsKey(typeof(PoweredByRevengeItem)))
            {
                PassiveItem.ActiveFlagItems[player].Add(typeof(PoweredByRevengeItem), 1);
            }
            else
            {
                PassiveItem.ActiveFlagItems[player][typeof(PoweredByRevengeItem)] = PassiveItem.ActiveFlagItems[player][typeof(PoweredByRevengeItem)] + 1;
            }

            buff = new StatModifier
            {
                statToBoost = PlayerStats.StatType.RateOfFire,
                modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE,
                amount = 1f
            };

            player.ownerlessStatModifiers.Add(buff);

            player.healthHaver.OnHealthChanged += GiveFireRatePerMissingHealth;
            player.OnReceivedDamage += HealRobot;
        }

        public override DebrisObject Drop(PlayerController player)
        {
            var drop = base.Drop(player);

            if (PassiveItem.ActiveFlagItems[player].ContainsKey(typeof(PoweredByRevengeItem)))
            {
                PassiveItem.ActiveFlagItems[player][typeof(PoweredByRevengeItem)] = Mathf.Max(0, PassiveItem.ActiveFlagItems[player][typeof(PoweredByRevengeItem)] - 1);
                if (PassiveItem.ActiveFlagItems[player][typeof(PoweredByRevengeItem)] == 0)
                {
                    PassiveItem.ActiveFlagItems[player].Remove(typeof(PoweredByRevengeItem));
                }
            }

            Cleanup(player);

            return drop;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (Owner)
            {
                if (this.m_pickedUp && PassiveItem.ActiveFlagItems[Owner].ContainsKey(typeof(PoweredByRevengeItem)))
                {
                    PassiveItem.ActiveFlagItems[Owner][typeof(PoweredByRevengeItem)] = Mathf.Max(0, PassiveItem.ActiveFlagItems[Owner][typeof(PoweredByRevengeItem)] - 1);
                    if (PassiveItem.ActiveFlagItems[Owner][typeof(PoweredByRevengeItem)] == 0)
                    {
                        PassiveItem.ActiveFlagItems[Owner].Remove(typeof(PoweredByRevengeItem));
                    }
                }
            }

            Cleanup(Owner);
        }

        private void Cleanup(PlayerController player)
        {
            if (player)
            {
                if (player.healthHaver)
                {
                    player.healthHaver.OnHealthChanged -= GiveFireRatePerMissingHealth;
                }

                player.OnReceivedDamage -= HealRobot;

                player.ownerlessStatModifiers.Remove(buff);
                buff = null;

                player.stats.RecalculateStats(player);
            }
        }

        public void GiveFireRatePerMissingHealth(float resultValue, float maxValue)
        {
            if (buff == null || !Owner || !Owner.healthHaver || Owner.healthHaver.IsDead)
            {
                return;
            }

            buff.amount = 1f;

            if (Owner.characterIdentity != PlayableCharacters.Robot)
            {
                float healthLost = maxValue - resultValue;
                healthLost *= 2;

                if (healthLost > 0.5)
                {
                    buff.amount = 1 + (fireRatePerHalfHeart * healthLost);
                }
            }
            else
            {
                float armorLost = 6 - Owner.healthHaver.Armor;

                if (armorLost > 0.5)
                {
                    buff.amount = 1 + (fireRatePerHalfHeart * armorLost);
                }
            }

            Owner.stats.RecalculateStats(Owner);
        }

        public static void NoRevengeFullHeal(Action<PlayerController, PlayerController> orig, PlayerController self, PlayerController obj)
        {
            if (obj.characterIdentity == PlayableCharacters.Robot)
            {
                obj.healthHaver.Armor += 2;
            }
            else
            {
                obj.healthHaver.ApplyHealing(1f);
            }
        }

        public void HealRobot(PlayerController player)
        {
            var m_revenging = Tools.GetFieldValue<bool>(typeof(PlayerController), "m_revenging", player);

            if (m_revenging && player.healthHaver.Armor <= 0f)
            {
                if (player.characterIdentity == PlayableCharacters.Robot)
                {
                    player.healthHaver.Armor = 1;
                }
            }
        }
    }
}