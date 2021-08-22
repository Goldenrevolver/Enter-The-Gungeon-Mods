using ItemAPI;
using UnityEngine;

namespace BeyondThePast
{
    public class EmbarrassingPhoto : RagePassiveItem
    {
        public static int EmbarrassingPhotoID;
        private static readonly string theItemName = "Embarrassing Photo";

        public static void Register()
        {
            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BeyondThePast/Resources/Group_Photo";

            //Create new GameObject
            GameObject obj = new GameObject(theItemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<EmbarrassingPhoto>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(theItemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Why Did I Do That...";
            string longDesc = "A photo that the Convict brought with her to the Gungeon. Deal extra damage for a short time after getting hit.\n\nShe often looks at the photo, longing to once again change her past.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");
            EmbarrassingPhotoID = item.PickupObjectId;

            var enragingPhoto = PickupObjectDatabase.GetById(353) as RagePassiveItem;

            foreach (var publicField in typeof(RagePassiveItem).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
            {
                publicField.SetValue(item, publicField.GetValue(enragingPhoto));
            }

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.EXCLUDED;
        }

        public static void HandleSynergy(AdvancedSynergyEntry synergy)
        {
            if (synergy.OptionalItemIDs != null && synergy.OptionalItemIDs.Contains(353) && !synergy.OptionalItemIDs.Contains(EmbarrassingPhotoID))
            {
                synergy.OptionalItemIDs.Add(EmbarrassingPhotoID);
            }

            if (synergy.MandatoryItemIDs != null && synergy.MandatoryItemIDs.Contains(353))
            {
                ETGModConsole.Log(SynergyHelper.GenerateModdedSynergyProblem(synergy, "Supply Drop", theItemName));
            }
        }
    }
}