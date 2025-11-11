using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Microsoft.Build.Tasks;
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
    public class SnowFlinx : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.SnowFlinx);

        public int AttackTimer;
        public int State;
        
        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);
            binaryWriter.Write(AttackTimer);
            binaryWriter.Write(State);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);
            AttackTimer = binaryReader.ReadInt32();
            State = binaryReader.ReadInt32();

        }

        public override bool SafePreAI(NPC npc)
        {
            AttackTimer++;
            if (AttackTimer >= 240 && (State == 1 || (npc.HasPlayerTarget && Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0))))
            {
                if (State == 0)
                {
                    npc.direction = (int)npc.HorizontalDirectionTo(Main.player[npc.target].Center);
                    AttackTimer = 240;
                }
                State = 1;

                if (Math.Abs(npc.velocity.X) < 15)
                    npc.velocity.X = npc.direction * (float) Math.Pow(1.1, (AttackTimer - 300));
                else if (AttackTimer % 2 == 0)
                {
                    Dust d = Dust.NewDustDirect(npc.Bottom, 0, 0, DustID.Snow);
                    d.velocity *= 0.5f;
                }

                // rotation cap
                float rot = (AttackTimer - 240) / 50f;
                if (rot > 0.6f)
                    rot = 0.6f;
                npc.rotation += npc.direction * rot;

                // check if it is ABOUT to hit a wall
                if (Collision.SolidCollision(npc.position + npc.velocity, npc.width, npc.height))
                {
                    SoundEngine.PlaySound(SoundID.Item14, npc.Center);
                    SoundEngine.PlaySound(SoundID.Item51, npc.Center);
                    FargoSoulsUtil.ScreenshakeRumble(2f);
                    npc.velocity = 10 * Vector2.UnitX.RotatedBy(-(MathHelper.PiOver2 + (npc.direction * MathHelper.PiOver4)));
                    npc.rotation = npc.velocity.ToRotation() + MathHelper.PiOver2;
                    State = 0;
                    AttackTimer = 0;
                }
                return false;
            }

            return base.SafePreAI(npc);
        }

        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            // Immune to kb and less damage taken during roll
            if (State == 1)
            {
                modifiers.Knockback *= 0;
                modifiers.FinalDamage *= 0.5f;
            }
            base.ModifyIncomingHit(npc, ref modifiers);
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            float xSpeed = Math.Abs(npc.velocity.X);
            // draw snowball second so it's in front
            if (State == 1)
            {
                Texture2D texture = TextureAssets.Npc[npc.type].Value;
                Rectangle frame = npc.frame;
                SpriteEffects flip = npc.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                Main.EntitySpriteDraw(texture, npc.Center - screenPos, frame, drawColor, npc.rotation, new Vector2(frame.Width / 2, frame.Height / 2), npc.scale, flip);

                // draw snowball second so it's in front
                Texture2D sTexture = ModContent.Request<Texture2D>("Terraria/Images/Projectile_109", AssetRequestMode.ImmediateLoad).Value;
                Rectangle sFrame = new(0, 0, sTexture.Width, sTexture.Height);
                
                Main.EntitySpriteDraw(sTexture, npc.Center - screenPos, sFrame, drawColor, npc.rotation, new Vector2(sFrame.Width / 2, sFrame.Height / 2), 3.5f * npc.scale * ((xSpeed)/15f), flip);
                return false;
            }
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);
        }
    }
}
