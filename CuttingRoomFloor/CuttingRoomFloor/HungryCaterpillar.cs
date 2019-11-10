using ItemAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CuttingRoomFloor
{
    class HungryCaterpillar
    {
        //template CaterpillarDevourHeartBehavior

        public static void Init()
        {
            //The name of the item
            string itemName = "Hungry Caterpillar";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "CuttingRoomFloor/Resources/hungry_caterpillar";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<CompanionItem>();
            item.CompanionGuid = "d375913a61d1465f8e4ffcf4894e4427";

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Hungry For Hearts";
            string longDesc = "There is a caterpillar following you.\n\nIt appears to be hungry for hearts.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.C;
        }

        public static int RequiredHearts = 4;

        public static bool IsHeartInRoom(Action<CaterpillarDevourHeartBehavior> orig, CaterpillarDevourHeartBehavior self)
        {
            AIActor m_aiActor = (AIActor)typeof(CaterpillarDevourHeartBehavior).GetField("m_aiActor", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(self);
            PlayerController companionOwner = m_aiActor.CompanionOwner;
            if (!companionOwner || companionOwner.CurrentRoom == null)
            {
                return false;
            }
            List<HealthPickup> componentsAbsoluteInRoom = companionOwner.CurrentRoom.GetComponentsAbsoluteInRoom<HealthPickup>();
            for (int i = 0; i < componentsAbsoluteInRoom.Count; i++)
            {
                HealthPickup healthPickup = componentsAbsoluteInRoom[i];
                if (healthPickup)
                {
                    if (healthPickup.armorAmount != 0)
                    {
                        componentsAbsoluteInRoom.RemoveAt(i);
                        i--;
                    }
                }
            }
            HealthPickup closestToPosition = BraveUtility.GetClosestToPosition<HealthPickup>(componentsAbsoluteInRoom, m_aiActor.CenterPosition, new HealthPickup[0]);
            if (closestToPosition != null)
            {
                typeof(CaterpillarDevourHeartBehavior).GetField("m_targetHeart", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(self, closestToPosition);
                return true;
            }
            return false;
        }
        
        public static void MunchHeart(Action<CaterpillarDevourHeartBehavior, PickupObject> orig, CaterpillarDevourHeartBehavior self, PickupObject targetHeart)
        {
            int heartValue = ((HealthPickup)targetHeart).healAmount == 0.5f ? 1 : 2;
            var hearts = typeof(CaterpillarDevourHeartBehavior).GetField("m_heartsMunched", BindingFlags.NonPublic | BindingFlags.Instance);
            hearts.SetValue(self, (int)hearts.GetValue(self) + heartValue);
            UnityEngine.Object.Destroy(targetHeart.gameObject);

            ((AIAnimator)typeof(CaterpillarDevourHeartBehavior).GetField("m_aiAnimator", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(self)).PlayUntilFinished("munch", false, null, -1f, false);
            
            if ((int)hearts.GetValue(self) >= RequiredHearts)
            {
                DoTransformation(self);
            }
        }
        
        private static void DoTransformation(CaterpillarDevourHeartBehavior self)
        {
            AIActor m_aiActor = (AIActor)typeof(CaterpillarDevourHeartBehavior).GetField("m_aiActor", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(self);
            PlayerController companionOwner = m_aiActor.CompanionOwner;
            if (companionOwner != null)
            {
                if (self.TransformVFX)
                {
                    SpawnManager.SpawnVFX(self.TransformVFX, m_aiActor.sprite.WorldBottomCenter, Quaternion.identity);
                }
                GameManager.Instance.StartCoroutine(DelayedGiveItem(m_aiActor.CompanionOwner, self));
                foreach (var item in m_aiActor.CompanionOwner.passiveItems)
                {
                    if (item is CompanionItem)
                    {
                        if (item != null && ((CompanionItem)item).CompanionGuid == m_aiActor.EnemyGuid && ((CompanionItem)item).ExtantCompanion == m_aiActor.gameObject)
                        {
                            m_aiActor.CompanionOwner.RemovePassiveItem(item.PickupObjectId);
                            break;
                        }
                    }
                }
            }
        }

        private static IEnumerator DelayedGiveItem(PlayerController targetPlayer, CaterpillarDevourHeartBehavior self)
        {
            yield return new WaitForSeconds(3.375f);
            if (targetPlayer && !targetPlayer.IsGhost)
            {
                PickupObject byId = PickupObjectDatabase.GetById(self.WingsItemIdToGive);
                if (byId != null)
                {
                    LootEngine.GivePrefabToPlayer(byId.gameObject, targetPlayer);
                }
            }
            yield break;
        }
    }
}
