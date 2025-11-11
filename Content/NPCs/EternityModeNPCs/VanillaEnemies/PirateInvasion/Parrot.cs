using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using FargowiltasSouls.Core.Systems;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.PirateInvasion
{
    public class Parrot : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.Parrot);

        public override void AI(NPC npc)
        {
            base.AI(npc);
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(ModContent.BuffType<SqueakyToyBuff>(), 120);
            target.AddBuff(ModContent.BuffType<MidasBuff>(), 600);

            LocalizedText DeathText = Language.GetText("Mods.FargowiltasSouls.DeathMessage.Parrots");
            if (WorldSavingSystem.MasochistModeReal && Main.getGoodWorld && npc.type == NPCID.Parrot && !target.Male)
            {
                target.KillMe(PlayerDeathReason.ByCustomReason(DeathText.ToNetworkText(target.name)), 999999, 0);
            }
        }

        public override bool CheckDead(NPC npc)
        {
            if (!Main.hardMode)
            {
                npc.active = false;
                if (npc.DeathSound != null)
                    SoundEngine.PlaySound(npc.DeathSound.Value, npc.Center);
                return false;
            }

            return base.CheckDead(npc);
        }
    }
}
