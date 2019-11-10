using ItemAPI;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace RobotReloaded
{
    public class RobotReloaded : ETGModule
    {
        public override void Init() { }

        public override void Exit() { }
        
        public override void Start()
        {
            new Hook(typeof(BasicStatPickup).GetMethod("Pickup", BindingFlags.Instance | BindingFlags.Public), typeof(RobotReloaded).GetMethod("PickupHook"));
            new Hook(typeof(Chest).GetMethod("OnBroken", BindingFlags.Instance | BindingFlags.NonPublic), typeof(RobotReloaded).GetMethod("OnBrokenHook"));
            
            FakePrefabHooks.Init();
            ItemBuilder.Init();
            RecycleItem.Init();
        }

        public static void OnBrokenHook(Action<Chest> orig, Chest self)
        {
            bool hasRecycleItem = false;
            foreach (var player in GameManager.Instance.AllPlayers)
            {
                foreach (var item in player.passiveItems)
                {
                    if(item is RecycleItem)
                        hasRecycleItem = true;
                }
            }
            if (hasRecycleItem)
            {
                float num = GameManager.Instance.RewardManager.ChestDowngradeChance;
                float num2 = GameManager.Instance.RewardManager.ChestHalfHeartChance;
                float num3 = GameManager.Instance.RewardManager.ChestExplosionChance;
                float num4 = GameManager.Instance.RewardManager.ChestJunkChance;
                float num5 = GameManager.Instance.RewardManager.HasKeyJunkMultiplier;
                float num6 = GameManager.Instance.RewardManager.HasJunkanJunkMultiplier;

                GameManager.Instance.RewardManager.ChestDowngradeChance = 0f;
                GameManager.Instance.RewardManager.ChestHalfHeartChance = 0f;
                GameManager.Instance.RewardManager.ChestExplosionChance = 0f;
                GameManager.Instance.RewardManager.ChestJunkChance = 1f;
                GameManager.Instance.RewardManager.HasKeyJunkMultiplier = 1f;
                GameManager.Instance.RewardManager.HasJunkanJunkMultiplier = 1f;
                orig(self);
                GameManager.Instance.RewardManager.ChestDowngradeChance = num;
                GameManager.Instance.RewardManager.ChestHalfHeartChance = num2;
                GameManager.Instance.RewardManager.ChestExplosionChance = num3;
                GameManager.Instance.RewardManager.ChestJunkChance = num4;
                GameManager.Instance.RewardManager.HasKeyJunkMultiplier = num5;
                GameManager.Instance.RewardManager.HasJunkanJunkMultiplier = num6;
            }
            else
            {
                orig(self);
            }
        }

        public static void PickupHook(Action<BasicStatPickup, PlayerController> orig, BasicStatPickup self, PlayerController player)
        {
            if (self.IsJunk)
            {
                bool pickedUpThisRun = (bool)(typeof(BasicStatPickup).GetField("m_pickedUpThisRun", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(self));
                bool hasRecycleItem = false;
                foreach (var item in player.passiveItems)
                {
                    if (item is RecycleItem)
                        hasRecycleItem = true;
                }
                if (!pickedUpThisRun && hasRecycleItem)
                {
                    StatModifier statModifier = new StatModifier();
                    statModifier.statToBoost = PlayerStats.StatType.Damage;
                    statModifier.amount = 0.05f;
                    statModifier.modifyType = StatModifier.ModifyMethod.ADDITIVE;
                    player.ownerlessStatModifiers.Add(statModifier);
                    player.stats.RecalculateStats(player, false, false);
                }
                self.IsJunk = false;
                orig(self, player);
                self.IsJunk = true;
            }
            else
            {
                orig(self, player);
            }
        }
    }
}
