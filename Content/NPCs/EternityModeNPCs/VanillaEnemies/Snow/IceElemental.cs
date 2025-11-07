using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.Snow;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Snow
{
    public class IceElemental : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.IceElemental);

        public int AttackTimer;
        Vector2 targetPos;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);
            binaryWriter.Write(AttackTimer);
            binaryWriter.WriteVector2(targetPos);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);
            AttackTimer = binaryReader.ReadInt32();
            targetPos = binaryReader.ReadVector2();
        }

        public override bool SafePreAI(NPC npc)
        {
            AttackTimer++;
            if (AttackTimer == 180)
            {
                if (npc.HasValidTarget && Collision.CanHitLine(npc.Center, 1, 1, Main.player[npc.target].Center, 1, 1))
                {
                    SoundEngine.PlaySound(SoundID.Item46, npc.Center);
                    targetPos = Main.player[npc.target].Center - npc.Center;

                    for (int i = 0; i < 12; i++)
                    {
                        float rot = (MathHelper.TwoPi / 10f) * i;
                        //float rot = Main.rand.NextFloat(0, MathHelper.TwoPi);
                        new SmallSparkle(npc.Center, 3 * Vector2.UnitX.RotatedBy(rot), Color.Blue, 1f, 25).Spawn();
                    }
                }
                else
                {
                    AttackTimer = 0;
                }
            }
            else if (AttackTimer > 180)
            {
                float speed = (float)Math.Pow((AttackTimer - 180) / 15f, 2) - 3;
                const float speedCap = 20f;

                if (speed > speedCap)
                    speed = speedCap;

                npc.velocity = speed * Vector2.UnitX.RotatedBy((targetPos).ToRotation());

                if (AttackTimer % 3 == 0)
                    Dust.NewDustPerfect(npc.position, DustID.Frost, Scale: 0.5f);


                if (Collision.SolidCollision(npc.position + npc.velocity, npc.width, npc.height))
                {
                    if (FargoSoulsUtil.HostCheck)
                    {
                        for (int i = 0; i < 6; i++)
                            Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, 4 * Vector2.UnitX.RotatedBy(i * (MathHelper.TwoPi / 6f)), ModContent.ProjectileType<IceShard>(), npc.damage / 6, 1f);
                    }

                    SoundEngine.PlaySound(SoundID.Item14, npc.Center);
                    SoundEngine.PlaySound(SoundID.Item109, npc.Center);
                    FargoSoulsUtil.ScreenshakeRumble(2f);
                    npc.velocity *= -0.3f;
                    AttackTimer = 0;

                    // lower ice shoot timer if about to shoot to prevent instantly shooting out of ram
                    if (npc.localAI[1] > 80 || npc.ai[3] > 0f)
                    {
                        npc.ai[3] = 0f;
                        npc.localAI[1] = 80;
                    }
                }
                return false;
            }
            return base.SafePreAI(npc);
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            base.ModifyIncomingHit(npc, ref modifiers);
            if (AttackTimer >= 180)
            {
                modifiers.DisableKnockback();
            }
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (AttackTimer > 180)
            {
                Texture2D bTexture = TextureAssets.Npc[npc.type].Value;
                Rectangle bFrame = npc.frame;
                SpriteEffects flip = npc.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                Main.EntitySpriteDraw(bTexture, npc.Center - screenPos, bFrame, drawColor, npc.rotation, new Vector2(bFrame.Width / 2, bFrame.Height / 2), npc.scale, flip);
                Vector3 color = Color.SkyBlue.ToVector3();
                Lighting.AddLight(npc.Center, color);

                float scale = 1f;
                if (AttackTimer < 200)
                    scale = (AttackTimer - 180) / 20f;

                Texture2D texture = ModContent.Request<Texture2D>("Terraria/Images/Projectile_464", AssetRequestMode.ImmediateLoad).Value;
                Rectangle frame = new(0, 0, texture.Width, texture.Height);
                Main.EntitySpriteDraw(texture, npc.Center - screenPos, frame, drawColor * 0.7f, npc.rotation, new Vector2(frame.Width / 2, frame.Height / 2), npc.scale * 0.8f * scale, flip);

                return false;
            }
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }
    }
}
