using Foundation.Extensions;
using projectrarahat.src.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace projectrarahat.src
{
    /// <summary>
    /// Controls end of round events
    /// </summary>
    public class CharClassMod : ModSystem
    {
        private ICoreServerAPI api;

        public override void StartPre(ICoreAPI api)
        {
            CharClassModConfigFile.Current = api.LoadOrCreateConfig<CharClassModConfigFile>($"{typeof(CharClassMod).Name}.json");
            api.World.Config.SetBool("loadGearNonDress", CharClassModConfigFile.Current.LoadGearNonDress);
            base.StartPre(api);
        }

        public override bool ShouldLoad(EnumAppSide side)
        {
            return true;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            this.api = api;
            // Check every 8 seconds
            this.api.World.RegisterGameTickListener(OnGameTick, 8000);
            this.api.Event.PlayerNowPlaying += new PlayerDelegate(this.OnPlayerNowPlaying);
            this.api.Event.SaveGameLoaded += new Vintagestory.API.Common.Action(this.OnSaveGameLoaded);
        }

        private void OnPlayerNowPlaying(IServerPlayer player)
        {
            TryGiveClassItems(player);
        }

        private void TryGiveClassItems(IServerPlayer player)
        {
            if (CharClassModState.Instance.GetCharacterClasses() == null || CharClassModState.Instance.GetCharacterClasses().Count < 1)
                return;

            if (player.IsGrantedInitialItems())
                return;

            player.Entity.WatchedAttributes.RegisterModifiedListener("characterClass", (System.Action)(() => player.GrantInitialItems()));
        }

        private void OnSaveGameLoaded()
        {
            CharClassModState.Instance.SetCharacterClasses(this.api.Assets.Get("config/characterclasses.json").ToObject<List<CharacterClass>>());
            LogInfo($"Found {CharClassModState.Instance.GetCharacterClasses().Count()} json character classes");
            LogInfo("CharClassMod started");
        }

        private void LogInfo(string message)
        {
            System.Diagnostics.Debug.WriteLine("[Server:CharClassMod] " + message);
        }

        private void OnGameTick(float tick)
        {

        }
    }

    public class CharClassModConfigFile
    {
        public static CharClassModConfigFile Current { get; set; }
        public bool LoadGearNonDress { get; set; } = true;
    }

}
