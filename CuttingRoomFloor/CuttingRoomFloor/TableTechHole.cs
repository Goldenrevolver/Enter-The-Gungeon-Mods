using ItemAPI;
using System.Collections;
using UnityEngine;

namespace CuttingRoomFloor
{
    class TableTechHole : PassiveItem
    {
        public static void Init()
        {
            //The name of the item
            string itemName = "Table Tech Hole";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "CuttingRoomFloor/Resources/tabletech_black_hole";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<TableTechHole>();

            //Adds a tk2dSprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Flip Oblivion";
            string longDesc = "This forbidden technique causes flipped tables to increase exponentially in mass, collapsing almost immediately.\n\nAn apocryphal text of the \"Tabla Sutra.\" Of that which we cannot flip, we must pass over in silence.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.B;
        }

        private GameObject objectToSpawn;

        public override void Pickup(PlayerController player)
        {
            base.Pickup(player);

            foreach (var item in Gungeon.Game.Items.Entries)
            {
                if (item is SpawnObjectPlayerItem)
                {
                    SpawnObjectPlayerItem item2 = (SpawnObjectPlayerItem)item;
                    if(item2.objectToSpawn != null && item2.objectToSpawn.name == "BlackHole")
                    {
                        objectToSpawn = item2.objectToSpawn;
                        break;
                    }
                }
            }

            player.OnTableFlipped += handleFlip;
        }

        public override DebrisObject Drop(PlayerController player)
        {
            var drop = base.Drop(player);
            player.OnTableFlipped -= handleFlip;
            return drop;
        }

        private void handleFlip(FlippableCover table)
        {
            GameManager.Instance.StartCoroutine(waitAndDestroy(table));
        }

        private IEnumerator waitAndDestroy(FlippableCover table)
        {
            yield return null;
            DoSpawn(table);
            table.DestroyCover();
        }

        private void DoSpawn(FlippableCover table)
        {
            Vector3 vector = table.transform.position;
            Vector3 vector2 = table.specRigidbody.UnitCenter;
            if (vector.y > 0f)
            {
                vector2 += Vector3.up * 0.25f;
            }
            GameObject gameObject2 = Instantiate<GameObject>(objectToSpawn, vector2, Quaternion.identity);
            tk2dBaseSprite component4 = gameObject2.GetComponent<tk2dBaseSprite>();
            if (component4)
            {
                component4.PlaceAtPositionByAnchor(vector2, tk2dBaseSprite.Anchor.MiddleCenter);
            }
            Vector2 vector3 = table.transform.position;
            vector3 = Quaternion.Euler(0f, 0f, 0f) * vector3;
            DebrisObject debrisObject = LootEngine.DropItemWithoutInstantiating(gameObject2, gameObject2.transform.position, vector3, 0, false, false, true, false);
            if (gameObject2.GetComponent<BlackHoleDoer>())
            {
                gameObject2.GetComponent<BlackHoleDoer>().coreDuration = 2f;
                debrisObject.PreventFallingInPits = true;
                debrisObject.PreventAbsorption = true;
            }
            if (vector.y > 0f && debrisObject)
            {
                debrisObject.additionalHeightBoost = -1f;
                if (debrisObject.sprite)
                {
                    debrisObject.sprite.UpdateZDepth();
                }
            }
            debrisObject.IsAccurateDebris = true;
            debrisObject.Priority = EphemeralObject.EphemeralPriority.Critical;
            debrisObject.bounceCount = 0;
        }
    }
}