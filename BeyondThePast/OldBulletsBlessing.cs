using ItemAPI;
using System.Collections.Generic;
using UnityEngine;

namespace BeyondThePast
{
    public class OldBulletsBlessing : PassiveItem
    {
        public static int OldBulletsBlessingID;

        // I know shield_of_the_maiden is not a direct zelda reference
        // bomb, boomerang, shield_of_the_maiden, bottle, grappling_hook
        private static readonly List<int> possibleItems = new List<int>() { 108, 448, 447, 558, 250 };

        public static void Register()
        {
            //The name of the item
            string itemName = "Old Bullet's Blessing";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BeyondThePast/Resources/Old_Bullets_Blessing";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<OldBulletsBlessing>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Aim True, My...";
            string longDesc = "Every floor you gain a random item from a select range, that you can only use on this floor. The first two chests you open every floor will also contain the Map and the Pocket Compass (does not work on mimics).\n\nThe Bullet still remembers the words of his late mentor.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");
            OldBulletsBlessingID = item.PickupObjectId;

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.EXCLUDED;
        }

        [SerializeField]
        private bool givenMapThisFloor = false;

        [SerializeField]
        private bool givenCompassThisFloor = false;

        // 109 is the ice bomb, just a sensible fallback item in case something breaks
        [SerializeField]
        private int itemIDGivenLastFloor = 109;

        [SerializeField]
        private int lastCheckedFloor = -1;

        public override void Pickup(PlayerController player)
        {
            if (this.m_pickedUp)
            {
                return;
            }

            // just in case
            if (ETGMod.Chest.OnPostOpen == null)
            {
                ETGMod.Chest.OnPostOpen = (Chest c, PlayerController p) => { };
            }

            ETGMod.Chest.OnPostOpen += OnOpenChest;
            player.OnNewFloorLoaded += OnNewFloor;

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
            base.OnDestroy();

            Cleanup(Owner);
        }

        private void Cleanup(PlayerController player)
        {
            ETGMod.Chest.OnPostOpen -= OnOpenChest;

            if (player)
            {
                player.OnNewFloorLoaded -= OnNewFloor;
            }
        }

        private void OnNewFloor(PlayerController player)
        {
            if (!player || lastCheckedFloor == GameManager.Instance.CurrentFloor || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
            {
                return;
            }

            lastCheckedFloor = GameManager.Instance.CurrentFloor;

            givenMapThisFloor = false;
            givenCompassThisFloor = false;

            DestroyAllZeldaItems(player);

            // check if there is space left to add the item
            if (player.activeItems.Count < player.maxActiveItemsHeld)
            {
                GiveRandomZeldaItem(player);
            }
        }

        private void GiveRandomZeldaItem(PlayerController player)
        {
            int newItem = itemIDGivenLastFloor;

            // find non duplicate item
            while (newItem == itemIDGivenLastFloor)
            {
                newItem = possibleItems[Random.Range(0, possibleItems.Count)];
            }

            itemIDGivenLastFloor = newItem;

            GiveActiveZeldaItem(player, newItem);

            foreach (var item in player.activeItems)
            {
                if (item && item.GetComponent<ZeldaItemTracker>())
                {
                    item.CanBeSold = false;

                    // fill the bottle
                    if (item.PickupObjectId == 558)
                    {
                        var bottle = item as EmptyBottleItem;
                        bottle.Contents = EmptyBottleItem.EmptyBottleContents.HALF_HEART;
                    }
                }
            }
        }

        private void GiveActiveZeldaItem(PlayerController player, int id)
        {
            PlayerItem playerItem = PickupObjectDatabase.GetById(id) as PlayerItem;

            // add tracker to the prefab
            playerItem.gameObject.AddComponent<ZeldaItemTracker>();

            EncounterTrackable.SuppressNextNotification = true;
            playerItem.Pickup(player);
            EncounterTrackable.SuppressNextNotification = false;

            // remove tracker from the prefab again so only the instance has it
            Object.Destroy(playerItem.gameObject.GetComponent<ZeldaItemTracker>());
        }

        private void DestroyAllZeldaItems(PlayerController player)
        {
            for (int i = player.passiveItems.Count - 1; i >= 0; i--)
            {
                var item = player.passiveItems[i];

                if (item && item.GetComponent<ZeldaItemTracker>())
                {
                    var deb = player.DropPassiveItem(item);
                    Object.Destroy(deb.gameObject);
                }
            }

            for (int i = player.activeItems.Count - 1; i >= 0; i--)
            {
                var item = player.activeItems[i];

                if (item && item.GetComponent<ZeldaItemTracker>())
                {
                    var deb = player.DropActiveItem(item);
                    Object.Destroy(deb.gameObject);
                }
            }
        }

        private void OnOpenChest(Chest chest, PlayerController player)
        {
            if (this.Owner && player == this.Owner)
            {
                if (chest && !chest.IsMimic)
                {
                    if (!givenMapThisFloor && !givenCompassThisFloor)
                    {
                        // 50 50 chance (int range, so 2 is exclusive)
                        if (Random.Range(0, 2) == 0)
                        {
                            GiveMap(player);
                        }
                        else
                        {
                            GiveCompass(player);
                        }

                        return;
                    }

                    if (!givenMapThisFloor)
                    {
                        GiveMap(player);

                        return;
                    }

                    if (!givenCompassThisFloor)
                    {
                        GiveCompass(player);

                        return;
                    }
                }
            }
        }

        private void GiveCompass(PlayerController player)
        {
            givenCompassThisFloor = true;

            // does not need a tracker, it will destroy itself
            PlayerItem playerItem = PickupObjectDatabase.GetById(CompassItem.CompassID) as PlayerItem;

            EncounterTrackable.SuppressNextNotification = true;
            playerItem.Pickup(player);
            EncounterTrackable.SuppressNextNotification = false;
        }

        private void GiveMap(PlayerController player)
        {
            givenMapThisFloor = true;

            player.EverHadMap = true;
            player.stats.RecalculateStats(player, false, false);

            if (Minimap.Instance != null)
            {
                Minimap.Instance.RevealAllRooms(true);
            }
        }
    }

    public class ZeldaItemTracker : MonoBehaviour
    {
    }
}