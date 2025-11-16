using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Core;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Bosses.Plantera
{
    public class DicerPlanteraSeed : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Eternity/Bosses/Plantera", Name + "1");
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.TrailCacheLength[Type] = 3;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.PoisonSeedPlantera);
            Projectile.aiStyle = -1;
            Projectile.alpha = 0;
            Projectile.width = Projectile.height = 24;
        }
        public int SpriteType = -1;

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(SpriteType);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            SpriteType = reader.ReadInt32();
        }
        public override void AI()
        {
            if (SpriteType == -1)
                SpriteType = Main.rand.Next(1, 6);

            bool recolor = SoulConfig.Instance.BossRecolors && WorldSavingSystem.EternityMode;
            if (recolor)
                Lighting.AddLight(Projectile.Center, 25f / 255, 47f / 255, 64f / 255);
            else
                Lighting.AddLight(Projectile.Center, 0.1f, 0.4f, 0.2f);

            if (Projectile.localAI[0] != 1)
            {
                Projectile.localAI[0] = 1;
            }
            if (Projectile.timeLeft < 4)
            {
                Projectile.Opacity -= 0.25f;
                Projectile.hostile = false;
            }
            if (++Projectile.frameCounter > Main.projFrames[Type])
            {
                if (++Projectile.frame >= Main.projFrames[Type])
                    Projectile.frame = 0;
                Projectile.frameCounter = 0;
            }
            Projectile.rotation += .13f;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<IvyVenomBuff>(), 240);
            target.AddBuff(BuffID.Poisoned, 300);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = SpriteType <= 0 ? TextureAssets.Projectile[Type].Value : FargoAssets.GetTexture2D("Content/Projectiles/Eternity/Bosses/Plantera", Name + SpriteType, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            FargoSoulsUtil.GenericProjectileDraw(Projectile, lightColor, texture);
            return false;
        }
    }
}
