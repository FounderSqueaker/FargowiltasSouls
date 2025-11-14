using FargowiltasSouls.Assets.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.OOA
{
    public class SnotBaseball : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_676";

        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.Opacity = 1;
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.extraUpdates = 1;
        }

        int npc;

        public override void OnSpawn(IEntitySource source)
        {
            if (source is EntitySource_Parent parent && parent.Entity is NPC n && (n.type == NPCID.DD2OgreT2 || n.type == NPCID.DD2OgreT3))
            {
                npc = n.whoAmI;
            }

            base.OnSpawn(source);
        }

        private int GetGoreType()
        {
            return Main.rand.NextFromList(GoreID.OgreSpit1, GoreID.OgreSpit2, GoreID.OgreSpit3);
        }


        public override void AI()
        {
            for (int i = 0; i < 2; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.JungleGrass);
                d.noGravity = true;
            }

            Projectile.rotation += 0.3f;
            Projectile.velocity.Y += 0.15f;
            Projectile.ai[0]++;
            if (Projectile.ai[1] == 0)
            {
                NPC ogre = Main.npc[npc];
                if (Projectile.ai[0] >= 10 && ogre.active && Projectile.Center.Y > ogre.Center.Y)
                {
                    Projectile.ai[0] = 0;
                    Projectile.ai[1] = 1;
                    Player player = Main.player[(int)Projectile.ai[2]];
                    Vector2 posDiff = player.Center - Projectile.Center;
                    Vector2 vel = 20 * Vector2.UnitX.RotatedBy(posDiff.ToRotation()) - posDiff.Length() / 500f * Vector2.UnitY;
                    Projectile.velocity = vel;
                    FargoSoulsUtil.DustRing(Projectile.Center, 20, DustID.Torch, 4f, scale: 2);
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Variants = [2], Pitch = 1f }, Projectile.Center);
                }
                    
            }
            else
            {
                if (Projectile.ai[0] >= 10)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath19, Projectile.Center);
                    float spread = 0.3f;
                    if (FargoSoulsUtil.HostCheck)
                        for (int i = 0; i < 7; i++)
                            Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center, 0.4f * Projectile.velocity.RotatedBy(Main.rand.NextFloat(-spread, spread)), ModContent.ProjectileType<SnotBaseballSplit>(), Projectile.damage, 1f);
                    Projectile.Kill();
                }   
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            base.OnHitPlayer(target, info);

            target.AddBuff(BuffID.OgreSpit, 120);
        }

        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);
            for (int i = 0; i < 40; i++)
                Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi), GetGoreType());
        }

        public override bool PreDraw(ref Color lightColor)
        {
            FargoSoulsUtil.GenericProjectileDraw(Projectile, lightColor, rotation: Projectile.rotation);
            return false;
        }
    }

    public class SnotBaseballSplit : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Eternity/Enemies/Vanilla/OOA", "OgreSmallSpit");

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Main.projFrames[Type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.06f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            for (int i = 0; i < 2; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.JungleGrass);
                d.noGravity = true;
                d.velocity *= 0.2f;
            }

            if (Projectile.frameCounter++ > 5)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame > 3)
                    Projectile.frame = 0;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            base.OnHitPlayer(target, info);

            target.AddBuff(BuffID.OgreSpit, 180);
        }

        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.JungleGrass);
            base.OnKill(timeLeft);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            FargoSoulsUtil.GenericProjectileDraw(Projectile, lightColor, rotation: Projectile.rotation + MathHelper.Pi);
            return false;
        }
    }
}
