using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Bosses.Champions.Earth;
using FargowiltasSouls.Content.Bosses.Champions.Spirit;
using FargowiltasSouls.Content.Bosses.Champions.Terra;
using FargowiltasSouls.Content.Bosses.Champions.Timber;
using FargowiltasSouls.Content.Bosses.Champions.Will;
using FargowiltasSouls.Content.Bosses.DeviBoss;
using FargowiltasSouls.Content.Bosses.MutantBoss;
using FargowiltasSouls.Content.Bosses.VanillaEternity;
using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Items;
using FargowiltasSouls.Content.Projectiles.Accessories.Souls;
using FargowiltasSouls.Content.Projectiles.Eternity;
using FargowiltasSouls.Content.Projectiles.Eternity.Bosses.LunaticCultist;
using FargowiltasSouls.Content.Projectiles.Eternity.Bosses.MoonLord;
using FargowiltasSouls.Content.Projectiles.Eternity.Bosses.Plantera;
using FargowiltasSouls.Content.Projectiles.Eternity.Enemies.Vanilla.BloodMoon;
using FargowiltasSouls.Content.Projectiles.Weapons;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace FargowiltasSouls.Content.Projectiles
{
    public class EModeGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public bool EModeCanHurt = true;
        public bool altBehaviour;
        private readonly List<Tuple<int, float>>? medusaList = typeof(Projectile).GetField("_medusaHeadTargetList", LumUtils.UniversalBindingFlags)?.GetValue(null) as List<Tuple<int, float>>;
        private int counter;
        private bool preAICheckDone;
        private bool firstTickAICheckDone;

        public int SourceItemType = 0;
        public float WormPierceResist = 0f;

        public bool isADD2Proj = false;
        public bool Jammed = false;
        public int beingWraithReflectBy = -1;
        public int JammedRecoverTime = 0;

        public static readonly List<int> PierceResistImmuneAiStyles =
        [
            ProjAIStyleID.Yoyo,
            ProjAIStyleID.Spear,
            ProjAIStyleID.ShortSword,
            ProjAIStyleID.Drill,
            ProjAIStyleID.HeldProjectile,
            ProjAIStyleID.NightsEdge, // all fancy sword swings
            ProjAIStyleID.CursedFlameWall, // clinger staff
            ProjAIStyleID.Rainbow, // rainbow gun
            ProjAIStyleID.MechanicalPiranha,
            ProjAIStyleID.SleepyOctopod
        ];

        /// <summary>
        /// Performs common safety checks used to run specialized EMode vanilla player projectile balance changes.
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="itemType"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        private bool PerformSafetyChecks(Projectile projectile, int itemType, out Player player, string str)
        {
            player = Main.player[projectile.owner];
            return projectile.owner.IsWithinBounds(Main.maxPlayers) && SourceItemType == itemType && EmodeItemBalance.HasEmodeChange(player, SourceItemType).Contains(str);
        }

        public override void SetStaticDefaults()
        {
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.SharpTears] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.JestersArrow] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.MeteorShot] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.ShadowFlame] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.MoonlordBullet] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.WaterBolt] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.WaterStream] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.DeathSickle] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.IceSickle] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.SwordBeam] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.CultistBossFireBall] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.CultistBossFireBallClone] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.SharknadoBolt] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.BloodShot] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.HallowBossRainbowStreak] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.HallowBossLastingRainbow] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.FairyQueenLance] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.BulletDeadeye] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.JestersArrow] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.MeteorShot] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.MoonlordBullet] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.FlamesTrap] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.FlamethrowerTrap] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.GeyserTrap] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.Fireball] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.EyeBeam] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.PhantasmalBolt] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.PhantasmalEye] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.PhantasmalSphere] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.ShadowBeamHostile] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.InfernoHostileBlast] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.InfernoHostileBolt] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.RuneBlast] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.EyeLaser] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.GoldenShowerHostile] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.CursedFlameHostile] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.Skull] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.DD2ExplosiveTrapT3Explosion] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.QueenSlimeGelAttack] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.BombSkeletronPrime] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.SandnadoHostile] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.NebulaSphere] = true;

            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.RainNimbus] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.FrostBeam] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.RockGolemRock] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.SaucerDeathray] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.SaucerLaser] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.SaucerMissile] = true;
            A_SourceNPCGlobalProjectile.SourceNPCSync[ProjectileID.SaucerScrap] = true;
        }

        public override void SetDefaults(Projectile projectile)
        {
            if (!WorldSavingSystem.EternityMode)
                return;

            switch (projectile.type)
            {
                case ProjectileID.DD2PhoenixBow:
                case ProjectileID.LaserMachinegun:
                case ProjectileID.ChargedBlasterCannon:
                case ProjectileID.VortexBeater:
                case ProjectileID.Phantasm:
                    projectile.GetGlobalProjectile<AttackSpeedScalingGlobalProjectile>().UseAttackSpeedForAdditionalUpdates = true;
                    break;

                case ProjectileID.Sharknado:
                case ProjectileID.Cthulunado:
                    EModeCanHurt = false;
                    projectile.hide = true;
                    break;

                case ProjectileID.DD2BetsyFlameBreath:
                    projectile.tileCollide = false;
                    projectile.penetrate = -1;
                    break;

                case ProjectileID.SaucerLaser:
                    projectile.tileCollide = false;
                    break;

                case ProjectileID.AncientDoomProjectile:
                    projectile.scale *= 1.5f;
                    break;

                case ProjectileID.UnholyTridentHostile:
                    projectile.extraUpdates++;
                    break;

                case ProjectileID.BulletSnowman:
                    projectile.tileCollide = false;
                    projectile.timeLeft = 600;
                    break;

                case ProjectileID.CannonballHostile:
                    projectile.scale = 2f;
                    break;

                case ProjectileID.EyeLaser:
                case ProjectileID.EyeFire:
                    projectile.tileCollide = false;
                    break;

                case ProjectileID.QueenSlimeMinionBlueSpike:
                    projectile.scale *= 1.5f;
                    projectile.timeLeft = 180;
                    projectile.tileCollide = false;
                    break;

                case ProjectileID.BloodShot:
                    projectile.tileCollide = false;
                    break;

                case ProjectileID.DeerclopsRangedProjectile:
                    projectile.extraUpdates = 1;
                    break;

                case ProjectileID.FairyQueenLance: //these are here due to mp sync concerns and edge case on spawn
                case ProjectileID.HallowBossLastingRainbow:
                case ProjectileID.HallowBossRainbowStreak:
                case ProjectileID.PhantasmalSphere:
                    EModeCanHurt = false;
                    break;
                /*case ProjectileID.SuperStar:
                    if (EmodeItemBalance.HasEmodeChange(Main.player[projectile.owner], ItemID.SuperStarCannon))
                        projectile.penetrate = 7;
                    break;*/

                case ProjectileID.QueenBeeStinger:
                    projectile.ignoreWater = true;
                    break;
                default:
                    break;
            }
        }

        private static bool NonSwarmFight(Projectile projectile, params int[] types)
        {
            NPC npc = projectile.GetSourceNPC();
            return projectile.GetSourceNPC() is NPC && types.Contains(npc.type);
        }

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (!WorldSavingSystem.EternityMode)
                return;

            Projectile? sourceProj = null;

            if (projectile.owner.IsWithinBounds(Main.maxPlayers) && (projectile.friendly || FargoSoulsUtil.IsSummonDamage(projectile, false, false)))
            {
                if (source is not null)
                {
                    if (FargoSoulsUtil.IsProjSourceItemUseReal(projectile, source))
                    {
                        projectile.FargoSouls().ItemSource = true;
                    }
                    FargoSoulsUtil.GetOrigin(projectile, source, out sourceProj);
                    SourceItemType = projectile.FargoSouls().SourceItemType;

                    if (sourceProj is not null && sourceProj.FargoSouls().ItemSource && (((sourceProj.minion || sourceProj.sentry) && (ProjectileID.Sets.MinionShot[projectile.type] || ProjectileID.Sets.SentryShot[projectile.type])) || FargoSoulsGlobalProjectile.DoesNotAffectHuntressType.Contains(sourceProj.type)))
                    { // reuse this with the intention to make shots from held projectiles work with Huntress, or make minion shots count as ItemSource
                        projectile.FargoSouls().ItemSource = true;
                    }
                    projectile.FargoSouls().Homing = projectile.IsHoming(Main.player[projectile.owner], source);
                if (source is not EntitySource_ItemUse_WithAmmo && source is EntitySource_ItemUse && ContentSamples.ItemsByType[SourceItemType].IsWeaponWithDamageClass())
                    projectile.FargoSouls().IsOnHitSource = true;
                }
            }

            switch (SourceItemType)
            {
                case ItemID.ProximityMineLauncher:
                case ItemID.PiranhaGun:
                    if (PerformSafetyChecks(projectile, SourceItemType, out _, "DynamicUpdating") && projectile.FargoSouls().ItemSource)
                    {
                        projectile.ContinuouslyUpdateDamageStats = true;
                    }
                    break;

                default:
                    break;
            }

            if (source is EntitySource_Parent parent && parent.Entity is Projectile p && ProjectileID.Sets.IsADD2Turret[p.type])
                projectile.Eternity().isADD2Proj = true;

            switch (projectile.type)
            {
                case ProjectileID.ZapinatorLaser:
                    switch (SourceItemType)
                    {
                        case ItemID.ZapinatorGray:
                        case ItemID.ZapinatorOrange:
                            if (PerformSafetyChecks(projectile, SourceItemType, out _, "Zapinator"))
                                projectile.originalDamage = projectile.damage;
                            break;

                        default:
                            break;
                    }
                    break;

                case ProjectileID.PygmySpear:
                    if (PerformSafetyChecks(projectile, ItemID.PygmyStaff, out _, "PygmyStaff") && sourceProj is Projectile pygmy && pygmy.type >= ProjectileID.Pygmy && pygmy.type <= ProjectileID.Pygmy4)
                    {
                        projectile.usesLocalNPCImmunity = true;
                        projectile.localNPCHitCooldown = -1;
                        projectile.penetrate = 2;
                    }
                    break;

                case ProjectileID.GladiusStab:
                    if (PerformSafetyChecks(projectile, ItemID.Gladius, out _, "Gladius"))
                    {
                        projectile.usesLocalNPCImmunity = true;
                        projectile.localNPCHitCooldown = -1;
                    }
                    break;

                case ProjectileID.AmethystBolt:
                    if (PerformSafetyChecks(projectile, ItemID.AmethystStaff, out _, "AmethystStaff"))
                    {
                        projectile.usesLocalNPCImmunity = true;
                        projectile.localNPCHitCooldown = -1;
                        projectile.penetrate = 2;
                        projectile.velocity *= 2f;
                    }
                    break;

                case ProjectileID.EmeraldBolt:
                    if (PerformSafetyChecks(projectile, ItemID.EmeraldStaff, out _, "EmeraldStaff"))
                    {
                        projectile.position = projectile.Center;
                        int mult = 4;
                        projectile.scale = mult;
                        projectile.width = (int)(projectile.width * mult);
                        projectile.height = (int)(projectile.height * mult);
                        projectile.Center = projectile.position;
                        projectile.knockBack *= 2;
                        projectile.penetrate = 4;
                    }
                    break;

                case ProjectileID.RubyBolt:
                    if (PerformSafetyChecks(projectile, ItemID.RubyStaff, out _, "RubyStaff"))
                    {
                        projectile.penetrate = 1;
                    }
                    break;

                case ProjectileID.AmberBolt:
                    if (PerformSafetyChecks(projectile, ItemID.AmberStaff, out _, "AmberStaff"))
                    {
                        projectile.usesLocalNPCImmunity = true;
                        projectile.localNPCHitCooldown = 30;
                    }
                    break;

                case ProjectileID.SkyFracture:
                    if (PerformSafetyChecks(projectile, ItemID.SkyFracture, out _, "SkyFracture"))
                    {
                        projectile.extraUpdates += 1;
                    }
                    break;

                case ProjectileID.SporeGas:
                case ProjectileID.SporeGas2:
                case ProjectileID.SporeGas3:
                    if (PerformSafetyChecks(projectile, ItemID.SporeSac, out _, "SporeSac"))
                    {
                        projectile.usesIDStaticNPCImmunity = true;
                        projectile.idStaticNPCHitCooldown = 12;
                    }
                    break;

                case ProjectileID.WeatherPainShot:
                    if (PerformSafetyChecks(projectile, ItemID.WeatherPain, out _, "WeatherPain"))
                    {
                        projectile.idStaticNPCHitCooldown = 10;
                        projectile.penetrate = 45;
                    }
                    break;

                case ProjectileID.PossessedHatchet:
                    if (PerformSafetyChecks(projectile, ItemID.PossessedHatchet, out _, "PossessedHatchet"))
                    {
                        projectile.usesLocalNPCImmunity = true;
                        projectile.localNPCHitCooldown = 30;
                    }
                    break;

                case ProjectileID.FinalFractal: //zenith
                    if (PerformSafetyChecks(projectile, ItemID.Zenith, out _, "ZenithHitRate") && !WorldSavingSystem.DownedMutant)
                    {
                        projectile.usesLocalNPCImmunity = false;
                        projectile.localNPCHitCooldown = 0;

                        projectile.usesIDStaticNPCImmunity = true;
                        if (WorldSavingSystem.DownedAbom)
                            projectile.idStaticNPCHitCooldown = 3;
                        else
                            projectile.idStaticNPCHitCooldown = 5;
                    }
                    break;

                case ProjectileID.FallingStar:
                    if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.mutantBoss, ModContent.NPCType<MutantBoss>()))
                        projectile.active = false;
                    break;

                case ProjectileID.VampireHeal:
                    if (PerformSafetyChecks(projectile, ItemID.VampireKnives, out Player pl, "VampireKnives"))
                    {
                        //each lifesteal hits timer again when above 50% life (total, halved lifesteal rate)
                        if (pl.statLife > pl.statLifeMax2 / 3)
                            pl.lifeSteal -= projectile.ai[1];

                        //each lifesteal hits timer again when above 75% life (stacks with above, total 1/3rd lifesteal rate)
                        if (pl.statLife > pl.statLifeMax2 * 2 / 3)
                            pl.lifeSteal -= projectile.ai[1];
                    }
                    break;

                case ProjectileID.Cthulunado:
                    if (NonSwarmFight(projectile, NPCID.DukeFishron) && WorldSavingSystem.MasochistModeReal)
                    {
                        if (projectile.ai[1] == 25 || sourceProj is Projectile && sourceProj.Eternity().altBehaviour)
                            altBehaviour = true;
                    }
                    break;

                case ProjectileID.DeerclopsRangedProjectile:
                    {
                        if (projectile.light < 1f)
                            projectile.light = 1f;
                    }
                    break;
                    
                case ProjectileID.DeerclopsIceSpike: //note to future self: these are all mp compatible apparently?
                    if (sourceProj is Projectile && sourceProj.type == projectile.type == sourceProj.Eternity().altBehaviour)
                    {
                        altBehaviour = true;
                        //float diff = (projectile.Bottom.Y - sourceProj.Bottom.Y);
                        //projectile.Bottom = LumUtils.FindGroundVertical(projectile.Bottom.ToTileCoordinates()).ToWorldCoordinates() + Vector2.UnitY * diff;
                    }

                    if (WorldSavingSystem.MasochistModeReal)
                        projectile.ai[0] -= 20;

                    if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.deerBoss, NPCID.Deerclops))
                    {
                        if (Main.npc[EModeGlobalNPC.deerBoss].ai[0] == 4) //double walls
                        {
                            projectile.ai[0] -= 30;
                            if (Main.npc[EModeGlobalNPC.deerBoss].GetGlobalNPC<Deerclops>().EnteredPhase2)
                                projectile.ai[0] -= 30;
                            if (Main.npc[EModeGlobalNPC.deerBoss].GetGlobalNPC<Deerclops>().EnteredPhase3)
                                projectile.ai[0] -= 120;
                        }
                    }

                    if (projectile.GetSourceNPC() != null && projectile.GetSourceNPC() is NPC npc && npc.type == NPCID.Deerclops && sourceProj is not Projectile)
                    {
                        //projectile.Bottom = LumUtils.FindGroundVertical(projectile.Bottom.ToTileCoordinates()).ToWorldCoordinates() + Vector2.UnitY * 10 * projectile.scale;
                        altBehaviour = true;

                        //is a final spike of the attack
                        if (npc.ai[0] == 1 && npc.ai[1] == 52 || npc.ai[0] == 4 && npc.ai[1] == 70 && !npc.GetGlobalNPC<Deerclops>().DoLaserAttack)
                        {
                            bool isSingleWaveAttack = npc.ai[0] == 1;
                            bool shouldSplit = projectile.ai[1] < 1.3f;
                            if (shouldSplit && isSingleWaveAttack) //because deerclops spawns like 4 of them stacked on each other?
                            {
                                for (int i = 0; i < Main.maxProjectiles; i++)
                                {
                                    if (Main.projectile[i].active && Main.projectile[i].type == projectile.type
                                        && Main.projectile[i].scale == projectile.scale
                                        && Math.Sign(Main.projectile[i].velocity.X) == Math.Sign(projectile.velocity.X))
                                    {
                                        if (i != projectile.identity)
                                            shouldSplit = false;
                                        break;
                                    }
                                }
                            }
                            if (shouldSplit)
                            {
                                //projectile.ai[0] -= 60;
                                //projectile.netUpdate = true;

                                float ai1 = 1.3f;
                                if (npc.TryGetGlobalNPC<Deerclops>(out var dcgnc) && dcgnc.EnteredPhase2)
                                    ai1 = 1.35f; //triggers recursive ai
                                Vector2 spawnPos = projectile.Center + 200 * projectile.velocity.SafeNormalize(Vector2.Zero);
                                
                                if (FargoSoulsUtil.HostCheck)
                                {
                                    Projectile.NewProjectile(projectile.GetSource_FromThis(), spawnPos, projectile.velocity, projectile.type, projectile.damage, projectile.knockBack, projectile.owner, 0f, ai1);
                                    
                                    if (isSingleWaveAttack)
                                    {
                                        Projectile.NewProjectile(projectile.GetSource_FromThis(), spawnPos, Vector2.UnitX * Math.Sign(projectile.velocity.X) * projectile.velocity.Length(), projectile.type, projectile.damage, projectile.knockBack, projectile.owner, 0f, ai1);
                                        Projectile.NewProjectile(projectile.GetSource_FromThis(), spawnPos, new Vector2(projectile.velocity.X, -projectile.velocity.Y), projectile.type, projectile.damage, projectile.knockBack, projectile.owner, 0f, ai1);
                                    }
                                    else
                                    {
                                        Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, new Vector2(-projectile.velocity.X, projectile.velocity.Y), projectile.type, projectile.damage, projectile.knockBack, projectile.owner, 0f, ai1);
                                        if (projectile.Center.Y < npc.Center.Y)
                                        {
                                            Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, -projectile.velocity, projectile.type, projectile.damage, projectile.knockBack, projectile.owner, 0f, ai1);
                                            Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, new Vector2(projectile.velocity.X, -projectile.velocity.Y), projectile.type, projectile.damage, projectile.knockBack, projectile.owner, 0f, ai1);
                                        }
                                        else
                                        {
                                            Projectile.NewProjectile(projectile.GetSource_FromThis(), spawnPos, new Vector2(-projectile.velocity.X, projectile.velocity.Y), projectile.type, projectile.damage, projectile.knockBack, projectile.owner, 0f, ai1);
                                        }
                                    }
                                }
                                
                            }


                        }
                    }
                    break;
                    
                default:
                    break;
            }
        }

        //separate from OnSpawn for multiplayer sync
        public void OnFirstTick(Projectile projectile)
        {
            if (A_SourceNPCGlobalProjectile.NeedsSync(A_SourceNPCGlobalProjectile.SourceNPCSync, projectile.type))
            {
                NPC sourceNPC = projectile.GetSourceNPC();

                switch (projectile.type)
                {
                    case ProjectileID.SharpTears:
                    case ProjectileID.JestersArrow:
                    case ProjectileID.MeteorShot:
                    case ProjectileID.ShadowFlame:
                    case ProjectileID.MoonlordBullet:
                    case ProjectileID.WaterBolt:
                    case ProjectileID.WaterStream:
                    case ProjectileID.DeathSickle:
                    case ProjectileID.IceSickle:
                    case ProjectileID.SwordBeam:
                        if (sourceNPC is NPC && !sourceNPC.friendly && !sourceNPC.townNPC)
                        {
                            projectile.friendly = false;
                            projectile.hostile = true;
                            projectile.DamageType = DamageClass.Default;
                        }
                        break;

                    case ProjectileID.PhantasmalBolt:
                        if (NonSwarmFight(projectile, NPCID.MoonLordHand, NPCID.MoonLordHead, NPCID.MoonLordFreeEye)
                            && !(WorldSavingSystem.MasochistModeReal && Main.getGoodWorld))
                        {
                            if (FargoSoulsUtil.HostCheck)
                            {
                                for (int i = -2; i <= 2; i++)
                                {
                                    Projectile.NewProjectile(Entity.InheritSource(projectile), projectile.Center,
                                        1.5f * Vector2.Normalize(projectile.velocity).RotatedBy(Math.PI / 2 / 2 * i),
                                        ModContent.ProjectileType<PhantasmalBolt2>(), projectile.damage, 0f, Main.myPlayer);
                                }
                            }
                            projectile.Kill();
                        }
                        break;

                    case ProjectileID.SharknadoBolt:
                        if (sourceNPC is NPC && sourceNPC.type == NPCID.DukeFishron && sourceNPC.GetGlobalNPC<DukeFishron>().IsEX)
                            projectile.extraUpdates++;
                        break;

                    case ProjectileID.HallowBossRainbowStreak:
                        if (NonSwarmFight(projectile, NPCID.HallowBoss))
                        {
                            if (WorldSavingSystem.MasochistModeReal && sourceNPC.ai[0] != 8 && sourceNPC.ai[0] != 9)
                                EModeCanHurt = true;

                            if (sourceNPC.ai[0] == 12)
                                projectile.velocity *= 0.7f;
                        }
                        break;

                    case ProjectileID.BloodShot:
                        if (sourceNPC is NPC && sourceNPC.type == NPCID.BloodSquid)
                            projectile.damage /= 2;
                        break;

                    case ProjectileID.HallowBossLastingRainbow:
                        if (NonSwarmFight(projectile, NPCID.HallowBoss))
                        {
                            projectile.timeLeft += 60;
                            projectile.localAI[1] = projectile.velocity.ToRotation();

                            if (sourceNPC.ai[0] == 7 && sourceNPC.ai[1] >= 255 && sourceNPC.GetGlobalNPC<EmpressofLight>().DoParallelSwordWalls)
                            {
                                altBehaviour = true;
                            }
                            else if (sourceNPC.GetGlobalNPC<EmpressofLight>().AttackTimer == 1)
                            {
                                projectile.localAI[0] = 1f;
                            }
                        }
                        break;

                    case ProjectileID.FairyQueenLance:
                        if (NonSwarmFight(projectile, NPCID.HallowBoss) && sourceNPC.ai[0] == 7)
                        {
                            if (sourceNPC.ai[1] < 255) //vanilla attack has random variation, purely visual
                            {
                                Vector2 appearVel = Main.rand.NextFloat(MathHelper.TwoPi).ToRotationVector2();
                                appearVel *= 2f;
                                projectile.position -= appearVel * 60f;
                                projectile.velocity = appearVel;
                            }
                            else if (sourceNPC.GetGlobalNPC<EmpressofLight>().DoParallelSwordWalls)
                            {
                                altBehaviour = true;
                            }
                        }
                        break;

                    //early bird vanilla projectile damage nerfs
                    case ProjectileID.RainNimbus:
                        if (!Main.hardMode && sourceNPC is NPC && sourceNPC.type == NPCID.AngryNimbus)
                            projectile.damage /= 2;
                        break;
                    case ProjectileID.RockGolemRock:
                        if (!Main.hardMode && sourceNPC is NPC && sourceNPC.type == NPCID.RockGolem)
                            projectile.damage = (int)Math.Round(projectile.damage * 0.35);
                        break;
                    case ProjectileID.SandnadoHostile:
                        if (!Main.hardMode && sourceNPC is NPC && sourceNPC.type == NPCID.SandElemental)
                            projectile.damage /= 2;
                        break;
                    case ProjectileID.FrostBeam:
                        if (!Main.hardMode && sourceNPC is NPC && sourceNPC.type == NPCID.IceGolem)
                            projectile.damage /= 2;
                        break;
                    case ProjectileID.SaucerDeathray:
                    case ProjectileID.SaucerScrap:
                    case ProjectileID.SaucerLaser:
                    case ProjectileID.SaucerMissile:
                        if (!NPC.downedGolemBoss && sourceNPC is NPC && (sourceNPC.type == NPCID.MartianSaucer || sourceNPC.type == NPCID.MartianSaucerCannon || sourceNPC.type == NPCID.MartianSaucerCore || sourceNPC.type == NPCID.MartianSaucerTurret))
                            projectile.damage /= 2;
                        break;

                    default:
                        break;
                }
            }
        }

        public override bool? CanDamage(Projectile projectile)
        {
            if (!WorldSavingSystem.EternityMode)
                return base.CanDamage(projectile);

            if (!EModeCanHurt)
                return false;

            return base.CanDamage(projectile);
        }
        public override bool PreAI(Projectile projectile)
        {
            if (!WorldSavingSystem.EternityMode)
            {
                preAICheckDone = true;
                return base.PreAI(projectile);
            }

            if (!preAICheckDone)
            {
                preAICheckDone = true;

                OnFirstTick(projectile);
            }

            if (WormPierceResist > 0)
                WormPierceResist *= 0.99f;
            if (WormPierceResist < 0.01f)
                WormPierceResist = 0;
            
            counter++;

            //delay the very bottom piece of sharknados spawning in, also delays spawning sharkrons
            if (counter < 30 && projectile.ai[0] == 15
                && (projectile.type == ProjectileID.Sharknado || projectile.type == ProjectileID.Cthulunado)
                && projectile.ai[1] == (projectile.type == ProjectileID.Sharknado ? 15 : 24))
            {
                projectile.timeLeft++;
                return false;
            }
            switch (projectile.type)
            {
                case ProjectileID.ChlorophyteBullet:
                    if (PerformSafetyChecks(projectile, ItemID.ChlorophyteBullet, out _, "ChlorophyteBullet"))
                    {
                        // vanilla Chlorophyte Bullet AI imitation to apply more elaborate tweaks.
                        if (projectile.alpha < 170)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                float chloroDustX = projectile.position.X - projectile.velocity.X / 10f * i;
                                float chloroDustY = projectile.position.Y - projectile.velocity.Y / 10f * i;
                                int chloroDust = Dust.NewDust(new Vector2(chloroDustX, chloroDustY), 1, 1, DustID.CursedTorch);
                                Main.dust[chloroDust].alpha = projectile.alpha;
                                Main.dust[chloroDust].position.X = chloroDustX;
                                Main.dust[chloroDust].position.Y = chloroDustY;
                                Main.dust[chloroDust].velocity *= 0f;
                                Main.dust[chloroDust].noGravity = true;
                            }
                        }
                        float velocityLength = projectile.velocity.Length();
                        float localAI = projectile.localAI[0];
                        if (localAI == 0f)
                        {
                            projectile.localAI[0] = velocityLength;
                            localAI = velocityLength;
                        }
                        if (projectile.alpha > 0)
                        {
                            projectile.alpha -= 25;
                        }
                        if (projectile.alpha < 0)
                        {
                            projectile.alpha = 0;
                        }
                        float projCenterX = projectile.Center.X;
                        float projCenterY = projectile.Center.Y;
                        float homingRadius = 22500f; // square of 150f, original 300f. Change this to tweak initial homing range
                        bool homingCheck = false;
                        int target = 0;
                        if (projectile.ai[1] == 0f)
                        {
                            for (int npc = 0; npc < 200; npc++)
                            {
                                NPC nPC = Main.npc[npc];
                                if (nPC.CanBeChasedBy(projectile))
                                {
                                    float targetDistance = Vector2.DistanceSquared(projectile.Center, nPC.Center);
                                    if (targetDistance < homingRadius && Collision.CanHit(projectile.Center, 1, 1, nPC.position, nPC.width, nPC.height))
                                    {
                                        homingRadius = targetDistance;
                                        projCenterX = nPC.Center.X;
                                        projCenterY = nPC.Center.Y;
                                        homingCheck = true;
                                        target = npc;
                                    }
                                }
                            }
                            if (homingCheck)
                            {
                                projectile.ai[1] = target + 1;
                            }
                            homingCheck = false;
                        }
                        if (projectile.ai[1] > 0f)
                        {
                            NPC nPC = Main.npc[(int)(projectile.ai[1] - 1f)];
                            if (nPC.CanBeChasedBy(projectile))
                            {
                                float targetDistance = Vector2.DistanceSquared(projectile.Center, nPC.Center);
                                const float MaxChaseDistance = 1000000f; // square of 1000f. Change this to tweak how far it can chase a found target
                                if (targetDistance < MaxChaseDistance)
                                {
                                    homingCheck = true;
                                    projCenterX = nPC.Center.X;
                                    projCenterY = nPC.Center.Y;
                                }
                            }
                            else
                            {
                                projectile.ai[1] = 0f;
                            }
                        }
                        if (!projectile.friendly)
                        {
                            homingCheck = false;
                        }
                        if (homingCheck) // this section has the homing strength
                        {
                            float velocityLength2 = localAI;
                            float projDistanceX = projCenterX - projectile.Center.X;
                            float projDistanceY = projCenterY - projectile.Center.Y;
                            float targetDistance = Vector2.Distance(projectile.Center, Main.npc[(int)(projectile.ai[1] - 1f)].Center);
                            targetDistance = velocityLength2 / targetDistance;
                            projDistanceX *= targetDistance;
                            projDistanceY *= targetDistance;
                            float multiplier = 8f; // markiplier
                            projectile.velocity.X = (projectile.velocity.X * (multiplier - 1f) + projDistanceX) / multiplier;
                            projectile.velocity.Y = (projectile.velocity.Y * (multiplier - 1f) + projDistanceY) / multiplier;
                        }
                        projectile.rotation = (float)Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + 1.57f;
                        return false;
                    }
                    break;
                case ProjectileID.AmberBolt:
                    if (PerformSafetyChecks(projectile, ItemID.AmberStaff, out Player player, "AmberStaff") && counter > 30 && counter < 60)
                    {
                        projectile.velocity += projectile.DirectionTo(player.Center) * 0.9f;
                    }
                    break;
                case ProjectileID.RubyBolt:
                    if (PerformSafetyChecks(projectile, ItemID.RubyStaff, out _, "RubyStaff") && counter == 20)
                    {
                        var ps = FargoSoulsGlobalProjectile.SplitProj(projectile, 2, MathHelper.PiOver2 * 0.12f, 0.66f);
                        foreach (Projectile p in ps)
                            p.Eternity().counter = counter + 1;
                        projectile.active = false;
                    }
                    break;
                case ProjectileID.SolarWhipSwordExplosion:
                    if (PerformSafetyChecks(projectile, ItemID.SolarEruption, out _, "MeleeDamageBugFix"))
                        projectile.DamageType = DamageClass.Melee;
                    break;
                case ProjectileID.DaybreakExplosion:
                    if (PerformSafetyChecks(projectile, ItemID.DayBreak, out _, "MeleeDamageBugFix"))
                        projectile.DamageType = DamageClass.Melee;
                    break;

                case ProjectileID.BookOfSkullsSkull:
                    if (projectile.owner == Main.myPlayer && PerformSafetyChecks(projectile, ItemID.BookofSkulls, out _, "BookofSkulls"))
                    {
                        bool Tracking = projectile.ai[1] > 0;
                        projectile.tileCollide = !Tracking;
                        projectile.netUpdate = true;
                    }
                    break;

                default:
                    break;

            }

            // OOA Sentry Jamming
            if (Jammed)
            {
                JammedRecoverTime = 90;
                if (Main.rand.NextBool(4))
                {
                    float rot = Main.rand.NextFloat(0, MathHelper.TwoPi);
                    new ElectricSpark(projectile.Center, 5 * Vector2.UnitX.RotatedBy(rot), Color.Purple, 1f, 25).Spawn();
                }
            }
            if (JammedRecoverTime > 0)
            {
                JammedRecoverTime--;
                if (JammedRecoverTime % 10 == 0)
                {
                    float rot = Main.rand.NextFloat(0, MathHelper.TwoPi);
                    new ElectricSpark(projectile.Center, 5 * Vector2.UnitX.RotatedBy(rot), Color.Purple, 1f, 25).Spawn();
                }
                return false;
            }

            return base.PreAI(projectile);
        }

        public override void AI(Projectile projectile)
        {
            if (!WorldSavingSystem.EternityMode)
                return;

            NPC sourceNPC = projectile.GetSourceNPC();

            switch (projectile.type)
            {
                case ProjectileID.ZapinatorLaser:
                    if (projectile.owner.IsWithinBounds(Main.maxPlayers))
                        switch (SourceItemType)
                        {
                            case ItemID.ZapinatorGray:
                            case ItemID.ZapinatorOrange:
                                if (PerformSafetyChecks(projectile, SourceItemType, out _, "Zapinator") && projectile.damage > projectile.originalDamage)
                                    projectile.damage = projectile.originalDamage;
                                break;

                            default:
                                break;
                        }
                    break;

                case ProjectileID.InsanityShadowHostile:

                    if (Main.player[projectile.owner].ownedProjectileCounts[projectile.type] >= 4)
                    {
                        projectile.extraUpdates = 1;
                        projectile.position += projectile.velocity * 0.5f;
                        EModeCanHurt = true;
                        counter = -600;
                    }
                    else if (!(WorldSavingSystem.MasochistModeReal && Main.getGoodWorld))
                    {
                        EModeCanHurt = false;
                        projectile.position -= projectile.velocity;
                        projectile.ai[0]--;
                        projectile.alpha = 255;

                        if (counter > 30 && Main.player[projectile.owner].ownedProjectileCounts[projectile.type] <= 1)
                            projectile.timeLeft = 0;
                    }
                    break;

                case ProjectileID.DeerclopsIceSpike:
                    if (altBehaviour)
                    {
                        if (projectile.ai[0] < -40)
                            projectile.ai[0] = -40;
                    }
                    if (counter == 2f && projectile.hostile && projectile.ai[1] > 1.3f) //only larger spikes
                    {
                        float ai1 = 1.3f;
                        if (projectile.ai[1] > 1.35f)
                            ai1 = 1.35f;

                        for (int i = -1; i <= 1; i++) //recursive fractal spread
                        {
                            Vector2 baseVel = Vector2.Lerp(projectile.velocity, Vector2.UnitX * projectile.velocity.Length() * Math.Sign(projectile.velocity.X), 0.75f);
                            if (FargoSoulsUtil.HostCheck)
                            {
                                Projectile.NewProjectile(Entity.InheritSource(projectile), projectile.Center + 200f * Vector2.Normalize(projectile.velocity), baseVel.RotatedBy(MathHelper.ToRadians(30) * i), projectile.type, projectile.damage, projectile.knockBack, projectile.owner, 0f, ai1);
                            }
                        }
                    }
                    break;

                case ProjectileID.BloodShot:
                case ProjectileID.BloodNautilusTears:
                case ProjectileID.BloodNautilusShot:
                    if (!Collision.SolidTiles(projectile.Center, 0, 0))
                    {
                        Lighting.AddLight(projectile.Center, TorchID.Crimson);

                        if (counter > 180)
                            projectile.tileCollide = true;
                    }
                    break;

                case ProjectileID.HallowBossLastingRainbow:
                    if (!NonSwarmFight(projectile, NPCID.HallowBoss))
                    {
                        EModeCanHurt = true;
                        altBehaviour = false;
                        break;
                    }

                    if (Math.Abs(MathHelper.WrapAngle(projectile.velocity.ToRotation() - projectile.localAI[1])) > MathHelper.Pi * 0.9f)
                        EModeCanHurt = true;

                    projectile.extraUpdates = EModeCanHurt ? 1 : 3;

                    if (projectile.localAI[0] == 1f)
                        projectile.velocity = projectile.velocity.RotatedBy(-projectile.ai[0] * 2f);

                    if (altBehaviour)
                    {
                        if (EModeCanHurt)
                            projectile.velocity = projectile.velocity.RotatedBy(-projectile.ai[0] * 0.5f);
                    }
                    break;

                case ProjectileID.HallowBossRainbowStreak:
                    if (!EModeCanHurt)
                        EModeCanHurt = projectile.timeLeft < 100;
                    break;

                case ProjectileID.FairyQueenLance:
                    EModeCanHurt = projectile.localAI[0] > 60;
                    if (altBehaviour)
                    {
                        const float slowdown = 0.33f;

                        if (!EModeCanHurt)
                        {
                            counter = 0;
                            projectile.timeLeft++;
                            projectile.localAI[0] -= slowdown;
                        }

                        projectile.position -= projectile.velocity * slowdown * Utils.Clamp((float)Math.Sqrt(1f - counter / 60f), 0f, 1f);
                    }
                    else if (NonSwarmFight(projectile, NPCID.HallowBoss))
                    {
                        if (sourceNPC.ai[0] == 7 && sourceNPC.ai[1] < 255) //phase 2 exclusive angled walls attack
                        {
                            if (sourceNPC.HasValidTarget)
                            {
                                float modifier = WorldSavingSystem.MasochistModeReal ? 0.6f : 0.4f;
                                projectile.position += modifier * (Main.player[sourceNPC.target].position - Main.player[sourceNPC.target].oldPosition);
                            }
                        }
                        else if (sourceNPC.ai[0] == 6 && sourceNPC.ai[1] > 60) //the massive aoe sword spam during sun beams
                        {
                            projectile.position += sourceNPC.position - sourceNPC.oldPosition;
                        }
                    }
                    break;

                case ProjectileID.FairyQueenSunDance:
                    if (true)
                    {
                        NPC npc = FargoSoulsUtil.NPCExists(projectile.ai[1], NPCID.HallowBoss);
                        if (npc != null)
                        {
                            if (npc.ai[0] == 8 || npc.ai[0] == 9) //doing dash
                            {
                                projectile.rotation = projectile.ai[0]; //negate rotation

                                if (counter < 60) //force proj into active state faster
                                    counter += 9;
                                if (projectile.localAI[0] < 60)
                                    projectile.localAI[0] += 9;
                            }

                            if (npc.ai[0] == 1 || npc.ai[0] == 10) //while empress is moving back over player or p2 transition
                            {
                                EModeCanHurt = false;
                                counter = 0;
                                projectile.timeLeft = 0;
                            }

                            if (npc.ai[0] == 6 && npc.GetGlobalNPC<EmpressofLight>().AttackCounter % 2 == 0)
                            {
                                projectile.scale *= Utils.Clamp(npc.ai[1] / 80f, 0f, 2.5f);
                            }
                            else if (counter >= 60 && projectile.scale > 0.5f && counter % 10 == 0)
                            {
                                float offset = MathHelper.ToRadians(90) * MathHelper.Lerp(0f, 1f, counter % 50f / 50f);
                                for (int i = -1; i <= 1; i += 2)
                                {
                                    if (Math.Abs(offset) < 0.001f && i < 0)
                                        continue;

                                    if (FargoSoulsUtil.HostCheck)
                                    {
                                        float spawnOffset = 800;
                                        Projectile.NewProjectile(Entity.InheritSource(projectile), projectile.Center + projectile.rotation.ToRotationVector2() * spawnOffset, Vector2.Zero, ProjectileID.FairyQueenLance, projectile.damage, projectile.knockBack, projectile.owner, projectile.rotation + offset * i, projectile.ai[0]);
                                    }
                                }
                            }
                        }
                    }
                    break;

                case ProjectileID.QueenBeeStinger:
                    projectile.velocity.Y -= 0.1f; // negate gravity
                    break;

                case ProjectileID.BeeHive:
                    if (projectile.timeLeft > 30 && (projectile.velocity.X != 0 || projectile.velocity.Y == 0))
                        projectile.timeLeft = 30;
                    break;

                case ProjectileID.TowerDamageBolt:
                    if (!firstTickAICheckDone)
                    {
                        NPC npc = FargoSoulsUtil.NPCExists(projectile.ai[0], NPCID.LunarTowerNebula, NPCID.LunarTowerSolar, NPCID.LunarTowerStardust, NPCID.LunarTowerVortex);
                        if (npc != null)
                        {
                            //not kill, because kill decrements shield
                            if (projectile.Distance(npc.Center) > 4000)
                                projectile.active = false;
                            int p = Player.FindClosest(projectile.Center, 0, 0);
                            if (p != -1 && !(Main.player[p].active && npc.Distance(Main.player[p].Center) < 4000))
                                projectile.active = false;
                        }
                    }
                    break;

                case ProjectileID.Sharknado: //this only runs after changes in preAI() finish blocking it
                case ProjectileID.Cthulunado:
                    EModeCanHurt = true;
                    projectile.hide = false;
                    if (!FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.fishBoss, NPCID.DukeFishron))
                        projectile.timeLeft = Math.Min(120, projectile.timeLeft);
                    break;

                case ProjectileID.Fireball:
                    {
                        NPC golem = FargoSoulsUtil.NPCExists(NPC.golemBoss, NPCID.Golem);
                        if (golem != null && !golem.dontTakeDamage)
                            projectile.timeLeft = 0;
                    }
                    break;

                case ProjectileID.GeyserTrap:
                    if (!WorldSavingSystem.MasochistModeReal && sourceNPC != null && sourceNPC.type == NPCID.Golem && counter > 45)
                        projectile.Kill();
                    break;

                case ProjectileID.CultistBossFireBall:
                case ProjectileID.CultistBossFireBallClone:
                    {
                        if (NonSwarmFight(projectile, NPCID.CultistBoss))
                            projectile.position -= projectile.velocity * Math.Max(0, 1f - counter / 60f / projectile.MaxUpdates); //accel startup

                    }
                    break;

                case ProjectileID.NebulaSphere:
                    if (sourceNPC is NPC && sourceNPC.type == NPCID.CultistBoss)
                    {
                        int p = Player.FindClosest(projectile.Center, 0, 0);
                        if (p != -1 && projectile.Distance(Main.player[p].Center) > 240)
                            projectile.position += projectile.velocity;
                    }
                    break;

                case ProjectileID.EyeBeam:
                    if (NonSwarmFight(projectile, NPCID.GolemHead, NPCID.GolemHeadFree))
                    {
                        if (!firstTickAICheckDone)
                        {
                            if (WorldSavingSystem.MasochistModeReal)
                            {
                                // only run alt behavior (accelerating) if too close to player
                                if (NPC.FindFirstNPC(NPCID.GolemHeadFree) is int id && id.IsWithinBounds(Main.maxNPCs) && Main.npc[id].TypeAlive(NPCID.GolemHeadFree) && Main.npc[id].HasPlayerTarget)
                                {
                                    NPC golemHead = Main.npc[id];
                                    if (golemHead.Distance(Main.player[golemHead.target].Center) > 350)
                                        break;
                                }
                                else
                                    break;
                            }
                            altBehaviour = true;
                            projectile.velocity = projectile.velocity.SafeNormalize(Vector2.UnitY);
                            projectile.timeLeft = 180 * projectile.MaxUpdates;
                        }

                        if (altBehaviour && projectile.timeLeft % projectile.MaxUpdates == 0) //only run once per tick
                        {
                            if (++projectile.localAI[1] < 90)
                                projectile.velocity *= 1.04f;
                        }
                    }
                    break;

                case ProjectileID.JavelinHostile:
                case ProjectileID.FlamingWood:
                    projectile.position += projectile.velocity * .5f;
                    break;

                case ProjectileID.VortexAcid:
                    projectile.position += projectile.velocity * .25f;
                    break;

                case ProjectileID.NebulaEye:
                    {
                        NPC npc = FargoSoulsUtil.NPCExists(projectile.ai[1], NPCID.NebulaBrain);
                        if (npc != null)
                        {
                            if (npc.ai[0] < 45 && projectile.ai[0] >= 180 - 5)
                                projectile.ai[0] -= 180; //prevent firing shortly after teleport
                        }
                    }
                    break;

                case ProjectileID.CultistRitual:
                    if (true)
                    {
                        NPC npc = FargoSoulsUtil.NPCExists(projectile.ai[1], NPCID.CultistBoss);
                        if (npc != null && npc.ai[3] == -1f && npc.ai[0] == 5)
                        {
                            projectile.Center = Main.player[npc.target].Center;
                        }

                        if (!firstTickAICheckDone) //MP sync data to server
                        {
                            if (npc != null)
                            {
                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                {
                                    LunaticCultist cultistData = npc.GetGlobalNPC<LunaticCultist>();

                                    var netMessage = Mod.GetPacket(); //sync damage contribution (which is client side) to server
                                    netMessage.Write((byte)FargowiltasSouls.PacketID.SyncCultistDamageCounterToServer);
                                    netMessage.Write((byte)npc.whoAmI);
                                    netMessage.Write(cultistData.MeleeDamageCounter);
                                    netMessage.Write(cultistData.RangedDamageCounter);
                                    netMessage.Write(cultistData.MagicDamageCounter);
                                    netMessage.Write(cultistData.MinionDamageCounter);
                                    netMessage.Send();
                                }
                                else //refresh ritual
                                {
                                    for (int i = 0; i < Main.maxProjectiles; i++)
                                    {
                                        if (Main.projectile[i].active && Main.projectile[i].ai[1] == npc.whoAmI && Main.projectile[i].type == ModContent.ProjectileType<CultistRitual>())
                                        {
                                            Main.projectile[i].Kill();
                                            break;
                                        }
                                    }

                                    Projectile.NewProjectile(npc.GetSource_FromThis(), projectile.Center, Vector2.Zero, ModContent.ProjectileType<CultistRitual>(), FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage), 0f, Main.myPlayer, 0f, npc.whoAmI);
                                    const int max = 16;
                                    const float appearRadius = 1600f - 100f;
                                    for (int i = 0; i < max; i++)
                                    {
                                        float rotation = MathHelper.TwoPi / max * i;
                                        for (int j = -1; j <= 1; j += 2)
                                        {
                                            Vector2 spawnoffset = new Vector2(appearRadius, 0).RotatedBy(rotation);
                                            Projectile.NewProjectile(npc.GetSource_FromThis(), projectile.Center + spawnoffset, j * Vector2.UnitY.RotatedBy(rotation), ModContent.ProjectileType<GlowLine>(), 0, 0f, Main.myPlayer, 18, npc.whoAmI);
                                        }
                                    }
                                }
                            }

                            for (int i = 0; i < Main.maxProjectiles; i++) //purge spectre mask bolts and homing nebula spheres
                            {
                                if (Main.projectile[i].active && (Main.projectile[i].type == ProjectileID.SpectreWrath || Main.projectile[i].type == ProjectileID.NebulaSphere))
                                    Main.projectile[i].Kill();
                            }
                        }

                        //if (Fargowiltas.Instance.MasomodeEXLoaded && projectile.ai[0] > 120f && projectile.ai[0] < 299f) projectile.ai[0] = 299f;

                        bool dunk = false;

                        if (projectile.ai[1] == -1)
                        {
                            if (counter == 5)
                                dunk = true;
                        }
                        else
                        {
                            counter = 0;
                            if (projectile.ai[0] == 299f)
                                dunk = true;
                        }

                        if (dunk) //pillar dunk
                        {
                            int cult = -1;
                            for (int i = 0; i < Main.maxNPCs; i++)
                            {
                                if (Main.npc[i].active && Main.npc[i].type == NPCID.CultistBoss && Main.npc[i].ai[2] == projectile.identity)
                                {
                                    cult = i;
                                    break;
                                }
                            }

                            if (cult != -1)
                            {
                                float ai0 = Main.rand.Next(4);

                                LunaticCultist cultistData = Main.npc[cult].GetGlobalNPC<LunaticCultist>();
                                int[] weight =
                                [
                                    cultistData.MagicDamageCounter,
                                    cultistData.MeleeDamageCounter,
                                    cultistData.RangedDamageCounter,
                                    cultistData.MinionDamageCounter,
                                ];
                                cultistData.MeleeDamageCounter = 0;
                                cultistData.RangedDamageCounter = 0;
                                cultistData.MagicDamageCounter = 0;
                                cultistData.MinionDamageCounter = 0;

                                Main.npc[cult].netUpdate = true;

                                int max = 0;
                                for (int i = 1; i < 4; i++)
                                    if (weight[max] < weight[i])
                                        max = i;
                                if (weight[max] > 0)
                                    ai0 = max;

                                if ((cultistData.EnteredPhase2 /*|| Fargowiltas.Instance.MasomodeEXLoaded*/ || WorldSavingSystem.MasochistModeReal) && FargoSoulsUtil.HostCheck && !Main.projectile.Any(p => p.active && p.hostile && p.type == ModContent.ProjectileType<CelestialPillar>()))
                                {
                                    Projectile.NewProjectile(Main.npc[cult].GetSource_FromThis(), projectile.Center, Vector2.UnitY * -10f, ModContent.ProjectileType<CelestialPillar>(),
                                        Math.Max(75, FargoSoulsUtil.ScaledProjectileDamage(Main.npc[cult].damage, 4)), 0f, Main.myPlayer, ai0);
                                }
                            }
                        }
                    }
                    break;

                case ProjectileID.MoonLeech:
                    if (projectile.ai[0] > 0f)
                    {
                        Vector2 distance = Main.player[(int)projectile.ai[1]].Center - projectile.Center - projectile.velocity;
                        if (distance != Vector2.Zero)
                            projectile.position += Vector2.Normalize(distance) * Math.Min(16f, distance.Length());
                    }
                    break;

                case ProjectileID.SandnadoHostile:
                    if (Main.hardMode && projectile.timeLeft == 1199 && NPC.CountNPCS(NPCID.SandShark) < 10 && FargoSoulsUtil.HostCheck)
                    {
                        if (!(sourceNPC is NPC && (sourceNPC.type == ModContent.NPCType<DeviBoss>() || sourceNPC.type == ModContent.NPCType<SpiritChampion>())))
                        {
                            FargoSoulsUtil.NewNPCEasy(Entity.InheritSource(projectile), projectile.Center, NPCID.SandShark,
                                velocity: new Vector2(Main.rand.NextFloat(-10, 10), Main.rand.NextFloat(-20, -10)));
                        }
                    }

                    if (sourceNPC is NPC && sourceNPC.type == ModContent.NPCType<DeviBoss>() && sourceNPC.ai[0] != 5)
                        projectile.ai[0] += 2; //despawn faster
                    break;

                case ProjectileID.PhantasmalEye:
                    if (NonSwarmFight(projectile, NPCID.MoonLordHand, NPCID.MoonLordHead, NPCID.MoonLordFreeEye))
                    {
                        if (projectile.ai[0] == 2 && counter > 60) //diving down and homing
                            projectile.velocity.Y = 9;
                        else
                            projectile.position.Y -= projectile.velocity.Y / 4;

                        float cap = WorldSavingSystem.MasochistModeReal ? 2 : 1;

                        if (projectile.velocity.X > cap)
                            projectile.velocity.X = cap;
                        else if (projectile.velocity.X < -cap)
                            projectile.velocity.X = -cap;
                    }
                    break;

                case ProjectileID.PhantasmalSphere:
                    if (!(WorldSavingSystem.MasochistModeReal && Main.getGoodWorld))
                    {
                        EModeCanHurt = projectile.alpha == 0;

                        //when from hand, nerf with telegraph and accel startup
                        if (sourceNPC is NPC && sourceNPC.type == NPCID.MoonLordHand)
                        {
                            if (projectile.ai[0] == -1) //sent to fly
                            {
                                if (++projectile.localAI[1] < 150)
                                    projectile.velocity *= 1.018f;

                                if (projectile.localAI[0] == 0 && projectile.velocity.Length() > 11) //only do this once
                                {
                                    projectile.localAI[0] = 1;
                                    projectile.velocity.Normalize();

                                    /*
                                    if (FargoSoulsUtil.HostCheck && !WorldSavingSystem.MasochistModeReal)
                                    {
                                        Projectile.NewProjectile(Entity.InheritSource(projectile), projectile.Center, projectile.velocity, ModContent.ProjectileType<PhantasmalSphereDeathray>(),
                                            0, 0f, Main.myPlayer, 0f, projectile.identity);
                                    }
                                    */

                                    projectile.netUpdate = true;
                                }
                               if (projectile.localAI[0] != 0 && projectile.velocity.Length() > 9)
                                {
                                    projectile.velocity = projectile.velocity.ClampLength(0, 9);
                                }
                            }
                        }
                    }
                    else
                    {
                        EModeCanHurt = true;
                    }
                    break;

                case ProjectileID.PhantasmalDeathray:
                    if (WorldSavingSystem.MasochistModeReal)
                    {
                        projectile.velocity = projectile.velocity.RotatedBy(projectile.ai[0] * 0.5f);
                        projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;

                        projectile.scale *= sourceNPC is NPC && sourceNPC.type == NPCID.MoonLordHead
                            ? Main.rand.NextFloat(6f, 9f)
                            : Main.rand.NextFloat(4f, 6f);

                        if (!Main.dedServ && Main.LocalPlayer.active)
                            FargoSoulsUtil.ScreenshakeRumble(4);
                    }
                    break;

                case ProjectileID.BombSkeletronPrime: //needs to be set every tick
                    if (sourceNPC is NPC && sourceNPC.type == NPCID.UndeadMiner)
                        projectile.damage = FargoSoulsUtil.ScaledProjectileDamage(sourceNPC.damage, 1.25f);
                    else
                        projectile.damage = 40;
                    break;

                case ProjectileID.UnholyTridentHostile:
                    if (sourceNPC is NPC && sourceNPC.type == NPCID.RedDevil && !Main.hardMode)
                        projectile.damage = 26;
                    break;

                case ProjectileID.DD2BetsyFireball: //when spawned, also spawn a phoenix
                    break;
                    if (!firstTickAICheckDone && NonSwarmFight(projectile, NPCID.DD2Betsy))
                    {
                        bool phase2 = sourceNPC.GetGlobalNPC<Betsy>().InPhase2;
                        int max = phase2 ? 2 : 1;
                        for (int i = 0; i < max; i++)
                        {
                            Vector2 speed = Main.rand.NextFloat(8, 12) * -Vector2.UnitY.RotatedByRandom(Math.PI / 2);
                            float ai1 = phase2 ? 60 + Main.rand.Next(60) : 90 + Main.rand.Next(30);
                            if (FargoSoulsUtil.HostCheck)
                            {
                                Projectile.NewProjectile(Entity.InheritSource(projectile), projectile.Center, speed, ModContent.ProjectileType<BetsyPhoenix>(),
                                    projectile.damage, 0f, Main.myPlayer, Player.FindClosest(projectile.Center, 0, 0), ai1);
                            }
                        }
                    }
                    break;

                case ProjectileID.DD2BetsyFlameBreath:
                    break;
                    if (NonSwarmFight(projectile, NPCID.DD2Betsy))
                    {
                        bool phase2 = sourceNPC.GetGlobalNPC<Betsy>().InPhase2;

                        //add chain blasts in maso p2
                        if (phase2 && !firstTickAICheckDone && WorldSavingSystem.MasochistModeReal && FargoSoulsUtil.HostCheck)
                        {
                            Projectile.NewProjectile(
                                Entity.InheritSource(projectile),
                                projectile.Center + 100f * Vector2.Normalize(sourceNPC.velocity),
                                Vector2.Zero,
                                ModContent.ProjectileType<EarthChainBlast>(),
                                projectile.damage,
                                0f,
                                Main.myPlayer,
                                sourceNPC.velocity.ToRotation(),
                                7);
                        }

                        //add fireballs
                        if (counter > (phase2 ? 2 : 4))
                        {
                            counter = 0;

                            SoundEngine.PlaySound(SoundID.Item34, projectile.Center);

                            Vector2 projVel = projectile.velocity.RotatedBy((Main.rand.NextDouble() - 0.5) * Math.PI / 10);
                            projVel.Normalize();
                            projVel *= Main.rand.NextFloat(8f, 12f);

                            int type = ModContent.ProjectileType<BetsyHomingFireball>();
                            if (!phase2 || Main.rand.NextBool())
                            {
                                type = ModContent.ProjectileType<WillFireball>();
                                projVel *= 2f;
                                if (phase2)
                                    projVel *= 1.5f;
                            }

                            if (FargoSoulsUtil.HostCheck)
                                Projectile.NewProjectile(Entity.InheritSource(projectile), projectile.Center, projVel, type, projectile.damage, 0f, Main.myPlayer);
                        }
                    }
                    break;

                case ProjectileID.QueenSlimeGelAttack:
                    projectile.velocity.Y += 0.12f;
                    break;

                case ProjectileID.QueenSlimeMinionPinkBall:
                    break;

                default:
                    break;
            }

            firstTickAICheckDone = true;

            // buff yoyos to move faster with melee speed
            /*if (projectile.friendly && projectile.aiStyle == ProjAIStyleID.Yoyo)
            {
                Vector2 nextPos = projectile.position + projectile.velocity * Math.Max(0, Main.player[projectile.owner].GetAttackSpeed(DamageClass.Melee) - 1);
                if (!Collision.SolidCollision(nextPos, projectile.width, projectile.height))
                    projectile.position = nextPos;
            }*/
        }
        private int FadeTimer = 0;
        public static int[] FancySwings => [
            ProjectileID.Excalibur,
            ProjectileID.TrueExcalibur,
            ProjectileID.TerraBlade2,
            ProjectileID.TheHorsemansBlade
        ];
        public override void PostAI(Projectile projectile)
        {
            if (!WorldSavingSystem.EternityMode)
                return;
            if (projectile.owner.IsWithinBounds(Main.maxPlayers))
            {
                Player player = Main.player[projectile.owner];
                if (projectile.owner == player.whoAmI)
                {
                    if (FancySwings.Contains(projectile.type))
                    {
                        if (player.FargoSouls().swingDirection != player.direction)
                        {
                            projectile.ai[0] = -player.direction;
                        }

                        float rotation = -90;
                        if (projectile.ai[0] == -1 && player.direction == -1)
                        {
                            rotation = -180;
                        }
                        else if (projectile.ai[0] == -1 && player.direction == 1)
                        {
                            rotation = 0;
                        }
                        projectile.rotation = player.itemRotation + MathHelper.ToRadians(rotation);
                    }
                }
            }
            switch (projectile.type)
            {
                case ProjectileID.HallowBossLastingRainbow:
                case ProjectileID.HallowBossRainbowStreak:
                    {
                        if (projectile.hostile)
                        {
                            const int FadeTime = 20;
                            if (!EModeCanHurt)
                            {
                                if (FadeTimer < FadeTime)
                                    FadeTimer++;
                            }
                            else
                            {
                                if (FadeTimer > 0)
                                    FadeTimer--;
                            }
                            float fade = 1f - (0.5f * FadeTimer / FadeTime);
                            projectile.Opacity = fade;
                        }
                    }
                    break;

                case ProjectileID.FlowerPow:
                    if (PerformSafetyChecks(projectile, ItemID.FlowerPow, out Player pl, "FlowerPow") && pl.whoAmI == Main.myPlayer && projectile.localAI[0] > 0f && projectile.localAI[0] < 20f)
                    {
                        projectile.localAI[0] += 2f; // tripled petal firerate
                        projectile.netUpdate = true;
                    }
                    break;

                case ProjectileID.EatersBite:
                    if (PerformSafetyChecks(projectile, ItemID.ScourgeoftheCorruptor, out Player pl2, "ScourgeoftheCorruptor") && pl2.whoAmI == Main.myPlayer)
                    {
                        const float GrazeDistanceSQ = 2500f; // 50f squared
                        double velSquared = (double)projectile.velocity.LengthSquared();
                        for (int npc = 0; npc < Main.maxNPCs && velSquared != 0; npc++)
                        {
                            NPC nPC = Main.npc[npc];
                            if (nPC.CanBeChasedBy(projectile))
                            {
                                float targetDistanceSQ = FargoSoulsUtil.Distance(projectile.Hitbox, nPC.Hitbox, false); // Accounts for distance between HITBOXES, NOT Entity.Center
                                if (targetDistanceSQ < GrazeDistanceSQ && Collision.CanHit(projectile.Center, 1, 1, nPC.position, nPC.width, nPC.height))
                                {
                                    const double LifeTimeFactor = 9000;
                                    int TimeUntilSplit = (int)Math.Round(LifeTimeFactor / (velSquared * projectile.MaxUpdates));
                                    projectile.timeLeft = TimeUntilSplit; // Account for velocity and extra updates so it splits around the same range even with variable speed
                                    projectile.netUpdate = true;
                                    break;
                                }
                            }
                        }
                    }
                    break;

                case ProjectileID.Trimarang:
                    if (PerformSafetyChecks(projectile, ItemID.Trimarang, out Player pl3, "Trimarang") && pl3.whoAmI == Main.myPlayer/* && projectile.ai[0] == 1f*/)
                    {
                        projectile.extraUpdates = 1;
                        projectile.netUpdate = true;
                    }
                    break;
            }
        }
        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!WorldSavingSystem.EternityMode)
                return;

            if (SpearRework.ReworkedSpears.Contains(projectile.type) && PerformSafetyChecks(projectile, SourceItemType, out _, "SpearRework"))
            {
                modifiers.SourceDamage *= 1.5f;
            }

            switch (projectile.type)
            {
                /*case ProjectileID.CopperCoin:
                    if (SourceItemType == ItemID.CoinGun && EmodeItemBalance.HasEmodeChange(player, SourceItemType))
                    {
                        modifiers.SourceDamage *= 1.6f;
                    }
                    break;
                case ProjectileID.SilverCoin:
                    if (SourceItemType == ItemID.CoinGun && EmodeItemBalance.HasEmodeChange(player, SourceItemType))
                    {
                        modifiers.SourceDamage *= 0.9f;
                    }
                    break;
                case ProjectileID.GoldCoin:
                    if (SourceItemType == ItemID.CoinGun && EmodeItemBalance.HasEmodeChange(player, SourceItemType))
                    {
                        modifiers.SourceDamage *= 0.47f;
                    }
                    break;
                case ProjectileID.PlatinumCoin:
                    if (SourceItemType == ItemID.CoinGun && EmodeItemBalance.HasEmodeChange(player, SourceItemType))
                    {
                        modifiers.SourceDamage *= 0.275f;
                    }
                    break;*/

                case ProjectileID.OrichalcumHalberd:
                    if (PerformSafetyChecks(projectile, ItemID.OrichalcumHalberd, out _, "OrichalcumHalberdRework"))
                    {
                        modifiers.SourceDamage *= SpearRework.OrichalcumDoTDamageModifier(target.lifeRegen);
                    }
                    break;

                case ProjectileID.MedusaHeadRay:
                    if (PerformSafetyChecks(projectile, ItemID.MedusaHead, out Player pl, "MedusaHead") && pl.Alive() && medusaList?.Count > 0)
                    {
                        float maxBonus = 1.25f;                    // Medusa Head can target up to 3 enemies max, this EMode buff makes it so that
                        float bonus = maxBonus / medusaList.Count; // for every enemy target slot that's empty, the damage goes up by +0.625x,
                        modifiers.SourceDamage *= 1 + bonus;       // up to +1.25x, resulting in a 2.25x max bonus
                    }
                    break;

                case ProjectileID.DD2BetsyArrow: // Aerial Bane arrow
                    if (PerformSafetyChecks(projectile, ItemID.DD2BetsyBow, out _, "AerialBane"))
                    {
                        if (!WorldUtils.Find(projectile.Center.ToTileCoordinates(), Searches.Chain(new Searches.Down(12), new Conditions.IsSolid()), out _)) //vanilla conditions for "airborne enemy"
                        {
                            modifiers.FinalDamage *= 9.2f / 12f; //results in 1.15x after vanilla 1.5x
                        }
                    }
                    break;

                case ProjectileID.StardustDragon1:
                case ProjectileID.StardustDragon2:
                case ProjectileID.StardustDragon3:
                case ProjectileID.StardustDragon4:
                    if (PerformSafetyChecks(projectile, ItemID.StardustDragonStaff, out _, "Damage"))
                    {
                        modifiers.SourceDamage *= 0.67f;
                    }
                    break;

                case ProjectileID.MoonlordTurretLaser:
                    if (PerformSafetyChecks(projectile, ItemID.MoonlordTurretStaff, out _, "Damage"))
                    {
                        modifiers.SourceDamage *= 0.5f;
                    }
                    break;

                case ProjectileID.RainbowCrystalExplosion:
                    if (PerformSafetyChecks(projectile, ItemID.RainbowCrystalStaff, out _, "Damage"))
                    {
                        modifiers.SourceDamage *= 0.6f;
                    }
                    break;

                case ProjectileID.GolemFist:
                    if (PerformSafetyChecks(projectile, ItemID.GolemFist, out Player pl2, "GolemFist") && pl2.Alive())
                    {
                        float maxBonus = 2f;
                        float bonus = maxBonus * pl2.Distance(target.Center) / 600f;
                        bonus = MathHelper.Clamp(bonus, 0f, maxBonus * 2);
                        modifiers.SourceDamage *= 1 + bonus;
                    }
                    break;

                case ProjectileID.UFOLaser: // Xeno Staff
                    if (PerformSafetyChecks(projectile, ItemID.XenoStaff, out _, "Damage"))
                    {
                        modifiers.SourceDamage *= 0.7f;
                    }
                    break;

                default:
                    break;
            }

            switch (SourceItemType)
            {
                case ItemID.SniperRifle:
                    if (PerformSafetyChecks(projectile, SourceItemType, out Player pl, "SniperRifle") && pl.Alive() && projectile.FargoSouls().ItemSource)
                    {
                        float maxBonus = 1f;
                        float bonus = maxBonus * pl.Distance(target.Center) / 1800f;
                        bonus = MathHelper.Clamp(bonus, 0f, maxBonus);
                        modifiers.SourceDamage *= 1 + bonus;
                    }
                    break;
                default:
                    break;
            }
        }

        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            if (!WorldSavingSystem.EternityMode)
                return base.OnTileCollide(projectile, oldVelocity);

            switch (projectile.type)
            {
                case ProjectileID.SnowBallHostile:
                    projectile.active = false; //no block
                    break;

                case ProjectileID.QueenSlimeMinionPinkBall:
                case ProjectileID.QueenSlimeGelAttack:
                    projectile.timeLeft = 0;
                    break;

                case ProjectileID.ThornBall:
                    projectile.timeLeft = 0;

                    NPC plantera = FargoSoulsUtil.NPCExists(NPC.plantBoss, NPCID.Plantera);
                    if (plantera != null && FargoSoulsUtil.HostCheck)
                    {
                        Vector2 vel = 200f / 25f * projectile.SafeDirectionTo(plantera.Center);
                        Projectile.NewProjectile(plantera.GetSource_FromThis(), projectile.Center - projectile.oldVelocity, vel, ModContent.ProjectileType<DicerPlantera>(), projectile.damage, projectile.knockBack, projectile.owner, 0, 0);
                    }
                    break;

                case ProjectileID.AmethystBolt:
                    if (PerformSafetyChecks(projectile, ItemID.AmethystStaff, out _, "AmethystStaff") && projectile.penetrate > 1)
                    {
                        projectile.penetrate -= 1;
                        NPC npc = projectile.FindTargetWithinRange(450, true);
                        if (npc != null)
                            projectile.velocity = projectile.velocity.RotateTowards(projectile.DirectionTo(npc.Center).ToRotation(), 4);
                        else
                        {
                            if (projectile.velocity.X != projectile.oldVelocity.X)
                                projectile.velocity.X = -projectile.oldVelocity.X;
                            if (projectile.velocity.Y != projectile.oldVelocity.Y)
                                projectile.velocity.Y = -projectile.oldVelocity.Y;
                        }
                        return false;
                    }
                    break;

                default:
                    break;
            }

            return base.OnTileCollide(projectile, oldVelocity);
        }

        public override void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers)
        {
            if (!WorldSavingSystem.EternityMode)
                return;

            if (NPC.downedGolemBoss && projectile.type == ProjectileID.VortexLightning)
                modifiers.SourceDamage *= 2;

            if (projectile.type == ProjectileID.DeerclopsIceSpike)
            {
                target.longInvince = true;
            }
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(projectile, target, hit, damageDone);

            if (!WorldSavingSystem.EternityMode)
                return;

            switch (projectile.type)
            {
                case ProjectileID.PalladiumPike:
                    if (PerformSafetyChecks(projectile, ItemID.PalladiumPike, out Player pl, "PalladiumPikeRework"))
                    {
                        if (target.type != NPCID.TargetDummy && !target.friendly) //may add more checks here idk
                        {
                            pl.AddBuff(BuffID.RapidHealing, 60 * 5);
                            if (pl.Eternity().PalladiumHealTimer <= 0)
                            {
                                pl.FargoSouls().HealPlayer(1);
                                pl.Eternity().PalladiumHealTimer = 30;
                            }
                        }
                    }
                    break;
                case ProjectileID.CobaltNaginata:
                    if (PerformSafetyChecks(projectile, ItemID.CobaltNaginata, out Player pl2, "CobaltNaginataRework"))
                    {
                        if (projectile.ai[2] < 2) //only twice per swing
                        {
                            Projectile p = FargoSoulsUtil.NewProjectileDirectSafe(pl2.GetSource_OnHit(target), target.position + Vector2.UnitX * Main.rand.Next(target.width) + Vector2.UnitY * Main.rand.Next(target.height), Vector2.Zero, ModContent.ProjectileType<CobaltExplosion>(), (int)(hit.SourceDamage * 0.4f), 0f, Main.myPlayer);
                            if (p != null)
                                p.FargoSouls().CanSplit = false;
                            projectile.ai[2]++;
                        }
                    }
                    break;
                case ProjectileID.IceBolt: //Ice Blade projectile
                    if (PerformSafetyChecks(projectile, ItemID.IceBlade, out _, "IceBladeFrostburn"))
                    {
                        target.AddBuff(BuffID.Frostburn, 60 * 3);
                    }
                    break;
                case ProjectileID.SporeCloud:
                    if (PerformSafetyChecks(projectile, ItemID.ChlorophyteSaber, out _, "ChlorophyteSaber"))
                    {
                        projectile.velocity *= 0.25f;
                    }
                    break;
                case ProjectileID.AmethystBolt:
                    if (PerformSafetyChecks(projectile, ItemID.AmethystStaff, out _, "AmethystStaff"))
                    {
                        NPC npc = projectile.FindTargetWithinRange(450, true);
                        if (npc != null)
                            projectile.velocity = projectile.velocity.RotateTowards(projectile.DirectionTo(npc.Center).ToRotation(), 4);
                    }
                    break;
                case ProjectileID.TopazBolt:
                    if (PerformSafetyChecks(projectile, ItemID.TopazStaff, out Player pl3, "TopazStaff"))
                    {
                        if (hit.Crit)
                        {
                            Projectile p = FargoSoulsUtil.NewProjectileDirectSafe(pl3.GetSource_OnHit(target), projectile.Center, Vector2.Zero, ModContent.ProjectileType<TopazBoltExplosion>(), hit.SourceDamage, 0f, Main.myPlayer);
                        }
                    }
                    break;
                case ProjectileID.PossessedHatchet:
                    if (PerformSafetyChecks(projectile, ItemID.PossessedHatchet, out _, "PossessedHatchet"))
                    {
                        NPC ricotarget = projectile.FindTargetWithinRange(640, true);
                        if (projectile.ai[2] < 4)
                        {
                            projectile.ai[2]++;
                            if (target != null)
                            {
                                projectile.ai[0] = 0; //reset vanilla bounceback state 
                                projectile.ai[1] = 0;
                                projectile.damage = (int)Math.Ceiling(projectile.damage * 0.8);

                                if (ricotarget != null && ricotarget.CanBeChasedBy()) //ricochet to other npc
                                    projectile.velocity = 12 * Vector2.UnitX.RotateTowards(projectile.DirectionTo(ricotarget.Center).ToRotation(), 4);
                                else if (target.CanBeChasedBy()) //if no other npcs, home on original target, deteriorates bounce count and dmg further
                                {
                                    projectile.ai[2] += 3;
                                    projectile.damage = (int)Math.Ceiling(projectile.damage * 0.625); // 0.5x after original mult
                                    projectile.velocity = 12 * Vector2.UnitX.RotatedByRandom(MathHelper.Pi * 10);
                                }
                            }
                        }
                    }
                    break;
                case ProjectileID.ApprenticeStaffT3Shot:
                    if (PerformSafetyChecks(projectile, ItemID.ApprenticeStaffT3, out _, "BetsysCurse"))
                    {
                        for (int i = 0; i < NPC.maxBuffs; i++)
                        {
                            if (target.buffType[i] == BuffID.BetsysCurse && target.buffTime[i] == 600)
                            {
                                if (projectile.FargoSouls().ApprenticeSupportProjectile)
                                    target.buffTime[i] = 30;
                                else target.buffTime[i] = 300;
                            }
                        }
                    }
                    break;

                case ProjectileID.Flamelash:
                case ProjectileID.RainbowRodBullet:
                    if ((SourceItemType == ItemID.Flamelash || SourceItemType == ItemID.RainbowRod) && PerformSafetyChecks(projectile, SourceItemType, out _, "AntiSpam"))
                    {
                        if (projectile.ai[0] >= 0 && projectile.penetrate > 1)
                            projectile.ResetLocalNPCHitImmunity();
                    }
                    break;

                default:
                    break;
            }
        }

        public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info)
        {
            if (!WorldSavingSystem.EternityMode)
                return;

            NPC sourceNPC = projectile.GetSourceNPC();

            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.moonBoss, NPCID.MoonLordCore)
                && sourceNPC is NPC
                && sourceNPC.TryGetGlobalNPC(out MoonLordBodyPart _))
            {
                target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 180);
            }

            if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.cultBoss, NPCID.CultistBoss)
                && target.Distance(Main.npc[EModeGlobalNPC.cultBoss].Center) < 2400)
            {
                target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 180);
            }

            //if (sourceNPC is NPC && sourceNPC.ModNPC is MutantBoss)
            //    target.AddBuff(ModContent.BuffType<MutantFang>(), 180);

            switch (projectile.type)
            {
                case ProjectileID.DD2ExplosiveTrapT3Explosion:
                    if (sourceNPC is NPC && sourceNPC.type == ModContent.NPCType<TimberChampion>())
                        target.AddBuff(ModContent.BuffType<DefenselessBuff>(), 300);
                    break;

                case ProjectileID.InsanityShadowHostile:
                    if (WorldSavingSystem.MasochistModeReal && Main.getGoodWorld)
                        Deerclops.SpawnFreezeHands(projectile, target);
                    goto case ProjectileID.DeerclopsIceSpike;
                case ProjectileID.DeerclopsIceSpike:
                case ProjectileID.DeerclopsRangedProjectile:
                    target.AddBuff(BuffID.Frostburn, 90);
                    //target.AddBuff(BuffID.BrokenArmor, 90);
                    if (WorldSavingSystem.MasochistModeReal)
                        target.AddBuff(ModContent.BuffType<MarkedforDeathBuff>(), 900);
                    break;

                case ProjectileID.BloodShot:
                case ProjectileID.BloodNautilusTears:
                case ProjectileID.BloodNautilusShot:
                case ProjectileID.SharpTears:
                    target.AddBuff(ModContent.BuffType<AnticoagulationBuff>(), 600);
                    break;

                case ProjectileID.FairyQueenLance:
                    if (sourceNPC is NPC && sourceNPC.type == ModContent.NPCType<MutantBoss>())
                    {
                        target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 240);
                        if (WorldSavingSystem.EternityMode)
                            target.AddBuff(ModContent.BuffType<MutantFangBuff>(), 180);
                    }
                    goto case ProjectileID.FairyQueenSunDance;

                case ProjectileID.FairyQueenHymn:
                case ProjectileID.FairyQueenSunDance:
                case ProjectileID.HallowBossRainbowStreak:
                case ProjectileID.HallowBossLastingRainbow:
                case ProjectileID.HallowBossSplitShotCore:
                    target.AddBuff(ModContent.BuffType<SmiteBuff>(), 600);
                    break;

                case ProjectileID.RollingCactus:
                case ProjectileID.RollingCactusSpike:
                    target.AddBuff(BuffID.Poisoned, 120);
                    break;

                case ProjectileID.TorchGod:
                    target.AddBuff(BuffID.OnFire, 60);
                    break;

                case ProjectileID.Boulder:
                    target.AddBuff(ModContent.BuffType<DefenselessBuff>(), 600);
                    break;

                case ProjectileID.PoisonDartTrap:
                case ProjectileID.SpearTrap:
                case ProjectileID.SpikyBallTrap:
                    target.AddBuff(ModContent.BuffType<IvyVenomBuff>(), 360);
                    break;

                case ProjectileID.JavelinHostile:
                    target.AddBuff(ModContent.BuffType<DefenselessBuff>(), 600);
                    target.FargoSouls().AddBuffNoStack(ModContent.BuffType<StunnedBuff>(), 30);
                    break;

                case ProjectileID.DemonSickle:
                    target.AddBuff(ModContent.BuffType<ShadowflameBuff>(), 300);
                    break;

                case ProjectileID.HarpyFeather:
                    //target.AddBuff(ModContent.BuffType<ClippedWingsBuff>(), 300);
                    break;

                case ProjectileID.SandBallFalling:
                    if (projectile.velocity.X != 0) //so only antlion sand and not falling sand 
                    {
                        target.FargoSouls().AddBuffNoStack(ModContent.BuffType<StunnedBuff>(), 60);
                    }
                    break;

                case ProjectileID.Stinger:
                case ProjectileID.QueenBeeStinger:
                    //target.AddBuff(ModContent.BuffType<SwarmingBuff>(), 300);
                    break;

                case ProjectileID.Skull:
                    if (sourceNPC != null && sourceNPC.type == NPCID.SkeletronHead)
                        target.AddBuff(ModContent.BuffType<LethargicBuff>(), 120);
                    if (sourceNPC != null && sourceNPC.type == NPCID.DungeonGuardian)
                        target.AddBuff(ModContent.BuffType<MarkedforDeathBuff>(), 600);
                    break;

                case ProjectileID.EyeLaser:
                    if (sourceNPC != null && (sourceNPC.type == NPCID.WallofFlesh || sourceNPC.type == NPCID.WallofFleshEye))
                    {
                        target.AddBuff(ModContent.BuffType<ShadowflameBuff>(), 60);
                    }
                    break;

                case ProjectileID.DrManFlyFlask:
                    switch (Main.rand.Next(6))
                    {
                        case 0:
                            target.AddBuff(BuffID.Venom, 300);
                            break;
                        case 1:
                            target.AddBuff(BuffID.Confused, 300);
                            break;
                        case 2:
                            target.AddBuff(BuffID.CursedInferno, 300);
                            break;
                        case 3:
                            target.AddBuff(BuffID.OgreSpit, 300);
                            break;
                        case 4:
                            target.AddBuff(ModContent.BuffType<RottingBuff>(), 600);
                            break;
                        case 5:
                            target.AddBuff(ModContent.BuffType<DefenselessBuff>(), 600);
                            break;

                        default:
                            break;
                    }
                    target.AddBuff(BuffID.Stinky, 1200);
                    break;

                case ProjectileID.SpikedSlimeSpike:
                    target.AddBuff(BuffID.Slimed, 120);
                    break;

                case ProjectileID.QueenSlimeGelAttack:
                case ProjectileID.QueenSlimeMinionBlueSpike:
                case ProjectileID.QueenSlimeMinionPinkBall:
                case ProjectileID.QueenSlimeSmash:
                    target.AddBuff(BuffID.Slimed, 180);
                    break;

                case ProjectileID.CultistBossLightningOrb:
                case ProjectileID.CultistBossLightningOrbArc:
                    target.AddBuff(BuffID.Electrified, 300);
                    break;

                case ProjectileID.CultistBossIceMist:
                    //target.FargoSouls().AddBuffNoStack(BuffID.Frozen, 45);
                    break;

                case ProjectileID.CultistBossFireBall:
                    target.AddBuff(ModContent.BuffType<DaybrokenBuff>(), 120);
                    break;

                case ProjectileID.CultistBossFireBallClone:
                    target.AddBuff(ModContent.BuffType<ShadowflameBuff>(), 600);
                    break;

                case ProjectileID.PaladinsHammerHostile:
                    target.FargoSouls().AddBuffNoStack(ModContent.BuffType<StunnedBuff>(), 60);
                    break;

                case ProjectileID.RuneBlast:
                    target.AddBuff(ModContent.BuffType<HexedBuff>(), 240);
                    target.FargoSouls().HexedInflictor = sourceNPC.whoAmI;

                    if (sourceNPC is NPC && sourceNPC.type == NPCID.RuneWizard)
                    {
                        //target.AddBuff(ModContent.BuffType<FlamesoftheUniverseBuff>(), 60);
                        target.AddBuff(BuffID.Suffocation, 240);
                    }
                    break;

                case ProjectileID.PoisonSeedPlantera:
                    target.AddBuff(BuffID.Poisoned, 300);
                    goto case ProjectileID.SeedPlantera;
                case ProjectileID.SeedPlantera:
                case ProjectileID.ThornBall:
                    target.AddBuff(ModContent.BuffType<IvyVenomBuff>(), 240);
                    break;

                case ProjectileID.DesertDjinnCurse:
                    if (target.ZoneCorrupt)
                        target.AddBuff(BuffID.CursedInferno, 240);
                    else if (target.ZoneCrimson)
                        target.AddBuff(BuffID.Ichor, 240);
                    break;

                case ProjectileID.BrainScramblerBolt:
                    target.AddBuff(ModContent.BuffType<UnstableBuff>(), 60);
                    break;

                case ProjectileID.MartianTurretBolt:
                case ProjectileID.GigaZapperSpear:
                    target.AddBuff(ModContent.BuffType<LightningRodBuff>(), 300);
                    break;

                case ProjectileID.RayGunnerLaser:
                    target.AddBuff(BuffID.VortexDebuff, 180);
                    break;

                case ProjectileID.SaucerMissile:
                    target.AddBuff(ModContent.BuffType<ClippedWingsBuff>(), 300);
                    break;

                case ProjectileID.SaucerLaser:
                    target.AddBuff(BuffID.Electrified, 300);
                    break;

                case ProjectileID.UFOLaser:
                case ProjectileID.SaucerDeathray:
                    target.AddBuff(ModContent.BuffType<MarkedforDeathBuff>(), 360);
                    break;

                case ProjectileID.FlamingWood:
                case ProjectileID.GreekFire1:
                case ProjectileID.GreekFire2:
                case ProjectileID.GreekFire3:
                    target.AddBuff(BuffID.OnFire, 300);
                    break;

                case ProjectileID.VortexAcid:
                case ProjectileID.VortexLaser:
                    target.AddBuff(ModContent.BuffType<LightningRodBuff>(), 600);
                    //target.AddBuff(ModContent.BuffType<ClippedWingsBuff>(), 300);
                    break;

                case ProjectileID.VortexLightning:
                    target.AddBuff(BuffID.Electrified, 300);
                    break;

                case ProjectileID.LostSoulHostile:
                    target.AddBuff(ModContent.BuffType<HexedBuff>(), 240);
                    target.FargoSouls().HexedInflictor = sourceNPC.whoAmI;
                    break;

                case ProjectileID.InfernoHostileBlast:
                case ProjectileID.InfernoHostileBolt:
                    if (!(sourceNPC is NPC && sourceNPC.type == ModContent.NPCType<DeviBoss>()))
                    {
                        if (Main.rand.NextBool(5))
                            target.AddBuff(ModContent.BuffType<FusedBuff>(), 1800);
                    }
                    break;

                case ProjectileID.ShadowBeamHostile:
                    target.AddBuff(ModContent.BuffType<RottingBuff>(), 1800);
                    target.AddBuff(ModContent.BuffType<ShadowflameBuff>(), 300);
                    break;

                case ProjectileID.PhantasmalDeathray:
                    target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 360);
                    break;

                case ProjectileID.PhantasmalBolt:
                case ProjectileID.PhantasmalEye:
                case ProjectileID.PhantasmalSphere:
                    target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 240);
                    if (WorldSavingSystem.EternityMode && sourceNPC is NPC && sourceNPC.type == ModContent.NPCType<MutantBoss>())
                    {
                        target.AddBuff(ModContent.BuffType<MutantFangBuff>(), 180);
                    }
                    break;

                case ProjectileID.RocketSkeleton:
                    target.AddBuff(BuffID.Dazed, 60);
                    target.AddBuff(ModContent.BuffType<DefenselessBuff>(), 300);
                    break;

                case ProjectileID.FlamesTrap:
                case ProjectileID.FlamethrowerTrap:
                case ProjectileID.GeyserTrap:
                case ProjectileID.Fireball:
                case ProjectileID.EyeBeam:
                    target.AddBuff(BuffID.OnFire, 300);

                    if (sourceNPC is NPC)
                    {
                        if (sourceNPC.type == NPCID.Golem)
                        {
                            target.AddBuff(ModContent.BuffType<DefenselessBuff>(), 60 * 3);

                            if (Framing.GetTileSafely(sourceNPC.Center).WallType != WallID.LihzahrdBrickUnsafe)
                                target.AddBuff(ModContent.BuffType<DaybrokenBuff>(), 120);
                        }

                        if (sourceNPC.type == ModContent.NPCType<EarthChampion>())
                            target.AddBuff(ModContent.BuffType<DaybrokenBuff>(), 300);

                        if (sourceNPC.type == ModContent.NPCType<TerraChampion>())
                        {
                            target.AddBuff(BuffID.OnFire, 600);
                        }
                    }
                    break;

                case ProjectileID.DD2BetsyFireball:
                case ProjectileID.DD2BetsyFlameBreath:
                    target.AddBuff(ModContent.BuffType<DaybrokenBuff>(), 300);
                    break;

                case ProjectileID.DD2DrakinShot:
                    target.AddBuff(ModContent.BuffType<ShadowflameBuff>(), 600);
                    break;

                case ProjectileID.NebulaSphere:
                case ProjectileID.NebulaLaser:
                case ProjectileID.NebulaBolt:
                    target.AddBuff(ModContent.BuffType<BerserkedBuff>(), 120);
                    target.AddBuff(ModContent.BuffType<LethargicBuff>(), 300);
                    break;

                case ProjectileID.StardustJellyfishSmall:
                case ProjectileID.StardustSoldierLaser:
                case ProjectileID.Twinkle:
                    target.AddBuff(BuffID.Obstructed, 20);
                    target.AddBuff(BuffID.Blackout, 300);
                    break;

                case ProjectileID.Sharknado:
                case ProjectileID.Cthulunado:
                    target.AddBuff(ModContent.BuffType<DefenselessBuff>(), 60 * 5);
                    target.AddBuff(ModContent.BuffType<OceanicMaulBuff>(), 10 * 60);
                    target.FargoSouls().MaxLifeReduction += FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.fishBossEX, NPCID.DukeFishron) ? 100 : 15;
                    break;

                case ProjectileID.FlamingScythe:
                    target.AddBuff(BuffID.OnFire, 900);
                    break;

                case ProjectileID.SnowBallHostile:
                    target.FargoSouls().AddBuffNoStack(BuffID.Frozen, 45);
                    break;

                case ProjectileID.UnholyTridentHostile:
                    target.AddBuff(BuffID.Darkness, 300);
                    target.AddBuff(BuffID.Blackout, 300);
                    target.AddBuff(ModContent.BuffType<ShadowflameBuff>(), 600);
                    //target.AddBuff(ModContent.BuffType<MarkedforDeath>(), 180);
                    break;

                case ProjectileID.BombSkeletronPrime:
                    target.AddBuff(ModContent.BuffType<DefenselessBuff>(), 600);
                    if (sourceNPC is NPC && sourceNPC.type == NPCID.UndeadMiner)
                        projectile.timeLeft = 0;
                    break;

                case ProjectileID.DeathLaser:
                    if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.retiBoss, NPCID.Retinazer))
                        target.AddBuff(BuffID.Ichor, 600);
                    if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.destroyBoss, NPCID.TheDestroyer))
                        target.AddBuff(BuffID.Electrified, 60);
                    break;

                case ProjectileID.MoonlordBullet:
                    if (sourceNPC is NPC && sourceNPC.type == NPCID.VortexRifleman)
                    {
                        target.AddBuff(ModContent.BuffType<LightningRodBuff>(), 300);
                        //target.AddBuff(ModContent.BuffType<ClippedWingsBuff>(), 120);
                    }
                    break;

                case ProjectileID.IceSickle:
                    target.AddBuff(BuffID.Frostburn, 240);
                    target.AddBuff(BuffID.Chilled, 120);
                    break;

                case ProjectileID.WaterBolt:
                case ProjectileID.WaterStream:
                    target.AddBuff(BuffID.Wet, 600);
                    break;

                case ProjectileID.DeathSickle:
                    target.AddBuff(ModContent.BuffType<MarkedforDeathBuff>(), 300);
                    break;

                case ProjectileID.MeteorShot:
                    if (sourceNPC is NPC && sourceNPC.type == NPCID.TacticalSkeleton)
                    {
                        target.AddBuff(BuffID.OnFire, 360);
                    }
                    goto case ProjectileID.BulletDeadeye;
                case ProjectileID.JestersArrow:
                    if (sourceNPC is NPC && sourceNPC.type == NPCID.BigMimicHallow)
                        target.AddBuff(ModContent.BuffType<SmiteBuff>(), 600);
                    goto case ProjectileID.BulletDeadeye;
                case ProjectileID.BulletDeadeye:
                    if (sourceNPC is NPC && (sourceNPC.type == NPCID.PirateShipCannon || sourceNPC.type == NPCID.PirateDeadeye || sourceNPC.type == NPCID.PirateCrossbower))
                        target.AddBuff(ModContent.BuffType<MidasBuff>(), 600);
                    break;

                case ProjectileID.CannonballHostile:
                    target.AddBuff(ModContent.BuffType<DefenselessBuff>(), 600);
                    target.AddBuff(ModContent.BuffType<MidasBuff>(), 900);
                    break;

                case ProjectileID.AncientDoomProjectile:
                    target.AddBuff(ModContent.BuffType<MarkedforDeathBuff>(), 300);
                    target.AddBuff(ModContent.BuffType<ShadowflameBuff>(), 300);
                    break;

                case ProjectileID.ShadowFlame:
                    target.AddBuff(ModContent.BuffType<ShadowflameBuff>(), 300);
                    break;

                case ProjectileID.SandnadoHostile:
                    if (!target.HasBuff(BuffID.Dazed))
                        target.AddBuff(BuffID.Dazed, 120);
                    break;

                case ProjectileID.DD2OgreSmash:
                    target.AddBuff(BuffID.BrokenArmor, 300);
                    break;

                case ProjectileID.DD2OgreStomp:
                    target.AddBuff(BuffID.Dazed, 120);
                    break;

                case ProjectileID.DD2DarkMageBolt:
                    target.AddBuff(ModContent.BuffType<HexedBuff>(), 240);
                    target.FargoSouls().HexedInflictor = sourceNPC.whoAmI;
                    break;

                case ProjectileID.IceSpike:
                    //target.AddBuff(BuffID.Slimed, 120);
                    break;

                case ProjectileID.JungleSpike:
                    //target.AddBuff(BuffID.Slimed, 120);
                    target.AddBuff(ModContent.BuffType<InfestedBuff>(), 300);
                    break;

                default:
                    break;
            }
        }
        public override bool PreKill(Projectile projectile, int timeLeft)
        {
            if (!WorldSavingSystem.EternityMode)
                return base.PreKill(projectile, timeLeft);

            return base.PreKill(projectile, timeLeft);
        }

        public override void OnKill(Projectile projectile, int timeLeft)
        {
            if (!WorldSavingSystem.EternityMode)
                return;

            switch (projectile.type)
            {
                case ProjectileID.BloodNautilusTears:
                case ProjectileID.BloodNautilusShot:
                    if (FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(Entity.InheritSource(projectile), projectile.Center, Vector2.Zero, ModContent.ProjectileType<BloodFountain>(), projectile.damage, 0f, Main.myPlayer, Main.rand.Next(16, 48));
                    break;

                default:
                    break;
            }
        }

        public override Color? GetAlpha(Projectile projectile, Color lightColor)
        {
            if (!WorldSavingSystem.EternityMode)
                return base.GetAlpha(projectile, lightColor);

            if (projectile.type == ProjectileID.PoisonSeedPlantera || projectile.type == ProjectileID.SeedPlantera)
            {
                if (counter % 8 < 4)
                    return new Color(255, 255, 255, 0);
                return lightColor;
            }

            return base.GetAlpha(projectile, lightColor);
        }
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (!WorldSavingSystem.EternityMode)
                return base.PreDraw(projectile, ref lightColor);
            switch (projectile.type)
            {
                case ProjectileID.PoisonSeedPlantera:
                case ProjectileID.SeedPlantera:
                    projectile.Opacity = 1f;
                    FargoSoulsUtil.GenericProjectileDraw(projectile, lightColor);
                    break;

                case ProjectileID.QueenBeeStinger:
                    if (FargoSoulsUtil.BossIsAlive(ref EModeGlobalNPC.beeBoss, NPCID.QueenBee) && Main.npc[EModeGlobalNPC.beeBoss] is NPC n && n.TryGetGlobalNPC(out QueenBee qb) && qb.RunEmodeAI)
                    {
                        FargoSoulsUtil.ProjectileWithGlowDraw(projectile, lightColor, glowColor: Color.Goldenrod, glowRadius: 2f);
                        return false;
                    }
                    break;
            }
                
            if (JammedRecoverTime > 0)
                lightColor = Color.Lerp(lightColor, Color.Purple, JammedRecoverTime / 90f);
            return base.PreDraw(projectile, ref lightColor);
        }

        public static bool CanBeAbsorbed(Projectile p)
        {
            if (!WorldSavingSystem.EternityMode)
                return false;
            return p.CanBeReflected() && p.FargoSouls().DeletionImmuneRank == 0 && !p.FargoSouls().IsOnHitSource && !FargoSoulsUtil.IsSummonDamage(p, false);
        }
    }
}
