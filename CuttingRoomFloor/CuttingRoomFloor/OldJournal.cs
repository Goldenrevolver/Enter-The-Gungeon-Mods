using ItemAPI;
using UnityEngine;

namespace CuttingRoomFloor
{
    class OldJournal : PassiveItem
    {
        public static void Init()
        {
            //The name of the item
            string itemName = "Old Journal";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "CuttingRoomFloor/Resources/old_journal";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<OldJournal>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Luck Or Skill?";
            string longDesc = "Not taking damage slightly increases the chance for a room reward.\n\nFilled with maps and the answers to half-forgotten riddles. Made by an experienced explorer.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.C;
        }
        
        public float ChanceToFindItemOnRoomClear = 0.05f;
        
        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.OnRoomClearEvent += this.HandleRoomCleared;
        }
        
        private void HandleRoomCleared(PlayerController player)
        {
            if (player.CurrentRoom.PlayerHasTakenDamageInThisRoom)
            {
                return;
            }
            if (UnityEngine.Random.value < this.ChanceToFindItemOnRoomClear)
            {
                var rewardTable = GameManager.Instance.RewardManager.CurrentRewardData.SingleItemRewardTable;
                LootEngine.SpawnItem(rewardTable.SelectByWeight(false), player.CenterPosition, Vector2.up, 1f, true, false, false);
            }
        }
        
        public override DebrisObject Drop(PlayerController player)
        {
            DebrisObject debrisObject = base.Drop(player);
            player.OnRoomClearEvent -= this.HandleRoomCleared;
            return debrisObject;
        }
    }
}
