using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;

namespace BetterBeholsterShrine
{
    public class BetterBeholsterShrine : ETGModule
    {
        public static readonly string MOD_NAME = "Better Beholster Shrine";

        public override void Init()
        {
        }

        public override void Exit()
        {
        }

        public override void Start()
        {
            try
            {
                ETGModConsole.Commands.AddGroup("beholsterShrine");
                ETGModConsole.Commands.GetGroup("beholsterShrine").AddUnit("cheat", delegate (string[] e)
                {
                    Cheat();
                }).AddUnit("uncheat", delegate (string[] e)
                {
                    Uncheat();
                });

                try
                {
                    new Hook(typeof(BeholsterShrineController).GetMethod("DoShrineEffect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(BetterBeholsterShrine).GetMethod(nameof(DoShrineEffectHook)));
                    new Hook(typeof(BeholsterShrineController).GetMethod("CheckCanBeUsed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(BetterBeholsterShrine).GetMethod(nameof(CheckCanBeUsedHook)));
                }
                catch (Exception e)
                {
                    ETGModConsole.Log($"<color=red>Exception whilst setting up hooks: {e}</color>");
                }

                ETGModConsole.Log($"{MOD_NAME} v{Metadata.Version} initialized");
            }
            catch (Exception e)
            {
                ETGModConsole.Log($"<color=red>Exception in Start: {e}</color>");
            }
        }

        private static readonly List<GungeonFlags> flags = new List<GungeonFlags>
        {
            GungeonFlags.SHRINE_BEHOLSTER_GUN_01,
            GungeonFlags.SHRINE_BEHOLSTER_GUN_02,
            GungeonFlags.SHRINE_BEHOLSTER_GUN_03,
            GungeonFlags.SHRINE_BEHOLSTER_GUN_04,
            GungeonFlags.SHRINE_BEHOLSTER_GUN_05,
            GungeonFlags.SHRINE_BEHOLSTER_GUN_06
        };

        private void Cheat()
        {
            foreach (var item in flags)
            {
                GameStatsManager.Instance.SetFlag(item, true);
            }
        }

        private void Uncheat()
        {
            foreach (var item in flags)
            {
                GameStatsManager.Instance.SetFlag(item, false);
            }
        }

        // we don't call orig at all, we replace the method instead
        public static void DoShrineEffectHook(Action<BeholsterShrineController, PlayerController> orig, BeholsterShrineController self, PlayerController interactor)
        {
            typeof(BeholsterShrineController).GetMethod("SetFlagForID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(self, new object[] { interactor.CurrentGun.PickupObjectId });

            int num = 0;

            foreach (var item in flags)
            {
                if (GameStatsManager.Instance.GetFlag(item))
                {
                    num++;
                }
            }

            if (num == 6)
            {
                LootEngine.TryGiveGunToPlayer(PickupObjectDatabase.GetById(self.Gun01ID).gameObject, interactor, false);
                LootEngine.TryGiveGunToPlayer(PickupObjectDatabase.GetById(self.Gun02ID).gameObject, interactor, false);
                LootEngine.TryGiveGunToPlayer(PickupObjectDatabase.GetById(self.Gun03ID).gameObject, interactor, false);
                LootEngine.TryGiveGunToPlayer(PickupObjectDatabase.GetById(self.Gun04ID).gameObject, interactor, false);
                LootEngine.TryGiveGunToPlayer(PickupObjectDatabase.GetById(self.Gun05ID).gameObject, interactor, false);
                LootEngine.TryGiveGunToPlayer(PickupObjectDatabase.GetById(self.Gun06ID).gameObject, interactor, false);

                // we prevent the call of the coroutine that resets the shrine (can't hook the coroutine directly for some reason)
                // base.StartCoroutine(this.HandleShrineCompletionVisuals());
                AkSoundEngine.PostEvent("Play_OBJ_shrine_accept_01", self.gameObject);

                // this prevents further uses of this exact shrine instance
                typeof(BeholsterShrineController).GetField("m_useCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(self, 100);

                interactor.inventory.GunChangeForgiveness = true;

                for (int i = 0; i < 100; i++)
                {
                    Gun targetGunWithChange = interactor.inventory.GetTargetGunWithChange(i);

                    if (targetGunWithChange.PickupObjectId == self.Gun01ID)
                    {
                        if (i != 0)
                        {
                            interactor.inventory.ChangeGun(i, false, false);
                        }

                        break;
                    }
                }

                interactor.inventory.GunChangeForgiveness = false;
            }
            else
            {
                interactor.inventory.DestroyCurrentGun();
            }
        }

        public static bool CheckCanBeUsedHook(Func<BeholsterShrineController, PlayerController, bool> orig, BeholsterShrineController self, PlayerController player)
        {
            // update the gun visibility so it looks nice in case the player cheated
            typeof(BeholsterShrineController).GetMethod("UpdateSpriteVisibility", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(self, null);

            // if all 6 guns are on the shrine, you can always use it; let's count how many they have
            int num = 0;

            foreach (var item in flags)
            {
                if (GameStatsManager.Instance.GetFlag(item))
                {
                    num++;
                }
            }

            // guarantee to call orig even if its not needed in case someone else hooks it or other weird stuff
            bool theirRet = orig(self, player);

            // player needs to have a gun no matter what, otherwise the other code fails
            bool ourRet = player && player.CurrentGun && num == 6;

            // if all guns are there, change the accept text option to the challenge shrine one, which is "Remain" in english (seems good enough)
            if (ourRet)
            {
                // we don't need to change this back, because the shrine can never get empty again with this mod installed
                self.acceptOptionKey = "#SHRINE_CHALLENGE_ACCEPT";
            }

            return ourRet || theirRet;
        }
    }
}