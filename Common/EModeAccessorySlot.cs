using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Items.Accessories.Forces;
using FargowiltasSouls.Content.Items.Accessories.Souls;
using FargowiltasSouls.Content.Patreon.ParadoxWolf;
using FargowiltasSouls.Core.Systems;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace FargowiltasSouls.Common
{
    public class EModeAccessorySlot : ModAccessorySlot
    {

        public override bool HasEquipmentLoadoutSupport => true;      

        

        public override bool CanAcceptItem(Item checkItem, AccessorySlotType context)
        {
            bool allowedType = FargoSoulsSets.Items.AllowedSoulItemExceptions[checkItem.type];
            if ((context == AccessorySlotType.FunctionalSlot || context == AccessorySlotType.VanitySlot) && (base.CanAcceptItem(checkItem, context) || allowedType))
            {
                if ((checkItem.ModItem != null && (checkItem.ModItem is BaseEnchant || checkItem.ModItem is BaseForce || checkItem.ModItem is BaseSoul)) || allowedType)
                {
                    return true;
                }
                return false;
            }
            return base.CanAcceptItem(checkItem, context);
        }
        
        public override bool IsVisibleWhenNotEnabled() => false;
        public override bool IsEnabled()
        {
            return WorldSavingSystem.EternityMode && Player.FargoSouls().MutantsPactSlot;
        }
        public override string FunctionalTexture => FargoAssets.GetAssetString("UI", "EnchantSlotIcon");
        //public override string FunctionalBackgroundTexture => "FargowiltasSouls/Assets/UI/EnchantSlotBackground";
        //public override string VanityBackgroundTexture => "FargowiltasSouls/Assets/UI/EnchantSlotBackground";
        //public override string DyeBackgroundTexture => "FargowiltasSouls/Assets/UI/EnchantSlotBackground";
        public override void OnMouseHover(AccessorySlotType context)
        {
            switch (context)
            {
                case AccessorySlotType.FunctionalSlot:
                    Main.hoverItemName = Language.GetTextValue("Mods.FargowiltasSouls.Common.AccessorySlot.EModeSlotFunctional");
                    break;
                case AccessorySlotType.VanitySlot:
                    Main.hoverItemName = Language.GetTextValue("Mods.FargowiltasSouls.Common.AccessorySlot.EModeSlotVanity");
                    break;
            }
        }
    }
}
