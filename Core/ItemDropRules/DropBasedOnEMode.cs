using Fargowiltas.Content.NPCs;
using FargowiltasSouls.Core.ItemDropRules.Conditions;
using FargowiltasSouls.Core.Systems;
using System.Collections.Generic;
using Terraria.GameContent.ItemDropRules;

namespace FargowiltasSouls.Core.ItemDropRules
{
    public class DropBasedOnEMode : IItemDropRule, INestedItemDropRule
    {
        protected readonly IItemDropRule RuleForEMode;
        protected readonly IItemDropRule RuleForDefault;
        protected readonly IItemDropRule RuleForSwarm;

        public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }

        public DropBasedOnEMode(IItemDropRule ruleForEMode, IItemDropRule ruleForDefault, IItemDropRule ruleForSwarm = null)
        {
            RuleForEMode = ruleForEMode;
            RuleForDefault = ruleForDefault;
            RuleForSwarm = ruleForSwarm;
            ChainedRules = [];
        }

        public bool CanDrop(DropAttemptInfo info)
        {
            bool swarm = info.npc.TryGetGlobalNPC<EnergizedGlobalNPC>(out var energizedNPC) && energizedNPC.SwarmActive(info.npc);
            return swarm ? RuleForSwarm.CanDrop(info) : (WorldSavingSystem.EternityMode ? RuleForEMode.CanDrop(info) : RuleForDefault.CanDrop(info));
        }

        public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
        {
            DropRateInfoChainFeed ratesInfo1 = ratesInfo.With(1f);
            ratesInfo1.AddCondition(new EModeDropCondition());
            RuleForEMode.ReportDroprates(drops, ratesInfo1);

            DropRateInfoChainFeed ratesInfo2 = ratesInfo.With(1f);
            ratesInfo2.AddCondition(new NotEModeDropCondition());
            RuleForDefault.ReportDroprates(drops, ratesInfo2);

            DropRateInfoChainFeed ratesInfo3 = ratesInfo.With(1f);
            ratesInfo3.AddCondition(new SwarmDropCondition());
            RuleForDefault.ReportDroprates(drops, ratesInfo3);

            Chains.ReportDroprates(ChainedRules, 1f, drops, ratesInfo);
        }

        public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info) => new()
        {
            State = ItemDropAttemptResultState.DidNotRunCode
        };

        public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info, ItemDropRuleResolveAction resolveAction)
        {
            bool swarm = info.npc.TryGetGlobalNPC<EnergizedGlobalNPC>(out var energizedNPC) && energizedNPC.SwarmActive(info.npc);
            return swarm ? resolveAction(RuleForSwarm, info) : (WorldSavingSystem.EternityMode ? resolveAction(RuleForEMode, info) : resolveAction(RuleForDefault, info));
        }
    }
}
