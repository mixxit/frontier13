using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace projectrarahat.src.Extensions
{
    public static class IServerPlayerExt
    {
        public static bool IsGrantedInitialItems(this IServerPlayer player)
        {
            return CharClassModState.Instance.IsPlayerGrantedInitialItems(player.PlayerUID);
        }

        public static void GrantInitialItems(this IServerPlayer player)
        {
            if (player.GetSelectedClassCode() == null)
                return;

            var charClass = CharClassModState.Instance.GetCharacterClassByCode(player.GetSelectedClassCode());
            if (charClass == null)
                return;

            var gear = new List<JsonItemStack>();
            // Filter out clothing
            foreach(var jsonItemStack in charClass.Gear)
            {
                if (!jsonItemStack.Resolve(player.Entity.World, "character class gear", false))
                    continue;

                ItemStack itemstack = jsonItemStack.ResolvedItemstack?.Clone();
                if (itemstack == null)
                    continue;

                if (itemstack.IsClothing())
                    continue;

                gear.Add(jsonItemStack);
            }

            player.DeliverItems(gear);
            CharClassModState.Instance.SetPlayerGrantedInitialItems(player.PlayerUID);
        }

        public static void DeliverItems(this IServerPlayer player, List<JsonItemStack> gear)
        {
            // Worn items already get delivered by the SurvivalMod, we are merely giving the rest
            foreach (JsonItemStack jsonItemStack in gear)
                player.DeliverItem(jsonItemStack);
        }

        public static void DeliverItem(this IServerPlayer player, JsonItemStack jsonItemStack)
        {
            // Skip unknown item
            if (!jsonItemStack.Resolve(player.Entity.World, "character class gear", false))
                return;

            ItemStack itemstack = jsonItemStack.ResolvedItemstack?.Clone();
            if (itemstack == null)
                return;

            player.Entity.TryGiveItemStack(itemstack);
        }

        public static string GetSelectedClassCode(this IServerPlayer player)
        {
            string classCode = player.Entity.WatchedAttributes.GetString("characterClass", (string)null);
            if (String.IsNullOrEmpty(classCode))
                return null;

            return classCode;
        }

    }
}
