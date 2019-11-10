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
            string longDesc = "Deliver three swift strikes before having to regain your energy.\n\nAs a famous ninja once said: \"You're already dead, you just haven't caught up yet.\"";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.D;
        }
        
        public float dashDistance = 10f;
        
        public float dashSpeed = 30f;
        
        public float collisionDamage = 50f;
        
        public float finalDelay = 0.25f;
        
        public int sequentialValidUses = 3;

        public GameObject poofVFX;

        private bool m_isDashing;
        
        private int m_useCount;
        
        private List<AIActor> actorsPassed = new List<AIActor>();
        
        private List<MajorBreakable> breakablesPassed = new List<MajorBreakable>();

        public override void Pickup(PlayerController player)
        {
            foreach (var item in Gungeon.Game.Items.Entries)
            {
                if (item is ConsumableStealthItem)
                {
                    this.poofVFX = ((ConsumableStealthItem)item).poofVfx;
                    break;
                }
            }
            base.Pickup(player);
            player.OnRoomClearEvent += ResetCooldown;
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
            base.StartCoroutine(this.EndAndDamage(new List<AIActor>(this.actorsPassed), new List<MajorBreakable>(this.breakablesPassed), user, dashDirection, startPosition, user.sprite.WorldCenter));
            if (this.finalDelay > 0f)
            {
                user.healthHaver.IsVulnerable = false;
                yield return new WaitForSeconds(this.finalDelay);
            }
            user.healthHaver.IsVulnerable = true;
            base.renderer.enabled = false;
            user.ToggleGunRenderers(true, "katana");
            playerHitbox.CollisionLayerCollidableOverride &= ~CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox);
            SpeculativeRigidbody specRigidbody2 = user.specRigidbody;
            specRigidbody2.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(specRigidbody2.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(this.KatanaPreCollision));
            user.FallingProhibited = false;
            user.ClearInputOverride("katana");
            this.m_isDashing = false;
            yield break;
        }
        
        private IEnumerator EndAndDamage(List<AIActor> actors, List<MajorBreakable> breakables, PlayerController user, Vector2 dashDirection, Vector2 startPosition, Vector2 endPosition)
        {
            yield return new WaitForSeconds(this.finalDelay);
            Exploder.DoLinearPush(user.sprite.WorldCenter, startPosition, 13f, 5f);
            for (int i = 0; i < actors.Count; i++)
            {
                if (actors[i])
                {
                    actors[i].healthHaver.ApplyDamage(this.collisionDamage, dashDirection, "Katana", CoreDamageTypes.None, DamageCategory.Normal, false, null, false);
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
