using System;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("BodyCount", "Kladskull", "0.0.1")]
    [Description("Keeps track of deaths in Rust and reports on it.")]
    public class BodyCount : CovalencePlugin
    {
        private void OnPluginLoaded(Plugin plugin)
        {
            LogWarning($"Plugin '{plugin.Name}' has been loaded");
        }

        private void OnEntityDeath(BaseCombatEntity victimEntity, HitInfo hitInfo)
        {
            if (victimEntity == null || hitInfo == null)
                return;

            var prefab = hitInfo.Initiator?.GetComponent<Flame>()?.SourceEntity?.ShortPrefabName ??
                         hitInfo.WeaponPrefab?.ShortPrefabName;

            var playerVictim = (BasePlayer) victimEntity;
            var playerMurderer = (BasePlayer) hitInfo.Initiator;
            var steamIdVictim = playerVictim.IPlayer.Id;
            var steamIdMurderer = playerMurderer.IPlayer.Id;

            try
            {
                webrequest.EnqueueGet(
                    $"http://bodycount.net/kill.php?steamIdVictim={steamIdVictim}&steamIdMurderer={steamIdMurderer}&playerVictim={playerVictim.displayName}&playerMurderer={playerMurderer.displayName}&weaponName={prefab}",
                    null, this);
            }
            catch
            {
                // ignore any web errors 
            }
        }

        [Command("top")]
        private void LeaderBoard(IPlayer player, string cmd, string[] args)
        {
            webrequest.Enqueue("https://bodycount.net/top10.php", null, (code, response) =>
            {
                if (code != 200 || response == null)
                {
                    player.Message("Couldn't get an answer from BodyCount! Try again later.");
                    Puts($"Couldn't get an answer from BodyCount!");
                    return;
                }

                var header = "Top 10 Killers" + Environment.NewLine + "--------------" + Environment.NewLine;
                player.Message(header + response);
            }, this, RequestMethod.GET);
        }

        private class Flame
        {
            public enum FlameSource
            {
                Flamethrower,
                IncendiaryProjectile
            }

            public FlameSource Source { get; set; }
            public BaseEntity SourceEntity { get; set; }
            public BaseEntity Initiator { get; set; }
        }
    }
}