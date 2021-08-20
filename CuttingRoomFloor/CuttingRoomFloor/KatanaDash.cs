using ItemAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoomFloor
{
    public class KatanaDash : PlayerItem
    {
        public static void Init()
        {
            //The name of the item
            string itemName = "Katana Dash";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "CuttingRoomFloor/Resources/katana_dash";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<KatanaDash>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Sword Of Doom";
            string longDesc = "Deliver three swift strikes before having to regain your energy.\n\nSwordplay is forbidden, and only the most powerful or most ignorant individuals can get away with using any melee weaponry.\n\nAngers the Jammed.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");
            item.AddPassiveStatModifier(PlayerStats.StatType.Curse, 1f);

            var smokeBomb = PickupObjectDatabase.GetById(462) as ConsumableStealthItem;

            item.poofVFX = smokeBomb.poofVfx;

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.D;
        }

        public float dashDistance = 6.5f;

        public float dashSpeed = 30f;

        public float swordDamage = 50f;

        public float afterUseBlankTime = 0.1f;

        public int sequentialValidUses = 3;

        public GameObject poofVFX;

        private bool m_isDashing;

        private int m_useCount;

        private readonly List<AIActor> actorsPassed = new List<AIActor>();

        private readonly List<MajorBreakable> breakablesPassed = new List<MajorBreakable>();

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            player.OnRoomClearEvent += ResetCooldown;
        }

        protected override void OnPreDrop(PlayerController user)
        {
            base.OnPreDrop(user);

            Cleanup(user);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Cleanup(LastOwner);
        }

        private void Cleanup(PlayerController player)
        {
            if (player)
            {
                player.OnRoomClearEvent -= ResetCooldown;
            }
        }

        public void ResetCooldown(PlayerController user)
        {
            this.m_useCount = 0;
            base.ClearCooldowns();
        }

        protected override void AfterCooldownApplied(PlayerController user)
        {
            if (this.m_useCount >= this.sequentialValidUses)
            {
                this.m_useCount = 0;
            }
            else
            {
                base.ClearCooldowns();
            }
        }

        public override void OnItemSwitched(PlayerController user)
        {
            base.OnItemSwitched(user);

            if (this.m_useCount > 0)
            {
                this.m_useCount = 0;
                base.ApplyCooldown(user);
            }
        }

        protected override void DoEffect(PlayerController user)
        {
            if (this.m_isDashing)
            {
                return;
            }

            Vector2 dashDirection = BraveInput.GetInstanceForPlayer(user.PlayerIDX).ActiveActions.Move.Vector;

            if (dashDirection.x == 0 && dashDirection.y == 0)
            {
                return;
            }

            AkSoundEngine.PostEvent("Play_CHR_ninja_dash_01", base.gameObject);
            this.m_useCount++;
            base.StartCoroutine(this.HandleDash(user, dashDirection));
        }

        private IEnumerator HandleDash(PlayerController user, Vector2 dashDirection)
        {
            this.m_isDashing = true;

            if (this.poofVFX != null)
            {
                user.PlayEffectOnActor(this.poofVFX, Vector3.zero, false, false, false);
            }

            Vector2 startPosition = user.sprite.WorldCenter;
            this.actorsPassed.Clear();
            this.breakablesPassed.Clear();
            user.IsVisible = false;
            user.SetInputOverride("katana");
            user.healthHaver.IsVulnerable = false;
            user.FallingProhibited = true;
            PixelCollider playerHitbox = user.specRigidbody.HitboxPixelCollider;
            playerHitbox.CollisionLayerCollidableOverride |= CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox);
            SpeculativeRigidbody specRigidbody = user.specRigidbody;
            specRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(specRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(this.KatanaPreCollision));
            float duration = Mathf.Max(0.0001f, this.dashDistance / this.dashSpeed);
            float elapsed = -BraveTime.DeltaTime;

            while (elapsed < duration)
            {
                user.healthHaver.IsVulnerable = false;
                elapsed += BraveTime.DeltaTime;
                float adjSpeed = Mathf.Min(this.dashSpeed, this.dashDistance / BraveTime.DeltaTime);
                user.specRigidbody.Velocity = dashDirection.normalized * adjSpeed;
                yield return null;
            }

            user.IsVisible = true;
            user.ToggleGunRenderers(false, "katana");

            base.renderer.enabled = true;
            base.transform.localPosition = new Vector3(-0.3f, -0.4f, 0f);
            base.transform.localRotation = new Quaternion(0, 0, 0, 1);

            if (this.poofVFX != null)
            {
                user.PlayEffectOnActor(this.poofVFX, Vector3.zero, false, false, false);
            }

            base.StartCoroutine(this.EndAndDamage(new List<AIActor>(this.actorsPassed), new List<MajorBreakable>(this.breakablesPassed), dashDirection, startPosition, user.sprite.WorldCenter));

            base.renderer.enabled = false;
            user.ToggleGunRenderers(true, "katana");
            playerHitbox.CollisionLayerCollidableOverride &= ~CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox);
            SpeculativeRigidbody specRigidbody2 = user.specRigidbody;
            specRigidbody2.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(specRigidbody2.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(this.KatanaPreCollision));
            user.FallingProhibited = false;
            user.ClearInputOverride("katana");
            user.healthHaver.IsVulnerable = true;

            if (this.afterUseBlankTime > 0f)
            {
                yield return DestroyEnemyBulletsInCircleForDuration(user.specRigidbody.UnitCenter, 2f, this.afterUseBlankTime);
            }

            this.m_isDashing = false;

            playerHitbox.CollisionLayerCollidableOverride &= ~CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox);

            yield break;
        }

        private IEnumerator DestroyEnemyBulletsInCircleForDuration(Vector2 center, float radius, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += BraveTime.DeltaTime;
                SilencerInstance.DestroyBulletsInRange(center, radius, true, false, null, false, null, false, null);
                yield return null;
            }
            yield break;
        }

        private IEnumerator EndAndDamage(List<AIActor> actors, List<MajorBreakable> breakables, Vector2 dashDirection, Vector2 startPosition, Vector2 endPosition)
        {
            yield return new WaitForSeconds(this.afterUseBlankTime);
            Exploder.DoLinearPush(endPosition, startPosition, 13f, 5f);

            float damageDone = this.swordDamage;

            GameLevelDefinition lastLoadedLevelDefinition = GameManager.Instance.GetLastLoadedLevelDefinition();

            if (lastLoadedLevelDefinition != null)
            {
                damageDone *= lastLoadedLevelDefinition.enemyHealthMultiplier;
            }

            for (int i = 0; i < actors.Count; i++)
            {
                if (actors[i])
                {
                    actors[i].healthHaver.ApplyDamage(damageDone, dashDirection, "Katana", CoreDamageTypes.None, DamageCategory.Normal, false, null, false);
                }
            }

            for (int j = 0; j < breakables.Count; j++)
            {
                if (breakables[j])
                {
                    breakables[j].ApplyDamage(100f, dashDirection, false, false, false);
                }
            }

            yield break;
        }

        private void KatanaPreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
        {
            if (otherRigidbody.projectile != null)
            {
                PhysicsEngine.SkipCollision = true;
            }

            if (otherRigidbody.aiActor != null)
            {
                PhysicsEngine.SkipCollision = true;

                if (!this.actorsPassed.Contains(otherRigidbody.aiActor))
                {
                    otherRigidbody.aiActor.DelayActions(1f);
                    this.actorsPassed.Add(otherRigidbody.aiActor);
                }
            }

            if (otherRigidbody.majorBreakable != null)
            {
                PhysicsEngine.SkipCollision = true;

                if (!this.breakablesPassed.Contains(otherRigidbody.majorBreakable))
                {
                    this.breakablesPassed.Add(otherRigidbody.majorBreakable);
                }
            }
        }
    }
}