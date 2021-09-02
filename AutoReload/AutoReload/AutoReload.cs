using MonoMod.RuntimeDetour;
using System;
using UnityEngine;

namespace AutoReload
{
    public class AutoReload : ETGModule
    {
        public static readonly string MOD_NAME = "AutoReload";

        public static bool EmptyClipReload;
        public static bool ClearedRoomReload;
        public static bool UseExceptions;

        // init is too early for using ETGModConsole
        public override void Init()
        {
            // default true
            EmptyClipReload = PlayerPrefs.GetInt("AutoReloadOnEmptyClip", 1) == 1;

            // default false
            ClearedRoomReload = PlayerPrefs.GetInt("AutoReloadOnClearedRoom", 0) == 1;

            // default false
            UseExceptions = PlayerPrefs.GetInt("AutoReloadUseExceptions", 0) == 1;

            GameObject reloadManagerObject = new GameObject("Reload Manager");
            reloadManagerObject.AddComponent<Reloader>();
            UnityEngine.Object.DontDestroyOnLoad(reloadManagerObject);
        }

        // exit is not getting called at all, so I have to save settings when I change them
        public override void Exit() { }

        public override void Start()
        {
            try
            {
                // AddGroup doesn't return the correct group for some reason so I have to get it again
                ETGModConsole.Commands.AddGroup("autoReload");
                ETGModConsole.Commands.GetGroup("autoReload").AddUnit("emptyClip", delegate (string[] e)
                {
                    // flips the bool value
                    EmptyClipReload ^= true;
                    ETGModConsole.Log("AutoReload on empty clip: " + EmptyClipReload);
                    PlayerPrefs.SetInt("AutoReloadOnEmptyClip", EmptyClipReload ? 1 : 0);
                    PlayerPrefs.Save();
                }).AddUnit("clearedRoom", delegate (string[] e)
                {
                    // flips the bool value
                    ClearedRoomReload ^= true;
                    ETGModConsole.Log("AutoReload on cleared room: " + ClearedRoomReload);
                    PlayerPrefs.SetInt("AutoReloadOnClearedRoom", ClearedRoomReload ? 1 : 0);
                    PlayerPrefs.Save();
                }).AddUnit("useExceptions", delegate (string[] e)
                {
                    // flips the bool value
                    UseExceptions ^= true;
                    ETGModConsole.Log("Use AutoReload exceptions: " + UseExceptions);
                    PlayerPrefs.SetInt("AutoReloadUseExceptions", UseExceptions ? 1 : 0);
                    PlayerPrefs.Save();
                });

                try
                {
                    Hook hook = new Hook(typeof(PlayerController).GetMethod(nameof(PlayerController.OnRoomCleared)), typeof(AutoReload).GetMethod(nameof(AutoReload.OnRoomClearedHook)));
                }
                catch (Exception e)
                {
                    ETGModConsole.Log($"<color=red>Exception whilst setting up hooks: {e}</color>");
                }

                ETGModConsole.Log($"{MOD_NAME} v{Metadata.Version} initialized (on empty clip: {EmptyClipReload}, on cleared room: {ClearedRoomReload}, use exceptions: {UseExceptions})");
            }
            catch (Exception e)
            {
                ETGModConsole.Log($"<color=red>Exception in Start: {e}</color>");
            }
        }

        public static void OnRoomClearedHook(Action<PlayerController> orig, PlayerController self)
        {
            orig(self);

            if (ClearedRoomReload && self)
            {
                Gun currentGun = self.CurrentGun;

                // similar to the code in Reloader.Update
                if (currentGun && currentGun.ClipCapacity > 1 && currentGun.ammo > 0 && !currentGun.IsReloading && !self.IsInputOverridden && !currentGun.IsHeroSword)
                {
                    Reloader.Reload(self);
                }
            }
        }
    }
}