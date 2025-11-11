using FargowiltasSouls.Core.Systems;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;

namespace FargowiltasSouls.Core.ItemDropRules.Conditions
{
    public class SwarmDropCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info) => !info.IsInSimulation;

        public bool CanShowItemDropInUI() => true;

        public string GetConditionDescription() => Language.GetTextValue("Mods.FargowiltasSouls.Conditions.Swarm");
    }
}
