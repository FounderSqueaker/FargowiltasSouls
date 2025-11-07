using System;
using System.IO;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Projectiles.Eternity.Bosses.EyeOfCthulhu;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Luminance.Assets;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static System.Net.Mime.MediaTypeNames;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.BloodMoon
{
    public class WanderingEyeFish : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.EyeballFlyingFish);

        public int TeleDashTimer;
        public int DashCount;
        public Vector2 Pos1;
        public Vector2 Pos2;
        public Vector2 Pos3;
        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);
            binaryWriter.Write(TeleDashTimer);
            binaryWriter.Write(DashCount);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);
            TeleDashTimer = binaryReader.ReadInt32();
            DashCount = binaryReader.ReadInt32();
        }
        public override bool SafePreAI(NPC npc)
        {
            if (npc.HasPlayerTarget && Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0))
                TeleDashTimer++;
            Player player = Main.player[npc.target];
            DelegateMethods.v3_1 = new Vector3(0.8f, 0f, 0);
            Utils.PlotTileLine(npc.Center, npc.Center + npc.velocity, 10, DelegateMethods.CastLight);

            if (TeleDashTimer == 360) //warning flashes
            {
                for (int i = 0; i < 300; i++) // 300 attempts to find 3 valid spots
                {
                    Vector2 pos = player.Center + Main.rand.NextVector2Circular(16 * 20, 16 * 15);
                    if (Collision.CanHitLine(npc.Center, 0, 0, pos, 0, 0) && pos.Distance(player.Center + player.velocity) > 16*5)
                    {
                        if (Pos1 == Vector2.Zero)
                            Pos1 = pos;
                        else if (Pos2 == Vector2.Zero && pos.Distance(Pos1) > 16 * 5)
                            Pos2 = pos;
                        else if (Pos3 == Vector2.Zero && pos.Distance(Pos2) > 16 * 5 && pos.Distance(Pos1) > 16 * 5)
                            Pos3 = pos;
                        else break;
                    }
                }
            }
            if (TeleDashTimer == 370) SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = 0.8f }, Pos1);
            if (TeleDashTimer == 380) SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = 0.8f }, Pos2);
            if (TeleDashTimer == 390) SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = 0.8f }, Pos3);

            if (TeleDashTimer >= 360)
            {
                if (Pos1 == Vector2.Zero || Pos2 == Vector2.Zero || Pos3 == Vector2.Zero) //find spots again bc something broke
                {
                    TeleDashTimer = 359;
                }
                else if (TeleDashTimer == 360)
                    SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = 0.8f }, npc.Center);
                if (TeleDashTimer < 390)
                {
                    npc.velocity = Vector2.Zero;
                    npc.rotation = FargoSoulsUtil.NPCRotateTowards(npc, Pos1, 10) * npc.direction;
                }
                if (TeleDashTimer >= 390)
                {
                    if (DashCount == 0 && Pos1 != Vector2.Zero)
                    {
                        npc.velocity = FargoSoulsUtil.SmartAccel(npc.Center, Pos1, npc.velocity, 1f, 2f);
                        if (npc.Center.Distance(Pos1) <= 16) SpawnScythes(npc);
                    }
                    if (DashCount == 1 && Pos2 != Vector2.Zero)
                    {
                        npc.velocity = FargoSoulsUtil.SmartAccel(npc.Center, Pos2, npc.velocity, 1f, 2f);
                        if (npc.Distance(Pos2) <= 16) SpawnScythes(npc);
                    }
                    if (DashCount == 2 && Pos3 != Vector2.Zero)
                    {
                        npc.velocity = FargoSoulsUtil.SmartAccel(npc.Center, Pos3, npc.velocity, 1f, 2f);
                        if (npc.Distance(Pos3) <= 16) SpawnScythes(npc);
                    }
                    npc.rotation = npc.velocity.ToRotation() * npc.direction;
                    for (int i = 0; i < 2; ++i)
                    {
                        int d = Dust.NewDust(npc.position, npc.width, npc.height, DustID.FireworksRGB, npc.velocity.X, npc.velocity.Y, 100, newColor: Color.DarkRed, Scale: 1.2f);
                        Main.dust[d].noGravity = true;
                        Main.dust[d].velocity /= 4f;
                        Main.dust[d].velocity -= npc.velocity;
                    }
                }
                if (TeleDashTimer >= 1200 || DashCount == 3) // reset everything
                {
                    TeleDashTimer = npc.life <= npc.lifeMax / 2 ? 180 : 60;
                    DashCount = 0;
                    Pos1 = Pos2 = Pos3 = Vector2.Zero;
                }
                return false;
            }
            else return base.SafePreAI(npc);
        }
        public void SpawnScythes(NPC npc)
        {
            if (FargoSoulsUtil.HostCheck)
            {
                float rng = MathHelper.ToRadians(Main.rand.NextFloat(0, 360));
                for (int i = 0; i < 5; i++)
                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, new Vector2(1f, 1f).RotatedBy(2 * Math.PI / 5 * i + rng), ModContent.ProjectileType<BloodScythe>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0, Main.myPlayer, ai2: 1);
            }
            DashCount += 1;
        }
        public override void AI(NPC npc)
        {
            base.AI(npc);
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);
            target.AddBuff(ModContent.BuffType<AnticoagulationBuff>(), 600);
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D shine = MiscTexturesRegistry.ShineFlareTexture.Value;
            if (TeleDashTimer >= 360 && TeleDashTimer <= 375)
            {
                Vector2 position = npc.Center + npc.rotation.ToRotationVector2() * npc.spriteDirection * npc.width * 0.4f;
                float flarescale = Math.Abs(TeleDashTimer - 375);
                for (int i = 0; i < 1; i++)
                {
                    Main.spriteBatch.Draw(shine, position - Main.screenPosition, null, Color.Lerp(Color.IndianRed, Color.Red, 0.2f) with { A = 0 }, Main.GlobalTimeWrappedHourly * 2f, shine.Size() * 0.5f, flarescale / 60 + 0.05f, 0, 0f);
                    Main.spriteBatch.Draw(shine, position - Main.screenPosition, null, Color.AliceBlue with { A = 0 }, Main.GlobalTimeWrappedHourly * -2f, shine.Size() * 0.5f, flarescale / 90 + 0.05f, 0, 0f);
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
            }
            if (TeleDashTimer >= 370 && TeleDashTimer <= 400 && Pos1 != Vector2.Zero)
            {
                for (int i = 0; i < 2; i++)
                {
                    Main.spriteBatch.Draw(shine, Pos1 - Main.screenPosition, null, Color.DarkRed with { A = 0 }, Main.GlobalTimeWrappedHourly * 2f, shine.Size() * 0.5f, (float)Math.Abs(TeleDashTimer - 400) / 60 + 0.05f, 0, 0f);
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
            }
            if (TeleDashTimer >= 380 && TeleDashTimer <= 410 && Pos2 != Vector2.Zero)
            {
                for (int i = 0; i < 2; i++)
                {
                    Main.spriteBatch.Draw(shine, Pos2 - Main.screenPosition, null, Color.DarkRed with { A = 0 }, Main.GlobalTimeWrappedHourly * 2f, shine.Size() * 0.5f, (float)Math.Abs(TeleDashTimer - 410) / 60 + 0.05f, 0, 0f);
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
            }
            if (TeleDashTimer >= 390 && TeleDashTimer <= 420 && Pos3 != Vector2.Zero)
            {
                for (int i = 0; i < 2; i++)
                {
                    Main.spriteBatch.Draw(shine, Pos3 - Main.screenPosition, null, Color.DarkRed with { A = 0 }, Main.GlobalTimeWrappedHourly * 2f, shine.Size() * 0.5f, (float)Math.Abs(TeleDashTimer - 420) / 60 + 0.05f, 0, 0f);                
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
            }
            base.PostDraw(npc, spriteBatch, screenPos, drawColor);
        }
    }
}
