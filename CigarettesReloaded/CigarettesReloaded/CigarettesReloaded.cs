using Dungeonator;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using UnityEngine;

namespace CigarettesReloaded
{
    public class CigarettesReloaded : ETGModule
    {
        private static readonly string version = "1.0";

        public static int cigarettesUsed;

        //init is too early for using ETGModConsole
        public override void Init() { }

        //exit is not getting called at all, so I have to save settings when I change them
        public override void Exit() { }

        public override void Start()
        {
            ETGModConsole.Log($"CigarettesReloaded v{version} initialized");
            new Hook(typeof(SpawnObjectPlayerItem).GetMethod("CanBeUsed", BindingFlags.Public | BindingFlags.Instance), typeof(CigarettesReloaded).GetMethod("CanBeUsedHook"));
            new Hook(typeof(SpawnObjectPlayerItem).GetMethod("DoEffect", BindingFlags.NonPublic | BindingFlags.Instance), typeof(CigarettesReloaded).GetMethod("DoEffectHook"));
        }

        public static bool CanBeUsedHook(Action<SpawnObjectPlayerItem, PlayerController> baseMethod, SpawnObjectPlayerItem self, PlayerController user)
        {
            //baseMethod is not called at all as I can't get its return value
            if (self.IsCigarettes)
            {
                if (user.characterIdentity == PlayableCharacters.Robot)
                    return false;
                if (user.CurrentGun && user.CurrentGun.IsReloading && user.CurrentGun.ClipShotsRemaining == 0 && !user.CurrentGun.IsHeroSword)
                {
                    foreach (PassiveItem item in user.passiveItems)
                    {
                        if (item is CrisisStoneItem)
                            return false;
                    }
                }
            }

            //calling the overridden base method: PlayerItem.CanBeUsed is actually always null but this is good practise
            var method = typeof(PlayerItem).GetMethod("CanBeUsed", BindingFlags.Public | BindingFlags.Instance);
            var ftn = method.MethodHandle.GetFunctionPointer();
            var func = (Func<bool>)Activator.CreateInstance(typeof(Func<bool>), self, ftn);
            bool b = func();

            return (!self.IsCigarettes || !user || !user.healthHaver || user.healthHaver.IsVulnerable) && (!self.RequireEnemiesInRoom || !user || user.CurrentRoom == null || user.CurrentRoom.GetActiveEnemiesCount(RoomHandler.ActiveEnemyType.All) != 0) && b;
        }

        public static void DoEffectHook(Action<SpawnObjectPlayerItem, PlayerController> baseMethod, SpawnObjectPlayerItem self, PlayerController user)
        {
            if (self.IsCigarettes)
            {
                cigarettesUsed++;
                float damage = 0.5f * cigarettesUsed;

                bool dRoom = false;
                bool dRun = user.HasTakenDamageThisRun;
                bool dFloor = user.HasTakenDamageThisFloor;
                if (user.CurrentRoom != null)
                {
                    dRoom = user.CurrentRoom.PlayerHasTakenDamageInThisRoom;
                }

                user.healthHaver.NextDamageIgnoresArmor = true;
                user.healthHaver.ApplyDamage(damage, Vector2.zero, StringTableManager.GetEnemiesString("#SMOKING", -1), CoreDamageTypes.None, DamageCategory.Normal, true, null, false);

                user.HasTakenDamageThisRun = dRun;
                user.HasTakenDamageThisFloor = dFloor;
                if (user.CurrentRoom != null)
                {
                    user.CurrentRoom.PlayerHasTakenDamageInThisRoom = dRoom;
                }

                StatModifier statModifier = new StatModifier();
                statModifier.statToBoost = PlayerStats.StatType.Coolness;
                statModifier.modifyType = StatModifier.ModifyMethod.ADDITIVE;
                statModifier.amount = 1f;
                user.ownerlessStatModifiers.Add(statModifier);
                user.stats.RecalculateStats(user, false, false);

                self.IsCigarettes = false;
                baseMethod(self, user);
                self.IsCigarettes = true;
            }
            else
            {
                baseMethod(self, user);
            }
        }
    }
}

