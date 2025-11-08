using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.UI.Elements;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Accessories.Souls
{
    public class ForbiddenTornado : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Accessories/Souls", Name);

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 0;

            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.timeLeft = 60 * 30;
            Projectile.FargoSouls().DeletionImmuneRank = 2;

            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            FargoSoulsPlayer modPlayer = player.FargoSouls();

            Projectile.damage = (int)(ForbiddenEffect.BaseDamage(player) * Projectile.scale);

            // Collision is done in FargoSoulsGlobalProjectile:PreAI

            if (Main.LocalPlayer.active && !Main.LocalPlayer.dead && !Main.LocalPlayer.ghost)
            {
                if (Projectile.Colliding(Projectile.Hitbox, Main.LocalPlayer.Hitbox))
                {
                    Main.LocalPlayer.wingTime = Main.LocalPlayer.wingTimeMax;
                }
            }

            float num1123 = 900f;
            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = -1;
                SoundEngine.PlaySound(SoundID.Item82, Projectile.Center);
            }
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] > 60 * 10 && Projectile.ai[2] >= 0)
            {
                Unleash();
            }
            if (Projectile.ai[2] > 0)
                Projectile.ai[2]--;
            if (Projectile.ai[2] < 0)
            {
                Projectile.localNPCHitCooldown = 10;
                Projectile.ai[2]--;
                Movement(player);
                if (Projectile.ai[2] < -60 * 5)
                    Projectile.Kill();
            }
            else
            {
                if (Projectile.scale < 2f)
                {
                    if (Main.myPlayer == Projectile.owner)
                        CooldownBarManager.Activate("ForbiddenTornadoCharge", FargoAssets.GetTexture2D("Content/Items/Accessories/Enchantments", "ForbiddenEnchant").Value, new(231, 178, 28),
                            () => (Projectile.scale - 1), activeFunction: () => Projectile != null && Projectile.active && Main.LocalPlayer.GetCooldown<ForbiddenEffect>() <= 0, displayAtFull: false);
                }
                Projectile.localNPCHitCooldown = 30;
                Projectile.velocity = Vector2.UnitY;
                Projectile.position -= Projectile.velocity;
            }
                
            if (Projectile.ai[0] >= num1123)
            {
                Projectile.Kill();
            }

            float num1125 = 15f * Projectile.scale;
            float num1126 = 15f * Projectile.scale;
            Point point8 = Projectile.Center.ToTileCoordinates();
            Collision.ExpandVertically(point8.X, point8.Y, out int num1127, out int num1128, (int)num1125, (int)num1126);
            num1127++;
            num1128--;
            Vector2 value72 = new Vector2(point8.X, num1127) * 16f + new Vector2(8f);
            Vector2 value73 = new Vector2(point8.X, num1128) * 16f + new Vector2(8f);
            Vector2 vector145 = Vector2.Lerp(value72, value73, 0.5f);
            Vector2 value74 = new(0f, value73.Y - value72.Y);
            value74.X = value74.Y * 0.2f;
            Projectile.position = Projectile.Center;
            Projectile.width = (int)(value74.X * 0.65f);
            Projectile.height = (int)value74.Y;
            Projectile.Center = Projectile.position;

            if (Projectile.ai[0] < num1123 - 120f)
            {
                for (int num1131 = 0; num1131 < 1; num1131++)
                {
                    float value75 = -0.5f;
                    float value76 = 0.9f;
                    float amount3 = Main.rand.NextFloat();
                    Vector2 value77 = new(MathHelper.Lerp(0.1f, 1f, Main.rand.NextFloat()), MathHelper.Lerp(value75, value76, amount3));
                    value77.X *= MathHelper.Lerp(2.2f, 0.6f, amount3);
                    value77.X *= -1f;
                    Vector2 value78 = new(6f, 10f);
                    Vector2 position3 = vector145 + value74 * value77 * 0.5f + value78;
                    Dust dust34 = Main.dust[Dust.NewDust(position3, 0, 0, DustID.Sandnado, 0f, 0f, 0, default, 1f)];
                    dust34.position = position3;
                    dust34.customData = vector145 + value78;
                    dust34.fadeIn = 1f;
                    dust34.scale = 0.3f;
                    if (value77.X > -1.2f)
                    {
                        dust34.velocity.X = 1f + Main.rand.NextFloat();
                    }
                    dust34.velocity.Y = Main.rand.NextFloat() * -0.5f - 1f;
                }
                return;
            }
        }
        public void Empower()
        {
            if (Projectile.ai[2] != 0) // cooldown
                return;
            Projectile.ai[2] = 30;
            if (Projectile.scale < 2f)
            {
                Projectile.position = Projectile.Center;
                Projectile.scale += 1f / 8;
                //Projectile.width = Projectile.height = (int)(10 * Projectile.scale);
                Projectile.Center = Projectile.position;
            }
            else
            {
                Unleash();
            }
            Projectile.netUpdate = true;
        }
        public void Unleash()
        {
            // unleash
            Projectile.ai[2] = -1;
            Projectile.ai[1] = -1;
            Projectile.netUpdate = true;
            Main.player[Projectile.owner].SetCooldown<ForbiddenEffect>(ForbiddenEffect.Cooldown(Main.player[Projectile.owner]));
        }
        public void Movement(Player player)
        {
            ref float Target = ref Projectile.ai[1];
            if (Target < 0) // has no target
            {
                NPC npc = Projectile.FindTargetWithinRange(1600, true);
                if (npc != null && npc.Alive())
                {
                    Target = npc.whoAmI;
                    Projectile.netUpdate = true;
                }
            }
            else // has target, seek it out
            {
                NPC npc = Main.npc[(int)Target];
                if (npc != null && npc.Alive())
                {
                    Vector2 idlePosition = npc.Center;
                    Vector2 toIdlePosition = idlePosition - Projectile.Center;
                    float distance = toIdlePosition.Length();
                    float speed = 28f;
                    float inertia = 30f;
                    toIdlePosition.Normalize();
                    toIdlePosition *= speed;
                    Projectile.velocity = (Projectile.velocity * (inertia - 1f) + toIdlePosition) / inertia;
                    if (Projectile.velocity == Vector2.Zero)
                    {
                        
                        Projectile.velocity.X = -0.15f;
                        Projectile.velocity.Y = -0.05f;
                    }
                }
                else
                    Target = -2;
            }

            
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float halfheight = 220;
            float density = 50f;
            for (float i = 0; i < (int)density; i++)
            {
                Color color = new(212, 192, 100);
                color.A /= 2;
                float lerpamount = Math.Abs(density / 2 - i) > density / 2 * 0.6f ? Math.Abs(density / 2 - i) / (density / 2) : 0f; //if too low or too high up, start making it transparent
                color = Color.Lerp(color, Color.Transparent, lerpamount);
                Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
                Vector2 offset = Vector2.SmoothStep(Projectile.Center + Vector2.UnitY * halfheight, Projectile.Center - Vector2.UnitY * halfheight, i / density);
                float scale = MathHelper.Lerp(Projectile.scale * 0.8f, Projectile.scale * 2.5f, i / density);
                Main.EntitySpriteDraw(texture, offset - Main.screenPosition,
                    new Rectangle(0, 0, texture.Width, texture.Height),
                    Projectile.GetAlpha(color),
                    i / 6f - Main.GlobalTimeWrappedHourly * 5f + Projectile.rotation,
                    texture.Size() / 2,
                    scale,
                    SpriteEffects.None,
                    0);
            }
            return false;
        }
    }
}