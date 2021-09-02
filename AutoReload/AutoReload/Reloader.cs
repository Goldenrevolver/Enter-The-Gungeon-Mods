using UnityEngine;

namespace AutoReload
{
    internal class Reloader : MonoBehaviour
    {
        public void Update()
        {
            // the variable checking is exactly the same as it would be in PlayerController.Update
            if (AutoReload.EmptyClipReload && !BraveUtility.isLoadingLevel && !GameManager.Instance.IsLoadingLevel)
            {
                foreach (var player in GameManager.Instance.AllPlayers)
                {
                    if (player)
                    {
                        Gun currentGun = player.CurrentGun;

                        // the commented out version is the original code from the GameUIRoot class for checking if the clip is empty (for showing the ui message 'reload')
                        // if (currentGun && currentGun.ClipShotsRemaining == 0 && (currentGun.ClipCapacity > 1 || currentGun.ammo == 0) && !currentGun.IsReloading && !player.IsInputOverridden && !currentGun.IsHeroSword)
                        if (currentGun && currentGun.ClipShotsRemaining == 0 && currentGun.ClipCapacity > 1 && currentGun.ammo > 0 && !currentGun.IsReloading && !player.IsInputOverridden && !currentGun.IsHeroSword)
                        {
                            Reload(player);
                        }
                    }
                }
            }
        }

        public static void Reload(PlayerController player)
        {
            if (AutoReload.UseExceptions)
            {
                // checks whether AutoReload should be disabled when you have the Turbo Gun, the Rad Gun (normal or synergy version) or Cog Of Battle equipped or any other modded items that use the base game implementation
                if (player.CurrentGun && (player.CurrentGun.GetComponent<RechargeGunModifier>() || player.CurrentGun.LocalActiveReload || (player.IsPrimaryPlayer && Gun.ActiveReloadActivated) || (!player.IsPrimaryPlayer && Gun.ActiveReloadActivatedPlayerTwo)))
                {
                    return;
                }
            }

            // original code is in PlayerController.HandlePlayerInput, called in PlayerController.Update
            if (player.AcceptingAnyInput && player.AcceptingNonMotionInput && player.CurrentGun)
            {
                player.CurrentGun.Reload();

                player.CurrentGun.OnReloadPressed?.Invoke(player, player.CurrentGun, true);

                if (player.CurrentSecondaryGun)
                {
                    player.CurrentSecondaryGun.Reload();

                    player.CurrentSecondaryGun.OnReloadPressed?.Invoke(player, player.CurrentSecondaryGun, true);
                }

                player.OnReloadPressed?.Invoke(player, player.CurrentGun);
            }
        }
    }
}