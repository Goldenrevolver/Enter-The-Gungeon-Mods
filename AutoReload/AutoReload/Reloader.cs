using UnityEngine;

namespace AutoReload
{
    internal class Reloader : MonoBehaviour
    {
        public void Update()
        {
            //the variable checking is exactly the same as it would be in PlayerController.Update
            if (AutoReload.EmptyClipReload && !BraveUtility.isLoadingLevel && !GameManager.Instance.IsLoadingLevel)
            {
                PlayerController[] players = GameManager.Instance.AllPlayers;
                foreach (var player in players)
                {
                    if (player != null)
                    {
                        Gun currentGun = player.CurrentGun;
                        //the commented out version is the original code from the GameUIRoot class for checking if the clip is empty (for showing the ui message 'reload')
                        //if (currentGun != null && currentGun.ClipShotsRemaining == 0 && (currentGun.ClipCapacity > 1 || currentGun.ammo == 0) && !currentGun.IsReloading && !player.IsInputOverridden && !currentGun.IsHeroSword)
                        if (currentGun != null && currentGun.ClipShotsRemaining == 0 && currentGun.ClipCapacity > 1 && currentGun.ammo > 0 && !currentGun.IsReloading && !player.IsInputOverridden && !currentGun.IsHeroSword)
                        {
                            Reload(player);
                        }
                    }
                }
            }
        }

        public static void Reload(PlayerController player)
        {
            // if AutoReload should be disabled when you have the Rad Gun or Cog Of Battle equipped
            if (AutoReload.UseExceptions && (player.CurrentGun.PickupObjectId == 556 || player.HasPassiveItem(135)))
            {
                return;
            }

            //delegate is not simplified on purpose as this is the original code from the HandlePlayerInput method called in the Update method of the PlayerController class
            if (player.AcceptingAnyInput && player.AcceptingNonMotionInput)
            {
                if (player.CurrentGun != null)
                {
                    player.CurrentGun.Reload();
                    if (player.CurrentGun.OnReloadPressed != null)
                    {
                        player.CurrentGun.OnReloadPressed(player, player.CurrentGun, true);
                    }
                    if (player.CurrentSecondaryGun)
                    {
                        player.CurrentSecondaryGun.Reload();
                        if (player.CurrentSecondaryGun.OnReloadPressed != null)
                        {
                            player.CurrentSecondaryGun.OnReloadPressed(player, player.CurrentSecondaryGun, true);
                        }
                    }
                    if (player.OnReloadPressed != null)
                    {
                        player.OnReloadPressed(player, player.CurrentGun);
                    }
                }
            }
        }
    }
}