using ItemAPI;
using UnityEngine;

namespace CuttingRoomFloor
{
    internal class TableTechMirror : PassiveItem
    {
        public static void Init()
        {
            //The name of the item
            string itemName = "Table Tech Mirror";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "CuttingRoomFloor/Resources/tabletech_mirror";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<TableTechMirror>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Flip Back";
            string longDesc = "This ancient technique causes flipped tables to reflect incoming projectiles back at its owner.\n\nAppendix F of the \"Tabla Sutra.\" Flipping a table shows you an image of your true self.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");
            item.AddItemToSynergy("#PAPERWORK");

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.D;
        }

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);

            player.OnTableFlipCompleted += HandleFlip;
        }

        public override DebrisObject Drop(PlayerController player)
        {
            var drop = base.Drop(player);

            Cleanup(player);

            return drop;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Cleanup(Owner);
        }

        private void Cleanup(PlayerController player)
        {
            if (player)
            {
                player.OnTableFlipCompleted -= HandleFlip;
            }
        }

        private void HandleFlip(FlippableCover table)
        {
            table.specRigidbody.OnPreRigidbodyCollision += OnPreCollision;
        }

        private void OnPreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherCollider)
        {
            Projectile component = otherRigidbody.GetComponent<Projectile>();

            if (component != null && !(component.Owner is PlayerController))
            {
                ReflectBullet(component, true, this.Owner, 10f, 1f, 1f, 0f);
                PhysicsEngine.SkipCollision = true;
            }
        }

        public static void ReflectBullet(Projectile p, bool retargetReflectedBullet, GameActor newOwner, float minReflectedBulletSpeed, float scaleModifier = 1f, float damageModifier = 1f, float spread = 0f)
        {
            p.RemoveBulletScriptControl();
            AkSoundEngine.PostEvent("Play_OBJ_metalskin_deflect_01", GameManager.Instance.gameObject);

            if (retargetReflectedBullet && p.Owner && p.Owner.specRigidbody)
            {
                p.Direction = (p.Owner.specRigidbody.GetUnitCenter(ColliderType.HitBox) - p.specRigidbody.UnitCenter).normalized;
            }

            if (spread != 0f)
            {
                p.Direction = p.Direction.Rotate(Random.Range(-spread, spread));
            }

            if (p.Owner && p.Owner.specRigidbody)
            {
                p.specRigidbody.DeregisterSpecificCollisionException(p.Owner.specRigidbody);
            }

            p.Owner = newOwner;
            p.SetNewShooter(newOwner.specRigidbody);
            p.allowSelfShooting = false;
            p.collidesWithPlayer = false;
            p.collidesWithEnemies = true;

            if (scaleModifier != 1f)
            {
                SpawnManager.PoolManager.Remove(p.transform);
                p.RuntimeUpdateScale(scaleModifier);
            }

            if (p.Speed < minReflectedBulletSpeed)
            {
                p.Speed = minReflectedBulletSpeed;
            }

            if (p.baseData.damage < ProjectileData.FixedFallbackDamageToEnemies)
            {
                p.baseData.damage = ProjectileData.FixedFallbackDamageToEnemies;
            }

            p.baseData.damage *= damageModifier;

            if (p.baseData.damage < 10f)
            {
                p.baseData.damage = 15f;
            }

            p.UpdateCollisionMask();
            p.Reflected();
            p.SendInDirection(p.Direction, true, true);
        }
    }
}