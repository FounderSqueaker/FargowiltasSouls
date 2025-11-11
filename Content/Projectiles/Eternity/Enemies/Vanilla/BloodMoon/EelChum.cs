using FargowiltasSouls.Content.Buffs.Eternity;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.BloodMoon
{
    public class EelChum : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.VanillaTextureProjectile(ProjectileID.ChumBucket);
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = Main.projFrames[ProjectileID.ChumBucket];
        }
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.ChumBucket);
            AIType = ProjectileID.ChumBucket;
            Projectile.friendly = false;
            Projectile.hostile = true;
            //Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.frame = Main.rand.Next(1, 4);
        }
        public override bool PreAI()
        {
            DelegateMethods.v3_1 = new Vector3(0.8f, 0f, 0);
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity, 10, DelegateMethods.CastLight);
            return base.PreAI();
        }
        public override bool CanHitPlayer(Player target) => Projectile.velocity != Vector2.Zero;
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<AnticoagulationBuff>(), 120);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> t = TextureAssets.Projectile[Type];
            int num156 = TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
            int frame = num156 * Projectile.frame;
            Rectangle rectangle = new(0, frame, t.Width(), num156);
            lightColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates());
            for (int j = 0; j < 12; j++)
            {
                Vector2 afterimageOffset = (MathHelper.TwoPi * j / 12f).ToRotationVector2() * 2;
                Color glowColor = Color.Red;
                Main.EntitySpriteDraw(t.Value, Projectile.Center + afterimageOffset - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), rectangle, glowColor, Projectile.rotation, rectangle.Size() / 2, Projectile.scale, SpriteEffects.None);
            }
            Main.EntitySpriteDraw(t.Value, Projectile.Center - Main.screenPosition, rectangle, lightColor, Projectile.rotation, rectangle.Size() / 2, 1, SpriteEffects.None);
            return false;
        }
    }
}
