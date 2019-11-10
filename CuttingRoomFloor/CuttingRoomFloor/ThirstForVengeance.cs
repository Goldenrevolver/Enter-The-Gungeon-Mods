using ItemAPI;
using UnityEngine;
using System;
using System.Reflection;

namespace CuttingRoomFloor
{
    class ThirstForVengeance
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
            var item = obj.AddComponent<PoweredByRevengeItem>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Taking You With Me";
            string longDesc = "Slaying an enemy during your last breath prevents death.\n\nSometimes getting even is all you’ve got left.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.C;
        }

        public static void InitializeCallbacks(Action<PlayerController> orig, PlayerController self)
        {
            orig(self);
            self.OnReceivedDamage += HealRobot;
        }

        public static void NoRevengeFullHeal(Action<PlayerController, PlayerController> orig, PlayerController self, PlayerController obj)
        {
            if(obj.characterIdentity == PlayableCharacters.Robot)
            {
                obj.healthHaver.Armor += 2;
            }
            else
            {
                obj.healthHaver.ApplyHealing(1f);
            }
        }

        public static void HealRobot(PlayerController player)
        {
            bool flag = player.healthHaver.Armor > 0f;

            bool m_revenging = (bool)typeof(PlayerController).GetField("m_revenging", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(player);

            if (!flag && m_revenging && PassiveItem.IsFlagSetForCharacter(player, typeof(PoweredByRevengeItem)))
            {
                if(player.characterIdentity == PlayableCharacters.Robot)
                {
                    player.healthHaver.Armor = 1;
                }
            }
        }
    }
}
