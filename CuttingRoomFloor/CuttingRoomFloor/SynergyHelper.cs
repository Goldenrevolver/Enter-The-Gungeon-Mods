using ItemAPI;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoomFloor
{
    public class SynergyHelper
    {
        public static List<string> synergiesToSetToActive = new List<string>() { "#TRASHJUNKAN", "#NANOMACHINES", "#HEARTUNLOCKET", "#WORLDWAR", "#DOUBLECHESTFRIENDS", "#MENINBLACK", "#THESTARWAR", "#BOWLING", "#HL2ROCKET" };
        public static List<string> synergiesToSetToActiveUnboosted = new List<string>() { "#PLATINUMGOLD", "#MOONRAKING_IT_IN", "#PLANPAIN" };
        public static List<string> starterSynergies = new List<string>() { "#DOUBLEMOLOTOV", "#MASTEROFUNLOCKING", "#ROBOTHANDS", "#SUPPLYDROP", "#HEROOFCHICKEN", "#EVENACHILD", "#TRUEHERO", "#DOGANDWOLF" };

        // things I could reimplement: ROLLSPHERE, SPONGE_CUBE, POISON_HOOK synergy, draguns heart and BLOOD locket synergy
        // Item Bullets, Key Bullet Bullets

        // in case I want to balance HL2ROCKET in the future

        //var rc = PickupObjectDatabase.GetById(372) as Gun;
        //var proj = rc.DefaultModule.projectiles[0].gameObject.GetComponent<ScalingProjectileModifier>();
        //Tools.Log(proj.PercentGainPerUnit); //4
        //Tools.Log(proj.ScaleMultiplier); //1
        //Tools.Log(proj.DamageMultiplier); //1
        //Tools.Log(proj.MaximumDamageMultiplier); //-1
        //Tools.Log(proj.ScaleToDamageRatio); //1

        //var moon = PickupObjectDatabase.GetById(20) as Gun;
        //var moonsynergy = PickupObjectDatabase.GetById(713) as Gun;
        //var banana = PickupObjectDatabase.GetById(478) as Gun;
        //var bananasynergy = PickupObjectDatabase.GetById(688) as Gun;
        //var r = PickupObjectDatabase.GetById(340) as Gun;
        //var rsynergy = PickupObjectDatabase.GetById(738) as Gun;

        public static void EnableAndFixSynergies(bool enableStarterSynergies)
        {
            foreach (var synergy in GameManager.Instance.SynergyManager.synergies)
            {
                try
                {
                    if (synergiesToSetToActive.Contains(synergy.NameKey))
                    {
                        synergy.ActivationStatus = SynergyEntry.SynergyActivation.ACTIVE;
                    }

                    if (synergiesToSetToActiveUnboosted.Contains(synergy.NameKey))
                    {
                        synergy.ActivationStatus = SynergyEntry.SynergyActivation.ACTIVE_UNBOOSTED;
                    }

                    if (starterSynergies.Contains(synergy.NameKey))
                    {
                        synergy.ActivationStatus = enableStarterSynergies ? SynergyEntry.SynergyActivation.ACTIVE_UNBOOSTED : SynergyEntry.SynergyActivation.INACTIVE;
                        synergy.IgnoreLichEyeBullets = true;
                    }

                    if (synergy.ActivationStatus == SynergyEntry.SynergyActivation.INACTIVE && synergy.bonusSynergies.Contains(CustomSynergyType.BIG_TABLE_SHOTGUN))
                    {
                        synergy.NameKey = "Hidden Tech Shotgun";
                        synergy.ActivationStatus = SynergyEntry.SynergyActivation.ACTIVE;
                    }

                    if (synergy.ActivationStatus == SynergyEntry.SynergyActivation.INACTIVE && synergy.bonusSynergies.Contains(CustomSynergyType.SHARDBLADE))
                    {
                        synergy.NameKey = "Shard Blade";
                        synergy.ActivationStatus = SynergyEntry.SynergyActivation.ACTIVE;
                    }

                    if (synergy.ActivationStatus == SynergyEntry.SynergyActivation.INACTIVE && synergy.bonusSynergies.Contains(CustomSynergyType.HIGH_ENERGY))
                    {
                        synergy.NameKey = "High Energy";
                        synergy.ActivationStatus = SynergyEntry.SynergyActivation.ACTIVE;
                    }

                    if (synergy.NameKey == "#RIOTGEAR")
                    {
                        synergy.ActivationStatus = SynergyEntry.SynergyActivation.ACTIVE_UNBOOSTED;
                        synergy.MandatoryItemIDs = new List<int>() { 442 }; // badge
                        synergy.MandatoryGunIDs = new List<int>();
                        synergy.OptionalGunIDs = new List<int>() { 30, 62, 50, 223 }; // M1911, Colt 1851, SAA, Cold 45
                        synergy.OptionalItemIDs = new List<int>();
                        synergy.bonusSynergies = new List<CustomSynergyType>() { CustomSynergyType.COP_VEST };
                        synergy.ActiveWhenGunUnequipped = true;
                        synergy.NumberObjectsRequired = 2;
                    }

                    if (synergy.NameKey == "#LOWER_CASE_R")
                    {
                        synergy.bonusSynergies = new List<CustomSynergyType>() { CustomSynergyType.LOWER_CASE_HACK };
                        synergy.MandatoryGunIDs.Remove(340);// lower case r
                        synergy.OptionalGunIDs.Add(340); // lower case r
                        synergy.OptionalGunIDs.Add(649); // upper case r, this is added to fix a bug when changing from one form change to another
                    }

                    if (synergy.NameKey == "#DIAZEPAM")
                    {
                        if (enableStarterSynergies)
                        {
                            if (!synergy.OptionalGunIDs.Contains(12))
                            {
                                synergy.OptionalGunIDs.Add(12);
                            }
                        }
                        else
                        {
                            synergy.OptionalGunIDs.RemoveAll((x) => x == 12);
                        }

                        // the form change ones should work automatically
                        synergy.OptionalGunIDs.Add(178); // crestfaller
                        //synergy.OptionalGunIDs.Add(687); // crestfaller + five_oclock_somewhere
                        synergy.OptionalGunIDs.Add(156); // laser lotus
                        synergy.OptionalGunIDs.Add(153); // shock_rifle
                        //synergy.OptionalGunIDs.Add(697); // shock_rifle + battery_powered
                        synergy.OptionalGunIDs.Add(13);  // thunderclap
                        //synergy.OptionalGunIDs.Add(683); // thunderclap + alistairs_ladder
                    }

                    if (synergy.NameKey == "#BATTERY_POWERED")
                    {
                        if (enableStarterSynergies)
                        {
                            if (!synergy.OptionalItemIDs.Contains(410))
                            {
                                synergy.OptionalItemIDs.Add(410);
                            }

                            var shotSpeedBonus = new StatModifier
                            {
                                statToBoost = PlayerStats.StatType.ProjectileSpeed,
                                modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE,
                                amount = 2
                            };

                            synergy.statModifiers = new List<StatModifier>() { shotSpeedBonus };
                        }
                        else
                        {
                            synergy.OptionalItemIDs.RemoveAll((x) => x == 410);
                            synergy.statModifiers = new List<StatModifier>();
                        }
                    }

                    if (synergy.NameKey == "#FISH_BACKUP")
                    {
                        // seems like it was a bug the entire time due to adding one non-gun
                        synergy.RequiresAtLeastOneGunAndOneItem = false;
                    }

                    if (synergy.NameKey == "#EVENACHILD")
                    {
                        synergy.MandatoryGunIDs.RemoveAll((x) => x == 24);
                        synergy.OptionalGunIDs = new List<int>() { 24, 811 };
                    }

                    if (synergy.NameKey == "#ROBOTHANDS")
                    {
                        synergy.MandatoryGunIDs.RemoveAll((x) => x == 88);
                        synergy.OptionalGunIDs = new List<int>() { 88, 812 };

                        var robotsLeftHand = PickupObjectDatabase.GetById(576) as Gun;

                        var dualWieldSwitcher = robotsLeftHand.gameObject.AddComponent<CustomDualWieldSynergySwitcher>();

                        dualWieldSwitcher.firstDualWieldGun = 88;
                        dualWieldSwitcher.secondDualWieldGun = 812;

                        robotsLeftHand.gameObject.AddComponent<CustomSynergyHandRemover>().SynergyToCheck = CustomSynergyType.ROBOT_HANDS;
                        PickupObjectDatabase.GetById(88).gameObject.AddComponent<CustomSynergyHandRemover>().SynergyToCheck = CustomSynergyType.ROBOT_HANDS;
                        PickupObjectDatabase.GetById(812).gameObject.AddComponent<CustomSynergyHandRemover>().SynergyToCheck = CustomSynergyType.ROBOT_HANDS;
                    }

                    if (synergy.NameKey == "#HEROOFCHICKEN")
                    {
                        synergy.MandatoryGunIDs.RemoveAll((x) => x == 417);
                        synergy.OptionalGunIDs = new List<int>() { 417, 813 };
                    }

                    if (synergy.ActivationStatus == SynergyEntry.SynergyActivation.INACTIVE)
                    {
                        if (synergy.MandatoryGunIDs != null && synergy.MandatoryGunIDs.Contains(10) && synergy.MandatoryGunIDs.Contains(24))
                        {
                            synergy.MandatoryGunIDs.RemoveAll((x) => x == 24);
                            synergy.OptionalGunIDs = new List<int>() { 24, 811 };

                            synergy.NameKey = "Harmless Fun";
                            synergy.ActivationStatus = enableStarterSynergies ? SynergyEntry.SynergyActivation.ACTIVE_UNBOOSTED : SynergyEntry.SynergyActivation.INACTIVE;
                            synergy.IgnoreLichEyeBullets = true;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    ETGModConsole.Log("Exception while reenabling inactive synergies: " + e);
                }
            }

            // make the unused cool tear jerker shoot 2 bullets/ tears
            var coolTearJerker = PickupObjectDatabase.GetById(725) as Gun;

            // create a new projectile because it uses the same projectile as the base tear jerker and this one needs to do less damage and be smaller
            Projectile projectile = Object.Instantiate(coolTearJerker.DefaultModule.projectiles[0]);

            projectile.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(projectile.gameObject);
            Object.DontDestroyOnLoad(projectile);

            coolTearJerker.DefaultModule.projectiles[0] = projectile;

            projectile.baseData.damage = 5f;
            projectile.AdditionalScaleMultiplier = 0.75f;

            coolTearJerker.DefaultModule.mirror = true;
            coolTearJerker.DefaultModule.positionOffset = new Vector3(0f, 0.25f, 0f);

            // add the custom synergy processor, so that it doesn't clash with the directional pad transformation
            var normalTearJerker = PickupObjectDatabase.GetById(33) as Gun;

            var synergyProcessor = normalTearJerker.gameObject.AddComponent<CustomTransformGunSynergyProcessor>();

            string synergyName = "20/20 Tears";

            synergyProcessor.NonSynergyGunId = 33;
            synergyProcessor.SynergyGunId = 725;
            synergyProcessor.SynergyToActivateTransformation = synergyName;
            synergyProcessor.SynergyToSuppressTransformation = CustomSynergyType.ISAACLIKE;

            AdvancedSynergyEntry entry = new AdvancedSynergyEntry()
            {
                NameKey = synergyName,
                ActivationStatus = SynergyEntry.SynergyActivation.ACTIVE_UNBOOSTED,
                MandatoryItemIDs = new List<int>() { 290 }, // Sunglasses
                MandatoryGunIDs = new List<int>() { 33 }, // Tear Jerker
                OptionalItemIDs = new List<int>(),
                OptionalGunIDs = new List<int>(),
                bonusSynergies = new List<CustomSynergyType>(),
                statModifiers = new List<StatModifier>(),
                IgnoreLichEyeBullets = false,
                RequiresAtLeastOneGunAndOneItem = true,
                NumberObjectsRequired = 2,
                ActiveWhenGunUnequipped = false
            };

            CustomSynergies.Add(entry);

            entry = new AdvancedSynergyEntry()
            {
                NameKey = "Irradiated Ones",
                ActivationStatus = SynergyEntry.SynergyActivation.ACTIVE,
                MandatoryItemIDs = new List<int>() { 204, 342 }, // Uranium Ammolet + Irradiated Lead
                MandatoryGunIDs = new List<int>(),
                OptionalItemIDs = new List<int>(),
                OptionalGunIDs = new List<int>(),
                bonusSynergies = new List<CustomSynergyType>() { CustomSynergyType.RADIOACTIVE_JEWELRY },
                statModifiers = new List<StatModifier>(),
                IgnoreLichEyeBullets = false,
                RequiresAtLeastOneGunAndOneItem = false,
                NumberObjectsRequired = 2,
                ActiveWhenGunUnequipped = true
            };

            CustomSynergies.Add(entry);

            entry = new AdvancedSynergyEntry()
            {
                NameKey = "Grasscutter",
                ActivationStatus = SynergyEntry.SynergyActivation.ACTIVE,
                MandatoryItemIDs = new List<int>(),
                MandatoryGunIDs = new List<int>() { 177, 180 }, // alien engine + grasschopper
                OptionalItemIDs = new List<int>(),
                OptionalGunIDs = new List<int>(),
                bonusSynergies = new List<CustomSynergyType>() { CustomSynergyType.GRASSCUTTER },
                statModifiers = new List<StatModifier>(),
                IgnoreLichEyeBullets = false,
                RequiresAtLeastOneGunAndOneItem = false,
                NumberObjectsRequired = 2,
                ActiveWhenGunUnequipped = false
            };

            CustomSynergies.Add(entry);

            entry = new AdvancedSynergyEntry()
            {
                NameKey = "#DEMONHUNTER",
                ActivationStatus = enableStarterSynergies ? SynergyEntry.SynergyActivation.ACTIVE_UNBOOSTED : SynergyEntry.SynergyActivation.INACTIVE,
                MandatoryItemIDs = new List<int>() { 538 }, // silver bullets
                MandatoryGunIDs = new List<int>() { 12 }, // crossbow
                OptionalItemIDs = new List<int>(),
                OptionalGunIDs = new List<int>(),
                bonusSynergies = new List<CustomSynergyType>() { CustomSynergyType.DEMONHUNTER },
                statModifiers = new List<StatModifier>(),
                IgnoreLichEyeBullets = true,
                RequiresAtLeastOneGunAndOneItem = true,
                NumberObjectsRequired = 2,
                ActiveWhenGunUnequipped = false
            };

            CustomSynergies.Add(entry);
        }

        public static void UpdateStarterSynergyStatus(bool enableStarterSynergies)
        {
            foreach (var synergy in GameManager.Instance.SynergyManager.synergies)
            {
                if (starterSynergies.Contains(synergy.NameKey))
                {
                    synergy.ActivationStatus = enableStarterSynergies ? SynergyEntry.SynergyActivation.ACTIVE_UNBOOSTED : SynergyEntry.SynergyActivation.INACTIVE;
                }

                if (synergy.NameKey == "#DIAZEPAM")
                {
                    if (enableStarterSynergies)
                    {
                        if (!synergy.OptionalGunIDs.Contains(12))
                        {
                            synergy.OptionalGunIDs.Add(12);
                        }
                    }
                    else
                    {
                        synergy.OptionalGunIDs.RemoveAll((x) => x == 12);
                    }
                }

                if (synergy.NameKey == "#BATTERY_POWERED")
                {
                    if (enableStarterSynergies)
                    {
                        if (!synergy.OptionalItemIDs.Contains(410))
                        {
                            synergy.OptionalItemIDs.Add(410);
                        }

                        var shotSpeedBonus = new StatModifier
                        {
                            statToBoost = PlayerStats.StatType.ProjectileSpeed,
                            modifyType = StatModifier.ModifyMethod.MULTIPLICATIVE,
                            amount = 2
                        };

                        synergy.statModifiers = new List<StatModifier>() { shotSpeedBonus };
                    }
                    else
                    {
                        synergy.OptionalItemIDs.RemoveAll((x) => x == 410);
                        synergy.statModifiers = new List<StatModifier>();
                    }
                }

                if (synergy.NameKey == "#DEMONHUNTER" || synergy.NameKey == "Harmless Fun")
                {
                    synergy.ActivationStatus = enableStarterSynergies ? SynergyEntry.SynergyActivation.ACTIVE_UNBOOSTED : SynergyEntry.SynergyActivation.INACTIVE;
                }
            }
        }
    }
}