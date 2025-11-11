using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Night
{
    public class Werewolf : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.Werewolf);

        public int JumpTimer = 140;
        public bool Jumped;
        public override bool SafePreAI(NPC npc)
        {
            //EModeGlobalNPC.Aura(npc, 200, ModContent.BuffType<BerserkedBuff>(), false, 60);
            JumpTimer--;
            if (JumpTimer <= 0)
            {
                if (JumpTimer == 0)
                {
                    FargoSoulsUtil.DustRing(npc.Center, 32, DustID.ViciousPowder, 5f, default, 2f);
                    SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/NPC_Hit_6") with { Pitch = -0.5f }, npc.Center);
                }
                npc.velocity *= 0.9f;
                if (JumpTimer <= -60)
                {
                    JumpTimer = 60 * 9;
                    Jumped = true;
                    if (npc.HasPlayerTarget && Collision.CanHitLine(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1))
                    {
                        Vector2 targetPoint = Main.player[npc.target].Center - Vector2.UnitY * 200;
                        float distanceScale = MathHelper.Clamp(npc.Distance(targetPoint) / 1000f, 0f, 1f);
                        float vel = 5f + 15f * distanceScale;
                        npc.velocity = npc.DirectionTo(targetPoint) * vel;
                        SoundEngine.PlaySound(FargosSoundRegistry.ThrowShort with { Pitch = 0.5f }, npc.Center);
                    }  
                }
            }
            else
            {
                if (JumpTimer < 10 && !(npc.HasPlayerTarget && Collision.CanHitLine(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1)))
                    JumpTimer++;
            }

            if (Jumped)
            {
                if (npc.velocity.Y == 0)
                    Jumped = false;
                int d = Dust.NewDust(npc.position, npc.width, npc.height, DustID.ViciousPowder, Alpha: 100, Scale: 1.4f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity /= 4f;
                Main.dust[d].velocity -= npc.velocity;
                npc.knockBackResist = 0;
            }
            else npc.knockBackResist = npc.FargoSouls().defKnockBackResist;
            return base.SafePreAI(npc);
        }

        public override void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit)
        {
            base.OnHitNPC(npc, target, hit);

            if (target.townNPC && (hit.InstantKill || target.life < hit.Damage))
            {
                target.Transform(npc.type);
                //SoundEngine.PlaySound(SoundID.);
            }
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(BuffID.Rabies, 1800);
        }
    }
}
