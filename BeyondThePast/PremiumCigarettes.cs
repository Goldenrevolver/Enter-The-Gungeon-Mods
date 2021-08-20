using ItemAPI;
using UnityEngine;

namespace BeyondThePast
{
    public class PremiumCigarettes : SpawnObjectPlayerItem
    {
        public static int PremiumCigarettesID;

        public static void Register()
        {
            //The name of the item
            string itemName = "Premium Cigarettes";

            //Refers to an embedded png in the project. Make sure to embed your resources! Google it
            string resourceName = "BeyondThePast/Resources/Cigarettes";

            //Create new GameObject
            GameObject obj = new GameObject(itemName);

            //Add a PassiveItem component to the object
            var item = obj.AddComponent<PremiumCigarettes>();

            //Adds a sprite component to the object and adds your texture to the item sprite collection
            ItemBuilder.AddSpriteToObject(itemName, resourceName, obj);

            //Ammonomicon entry variables
            string shortDesc = "Hazardous To Health";
            string longDesc = "Premium or not, can result in serious health problems and even death. They do have a certain appeal though.";

            //Adds the item to the gungeon item list, the ammonomicon, the loot table, etc.
            //Do this after ItemBuilder.AddSpriteToObject!
            ItemBuilder.SetupItem(item, shortDesc, longDesc, "gr");
            PremiumCigarettesID = item.PickupObjectId;

            var cigarettes = PickupObjectDatabase.GetById(203) as SpawnObjectPlayerItem;

            foreach (var publicField in typeof(SpawnObjectPlayerItem).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
            {
                publicField.SetValue(item, publicField.GetValue(cigarettes));
            }

            item.damageCooldown = cigarettes.damageCooldown;
            item.roomCooldown = cigarettes.roomCooldown;
            item.timeCooldown = cigarettes.timeCooldown;

            //Set the rarity of the item
            item.quality = PickupObject.ItemQuality.EXCLUDED;
        }

        private readonly bool damageIncreasesWithEveryUse = false;

        public override bool CanBeUsed(PlayerController user)
        {
            return user.characterIdentity != PlayableCharacters.Robot && !user.ForceZeroHealthState && base.CanBeUsed(user);
        }

        protected override void DoEffect(PlayerController user)
        {
            float damage = 0.5f;

            if (damageIncreasesWithEveryUse)
            {
                var counter = user.GetComponent<CigarettesCounter>();

                float cigarettesUsed;

                if (counter)
                {
                    counter.CigarettesUsed++;
                    cigarettesUsed = counter.CigarettesUsed;
                }
                else
                {
                    user.gameObject.AddComponent<CigarettesCounter>();
                    cigarettesUsed = 1;
                }

                damage *= cigarettesUsed;
            }

            // save the HasTakenDamage values before the ApplyDamage
            bool dRoom = false;
            bool dRun = user.HasTakenDamageThisRun;
            bool dFloor = user.HasTakenDamageThisFloor;

            if (user.CurrentRoom != null)
            {
                dRoom = user.CurrentRoom.PlayerHasTakenDamageInThisRoom;
            }

            user.healthHaver.NextDamageIgnoresArmor = true;
            user.healthHaver.ApplyDamage(damage, Vector2.zero, StringTableManager.GetEnemiesString("#SMOKING", -1), CoreDamageTypes.None, DamageCategory.Normal, true, null, false);

            // reapply the previous HasTakenDamage values from before the ApplyDamage
            user.HasTakenDamageThisRun = dRun;
            user.HasTakenDamageThisFloor = dFloor;

            if (user.CurrentRoom != null)
            {
                user.CurrentRoom.PlayerHasTakenDamageInThisRoom = dRoom;
            }

            // give coolness as usual
            StatModifier statModifier = new StatModifier
            {
                statToBoost = PlayerStats.StatType.Coolness,
                modifyType = StatModifier.ModifyMethod.ADDITIVE,
                amount = 1f
            };
            user.ownerlessStatModifiers.Add(statModifier);
            user.stats.RecalculateStats(user, false, false);

            // do other things that the cigarettes want to do outside the iscigarettes conditional statement (like play sound and spawn the cigarettes object)
            this.IsCigarettes = false;
            base.DoEffect(user);
            this.IsCigarettes = true;
        }
    }

    public class CigarettesCounter : MonoBehaviour
    {
        public int CigarettesUsed = 1;
    }
}