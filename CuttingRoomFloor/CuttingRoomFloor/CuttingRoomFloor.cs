using ItemAPI;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace CuttingRoomFloor
{
    public class CuttingRoomFloor : ETGModule
    {
        public static readonly string MOD_NAME = "Cutting Room Floor";

        public static bool StarterSynergiesEnabled = false;

        public override void Init()
        {
            StarterSynergiesEnabled = PlayerPrefs.GetInt("CuttingRoomFloorStarterSynergiesEnabled", 1) == 1;
        }

        public override void Exit()
        {
        }

        public override void Start()
        {
            try
            {
                ETGModConsole.Commands.AddGroup("cuttingRoomFloor");
                ETGModConsole.Commands.GetGroup("cuttingRoomFloor").AddUnit("StarterSynergiesEnabled", delegate (string[] e)
                {
                // flips the bool value
                StarterSynergiesEnabled ^= true;
                    ETGModConsole.Log($"Starter synergies are now " + (StarterSynergiesEnabled ? "enabled" : "disabled") + ".");
                    PlayerPrefs.SetInt("CuttingRoomFloorStarterSynergiesEnabled", StarterSynergiesEnabled ? 1 : 0);
                    PlayerPrefs.Save();
                    try
                    {
                        SynergyHelper.UpdateStarterSynergyStatus(StarterSynergiesEnabled);
                    }
                    catch (System.Exception ex)
                    {
                        Tools.LogError("Exception whilst updating synergies: " + ex);
                    }
                });

                FakePrefabHooks.Init();
                ItemBuilder.Init();

                KatanaDash.Init();
                CueBullets.Init();
                ThirstForVengeance.Init();
                OldJournal.Init();
                HungryCaterpillar.Init();
                TableTechHole.Init();
                TableTechMirror.Init();
                Thunderbolt.Init();
                BrittleBullets.Init();
                BubbleShield.Init();
                MonsterBall.Init();

                //RingOfLightningResistance.Init();

                try
                {
                    new Hook(Tools.GetMethod(typeof(CaterpillarDevourHeartBehavior), "IsHeartInRoom"), typeof(HungryCaterpillar).GetMethod(nameof(HungryCaterpillar.IsHeartInRoom)));
                    new Hook(Tools.GetMethod(typeof(CaterpillarDevourHeartBehavior), "MunchHeart"), typeof(HungryCaterpillar).GetMethod(nameof(HungryCaterpillar.MunchHeart)));

                    new Hook(Tools.GetMethod(typeof(PlayerController), "RevengeRevive"), typeof(ThirstForVengeance).GetMethod(nameof(ThirstForVengeance.NoRevengeFullHeal)));

                    new Hook(Tools.GetMethod(typeof(CompanionController), "HandleCompanionPostProcessProjectile"), typeof(MonsterBall).GetMethod(nameof(MonsterBall.HandleCompanionPostProcessProjectile)), typeof(CompanionController));

                    // bug fix for dual wielding because people could run into it a lot with CRF and Beyond The Past when playing robot
                    new Hook(Tools.GetMethod(typeof(PlayerController), "OnGunChanged"), typeof(CuttingRoomFloor).GetMethod(nameof(CuttingRoomFloor.FixDualWieldGunChange)));
                }
                catch (System.Exception e)
                {
                    Tools.LogError("Exception whilst setting up hooks: " + e);
                }

                try
                {
                    SynergyHelper.EnableAndFixSynergies(StarterSynergiesEnabled);
                }
                catch (System.Exception e)
                {
                    Tools.LogError("Exception whilst setting up synergies: " + e);
                }

                Tools.Log($"{MOD_NAME} v{Metadata.Version} initialized (starter synergies: {(StarterSynergiesEnabled ? "Enabled" : "Disabled")})");
            }
            catch (System.Exception e)
            {
                Tools.LogError("Exception in Start: " + e);
            }
        }

        public delegate void Action<T1, T2, T3, T4, T5, T6>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6);

        public static void FixDualWieldGunChange(Action<PlayerController, Gun, Gun, Gun, Gun, bool> orig, PlayerController self, Gun previous, Gun current, Gun previousSecondary, Gun currentSecondary, bool newGun)
        {
            bool wasDualWielding = self.inventory.DualWielding;

            orig(self, previous, current, previousSecondary, currentSecondary, newGun);

            if (wasDualWielding && !self.inventory.DualWielding)
            {
                // fix hand position:
                self.ProcessHandAttachment();

                // fix hand layering:

                // unequip and requip without calling reequip for the erroneously assigned currentSecondary which is still the previousSecondary
                Tools.GetMethod(typeof(PlayerController), "HandleGunUnequipInternal").Invoke(self, new object[1] { current });
                Tools.GetMethod(typeof(PlayerController), "HandleGunUnequipInternal").Invoke(self, new object[1] { currentSecondary });
                Tools.GetMethod(typeof(PlayerController), "HandleGunEquipInternal").Invoke(self, new object[2] { current, self.primaryHand });

                // not calling this one as pointed out in the above comment
                // Tools.GetMethod(typeof(PlayerController), "HandleGunEquipInternal").Invoke(self, new object[2] { currentSecondary, self.secondaryHand });
            }
        }
    }
}