using Dungeonator;
using ItemAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;

namespace CuttingRoomFloor
{
    class MonsterBall : PlayerItem
    {
        public static void Init()
        {
            //The name of the item
            string itemName = "Monster Ball";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "CuttingRoomFloor/Resources/monster_ball";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PlayerItem component to the object
            var item = obj.AddComponent<MonsterBall>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "100% Catch Rate";
            string longDesc = "Captures enemies and charms them upon release.\n\nAncient monster trainers tried to use these balls to domesticate inhabitants of the Gungeon.\n\nAccording to local legend, Emmitt Calx threw one of these at a Beholster. It didn't work. However, it did force the beast to blink, which allowed Calx to escape.\n";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.C;
        }

        public MonsterBall()
        {
            AutoPickup = true;
            EnemySearchRadius = 10f;
            m_containsEnemy = false;
            m_wasBlackPhantom = false;
            m_storedEnemyGuid = string.Empty;
            // Enable for debug output
            m_Debug = false;
        }

        public bool AutoPickup;
        public float EnemySearchRadius;

        public static string[] BannedMonsterBallEnemies =
        {
            "699cd24270af4cd183d671090d8323a1", // key_bullet_kin // It would just run a way and vanish...Not exactly useful. :P
            "a446c626b56d4166915a4e29869737fd", // chance_bullet_kin // Same reason as key bullet kin.
            "22fc2c2c45fb47cf9fb5f7b043a70122", // grip_master // Uhm...not even sure how this would work. But I know it would cause problems... :P
            "56f5a0f2c1fc4bc78875aea617ee31ac", // spectre
            // "9215d1a221904c7386b481a171e52859", // lead_maiden_fridge
            // "cd4a4b7f612a4ba9a720b9f97c52f38c", // lead_maiden
            "21dd14e5ca2a4a388adab5b11b69a1e1", // shelleton
            "1bc2a07ef87741be90c37096910843ab", // chancebulon // Attacks even in empty rooms.
            // "98ca70157c364750a60f5e0084f9d3e2", // phaser_spider
            "78eca975263d4482a4bfa4c07b32e252", // draguns_knife // Can't move. Stuck to wall. Not a useful companion.
            "2e6223e42e574775b56c6349921f42cb", // dragun_knife_advanced // Same reason as dragun's knife.
            "0d3f7c641557426fbac8596b61c9fb45", // lord_of_the_jammed // Ha ha.....no.
            "43426a2e39584871b287ac31df04b544", // wizbang // Doen't attack properly.
            "3e98ccecf7334ff2800188c417e67c15", // killithid // Attacks in empty rooms + his bullets still hurt the player.
            "19b420dec96d4e9ea4aebc3398c0ba7a", // bombshee // Doesn't really work.
            "116d09c26e624bca8cca09fc69c714b3", // poopulon // attacks in empty rooms.
            // "249db525a9464e5282d02162c88e0357", // spent
            // "0239c0680f9f467dbe5c4aab7dd1eca6", // blobulon
            // "042edb1dfb614dc385d5ad1b010f2ee3", // blobuloid
            // "fe3fe59d867347839824d5d9ae87f244", // poisbuloid
            // "e61cab252cfb435db9172adc96ded75f", // poisbulon
            // "1a4872dafdb34fd29fe8ac90bd2cea67", // king_bullat
            "e667fdd01f1e43349c03a18e5b79e579", // tutorial_turret // These...would not work for obvious reasons.
            "41ba74c517534f02a62f2e2028395c58", // faster_tutorial_turret // These...would not work for obvious reasons.
            // "3f6d6b0c4a7c4690807435c7b37c35a5", // agonizer
            // "d5a7b95774cd41f080e517bea07bf495", // revolvenant
            "88f037c3f93b4362a040a87b30770407", // gunreaper // Would self destruct on room clear. Not useful. (also is invincible so a bit too OP)
            "45192ff6d6cb43ed8f1a874ab6bef316", // misfire_beast // The secondary AIActor this spawns would attack the player. Not helpful
            "eeb33c3a5a8e4eaaaaf39a743e8767bc", // candle_guy // Can't move. Plus doesn't appear anywhere besides certain boss in challenge mode.
            "475c20c1fd474dfbad54954e7cba29c1", // tarnisher // Would have been interesting if this worked properly. :(
            "463d16121f884984abe759de38418e48", // chain_gunner // Chain attack kinda bugs out when captured. Also he does attacks even in empty room. So too annoying to use. :P
            "df4e9fedb8764b5a876517431ca67b86", // bullet_kin_gal_titan_boss // Too large to fit through door ways.
            "1f290ea06a4c416cabc52d6b3cf47266", // bullet_kin_titan_boss // Too large to fit through door ways.
            "c4cf0620f71c4678bb8d77929fd4feff", // bullet_kin_titan // Too large to fit through door ways.
            // "2b6854c0849b4b8fb98eb15519d7db1c", // bullet_kin_mech
            "c5a0fd2774b64287bf11127ca59dd8b4", // diagonal_x_det
            "b67ffe82c66742d1985e5888fd8e6a03", // vertical_det
            "d9632631a18849539333a92332895ebd", // diagonal_det
            "1898f6fe1ee0408e886aaf05c23cc216", // horizontal_det
            "abd816b0bcbf4035b95837ca931169df", // vertical_x_det
            "07d06d2b23cc48fe9f95454c839cb361", // horizontal_x_det
            "48d74b9c65f44b888a94f9e093554977", // x_det
            "6ad1cafc268f4214a101dca7af61bc91", // rat
            "14ea47ff46b54bb4a98f91ffcffb656d", // rat_candle
            "95ea1a31fc9e4415a5f271b9aedf9b15", // robots_past_critter_1
            "42432592685e47c9941e339879379d3a", // robots_past_critter_2
            "4254a93fc3c84c0dbe0a8f0dddf48a5a", // robots_past_critter_3
            "76bc43539fc24648bff4568c75c686d1", // chicken
            "1386da0f42fb4bcabc5be8feb16a7c38", // snake            
            "fa6a7ac20a0e4083a4c2ee0d86f8bbf7", // red_caped_bullet_kin
            // Companions should also be excluded
            "c07ef60ae32b404f99e294a6f9acba75", // dog
            "7bd9c670f35b4b8d84280f52a5cc47f6", // cucco
            "998807b57e454f00a63d67883fcf90d6", // portable_turret
            "11a14dbd807e432985a89f69b5f9b31e", // phoenix
            "6f9c28403d3248c188c391f5e40774c5", // turkey            
            "705e9081261446039e1ed9ff16905d04", // cop
            "640238ba85dd4e94b3d6f68888e6ecb8", // cop_android
            "3a077fa5872d462196bb9a3cb1af02a3", // super_space_turtle
            "1ccdace06ebd42dc984d46cb1f0db6cf", // r2g2
            "fe51c83b41ce4a46b42f54ab5f31e6d0", // pig
            "ededff1deaf3430eaf8321d0c6b2bd80", // hunters_past_dog
            "d375913a61d1465f8e4ffcf4894e4427", // caterpillar
            "5695e8ffa77c4d099b4d9bd9536ff35e", // blank_companion
            "c6c8e59d0f5d41969c74e802c9d67d07", // ser_junkan
            "86237c6482754cd29819c239403a2de7", // pig_synergy
            "ad35abc5a3bf451581c3530417d89f2c", // blank_companion_synergy
            "e9fa6544000942a79ad05b6e4afb62db", // raccoon
            "ebf2314289ff4a4ead7ea7ef363a0a2e", // dog_synergy_1
            "ab4a779d6e8f429baafa4bf9e5dca3a9", // dog_synergy_2
            "9216803e9c894002a4b931d7ea9c6bdf", // super_space_turtle_synergy
            "cc9c41aa8c194e17b44ac45f993dd212", // super_space_turtle_dummy
            "45f5291a60724067bd3ccde50f65ac22", // payday_shooter_01
            "41ab10778daf4d3692e2bc4b370ab037", // payday_shooter_02
            "2976522ec560460c889d11bb54fbe758", // payday_shooter_03
            "e456b66ed3664a4cb590eab3a8ff3814", // baby_mimic
            "3f40178e10dc4094a1565cd4fdc4af56" // baby_shelleton
        };

