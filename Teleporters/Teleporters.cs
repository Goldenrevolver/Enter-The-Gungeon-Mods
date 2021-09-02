using Dungeonator;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;

namespace Teleporters
{
    public class Teleporters : ETGModule
    {
        public static readonly string MOD_NAME = "Teleporters Everywhere";

        public override void Init()
        {
        }

        public override void Start()
        {
            // can be used for testing
            // GameStatsManager.Instance.SetFlag(GungeonFlags.BLACKSMITH_ELEMENT2, false);
            // GameStatsManager.Instance.SetFlag(GungeonFlags.BLACKSMITH_ELEMENT3, false);

            try
            {
                new Hook(typeof(PlayerController).GetMethod(nameof(PlayerController.BraveOnLevelWasLoaded)), typeof(Teleporters).GetMethod(nameof(Teleporters.AddMoreTeleporters)));
            }
            catch (Exception e)
            {
                ETGModConsole.Log($"<color=red>Exception whilst setting up hooks: {e}</color>");
            }

            ETGModConsole.Log($"{MOD_NAME} v{Metadata.Version} initialized");
        }

        public override void Exit()
        {
        }

        public static void AddMoreTeleporters(Action<PlayerController> orig, PlayerController player)
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
                ETGModConsole.Log($"<color=red>Exception while getting room data: {e}</color>");
            }
        }
    }
}