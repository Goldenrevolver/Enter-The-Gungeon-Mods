using Dungeonator;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using UnityEngine;

namespace BlankReminder
{
    public class BlankReminder : ETGModule
    {
        public static readonly string MOD_NAME = "Blank And Active Item Reminder";

        public static bool ShowBlankReminder;
        public static bool ShowActiveItemReminder;

        private static GameObject messageHolder;

        public override void Init()
        {
            // creating a new object to be the parent for the text boxes to make sure we don't cause a TextBoxManager conflict if we were to attach it to something else (like the door or the player)
            messageHolder = new GameObject("Message Holder");
            UnityEngine.Object.DontDestroyOnLoad(messageHolder);

            // default true
            ShowBlankReminder = PlayerPrefs.GetInt("BlankReminderShowBlankReminder", 1) == 1;

            // default true
            ShowActiveItemReminder = PlayerPrefs.GetInt("BlankReminderShowActiveItemReminder", 1) == 1;
        }

        public override void Start()
        {
            try
            {
                ETGModConsole.Commands.AddGroup("reminder");
                ETGModConsole.Commands.GetGroup("reminder").AddUnit("showBlankReminder", delegate (string[] e)
                {
                    // flips the bool value
                    ShowBlankReminder ^= true;
                    ETGModConsole.Log("Show blank reminder: " + ShowBlankReminder);
                    PlayerPrefs.SetInt("BlankReminderShowBlankReminder", ShowBlankReminder ? 1 : 0);
                    PlayerPrefs.Save();
                }).AddUnit("showActiveItemReminder", delegate (string[] e)
                {
                    // flips the bool value
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
                    ETGModConsole.Log($"<color=red>Exception whilst setting up hooks: {e}</color>");
                }

                ETGModConsole.Log($"{MOD_NAME} v{Metadata.Version} initialized (blank reminder: {ShowBlankReminder}, active item reminder: {ShowActiveItemReminder})");
            }
            catch (Exception e)
            {
                ETGModConsole.Log($"<color=red>Exception in Start: {e}</color>");
            }
        }

        public override void Exit()
        {
        }

        private static bool IsFloorBossRoom(RoomHandler room)
        {
            return room.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS && room.area.PrototypeRoomBossSubcategory == PrototypeDungeonRoom.RoomBossSubCategory.FLOOR_BOSS;
        }

        public static void DoBlankReminder(Action<DungeonDoorController> orig, DungeonDoorController self)
        {
            try
            {
                // if no setting is enabled or its the wrong door don't do anything
                if ((ShowBlankReminder || ShowActiveItemReminder) && self.IsSealed && (self.Mode == DungeonDoorController.DungeonDoorMode.BOSS_DOOR_ONLY_UNSEALS || self.Mode == DungeonDoorController.DungeonDoorMode.FINAL_BOSS_DOOR))
                {
                    // we will make sure to show the correct message including for coop
                    PlayerController[] players = GameManager.Instance.AllPlayers;

                    // we are now getting the boss room so we don't send a message when the player is inside it (because that means they won the boss fight and the doors are reopening)
                    RoomHandler bossRoom = null;

                    // limited testing says the downstreamRoom is always the boss room, but it doesn't hurt to be thorough
                    bool downIsBoss = IsFloorBossRoom(self.downstreamRoom);
                    bool upIsBoss = IsFloorBossRoom(self.upstreamRoom);

                    if (downIsBoss == upIsBoss)
                    {
                        Debug.Log("Apparently both rooms are either the boss room or not the boss room, weird, ignoring");
                    }
                    else
                    {
                        bossRoom = downIsBoss ? self.downstreamRoom : self.upstreamRoom;
                    }

                    bool hasBlanks = false;
                    bool hasActiveItems = false;
                    bool isInBossRoom = false;

                    for (int i = 0; i < players.Length; i++)
                    {
                        if (!players[i].IsGhost && !players[i].healthHaver.IsDead)
                        {
                            hasBlanks |= players[i].Blanks > 0;
                            isInBossRoom |= players[i].CurrentRoom == bossRoom;

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

                    // don't send a message when there is none or we are in the boss room instead of infront of it
                    if (message != null && !isInBossRoom)
                    {
                        TextBoxManager.ShowThoughtBubble(self.transform.position, messageHolder.transform, 2f, message);
                    }
                }
            }
            catch (Exception e)
            {
                ETGModConsole.Log($"<color=red>Exception whilst trying to show reminder message: {e}</color>");
            }

            orig(self);
        }
    }
}