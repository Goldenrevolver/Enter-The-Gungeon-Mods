using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace AutoConsole
{
    public enum HookLocation
    {
        OnCharacterSelect,
        OnCharacterDeselect,
        OnGungeonEnter,
        OnChamberEnter
    }

    public struct AutoConsoleUnit
    {
        public HookLocation HookLocation;
        public string Command;
        public PlayableCharacters? CharacterLimitation;
        public string ModdedCharacterLimitation;
        public bool DoneOnlyOnce;
    }

    public class AutoConsole : ETGModule
    {
        private static CoroutineStarter coroutineStarter;
        private static readonly string version = "2.1.1";
        private static readonly string filePath = Path.Combine(ETGMod.ResourcesDirectory, "AutoConsole/");

        public static bool Enabled;
        public static bool ShowLog;

        public static List<List<AutoConsoleUnit>> Commands = new List<List<AutoConsoleUnit>>();

        //init is too early for using ETGModConsole
        public override void Init()
        {
            foreach (var item in Enum.GetValues(typeof(HookLocation)))
            {
                Commands.Add(new List<AutoConsoleUnit>());
            }

            GameObject coroutineStarterObject = new GameObject("Coroutine Starter");
            coroutineStarter = coroutineStarterObject.AddComponent<CoroutineStarter>();
            UnityEngine.Object.DontDestroyOnLoad(coroutineStarterObject);

            Enabled = PlayerPrefs.GetInt("AutoConsoleEnabled", 1) == 1;
            ShowLog = PlayerPrefs.GetInt("AutoConsoleLogging", 1) == 1;
        }

        //exit is not getting called at all, so I have to save settings when I change them
        public override void Exit() { }

        public override void Start()
        {
            //AddGroup doesnt return the correct group for some reason so I have to get it again
            ETGModConsole.Commands.AddGroup("autoConsole");
            ETGModConsole.Commands.GetGroup("autoConsole").AddUnit("enabled", delegate (string[] e)
            {
                Enabled ^= true;
                ETGModConsole.Log("AutoConsole enabled: " + Enabled);
                PlayerPrefs.SetInt("AutoConsoleEnabled", Enabled ? 1 : 0);
                PlayerPrefs.Save();
            }).AddUnit("logging", delegate (string[] e)
            {
                ShowLog ^= true;
                ETGModConsole.Log("AutoConsole logging: " + ShowLog);
                PlayerPrefs.SetInt("AutoConsoleLogging", ShowLog ? 1 : 0);
                PlayerPrefs.Save();
            });

            Directory.CreateDirectory(filePath);
            parseCommands(getCommands());

            new Hook(typeof(GameManager).GetMethod("ClearPrimaryPlayer", BindingFlags.Instance | BindingFlags.Public), typeof(AutoConsole).GetMethod("OnPrimaryCharacterDeselect"));
            new Hook(typeof(GameManager).GetMethod("ClearSecondaryPlayer", BindingFlags.Instance | BindingFlags.Public), typeof(AutoConsole).GetMethod("OnSecondaryCharacterDeselect"));
            new Hook(typeof(PlayerController).GetMethod("Start", BindingFlags.Instance | BindingFlags.Public), typeof(AutoConsole).GetMethod("OnCharacterSelect"));
            new Hook(typeof(PlayerController).GetMethod("DoInitialFallSpawn", BindingFlags.Instance | BindingFlags.Public), typeof(AutoConsole).GetMethod("OnChamberEnter"));
            new Hook(typeof(PlayerController).GetMethod("DoSpinfallSpawn", BindingFlags.Instance | BindingFlags.Public), typeof(AutoConsole).GetMethod("OnChamberEnter"));
            
            ETGModConsole.Log($"AutoConsole v{version} initialized (enabled: {Enabled}, logging: {ShowLog})");
        }

        public static void OnPrimaryCharacterDeselect(Action<GameManager> orig, GameManager self)
        {
            if(GameManager.Instance.PrimaryPlayer != null)
                coroutineStarter.ExecuteCharacterDeselect(true);
            orig(self);
        }

        public static void OnSecondaryCharacterDeselect(Action<GameManager> orig, GameManager self)
        {
            if (GameManager.Instance.SecondaryPlayer != null)
                coroutineStarter.ExecuteCharacterDeselect(false);
            orig(self);
        }

        public static void OnCharacterSelect(Action<PlayerController> orig, PlayerController self)
        {
            //we need to remember which player is originally adressed because commands can delete the player
            bool? isForPlayerOne = null;
            if (GameManager.Instance.PrimaryPlayer == self)
            {
                isForPlayerOne = true;
            }
            else if (GameManager.Instance.SecondaryPlayer == self)
            {
                isForPlayerOne = false;
            }
            orig(self);
            if (isForPlayerOne != null)
                coroutineStarter.WaitAndExecuteCharacterSelect(isForPlayerOne.Value);
        }

        public static void OnChamberEnter(Action<PlayerController, float> orig, PlayerController self, float invisibleDelay)
        {
            //we need to remember which player is originally adressed because commands can delete the player
            bool? isForPlayerOne = null;
            if (GameManager.Instance.PrimaryPlayer == self)
            {
                isForPlayerOne = true;
            }
            else if (GameManager.Instance.SecondaryPlayer == self)
            {
                isForPlayerOne = false;
            }
            orig(self, invisibleDelay);
            if(isForPlayerOne != null)
            {
                coroutineStarter.WaitAndExecuteChamberEnter(isForPlayerOne.Value);
            }
        }

        private static List<string> getCommands()
        {
            List<string> commands = new List<string>();

            string[] files = Directory.GetFiles(filePath);
            foreach (var file in files)
            {
                string[] lines = File.ReadAllLines(file);
                commands.AddRange(lines);
            }

            return commands;
        }

        private static void parseCommands(List<string> commands)
        {
            if(commands != null)
            {
                HookLocation? location = null;
                PlayableCharacters? characterLimitation = null;
                string moddedCharacterLimitation = null;
                bool isFirstOnly = false;

                foreach (var command in commands)
                {
                    if(command == null)
                    {
                        continue;
                    }
                    string trimmedCommand = command.Trim();
                    if (trimmedCommand.Any() && trimmedCommand[0] != '#')
                    {
                        string[] parts = trimmedCommand.Split(' ');

                        if (parts[0] == "OnCharacterSelect")
                        {
                            location = HookLocation.OnCharacterSelect;
                            isFirstOnly = parts.Length > 1 && parts[1].ToLower() == "first";
                            characterLimitation = null;
                            moddedCharacterLimitation = null;
                        }
                        else if (parts[0] == "OnCharacterDeselect")
                        {
                            location = HookLocation.OnCharacterDeselect;
                            isFirstOnly = parts.Length > 1 && parts[1].ToLower() == "first";
                            characterLimitation = null;
                            moddedCharacterLimitation = null;
                        }
                        else if (parts[0] == "OnGungeonEnter")
                        {
                            location = HookLocation.OnGungeonEnter;
                            isFirstOnly = parts.Length > 1 && parts[1].ToLower() == "first";
                            characterLimitation = null;
                            moddedCharacterLimitation = null;
                        }
                        else if(parts[0] == "OnChamberEnter")
                        {
                            location = HookLocation.OnChamberEnter;
                            isFirstOnly = parts.Length > 1 && parts[1].ToLower() == "first";
                            characterLimitation = null;
                            moddedCharacterLimitation = null;
                        }
                        else if (parts[0] == "if")
                        {
                            if (location == null || parts.Length < 2)
                            {
                                continue;
                            }
                            moddedCharacterLimitation = parts[1];
                            //fix the casing for default name parsing
                            string name = char.ToUpper(parts[1][0]) + parts[1].Substring(1).ToLower();
                            if(name == "Rogue")
                            {
                                characterLimitation = PlayableCharacters.Pilot;
                            }
                            else if (name == "Hunter")
                            {
                                characterLimitation = PlayableCharacters.Guide;
                            }
                            else if (name == "Marine")
                            {
                                characterLimitation = PlayableCharacters.Soldier;
                            }
                            //ask for both options because the casing doesnt work here
                            else if (name == "Cultist" || name == "Coopcultist")
                            {
                                characterLimitation = PlayableCharacters.CoopCultist;
                            }
                            else if(name == "Paradox")
                            {
                                characterLimitation = PlayableCharacters.Eevee;
                            }
                            else
                            {
                                try
                                {
                                    characterLimitation = (PlayableCharacters)Enum.Parse(typeof(PlayableCharacters), name);
                                }
                                catch (Exception)
                                {
                                    characterLimitation = null;
                                }
                            }
                        }
                        else if (parts[0] == "always")
                        {
                            if (location == null)
                            {
                                continue;
                            }
                            characterLimitation = null;
                            moddedCharacterLimitation = null;
                        }
                        else
                        {
                            if (location == null)
                            {
                                continue;
                            }
                            AutoConsoleUnit unit = new AutoConsoleUnit();
                            unit.HookLocation = location.Value;
                            unit.CharacterLimitation = characterLimitation;
                            unit.ModdedCharacterLimitation = moddedCharacterLimitation;
                            unit.Command = trimmedCommand;
                            unit.DoneOnlyOnce = isFirstOnly;
                            if(location == HookLocation.OnCharacterSelect && !isFirstOnly //isAlwaysCharSelect
                                && (parts[0] == "character" || parts[0] == "character2") && parts.Length > 1 //wantsToChangeChar
                                && ((characterLimitation != null && characterLimitation.ToString().ToLower() == parts[1].ToLower()) //wantsSameChar
                                || (moddedCharacterLimitation != null && moddedCharacterLimitation.ToLower() == parts[1].ToLower()))) //wantsSameModdedChar
                            {
                                ETGModConsole.Log("AC Error: you have a character select command that could lead to an infinite loop. Command skipped.");
                                continue;
                            }
                            if (location == HookLocation.OnCharacterDeselect && (parts[0] == "character" || parts[0] == "character2") && parts.Length > 1)
                            {
                                ETGModConsole.Log("AC Error: you have a character deselect command that wants to change character. Command skipped.");
                                continue;
                            }

                            Commands[(int)location].Add(unit);
                        }
                    }
                }
            }
        }
    }
}