        public static string[] ContactDamageDealers =
        {
            "b5e699a0abb94666bda567ab23bd91c4", // bullet_kings_toadie
            "d4dd2b2bbda64cc9bcec534b4e920518", // bullet_kings_toadie_revenge
            "02a14dec58ab45fb8aacde7aacd25b01", // old_kings_toadie
            "d1c9781fdac54d9e8498ed89210a0238", // tiny_blobulord
            "98fdf153a4dd4d51bf0bafe43f3c77ff", // tazie
            "226fd90be3a64958a5b13cb0a4f43e97", // musket_kin
            "be0683affb0e41bbb699cb7125fdded6", // mouser
            "c2f902b7cbe745efb3db4399927eab34", // skusket_head
            "249db525a9464e5282d02162c88e0357", // spent
            "42be66373a3d4d89b91a35c9ff8adfec", // blobulin
            "0239c0680f9f467dbe5c4aab7dd1eca6", // blobulon
            "042edb1dfb614dc385d5ad1b010f2ee3", // blobuloid
            "fe3fe59d867347839824d5d9ae87f244", // poisbuloid
            "e61cab252cfb435db9172adc96ded75f", // poisbulon
            "b8103805af174924b578c98e95313074", // poispulin            
            "4538456236f64ea79f483784370bc62f", // fusebot
            "f155fd2759764f4a9217db29dd21b7eb", // mountain_cube
            "33b212b856b74ff09252bf4f2e8b8c57", // lead_cube
            "3f2026dc3712490289c4658a2ba4a24b", // flesh_cube
            "ba928393c8ed47819c2c5f593100a5bc", // metal_cube_guy
            "56f5a0f2c1fc4bc78875aea617ee31ac", // spectre
            "ec8ea75b557d4e7b8ceeaacdf6f8238c" // gun_nut
        };

