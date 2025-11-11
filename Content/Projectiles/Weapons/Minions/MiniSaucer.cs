using System;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Content.Items.Accessories.Eternity;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Projectiles.Weapons.Minions
{
    public class MiniSaucer : ModProjectile
    {
        public override string Texture => FargoAssets.GetAssetString("Content/Projectiles/Weapons/Minions", Name);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 25;
            Projectile.height = 25;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        ref float PlayerHitTimer => ref Projectile.localAI[0];
        ref float PlayerHitCooldown => ref Projectile.ai[0];
        ref float FunnyHoverMovement => ref Projectile.ai[1];
        ref float HitTarget => ref Projectile.localAI[1];
        ref float AttackTimer => ref Projectile.localAI[2];
        private bool hoverleft;
        private bool reposition;
        private Vector2 hoverrng;
        public static Item FakeItem = null; // fake item exclusively used so that the accessory consumes ammo using vanilla method

        public void ConsumeAmmo(int ammoid)
        {
            if (FakeItem == null || FakeItem.useAmmo != ammoid)
            {
                FakeItem = new Item();
                FakeItem.useAmmo = Main.LocalPlayer.FindAmmo([AmmoID.Arrow, AmmoID.Bullet, AmmoID.Rocket, AmmoID.CandyCorn, AmmoID.Stake]).ammo;
            }
            if (Main.rand.NextBool(3)) //66% chance to not consume
                Main.player[Projectile.owner].PickAmmo(FakeItem, out int _, out float _, out int _, out float _, out int _);
        }

        public static int RocketTypeAi(int ammoitem)
        {
            return ammoitem switch
            {
                ItemID.RocketI => 1,
                ItemID.RocketII => 2,
                ItemID.RocketIII => 3,
                ItemID.RocketIV => 4,
                ItemID.MiniNukeI => 5,
                ItemID.MiniNukeII => 6,
                ItemID.ClusterRocketI => 7,
                ItemID.ClusterRocketII => 8,
                ItemID.DryRocket => 9,
                ItemID.WetRocket => 10,
                ItemID.LavaRocket => 11,
                ItemID.HoneyRocket => 12,
                _ => 1,
            };
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            NPC target = null;
            Item ammo = player.FindAmmo([AmmoID.Arrow, AmmoID.Bullet, AmmoID.Rocket, AmmoID.CandyCorn, AmmoID.Stake]);
            int damage = (int)((UfoMinionEffect.BaseDamage(player) + ammo.damage) * player.ActualClassDamage(DamageClass.Ranged));
            float shotspeed = ammo.shootSpeed + 14;
            bool sotm = player.FargoSouls().MasochistSoul;

            int switcher = (ammo.ammo == AmmoID.Bullet || ammo.ammo == AmmoID.CandyCorn) ? 1 : (ammo.ammo == AmmoID.Arrow || ammo.ammo == AmmoID.Stake) ? 2 : ammo.ammo == AmmoID.Rocket ? 3 : 4;
            if (player.active && !player.dead && player.FargoSouls().MiniSaucer)
                Projectile.timeLeft = 2;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].whoAmI == HitTarget && HitTarget != 0 && Main.npc[i].CanBeChasedBy())
                {
                    target = Main.npc[i];
                }
            }

            if (target != null && PlayerHitTimer > 0 && switcher != 4)
            {
                PlayerHitTimer--;
                AttackTimer++;
                switch (switcher) //attacks
                {
                    case 1: //bullets
                        HoveringMovement(target.Top, target.Center);
                        if (AttackTimer >= (sotm ? 16 : 12) && Projectile.owner == Main.myPlayer)
                        {
                            AttackTimer = 0;
                            int p = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Projectile.velocity, Vector2.UnitX.RotatedBy(Projectile.rotation + MathHelper.PiOver2) * shotspeed, ammo.shoot, damage, ammo.knockBack / 2, player.whoAmI);
                            if (p != Main.maxProjectiles)
                            {
                                if ((Main.projectile[p].penetrate > 1 || Main.projectile[p].penetrate <= -1) && !Main.projectile[p].usesIDStaticNPCImmunity && !Main.projectile[p].usesLocalNPCImmunity)
                                {
                                    Main.projectile[p].usesIDStaticNPCImmunity = true;
                                    Main.projectile[p].idStaticNPCHitCooldown = 10; //todo: shared static iframes between different proj types
                                }
                            }
                            SoundEngine.PlaySound(SoundID.Item11 with { Volume = 0.5f }, Projectile.Center);
                            ConsumeAmmo(ammo.ammo);
                        }
                        break;

                    case 2: //arrows
                        if (reposition)
                        {
                            float reporng = 64;
                            hoverrng = new Vector2(Main.rand.NextFloat(-reporng, reporng), Main.rand.NextFloat(-reporng, reporng));
                            reposition = false;
                        }
                        Vector2 pos = target.Top + new Vector2(0, -160);
                        Projectile.velocity = FargoSoulsUtil.SmartAccel(Projectile.Center, pos + hoverrng, Projectile.velocity, 1.5f, 1.5f);
                        Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.DirectionTo(target.Center).ToRotation() - MathHelper.PiOver2, 0.2f);

                        if (Projectile.Distance(pos + hoverrng) < 64f)
                        {
                            if (AttackTimer >= (sotm ? 40 : 30) && AttackTimer % 6 == 0 && Projectile.owner == Main.myPlayer)
                            {
                                int p = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Projectile.velocity, Vector2.UnitX.RotatedBy(Projectile.rotation + MathHelper.PiOver2) * shotspeed, ammo.shoot, damage, ammo.knockBack / 2, player.whoAmI);
                                if (p != Main.maxProjectiles)
                                {
                                    if ((Main.projectile[p].penetrate > 1 || Main.projectile[p].penetrate <= -1) && !Main.projectile[p].usesIDStaticNPCImmunity && !Main.projectile[p].usesLocalNPCImmunity)
                                    {
                                        Main.projectile[p].usesIDStaticNPCImmunity = true;
                                        Main.projectile[p].idStaticNPCHitCooldown = 10; //todo: shared static iframes between different proj types
                                    }
                                }
                                SoundEngine.PlaySound(SoundID.Item5 with { Volume = 0.5f }, Projectile.Center);
                                ConsumeAmmo(ammo.ammo);
                            }
                        }
                        if (AttackTimer >= 60)
                        {
                            AttackTimer = 0;
                            reposition = true;
                        }
                        break;

                    case 3: //rockets
                        if (reposition)
                        {
                            if (hoverleft) hoverleft = false;
                            else hoverleft = true;
                            reposition = false;
                        }
                        float rotation;
                        Vector2 corner = hoverleft ? target.TopLeft : target.TopRight;
                        Vector2 dest = new(corner.X + target.width * 3 * (hoverleft ? 1 : -1), corner.Y - target.height * 1.5f);
                        if (Projectile.Center.Distance(dest) > 4f)
                        {
                            Projectile.velocity = FargoSoulsUtil.SmartAccel(Projectile.Center, dest, Projectile.velocity, 1f, 1.5f);
                            rotation = dest.X > Projectile.Center.X ? 0.5f : -0.5f;
                        }
                        else //snap to position
                        {
                            Projectile.velocity = Vector2.Zero;
                            Projectile.Center = dest;
                            rotation = 0;
                        }
                        Projectile.rotation = Projectile.rotation.AngleLerp(rotation, 0.2f);

                        if (AttackTimer >= (sotm ? 80 : 60) && AttackTimer % 12 == 0 && Projectile.owner == Main.myPlayer)
                        {
                            int p = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Projectile.velocity, Vector2.UnitX.RotatedBy(Projectile.rotation).RotatedByRandom(MathHelper.TwoPi) * shotspeed, ModContent.ProjectileType<SaucerRocket>(), damage, ammo.knockBack / 2, player.whoAmI, target.whoAmI, 20f, RocketTypeAi(ammo.type));
                            if (p != Main.maxProjectiles)
                            {
                                if ((Main.projectile[p].penetrate > 1 || Main.projectile[p].penetrate <= -1) && !Main.projectile[p].usesIDStaticNPCImmunity && !Main.projectile[p].usesLocalNPCImmunity)
                                {
                                    Main.projectile[p].usesIDStaticNPCImmunity = true;
                                    Main.projectile[p].idStaticNPCHitCooldown = 10; //todo: shared static iframes between different proj types
                                }
                            }
                            SoundEngine.PlaySound(SoundID.Item39, Projectile.Center);
                            ConsumeAmmo(ammo.ammo);
                        }
                        if (AttackTimer >= 120)
                        {
                            AttackTimer = 0;
                            reposition = true;
                        }
                        break;

                    default:
                        Main.NewText("you shouldnt be seeing this text, show schmoovi");
                        break;
                }
            }
            else //idle
            {
                HitTarget = -1;
                AttackTimer = 0;
                HoveringMovement(player.Top, player.Center);
            }

            if (Projectile.Distance(player.Center) > 2000)
            {
                AttackTimer = 0;
                Projectile.Center = player.Center;
            }
            if (PlayerHitCooldown > 0)
                PlayerHitCooldown--;
            float lightFade = (255f - Projectile.alpha) / 255f;
            Color color = Color.Cyan;
            Lighting.AddLight(Projectile.Center + Projectile.velocity, lightFade * color.R / 255f, lightFade * color.G / 255f, lightFade * color.B / 255f);
        }
        public void HoveringMovement(Vector2 pos, Vector2 rotateat)
        { //only used by idle and bullet behavior
            if (FunnyHoverMovement >= 64) hoverleft = false;
            if (FunnyHoverMovement <= -64) hoverleft = true;
            if (hoverleft) FunnyHoverMovement += 2;
            else FunnyHoverMovement -= 2;

            float resistance = Projectile.velocity.Length() * 1 / 50f;
            Projectile.velocity = FargoSoulsUtil.SmartAccel(Projectile.Center, pos + new Vector2(FunnyHoverMovement, -128 + Math.Abs(FunnyHoverMovement / 2)), Projectile.velocity, 1 - resistance, 1 + resistance);
            Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.DirectionTo(rotateat).ToRotation() - MathHelper.PiOver2, 0.2f);
        }

        public override bool? CanDamage()
        {
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            FargoSoulsUtil.GenericProjectileDraw(Projectile, lightColor);
            return false;
        }
    }
}