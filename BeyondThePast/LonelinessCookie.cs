using ItemAPI;
using UnityEngine;

namespace BeyondThePast
{
    public class LonelinessCookie : RationItem
    {
        public static int LonelinessCookieID;
        private static readonly string theItemName = "Loneliness Cookie";

        public static void Register()
        {
            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BeyondThePast/Resources/Loneliness_Cookie";

            //Create new GameObject
            GameObject obj = new GameObject(theItemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<LonelinessCookie>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(theItemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "It's Delicious!";
            string longDesc = "Heals for one full heart. Automatically used if you run out of health.\n\nBaked fresh every morning by Mom! It's to die for! Or, just maybe, to live for... if only you could share them.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");
            LonelinessCookieID = item.PickupObjectId;

            var friendshipCookie = PickupObjectDatabase.GetById(412) as HealPlayerItem;

            item.healingAmount = 1f;
            item.healVFX = friendshipCookie.healVFX;

            item.damageCooldown = friendshipCookie.damageCooldown;
            item.roomCooldown = friendshipCookie.roomCooldown;
            item.timeCooldown = friendshipCookie.timeCooldown;

            item.consumable = friendshipCookie.consumable;
            item.consumableOnActiveUse = friendshipCookie.consumableOnActiveUse;
            item.numberOfUses = friendshipCookie.numberOfUses;

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.EXCLUDED;
        }

        public static void HandleSynergy(AdvancedSynergyEntry synergy)
        {
            if (synergy.OptionalItemIDs != null && synergy.OptionalItemIDs.Contains(412) && !synergy.OptionalItemIDs.Contains(LonelinessCookieID))
            {
                synergy.OptionalItemIDs.Add(LonelinessCookieID);
            }

            if (synergy.MandatoryItemIDs != null && synergy.MandatoryItemIDs.Contains(412))
            {
                ETGModConsole.Log(SynergyHelper.GenerateModdedSynergyProblem(synergy, "Friendship Cookie", theItemName));
            }
        }
    }
}