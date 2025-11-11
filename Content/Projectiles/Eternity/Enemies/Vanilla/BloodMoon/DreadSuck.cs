using System.Linq;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.BloodMoon;
using FargowiltasSouls.Content.Projectiles.Eternity.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.BloodMoon
{
    public class DreadSuck : ModProjectile
    {
        public override string Texture => FargoSoulsUtil.EmptyTexture;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.ignoreWater = true;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 200;
        }

        public override void AI()
        {
            NPC owner = Main.npc[(int)Projectile.ai[0]];
            if (owner == null || !owner.active || !owner.HasValidTarget)
            {
                Projectile.Kill();
                return;
            }
            Dreadnautilus.BloodNautilus_GetMouthPositionAndRotation(owner, out var mouthpos, out var mouthdir);
            Projectile.Center = mouthpos;
            Projectile.rotation = mouthdir;

            //suck in enemies
            foreach (NPC n in Main.npc.Where(n => n.Alive() && n.Hostile() && !n.boss && n.type != owner.type && (n.lifeMax <= owner.lifeMax || n.type == NPCID.BloodEelHead)))
            {
                if (mouthpos.Distance(n.Center) < owner.width / 2)
                {
                    owner.AddBuff(ModContent.BuffType<BloodDrinkerBuff>(), 360);
                    CombatText.NewText(n.Hitbox, Color.Red, n.life);

                    n.velocity = Vector2.Zero; //dont sent gores flying
                    n.life = 0;
                    n.HitEffect();
                    n.checkDead();
                    n.active = false;
                }
                else if (Projectile.timeLeft >= 30)
                {
                    float angleDif = MathHelper.ToDegrees(FargoSoulsUtil.RotationDifference(Projectile.rotation.ToRotationVector2(), Projectile.AngleTo(n.Center).ToRotationVector2()));
                    if ((angleDif < 45 && angleDif > -45) || n.Distance(mouthpos) <= 16 * 10 || n.Eternity().BeingSuckedByDread)
                    {
                        n.velocity = -n.AngleFrom(Projectile.Center).ToRotationVector2() * 30f;

                        n.noTileCollide = true;
                        n.Eternity().BeingSuckedByDread = true;
                    }
                }
            }

            //i can also suck in anticoag clots
            foreach (Projectile p in Main.projectile.Where(p => p.Alive()))
            {
                if (p.type == ModContent.ProjectileType<Bloodshed>() && Projectile.timeLeft >= 30)
                {
                    float angleDif = MathHelper.ToDegrees(FargoSoulsUtil.RotationDifference(Projectile.rotation.ToRotationVector2(), Projectile.AngleTo(p.Center).ToRotationVector2()));
                    if ((angleDif < 45 && angleDif > -45) || p.Distance(mouthpos) <= 16 * 10)
                        p.velocity = -p.AngleFrom(Projectile.Center).ToRotationVector2() * 30f;
                }
            }

            //and you.
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active && Projectile.timeLeft >= 60)
                {
                    float angleDif = MathHelper.ToDegrees(FargoSoulsUtil.RotationDifference(Projectile.rotation.ToRotationVector2(), Projectile.AngleTo(Main.player[i].Center).ToRotationVector2()));
                    if (angleDif < 30 && angleDif > -30)
                        Main.player[i].velocity -= Main.player[i].AngleFrom(Projectile.Center).ToRotationVector2() * 0.66f;
                }
            }

            if (Projectile.ai[1] > 0)
                Projectile.ai[1]--;
            if (Projectile.timeLeft < 90)
            {
                if (Projectile.ai[2] > 0)
                    Projectile.ai[2]--;
                if (Projectile.ai[2] <= 0)
                    Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.instance.LoadProjectile(ProjectileID.SandnadoHostile);
            Texture2D wind = TextureAssets.Projectile[ProjectileID.SandnadoHostile].Value;
            for (int i = (int)Projectile.ai[2]; i > (int)Projectile.ai[1]; i--)
            {
                float opacity = 1;
                if (i < 100)
                {
                    opacity = i / 100f;
                }
                if (i < Projectile.ai[2] + 50)
                {
                    opacity = i / (Projectile.ai[2] + 70);
                }

                Vector2 pos = Projectile.Center - Main.screenPosition + new Vector2(20, 0).RotatedBy(Projectile.rotation) + new Vector2(2, 0).RotatedBy(Projectile.rotation) * i * 5 * (i / 100f);
                Vector2 pos2 = Projectile.Center + new Vector2(20, 0).RotatedBy(Projectile.rotation) + new Vector2(2, 0).RotatedBy(Projectile.rotation) * i * 5 * (i / 100f) * Projectile.direction;
                Color color = Color.Lerp(Color.GhostWhite, Color.Red, 0.8f) * 0.5f * opacity;
                if (Lighting.GetColor(pos2.ToTileCoordinates()).Equals(Color.Black))
                    color = Color.Black * 0;
                Main.EntitySpriteDraw(wind, pos, null, color, Projectile.localAI[0] + MathHelper.ToRadians(i), wind.Size() / 2, 1f + i / (20f), SpriteEffects.None);
            }
            Projectile.localAI[0] += MathHelper.ToRadians(3);
            return false;
        }
    }
}

