using Dungeonator;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;

namespace TeleportersEverywhere
{
    public class TeleportersEverywhere : ETGModule
    {
        public static readonly string MOD_NAME = "Teleporters Everywhere";
        public static readonly string VERSION = "1.0";

        public override void Init()
        {
        }

        public override void Start()
        {
            // can be used for testing
            // GameStatsManager.Instance.SetFlag(GungeonFlags.BLACKSMITH_ELEMENT2, false);
            // GameStatsManager.Instance.SetFlag(GungeonFlags.BLACKSMITH_ELEMENT3, false);

            new Hook(typeof(PlayerController).GetMethod(nameof(PlayerController.BraveOnLevelWasLoaded)), typeof(TeleportersEverywhere).GetMethod(nameof(TeleportersEverywhere.AddTeleporters)));

            ETGModConsole.Log($"{MOD_NAME} v{VERSION} initialized");
        }

        public override void Exit()
        {
        }

        public static void AddTeleporters(Action<PlayerController> orig, PlayerController player)
        {
            orig(player);

            if (!player || !player.IsPrimaryPlayer)
            {
                return;
            }

            try
            {
                List<RoomHandler> rooms = GameManager.Instance.Dungeon.data.rooms;

                foreach (RoomHandler roomHandler in rooms)
                {
                    try
                    {
                        if (roomHandler.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.NORMAL && roomHandler.area.PrototypeRoomNormalSubcategory == PrototypeDungeonRoom.RoomNormalSubCategory.TRAP)
                        {
                            continue;
                        }

                        if (roomHandler.area.PrototypeRoomName.StartsWith("BulletComponent_Catacombs_Bridge_") || roomHandler.area.PrototypeRoomName.StartsWith("BulletComponent_Mines_Carts_"))
                        {
                            continue;
                        }

                        roomHandler.AddProceduralTeleporterToRoom();
                    }
                    catch (Exception)
                    {
                        // if something goes wrong here it doesn't really matter
                    }
                }
            }
            catch (Exception e)
            {
                ETGModConsole.Log("Error getting room data: " + e);
            }
        }
    }
}