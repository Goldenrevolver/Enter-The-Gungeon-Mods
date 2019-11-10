using UnityEngine;

namespace SkipToFoyer
{
    public class SkipToFoyer : ETGModule
    {
        public static bool UseQuickStart;

        public override void Exit() { }

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
        }
    }
}
