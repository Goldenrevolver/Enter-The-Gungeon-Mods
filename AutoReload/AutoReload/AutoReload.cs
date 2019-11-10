using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using UnityEngine;

namespace AutoReload
{
    public class AutoReload : ETGModule
    {
        private static readonly string version = "1.1";

        public static bool EmptyClipReload;
        public static bool ClearedRoomReload;

        //init is too early for using ETGModConsole
        public override void Init()
        {
            //default true
            EmptyClipReload = PlayerPrefs.GetInt("AutoReloadOnEmptyClip", 1) == 1;
            //default false
            ClearedRoomReload = PlayerPrefs.GetInt("AutoReloadOnClearedRoom", 0) == 1;

            GameObject reloadManagerObject = new GameObject("Reload Manager");
            reloadManagerObject.AddComponent<Reloader>();
            UnityEngine.Object.DontDestroyOnLoad(reloadManagerObject);
        }

        //exit is not getting called at all, so I have to save settings when I change them
        public override void Exit() { }

        public override void Start()
        {
            //AddGroup doesnt return the correct group for some reason so I have to get it again
            ETGModConsole.Commands.AddGroup("autoReload");
            ETGModConsole.Commands.GetGroup("autoReload").AddUnit("emptyClip", delegate (string[] e)
            {
                //flips the bool value
                EmptyClipReload ^= true;
                ETGModConsole.Log("AutoReload on empty clip: " + EmptyClipReload);
                PlayerPrefs.SetInt("AutoReloadOnEmptyClip", EmptyClipReload ? 1 : 0);
                PlayerPrefs.Save();
            }).AddUnit("clearedRoom", delegate (string[] e)
            {
                //flips the bool value
                ClearedRoomReload ^= true;
                ETGModConsole.Log("AutoReload on cleared room: " + ClearedRoomReload);
                PlayerPrefs.SetInt("AutoReloadOnClearedRoom", ClearedRoomReload ? 1 : 0);
                PlayerPrefs.Save();
            });

            Hook hook = new Hook(typeof(PlayerController).GetMethod("OnRoomCleared", BindingFlags.Public | BindingFlags.Instance), typeof(AutoReload).GetMethod("OnRoomClearedHook"));

            ETGModConsole.Log($"AutoReload v{version} initialized (on empty clip: {EmptyClipReload}, on cleared room: {ClearedRoomReload})");
        }

        public static void OnRoomClearedHook(Action<PlayerController> baseMethod, PlayerController player)
        {
            //if you dont call the base method, its not getting called at all (I wonder how that affects compatibility if you hook the same method)
            baseMethod(player);
            if (ClearedRoomReload && player != null)
            {
                Gun currentGun = player.CurrentGun;
                //similar to the description in Reloader.Update
                if (currentGun != null && currentGun.ClipCapacity > 1 && currentGun.ammo > 0 && !currentGun.IsReloading && !player.IsInputOverridden && !currentGun.IsHeroSword)
                {
                    Reloader.Reload(player);
                }
            }
        }
    }
}
