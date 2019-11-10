using ItemAPI;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace CuttingRoomFloor
{
    public class CuttingRoomFloor : ETGModule
    {
        public override void Init() { }

        public override void Exit() { }

        public override void Start()
        {
            FakePrefabHooks.Init();
            ItemBuilder.Init();

            KatanaDash.Init();
            CueBullets.Init();
            ThirstForVengeance.Init();
            OldJournal.Init();
            //RingOfLightningResistance.Init();
            HungryCaterpillar.Init();
            TableTechHole.Init();
            TableTechMirror.Init();
            Thunderbolt.Init();
            BrittleBullets.Init();
            BubbleShield.Init();
            MonsterBall.Init();

            //e9fa6544000942a79ad05b6e4afb62db

            new Hook(typeof(CaterpillarDevourHeartBehavior).GetMethod("IsHeartInRoom", BindingFlags.NonPublic | BindingFlags.Instance), typeof(HungryCaterpillar).GetMethod("IsHeartInRoom"));
            new Hook(typeof(CaterpillarDevourHeartBehavior).GetMethod("MunchHeart", BindingFlags.NonPublic | BindingFlags.Instance), typeof(HungryCaterpillar).GetMethod("MunchHeart"));

            new Hook(typeof(PlayerController).GetMethod("InitializeCallbacks", BindingFlags.NonPublic | BindingFlags.Instance), typeof(ThirstForVengeance).GetMethod("InitializeCallbacks"));
            new Hook(typeof(PlayerController).GetMethod("RevengeRevive", BindingFlags.NonPublic | BindingFlags.Instance), typeof(ThirstForVengeance).GetMethod("NoRevengeFullHeal"));

            new Hook(typeof(CompanionController).GetMethod("HandleCompanionPostProcessProjectile", BindingFlags.NonPublic | BindingFlags.Instance), typeof(MonsterBall).GetMethod("HandleCompanionPostProcessProjectile", BindingFlags.Public | BindingFlags.Instance), typeof(CompanionController));
        }
    }
}
