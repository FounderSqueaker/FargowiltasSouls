using FargowiltasSouls.Assets.Textures;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Bosses.Betsy
{
    public class BetsySpawnPortal : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/NPCs", "TavernkeepPortal");

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 9;
        }
        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
            Projectile.width = 60;
            Projectile.height = 100;
            Projectile.hide = true;

            Projectile.scale = 0f;
            Projectile.light = 1f;
        }

        int timer = 0;

        public override void AI()
        {
            ref float target = ref Projectile.ai[1];
            ref float type = ref Projectile.ai[0];

            if (type == 0 || (type == NPCID.DD2WyvernT3 && !Main.player[(int)target].Alive()))
            {
                Projectile.Kill();
                return;
            }

            if (timer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item104, Projectile.Center);
                FargoSoulsUtil.DustRing(Projectile.Center, 20, DustID.Shadowflame, 5f);
            }

            if (timer <= 60)
            {
                Projectile.scale = MathHelper.SmoothStep(0f, 1f, timer / 60f);
                Projectile.Opacity = LumUtils.Saturate(MathHelper.Lerp(0f, 1f, timer / 30f));
            }
            else
            {
                Projectile.scale = MathHelper.SmoothStep(1f, 0f, (timer - 60f) / 90f);
                Projectile.Opacity = LumUtils.Saturate(MathHelper.Lerp(1f, 0f, (timer - 60f) / 90f));
            }


            if (++timer == 60f)
            {
                switch (type)
                {
                    case NPCID.DD2WyvernT3:
                        SoundEngine.PlaySound(SoundID.DD2_WyvernScream, Projectile.Center);
                        if (FargoSoulsUtil.HostCheck)
                            Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center + Vector2.UnitY * 30, Vector2.Zero,
                                ModContent.ProjectileType<BetsyWyvernClone>(), Projectile.damage, 1f, ai0: target);
                        break;
                    case NPCID.DD2KoboldFlyerT3:
                        SoundEngine.PlaySound(SoundID.DD2_KoboldFlyerChargeScream, Projectile.Center);
                        if (FargoSoulsUtil.HostCheck)
                            Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center + Vector2.UnitY * 30, Vector2.Zero,
                                ModContent.ProjectileType<BetsyKoboldClone>(), Projectile.damage, 1f, ai0: target);
                        break;
                    case NPCID.DD2LightningBugT3:
                        SoundEngine.PlaySound(SoundID.DD2_LightningBugHurt, Projectile.Center);
                        if (FargoSoulsUtil.HostCheck)
                            Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center + Vector2.UnitY * 30, Vector2.Zero,
                                ModContent.ProjectileType<BetsyLightningBugClone>(), Projectile.damage, 1f, ai0: target, ai1: Projectile.ai[2]);
                        break;
                    case NPCID.DD2DarkMageT3:
                        SoundEngine.PlaySound(SoundID.DD2_DarkMageHurt, Projectile.Center);
                        if (FargoSoulsUtil.HostCheck)
                            Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center + Vector2.UnitY * 30, Vector2.Zero,
                                ModContent.ProjectileType<BetsyDarkMageClone>(), Projectile.damage, 1f, ai0: target, ai1: Projectile.ai[2]);
                        break;
                    default:
                        Projectile.Kill();
                        return;
                }
            }

            if (timer >= 60 + 90)
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.frameCounter++ > 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame > 8)
                    Projectile.frame = 0;
            }
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
            behindProjectiles.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            FargoSoulsUtil.GenericProjectileDraw(Projectile, Color.Pink);
            return false;
        }
    }
}
