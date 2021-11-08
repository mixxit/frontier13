using Foundation.Extensions;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace VSModLauncher
{
    /// <summary>
    /// Controls end of round events
    /// </summary>
    public class EndOfRoundEventMod : ModSystem
    {
        private ICoreServerAPI api;

        private DateTime startTime { get; set; }
        private DateTime endOfRoundEventTime { get; set; }
        private bool eventTriggered { get; set; } = false;

        public override void StartPre(ICoreAPI api)
        {
            EndOfRoundEventModModConfigFile.Current = api.LoadOrCreateConfig<EndOfRoundEventModModConfigFile>($"{typeof(EndOfRoundEventMod).Name}.json");
            api.World.Config.SetInt("roundTimeSeconds", EndOfRoundEventModModConfigFile.Current.RoundTimeSeconds);
            base.StartPre(api);
        }
        
        public override bool ShouldLoad(EnumAppSide side)
        {
            return true;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            this.api = api;
            startTime = DateTime.Now;
            endOfRoundEventTime = startTime.AddSeconds(EndOfRoundEventModModConfigFile.Current.RoundTimeSeconds);

            // Check every minute
            this.api.World.RegisterGameTickListener(OnGameTick, 8000);
        }

        private void OnGameTick(float tick)
        {
            if (GetMinutesTillEndOfRoundEventTime() < 1)
                TriggerEvent();
        }

        private void TriggerEvent()
        {
            this.eventTriggered = true;
            this.api.BroadcastMessageToAllGroups("You feel at unease. Something is about to happen that will change everything.", EnumChatType.Notification);
        }

        private int GetMinutesTillEndOfRoundEventTime()
        {
            return (int)(endOfRoundEventTime-startTime).TotalMinutes;
        }
    }

    public class EndOfRoundEventModModConfigFile
    {
        public static EndOfRoundEventModModConfigFile Current { get; set; }

        public int RoundTimeSeconds { get; set; } = 14400;
    }

}
