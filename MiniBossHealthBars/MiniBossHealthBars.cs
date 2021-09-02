using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;

namespace MiniBossHealthBars
{
    public class MiniBossHealthBars : ETGModule
    {
        public static readonly string MOD_NAME = "Mini Boss Health Bars";

        // other mods can add their boss guids to this list (with reflection, or a dependency if they really want to)
        public static readonly List<string> horizontalMiniBossGuidList = new List<string>() { "edc61b105ddd4ce18302b82efdc47178", "db97e486ef02425280129e1e27c33118" };

        public override void Init()
        {
        }

        public override void Start()
        {
            try
            {
                new Hook(typeof(HealthHaver).GetProperty("HasHealthBar").GetGetMethod(), typeof(MiniBossHealthBars).GetMethod(nameof(MiniBossHealthBars.HasHealthBarHook)));
                new Hook(typeof(HealthHaver).GetProperty("UsesVerticalBossBar").GetGetMethod(), typeof(MiniBossHealthBars).GetMethod(nameof(MiniBossHealthBars.UsesVerticalBossBarHook)));
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

        // for reference:
        // bb73eeeb9e584fbeaf35877ec176de28 blockner is a main boss
        // edc61b105ddd4ce18302b82efdc47178 blockner_rematch is a sub boss
        // 39de9bd6a863451a97906d949c103538 tsar_bomba is a sub boss
        // db97e486ef02425280129e1e27c33118 shadow_agunim is a sub boss
        // these above 3 are the only sub bosses in the entire enemydatabase entries list

        public static bool HasHealthBarHook(Func<HealthHaver, bool> orig, HealthHaver self)
        {
            return self.IsSubboss || orig(self);
        }

        public static bool UsesVerticalBossBarHook(Func<HealthHaver, bool> orig, HealthHaver self)
        {
            // fuselier and all modded mini bosses get vertical health bars (just to be safe for modded ones, so blockner rematch and shadow agunim get horizontal ones)
            bool getsHorizontal = self.aiActor && horizontalMiniBossGuidList.Contains(self.aiActor.EnemyGuid);
            return orig(self) || (self.IsSubboss && !getsHorizontal);
        }
    }
}