using FargowiltasSouls.Content.Projectiles.Deathrays;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Patreon.Duck
{
    public class RailgunBlast : MutantSpecialDeathray
    {
        public RailgunBlast() : base(20, 1.25f) { }
        public override void SetDefaults()
        {
            base.SetDefaults();

            CooldownSlot = ImmunityCooldownID.General;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.hide = true;
            Projectile.penetrate = -1;

            Projectile.FargoSouls().TimeFreezeImmune = true;

            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 12;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.Distance(FargoSoulsUtil.ClosestPointInHitbox(targetHitbox, Projectile.Center)) < TipOffset.Length())
                return true;

            return base.Colliding(projHitbox, targetHitbox);
        }

        Vector2 TipOffset => 18f * Projectile.scale * Projectile.velocity;

        public override void AI()
        {
            base.AI();
            Projectile.frameCounter += 60;

            Player player = Main.player[Projectile.owner];

            if (!Main.dedServ && Main.LocalPlayer.active)
                FargoSoulsUtil.ScreenshakeRumble(6);

            Projectile.velocity = Projectile.velocity.SafeNormalize(-Vector2.UnitY);

            float num801 = 10f;
            Projectile.scale = (float)Math.Cos(Projectile.localAI[0] * MathHelper.PiOver2 / maxTime) * num801;
            if (Projectile.scale > num801)
                Projectile.scale = num801;

            if (player.active && !player.dead && !player.ghost)
            {
                Projectile.Center = player.Center + 50f * Projectile.velocity + TipOffset + Main.rand.NextVector2Circular(5, 5);
            }
            else
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.localAI[0] == 0f)
            {
                if (!Main.dedServ)
                {
                    SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/Weapons/Railgun") with { Volume = 0.5f }, Projectile.Center);
                    SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/Thunder") with { Volume = 0.5f }, player.Center + Projectile.velocity * Math.Min(Main.screenWidth / 2, 900f));
                }

                Vector2 dustPos = player.Center + Projectile.velocity * 50f;

                for (int i = 0; i < 40; i++)
                {
                    int dust = Dust.NewDust(dustPos - new Vector2(16, 16), 32, 32, DustID.Smoke, 0f, 0f, 100, default, 2f);
                    Main.dust[dust].velocity -= Projectile.velocity * 2;
                    Main.dust[dust].velocity *= 3f;
                    Main.dust[dust].velocity += player.velocity / 2;
                }

                for (int i = 0; i < 30; i++)
                {
                    int dust = Dust.NewDust(dustPos - new Vector2(16, 16), 32, 32, DustID.FireworkFountain_Yellow, 0f, 0f, 100, default, 2f);
                    Main.dust[dust].velocity -= Projectile.velocity * 2;
                    Main.dust[dust].velocity *= 5f;
                    Main.dust[dust].velocity *= Main.rand.NextFloat(1f, 2f);
                    Main.dust[dust].velocity += player.velocity / 2;
                }

                for (int j = 0; j < 10; j++)
                {
                    int gore = Gore.NewGore(Projectile.GetSource_FromThis(), dustPos, -Projectile.velocity, Main.rand.Next(61, 64), 1f);
                    Main.gore[gore].velocity -= Projectile.velocity;
                    Main.gore[gore].velocity.Y += 2f;
                    Main.gore[gore].velocity *= 4f;
                    Main.gore[gore].velocity += player.velocity / 2;
                }
            }

            if (++Projectile.localAI[0] >= maxTime)
            {
                Projectile.Kill();
                return;
            }
            //Projectile.scale = num801;
            //float num804 = Projectile.velocity.ToRotation();
            //num804 += Projectile.ai[0];
            //Projectile.rotation = num804 - 1.57079637f;
            //float num804 = Main.npc[(int)Projectile.ai[1]].ai[3] - 1.57079637f;
            //if (Projectile.ai[0] != 0f) num804 -= (float)Math.PI;
            //Projectile.rotation = num804;
            //num804 += 1.57079637f;
            //Projectile.velocity = num804.ToRotationVector2();
            float amount = 0.5f;
            Projectile.localAI[1] = MathHelper.Lerp(Projectile.localAI[1], 3000f, amount);
            //DelegateMethods.v3_1 = new Vector3(0.3f, 0.65f, 0.7f);
            //Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * Projectile.localAI[1], (float)Projectile.width * Projectile.scale, new Utils.PerLinePoint(DelegateMethods.CastLight));

            Projectile.position -= Projectile.velocity;
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            const int increment = 200;
            for (int i = 0; i < 3000; i += increment)
            {
                float offset = i + Main.rand.NextFloat(-increment, increment);
                if (offset < 0)
                    offset = 0;
                if (offset > 3000)
                    offset = 3000;
                float spawnRange = Projectile.scale * 16f;
                int d = Dust.NewDust(Projectile.position + Projectile.velocity * offset + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-spawnRange, spawnRange),
                    Projectile.width, Projectile.height, DustID.Teleporter, Scale: Main.rand.NextFloat(2f, 3f));
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity += Projectile.velocity * 20f;
                //Main.dust[d].velocity *= Main.rand.NextFloat(12f, 24f) / 10f * Projectile.scale;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
          //target.immune[Projectile.owner] = 12;
            target.AddBuff(BuffID.Electrified, 600);

            if (Projectile.owner == Main.myPlayer && Main.player[Projectile.owner].ownedProjectileCounts[ModContent.ProjectileType<Content.Projectiles.LightningArc>()] < 60)
            {
                const int max = 3;
                int count = max;
                foreach (NPC n in Main.npc)
                {
                    if (n.CanBeChasedBy() && n.whoAmI != target.whoAmI && target.Distance(n.Center) < 1500 && Collision.CanHitLine(target.Center, 0, 0, n.Center, 0, 0))
                    {
                        if (--count < 0)
                            break;
                        Vector2 vel = Main.rand.NextFloat(10f, 20f) * target.SafeDirectionTo(n.Center);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, vel, ModContent.ProjectileType<Content.Projectiles.LightningArc>(),
                            Projectile.damage / 3, Projectile.knockBack / 10, Projectile.owner, vel.ToRotation(), Main.rand.Next(80));
                    }
                }
                int spray = (max + count) / 2;
                for (int i = -spray; i <= spray; i++)
                {
                    Vector2 vel = Main.rand.NextFloat(10f, 20f) * Projectile.velocity.RotatedBy(MathHelper.ToRadians(30) / spray * (i + Main.rand.NextFloat(-0.5f, 0.5f)));
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, vel, ModContent.ProjectileType<Content.Projectiles.LightningArc>(),
                        Projectile.damage / 3, Projectile.knockBack / 10, Projectile.owner, vel.ToRotation(), Main.rand.Next(80));
                }
                Main.player[Projectile.owner].ownedProjectileCounts[ModContent.ProjectileType<Content.Projectiles.LightningArc>()] += max * 2;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            ArmorShaderData shader = GameShaders.Armor.GetShaderFromItemId(ItemID.BrightYellowDye);
            shader.Apply(Projectile, new Terraria.DataStructures.DrawData?());

            bool retval = base.PreDraw(ref lightColor);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);

            return retval;
        }
    }
}
