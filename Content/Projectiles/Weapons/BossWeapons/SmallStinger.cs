using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Buffs.Eternity;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Weapons.BossWeapons
{
    public class SmallStinger : ModProjectile, IPixelatedPrimitiveRenderer
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Weapons/BossWeapons", Name);
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Small Stinger");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.HornetStinger);
            AIType = ProjectileID.Bullet;
            Projectile.penetrate = -1;
            Projectile.minion = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 120;
            Projectile.width = 10;
            Projectile.height = 18;
            //Projectile.scale *= 1.5f;
            Projectile.height = 28;
            Projectile.width = 14;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public static float TrailFadeTime => 20;
        public override void AI()
        {
            //stuck in enemy
            if (Projectile.ai[0] == 1)
            {
                Projectile.damage = 0;
                if (Projectile.ai[2] < TrailFadeTime) // trail timer
                    Projectile.ai[2]++;
                Projectile.aiStyle = -1;

                Projectile.ignoreWater = true;
                Projectile.tileCollide = false;

                int secondsStuck = 15;
                bool kill = false;

                Projectile.localAI[0] += 1f;

                int npcIndex = (int)Projectile.ai[1];
                if (Projectile.localAI[0] >= 60 * secondsStuck)
                {
                    kill = true;
                }
                else if (npcIndex < 0 || npcIndex >= 200)
                {
                    kill = true;
                }
                else if (Main.npc[npcIndex].active && !Main.npc[npcIndex].dontTakeDamage)
                {
                    Projectile.Center = Main.npc[npcIndex].Center - Projectile.velocity * 2f;
                    Projectile.gfxOffY = Main.npc[npcIndex].gfxOffY;
                }
                else
                {
                    kill = true;
                }

                if (kill)
                {
                    Projectile.Kill();
                }
            }
            else
            {
                Projectile.position += Projectile.velocity * 0.5f;

                //dust from stinger
                if (Main.rand.NextBool())
                {
                    int num92 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CorruptGibs, 0f, 0f, 0, default, 0.9f);
                    Main.dust[num92].noGravity = true;
                    Main.dust[num92].velocity *= 0.5f;
                }
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            //modifiers.DisableCrit();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];

                if (p.active && p.type == Projectile.type && p != Projectile && Projectile.Hitbox.Intersects(p.Hitbox))
                {
                    //target.SimpleStrikeNPC(Projectile.damage / 2, 0, noPlayerInteraction: true);
                    // target.StrikeNPC(damage / 2, 0, 0, true); //normal damage but looks like a crit ech
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), p.Center, Vector2.Zero, ModContent.ProjectileType<SmallStingHitbox>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner);
                    target.AddBuff(ModContent.BuffType<InfestedBuff>(), 300);
                    p.Kill();
                    SoundEngine.PlaySound(SoundID.Item27 with { Pitch = -0.4f }, Projectile.Center);
                    break;
                }
            }

            Projectile.ai[0] = 1;
            Projectile.ai[1] = target.whoAmI;
            Projectile.velocity = (Main.npc[target.whoAmI].Center - Projectile.Center) * 1f; //distance it sticks out
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 300;
            Projectile.netUpdate = true;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                //int num92 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CorruptGibs, Projectile.velocity.X, Projectile.velocity.Y, 0, default, 0.9f);
                //Main.dust[num92].noGravity = true;
                //Main.dust[num92].velocity *= 0.25f;
                //Main.dust[num92].fadeIn = 1.3f;

                Particle p = new RectangleParticle(Projectile.Center, Projectile.velocity * 0.7f + Main.rand.NextVector2Circular(15, 45), Color.Yellow, Main.rand.NextFloat(0.1f, 0.25f), 50, true);
                p.Spawn();
            }
            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            Color color25 = Lighting.GetColor((int)(Projectile.position.X + Projectile.width * 0.5) / 16, (int)((Projectile.position.Y + Projectile.height * 0.5) / 16.0));
            Texture2D texture2D3 = TextureAssets.Projectile[Projectile.type].Value;
            int num156 = TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
            int y3 = num156 * Projectile.frame;
            Rectangle rectangle = new(0, y3, texture2D3.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;
            int num157 = 7;
            int num159 = 0;
            float num160 = 0f;
            Color yellow = Projectile.GetAlpha(Color.Orange);

            Color color26 = lightColor;
            color26 = Projectile.GetAlpha(color26);

            int num161 = num159;

            /*for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
            {
                Vector2 value4 = Projectile.oldPos[i];
                float num165 = Projectile.oldRot[i];
                if (i < 4)
                {
                    Color color27 = color26;
                    color27 *= (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                    Main.EntitySpriteDraw(texture2D3, value4 + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color27 * 0.4f, num165, origin2, Projectile.scale, SpriteEffects.None, 0);
                }
            }*/

            Texture2D glow = FargoAssets.GetTexture2D("Content/Projectiles/Weapons/BossWeapons", "SmallStingerGlow").Value;

            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition + new Vector2(0.5f, Projectile.gfxOffY - 0.1f), new Microsoft.Xna.Framework.Rectangle?(rectangle), Color.Orange, Projectile.rotation, origin2, Projectile.scale * 1.1f * Main.cursorScale, spriteEffects, 0);
            Main.EntitySpriteDraw(texture2D3, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color26, Projectile.rotation, origin2, Projectile.scale, spriteEffects, 0);
            
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            //smaller tile hitbox
            width = 8;
            height = 8;
            return true;
        }

        public float WidthFunction(float completionRatio)
        {
            float baseWidth = Projectile.width * 0.8f;
            return MathHelper.SmoothStep(baseWidth, 3.5f, completionRatio);
        }

        public Color ColorFunction(float completionRatio)
        {
            return Color.Lerp(Color.DarkOrange, Color.Orange, completionRatio);
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            ManagedShader shader = ShaderManager.GetShader("FargowiltasSouls.StingerTrail");
            FargoAssets.ColorNoiseMap.Value.SetTexture1();
            float progress = 1 - Projectile.ai[2] / TrailFadeTime;
            int num = (int)Math.Round(Projectile.oldPos.Length * progress);
            Vector2[] trail = Projectile.oldPos.Take(num).ToArray();
            PrimitiveRenderer.RenderTrail(trail, new(WidthFunction, ColorFunction, _ => Projectile.Size * 0.5f, Pixelate: true, Shader: shader), 25);
        }
    }
}
