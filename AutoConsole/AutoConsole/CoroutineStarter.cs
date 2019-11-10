using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AutoConsole
{
    class CoroutineStarter : MonoBehaviour
    {
        private bool isFirstCharacterSelect = true;
        private bool isFirstCharacterDeselect = true;
        private bool isFirstGungeonEnter = true;
        private bool isFirstChamberEnter = true;

        public void ExecuteCharacterDeselect(bool isForPlayerOne)
        {
            if (AutoConsole.Enabled)
            {
                executeCommands(isForPlayerOne, AutoConsole.Commands[(int)HookLocation.OnCharacterDeselect], isFirstCharacterDeselect);
                isFirstCharacterDeselect = false;
            }
        }

        public void WaitAndExecuteCharacterSelect(bool isForPlayerOne)
        {
            StartCoroutine(waitAndExecuteCharacterSelect(isForPlayerOne));
        }

        private IEnumerator waitAndExecuteCharacterSelect(bool isForPlayerOne)
        {
            //wait three frames for character change method to finish
            yield return null;
            yield return null;
            yield return null;
            if (AutoConsole.Enabled)
            {
                executeCommands(isForPlayerOne, AutoConsole.Commands[(int)HookLocation.OnCharacterSelect], isFirstCharacterSelect);
                isFirstCharacterSelect = false;
            }
        }

        public void WaitAndExecuteChamberEnter(bool isForPlayerOne)
        {
            StartCoroutine(waitAndExecuteChamberEnter(isForPlayerOne));
        }

        private IEnumerator waitAndExecuteChamberEnter(bool isForPlayerOne)
        {
            PlayerController player = isForPlayerOne ? GameManager.Instance.PrimaryPlayer : GameManager.Instance.SecondaryPlayer;
            while (player.CurrentInputState != PlayerInputState.AllInput || ETGModConsole.Instance.GUI.Visible)
            {
                yield return null;
            }
            if (AutoConsole.Enabled)
            {
                //the 'has taken damage' check is to check for clone (you need to take damage to die to trigger clone)
                if(!hasTakenDamage() && GameManager.Instance.CurrentFloor == getStartingFloor())
                {
                    executeCommands(isForPlayerOne, AutoConsole.Commands[(int)HookLocation.OnGungeonEnter], isFirstGungeonEnter);
                    isFirstGungeonEnter = false;
                }
                executeCommands(isForPlayerOne, AutoConsole.Commands[(int)HookLocation.OnChamberEnter], isFirstChamberEnter);
                isFirstChamberEnter = false;
            }
        }
        
        private static bool hasTakenDamage()
        {
            foreach (var player in GameManager.Instance.AllPlayers)
            {
                if (player.HasTakenDamageThisRun)
                    return true;
            }
            return false;
        }

        private static int getStartingFloor()
        {
            int nextLevelIndex;
            if (GameManager.Instance.TargetQuickRestartLevel != -1)
            {
                nextLevelIndex = GameManager.Instance.TargetQuickRestartLevel;
            }
            else
            {
                nextLevelIndex = 1;
                if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.SHORTCUT)
                {
                    nextLevelIndex += GameManager.Instance.LastShortcutFloorLoaded;
                }
            }
            return nextLevelIndex;
        }

        private static void executeCommands(bool isForPlayerOne, List<AutoConsoleUnit> list, bool isFirst)
        {
            PlayerController player;
            foreach (var command in list)
            {
                player = isForPlayerOne ? GameManager.Instance.PrimaryPlayer : GameManager.Instance.SecondaryPlayer;

                if (command.DoneOnlyOnce && !isFirst)
                    continue;

                object cc = player.GetComponent("CustomCharacter");
                if (cc != null)
                {
                    string name = player.gameObject.name;
                    name = name.RemovePrefix("Player").RemoveSuffix("(Clone)");
                    //var gm = cc.GetType().GetField("gameobject", BindingFlags.Public | BindingFlags.Instance).GetValue(cc);
                    //var name = data.GetType().GetField("nameShort", BindingFlags.Public | BindingFlags.Instance).GetValue(data);
                    if (command.ModdedCharacterLimitation != null && command.ModdedCharacterLimitation.ToLower() != ((string)name).ToLower())
                        continue;
                }
                else
                {
                    PlayableCharacters character = player.characterIdentity;
                    //changing the secondary character with CC doesnt save the characterIdentity
                    if (!isForPlayerOne)
                    {
                        switch (player.gameObject.name)
                        {
                            case "PlayerRogue(Clone)":
                                character = PlayableCharacters.Pilot;
                                break;
                            case "PlayerConvict(Clone)":
                                character = PlayableCharacters.Convict;
                                break;
                            case "PlayerGuide(Clone)":
                                character = PlayableCharacters.Guide;
                                break;
                            case "PlayerMarine(Clone)":
                                character = PlayableCharacters.Soldier;
                                break;
                            case "PlayerRobot(Clone)":
                                character = PlayableCharacters.Robot;
                                break;
                            case "PlayerBullet(Clone)":
                                character = PlayableCharacters.Bullet;
                                break;
                            case "PlayerCoopCultist(Clone)":
                                character = PlayableCharacters.CoopCultist;
                                break;
                            case "PlayerEevee(Clone)":
                                character = PlayableCharacters.Eevee;
                                break;
                            case "PlayerGunslinger(Clone)":
                                character = PlayableCharacters.Gunslinger;
                                break;
                            case "PlayerCosmonaut(Clone)":
                                character = PlayableCharacters.Cosmonaut;
                                break;
                            case "PlayerNinja(Clone)":
                                character = PlayableCharacters.Ninja;
                                break;
                        }
                    }

                    //if the required character is modded only
                    if (command.CharacterLimitation == null && command.ModdedCharacterLimitation != null)
                        continue;
                    if (command.CharacterLimitation != null && command.CharacterLimitation != character)
                        continue;
                }

                ETGModConsole.Instance.ParseCommand(command.Command);

                if (AutoConsole.ShowLog)
                    ETGModConsole.Log(command.Command + " (AC)");
                
                string[] parts = command.Command.Split(' ');
                if (parts[0] == "character" && GameManager.Instance.IsFoyer)
                {
                    player = GameManager.Instance.PrimaryPlayer;
                    player.TeleportToPoint(player.transform.position + new Vector3(0.1f, 0.1f), false);
                }
            }
        }
    }
}
