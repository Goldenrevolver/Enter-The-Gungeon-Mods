using ItemAPI;
using UnityEngine;

namespace BeyondThePast
{
    public class EmptyBriefcase : PassiveItem
    {
        public static int EmptyBriefcaseID;

        public static void Register()
        {
            //The name of the item
            string itemName = "Empty Briefcase";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BeyondThePast/Resources/Empty_Briefcase";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<EmptyBriefcase>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Broke";
            string longDesc = "Increases active item capacity, but also increases prices at shops.\n\nA briefcase that once held a fortune. While the money is long gone, people regularly assume otherwise. At least it can still be used to carry other things.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");
            EmptyBriefcaseID = item.PickupObjectId;
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.AdditionalItemCapacity, 2, StatModifier.ModifyMethod.ADDITIVE);
            ItemBuilder.AddPassiveStatModifier(item, PlayerStats.StatType.GlobalPriceMultiplier, 1.1f, StatModifier.ModifyMethod.MULTIPLICATIVE);

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.EXCLUDED;
        }
    }
}