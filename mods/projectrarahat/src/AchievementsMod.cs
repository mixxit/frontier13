using Foundation.Extensions;
using Newtonsoft.Json;
using projectrarahat.src.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using static projectrarahat.src.AchievementsModState;

namespace projectrarahat.src
{
    /// <summary>
    /// Controls end of round events
    /// </summary>
    public class AchievementsMod : ModSystem
    {
        private ICoreServerAPI api;
        
        public override void StartPre(ICoreAPI api)
        {
            AchievementsModConfigFile.Current = api.LoadOrCreateConfig<AchievementsModConfigFile>($"{typeof(AchievementsMod).Name}.json");
            api.World.Config.SetBool("enableEndOfRoundPopup", AchievementsModConfigFile.Current.EnableEndOfRoundPopup);
            api.World.Config.SetString("classMonitoredAchievementsData", AchievementsModConfigFile.Current.ClassMonitoredAchievementsData);
            base.StartPre(api);
        }

        
        public override bool ShouldLoad(EnumAppSide side)
        {
            return true;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            this.api = api;
            this.api.Event.SaveGameLoaded += new Vintagestory.API.Common.Action(this.OnSaveGameLoaded);
            this.api.Event.PlayerNowPlaying += new PlayerDelegate(this.OnPlayerNowPlaying);
            this.api.Event.BreakBlock += new BlockBreakDelegate(this.OnPlayerBreakBlock);
            // Check every 8 secs if its end of round yet
            this.api.World.RegisterGameTickListener(OnGameTick, 8000);
            
        }

        private void OnSaveGameLoaded()
        {
            AchievementsModState.Instance.SetTrackedClassEvents(AchievementsModConfigFile.Current.GetClassMonitoredAchievements());
            LogUtils<CharClassMod>.LogInfo("AchievementMod started");
        }

        private void OnPlayerNowPlaying(IServerPlayer player)
        {
            RegisterPlayerClassChangedListener(player);
        }

        private void RegisterPlayerClassChangedListener(IServerPlayer player)
        {
            if (CharClassModState.Instance.GetCharacterClasses() == null || CharClassModState.Instance.GetCharacterClasses().Count < 1)
                return;

            player.Entity.WatchedAttributes.RegisterModifiedListener("characterClass", (System.Action)(() => OnPlayerClassChanged(player)));
        }

        private void OnPlayerClassChanged(IServerPlayer player)
        {
            player.RegisterClassAchievements();
        }

        private void OnPlayerBreakBlock(IServerPlayer player, BlockSelection blockSelection, ref float dropQuantityMultiplier, ref EnumHandling handling)
        {
            if (blockSelection == null)
                return;

            var block = player.Entity.World.BlockAccessor.GetBlock(blockSelection.Position);
            if (block == null)
                return;

            if (AchievementsModState.Instance.IsPlayerTrackingEvent(player, new EventToTrack(TrackedEventEnum.BreakBlock, block.BlockMaterial.ToString())))
                AchievementsModState.Instance.RecordEvent(TrackedEventEnum.BreakBlock, player, block);
        }

        
        private void OnGameTick(float tick)
        {

        }
    }

    public class AchievementsModConfigFile
    {
        public static AchievementsModConfigFile Current { get; set; }

        public bool EnableEndOfRoundPopup { get; set; } = true;
        public string ClassMonitoredAchievementsData { get; set; } = InitMonitoredAchievements();

        private static string InitMonitoredAchievements()
        {
            var data = new Dictionary<string, List<EventToTrack>>();
            data["farmer"] = new List<EventToTrack>()
            {
                new EventToTrack(TrackedEventEnum.BreakBlock,"Gravel")
            };

            return Base64Encode(JsonConvert.SerializeObject(data));
        }

        internal Dictionary<string, List<EventToTrack>> GetClassMonitoredAchievements()
        {
            if (String.IsNullOrEmpty(ClassMonitoredAchievementsData))
                return new Dictionary<string, List<EventToTrack>>();

            return JsonConvert.DeserializeObject<Dictionary<string, List<EventToTrack>>>(Base64Decode(ClassMonitoredAchievementsData));

        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }

}