        private bool m_containsEnemy;
        private bool m_wasBlackPhantom;
        private bool m_Debug;

        private string m_storedEnemyGuid;

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);
            if (this.m_containsEnemy)
            {
                base.IsCurrentlyActive = true;
                base.ClearCooldowns();
            }
        }

        protected override void DoEffect(PlayerController user)
        {
            if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES) { return; }
            DebrisObject debrisObject = user.DropActiveItem(this, 10f, false);
            if (debrisObject)
            {
                MonsterBall component = debrisObject.GetComponent<MonsterBall>();
                //component.spriteAnimator.Play("monster_ball_throw");
                component.m_containsEnemy = this.m_containsEnemy;
                component.m_storedEnemyGuid = this.m_storedEnemyGuid;
                DebrisObject debrisObject2 = debrisObject;
                debrisObject2.OnGrounded += this.HandleTossedBallGrounded;
            }
        }

        protected override void DoActiveEffect(PlayerController user)
        {
            if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES) { return; }
            DebrisObject debrisObject = user.DropActiveItem(this, 10f, false);
            if (debrisObject)
            {
                AIActor[] currentActors = FindObjectsOfType<AIActor>();
                if (currentActors != null && currentActors.Length > 0)
                {
                    foreach (AIActor actor in currentActors)
                    {
                        if (!string.IsNullOrEmpty(actor.name))
                        {
                            if (actor.name.ToLower().Contains("companionpet"))
                            {
                                LootEngine.DoDefaultItemPoof(actor.CenterPosition, false, false);
                                actor.EraseFromExistence(false);
                            }
                        }
                    }
                }
                MonsterBall component = debrisObject.GetComponent<MonsterBall>();
                // component.spriteAnimator.Play("monster_ball_throw");
                component.m_containsEnemy = this.m_containsEnemy;
                component.m_storedEnemyGuid = this.m_storedEnemyGuid;
                component.m_wasBlackPhantom = this.m_wasBlackPhantom;
                component.m_Debug = this.m_Debug;
                DebrisObject debrisObject2 = debrisObject;
                debrisObject2.OnGrounded += this.HandleActiveTossedBallGrounded;
            }
        }

        private void HandleTossedBallGrounded(DebrisObject obj)
        {
            obj.OnGrounded -= this.HandleTossedBallGrounded;
            MonsterBall component = obj.GetComponent<MonsterBall>();
            // component.spriteAnimator.Play("monster_ball_open");
            float distance = -1f;
            float nearestDistance = float.MaxValue;
            AIActor nearestEnemy = null;
            try
            {
                List<AIActor> activeEnemies = obj.transform.position.GetAbsoluteRoom().GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
                if (activeEnemies == null) { goto SKIP; }
                for (int i = 0; i < activeEnemies.Count; i++)
                {
                    AIActor enemy = activeEnemies[i];
                    if (!enemy.IsMimicEnemy && enemy.healthHaver && !enemy.healthHaver.IsBoss && enemy.healthHaver.IsVulnerable)
                    {
                        if (!enemy.healthHaver.IsDead)
                        {
                            if (!BannedMonsterBallEnemies.Contains(enemy.EnemyGuid))
                            {
                                float num = Vector2.Distance(obj.sprite.WorldCenter, enemy.CenterPosition);
                                if (num < nearestDistance)
                                {
                                    nearestDistance = num;
                                    nearestEnemy = enemy;
                                }
                            }
                        }
                    }
                }
            SKIP:
                if (nearestEnemy == null)
                {
                    AIActor[] AllEnemiesOnFloor = FindObjectsOfType<AIActor>();
                    if (AllEnemiesOnFloor == null)
                    {
                        if (component.m_Debug) { ETGModConsole.Log("[Monster_Ball] No Enemies present on the floor?"); }
                    }
                    else
                    {
                        for (int i = 0; i < AllEnemiesOnFloor.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(AllEnemiesOnFloor[i].name))
                            {
                                if (AllEnemiesOnFloor[i].name.ToLower().Contains("companionpet"))
                                {
                                    nearestEnemy = AllEnemiesOnFloor[i];
                                    nearestDistance = component.EnemySearchRadius;
                                }
                            }
                        }
                    }
                }
                if (component.m_Debug && nearestEnemy == null) { ETGModConsole.Log("[Monster_Ball] activeEnemies is null."); }
                if (nearestEnemy && distance <= component.EnemySearchRadius)
                {
                    if (component.m_Debug) { ETGModConsole.Log("Monster_Ball: Attempting to capture: " + nearestEnemy.GetActorName()); }
                    component.m_containsEnemy = true;
                    component.m_storedEnemyGuid = nearestEnemy.EnemyGuid;
                    component.m_wasBlackPhantom = nearestEnemy.IsBlackPhantom;
                    GameManager.Instance.StartCoroutine(SuckUpEnemy(nearestEnemy, obj, this.LastOwner));
                }
                else
                {
                    component.m_containsEnemy = false;
                    component.m_storedEnemyGuid = string.Empty;
                    component.m_wasBlackPhantom = false;
                    if (component.m_Debug && nearestEnemy == null)
                    {
                        ETGModConsole.Log("[Monster_Ball] No enemies in room!");
                    }
                    else if (component.m_Debug && nearestEnemy != null && distance > component.EnemySearchRadius)
                    {
                        ETGModConsole.Log("[Monster_Ball] No enemy in range!");
                    }
                    return;
                }
            }
            catch (Exception)
            {
                if (component.m_Debug)
                {
                    ETGModConsole.Log("[Monster Ball] Exception in HandleTossedBallGrounded!");
                }
                component.m_containsEnemy = false;
                component.m_storedEnemyGuid = string.Empty;
                component.m_wasBlackPhantom = false;
            }
        }

        private void HandleActiveTossedBallGrounded(DebrisObject obj)
        {
            obj.OnGrounded -= this.HandleActiveTossedBallGrounded;
            MonsterBall component = obj.GetComponent<MonsterBall>();
            //component.spriteAnimator.Play("monster_ball_open");
            AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(component.m_storedEnemyGuid);
            if (orLoadByGuid == null)
            {
                if (m_Debug) { ETGModConsole.Log("[Monster_Ball] Tried to spawn an unknown AIActor! (Enemy GUID not found)"); }
                component.m_containsEnemy = false;
                component.m_wasBlackPhantom = false;
                component.m_storedEnemyGuid = string.Empty;
                component.IsCurrentlyActive = false;
                component.ApplyCooldown(this.LastOwner);
                return;
            }
            IntVector2 bestRewardLocation = obj.transform.position.GetAbsoluteRoom().GetBestRewardLocation(orLoadByGuid.Clearance, obj.sprite.WorldCenter, true);
            AIActor m_CachedEnemy = AIActor.Spawn(orLoadByGuid, bestRewardLocation, obj.transform.position.GetAbsoluteRoom(), true, AIActor.AwakenAnimationType.Default, true);
            // m_CachedEnemy.ApplyEffect(GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultPermanentCharmEffect, 1f, null);
            this.MakeCompanion(this.LastOwner, m_CachedEnemy, component.m_wasBlackPhantom);
            LootEngine.DoDefaultItemPoof(m_CachedEnemy.CenterPosition, false, false);
            component.m_containsEnemy = false;
            component.m_storedEnemyGuid = string.Empty;
            component.IsCurrentlyActive = false;
            component.ApplyCooldown(this.LastOwner);
        }

        // Re-use code from baby dragun NPC to make Enemy get sucked into ball. Looks cooler then a simple poof effect. :D
        private IEnumerator SuckUpEnemy(AIActor targetEnemy, DebrisObject obj, PlayerController owner)
        {
            float elapsed = 0f;
            float duration = 0.5f;
            Vector3 startPos = targetEnemy.transform.position;
            Vector3 finalOffset = obj.sprite.WorldCenter - startPos.XY();
            tk2dBaseSprite targetSprite = targetEnemy.GetComponentInChildren<tk2dBaseSprite>();
            targetEnemy.behaviorSpeculator.InterruptAndDisable();
            Destroy(targetEnemy);
            Destroy(targetEnemy.specRigidbody);
            yield return null;
            AkSoundEngine.PostEvent("Play_NPC_BabyDragun_Munch_01", obj.gameObject);
            while (elapsed < duration)
            {
                elapsed += BraveTime.DeltaTime;
                if (!targetSprite || !targetSprite.transform)
                {
                    yield return null;
                }
                else
                {
                    targetSprite.transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.01f, 0.01f, 1f), elapsed / duration);
                    targetSprite.transform.position = Vector3.Lerp(startPos, startPos + finalOffset, elapsed / duration);
                }
                yield return null;
            }
            Destroy(targetSprite.gameObject);

            MonsterBall monsterBall = obj.GetComponent<MonsterBall>();
            if (monsterBall)
            {
                if (monsterBall.AutoPickup)
                {
                    yield return new WaitForSeconds(0.2f);
                    LootEngine.DoDefaultItemPoof(obj.transform.position, false, false);
                    monsterBall.Pickup(owner);
                }
            }
            yield break;
        }

        private void MakeCompanion(PlayerController owner, AIActor targetActor, bool makeBlackPhantom, AIActor sourceCompanionData = null)
        {
            if (sourceCompanionData == null) { sourceCompanionData = EnemyDatabase.GetOrLoadByGuid("3a077fa5872d462196bb9a3cb1af02a3"); }

            targetActor.behaviorSpeculator.MovementBehaviors.Add(sourceCompanionData.behaviorSpeculator.MovementBehaviors[0]);

            targetActor.CanTargetPlayers = false;
            targetActor.CanTargetEnemies = true;
            targetActor.IgnoreForRoomClear = true;
            targetActor.HitByEnemyBullets = true;
            targetActor.IsSignatureEnemy = false;
            targetActor.IsHarmlessEnemy = false;
            targetActor.RegisterOverrideColor(new Color(0.5f, 0, 0.5f), "Charm Effect");
            targetActor.name = "CompanionPet";
            targetActor.PreventAutoKillOnBossDeath = true;

            targetActor.gameObject.AddComponent<CompanionController>();
            CompanionController companionController = targetActor.gameObject.GetComponent<CompanionController>();
            companionController.CanInterceptBullets = true;
            companionController.IsCop = false;
            companionController.IsCopDead = false;
            companionController.CopDeathStatModifier = new StatModifier()
            {
                statToBoost = 0,
                modifyType = StatModifier.ModifyMethod.ADDITIVE,
                amount = 0
            };
            companionController.CurseOnCopDeath = 2;
            companionController.CanCrossPits = targetActor.IsFlying;
            companionController.BlanksOnActiveItemUsed = false;
            companionController.InternalBlankCooldown = 10;
            companionController.HasStealthMode = false;
            companionController.PredictsChests = false;
            companionController.PredictsChestSynergy = 0;
            companionController.CanBePet = false;
            companionController.companionID = CompanionController.CompanionIdentifier.NONE;
            companionController.TeaSynergyHeatRing = new HeatRingModule();
            companionController.m_petOffset = new Vector2(0, 0);

            // Do needed setup for Companion system. (makes enemy able to go through sealed doors, not damage player, etc)
            companionController.Initialize(owner);
            // Make things that deal damage via contact damage more useful. (they normally don't damage other enemies on their own) :P
            if (ContactDamageDealers.Contains(targetActor.EnemyGuid))
            {
                targetActor.OverrideHitEnemies = true;
                targetActor.CollisionDamage = 1f;
                targetActor.CollisionDamageTypes = CoreDamageTypes.Electric;
            }
            if (makeBlackPhantom) { targetActor.BecomeBlackPhantom(); }
        }

        public void HandleCompanionPostProcessProjectile(Action<CompanionController, Projectile> orig, CompanionController self, Projectile obj)
        {
            PlayerController m_owner = ReflectGetField<PlayerController>(typeof(CompanionController), "m_owner", self);
            if (obj)
            {
                obj.collidesWithPlayer = false;
                obj.TreatedAsNonProjectileForChallenge = true;
            }
            if (m_owner)
            {
                if (PassiveItem.IsFlagSetForCharacter(m_owner, typeof(BattleStandardItem)))
                {
                    obj.baseData.damage *= BattleStandardItem.BattleStandardCompanionDamageMultiplier;
                }
                if (m_owner.CurrentGun && m_owner.CurrentGun.LuteCompanionBuffActive)
                {
                    obj.baseData.damage *= 2f;
                    obj.RuntimeUpdateScale(1f / obj.AdditionalScaleMultiplier);
                    obj.RuntimeUpdateScale(1.75f);
                }
                // Prevent bullet modifiers from being applied to caught enemies. 
                // This causes nasty bugs like flak bullets being applied to other enemies on the floor!
                if (!string.IsNullOrEmpty(self.aiActor.name))
                {
                    if (self.aiActor.name.ToLower().Contains("companionpet"))
                    {
                        // Without the additioanl damage modifiers done from DoPostProcessProjectile Monster Ball enemies end up incredably weak.                        
                        if (!self.aiActor.IsBlackPhantom)
                        {
                            obj.baseData.damage *= 13f;
                        }
                        else
                        {
                            obj.baseData.damage *= 15f;
                        }
                        return;
                    }
                }
                m_owner.DoPostProcessProjectile(obj);
            }
        }

        public static T ReflectGetField<T>(Type classType, string fieldName, object o = null)
        {
            FieldInfo field = classType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | ((o != null) ? BindingFlags.Instance : BindingFlags.Static));
            return (T)field.GetValue(o);
        }
    }
}

