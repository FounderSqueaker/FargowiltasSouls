using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.OOA;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.OOA
{
    public class DD2Javelinist : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(
            NPCID.DD2JavelinstT1,
            NPCID.DD2JavelinstT2,
            NPCID.DD2JavelinstT3
        );

        public int Timer = -60;
        public int Javelin = -1;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);
            binaryWriter.Write(Timer);
            binaryWriter.Write(Javelin);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);
            Timer = binaryReader.ReadInt32();
            Javelin = binaryReader.ReadInt32();
        }

        public override bool SafePreAI(NPC npc)
        {
            Timer++;
            if (Timer < 0)
            {
                return base.SafePreAI(npc);
            }
            if (Timer == 0)
                Timer = 100;

            if (npc.noGravity) // is phasing through something
                Timer--;

            if (Timer <= 180)
                npc.TargetClosest();

            if (npc.HasValidTarget && Timer <= 180)
            {
                if (npc.HasPlayerTarget)
                    npc.direction = (int)npc.HorizontalDirectionTo(Main.player[npc.target].Center);
                else
                    npc.direction = (int)npc.HorizontalDirectionTo(Main.npc[npc.target].Center);
                npc.spriteDirection = npc.direction;
            }

            if (Timer >= 180)
            {
                if (Javelin == -1)
                {
                    if (FargoSoulsUtil.HostCheck)
                        Javelin = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<JavelinSpin>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage, 0.7f), 3f, ai0: npc.whoAmI);
                    SoundEngine.PlaySound(SoundID.Item60, npc.Center);
                    npc.netUpdate = true;
                    NetSync(npc);
                }
                if (Timer > 390)
                {
                    Timer = 0;
                    if (Javelin != -1)
                    {
                        Main.projectile[Javelin].Kill();
                        Javelin = -1;
                    }
                    return false;
                }
                float jTimer = Math.Min(Main.projectile[Javelin].ai[1], 120);

                npc.velocity.X = npc.direction * MathHelper.Lerp(0, 3f, jTimer / 120);


                if (jTimer >= 100)
                    new SparkParticle(npc.Top - npc.spriteDirection * npc.width * Vector2.UnitX + Main.rand.NextFloat(0, npc.height) * Vector2.UnitY, -npc.velocity * 0.5f, Color.Green, 0.3f, 8).Spawn();

                return false;
            }

            return base.SafePreAI(npc);
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (Javelin != -1)
                npc.frame.Y = 988;
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }
    }
}
