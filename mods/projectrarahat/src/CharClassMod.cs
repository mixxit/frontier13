using Foundation.Extensions;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

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
