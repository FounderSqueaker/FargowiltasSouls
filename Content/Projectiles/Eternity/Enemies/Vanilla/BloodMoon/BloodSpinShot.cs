using System;
using System.Linq;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.BloodMoon;
using FargowiltasSouls.Content.Projectiles.Eternity.Buffs;
using Microsoft.Build.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.BloodMoon
{
    public class BloodSpinShot : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.VanillaTextureProjectile(ProjectileID.BatOfLight);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = ProjectileID.Sets.TrailCacheLength[ProjectileID.BatOfLight];
            ProjectileID.Sets.TrailingMode[Type] = ProjectileID.Sets.TrailingMode[ProjectileID.BatOfLight];
            Main.projFrames[Type] = Main.projFrames[ProjectileID.BatOfLight];
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
        }

        ref float OrderNumber => ref Projectile.ai[1];
        ref float State => ref Projectile.ai[2];
        public override void AI()
        {
            if (Projectile.frameCounter++ > 4)
            {
                Projectile.frameCounter = 0;
                if (Projectile.frame++ > 2)
                    Projectile.frame = 0;
            }

            Projectile.spriteDirection = Projectile.velocity.X < 0 ? -1 : 1;
            NPC owner = Main.npc[(int)Projectile.ai[0]];
            if (owner == null || !owner.active || owner.ai[0] != 1)
            {
                Projectile.Kill();
                return;
            }

            Dreadnautilus.BloodNautilus_GetMouthPositionAndRotation(owner, out var mouthpos, out var mouthdir);
            Player target = Main.player[owner.target];
            if (State == -1)
            {
                float hoverdist = 80 * (5 - OrderNumber);

                //semicircle in front of shell
                Vector2 hoverpos = (owner.Center - mouthdir.ToRotationVector2() * 50f) + 120 * Vector2.UnitX.RotatedBy(mouthdir).RotatedBy(Math.PI / 6 * (OrderNumber) + MathHelper.PiOver2);
                //following behind it
                Vector2 hoverpos2 = mouthpos + hoverdist * Vector2.UnitX.RotatedBy(mouthdir - MathHelper.TwoPi);

                float accel = 2f;
                float decel = 3f;
                float resistance = Projectile.velocity.Length() * accel / (35f);
                if (Projectile.Distance(hoverpos2) > 16) 
                    Projectile.velocity = FargoSoulsUtil.SmartAccel(Projectile.Center, owner.ai[1] <= 90 ? hoverpos : hoverpos2, Projectile.velocity, accel - resistance, decel + resistance);

                if (owner.ai[1] > 119 && (owner.ai[1] - 120) / OrderNumber == 30)
                {
                    Projectile.velocity = -(Projectile.DirectionTo(target.Center) * 5f);
                    State = 0;
                    Projectile.timeLeft = 120;
                }
            }
            else if (State > -1)
            {
                if (State++ >= 15)
                {
                    if (State <= 60)
                        Projectile.velocity += Projectile.DirectionTo(target.Center);
                    if (Projectile.Distance(target.Center) < 16 * 6)
                        Projectile.velocity += Projectile.DirectionTo(target.Center) * 0.5f;
                }
            }
            else if (State == -2)
            {
                if (State-- <= -15)
                {
                    if (State >= -60)
                        Projectile.velocity += Projectile.DirectionTo(target.Center);
                    if (Projectile.Distance(target.Center) < 16 * 6)
                        Projectile.velocity += Projectile.DirectionTo(target.Center) * 0.5f;
                }
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (Main.rand.NextBool(5))
                target.AddBuff(BuffID.Rabies, 1200); //yknow cuz bat,, idk u probably have the upgrade by now
            target.AddBuff(ModContent.BuffType<AnticoagulationBuff>(), 600);
        }

        public override void OnKill(int timeLeft)
        {
            if (!Main.dedServ)
                SoundEngine.PlaySound(SoundID.NPCDeath4 with { Volume = 0.8f }, Projectile.Center);
            FargoSoulsUtil.DustRing(Projectile.Center, 5, DustID.Firework_Red, 4);
        }

        public override Color? GetAlpha(Color lightColor) => Color.Lerp(lightColor, Color.White, 0.75f);
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2 = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawpos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            int sizeY = texture2.Height / Main.projFrames[Type];
            drawpos.Y += sizeY * 2; //temporary offset
            int frameY = sizeY * Projectile.frame;
            Rectangle rectangle = new(0, frameY, texture2.Width, sizeY);
            SpriteEffects dir = Projectile.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            //red outline
            for (int i = 0; i < 4; i++)
            {
                Vector2 spinningpoint14 = Projectile.rotation.ToRotationVector2();
                Main.EntitySpriteDraw(texture2, drawpos + spinningpoint14.RotatedBy(MathHelper.PiOver2 * i) * 2f, rectangle, Color.Crimson with {A = 120}, Projectile.rotation, texture2.Size() / 2, Projectile.scale, dir);
            }

            //red trail
            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Type]; i++)
            {
                Main.spriteBatch.UseBlendState(BlendState.Additive);
                Vector2 oldPos = Projectile.oldPos[i] + Projectile.Size / 2;
                float oldRot = Projectile.oldRot[i];
                Color color = Color.Crimson;
                color *= (ProjectileID.Sets.TrailCacheLength[Type] - i - 2.6f) / (ProjectileID.Sets.TrailCacheLength[Type] * 0.8f); //slightly more visible than vanilla
                float scale = MathHelper.Lerp(Projectile.scale, 2.6f, (float)i / 15);
                Main.spriteBatch.Draw(texture2, oldPos - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), rectangle, color, oldRot, rectangle.Size() / 2f, scale, dir, 0);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            //white bat
            FargoSoulsUtil.GenericProjectileDraw(Projectile, lightColor);
            return false;
        }
    }
}