using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Common.Utilities;
using FargowiltasSouls.Core.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.OOA
{
    public class JavelinSpin : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_662";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.scale = 1.2f;
            Projectile.penetrate = -1;
        }

        public ref float owner => ref Projectile.ai[0];

        public override void AI()
        {
            Projectile.ai[1]++;
            NPC npc = Main.npc[(int)owner];
            if (!npc.active || !(npc.type is NPCID.DD2JavelinstT1 or NPCID.DD2JavelinstT2 or NPCID.DD2JavelinstT3))
                Projectile.Kill();


            float distance = 3f * 16;

            if (Projectile.ai[1] > 80) // reflect
            {
                Main.projectile.Where(x => EModeGlobalProjectile.CanBeAbsorbed(x)).ToList().ForEach(x =>
                {
                    if (Vector2.Distance(x.Center, npc.Center) <= distance)
                    {
                        new SmallSparkle(x.Center, Vector2.UnitX, Color.White, 1f, 10).Spawn();
                        SoundEngine.PlaySound(SoundID.Item52 with { Pitch = -0.3f }, Projectile.Center);
                        SoundEngine.PlaySound(SoundID.Item13 with { Volume = 0.7f }, Projectile.Center);

                        if (FargoSoulsUtil.HostCheck)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                float vel = 9 + Main.rand.NextFloat(-0.5f, 0.5f);
                                float rot = Main.rand.NextFloat(-0.2f, 0.2f);
                                Projectile.NewProjectile(Projectile.InheritSource(Projectile), x.Center, (new Vector2(-x.direction, -1) * (vel * Vector2.One)).RotatedBy(rot), ModContent.ProjectileType<JavelinSpark>(), Projectile.damage, 1f);
                            }
                        }

                        // kill
                        x.active = false;
                    }
                });
            }

            if (Projectile.ai[1] >= 90 && Projectile.ai[1] % 5 == 0)
                SoundEngine.PlaySound(SoundID.DD2_JavelinThrowersAttack with { Pitch = -0.5f, Volume = 0.5f }, Projectile.Center);
            else if (Projectile.ai[1] >= 30 && Projectile.ai[1] % 10 == 0)
                SoundEngine.PlaySound(SoundID.DD2_JavelinThrowersAttack with { Pitch = -0.25f, Volume = 0.5f }, Projectile.Center);

            Vector2 offset = new Vector2(npc.spriteDirection * 15, 3);
            Projectile.Center = npc.Center + offset;
            if (Projectile.ai[1] > 30)
            {

                Projectile.rotation += MathHelper.Lerp(0f, 1.03f, Projectile.ai[1]-60);
                if (Projectile.ai[1] >= 90)
                    new SparkParticle(Projectile.Center, 5 * Vector2.UnitX.RotatedBy(Main.rand.NextFloat(0, MathHelper.TwoPi)), Color.Brown, 0.3f, 7).Spawn();
            }
            else
            {
                Projectile.rotation = -MathHelper.PiOver4;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            base.OnHitPlayer(target, info);

            target.AddBuff(BuffID.Bleeding, 300);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D t = TextureAssets.Projectile[ProjectileID.DD2JavelinHostile].Value;
            Rectangle frame = t.Frame();
            int drawCount = 35;
            if (Projectile.ai[1] > 90)
            {
                for (int i = 0; i < drawCount; i++)
                {
                    Main.EntitySpriteDraw(t, Projectile.Center - Main.screenPosition, frame, lightColor * 0.2f, Projectile.rotation + (MathHelper.TwoPi * i / drawCount), frame.Size() / 2, Projectile.scale, SpriteEffects.None);
                }
                FargoSoulsUtil.GenericProjectileDraw(Projectile, lightColor * 0.2f);
            }
            else
            {
                FargoSoulsUtil.GenericProjectileDraw(Projectile, lightColor);
            }
            return false;
        }
    }

    public class JavelinSpark : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles", "Empty");

        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.width = 10;
            Projectile.height = 10;
            
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            Projectile.velocity.Y += 0.35f;
            Projectile.rotation = Projectile.velocity.ToRotation();

            new SparkParticle(Projectile.Center, Projectile.velocity, Color.Lerp(Color.Orange, Color.Red, 0.5f), 0.3f, 4).Spawn();
            Dust.NewDust(Projectile.Center, 1, 1, DustID.Torch, Scale: 0.2f);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            base.OnHitPlayer(target, info);

            target.AddBuff(BuffID.OnFire, 120);
        }
    }
}
