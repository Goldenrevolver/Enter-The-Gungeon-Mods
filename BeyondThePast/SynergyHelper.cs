namespace BeyondThePast
{
    internal class SynergyHelper
    {
        public static void Help()
        {
            foreach (AdvancedSynergyEntry synergy in GameManager.Instance.SynergyManager.synergies)
            {
                if (synergy == null)
                {
                    continue;
                }

                EmbarrassingPhoto.HandleSynergy(synergy);
                FakeHeroBandana.HandleSynergy(synergy);
                LonelinessCookie.HandleSynergy(synergy);
                MasterOfUnlocking.HandleSynergy(synergy);
                PremiumCigarettes.HandleSynergy(synergy);
                SupplySupport.HandleSynergy(synergy);
                CompassItem.HandleSynergy(synergy);
            }
        }

        public static string GenerateModEditedSynergyProblem(AdvancedSynergyEntry synergy, string originalItemName, string customItemName)
        {
            return $"Some mod edited a synergy containing a mandatory {originalItemName} item, so the {customItemName} item may have not been properly added to it:" + synergy.NameKey;
        }

        public static string GenerateModdedSynergyProblem(AdvancedSynergyEntry synergy, string originalItemName, string customItemName)
        {
            return $"There was a modded synergy with a mandatory {originalItemName} item, so I couldn't add the {customItemName} item to it:" + synergy.NameKey;
        }
    }
}