using ItemAPI;
using System.Collections.Generic;
using UnityEngine;

namespace BeyondThePast
{
    public class FakeHeroBandana : PassiveItem
    {
        public static int FakeHeroBandanaID;
        private static readonly string theItemName = "Fake Hero Bandana";

        public static void Register()
        {
            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BeyondThePast/Resources/Fake_Hero_Bandana";

            //Create new GameObject
            GameObject obj = new GameObject(theItemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<FakeHeroBandana>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(theItemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Limitless?";
            string longDesc = "Ammo Capacity multiplied by 1.04.\n\nThis simple bandana, which once covered the brow of an ancient... wait a minute, this is just a cheap replica made by a child playing hero.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");
            FakeHeroBandanaID = item.PickupObjectId;
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.AmmoCapacityMultiplier, 1.04f, StatModifier.ModifyMethod.MULTIPLICATIVE);

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.EXCLUDED;
        }

        public static void HandleSynergy(AdvancedSynergyEntry synergy)
        {
            if (synergy.NameKey == "#TRUEHERO")
            {
                if (synergy.OptionalItemIDs == null)
                {
                    synergy.OptionalItemIDs = new List<int>();
                }

                if (!synergy.OptionalItemIDs.Contains(FakeHeroBandanaID))
                {
                    synergy.OptionalItemIDs.Add(FakeHeroBandanaID);
                }
                else
                {
                    return;
                }

                if (!synergy.OptionalItemIDs.Contains(255))
                {
                    synergy.OptionalItemIDs.Add(255);
                }

                if (synergy.MandatoryItemIDs.Count != 2)
                {
                    ETGModConsole.Log(SynergyHelper.GenerateModEditedSynergyProblem(synergy, "Ancient Hero's Bandana", theItemName));
                }

                synergy.MandatoryItemIDs.RemoveAll((int x) => x == 255);
            }
            else
            {
                if (synergy.OptionalItemIDs != null && synergy.OptionalItemIDs.Contains(255) && !synergy.OptionalItemIDs.Contains(FakeHeroBandanaID))
                {
                    synergy.OptionalItemIDs.Add(FakeHeroBandanaID);
                }

                if (synergy.MandatoryItemIDs != null && synergy.MandatoryItemIDs.Contains(255))
                {
                    ETGModConsole.Log(SynergyHelper.GenerateModdedSynergyProblem(synergy, "Ancient Hero's Bandana", theItemName));
                }
            }
        }
    }
}