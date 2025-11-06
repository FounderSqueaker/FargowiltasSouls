using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Desert
{
    public class AngryTumbler : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.Tumbleweed);

        public int BlastTimer = 100;
        public override bool SafePreAI(NPC npc)
        {
            BlastTimer--;
            if (BlastTimer <= 0)
            {
                if (BlastTimer == 0)
                {
                    FargoSoulsUtil.DustRing(npc.Center, 32, DustID.Dirt, 5f, default, 2f);
                    SoundEngine.PlaySound(SoundID.NPCHit54, npc.Center);
                }
                if (BlastTimer <= -30)
                {
                    BlastTimer = 300;
                    if (npc.HasPlayerTarget)
                    {
                        npc.velocity = new Vector2(15f * npc.direction, -5);
                        SoundEngine.PlaySound(SoundID.DoubleJump with { Pitch = 0.75f }, npc.Center);
                    }

                }
            }
            return base.SafePreAI(npc);
        }
        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);
            target.AddBuff(BuffID.Slow, 180);
        }
    }
}
