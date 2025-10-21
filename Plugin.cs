using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace BossHealthChanger
{
    public record ModMetadata : AbstractModMetadata
    {
        public override string ModGuid { get; init; } = "com.SkebbZ.BossHealthChanger";
        public override string Name { get; init; } = "Boss Health Changer";
        public override string Author { get; init; } = "SkebbZ";
        public override Version Version { get; init; } = new("1.0.0");
        public override Range SptVersion { get; init; } = new("~4.0.1");
        public override string License { get; init; } = "MIT";
        public override List<string>? Contributors { get; init; } = null;
        public override List<string>? Incompatibilities { get; init; } = null;
        public override Dictionary<string, Range>? ModDependencies { get; init; } = null;
        public override string? Url { get; init; } = null;
        public override bool? IsBundleMod { get; init; } = null;
    }

    [Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
    public class BossHealthChanger(
        ISptLogger<BossHealthChanger> logger,
        DatabaseService databaseService)
        : IOnLoad
    {
        private static readonly List<string> BossRoles = new List<string>
        {
            "bossbully", "bossboar", "bossboarsniper", "bossgluhar", "bosskilla", "bosskillaagro", "bosskojaniy", "bosskolontay",
            "bosssanitar", "bosstagilla", "bosstagillaagro", "bosszryachiy", "sectantpriest", "sectantwarrior", "bosspartisan",
            "sectantprizrak", "followerbully", "followergluharassault", "followerboar", "followerboarclose1", "followerboarclose2",
            "followergluharscout", "followergluharsecurity", "followergluharsnipe", "sectantpredvestnik", "sectantoni",
            "followerkilla", "followersanitar", "followertagilla", "followerkojaniy", "followerkolontayassault", "followerkolontaysecurity",
            "followerzryachiy", "gifter", "followerbigpipe", "followerbirdeye", "bossknight", "arenafighterevent", "crazyassaultevent", "cursedassault", "exusec", "pmcbot"
        };

        public Task OnLoad()
        {
            logger.Info("[Boss Health Changer]: Mod loading...");
            try { 
                ModifyBossHealth();
                logger.Success("[Boss Health Changer]: Finished applying health changes!");
            }
            catch (System.Exception ex)
            {
                logger.Error($"[Boss Health Changer]: An error occurred while applying health changes: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        private void ModifyBossHealth()
        {
            var botTypes = databaseService.GetBots().Types;

            var newHealthBodyParts = new BodyPart[1];
            newHealthBodyParts[0] = new BodyPart
            {
                Chest = new MinMax<double> { Min = 85, Max = 85 },
                Head = new MinMax<double> { Min = 35, Max = 35 },
                LeftArm = new MinMax<double> { Min = 60, Max = 60 },
                RightArm = new MinMax<double> { Min = 60, Max = 60 },
                LeftLeg = new MinMax<double> { Min = 65, Max = 65 },
                RightLeg = new MinMax<double> { Min = 65, Max = 65 },
                Stomach = new MinMax<double> { Min = 70, Max = 70 }
            };

            int changedCount = 0;
            foreach (var bot in botTypes)
            {
                if (BossRoles.Contains(bot.Key.ToLower()))
                {
                    if (bot.Value != null && bot.Value.BotHealth != null)
                    {
                        bot.Value.BotHealth.BodyParts = newHealthBodyParts;
                        logger.Debug($"[Boss Health Changer]: Changed health for boss: {bot.Key}");
                        changedCount++;
                    }
                }
            }
            logger.Info($"[Boss Health Changer]: Successfully applied standard player/PMC health values to {changedCount} boss/special bot types.");
        }
    }
}