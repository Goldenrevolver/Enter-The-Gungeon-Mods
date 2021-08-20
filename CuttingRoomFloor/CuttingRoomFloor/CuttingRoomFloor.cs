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
            ETGModConsole.Commands.AddGroup("cuttingRoomFloor");
            ETGModConsole.Commands.GetGroup("cuttingRoomFloor").AddUnit("StarterSynergiesEnabled", delegate (string[] e)
            {
                // flips the bool value
                StarterSynergiesEnabled ^= true;
                ETGModConsole.Log($"Starter synergies are now " + (StarterSynergiesEnabled ? "enabled" : "disabled") + ".");
                PlayerPrefs.SetInt("CuttingRoomFloorStarterSynergiesEnabled", StarterSynergiesEnabled ? 1 : 0);
                PlayerPrefs.Save();
                SynergyHelper.UpdateStarterSynergyStatus(StarterSynergiesEnabled);
            });

            try
            {
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

                new Hook(Tools.GetMethod(typeof(CaterpillarDevourHeartBehavior), "IsHeartInRoom"), typeof(HungryCaterpillar).GetMethod(nameof(HungryCaterpillar.IsHeartInRoom)));
                new Hook(Tools.GetMethod(typeof(CaterpillarDevourHeartBehavior), "MunchHeart"), typeof(HungryCaterpillar).GetMethod(nameof(HungryCaterpillar.MunchHeart)));

                new Hook(Tools.GetMethod(typeof(PlayerController), "RevengeRevive"), typeof(ThirstForVengeance).GetMethod(nameof(ThirstForVengeance.NoRevengeFullHeal)));

                new Hook(Tools.GetMethod(typeof(CompanionController), "HandleCompanionPostProcessProjectile"), typeof(MonsterBall).GetMethod(nameof(MonsterBall.HandleCompanionPostProcessProjectile)), typeof(CompanionController));

                SynergyHelper.EnableAndFixSynergies(StarterSynergiesEnabled);

                Tools.Log($"{MOD_NAME} v{Metadata.Version} initialized, starter synergies: {(StarterSynergiesEnabled ? "enabled" : "disabled")}");
            }
            catch (System.Exception e)
            {
                Tools.LogError("Exception in Start: " + e);
            }
        }
    }
}