using Fargowiltas.Content.Projectiles;
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

namespace FargowiltasSouls.Content.Projectiles.Eternity.Environment
{
    public class FallingIcicle : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Eternity/Environment", "FallingIcicle");
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 32;
            Projectile.hostile = true;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 600;
            Projectile.scale = 1f;
            Projectile.netImportant = true;
            Projectile.coldDamage = true;

            //Projectile.hide = true;
        }

        private bool firstTick = true;
        private readonly bool CanBeDangerSensed = !Main.gamePaused && Main.instance.IsActive && Main.netMode != NetmodeID.Server && Main.LocalPlayer.dangerSense;

        public const int telegraphTime = 40;
        public override void AI()
        {
            if (firstTick)
            {
                firstTick = false;
                Projectile.scale = 0.1f;
            }

            if (CanBeDangerSensed && Main.rand.NextBool(30))
            {
                // Dangersense
                int danger = Dust.NewDust(Projectile.position, 16, 32, DustID.RedTorch, 0f, 0f, 100, default, 0.3f);
                Main.dust[danger].fadeIn = 1f;
                Main.dust[danger].velocity *= 0.1f;
                Main.dust[danger].noLight = true;
                Main.dust[danger].noGravity = true;
            }

            Projectile.timeLeft = 2;
            Entity? target = null;
            if (Main.netMode == NetmodeID.SinglePlayer)
                target = Main.LocalPlayer;
            else
            {
                foreach (Player p in Main.player.Where(p => p.active && !p.dead))
                {
                    if (target == null)
                    {
                        target = p;
                    }
                    else if (target != null && target.Center.Distance(Projectile.Center) > p.Center.Distance(Projectile.Center))
                    {
                        target = p;
                    }
                }
            }

            bool hasTop = Main.tile[new Vector2(Projectile.position.X, Projectile.position.Y - 8f).ToTileCoordinates16()].HasUnactuatedTile ? true : false; // TODO: Make this better later so that it makes icicle fall instead

            if (target == null || Projectile.Center.Distance(target.Center) > 500 || (Projectile.ai[0] == 0 && !hasTop))
            {
                if (!hasTop)
                    Projectile.ai[0]++;
                Projectile.Kill();
            }

            //size increases over time until max size
            if (Projectile.scale < 1f)
            {
                Projectile.scale += 0.01f;

                if (Projectile.scale == 1f)
                {
                    //do a twinkle or sound or something tm
                    FargoSoulsUtil.DustRing(Projectile.Center, 5, DustID.SnowflakeIce, 1);
                }
                Projectile.netUpdate = true;
                return;
            }

            foreach (NPC nPC in Main.ActiveNPCs)
            {
                if (target != null && target.Center.Distance(Projectile.Center) > nPC.Center.Distance(Projectile.Center))
                {
                    target = nPC;
                }
            }

            foreach (Projectile projectile in Main.projectile.Where(p => p.active && p.friendly && p.type != Type && p.damage > 0 && !FargoSoulsUtil.IsSummonDamage(p, false, false) && p.Hitbox.Intersects(Projectile.Hitbox) && Projectile.ai[0] == 0 && ProjectileLoader.CanDamage(p) != false && ProjectileLoader.CanCutTiles(p) != false))
            {
                Projectile.ai[0]++;
            }
            //waits for target to walk below
            if (Projectile.ai[0] == 0 && target?.Center.Y > Projectile.Center.Y && (Math.Abs(target.Center.X - Projectile.Center.X) < 10))
            {
                bool lineCol = false;
                Point p = Projectile.Bottom.ToTileCoordinates();
                int maxTiles = (int)(600f / 16);
                maxTiles = int.Clamp(maxTiles, 0, (int)((target.Top.Y - Projectile.Bottom.Y) / 16f));
                for (int i = 0; i < maxTiles; i++)
                {
                    if (Main.tile[p.X, p.Y].HasUnactuatedTile)
                    {
                        lineCol = true;
                        break;
                    }
                    p.Y += 1;
                }
                if (!lineCol)
                {
                    // telegraph fall
                    Projectile.ai[0] = 1;
                    Projectile.netUpdate = true;
                    FargoSoulsUtil.DustRing(Projectile.Bottom, 12, DustID.SnowflakeIce, 3);
                }
            }

            if (Projectile.ai[0] > 0 && Projectile.ai[0] < telegraphTime)
            {
                Projectile.ai[0]++;
                Projectile.rotation = MathHelper.PiOver2 * 0.08f * MathF.Sin(MathF.Tau * Projectile.ai[0] / 7f);

                // start falling
                if (Projectile.ai[0] == telegraphTime)
                {
                    Projectile.velocity.Y = 1.5f;
                    SoundEngine.PlaySound(SoundID.Item50, Projectile.Bottom);
                }
            }
            // falling
            if (Projectile.ai[0] >= telegraphTime)
            {
                Projectile.rotation = 0;
                Projectile.velocity.Y = Projectile.velocity.Y * 1.15f;
                Projectile.velocity.Y = Math.Clamp(Projectile.velocity.Y, 1, 16);
                Projectile.netUpdate = true;
                return;
            }
            Projectile.hide = false;

        }

        public override bool CanHitPlayer(Player target) => !target.FargoSouls().PureHeart;

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Frostburn, 120);
            Projectile.Kill();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 120);
            Projectile.Kill();
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.ai[0] > 0)
            {
                SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);

                FargoSoulsUtil.DustRing(Projectile.Center, 5, DustID.SnowflakeIce, 1);
            }
            
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            if (Projectile.hide)
                behindNPCsAndTiles.Add(index);
        }
    }
}
