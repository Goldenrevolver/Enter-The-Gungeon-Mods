using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ShrineDesc
{
    public class ShrineDesc : ETGModule
    {
        public static readonly string MOD_NAME = "Shrine Descriptions";
        public static readonly string VERSION = "1.0.2";

        private static GameObject shrineTextHolder;
        private static Dictionary<string, string> descriptions;

        public override void Init()
        {
            // creating a new object to be the parent for the text boxes to make sure we don't cause a TextBoxManager conflict if we were to attach it to something else (like the shrine or the player)
            shrineTextHolder = new GameObject("Shrine Text Holder");
            UnityEngine.Object.DontDestroyOnLoad(shrineTextHolder);

            descriptions = new Dictionary<string, string>
            {
                { "shrine_ammo", "Refills the ammo of all weapons, but increases curse by 3.5. Can only be used once." },
                { "shrine_blank", "Using a blank near the statue has a 90% chance to spawn a chest. This chest may be locked or unlocked and can be a mimic. Repeated use of blanks can spawn more chests, but each chest received decreases the success chance by 45%, down to a minimum of 25%." },
                { "shrine_blood", "Removes one heart container (or two armor if The Robot). Some enemies become highlighted in red. Standing near highlighted enemies damages and drains them, healing the player after draining enough." },
                { "shrine_challenge", "Using the shrine forces the player to fight through three waves of enemies. Surviving the waves rewards the player with an unlocked chest. The chest can be a mimic. Can only be used once." },
                { "shrine_cleanse", "Sets the player's curse to 0 in exchange for 5 Shells per point of curse. Can be used multiple times." },
                { "shrine_companion", "Removes one heart container (or two armor if The Robot), and grants a random item that summons a familiar. Can only be used once. (Doesn't give a familiar in Rainbow Mode, but still takes a heart container!)" },
                { "shrine_dice", "Using this shrine grants one positive and one negative effect. Can only be used once." },
                { "shrine_glass", "Grants three Glass Guon Stones which block enemy bullets but break if the player takes damage. Can only be used once." },
                { "shrine_junk", "Grants a piece of armor in exchange for Junk. Can be used multiple times." },
                { "shrine_yv", "Costs 10 Shells to use the first time, and the cost increases by 10 Shells per use. Every time the player fires a weapon, there is a chance for it to quickly fire 2 to 4 times at no extra ammo cost. Each use linearly increases the chance to activate the effect by 3.7%." },
                { "shrine_fallenangel", "Removes one heart container (or two armor if The Robot), increases damage by 25%, and increases curse by 1.5. Can only be used once." },
                { "shrine_health", "Heals the player for one heart in exchange for their currently held weapon (cannot be used by The Robot). Can be used multiple times." }
            };
        }

        public override void Start()
        {
            // no, they don't have a super class we could use (they implement the IPlayerInteractable interface but we can't hook that)

            try
            {
                new Hook(typeof(ShrineController).GetMethod(nameof(ShrineController.OnEnteredRange), BindingFlags.Public | BindingFlags.Instance), typeof(ShrineDesc).GetMethod(nameof(ShrineDesc.OnEnteredRange)));
                new Hook(typeof(ShrineController).GetMethod(nameof(ShrineController.Interact), BindingFlags.Public | BindingFlags.Instance), typeof(ShrineDesc).GetMethod(nameof(ShrineDesc.ClearTextBox)));
                new Hook(typeof(ShrineController).GetMethod(nameof(ShrineController.OnExitRange), BindingFlags.Public | BindingFlags.Instance), typeof(ShrineDesc).GetMethod(nameof(ShrineDesc.ClearTextBox)));

                new Hook(typeof(AdvancedShrineController).GetMethod(nameof(AdvancedShrineController.OnEnteredRange), BindingFlags.Public | BindingFlags.Instance), typeof(ShrineDesc).GetMethod(nameof(ShrineDesc.OnEnteredRange)));
                new Hook(typeof(AdvancedShrineController).GetMethod(nameof(AdvancedShrineController.Interact), BindingFlags.Public | BindingFlags.Instance), typeof(ShrineDesc).GetMethod(nameof(ShrineDesc.ClearTextBox)));
                new Hook(typeof(AdvancedShrineController).GetMethod(nameof(AdvancedShrineController.OnExitRange), BindingFlags.Public | BindingFlags.Instance), typeof(ShrineDesc).GetMethod(nameof(ShrineDesc.ClearTextBox)));

                new Hook(typeof(ChallengeShrineController).GetMethod(nameof(ChallengeShrineController.OnEnteredRange), BindingFlags.Public | BindingFlags.Instance), typeof(ShrineDesc).GetMethod(nameof(ShrineDesc.OnEnteredRange)));
                new Hook(typeof(ChallengeShrineController).GetMethod(nameof(ChallengeShrineController.Interact), BindingFlags.Public | BindingFlags.Instance), typeof(ShrineDesc).GetMethod(nameof(ShrineDesc.ClearTextBox)));
                new Hook(typeof(ChallengeShrineController).GetMethod(nameof(ChallengeShrineController.OnExitRange), BindingFlags.Public | BindingFlags.Instance), typeof(ShrineDesc).GetMethod(nameof(ShrineDesc.ClearTextBox)));

                new Hook(typeof(BeholsterShrineController).GetMethod(nameof(BeholsterShrineController.OnEnteredRange), BindingFlags.Public | BindingFlags.Instance), typeof(ShrineDesc).GetMethod(nameof(ShrineDesc.OnEnteredRangeBeholster)));
                new Hook(typeof(BeholsterShrineController).GetMethod(nameof(BeholsterShrineController.Interact), BindingFlags.Public | BindingFlags.Instance), typeof(ShrineDesc).GetMethod(nameof(ShrineDesc.ClearTextBox)));
                new Hook(typeof(BeholsterShrineController).GetMethod(nameof(BeholsterShrineController.OnExitRange), BindingFlags.Public | BindingFlags.Instance), typeof(ShrineDesc).GetMethod(nameof(ShrineDesc.ClearTextBox)));
            }
            catch (Exception e)
            {
                ETGModConsole.Log($"<color=#FF0000>{MOD_NAME} v{VERSION}: error while setting up hooks: {e.Message}</color>");
            }

            ETGModConsole.Log($"{MOD_NAME} v{VERSION} initialized");
        }

        public override void Exit()
        {
        }

        public static string RemoveFromEnd(string s, string suffix)
        {
            if (s.EndsWith(suffix))
            {
                return s.Substring(0, s.Length - suffix.Length);
            }
            else
            {
                return s;
            }
        }

        public static void ClearTextBox(Action<DungeonPlaceableBehaviour, PlayerController> baseMethod, DungeonPlaceableBehaviour shrine, PlayerController player)
        {
            // just to make sure the base method gets called, but no need to handle an exception
            try
            {
                TextBoxManager.ClearTextBox(shrineTextHolder.transform);
            }
            catch (Exception)
            {
            }

            baseMethod(shrine, player);
        }

        public static void OnEnteredRange(Action<DungeonPlaceableBehaviour, PlayerController> baseMethod, DungeonPlaceableBehaviour shrine, PlayerController player)
        {
            baseMethod(shrine, player);

            if (shrine.name == null || (int)shrine.GetType().GetField("m_useCount", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(shrine) > 0)
            {
                return;
            }

            // we have to go one level higher to find out the name of the instantiated prefab (the level below has oftentimes the wrong name probably due to prefab copying, e.g. the glass shrine is called blank shrine and the challenge shrine is called ammo shrine)
            string key = RemoveFromEnd(shrine.transform.parent.name.ToLower(), "(clone)");

            if (shrine is ChallengeShrineController)
            {
                key = "shrine_challenge";
            }

            Transform talkPoint = (Transform)shrine.GetType().GetField("talkPoint", BindingFlags.Public | BindingFlags.Instance).GetValue(shrine);

            if (descriptions.ContainsKey(key))
            {
                TextBoxManager.ShowStoneTablet(talkPoint.position, shrineTextHolder.transform, -1f, descriptions[key], true, false);
            }
        }

        public static void OnEnteredRangeBeholster(Action<BeholsterShrineController, PlayerController> baseMethod, BeholsterShrineController shrine, PlayerController player)
        {
            baseMethod(shrine, player);

            if ((int)shrine.GetType().GetField("m_useCount", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(shrine) > 0)
            {
                return;
            }

            StringBuilder tooltip = new StringBuilder("Give the shrine the weapons the Beholster uses in game throughout multiple runs. When placing the last gun on the shrine, they will all be returned to the player, granting the Behold! synergy");

            bool hasBetterBeholsterShrineMod = false;
            foreach (var item in ETGMod.GameMods)
            {
                // mod is installed
                if (item.Metadata.Name == "BetterBeholsterShrine")
                {
                    hasBetterBeholsterShrineMod = true;
                    break;
                }
            }

            tooltip.Append(!hasBetterBeholsterShrineMod ? " and resetting the shrine." : ". After that, future encounters with the shrine do this without needing the guns again.");

            if (!GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_01))
            {
                tooltip.Append($"\nEye of the Beholster needed{(player.inventory.ContainsGun(shrine.Gun01ID) ? " (in your inventory)" : "")}");
            }
            if (!GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_02))
            {
                tooltip.Append($"\nCom4nd0 needed{(player.inventory.ContainsGun(shrine.Gun02ID) ? " (in your inventory)" : "")}");
            }
            if (!GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_03))
            {
                tooltip.Append($"\nMachine Pistol needed{(player.inventory.ContainsGun(shrine.Gun03ID) ? " (in your inventory)" : "")}");
            }
            if (!GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_04))
            {
                tooltip.Append($"\nVoid Marshal needed{(player.inventory.ContainsGun(shrine.Gun04ID) ? " (in your inventory)" : "")}");
            }
            if (!GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_05))
            {
                tooltip.Append($"\nTrank Gun needed{(player.inventory.ContainsGun(shrine.Gun05ID) ? " (in your inventory)" : "")}");
            }
            if (!GameStatsManager.Instance.GetFlag(GungeonFlags.SHRINE_BEHOLSTER_GUN_06))
            {
                tooltip.Append($"\nM1911 needed{(player.inventory.ContainsGun(shrine.Gun06ID) ? " (in your inventory)" : "")}");
            }

            TextBoxManager.ShowStoneTablet(shrine.talkPoint.position, shrineTextHolder.transform, -1f, tooltip.ToString(), true, false);
        }
    }
}