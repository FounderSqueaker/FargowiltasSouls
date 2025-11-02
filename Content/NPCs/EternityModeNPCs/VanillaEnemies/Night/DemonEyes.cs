using System;
using System.IO;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Projectiles.Eternity.Bosses.EyeOfCthulhu;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using Luminance.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Night
{
    public class DemonEyes : EModeNPCBehaviour
    {
        public override NPCMatcher CreateMatcher() =>
            new NPCMatcher().MatchTypeRange(
                NPCID.DemonEye,
                NPCID.DemonEye2,
                NPCID.DemonEyeOwl,
                NPCID.DemonEyeSpaceship,
                NPCID.CataractEye,
                NPCID.CataractEye2,
                NPCID.SleepyEye,
                NPCID.SleepyEye2,
                NPCID.DialatedEye,
                NPCID.DialatedEye2,
                NPCID.GreenEye,
                NPCID.GreenEye2,
                NPCID.PurpleEye,
                NPCID.PurpleEye2
            );

        public int AttackTimer;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write7BitEncodedInt(AttackTimer);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            AttackTimer = binaryReader.Read7BitEncodedInt();
        }

        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);
        }

        public override void AI(NPC npc)
        {
            base.AI(npc);
            if (npc.HasPlayerTarget)
                AttackTimer++;
            if (AttackTimer == 360) //warning flash
            {
                if (!Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0))
                {
                    AttackTimer = Main.rand.Next(0, 120);
                }
                else
                {
                    SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = 0.8f, MaxInstances = 1 }, npc.Center);
                }
                npc.netUpdate = true;
                NetSync(npc);
            }
            if (AttackTimer >= 420)
            {
                npc.TargetClosest();

                Vector2 velocity = Vector2.Normalize(Main.player[npc.target].Center - npc.Center) * 10;
                npc.velocity = velocity;

                AttackTimer = Main.rand.Next(-300, 0);
                npc.netUpdate = true;
                NetSync(npc);
            }

            if (Math.Abs(npc.velocity.Y) > 5 || Math.Abs(npc.velocity.X) > 5)
            {
                int dustId = Dust.NewDust(npc.position, npc.width, npc.height, DustID.SomethingRed, npc.velocity.X * 0.2f,
                    npc.velocity.Y * 0.2f, 100, default, 1f);
                Main.dust[dustId].noGravity = true;
                Main.dust[dustId].noLight = true;
                Main.dust[dustId].velocity *= 0.1f;
            }
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(ModContent.BuffType<BerserkedBuff>(), 120);
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (AttackTimer >= 360 && AttackTimer <= 375)
            {
                Vector2 position = npc.Center + npc.rotation.ToRotationVector2() * npc.spriteDirection * npc.width * 0.4f;
                Texture2D shine = MiscTexturesRegistry.ShineFlareTexture.Value;
                float flarescale = Math.Abs(AttackTimer - 375);
                for (int i = 0; i < 1; i++)
                {
                    Main.spriteBatch.Draw(shine, position - Main.screenPosition, null, Color.Lerp(Color.IndianRed, Color.Red, 0.2f) with { A = 0 }, Main.GlobalTimeWrappedHourly * 2f, shine.Size() * 0.5f, flarescale/60 + 0.05f, 0, 0f);
                    Main.spriteBatch.Draw(shine, position - Main.screenPosition, null, Color.AliceBlue with { A = 0 }, Main.GlobalTimeWrappedHourly * -2f, shine.Size() * 0.5f, flarescale/90 + 0.05f, 0, 0f);
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
            }
            base.PostDraw(npc, spriteBatch, screenPos, drawColor);
        }
    }

    public class WanderingEye : DemonEyes
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.WanderingEye);

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);
        }

        public override void OnFirstTick(NPC npc) { }

        public override void AI(NPC npc)
        {
            if (npc.life < npc.lifeMax / 2)
            {
                npc.knockBackResist = 0f;
                if (++AttackTimer > 40)
                {
                    AttackTimer = 0;
                    if (FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Normalize(npc.velocity), ModContent.ProjectileType<BloodScythe>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer, ai2: 1);
                }
            }
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            //target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 120);
        }
    }

    public class ServantofCthulhu : DemonEyes
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.ServantofCthulhu);

        public override void OnFirstTick(NPC npc) { }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            npc.lifeMax *= 2;
        }

        public override void AI(NPC npc)
        {
            npc.position += npc.velocity;

            Lighting.AddLight(npc.Center, 0.75f / 3, 1.35f / 3, 1.5f / 3);
        }

        public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(npc, target, hurtInfo);

            //target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 120);
        }

        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, npc.type);
        }
    }
}
