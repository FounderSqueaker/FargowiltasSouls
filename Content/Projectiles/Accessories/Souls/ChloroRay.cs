using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Accessories.Souls
{
    public class ChloroRay : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.VanillaTextureProjectile(ProjectileID.CrystalLeafShot);
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.CrystalLeafShot);
            Projectile.DamageType = DamageClass.Generic;
            AIType = 0;
            Projectile.aiStyle = -1;
        }
        public override void AI()
        {
            Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + 3.14f;
            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = -1;
                //SoundEngine.PlaySound(SoundID., Projectile.Center);
                for (int num345 = 0; num345 < 8; num345++)
                {
                    int num346 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch);
                    Main.dust[num346].noGravity = true;
                    Dust dust2 = Main.dust[num346];
                    dust2.velocity *= 3f;
                    Main.dust[num346].scale = 1.5f;
                    dust2 = Main.dust[num346];
                    dust2.velocity += Projectile.velocity * Main.rand.NextFloat();
                }
            }
            float num347 = 1f - (float)Projectile.timeLeft / 180f;
            float num348 = ((num347 * -6f * 0.85f + 0.33f) % 1f + 1f) % 1f;
            Color value3 = Color.White;
            if (Projectile.frameCounter++ >= 1)
            {
                Projectile.frameCounter = 0; 
                ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.ChlorophyteLeafCrystalShot, new ParticleOrchestraSettings
                {
                    PositionInWorld = Projectile.Center,
                    MovementVector = Projectile.velocity,
                    UniqueInfoPiece = (byte)(Main.rgbToHsl(value3).X * 255f)
                });
            }
            Lighting.AddLight(Projectile.Center, new Vector3(0.05f, 0.2f, 0.1f) * 1.5f);
            if (Main.rand.Next(5) == 0)
            {
                Dust dust12 = Dust.NewDustDirect(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.WhiteTorch);
                dust12.noGravity = true;
                Dust dust2 = dust12;
                dust2.velocity *= 0.1f;
                dust12.scale = 1.5f;
                dust2 = dust12;
                dust2.velocity += Projectile.velocity * Main.rand.NextFloat();
                dust12.color = value3;
                dust12.color.A /= 4;
                dust12.alpha = 100;
                dust12.noLight = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int num506 = 0; num506 < 15; num506++)
            {
                int num507 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WhiteTorch);
                Main.dust[num507].noGravity = true;
                Dust dust2 = Main.dust[num507];
                dust2.velocity += Projectile.oldVelocity * Main.rand.NextFloat();
                Main.dust[num507].scale = 1.5f;
                dust2.color = Color.White;
                dust2.color.A /= 4;
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            int r;
            int g;
            int b;
            r = (g = (b = 255));
            float num9 = (float)(int)Main.mouseTextColor / 100f - 1.6f;
            r = (int)((float)r * num9);
            g = (int)((float)g * num9);
            b = (int)((float)b * num9);
            int a4 = (int)(100f * num9);
            r += 50;
            if (r > 255)
            {
                r = 255;
            }
            g += 50;
            if (g > 255)
            {
                g = 255;
            }
            b += 50;
            if (b > 255)
            {
                b = 255;
            }
            return new Color(r, g, b, a4);
        }
    }
}
