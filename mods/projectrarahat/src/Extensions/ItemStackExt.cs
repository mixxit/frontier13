using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace projectrarahat.src.Extensions
{
    public static class ItemStackExt
    {
        public static bool IsClothing(this ItemStack itemStack)
        {
            if (itemStack == null)
                throw new InvalidOperationException("missing item");

            EnumCharacterDressType result;
            if (itemStack.ItemAttributes != null && Enum.TryParse<EnumCharacterDressType>(itemStack.ItemAttributes["clothescategory"].AsString(), true, out result))
                return true;

            return false;
        }
    }
}
