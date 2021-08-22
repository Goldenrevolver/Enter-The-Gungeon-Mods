using ItemAPI;
using System.Collections.Generic;
using UnityEngine;

namespace BeyondThePast
{
    public class CompassItem : SenseOfDirectionItem
    {
        public static int CompassID;
        private static readonly string theItemName = "Light Compass";

        public static void Register()
        {
            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BeyondThePast/Resources/Compass";

            //Create new GameObject
            GameObject obj = new GameObject(theItemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<CompassItem>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(theItemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Surprisingly Common";
            string longDesc = "Strangely, the needle in this compass always points toward the descending elevator on this specific floor. It's so small and light, that it doesn't really take up any space.\n\nThe words \"Cannon was here\" are engraved upon the back.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");
            item.AddPassiveStatModifier(PlayerStats.StatType.AdditionalItemCapacity, 1f, StatModifier.ModifyMethod.ADDITIVE);
            CompassID = item.PickupObjectId;

            item.CanBeSold = false;

            var senseOfDirection = PickupObjectDatabase.GetById(209) as SenseOfDirectionItem;

            item.arrowVFX = senseOfDirection.arrowVFX;

            item.damageCooldown = senseOfDirection.damageCooldown;
            item.roomCooldown = senseOfDirection.roomCooldown;
            item.timeCooldown = senseOfDirection.timeCooldown;

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.EXCLUDED;

            GameManager.Instance.OnNewLevelFullyLoaded += RemoveOnFloorChange;
        }

        public static void HandleSynergy(AdvancedSynergyEntry synergy)
        {
            if (synergy.NameKey == "#EXPLORER")
            {
                if (synergy.OptionalItemIDs == null)
                {
                    synergy.OptionalItemIDs = new List<int>();
                }

                if (!synergy.OptionalItemIDs.Contains(CompassID))
                {
                    synergy.OptionalItemIDs.Add(CompassID);
                }
                else
                {
                    return;
                }

                if (!synergy.OptionalItemIDs.Contains(209))
                {
                    synergy.OptionalItemIDs.Add(209);
                }

                if (synergy.MandatoryItemIDs.Count != 1)
                {
                    ETGModConsole.Log(SynergyHelper.GenerateModEditedSynergyProblem(synergy, "Sense of Direction", theItemName));
                }

                synergy.MandatoryItemIDs.RemoveAll((int x) => x == 209);
            }
            else
            {
                if (synergy.OptionalItemIDs != null && synergy.OptionalItemIDs.Contains(209) && !synergy.OptionalItemIDs.Contains(CompassID))
                {
                    synergy.OptionalItemIDs.Add(CompassID);
                }

                if (synergy.MandatoryItemIDs != null && synergy.MandatoryItemIDs.Contains(209))
                {
                    ETGModConsole.Log(SynergyHelper.GenerateModdedSynergyProblem(synergy, "Sense of Direction", theItemName));
                }
            }
        }

        public static void RemoveOnFloorChange()
        {
            foreach (var player in GameManager.Instance.AllPlayers)
            {
                if (player)
                {
                    for (int i = player.activeItems.Count - 1; i >= 0; i--)
                    {
                        var item = player.activeItems[i];

                        if (item is CompassItem)
                        {
                            var deb = player.DropActiveItem(item);
                            Object.Destroy(deb.gameObject);
                        }
                    }
                }
            }
        }
    }
}