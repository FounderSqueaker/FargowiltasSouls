using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.OOA
{
    public class DD2LightningBug : EModeNPCBehaviour
    {

        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.DD2LightningBugT3);

        public int State = 0;
        public int Target = -1;
        public int Timer = 0;

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);
            npc.knockBackResist = 0f;
            npc.lifeMax /= 2;
        }

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);
            binaryWriter.Write(State);
            binaryWriter.Write(Target);
            binaryWriter.Write(Timer);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);
            State = binaryReader.ReadInt32();
            Target = binaryReader.ReadInt32();
            Timer = binaryReader.ReadInt32();
        }

        public override bool SafePreAI(NPC npc)
        {
            Timer++;
            if (Timer < 60)
            {
                return base.SafePreAI(npc);
            }

            npc.spriteDirection = npc.direction;
            Projectile sentry;
            switch (State)
            {
                case 0: // Looking for target
                    Target = EModeDD2Event.FindClosestDD2Sentry(npc.Center);
                    if (Target != -1)
                    {
                        State = 1;
                    }
                    else
                        return base.SafePreAI(npc);
                    return false;
                case 1: // Approaching Target
                    sentry = Main.projectile[Target];
                    if (!sentry.active || sentry.Eternity().Jammed)
                    {
                        State = 0;
                        Target = -1;
                        return base.SafePreAI(npc);
                    }
                    Vector2 targetPos = sentry.Center - 60 * Vector2.UnitY;
                    npc.velocity += 0.05f * Vector2.UnitX.RotatedBy((targetPos - npc.Center).ToRotation());
                    npc.direction = (int)npc.HorizontalDirectionTo(targetPos);
                    float dist = npc.Distance(targetPos);
                    if (dist < 40f)
                    {
                        SoundEngine.PlaySound(SoundID.Item94, npc.Center);
                        npc.velocity *= 0f;
                        State = 2;
                        return false;
                    }
                    break;
                case 2: // Jamming Target
                    sentry = Main.projectile[Target];
                    if (!sentry.active)
                    {
                        State = 0;
                        Target = -1;
                        return base.SafePreAI(npc);
                    }
                    npc.Center = sentry.Center - 60 * Vector2.UnitY;
                    sentry.Eternity().Jammed = true;
                    // particles!!
                    if (Main.rand.NextBool(4))
                    {
                        Vector2 tailPos = npc.Bottom + new Vector2(npc.direction * -20,0);
                        float rot = Main.rand.NextFloat((sentry.TopLeft - tailPos).ToRotation(), (sentry.BottomRight - tailPos).ToRotation());
                        new ElectricSpark(tailPos, 3 * Vector2.UnitX.RotatedBy(rot), Color.Pink, 0.5f, 25).Spawn();
                    }
                    return false;
            }

            float speedCap = 2.5f;
            if (npc.velocity.Length() > speedCap)
            {
                npc.velocity.Normalize();
                npc.velocity *= speedCap;
            }

            return false;
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);

            //EModeGlobalNPC.Aura(npc, 400, ModContent.BuffType<LightningRodBuff>(), false, DustID.Vortex);
        }

        public override void OnKill(NPC npc)
        {
            if (Target != -1 && Main.projectile[Target].active)
                Main.projectile[Target].Eternity().Jammed = false;
            base.OnKill(npc);
        }

        public override void SafeModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (projectile.Eternity().isADD2Proj)
            {
                modifiers.FinalDamage *= 0.1f;
            }
            base.SafeModifyHitByProjectile(npc, projectile, ref modifiers);
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            target.AddBuff(BuffID.Electrified, 300);
            target.FargoSouls().AddBuffNoStack(BuffID.Webbed, 60);
        }
    }
}
