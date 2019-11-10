using ItemAPI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CuttingRoomFloor
{
    class Thunderbolt : AffectEnemiesInRoomItem
    {
        public static void Init()
        {
            //The name of the item
            string itemName = "Thunderbolt";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "CuttingRoomFloor/Resources/thunderbolt";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<Thunderbolt>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Shrinks And Dazes";
            string longDesc = "Shrinks and dazes enemies for a brief period of time.\n\nSaid to contain the soul of a famous Gungeoneer.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");
            ItemBuilder.SetCooldownType(item, ItemBuilder.CooldownType.Damage, 400f);

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.C;
        }
        
        public Vector2 TargetScale = new Vector2(0.5f, 0.5f);
        
        public float ShrinkTime = 0.1f;
        
        public float HoldTime = 3f;
        
        public float RegrowTime = 3f;
        
        public float DamageMultiplier = 2f;
        
        public bool DepixelatesTargets = true;

        private List<AIActor> affectedEnemies = new List<AIActor>();

        protected override void DoEffect(PlayerController user)
        {
            base.DoEffect(user);
            GameManager.Instance.StartCoroutine(this.HandleTime());
        }

        protected override void AffectEnemy(AIActor target)
        {
            if(target != null && target.healthHaver.IsVulnerable && !BannedThunderboltEnemies.Contains(target.EnemyGuid))
            {
                affectedEnemies.Add(target);
                GameManager.Instance.StartCoroutine(this.HandleShrink(target));
            }
        }

        private IEnumerator HandleTime()
        {
            this.m_isCurrentlyActive = true;
            while (affectedEnemies.Any())
            {
                yield return null;
            }
            this.m_isCurrentlyActive = false;
        }

        private IEnumerator HandleShrink(AIActor target)
        {
            AkSoundEngine.PostEvent("Play_OBJ_lightning_flash_01", base.gameObject);
            float elapsed = 0f;
            if (target == null)
            {
                affectedEnemies.Remove(target);
                yield break;
            }
            Vector2 startScale = target.EnemyScale;
            int cachedLayer = target.gameObject.layer;
            int cachedOutlineLayer = cachedLayer;
            if (this.DepixelatesTargets)
            {
                target.gameObject.layer = LayerMask.NameToLayer("Unpixelated");
                cachedOutlineLayer = SpriteOutlineManager.ChangeOutlineLayer(target.sprite, LayerMask.NameToLayer("Unpixelated"));
            }
            target.ClearPath();
            DazedBehavior db = new DazedBehavior();
            db.PointReachedPauseTime = 0.5f;
            db.PathInterval = 0.5f;
            if (target.knockbackDoer)
            {
                target.knockbackDoer.weight /= 3f;
            }
            if (target.healthHaver)
            {
                target.healthHaver.AllDamageMultiplier *= this.DamageMultiplier;
            }
            target.behaviorSpeculator.OverrideBehaviors.Insert(0, db);
            target.behaviorSpeculator.RefreshBehaviors();
            while (elapsed < this.ShrinkTime)
            {
                if (target == null)
                {
                    affectedEnemies.Remove(target);
                    yield break;
                }
                elapsed += target.LocalDeltaTime;
                target.EnemyScale = Vector2.Lerp(startScale, this.TargetScale, elapsed / this.ShrinkTime);
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < this.HoldTime)
            {
                this.m_activeElapsed = elapsed;
                this.m_activeDuration = this.HoldTime;
                if (target == null)
                {
                    affectedEnemies.Remove(target);
                    yield break;
                }
                elapsed += target.LocalDeltaTime;
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < this.RegrowTime)
            {
                if (target == null)
                {
                    affectedEnemies.Remove(target);
                    yield break;
                }
                elapsed += target.LocalDeltaTime;
                target.EnemyScale = Vector2.Lerp(this.TargetScale, startScale, elapsed / this.RegrowTime);
                yield return null;
            }
            if (target == null)
            {
                affectedEnemies.Remove(target);
                yield break;
            }
            if (target.knockbackDoer)
            {
                target.knockbackDoer.weight *= 3f;
            }
            if (target.healthHaver)
            {
                target.healthHaver.AllDamageMultiplier /= this.DamageMultiplier;
            }
            target.behaviorSpeculator.OverrideBehaviors.Remove(db);
            target.behaviorSpeculator.RefreshBehaviors();
            if (this.DepixelatesTargets)
            {
                target.gameObject.layer = cachedLayer;
                SpriteOutlineManager.ChangeOutlineLayer(target.sprite, cachedOutlineLayer);
            }
            affectedEnemies.Remove(target);
            yield break;
        }

        //private List<AIActor> filterThunderboltEnemies(List<AIActor> actors, string[] filter)
        //{
        //    if (actors == null || filter == null)
        //        return actors;
        //    List<AIActor> returnV = new List<AIActor>();
        //    foreach (var item in actors)
        //    {
        //        if (!filter.Contains(item.EnemyGuid))
        //        {
        //            returnV.Add(item);
        //        }
        //    }
        //    return returnV;
        //}

        public static string[] BannedThunderboltEnemies =
        {
            "699cd24270af4cd183d671090d8323a1", // key_bullet_kin // Flee behaviour generates an exception in the logs.
            "a446c626b56d4166915a4e29869737fd", // chance_bullet_kin // His drops sometimes don't appear correctly when resized.
            "22fc2c2c45fb47cf9fb5f7b043a70122", // grip_master // Being tossed from a room from tiny Grip Master can soft lock the game.
            "78eca975263d4482a4bfa4c07b32e252", // draguns_knife
            "2e6223e42e574775b56c6349921f42cb", // dragun_knife_advanced
            "0d3f7c641557426fbac8596b61c9fb45", // lord_of_the_jammed
        };
    }
}
