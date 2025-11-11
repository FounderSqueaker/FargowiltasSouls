using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.Dungeon;
using FargowiltasSouls.Content.Projectiles.Eternity.Environment;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static FargowiltasSouls.Content.Projectiles.EffectVisual;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.BloodMoon
{
    public class EelChumBucket : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.VanillaTextureItem(ItemID.ChumBucket);
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Projectile.ai[0] = Main.rand.NextBool() ? 1 : -1;
        }

        public override void AI()
        {
            DelegateMethods.v3_1 = new Vector3(0.8f, 0f, 0);
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity, 10, DelegateMethods.CastLight);
            Projectile.velocity.Y += 0.5f;
            if (Projectile.velocity.Y > 8f)
                Projectile.velocity.Y = 8f;
            Projectile.ai[1]++;
            Projectile.rotation += (Projectile.velocity.Length() / 24f * (Projectile.velocity.X > 0 ? -1f : 1f)) * Projectile.ai[0];

            if (Projectile.ai[1] % 30 == 0 && Projectile.velocity.Y > 0 && FargoSoulsUtil.HostCheck)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, 5 * Vector2.UnitX.RotatedBy(Projectile.rotation - MathHelper.PiOver2), ModContent.ProjectileType<EelChum>(), Projectile.damage, 0, Main.myPlayer);
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<AnticoagulationBuff>(), 300);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCHit42, Projectile.Center);
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Iron);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> t = TextureAssets.Projectile[Type];
            lightColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates());
            for (int j = 0; j < 12; j++)
            {
                Vector2 afterimageOffset = (MathHelper.TwoPi * j / 12f).ToRotationVector2() * 3;
                Color glowColor = Color.Red;
                Main.EntitySpriteDraw(t.Value, Projectile.Center + afterimageOffset - Main.screenPosition, null, glowColor, Projectile.rotation, t.Size() / 2, Projectile.scale, SpriteEffects.None);
            }
            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
            {
                Color color27 = Projectile.GetAlpha(lightColor) * 0.75f;
                color27 *= (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                Main.EntitySpriteDraw(t.Value, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), null, color27, Projectile.oldRot[i], t.Size() / 2, Projectile.scale, SpriteEffects.None);
            }
            Main.EntitySpriteDraw(t.Value, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, t.Size() / 2, 1, SpriteEffects.None);
            return false;
        }
    }
}
