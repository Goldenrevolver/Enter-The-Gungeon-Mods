using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using UnityEngine;

namespace BlankReminder
{
    public class BlankReminder : ETGModule
    {
        public static readonly string MOD_NAME = "Blank And Active Item Reminder";
        public static readonly string VERSION = "1.0";

        public static bool ShowBlankReminder;
        public static bool ShowActiveItemReminder;

        private static GameObject messageHolder;

        public override void Init()
        {
            // creating a new object to be the parent for the text boxes to make sure we don't cause a TextBoxManager conflict if we were to attach it to something else (like the door or the player)
            messageHolder = new GameObject("Message Holder");
            UnityEngine.Object.DontDestroyOnLoad(messageHolder);

            //default true
            ShowBlankReminder = PlayerPrefs.GetInt("BlankReminderShowBlankReminder", 1) == 1;
            //default true
            ShowActiveItemReminder = PlayerPrefs.GetInt("BlankReminderShowActiveItemReminder", 1) == 1;
        }

        public override void Start()
        {
            ETGModConsole.Commands.AddGroup("reminder");
            ETGModConsole.Commands.GetGroup("reminder").AddUnit("showBlankReminder", delegate (string[] e)
            {
                //flips the bool value
                ShowBlankReminder ^= true;
                ETGModConsole.Log("Show blank reminder: " + ShowBlankReminder);
                PlayerPrefs.SetInt("BlankReminderShowBlankReminder", ShowBlankReminder ? 1 : 0);
                PlayerPrefs.Save();
            }).AddUnit("showActiveItemReminder", delegate (string[] e)
            {
                //flips the bool value
                ShowActiveItemReminder ^= true;
                ETGModConsole.Log("Show active item reminder: " + ShowActiveItemReminder);
                PlayerPrefs.SetInt("BlankReminderShowActiveItemReminder", ShowActiveItemReminder ? 1 : 0);
                PlayerPrefs.Save();
            });

            try
            {
                new Hook(typeof(DungeonDoorController).GetMethod("UnsealInternal", BindingFlags.NonPublic | BindingFlags.Instance), typeof(BlankReminder).GetMethod(nameof(BlankReminder.DoBlankReminder)));
            }
            catch (Exception e)
            {
                ETGModConsole.Log($"<color=#FF0000>{MOD_NAME} v{VERSION}: error while setting up hooks: {e.Message}</color>");
            }

            ETGModConsole.Log($"{MOD_NAME} v{VERSION} initialized (blank reminder: {ShowBlankReminder}, active item reminder: {ShowActiveItemReminder})");
        }

        public override void Exit()
        {
        }

        public static void DoBlankReminder(Action<DungeonDoorController> baseMethod, DungeonDoorController door)
        {
            try
            {
                // if no setting is enabled or its the wrong door don't do anything
                if ((ShowBlankReminder || ShowActiveItemReminder) && door.IsSealed && (door.Mode == DungeonDoorController.DungeonDoorMode.BOSS_DOOR_ONLY_UNSEALS || door.Mode == DungeonDoorController.DungeonDoorMode.FINAL_BOSS_DOOR))
                {
                    // make sure to show the correct message including for coop
                    PlayerController[] players = GameManager.Instance.AllPlayers;

                    bool hasBlanks = false;
                    bool hasActiveItems = false;

                    for (int i = 0; i < players.Length; i++)
                    {
                        if (!players[i].IsGhost && !players[i].healthHaver.IsDead)
                        {
                            hasBlanks |= players[i].Blanks > 0;
                            // ignore the pilots lockpick, but don't bother with any other useless item checks
                            hasActiveItems |= players[i].activeItems.Count > 0 && !(players[i].HasActiveItem(356) && players[i].activeItems.Count == 1);
                        }
                    }

                    // if the setting is disabled, treat it as if you don't have the item (better readability than integrating the setting into the loop)
                    if (!ShowBlankReminder)
                    {
                        hasBlanks = false;
                    }

                    if (!ShowActiveItemReminder)
                    {
                        hasActiveItems = false;
                    }

                    string message = null;

                    if (hasBlanks)
                    {
                        message = "Remember to use blanks";
                        if (hasActiveItems)
                        {
                            message += "\nand active items";
                        }
                    }
                    else if (hasActiveItems)
                    {
                        message = "Remember to use active items";
                    }

                    if (message != null)
                    {
                        TextBoxManager.ShowThoughtBubble(door.transform.position, messageHolder.transform, 2f, message);
                    }
                }
            }
            catch (Exception e)
            {
                ETGModConsole.Log($"<color=#FF0000>{MOD_NAME} v{VERSION}: error whilst trying to show reminder message: {e.Message}</color>");
            }

            baseMethod(door);
        }
    }
}