using System;
using System.Collections.Generic;
using System.Linq;
using FargowiltasSouls.Assets.Particles;
using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Achievements;
using FargowiltasSouls.Content.Buffs;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Items.Accessories.Eternity;
using FargowiltasSouls.Content.Items.Accessories.Forces;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Projectiles.Accessories;
using FargowiltasSouls.Content.Projectiles.Accessories.HeartOfTheMaster;
using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Content.Projectiles.Accessories.SupremeDeathbringerFairy;
using FargowiltasSouls.Content.Projectiles.Accessories.VerdantDoomsayerMask;
using FargowiltasSouls.Content.Projectiles.Eternity;
using FargowiltasSouls.Content.Projectiles.Eternity.Buffs;
using FargowiltasSouls.Content.Projectiles.Weapons.Minions;
using FargowiltasSouls.Content.UI.Elements;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace FargowiltasSouls.Core.ModPlayers
{
    public partial class FargoSoulsPlayer
    {

        public void GoldKey(bool stunned)
        {
            if (!Player.HasBuff(ModContent.BuffType<GoldenStasisBuff>()) && !Player.HasBuff(ModContent.BuffType<GoldenStasisCDBuff>()))
            {
                if (stunned)
                    return;
                int duration = 300;

                if (ForceEffect<GoldEnchant>())
                {
                    duration *= 2;
                }

                Player.AddBuff(ModContent.BuffType<GoldenStasisBuff>(), duration);
                Player.AddBuff(ModContent.BuffType<GoldenStasisCDBuff>(), 3600);

                goldHP = Player.statLife;

                if (!Main.dedServ)
                    SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/Accessories/GoldEncase"), Player.Center);
            }
            //cancel it early
            else
            {
                Player.ClearBuff(ModContent.BuffType<GoldenStasisBuff>());
            }
        }

        public void GoldUpdate()
        {
            Player.immune = true;
            Player.immuneTime = 90;
            Player.hurtCooldowns[0] = 90;
            Player.hurtCooldowns[1] = 90;
            Player.stealth = 1;

            //immune to DoT
            if (Player.statLife < goldHP)
                Player.statLife = goldHP;

            if (Player.ownedProjectileCounts[ModContent.ProjectileType<GoldShellProj>()] <= 0)
                Projectile.NewProjectile(Player.GetSource_Misc(""), Player.Center.X, Player.Center.Y, 0f, 0f, ModContent.ProjectileType<GoldShellProj>(), 0, 0, Main.myPlayer);
        }

        
        public bool JungleJumping = false;


        private int GetNumSentries()
        {
            int count = 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];

                if (p.active && p.owner == Player.whoAmI && p.sentry)
                {
                    count++;
                }
            }

            return count;
        }

        //saved mount stats for squire
        //public Mount.MountData OriginalSquireMountData;
        public Mount.MountData BaseSquireMountData;

        public int BaseMountType = -1;


        //public float baseMountAccel;
        //public float baseMountRunSpeed;
        //public int baseMountJumpHeight;

        //        #endregion

        //        #region souls
        

        public bool extraCarpetDuration = true;



        //        #endregion

        #region maso acc

        public void OnLandingEffects()
        {
            if (SlimyShieldFalling) //landing
            {
                if (Player.velocity.Y < 0f)
                    SlimyShieldFalling = false;

                if (Player.velocity.Y == 0f)
                {
                    SlimyShieldFalling = false;
                    if (Player.whoAmI == Main.myPlayer && Player.gravDir > 0)
                    {
                        if (SlimyShieldItem != null && Player.HasEffect<SlimyShieldEffect>())
                        {
                            SoundEngine.PlaySound(SoundID.Item21 with { Volume = 0.5f }, Player.Center);
                            for (int i = 0; i < 3; i++)
                            {
                                Vector2 dir = -Vector2.UnitY.RotatedByRandom(MathHelper.PiOver2 * 0.2f);
                                Projectile.NewProjectile(Player.GetSource_Accessory(SlimyShieldItem, "SlimyShield"), Player.Center + dir * Main.rand.NextFloat(10f, 40f), dir * Main.rand.NextFloat(10f, 14f), ModContent.ProjectileType<KingSlimeBallFriendly>(), SlimyShieldEffect.BaseDamage(Player), 1f, Main.myPlayer);
                            }
                        }
                    }
                }
            }
            else if (Player.velocity.Y > 3f)
            {
                SlimyShieldFalling = true;
            }
        }




        public void FrigidGraspKey()
        {
            if (Player.GetCooldown<FrigidGraspKeyEffect>() > 0)
                return;

            if (!Player.CheckMana(6, true))
                return;

            Player.SetCooldown<FrigidGraspKeyEffect>(20);
            Player.manaRegenDelay = Math.Max(Player.manaRegenDelay, 30);

            SoundEngine.PlaySound(SoundID.Item28, Player.Center);
            Projectile.NewProjectile(Player.GetSource_EffectItem<FrigidGraspKeyEffect>(), Player.Center, 12f * Player.SafeDirectionTo(Main.MouseWorld), ProjectileID.IceBlock, FrigidGraspKeyEffect.BaseDamage(Player), 2f, Player.whoAmI, Player.tileTargetX, Player.tileTargetY);
        }


        public void SpecialDashKey(int type)
        {
            Player player = Main.player[Main.myPlayer];
            if (Player.GetCooldown<SpecialDashEffect>() <= 0)
            {
                Player.SetCooldown<SpecialDashEffect>(LumUtils.SecondsToFrames(5));

                if (Player.whoAmI == Main.myPlayer)
                {
                    Player.RemoveAllGrapplingHooks();

                    /*Player.controlLeft = false;
                    Player.controlRight = false;
                    Player.controlJump = false;
                    Player.controlDown = false;*/
                    Player.controlUseItem = false;
                    Player.controlUseTile = false;
                    Player.controlHook = false; 
                    //Player.controlMount = false;

                    Player.itemAnimation = 0;
                    Player.itemTime = 0;
                    Player.reuseDelay = 0;

                    if (player.HasEffect<BetsyDashEffect>() && type == 2)
                    {
                        Vector2 vel = Player.SafeDirectionTo(Main.MouseWorld) * 25;
                        Projectile.NewProjectile(Player.GetSource_Accessory(BetsysHeartItem), Player.Center, vel, ModContent.ProjectileType<BetsyDash>(), BetsyDashEffect.BaseDamage(player), 6f, Player.whoAmI);

                        Player.immune = true;
                        Player.immuneTime = Math.Max(Player.immuneTime, 2);
                        Player.hurtCooldowns[0] = Math.Max(Player.hurtCooldowns[0], 2);
                        Player.hurtCooldowns[1] = Math.Max(Player.hurtCooldowns[1], 2);

                        CooldownBarManager.Activate("SpecialDashCooldown", FargoAssets.GetTexture2D("Content/Items/Accessories/Eternity","BetsysHeart").Value, Color.OrangeRed, 
                            () => 1 - Main.LocalPlayer.GetCooldown<SpecialDashEffect>() / LumUtils.SecondsToFrames(5), activeFunction: () => BetsysHeartItem != null);
                    }
                    else if (player.HasEffect<SupremeDashEffect>() && type == 1)
                    {
                        Player.IncrementCooldown<SpecialDashEffect>(LumUtils.SecondsToFrames(1));

                        Vector2 vel = Player.SafeDirectionTo(Main.MouseWorld) * 25;
                        Projectile.NewProjectile(Player.GetSource_Accessory(QueenStingerItem), Player.Center, vel, ModContent.ProjectileType<SupremeDash>(), SupremeDashEffect.BaseDamage(player) * 3, 6f, Player.whoAmI);

                        CooldownBarManager.Activate("SpecialDashCooldown", FargoAssets.GetTexture2D("Content/Items/Accessories/Eternity", "SupremeDeathbringerFairy").Value, Color.LightGray,
                            () => 1 - Main.LocalPlayer.GetCooldown<SpecialDashEffect>() / LumUtils.SecondsToFrames(6), activeFunction: () => QueenStingerItem != null);
                    }
                    else if (player.HasEffect<SpecialDashEffect>() && type == 0)
                    {
                        Player.IncrementCooldown<SpecialDashEffect>(LumUtils.SecondsToFrames(1));

                        Vector2 vel = Player.SafeDirectionTo(Main.MouseWorld) * 20;
                        Projectile.NewProjectile(Player.GetSource_Accessory(QueenStingerItem), Player.Center, vel, ModContent.ProjectileType<BeeDash>(), (int)(44 * Player.ActualClassDamage(DamageClass.Melee)), 6f, Player.whoAmI);

                        CooldownBarManager.Activate("SpecialDashCooldown", FargoAssets.GetTexture2D("Content/Items/Accessories/Eternity", "QueenStinger").Value, Color.Yellow, 
                            () => 1 - Main.LocalPlayer.GetCooldown<SpecialDashEffect>() / LumUtils.SecondsToFrames(6), activeFunction: () => QueenStingerItem != null);
                    }
                    Player.AddBuff(ModContent.BuffType<BetsyDashBuff>(), 20);
                }
            }
        }

        public bool TryCleanseDebuffs()
        {
            bool cleansed = false;

            int max = Player.buffType.Length;
            for (int i = 0; i < max; i++)
            {
                int timeLeft = Player.buffTime[i];
                if (timeLeft <= 0)
                    continue;

                int buffType = Player.buffType[i];
                if (buffType <= 0)
                    continue;

                if (timeLeft > 5
                    && Main.debuff[buffType]
                    && !Main.buffNoTimeDisplay[buffType]
                    && !BuffID.Sets.NurseCannotRemoveDebuff[buffType])
                {
                    Player.DelBuff(i);

                    cleansed = true;

                    i--;
                    max--; //just in case, to prevent being stuck here forever
                }
            }

            return cleansed;
        }

        public void MagicalBulbKey()
        {
            if (Player.HasBuff(ModContent.BuffType<MagicalCleanseCDBuff>()))
                return;
            TryCleanseDebuffs();
            if (true)
            {
                int cdInSec = 40;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].Distance(Player.Center) < 2500 && Main.npc[i].active && !Main.npc[i].friendly && Main.npc[i].lifeMax > 5 && !Main.npc[i].dontTakeDamage)
                    {
                        Main.npc[i].AddBuff(ModContent.BuffType<MagicalCurseBuff>(), LumUtils.SecondsToFrames(5));
                        float speed = Main.npc[i].scale * 10;
                        for (int p = 0; p < 10; p++)
                        {
                            Color color = Color.Lime;
                            if (p % 2 == 0)
                                color = Color.DeepPink;
                            Particle s = new SparkParticle(Main.npc[i].Center, Main.rand.NextVector2CircularEdge(speed, speed) + Main.npc[i].velocity, color, 1f, 60, true, Color.White);
                            if (!Main.npc[i].buffImmune[ModContent.BuffType<MagicalCurseBuff>()])
                                s.Spawn();
                        }
                    }
                }    

                Player.AddBuff(ModContent.BuffType<MagicalCleanseCDBuff>(), LumUtils.SecondsToFrames(cdInSec));

                SoundEngine.PlaySound(SoundID.Item4, Player.Center);

                for (int i = 0; i < 8; ++i)
                {
                    Color color = Color.Lime;
                    if (i % 2 == 0)
                        color = Color.DeepPink;
                    Particle s = new SparkParticle(Player.Bottom, Vector2.UnitX.RotatedBy(MathHelper.PiOver4 * i) * 5, color, 1f, 60, true, Color.White);
                    s.Velocity.Y -= 5;
                    if (s.Velocity.Y == 0)
                        s.Lifetime = 0; //kills bottom particle, pattern should look like a leaf or bulb kinda
                    s.Spawn();
                }
            }
        }

        public void BombKey()
        {
            if (MutantEyeItem != null && Player.GetCooldown<BombKeyEffect>() <= 0)
            {
                Player.SetCooldown<BombKeyEffect>(3600);

                if (!Main.dedServ && Main.LocalPlayer.active)
                    ScreenShakeSystem.StartShake(10, shakeStrengthDissipationIncrement: 10f / 30);

                const int invulTime = 90;
                Player.immune = true;
                Player.immuneTime = invulTime;
                Player.hurtCooldowns[0] = invulTime;
                Player.hurtCooldowns[1] = invulTime;

                SoundEngine.PlaySound(SoundID.Item84, Player.Center);

                const int max = 100; //make some indicator dusts
                for (int i = 0; i < max; i++)
                {
                    Vector2 vector6 = Vector2.UnitY * 30f;
                    vector6 = vector6.RotatedBy((i - (max / 2 - 1)) * 6.28318548f / max) + Main.LocalPlayer.Center;
                    Vector2 vector7 = vector6 - Main.LocalPlayer.Center;
                    int d = Dust.NewDust(vector6 + vector7, 0, 0, DustID.Vortex, 0f, 0f, 0, default, 3f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity = vector7;
                }

                for (int i = 0; i < 50; i++) //make some indicator dusts
                {
                    int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.Vortex, 0f, 0f, 0, default, 2.5f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].noLight = true;
                    Main.dust[d].velocity *= 24f;
                }

                FargoSoulsUtil.ClearHostileProjectiles(1);

                void SpawnSphereRing(int ringMax, float speed, int damage2, float rotationModifier)
                {
                    float rotation = 2f * (float)Math.PI / ringMax;
                    Vector2 vel = Vector2.UnitY * speed;
                    int type = ModContent.ProjectileType<PhantasmalSphereRing>();
                    for (int i = 0; i < ringMax; i++)
                    {
                        vel = vel.RotatedBy(rotation);
                        Projectile.NewProjectile(Player.GetSource_Accessory(MutantEyeItem), Player.Center, vel, type, damage2, 3f, Main.myPlayer, rotationModifier * Player.direction, speed);
                    }
                }

                int damage = (int)(1700 * Player.ActualClassDamage(DamageClass.Magic));
                SpawnSphereRing(24, 12f, damage, -1f);
                SpawnSphereRing(24, 12f, damage, 1f);
            }
        }

        public void DebuffInstallKey()
        {
            if (Player.HasEffect<FusedLensInstall>())
            {
                int buffType = ModContent.BuffType<TwinsInstallBuff>();
                if (Player.HasBuff(buffType))
                {
                    Player.ClearBuff(buffType);
                }
                else
                {
                    SoundEngine.PlaySound(FargosSoundRegistry.TrojanCannonDeath with { Volume = 0.5f }, Player.Center);

                    Player.AddBuff(buffType, 2);
                    for (int i = 0; i < 30; i++)
                    {
                        Vector2 dir = new(Main.rand.NextFloat(-18, 22), Main.rand.NextFloat(-4, -22));
                        Color color = Color.Lerp(Color.Green, Color.Yellow, Main.rand.NextFloat(0, 1));
                        float offset = -MathHelper.PiOver2 + Main.rand.NextFloatDirection() * 0.51f;
                        Particle p = new SmokeParticle(Player.Center, Player.velocity * 0.7f + dir, color, Main.rand.Next(40, 80), Main.rand.NextFloat(0.4f, 0.8f), Main.rand.NextFloat(0.04f, 0.08f), offset, false);
                        p.Spawn();
                    }
                }
            }

            if (Player.HasEffect<AgitatingLensInstall>())
            {
				if (!Player.HasBuff(ModContent.BuffType<BerserkerInstallBuff>())
					&& !Player.HasBuff(ModContent.BuffType<BerserkerInstallCDBuff>()))
				{
					SoundEngine.PlaySound(SoundID.Item119, Player.Center);
					Player.AddBuff(ModContent.BuffType<BerserkerInstallBuff>(), LumUtils.SecondsToFrames(8f));
                    Player.FargoSouls().BerserkedFromAgitation = true;
					for (int i = 0; i < 60; i++)
					{
						int index2 = Dust.NewDust(Player.position, Player.width, Player.height, DustID.RedTorch, 0f, 0f, 0, default, 3f);
						Main.dust[index2].noGravity = true;
						Main.dust[index2].velocity *= 9;
					}
				}
            }
        }

        public void AmmoCycleKey()
        {
            SoundEngine.PlaySound(SoundID.Item149, Player.Center);
            Vector2 vel = new(Main.rand.NextFloat(-2, -6) * Player.direction, Main.rand.NextFloat(-5, -8));
            Gore.NewGore(Player.GetSource_Accessory(Player.EffectItem<AmmoCycleEffect>()), Player.Top, vel, ModContent.Find<ModGore>(Mod.Name, "MartianCartridge").Type);
            for (int i = 54; i <= 56; i++)
            {
                int j = i + 1;
                (Player.inventory[j], Player.inventory[i]) = (Player.inventory[i], Player.inventory[j]);
            }

            if (Player.HasEffect<UfoMinionEffect>())
            {
                foreach (Projectile proj in Main.ActiveProjectiles)
                {
                    if (proj.type == ModContent.ProjectileType<MiniSaucer>() && proj.owner == Player.whoAmI)
                    {
                        Gore.NewGore(Player.GetSource_Accessory(Player.EffectItem<AmmoCycleEffect>()), proj.Top, new(Main.rand.NextFloat(-3, 3), vel.Y), ModContent.Find<ModGore>(Mod.Name, "MartianCartridge").Type);
                        break;
                    }
                }
            }
        }

        int fastFallCD;
        public void TryFastfallUpdate()
        {
            if (fastFallCD > 0)
                fastFallCD--;
            if (!(Player.gravDir > 0 && Player.HasEffect<DiveEffect>()))
                return;

            bool keying = Player.controlDown && Player.releaseDown && Player.doubleTapCardinalTimer[0] > 0 && Player.doubleTapCardinalTimer[0] != 15;
            
            ModKeybind key = AccessoryEffectLoader.GetKeybind<DiveEffect>(this);
            if (key == null)
                return;
            if (key.GetAssignedKeys().Any())
                keying = key.JustPressed;

            if (fastFallCD <= 0 && !Player.mount.Active && !Player.controlJump && keying)
            {
                if (Player.velocity.Y != 0f)
                {
                    if (Player.velocity.Y < 15f)
                    {
                        Player.velocity.Y = 15f;
                    }

                    if (GroundPound <= 0)
                    {
                        GroundPound = 1;
                    }
                }
            }

            if (GroundPound > 0)
            {
                fastFallCD = 90;

                if (Player.velocity.Y == 0f && Player.controlDown)
                {
                    Vector2 vec = Collision.TileCollision(Player.position, 15f * Vector2.UnitY, Player.width, Player.height, true, true, (int)Player.gravDir);
                    if (vec != Vector2.Zero)
                    {
                        Player.position += vec;
                        Player.velocity.Y = 15f;
                    }
                }

                if (Player.velocity.Y < 0f || Player.mount.Active || (Player.controlJump && !Player.controlDown))
                {
                    GroundPound = 0;
                }
                else if (Player.velocity.Y == 0f && Player.oldVelocity.Y == 0f)
                {
                    int x = (int)Player.Center.X / 16;
                    int y = (int)(Player.position.Y + Player.height + 8) / 16;
                    if (x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY
                        && Main.tile[x, y] != null && Main.tile[x, y].HasUnactuatedTile && Main.tileSolid[Main.tile[x, y].TileType])
                    {
                        GroundPound = 0;

                        if (Player.HasEffect<LihzahrdBoulders>())
                        {
                            if (!Main.dedServ)
                                ScreenShakeSystem.StartShake(10, shakeStrengthDissipationIncrement: 10f / 30);

                            if (Player.whoAmI == Main.myPlayer)
                            {
                                //explosion
                                Projectile.NewProjectile(Player.GetSource_Accessory(LihzahrdTreasureBoxItem), Player.Center, Vector2.Zero, ModContent.ProjectileType<MoonLordSunBlast>(), 0, 0f, Player.whoAmI);
                                int p = Projectile.NewProjectile(Player.GetSource_Accessory(LihzahrdTreasureBoxItem), Player.Center, Vector2.Zero, ModContent.ProjectileType<Explosion>(), LihzahrdGroundPound.BaseDamage(Player) * 3, 9f, Player.whoAmI);
                                if (p != Main.maxProjectiles)
                                    Main.projectile[p].DamageType = DamageClass.Melee;

                                //boulders
                                for (int i = -5; i <= 5; i += 2)
                                {
                                    int b = Projectile.NewProjectile(Player.GetSource_Accessory(LihzahrdTreasureBoxItem), Player.Center, -10f * Vector2.UnitY.RotatedBy(MathHelper.PiOver2 / 6 * i),
                                        ModContent.ProjectileType<LihzahrdBoulderFriendly>(), LihzahrdGroundPound.BaseDamage(Player), 7.5f, Player.whoAmI);
                                    if (b != Main.maxProjectiles)
                                        Main.projectile[b].DamageType = DamageClass.Melee;
                                }

                                //geysers
                                y -= 2;
                                for (int i = -3; i <= 3; i++)
                                {
                                    if (i == 0)
                                        continue;
                                    int tilePosX = x + 16 * i;
                                    int tilePosY = y;
                                    if (Main.tile[tilePosX, tilePosY] != null && tilePosX >= 0 && tilePosX < Main.maxTilesX)
                                    {
                                        while (Main.tile[tilePosX, tilePosY] != null && tilePosY >= 0 && tilePosY < Main.maxTilesY
                                            && !(Main.tile[tilePosX, tilePosY].HasUnactuatedTile && Main.tileSolid[Main.tile[tilePosX, tilePosY].TileType]))
                                        {
                                            tilePosY++;
                                        }
                                        int g = Projectile.NewProjectile(Player.GetSource_Accessory(LihzahrdTreasureBoxItem), tilePosX * 16 + 8, tilePosY * 16 + 8, 0f, -8f, ModContent.ProjectileType<GeyserFriendly>(), LihzahrdGroundPound.BaseDamage(Player) / 2, 8f, Player.whoAmI);
                                        if (g != Main.maxProjectiles)
                                            Main.projectile[g].DamageType = DamageClass.Melee;
                                    }
                                }
                            }
                        }
                    }

                }
                else
                {
                    if (Player.controlDown && Player.velocity.Y < 15f)
                    {
                        Player.velocity.Y = 15f;
                    }

                    Player.maxFallSpeed = 20f;
                    GroundPound++;
                    for (int i = 0; i < 5; i++)
                    {
                        int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.Torch, Scale: 1.5f);
                        Main.dust[d].noGravity = true;
                        Main.dust[d].velocity *= 0.2f;
                    }
                }
            }
        }

        public void AbomWandUpdate()
        {
            if (AbominableWandRevived) //has been revived already
            {
                if (Player.statLife >= Player.statLifeMax2 && Player.statLife >= 400) //can revive again
                {
                    AbominableWandRevived = false;

                    SoundEngine.PlaySound(SoundID.Item28, Player.Center);

                    FargoSoulsUtil.DustRing(Player.Center, 50, 87, 8f, default, 2f);

                    for (int i = 0; i < 30; i++)
                    {
                        int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.GemTopaz, 0f, 0f, 0, default, 2.5f);
                        Main.dust[d].noGravity = true;
                        Main.dust[d].velocity *= 8f;
                    }
                }
                else //cannot currently revive
                {
                    Player.AddBuff(ModContent.BuffType<AbomCooldownBuff>(), 2);
                }
            }
        }

        public void DreadParryCounter()
        {
            SoundEngine.PlaySound(SoundID.Item119, Player.Center);

            if (Player.whoAmI == Main.myPlayer)
            {
                int type = ModContent.ProjectileType<Bloodshed>();
                const float speed = 12f;
                Projectile.NewProjectile(Player.GetSource_EffectItem<DreadShellEffect>(), Player.Center, Main.rand.NextVector2Circular(speed, speed), type, 0, 0f, Main.myPlayer, 1f);

                const int max = 20;
                for (int i = 0; i < max; i++)
                {
                    void SharpTears(Vector2 pos, Vector2 vel)
                    {
                        int p = Projectile.NewProjectile(Player.GetSource_EffectItem<DreadShellEffect>(), pos, vel, ProjectileID.SharpTears, DreadShellEffect.BaseDamage(Player), 12f, Player.whoAmI, 0f, Main.rand.NextFloat(0.5f, 1f));
                        if (p != Main.maxProjectiles)
                        {
                            Main.projectile[p].DamageType = DamageClass.Melee;
                            Main.projectile[p].usesLocalNPCImmunity = false;
                            Main.projectile[p].usesIDStaticNPCImmunity = true;
                            Main.projectile[p].idStaticNPCHitCooldown = 60;
                            Main.projectile[p].FargoSouls().noInteractionWithNPCImmunityFrames = true;
                            Main.projectile[p].FargoSouls().DeletionImmuneRank = 1;
                        }
                    }

                    SharpTears(Player.Center, 16f * Vector2.UnitX.RotatedBy(MathHelper.TwoPi / max * (i + Main.rand.NextFloat(-0.5f, 0.5f))));

                    for (int k = 0; k < 6; k++)
                    {
                        Vector2 spawnPos = Player.Bottom;
                        spawnPos.X += Main.rand.NextFloat(-256, 256);

                        bool foundTile = false;

                        for (int j = 0; j < 40; j++)
                        {
                            Tile tile = Framing.GetTileSafely(spawnPos);
                            if (tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]))
                            {
                                foundTile = true;
                                break;
                            }
                            spawnPos.Y += 16;
                        }

                        if (!foundTile)
                            spawnPos.Y = Player.Bottom.Y + Main.rand.NextFloat(-64, 64);

                        for (int j = 0; j < 40; j++)
                        {
                            Tile tile = Framing.GetTileSafely(spawnPos);
                            if (tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType]))
                                spawnPos.Y -= 16;
                            else
                                break;
                        }

                        if (!Collision.SolidCollision(spawnPos, 0, 0))
                        {
                            spawnPos.Y += 16;
                            SharpTears(spawnPos, 16f * -Vector2.UnitY.RotatedByRandom(MathHelper.ToRadians(30)));
                            break;
                        }
                    }
                }
            }
        }

        public void ShadowParryCounter()
        {
            SoundEngine.PlaySound(FargosSoundRegistry.CoffinBigShot, Player.Center);
            Vector2 spawnpos = Player.Center;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].type == ModContent.ProjectileType<PungentEyeballMinion>() && Main.projectile[i].owner == Player.whoAmI && Main.projectile[i].active)
                {
                    spawnpos = Main.projectile[i].Center;
                    Main.projectile[i].localAI[2] = 30;
                    break;
                }
            }
            for (int s = 0; s < 12; s++)
            {
                Vector2 vel = Main.rand.NextFloat(7, 15) * Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
                int p = Projectile.NewProjectile(Player.GetSource_EffectItem<LithosphericEffect>(), spawnpos, vel, ModContent.ProjectileType<LithoFlame>(), LithosphericEffect.BaseDamage(Player) / 2, 0, Player.whoAmI);
            }
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].Distance(Player.Center) < 2500 && Main.npc[i].active && !Main.npc[i].friendly && Main.npc[i].lifeMax > 5 && !Main.npc[i].dontTakeDamage)
                    Main.npc[i].AddBuff(BuffID.Cursed, 360);
            }
        }

        public void PumpkingsCapeCounter(int damage)
        {
            SoundEngine.PlaySound(SoundID.Item62, Player.Center);

            if (Player.whoAmI == Main.myPlayer)
            {
                int heal = GetHealMultiplier(damage) / 2;
                Player.statLife += heal;
                if (Player.statLife > Player.statLifeMax2)
                    Player.statLife = Player.statLifeMax2;
                Player.HealEffect(heal);

                for (int i = 0; i < 16; i++)
                {
                    Color color = Color.OrangeRed;
                    if (i % 2 == 0)
                        color = new Color(30, 0, 60);
                    Particle s = new SparkParticle(Player.Center, Vector2.UnitX.RotatedBy(MathHelper.Pi/8 * i) * 15, color, 2f, 60);
                    s.Spawn();
                }

                Projectile.NewProjectile(Player.GetSource_EffectItem<PumpkingsCapeEffect>(), Player.Center, Vector2.Zero, ModContent.ProjectileType<Pumpscare>(), 0, 0f, Main.myPlayer);
            }
        }

        public void DeerSinewDash(int dir)
        {
            float dashSpeed = 12f;
            DashCD = 60;
            Player.dashDelay = DashCD;
            if (IsDashingTimer < 15)
                IsDashingTimer = 15;
            Player.velocity.X = dir * dashSpeed;
            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.PlayerControls, number: Player.whoAmI);
        }
        public float DeerSinewCritNerf()
        {
            float ratio = Math.Min(Player.velocity.Length() / 16f, 1f);
            return MathHelper.Lerp(1f, 0.75f, ratio);
        }

        #endregion maso acc

        // TODO: rework this because we can only get final damage in hurtinfo hooks
        public void TryParryAttack(ref Player.HurtInfo hurtInfo)
        {
            bool silverEffect = Player.HasEffect<SilverEffect>();
            bool dreadEffect = Player.HasEffect<DreadShellEffect>();
            bool lithoEffect = Player.HasEffect<LithosphericEffect>();
            bool pumpkingEffect = Player.HasEffect<PumpkingsCapeEffect>();
            if (GuardRaised && shieldTimer > 0 && !Player.immune)
            {
                Player.immune = true;
                int invul = Player.longInvince ? 90 : 60;
                int extrashieldCD = 40;

                int damageBlockCap = 100;
                const int higherCap = 200;

                if (dreadEffect || pumpkingEffect)
                {
                    damageBlockCap = higherCap;

                    invul += 60;

                    extrashieldCD = LONG_SHIELD_COOLDOWN;
                }
                else if (silverEffect)
                {
                    extrashieldCD = BASE_SHIELD_COOLDOWN;
                }

                bool perfectParry = shieldHeldTime <= HARD_PARRY_WINDOW;

                if (silverEffect)
                {
                    if (perfectParry)
                    {
                        damageBlockCap = 150;
                        if (Player.HasEffectEnchant<SilverEffect>())
                            Player.AddBuff(ModContent.BuffType<SilverBuff>(), ForceEffect<SilverEnchant>() ? 180 : 60);
                        SoundEngine.PlaySound(SoundID.Item4, Player.Center);

                        if (Player.HasEffect<TerraEffect>())
                        {
                            TerraProcCD = 0;
                            int targetID = FargoSoulsUtil.FindClosestHostileNPC(Player.Center, 1000, true, true);
                            if (Player.HasEffect<TinEffect>())
                            {
                                TinCrit += 25;
                                if (TinCrit > TinCritMax)
                                    TinCrit = TinCritMax;
                                else
                                    CombatText.NewText(Player.Hitbox, Color.Yellow, Language.GetTextValue("Mods.FargowiltasSouls.Items.TinEnchant.CritUp", 25));
                            }
                            /*if (targetID.IsWithinBounds(Main.maxNPCs) && Main.npc[targetID] is NPC target && target.Alive())
                            {
                                TerraLightningEffect.LightningProc(Player, target, 7f);
                            }*/

                        }
                    }
                    if (ForceEffect<SilverEnchant>())
                        damageBlockCap = higherCap;
                    int sheet = perfectParry ? 1 : 0; // which parry vfx sprite sheet to use
                    Projectile.NewProjectile(Player.GetSource_Misc(""), Player.Center, Vector2.Zero, ModContent.ProjectileType<IronParry>(), 0, 0f, Main.myPlayer, sheet);
                }

                int damageBlocked = Math.Min(damageBlockCap, hurtInfo.Damage);
                var newDamage = hurtInfo.Damage - damageBlocked;
                if (newDamage < 1)
                {
                    hurtInfo.Null();
                }
                else
                {
                    hurtInfo.Damage = newDamage;
                }

                if (dreadEffect && !lithoEffect && perfectParry)
                {
                    DreadParryCounter();
                }
                else if (lithoEffect && perfectParry)
                {
                    ShadowParryCounter();
                }

                if (pumpkingEffect && perfectParry)
                {
                    PumpkingsCapeCounter(damageBlocked);
                }

                Player.immuneTime = invul;
                Player.hurtCooldowns[0] = invul;
                Player.hurtCooldowns[1] = invul;
                ParryDebuffImmuneTime = invul;
                shieldCD = invul + extrashieldCD;

                if (pumpkingEffect)
                {   
                    
                    CooldownBarManager.Activate("ParryCooldown", FargoAssets.GetTexture2D("Content/Items/Accessories/Eternity", "PumpkingsCape").Value, new Color(30, 0, 60), () => 1 - shieldCD / (float)(invul + extrashieldCD), activeFunction: () => pumpkingEffect);
                }
                else if (dreadEffect)
                {
                    CooldownBarManager.Activate("ParryCooldown", FargoAssets.GetTexture2D("Content/Items/Accessories/Eternity", "DreadShell").Value, Color.DarkRed, () => 1 - shieldCD / (float)(invul + extrashieldCD), activeFunction: () => dreadEffect);
                }
                else
                {
                    CooldownBarManager.Activate("ParryCooldown", FargoAssets.GetTexture2D("Content/Items/Accessories/Enchantments", "SilverEnchant").Value, Color.Gray, () => 1 - shieldCD / (float)(invul + extrashieldCD), activeFunction: () => silverEffect);
                }

                if (perfectParry)
                {
                    if (Main.myPlayer != Player.whoAmI)
                        return;
                    ModContent.GetInstance<PerfectParryAchievement>().Condition.Complete();
                }

                foreach (int debuff in FargowiltasSouls.DebuffIDs) //immune to all debuffs
                {
                    if (!Player.HasBuff(debuff))
                        Player.buffImmune[debuff] = true;
                }
            }
        }

        public const int BASE_PARRY_WINDOW = 16;
        public const int HARD_PARRY_WINDOW = 9;
        public const int BASE_SHIELD_COOLDOWN = 100;
        public const int LONG_SHIELD_COOLDOWN = 360;

        public static int ShieldCooldown(Player player) => player.HasEffect<DreadShellEffect>() || player.HasEffect<PumpkingsCapeEffect>() ? LONG_SHIELD_COOLDOWN : BASE_SHIELD_COOLDOWN;

        void RaisedShieldEffects()
        {
            bool silverEffect = Player.HasEffect<SilverEffect>();
            bool dreadEffect = Player.HasEffect<DreadShellEffect>();
            bool lithoEffect = Player.HasEffect<LithosphericEffect>();
            bool pumpkingEffect = Player.HasEffect<PumpkingsCapeEffect>();

            if (dreadEffect) // should still apply in litho
            {
                if (!MasochistSoul)
                {
                    DreadShellVulnerabilityTimer = 120;
                    if (!silverEffect && !pumpkingEffect)
                        Player.statDefense -= 20; // counteract vanilla raised shield defense
                }
            }

            if (lithoEffect)
            {
                Player.FargoSouls().LanternGuarding = true;
            }

            if (pumpkingEffect)
            {
                Player.lifeRegen += 10;
            }

            if (shieldTimer > 0) // while parry window is up, you almost instantly freeze
            {
                Player.velocity *= 0.5f;
            }
            else // for when you keep holding shield after parry window ends
            {
                Player.velocity.X *= 0.85f;
                if (Player.velocity.Y < 0)
                    Player.velocity.Y *= 0.85f;
            }

            int cooldown = ShieldCooldown(Player);

            if (shieldCD < cooldown)
                shieldCD = cooldown;
        }

        public void UpdateShield()
        {
            bool silverEffect = Player.HasEffect<SilverEffect>();
            bool dreadEffect = Player.HasEffect<DreadShellEffect>();
            bool lithoEffect = Player.HasEffect<LithosphericEffect>();
            bool pumpkingEffect = Player.HasEffect<PumpkingsCapeEffect>();
            GuardRaised = false;
            ModKeybind key = AccessoryEffectLoader.GetKeybind<ParryEffect>(this);

            //no need when player has brand of inferno
            if ((!silverEffect && !dreadEffect && !pumpkingEffect) || key == null ||
                Player.inventory[Player.selectedItem].type == ItemID.DD2SquireDemonSword || Player.inventory[Player.selectedItem].type == ItemID.BouncingShield)
            {
                shieldTimer = 0;
                shieldHeldTime = 0;
                wasHoldingShield = false;
                return;
            }
            bool holdingKey = PlayerInput.Triggers.Current.MouseRight && Player.controlUseTile && Player.releaseUseItem && !Player.tileInteractionHappened;
            if (key.GetAssignedKeys().Any())
                holdingKey = key.Current;
            bool preventRightClick = Main.HoveringOverAnNPC || Main.SmartInteractShowingGenuine;
            if (key != null)
                preventRightClick = false;
            Player.shieldRaised = (Player.selectedItem != 58 && FargoSoulsUtil.ActuallyClickingInGameplay(Player)
                && !preventRightClick && holdingKey && Player.altFunctionUse != 2)
                || (GuardRaised && shieldTimer > 0);

            if (Player.shieldRaised)
            {
                GuardRaised = true;
                shieldHeldTime++;

                for (int i = 3; i < 8 + Player.extraAccessorySlots; i++)
                {
                    if (Player.shield == -1 && Player.armor[i].shieldSlot != -1)
                        Player.shield = Player.armor[i].shieldSlot;
                }

                if (shieldTimer > 0)
                    shieldTimer--;

                if (!wasHoldingShield)
                {
                    wasHoldingShield = true;
                    if (shieldCD == 0)
                        shieldTimer = silverEffect ? BASE_PARRY_WINDOW : HARD_PARRY_WINDOW;
                }
                else //doing this so that on the first tick, these things DO NOT run
                {
                    RaisedShieldEffects();
                }

                if (shieldTimer == 1) //parry window over
                {
                    //theoretically none of this is necessary when the dye effect indicates the parry window already
                    /*SoundEngine.PlaySound(SoundID.Item27, Player.Center);
                    List<int> dusts = [];
                    if (dreadEffect)
                        dusts.Add(DustID.LifeDrain);
                    if (pumpkingEffect)
                        dusts.Add(87);
                    if (silverEffect)
                        dusts.Add(66);

                    if (dusts.Count > 0)
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            int d = Dust.NewDust(Player.position, Player.width, Player.height, Main.rand.Next(dusts), 0, 0, 0, default, 1.5f);
                            Main.dust[d].noGravity = true;
                            Main.dust[d].velocity *= 3f;
                        }
                    }*/
                }
            }
            else
            {
                shieldTimer = 0;
                shieldHeldTime = 0;

                if (wasHoldingShield)
                {
                    wasHoldingShield = false;

                    Player.shield_parry_cooldown = 0; //prevent that annoying tick noise
                }

                if (shieldCD == 1) //cooldown over
                {
                    SoundEngine.PlaySound(SoundID.MaxMana, Player.Center); //make a sound for refresh

                    List<Color> colors = [];
                    if (lithoEffect)
                        colors.Add(Color.DarkMagenta);
                    else if (dreadEffect)
                        colors.Add(Color.DarkRed);
                    if (pumpkingEffect)
                    {
                        colors.Add(Color.OrangeRed);
                        colors.Add(new Color(30, 0, 60));
                    }
                    if (silverEffect)
                        colors.Add(Color.Silver);

                    if (colors.Count > 0)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Vector2 spawnpos = Player.Center + Main.rand.NextVector2Circular(48, 48);
                            Vector2 vel = new(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-2, -3));
                            Particle sparkle = new SmallSparkle(spawnpos, vel, Main.rand.Next(colors), Main.rand.NextFloat(1f, 1.5f), Main.rand.Next(20, 40));
                            sparkle.Spawn();
                        }
                    }
                }

                if (shieldCD > 0)
                    shieldCD--;
            }
        }
    }
}
