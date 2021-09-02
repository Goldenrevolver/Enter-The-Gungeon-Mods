using System;
using UnityEngine;

namespace SkipToFoyer
{
    public class SkipToFoyer : ETGModule
    {
        public static bool UseQuickStart;

        public override void Exit()
        {
        }

        public override void Init()
        {
            UseQuickStart = PlayerPrefs.GetInt("SkipToFoyerUseQuickStart", 0) == 1;

            if (UseQuickStart)
            {
                GameObject updater = new GameObject("Skip Intro");
                updater.AddComponent<Updater>();
                UnityEngine.Object.DontDestroyOnLoad(updater);
            }
            else
            {
                Foyer.DoIntroSequence = false;
                Foyer.DoMainMenu = false;
            }
        }

        public override void Start()
        {
            ETGModConsole.Commands.AddGroup("skipIntro");
            ETGModConsole.Commands.GetGroup("skipIntro").AddUnit("toggleQuickStart", delegate (string[] e)
            {
                //flips the bool value
                UseQuickStart ^= true;
                ETGModConsole.Log("Use quick start: " + UseQuickStart);
                PlayerPrefs.SetInt("SkipToFoyerUseQuickStart", UseQuickStart ? 1 : 0);
                PlayerPrefs.Save();
            });

            //new Hook(typeof(BraveInput).GetProperty(nameof(BraveInput.MenuInteractPressed)).GetGetMethod(), typeof(SkipToFoyer).GetMethod(nameof(SkipToFoyer.MenuInteractPressedOverride));
        }

        //// hook for skip boss intro/outro
        //public bool MenuInteractPressed
        //{
        //    get
        //    {
        //        return this.ActiveActions != null && (this.ActiveActions.InteractAction.WasPressed || this.ActiveActions.MenuSelectAction.WasPressed);
        //    }
        //}

        // TODO there must be a hard override for the boss animations anyway as boss rush I think doesn't have them

        public static bool overrideMenuPress = false;

        public static bool MenuInteractPressedOverride(Func<BraveInput, bool> orig, BraveInput self)
        {
            var ret = orig(self) || (self.ActiveActions != null && overrideMenuPress);

            return ret;
        }
    }
}