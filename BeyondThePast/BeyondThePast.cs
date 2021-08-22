using ItemAPI;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeyondThePast
{
    public class BeyondThePast : ETGModule
    {
        public static readonly string MOD_NAME = "Beyond The Past - Stronger Starting Loadouts";

        public static bool NonMedalMarine = false;

        public static bool ModEnabled;

        public override void Init()
        {
            ModEnabled = PlayerPrefs.GetInt("BeyondThePastEnabled", 1) == 1;
        }

        public override void Exit()
        {
        }

        // TODO add all custom items to all vanilla synergies of the base item

        public override void Start()
        {
            ETGModConsole.Commands.AddGroup("beyondThePast");
            ETGModConsole.Commands.GetGroup("beyondThePast").AddUnit("enabled", delegate (string[] e)
            {
                // flips the bool value
                ModEnabled ^= true;
                ETGModConsole.Log($"Beyond The Past is now " + (ModEnabled ? "enabled" : "disabled") + ".");
                PlayerPrefs.SetInt("BeyondThePastEnabled", ModEnabled ? 1 : 0);
                PlayerPrefs.Save();
            });

            try
            {
                ItemBuilder.Init();

                if (GameStatsManager.Instance.GetCharacterSpecificFlag(PlayableCharacters.Robot, CharacterSpecificGungeonFlags.KILLED_PAST))
                {
                    LoadRobotModule();
                }
                if (GameStatsManager.Instance.GetCharacterSpecificFlag(PlayableCharacters.Bullet, CharacterSpecificGungeonFlags.KILLED_PAST))
                {
                    LoadBulletModule();
                }
                if (GameStatsManager.Instance.GetCharacterSpecificFlag(PlayableCharacters.Convict, CharacterSpecificGungeonFlags.KILLED_PAST))
                {
                    LoadConvictModule();
                }
                if (GameStatsManager.Instance.GetCharacterSpecificFlag(PlayableCharacters.Guide, CharacterSpecificGungeonFlags.KILLED_PAST))
                {
                    LoadHunterModule();
                }
                if (GameStatsManager.Instance.GetCharacterSpecificFlag(PlayableCharacters.Pilot, CharacterSpecificGungeonFlags.KILLED_PAST))
                {
                    LoadPilotModule();
                }
                if (GameStatsManager.Instance.GetCharacterSpecificFlag(PlayableCharacters.Soldier, CharacterSpecificGungeonFlags.KILLED_PAST))
                {
                    LoadMarineModule();
                }

                // no condition for cultist (I'm not that evil)
                LoadCultistModule();

                SynergyHelper.Help();

                new Hook(typeof(PlayerController).GetMethod("InitializeInventory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(BeyondThePast).GetMethod(nameof(BeyondThePast.OverrideStartingInventory)));

                string output = $"{MOD_NAME} v{Metadata.Version} initialized";

                if (!ModEnabled)
                {
                    output += ", currently disabled";
                }

                ETGModConsole.Log(output);
            }
            catch (Exception e)
            {
                ETGModConsole.Log($"<color=FF0000>Exception in Start: {e}</color>");
            }
        }

        public static void OverrideStartingInventory(Action<PlayerController> orig, PlayerController self)
        {
            List<string> defaultCharNames = new List<string>() { "PlayerConvict(Clone)", "PlayerMarine(Clone)", "PlayerGuide(Clone)", "PlayerRogue(Clone)", "PlayerBullet(Clone)", "PlayerRobot(Clone)", "PlayerCoopCultist(Clone)" };

            // this also affects custom characters, but that's intended
            if (ModEnabled && self.characterIdentity == PlayableCharacters.CoopCultist && GameManager.Instance.CurrentGameType == GameManager.GameType.SINGLE_PLAYER)
            {
                self.startingPassiveItemIds = new List<int>() { };
                self.startingActiveItemIds = new List<int>() { WarningItem.WarningItemID };
                self.startingGunIds = new List<int>() { };
                self.startingAlternateGunIds = new List<int>() { };

                orig(self);
                return;
            }

            // if the mod is disabled, return
            // if it's a custom character. don't mess with it
            // if you have not beaten that character's past, don't switch (excluding cultist)
            if (!ModEnabled || !defaultCharNames.Contains(self.name) || (!GameStatsManager.Instance.GetCharacterSpecificFlag(self.characterIdentity, CharacterSpecificGungeonFlags.KILLED_PAST) && self.name != "PlayerCoopCultist(Clone)"))
            {
                orig(self);
                return;
            }

            switch (self.characterIdentity)
            {
                case PlayableCharacters.Pilot:
                    self.startingPassiveItemIds = new List<int>() { 187, 473, 491, MasterOfUnlocking.MasterOfUnlockingID }; // TODO synergies
                    self.startingActiveItemIds = new List<int>() { 356 };
                    self.startingGunIds = new List<int>() { 89 };
                    self.startingAlternateGunIds = new List<int>() { 651 };
                    break;

                case PlayableCharacters.Convict:
                    self.startingPassiveItemIds = new List<int>() { EmbarrassingPhoto.EmbarrassingPhotoID, EmptyBriefcase.EmptyBriefcaseID };
                    self.startingActiveItemIds = new List<int>() { PremiumCigarettes.PremiumCigarettesID, 460 }; // TODO synergies
                    self.startingGunIds = new List<int>() { 80, 2 };
                    self.startingAlternateGunIds = new List<int>() { 652, 2 };
                    break;

                case PlayableCharacters.Robot:

                    self.startingPassiveItemIds = new List<int>() { 410, RecycleItem.RecycleItemID };
                    self.startingActiveItemIds = new List<int>() { 411 };
                    self.startingGunIds = new List<int>() { 88, 576 };
                    self.startingAlternateGunIds = new List<int>() { 812, 576 };
                    break;

                case PlayableCharacters.Soldier:
                    if (!NonMedalMarine)
                    {
                        self.startingPassiveItemIds = new List<int>() { 494 };
                        self.startingActiveItemIds = new List<int>() { SupplySupport.SupplySupportID };
                        self.startingGunIds = new List<int>() { 86 };
                        self.startingAlternateGunIds = new List<int>() { 809 };
                    }
                    else
                    {
                        self.startingPassiveItemIds = new List<int>() { 354 };
                        self.startingActiveItemIds = new List<int>() { SupplySupport.SupplySupportID };
                        self.startingGunIds = new List<int>() { 86, 229 };
                        self.startingAlternateGunIds = new List<int>() { 809, 229 };
                    }
                    break;

                case PlayableCharacters.Guide:
                    self.startingPassiveItemIds = new List<int>() { 300, 492, PackLeader.PackLeaderID };
                    self.startingActiveItemIds = new List<int>() { };
                    self.startingGunIds = new List<int>() { 62, 4 };
                    self.startingAlternateGunIds = new List<int>() { 62, 4 };
                    break;

                case PlayableCharacters.Bullet:
                    self.startingPassiveItemIds = new List<int>() { 414, 572, OldBulletsBlessing.OldBulletsBlessingID };
                    self.startingActiveItemIds = new List<int>() { };
                    self.startingGunIds = new List<int>() { 417 };
                    self.startingAlternateGunIds = new List<int>() { 813 };
                    break;

                case PlayableCharacters.CoopCultist:

                    if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
                    {
                        self.startingPassiveItemIds = new List<int>() { 326, FakeHeroBandana.FakeHeroBandanaID, 375 };
                        self.startingActiveItemIds = new List<int>() { 412 };
                        self.startingGunIds = new List<int>() { 24, 10 };
                        self.startingAlternateGunIds = new List<int>() { 811, 10 };
                    }

                    break;
            }

            orig(self);
        }

        private void LoadPilotModule()
        {
            MasterOfUnlocking.SetupHook();
            MasterOfUnlocking.Register();
        }

        private void LoadConvictModule()
        {
            EmptyBriefcase.Register();
            EmbarrassingPhoto.Register();
            PremiumCigarettes.Register();
        }

        private void LoadRobotModule()
        {
            RecycleItem.SetupHook();
            RecycleItem.Register();
        }

        private void LoadMarineModule()
        {
            SupplySupport.Register();
        }

        private void LoadHunterModule()
        {
            PackLeader.Register();
        }

        private void LoadBulletModule()
        {
            OldBulletsBlessing.Register();
            CompassItem.Register();
        }

        private void LoadCultistModule()
        {
            FakeHeroBandana.Register();
            LonelinessCookie.Register();
            WarningItem.Register();
        }
    }
}