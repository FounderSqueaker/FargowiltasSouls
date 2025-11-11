using FargowiltasSouls.Core.Systems;
using FargowiltasSouls.Core;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Luminance.Core.Graphics;
using FargowiltasSouls.Assets.Textures;
using Terraria.Audio;
using Terraria.DataStructures;
//using System.Drawing;
using Terraria.GameContent;

namespace FargowiltasSouls.Content.Projectiles.Accessories.SupremeDeathbringerFairy
{
    public class BloodScytheFriendly : ModProjectile//, IPixelatedPrimitiveRenderer
    {
        public ref float randomize => ref Projectile.ai[1];
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Eternity/Bosses/EyeOfCthulhu", "BloodScytheVanilla1");
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 1;
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.Red * Projectile.Opacity;
        }
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.DemonScythe);
            AIType = ProjectileID.DemonScythe;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
            Projectile.FargoSouls().CanSplit = false;

            FargowiltasSouls.MutantMod.Call("LowRenderProj", Projectile);

            Projectile.Opacity = 0.4f;
            Projectile.aiStyle = 0;
        }

        public override void AI()
        {
            Projectile.rotation += Projectile.direction * 0.8f;
            Projectile.ai[0] += 1f;
            if (!(Projectile.ai[0] < 30f))
            {
                if (Projectile.ai[0] < 100f)
                {
                    Projectile.velocity *= 1.06f;
                }
                else
                {
                    Projectile.ai[0] = 200f;
                }
            }

            Vector2 offset = new Vector2(0, -20).RotatedBy(Projectile.rotation);
            offset = offset.RotatedByRandom(MathHelper.Pi / 6);
            int d = Dust.NewDust(Projectile.Center, 0, 0, DustID.BloodWater, 0f, 0f, 150);
            Main.dust[d].position += offset;
            float velrando = Main.rand.Next(20, 31) / 10;
            Main.dust[d].velocity = Projectile.velocity / velrando;
            Main.dust[d].noGravity = true;
            Main.dust[d].scale = 1.2f;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            if (randomize == 0)
            {
                randomize += Main.rand.Next(1, 4);
                Projectile.netUpdate = true;
            }
            Texture2D texture = FargoAssets.GetTexture2D("Content/Projectiles/Eternity/Bosses/EyeOfCthulhu", "BloodScytheVanilla" + randomize).Value;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.DarkRed, Projectile.rotation, texture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
