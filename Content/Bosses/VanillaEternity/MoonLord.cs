using FargowiltasSouls.Common.Utilities;
using FargowiltasSouls.Content.Bosses.MutantBoss;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Content.Projectiles.Deathrays;
using FargowiltasSouls.Content.Projectiles.Eternity;
using FargowiltasSouls.Content.Projectiles.Eternity.Bosses;
using FargowiltasSouls.Content.Projectiles.Eternity.Bosses.MoonLord;
using FargowiltasSouls.Core.Globals;
using FargowiltasSouls.Core.NPCMatching;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace FargowiltasSouls.Content.Bosses.VanillaEternity
{
    public abstract class MoonLord : EModeNPCBehaviour
    {
        public abstract ClassState GetVulnerabilityState(NPC npc);
        public enum ClassState
        {
            Melee,
            Ranged,
            Magic,
            Summon,
            All
        }
        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);
            npc.lifeMax = (int)(MathF.Round(npc.lifeMax * 1.6f));
        }
        public override void OnFirstTick(NPC npc)
        {
            base.OnFirstTick(npc);

            npc.buffImmune[ModContent.BuffType<LethargicBuff>()] = true;
            npc.buffImmune[BuffID.Suffocation] = true;
        }
        public bool IsItemValid(NPC npc, Player player, Item item)
        {
            if (player.FargoSouls().GravityGlobeEXItem != null)
                return true;
            ClassState masoStateML = GetVulnerabilityState(npc);
            if (item.CountsAsClass(DamageClass.Melee) && masoStateML > ClassState.Melee && masoStateML < ClassState.All)
                return false;
            return true;
        }
        public bool IsProjectileValid(NPC npc, Projectile projectile)
        {
            if (Main.player[projectile.owner].FargoSouls().GravityGlobeEXItem != null)
                return true;
            switch (GetVulnerabilityState(npc))
            {
                case ClassState.Melee: if (!projectile.CountsAsClass(DamageClass.Melee)) return false; break;
                case ClassState.Ranged: if (!projectile.CountsAsClass(DamageClass.Ranged)) return false; break;
                case ClassState.Magic: if (!projectile.CountsAsClass(DamageClass.Magic)) return false; break;
                case ClassState.Summon: if (!FargoSoulsUtil.IsSummonDamage(projectile)) return false; break;
                default: break;
            }
            return true;
        }
        public override bool? CanBeHitByItem(NPC npc, Player player, Item item)
        {
            if (Main.getGoodWorld)
            {
                if (!IsItemValid(npc, player, item)) 
                    return false;
            }
            return base.CanBeHitByItem(npc, player, item);
        }
        public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
        {
            if (Main.getGoodWorld)
            {
                if (!IsProjectileValid(npc, projectile))
                    return false;
            }
            return base.CanBeHitByProjectile(npc, projectile);
        }
        public override void SafeModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            bool valid = IsItemValid(npc, player, item);
            if (Main.getGoodWorld && !valid)
                modifiers.Null();
            if (valid)
                modifiers.FinalDamage *= 1.75f;
        }
        public override void SafeModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            bool valid = IsProjectileValid(npc, projectile);
            if (Main.getGoodWorld && !valid)
                modifiers.Null();
            if (valid)
                modifiers.FinalDamage *= 1.75f;

            if (FargoSoulsUtil.IsSummonDamage(projectile) && projectile.FargoSouls().ItemSource && (GetVulnerabilityState(npc) != ClassState.Summon))
            {
                if (Main.player[projectile.owner].HeldItem.DamageType.CountsAsClass(DamageClass.Summon))
                    modifiers.FinalDamage *= 0.5f;
            }
        }

        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            if (WorldSavingSystem.EternityMode)
                return false;
            return base.CanHitPlayer(npc, target, ref cooldownSlot);
        }
        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadNPCSprite(recolor, npc.type);
        }
    }

    public class MoonLordCore : MoonLord
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.MoonLordCore);

        public override ClassState GetVulnerabilityState(NPC npc) => VulnerabilityState;

        public ClassState VulnerabilityState;
        public int AttackMemory;

        public float VulnerabilityTimer;
        public float AttackTimer;

        public bool EnteredPhase2;
        public bool SpawnedRituals;

        public bool DroppedSummon;
        public int SkyTimer;



        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write7BitEncodedInt((byte)VulnerabilityState);
            binaryWriter.Write7BitEncodedInt(AttackMemory);
            binaryWriter.Write(VulnerabilityTimer);
            binaryWriter.Write(AttackTimer);
            bitWriter.WriteBit(EnteredPhase2);
            bitWriter.WriteBit(SpawnedRituals);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            VulnerabilityState = (ClassState)binaryReader.Read7BitEncodedInt();
            AttackMemory = binaryReader.Read7BitEncodedInt();
            VulnerabilityTimer = binaryReader.ReadSingle();
            AttackTimer = binaryReader.ReadSingle();
            EnteredPhase2 = bitReader.ReadBit();
            SpawnedRituals = bitReader.ReadBit();
        }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

            npc.lifeMax *= 2;
        }
        public void ShowClassDamage(NPC npc)
        {
            string damageClass = VulnerabilityState switch
            {
                ClassState.Melee => "Melee",
                ClassState.Ranged => "Ranged",
                ClassState.Magic => "Magic",
                ClassState.Summon => "Summon",
                _ => "All"
            };
            Color color = VulnerabilityState switch
            {
                ClassState.Melee => Color.Yellow,
                ClassState.Ranged => Color.LightCyan,
                ClassState.Magic => Color.Magenta,
                ClassState.Summon => Color.Cyan,
                _ => Color.White
            };
            string path = "Mods.FargowiltasSouls.Buffs.PoweroftheCosmosBuff.";
            string classText = Language.GetTextValue(path + damageClass);
            foreach (Player player in Main.ActivePlayers)
            {
                CombatText.NewText(player.Hitbox, color, text: Language.GetTextValue(path + "CombatText", classText), dramatic: true);
            }
    }
        public override bool SafePreAI(NPC npc)
        {
            bool result = base.SafePreAI(npc);

            EModeGlobalNPC.moonBoss = npc.whoAmI;

            if (!SpawnedRituals)
            {
                SpawnedRituals = true;
                VulnerabilityState = ClassState.Melee;
                ShowClassDamage(npc);
                if (FargoSoulsUtil.HostCheck)
                {
                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<LunarRitual>(), 25, 0f, Main.myPlayer, 0f, npc.whoAmI);
                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<FragmentRitual>(), 0, 0f, Main.myPlayer, 0f, npc.whoAmI);
                }
            }

            if (Main.LocalPlayer.active && !Main.LocalPlayer.dead && !Main.LocalPlayer.ghost)
                Main.LocalPlayer.AddBuff(ModContent.BuffType<PoweroftheCosmosBuff>(), 2);

            if (!(WorldSavingSystem.MasochistModeReal && Main.getGoodWorld))
                npc.position -= npc.velocity * 2f / 3f; //SLOW DOWN

            if (npc.dontTakeDamage)
            {
                if (AttackTimer == 370 && FargoSoulsUtil.HostCheck)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        NPC bodyPart = Main.npc[(int)npc.localAI[i]];
                        if (bodyPart.active)
                            Projectile.NewProjectile(npc.GetSource_FromThis(), bodyPart.Center, Vector2.Zero, ModContent.ProjectileType<GlowRing>(), 0, 0f, Main.myPlayer, bodyPart.whoAmI, bodyPart.type);
                    }
                }

                if (AttackTimer > 400)
                {
                    AttackTimer = 0;
                    npc.netUpdate = true;
                    NetSync(npc);

                    switch (VulnerabilityState)
                    {
                        case ClassState.Melee: //melee
                            for (int i = 0; i < 3; i++)
                            {
                                NPC bodyPart = Main.npc[(int)npc.localAI[i]];

                                if (bodyPart.active)
                                {
                                    int damage = 30;
                                    for (int j = -2; j <= 2; j++)
                                    {
                                        if (FargoSoulsUtil.HostCheck)
                                        {
                                            Projectile.NewProjectile(npc.GetSource_FromThis(), bodyPart.Center,
                                                6f * bodyPart.DirectionFrom(Main.player[npc.target].Center).RotatedBy(Math.PI / 2 / 4 * j),
                                                ModContent.ProjectileType<MoonLordFireball>(), damage, 0f, Main.myPlayer, 20, 20 + 60);
                                        }
                                    }
                                }
                            }
                            break;

                        case ClassState.Ranged: //ranged
                            for (int j = 0; j < 6; j++)
                            {
                                Vector2 spawn = Main.player[npc.target].Center + 500 * npc.DirectionFrom(Main.player[npc.target].Center).RotatedBy(MathHelper.TwoPi / 6 * (j + 0.5f));
                                if (FargoSoulsUtil.HostCheck)
                                    Projectile.NewProjectile(npc.GetSource_FromThis(), spawn, Vector2.Zero, ModContent.ProjectileType<LightningVortexHostile>(), 30, 0f, Main.myPlayer, 1, Main.player[npc.target].DirectionFrom(spawn).ToRotation());
                            }
                            break;

                        case ClassState.Magic: //magic
                            for (int i = 0; i < 3; i++)
                            {
                                NPC bodyPart = Main.npc[(int)npc.localAI[i]];

                                if (bodyPart.active &&
                                    (i == 2 && bodyPart.type == NPCID.MoonLordHead ||
                                    bodyPart.type == NPCID.MoonLordHand))
                                {
                                    int damage = 35;
                                    const int max = 6;
                                    for (int j = 0; j < max; j++)
                                    {
                                        if (FargoSoulsUtil.HostCheck)
                                        {
                                            int p = Projectile.NewProjectile(npc.GetSource_FromThis(), bodyPart.Center,
                                              2.5f * bodyPart.DirectionFrom(Main.player[npc.target].Center).RotatedBy(Math.PI * 2 / max * (j + 0.5)),
                                              ModContent.ProjectileType<MoonLordNebulaBlaze>(), damage, 0f, Main.myPlayer);
                                            if (p != Main.maxProjectiles)
                                                Main.projectile[p].timeLeft = 1200;
                                        }
                                    }
                                }
                            }
                            break;

                        case ClassState.Summon: //summoner
                            for (int i = 0; i < 3; i++)
                            {
                                NPC bodyPart = Main.npc[(int)npc.localAI[i]];

                                if (bodyPart.active &&
                                    (i == 2 && bodyPart.type == NPCID.MoonLordHead ||
                                    bodyPart.type == NPCID.MoonLordHand))
                                {
                                    Vector2 speed = Main.player[npc.target].Center - bodyPart.Center;
                                    speed.Normalize();
                                    speed *= 5f;
                                    for (int j = -1; j <= 1; j++)
                                    {
                                        Vector2 vel = speed.RotatedBy(MathHelper.ToRadians(15) * j);
                                        FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(), bodyPart.Center, NPCID.AncientLight, 0, 0f, (Main.rand.NextFloat() - 0.5f) * 0.3f * 6.28318548202515f / 60f, vel.X, vel.Y, velocity: vel);
                                    }
                                }
                            }
                            break;

                        default: //phantasmal eye rings
                            if (FargoSoulsUtil.HostCheck)
                            {
                                const int max = 4;
                                const int speed = 8;
                                const float rotationModifier = 0.5f;
                                int damage = 40;
                                float rotation = 2f * (float)Math.PI / max;
                                Vector2 vel = Vector2.UnitY * speed;
                                int type = ModContent.ProjectileType<MutantSphereRing>();
                                for (int i = 0; i < max; i++)
                                {
                                    vel = vel.RotatedBy(rotation);
                                    if (FargoSoulsUtil.HostCheck)
                                    {
                                        int p = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, vel, type, damage, 0f, Main.myPlayer, rotationModifier, speed);
                                        if (p != Main.maxProjectiles)
                                            Main.projectile[p].timeLeft = 1800 - (int)VulnerabilityTimer;
                                        p = Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, vel, type, damage, 0f, Main.myPlayer, -rotationModifier, speed);
                                        if (p != Main.maxProjectiles)
                                            Main.projectile[p].timeLeft = 1800 - (int)VulnerabilityTimer;
                                    }
                                }
                                SoundEngine.PlaySound(SoundID.Item84, npc.Center);
                            }
                            break;
                    }
                }
            }
            else //only when vulnerable
            {
                if (!EnteredPhase2)
                {
                    EnteredPhase2 = true;
                    AttackTimer = 0;
                    SoundEngine.PlaySound(SoundID.Roar, Main.LocalPlayer.Center);
                    npc.netUpdate = true;
                    NetSync(npc);
                }

                Player player = Main.player[npc.target];
                switch (VulnerabilityState)
                {
                    case ClassState.Melee: //melee
                        {
                            if (AttackTimer > 30)
                            {
                                AttackTimer -= 300;
                                AttackMemory = AttackMemory == 0 ? 1 : 0;

                                float handToAttackWith = npc.localAI[AttackMemory];
                                if (FargoSoulsUtil.HostCheck)
                                    Projectile.NewProjectile(npc.GetSource_FromThis(), Main.npc[(int)handToAttackWith].Center, Vector2.Zero, ModContent.ProjectileType<MoonLordSun>(), 40, 0f, Main.myPlayer, npc.whoAmI, handToAttackWith);
                            }
                        }
                        break;

                    case ClassState.Ranged: //vortex
                        {
                            if (AttackMemory == 0) //spawn the vortex
                            {
                                AttackMemory = 1;
                                for (int i = -1; i <= 1; i += 2)
                                {
                                    if (FargoSoulsUtil.HostCheck)
                                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<MoonLordVortex>(), 40, 0f, Main.myPlayer, i, npc.whoAmI);
                                }
                            }
                        }
                        break;

                    case ClassState.Magic: //nebula
                        {
                            if (AttackTimer > 30)
                            {
                                AttackTimer -= 420;

                                for (int i = 0; i < 3; i++)
                                {
                                    NPC bodyPart = Main.npc[(int)npc.localAI[i]];
                                    int damage = 35;
                                    for (int j = -2; j <= 2; j++)
                                    {
                                        if (FargoSoulsUtil.HostCheck)
                                        {
                                            Projectile.NewProjectile(npc.GetSource_FromThis(), bodyPart.Center,
                                                2.5f * bodyPart.DirectionFrom(Main.player[npc.target].Center).RotatedBy(Math.PI / 2 / 2 * (j + Main.rand.NextFloat(-0.25f, 0.25f))),
                                                ModContent.ProjectileType<MoonLordNebulaBlaze2>(), damage, 0f, Main.myPlayer, npc.whoAmI);
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    case ClassState.Summon: //stardust
                        {
                            if (AttackTimer > 360)
                            {
                                AttackTimer -= 360;
                                AttackMemory = 0;
                            }

                            float baseRotation = MathHelper.ToRadians(50);
                            if (++AttackMemory == 10)
                            {
                                if (FargoSoulsUtil.HostCheck)
                                    Projectile.NewProjectile(npc.GetSource_FromThis(), Main.npc[(int)npc.localAI[0]].Center, Main.npc[(int)npc.localAI[0]].SafeDirectionTo(player.Center), ModContent.ProjectileType<PhantasmalDeathrayMLSmall>(),
                                        40, 0f, Main.myPlayer, baseRotation * Main.rand.NextFloat(0.9f, 1.1f), npc.localAI[0]);
                            }
                            else if (AttackMemory == 20)
                            {
                                if (FargoSoulsUtil.HostCheck)
                                    Projectile.NewProjectile(npc.GetSource_FromThis(), Main.npc[(int)npc.localAI[1]].Center, Main.npc[(int)npc.localAI[2]].SafeDirectionTo(player.Center), ModContent.ProjectileType<PhantasmalDeathrayMLSmall>(),
                                        40, 0f, Main.myPlayer, -baseRotation * Main.rand.NextFloat(0.9f, 1.1f), npc.localAI[1]);
                            }
                            else if (AttackMemory == 30)
                            {
                                if (FargoSoulsUtil.HostCheck)
                                    Projectile.NewProjectile(npc.GetSource_FromThis(), Main.npc[(int)npc.localAI[2]].Center, Main.npc[(int)npc.localAI[1]].SafeDirectionTo(player.Center), ModContent.ProjectileType<PhantasmalDeathrayMLSmall>(),
                                        40, 0f, Main.myPlayer, baseRotation * Main.rand.NextFloat(0.9f, 1.1f), npc.localAI[2]);
                            }
                            else if (AttackMemory == 40)
                            {
                                if (FargoSoulsUtil.HostCheck)
                                    Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, npc.SafeDirectionTo(player.Center), ModContent.ProjectileType<PhantasmalDeathrayMLSmall>(),
                                        40, 0f, Main.myPlayer, -baseRotation * Main.rand.NextFloat(0.9f, 1.1f), npc.whoAmI);
                            }
                        }
                        break;

                    default: //any
                        {
                            if (AttackMemory == 0) //spawn the moons
                            {
                                AttackMemory = 1;

                                foreach (Projectile p in Main.projectile.Where(p => p.active && p.hostile))
                                {
                                    if (p.type == ModContent.ProjectileType<LunarRitual>() && p.ai[1] == npc.whoAmI) //find my arena
                                    {
                                        if (FargoSoulsUtil.HostCheck)
                                        {
                                            for (int i = 0; i < 4; i++)
                                            {
                                                Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, p.SafeDirectionTo(Main.player[npc.target].Center).RotatedBy(MathHelper.TwoPi / 4 * i), ModContent.ProjectileType<MoonLordMoon>(),
                                                    40, 0f, Main.myPlayer, p.identity, 1450);
                                            }
                                            for (int i = 0; i < 4; i++)
                                            {
                                                Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, p.SafeDirectionTo(Main.player[npc.target].Center).RotatedBy(MathHelper.TwoPi / 4 * (i + 0.5f)), ModContent.ProjectileType<MoonLordMoon>(),
                                                    40, 0f, Main.myPlayer, p.identity, -950);
                                            }
                                        }
                                        break;
                                    }
                                }
                            }

                            if (WorldSavingSystem.MasochistModeReal && AttackTimer > 300)
                            {
                                AttackTimer -= 540;

                                if (FargoSoulsUtil.HostCheck)
                                {
                                    const int max = 4;
                                    const int speed = 8;
                                    const float rotationModifier = 0.5f;
                                    int damage = 40;
                                    float rotation = 2f * (float)Math.PI / max;
                                    Vector2 vel = Vector2.UnitY * speed;
                                    int type = ModContent.ProjectileType<MutantSphereRing>();
                                    for (int i = 0; i < max; i++)
                                    {
                                        vel = vel.RotatedBy(rotation);
                                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, vel, type, damage, 0f, Main.myPlayer, rotationModifier, speed);
                                        Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, vel, type, damage, 0f, Main.myPlayer, -rotationModifier, speed);
                                    }
                                    SoundEngine.PlaySound(SoundID.Item84, npc.Center);
                                }
                            }
                        }
                        break;
                }
            }

            if (npc.ai[0] == 2f) //moon lord is dead
            {
                VulnerabilityState = ClassState.All;
                VulnerabilityTimer = 0;
                AttackTimer = 0;
            }
            else //moon lord isn't dead
            {
                const float maxRampup = 3;
                float lerp = (float)npc.life / npc.lifeMax;
                if (WorldSavingSystem.MasochistModeReal)
                    lerp = MathF.Pow(lerp, 1.5f);
                float increment = (int)Math.Round(MathHelper.Lerp(maxRampup, 1, lerp));
                if (increment < 2)
                {
                    increment += WorldSavingSystem.MasochistModeReal ? 0.5f : 0.25f;
                    if (increment > 2)
                        increment = 2;
                }
                    
                VulnerabilityTimer += increment;
                AttackTimer += increment;

                if (VulnerabilityTimer > 1800) //next vuln phase
                {
                    VulnerabilityState = VulnerabilityState.NextEnum();
                    ShowClassDamage(npc);

                    VulnerabilityTimer = 0;
                    AttackTimer = 0;
                    AttackMemory = 0;

                    npc.netUpdate = true;
                    NetSync(npc);

                    if (WorldSavingSystem.MasochistModeReal && Main.zenithWorld)
                    {
                        switch (VulnerabilityState)
                        {
                            case ClassState.Melee: //melee
                                for (int i = 0; i < 3; i++)
                                {
                                    NPC bodyPart = Main.npc[(int)npc.localAI[i]];

                                    if (bodyPart.active && bodyPart.type == NPCID.MoonLordHead)
                                    {
                                        for (int j = -3; j <= 3; j++)
                                        {
                                            FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(),
                                                bodyPart.Center, NPCID.SolarGoop, target: npc.target,
                                                velocity: -10f * Vector2.UnitY.RotatedBy(MathHelper.ToRadians(20 * j)));
                                        }
                                    }
                                }
                                break;

                            case ClassState.Ranged: //ranged
                                if (FargoSoulsUtil.HostCheck)
                                {
                                    Projectile.NewProjectile(
                                          npc.GetSource_FromThis(),
                                          npc.Center, Vector2.Zero,
                                          ModContent.ProjectileType<MoonLordVortexOld>(),
                                          40, 0f, Main.myPlayer, 0, npc.whoAmI);
                                }
                                break;

                            case ClassState.Magic: //magic
                                if (FargoSoulsUtil.HostCheck)
                                {
                                    //for (int i = -1; i <= 1; i++)
                                    //{
                                    int p = Projectile.NewProjectile(
                                      npc.GetSource_FromThis(),
                                      npc.Center, Vector2.Zero,
                                      ModContent.ProjectileType<GlowLine>(),
                                      0, 0f, Main.myPlayer, 17f, npc.whoAmI);
                                    if (p != Main.maxProjectiles)
                                    {
                                        //Main.projectile[p].localAI[0] = 950f * i;
                                        if (Main.netMode == NetmodeID.Server)
                                            NetMessage.SendData(MessageID.SyncProjectile, number: p);
                                    }
                                    //}
                                }
                                break;

                            case ClassState.Summon: //summon
                                for (int i = 0; i < 3; i++)
                                {
                                    NPC bodyPart = Main.npc[(int)npc.localAI[i]];

                                    if (bodyPart.active)
                                    {
                                        for (int j = -2; j <= 2; j++)
                                        {
                                            Vector2 vel = 9f * bodyPart.SafeDirectionTo(Main.player[npc.target].Center).RotatedBy(MathHelper.Pi / 5 * j);
                                            FargoSoulsUtil.NewNPCEasy(npc.GetSource_FromAI(),
                                                bodyPart.Center, NPCID.AncientLight, 0,
                                                0f,
                                                (Main.rand.NextFloat() - 0.5f) * 0.3f * 6.28318548202515f / 60f,
                                                vel.X,
                                                vel.Y,
                                                npc.target,
                                                vel);
                                        }
                                    }
                                }
                                break;

                            default:
                                break;
                        }
                    }
                }
            }

            //because 1.4 is fucking stupid and time freeze prevents custom skies from working I HATE 1.4
            if (Main.GameModeInfo.IsJourneyMode && CreativePowerManager.Instance.GetPower<CreativePowers.FreezeTime>().Enabled)
                CreativePowerManager.Instance.GetPower<CreativePowers.FreezeTime>().SetPowerInfo(false);

            if (!Main.dedServ && ++SkyTimer > 30 && NPC.FindFirstNPC(npc.type) == npc.whoAmI)
            {
                SkyTimer = 0;

                if (!SkyManager.Instance["FargowiltasSouls:MoonLordSky"].IsActive())
                    SkyManager.Instance.Activate("FargowiltasSouls:MoonLordSky");

                static void HandleScene(string name)
                {
                    if (!Filters.Scene[$"FargowiltasSouls:{name}"].IsActive())
                        Filters.Scene.Activate($"FargowiltasSouls:{name}");
                }

                switch (VulnerabilityState)
                {
                    case ClassState.Melee: HandleScene("Solar"); break;
                    case ClassState.Ranged: HandleScene("Vortex"); break;
                    case ClassState.Magic: HandleScene("Nebula"); break;
                    case ClassState.Summon: HandleScene("Stardust"); break;
                    default: break;
                }
            }

            EModeUtils.DropSummon(npc, ItemID.CelestialSigil, NPC.downedMoonlord, ref DroppedSummon, NPC.downedAncientCultist);

            return result;
        }

        public override void LoadSprites(NPC npc, bool recolor)
        {
            base.LoadSprites(npc, recolor);

            LoadBossHeadSprite(recolor, 8);
            for (int i = 13; i <= 26; i++)
            {
                if (i == 20) continue;
                LoadExtra(recolor, i);
            }
            LoadExtra(recolor, 29);
        }
    }

    public class MoonLordFreeEye : MoonLord
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchType(NPCID.MoonLordFreeEye);

        public override ClassState GetVulnerabilityState(NPC npc)
        {
            NPC core = FargoSoulsUtil.NPCExists(npc.ai[3], NPCID.MoonLordCore);
            return core == null ? default : core.GetGlobalNPC<MoonLordCore>().VulnerabilityState;
        }

        public int OnSpawnCounter;
        public int RitualProj;

        public bool SpawnSynchronized;
        public bool SlowMode;

        public float LastState;


        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            base.SendExtraAI(npc, bitWriter, binaryWriter);

            binaryWriter.Write7BitEncodedInt(OnSpawnCounter);
            binaryWriter.Write7BitEncodedInt(RitualProj);
            bitWriter.WriteBit(SpawnSynchronized);
            bitWriter.WriteBit(SlowMode);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            base.ReceiveExtraAI(npc, bitReader, binaryReader);

            OnSpawnCounter = binaryReader.Read7BitEncodedInt();
            RitualProj = binaryReader.Read7BitEncodedInt();
            SpawnSynchronized = bitReader.ReadBit();
            SlowMode = bitReader.ReadBit();
        }

        public override bool SafePreAI(NPC npc)
        {

            NPC core = FargoSoulsUtil.NPCExists(npc.ai[3], NPCID.MoonLordCore);

            if (core == null)
                return true;

            //EVIL EVIL EVIL change
            if (WorldSavingSystem.MasochistModeReal && Main.zenithWorld)
                return true;

            if (!SpawnSynchronized && ++OnSpawnCounter > 2) //sync to other eyes of same core when spawned
            {
                SpawnSynchronized = true;
                OnSpawnCounter = 0;
                for (int i = 0; i < Main.maxProjectiles; i++) //find ritual
                {
                    if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<LunarRitual>()
                        && Main.projectile[i].ai[1] == npc.ai[3])
                    {
                        RitualProj = i;
                        break;
                    }
                }
                for (int i = 0; i < Main.maxNPCs; i++) //eye sync
                {
                    if (Main.npc[i].active && Main.npc[i].type == NPCID.MoonLordFreeEye && Main.npc[i].ai[3] == npc.ai[3] && i != npc.whoAmI)
                    {
                        npc.ai[0] = Main.npc[i].ai[0];
                        npc.ai[1] = Main.npc[i].ai[1];
                        npc.ai[2] = Main.npc[i].ai[2];
                        npc.ai[3] = Main.npc[i].ai[3];
                        npc.localAI[0] = Main.npc[i].localAI[0];
                        npc.localAI[1] = Main.npc[i].localAI[1];
                        npc.localAI[2] = Main.npc[i].localAI[2];
                        npc.localAI[3] = Main.npc[i].localAI[3];
                        break;
                    }
                }
                npc.netUpdate = true;
                NetSync(npc);
            }

            if (WorldSavingSystem.MasochistModeReal && Main.getGoodWorld && LastState != npc.ai[0])
            {
                LastState = npc.ai[0];

                for (int i = 0; i < Main.maxNPCs; i++) //gradually desync from each other
                {
                    if (Main.npc[i].active && Main.npc[i].type == NPCID.MoonLordFreeEye && Main.npc[i].ai[3] == npc.ai[3])
                    {
                        if (i == npc.whoAmI)
                            break;

                        npc.ai[1] += 1;
                    }
                }
            }
            bool slow = false;
            if (core.dontTakeDamage) //behave slower until p2 proper
            {
                slow = true;
                /*
                SlowMode = !SlowMode;
                if (SlowMode)
                {
                    npc.position -= npc.velocity;
                    return false;
                }
                */
            }

            Projectile ritual = FargoSoulsUtil.ProjectileExists(RitualProj, ModContent.ProjectileType<LunarRitual>());
            if (ritual != null && ritual.ai[1] == npc.ai[3])
            {
                int threshold = (int)ritual.localAI[0] - 150;
                if (GetVulnerabilityState(npc) == ClassState.All)
                    threshold = 800 - 150;
                if (npc.Distance(ritual.Center) > threshold) //stay within ritual range
                {
                    npc.Center = Vector2.Lerp(npc.Center, ritual.Center + npc.DirectionFrom(ritual.Center) * threshold, 0.05f);
                }
            }

            CustomFreeEyeAI(npc, core, slow);
            return false;
        }
        // evil and intimidating vanilla AI but with some tweaks
        public void CustomFreeEyeAI(NPC npc, NPC core, bool slow)
        {
            ref Vector2 position = ref npc.position;
            if (Main.rand.NextBool(420))
            {
                SoundEngine.PlaySound(Main.rand.NextFromCollection([SoundID.Zombie100, SoundID.Zombie101]), npc.Center);
            }
            Vector2 vector229 = new Vector2(30f);
            if (!Main.npc[(int)npc.ai[3]].active || Main.npc[(int)npc.ai[3]].type != NPCID.MoonLordCore)
            {
                npc.life = 0;
                npc.HitEffect();
                npc.active = false;
            }

            float num1281 = 0f;
            float num1282 = 0f;
            float num1283 = npc.ai[0];

            float increment = slow ? 0.5f : 1f;
            npc.ai[1] += increment;

            int num1284 = 0;
            int num1285 = 0;
            for (; num1284 < 10; num1284++)
            {
                num1282 = NPC.MoonLordAttacksArray2[1, num1284];
                if (!(num1282 + (float)num1285 <= npc.ai[1]))
                {
                    break;
                }
                num1285 += (int)num1282;
            }
            if (num1284 == 10)
            {
                num1284 = 0;
                npc.ai[1] = 0f;
                num1282 = NPC.MoonLordAttacksArray2[1, num1284];
                num1285 = 0;
            }
            npc.ai[0] = NPC.MoonLordAttacksArray2[0, num1284];
            num1281 = (int)npc.ai[1] - num1285;
            if (npc.ai[0] != num1283)
            {
                npc.netUpdate = true;
            }
            if (npc.ai[0] == -1f)
            {
                npc.ai[1] += increment;
                if (npc.ai[1] > 180f)
                {
                    npc.ai[1] = 0f;
                }
                float num1286 = 1f;
                if (npc.ai[1] < 60f)
                {
                    num1286 = 0.75f;
                    npc.localAI[0] = 0f;
                    npc.localAI[1] = (float)Math.Sin(npc.ai[1] * ((float)Math.PI * 2f) / 15f) * 0.35f;
                    if (npc.localAI[1] < 0f)
                    {
                        npc.localAI[0] = (float)Math.PI;
                    }
                }
                else if (npc.ai[1] < 120f)
                {
                    num1286 = 1f;
                    if (npc.localAI[1] < 0.5f)
                    {
                        npc.localAI[1] += 0.025f;
                    }
                    npc.localAI[0] += (float)Math.PI / 15f;
                }
                else
                {
                    num1286 = 1.15f;
                    npc.localAI[1] -= 0.05f;
                    if (npc.localAI[1] < 0f)
                    {
                        npc.localAI[1] = 0f;
                    }
                }
                npc.localAI[2] = MathHelper.Lerp(npc.localAI[2], num1286, 0.3f);
            }
            if (npc.ai[0] == 0f)
            {
                npc.TargetClosest(faceTarget: false);
                Vector2 v10 = Main.player[npc.target].Center + Main.player[npc.target].velocity * 20f - npc.Center;
                npc.localAI[0] = npc.localAI[0].AngleLerp(v10.ToRotation(), 0.5f);
                npc.localAI[1] += 0.05f;
                if (npc.localAI[1] > 0.7f)
                {
                    npc.localAI[1] = 0.7f;
                }
                npc.localAI[2] = MathHelper.Lerp(npc.localAI[2], 1f, 0.2f);
                /*
                float num1287 = 24f;
                Vector2 center25 = npc.Center;
                Vector2 center26 = Main.player[npc.target].Center;
                Vector2 vector230 = center26 - center25;
                Vector2 vector231 = vector230 - Vector2.UnitY * 200f;
                vector231 = Vector2.Normalize(vector231) * num1287;
                int num1288 = 30;
                */
                float maxSpeed = 24f * increment;
                float accel = 0.5f * increment;
                float decel = 1f * increment;
                float resistance = npc.velocity.Length() * accel / maxSpeed;
                npc.velocity = FargoSoulsUtil.SmartAccel(npc.Center, Main.player[npc.target].Center - Vector2.UnitY * 200, npc.velocity, accel - resistance, decel + resistance);
                //npc.velocity.X = MathHelper.Lerp(npc.velocity.X, (npc.velocity.X * (float)(num1288 - 1) + vector231.X) / (float)num1288, 1f);
                //npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, (npc.velocity.Y * (float)(num1288 - 1) + vector231.Y) / (float)num1288, 1f);
                float num1289 = 1f;
                for (int num1290 = 0; num1290 < 200; num1290++)
                {
                    if (num1290 != npc.whoAmI && Main.npc[num1290].active && Main.npc[num1290].type == NPCID.MoonLordFreeEye && Vector2.Distance(npc.Center, Main.npc[num1290].Center) < 150f)
                    {
                        if (npc.position.X < Main.npc[num1290].position.X)
                        {
                            npc.velocity.X -= num1289;
                        }
                        else
                        {
                            npc.velocity.X += num1289;
                        }
                        if (npc.position.Y < Main.npc[num1290].position.Y)
                        {
                            npc.velocity.Y -= num1289;
                        }
                        else
                        {
                            npc.velocity.Y += num1289;
                        }
                    }
                }
            }
            else if (npc.ai[0] == 1f)
            {
                if (num1281 == 0f)
                {
                    npc.TargetClosest(faceTarget: false);
                    npc.netUpdate = true;
                }
                npc.velocity *= 0.95f;
                if (npc.velocity.Length() < 1f)
                {
                    npc.velocity = Vector2.Zero;
                }
                Vector2 v11 = Main.player[npc.target].Center + Main.player[npc.target].velocity * 20f - npc.Center;
                npc.localAI[0] = npc.localAI[0].AngleLerp(v11.ToRotation(), 0.5f);
                npc.localAI[1] += 0.05f;
                if (npc.localAI[1] > 1f)
                {
                    npc.localAI[1] = 1f;
                }
                if (num1281 < 20f)
                {
                    npc.localAI[2] = MathHelper.Lerp(npc.localAI[2], 1.1f, 0.2f);
                }
                else
                {
                    npc.localAI[2] = MathHelper.Lerp(npc.localAI[2], 0.4f, 0.2f);
                }
                if (num1281 == num1282 - 35f)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath6, npc.Center);
                }
                if ((num1281 == num1282 - 14f || num1281 == num1282 - 7f || num1281 == num1282) && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 vector232 = Utils.Vector2FromElipse(npc.localAI[0].ToRotationVector2(), vector229 * npc.localAI[1]);
                    Vector2 vector233 = Vector2.Normalize(v11) * 8f;
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center.X + vector232.X, npc.Center.Y + vector232.Y, vector233.X, vector233.Y, 462, 35, 0f, Main.myPlayer);
                }
            }
            else if (npc.ai[0] == 2f)
            {
                if (num1281 < 15f)
                {
                    npc.localAI[1] -= 0.07f;
                    if (npc.localAI[1] < 0f)
                    {
                        npc.localAI[1] = 0f;
                    }
                    npc.localAI[2] = MathHelper.Lerp(npc.localAI[2], 0.4f, 0.2f);
                    npc.velocity *= 0.8f;
                    if (npc.velocity.Length() < 1f)
                    {
                        npc.velocity = Vector2.Zero;
                    }
                }
                else if (num1281 < 75f)
                {
                    float num1291 = (num1281 - 15f) / 10f;
                    int num1292 = 0;
                    int num1293 = 0;
                    switch ((int)num1291)
                    {
                        case 0:
                            num1292 = 0;
                            num1293 = 2;
                            break;
                        case 1:
                            num1292 = 2;
                            num1293 = 5;
                            break;
                        case 2:
                            num1292 = 5;
                            num1293 = 3;
                            break;
                        case 3:
                            num1292 = 3;
                            num1293 = 1;
                            break;
                        case 4:
                            num1292 = 1;
                            num1293 = 4;
                            break;
                        case 5:
                            num1292 = 4;
                            num1293 = 0;
                            break;
                    }
                    Vector2 spinningpoint10 = Vector2.UnitY * -30f;
                    Vector2 value8 = spinningpoint10.RotatedBy((float)num1292 * ((float)Math.PI * 2f) / 6f);
                    Vector2 value9 = spinningpoint10.RotatedBy((float)num1293 * ((float)Math.PI * 2f) / 6f);
                    Vector2 vector234 = Vector2.Lerp(value8, value9, num1291 - (float)(int)num1291);
                    float value10 = vector234.Length() / 30f;
                    npc.localAI[0] = vector234.ToRotation();
                    npc.localAI[1] = MathHelper.Lerp(npc.localAI[1], value10, 0.5f);
                    for (int num1294 = 0; num1294 < 2; num1294++)
                    {
                        int num1295 = Dust.NewDust(npc.Center + vector234 - Vector2.One * 4f, 0, 0, DustID.Vortex);
                        Dust dust = Main.dust[num1295];
                        dust.velocity += vector234 / 15f;
                        Main.dust[num1295].noGravity = true;
                    }
                    if ((num1281 - 15f) % 10f == 0f && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 vec3 = Vector2.Normalize(vector234);
                        if (vec3.HasNaNs())
                        {
                            vec3 = Vector2.UnitY * -1f;
                        }
                        vec3 *= 4f;
                        int num1296 = Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center.X + vector234.X, npc.Center.Y + vector234.Y, vec3.X, vec3.Y, 454, 40, 0f, Main.myPlayer, 30f, npc.whoAmI);
                    }
                }
                else if (num1281 < 105f)
                {
                    npc.localAI[0] = npc.localAI[0].AngleLerp(npc.ai[2] - (float)Math.PI / 2f, 0.2f);
                    npc.localAI[2] = MathHelper.Lerp(npc.localAI[2], 0.75f, 0.2f);
                    if (num1281 == 75f)
                    {
                        npc.TargetClosest(faceTarget: false);
                        npc.netUpdate = true;
                        npc.velocity = Vector2.UnitY * -7f;
                        for (int num1297 = 0; num1297 < 1000; num1297++)
                        {
                            Projectile projectile7 = Main.projectile[num1297];
                            if (projectile7.active && projectile7.type == 454 && projectile7.ai[1] == (float)npc.whoAmI && projectile7.ai[0] != -1f)
                            {
                                Projectile projectile8 = projectile7;
                                projectile8.velocity += npc.velocity;
                                projectile7.netUpdate = true;
                            }
                        }
                    }
                    npc.velocity.Y *= 0.96f;
                    npc.ai[2] = (Main.player[npc.target].Center - npc.Center).ToRotation() + (float)Math.PI / 2f;
                    npc.rotation = npc.rotation.AngleTowards(npc.ai[2], (float)Math.PI / 30f);
                }
                else if (num1281 < 120f)
                {
                    SoundEngine.PlaySound(SoundID.Zombie102, npc.Center);
                    if (num1281 == 105f)
                    {
                        npc.netUpdate = true;
                    }
                    Vector2 vector235 = (npc.ai[2] - (float)Math.PI / 2f).ToRotationVector2() * 12f;
                    npc.velocity = vector235 * 2f;
                    for (int num1298 = 0; num1298 < 1000; num1298++)
                    {
                        Projectile projectile9 = Main.projectile[num1298];
                        if (projectile9.active && projectile9.type == 454 && projectile9.ai[1] == (float)npc.whoAmI && projectile9.ai[0] != -1f)
                        {
                            projectile9.ai[0] = -1f;
                            projectile9.velocity = vector235;
                            projectile9.netUpdate = true;
                        }
                    }
                }
                else
                {
                    npc.velocity *= 0.92f;
                    npc.rotation = npc.rotation.AngleLerp(0f, 0.2f);
                }
            }
            else if (npc.ai[0] == 3f)
            {
                if (num1281 < 15f)
                {
                    npc.localAI[1] -= 0.07f;
                    if (npc.localAI[1] < 0f)
                    {
                        npc.localAI[1] = 0f;
                    }
                    npc.localAI[2] = MathHelper.Lerp(npc.localAI[2], 0.4f, 0.2f);
                    npc.velocity *= 0.9f;
                    if (npc.velocity.Length() < 1f)
                    {
                        npc.velocity = Vector2.Zero;
                    }
                }
                else if (num1281 < 45f)
                {
                    npc.localAI[0] = 0f;
                    npc.localAI[1] = (float)Math.Sin((num1281 - 15f) * ((float)Math.PI * 2f) / 15f) * 0.5f;
                    if (npc.localAI[1] < 0f)
                    {
                        npc.localAI[0] = (float)Math.PI;
                    }
                }
                else if (num1281 < 185f)
                {
                    if (num1281 == 45f)
                    {
                        npc.ai[2] = (float)(Main.rand.Next(2) == 0).ToDirectionInt() * ((float)Math.PI * 2f) / 40f;
                        npc.netUpdate = true;
                    }
                    if ((num1281 - 15f - 30f) % 40f == 0f)
                    {
                        npc.ai[2] *= 0.95f;
                    }
                    npc.localAI[0] += npc.ai[2];
                    npc.localAI[1] += 0.05f;
                    if (npc.localAI[1] > 1f)
                    {
                        npc.localAI[1] = 1f;
                    }
                    Vector2 vector236 = npc.localAI[0].ToRotationVector2() * vector229 * npc.localAI[1];
                    float num1299 = MathHelper.Lerp(8f, 20f, (num1281 - 15f - 30f) / 140f);
                    npc.velocity = Vector2.Normalize(vector236) * num1299 * increment;
                    npc.rotation = npc.rotation.AngleLerp(npc.velocity.ToRotation() + (float)Math.PI / 2f, 0.2f);
                    if ((num1281 - 15f - 30f) % 10f == 0f && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 vector237 = npc.Center + Vector2.Normalize(vector236) * vector229.Length() * 0.4f;
                        Vector2 vector238 = Vector2.Normalize(vector236) * 8f;
                        float ai3 = ((float)Math.PI * 2f * (float)Main.rand.NextDouble() - (float)Math.PI) / 30f + (float)Math.PI / 180f * npc.ai[2];
                        Projectile.NewProjectile(npc.GetSource_FromAI(), vector237.X, vector237.Y, vector238.X, vector238.Y, 452, 35, 0f, Main.myPlayer, 0f, ai3);
                    }
                }
                else
                {
                    npc.velocity *= 0.88f;
                    npc.rotation = npc.rotation.AngleLerp(0f, 0.2f);
                    npc.localAI[1] -= 0.07f;
                    if (npc.localAI[1] < 0f)
                    {
                        npc.localAI[1] = 0f;
                    }
                    npc.localAI[2] = MathHelper.Lerp(npc.localAI[2], 1f, 0.2f);
                }
            }
            else
            {
                if (npc.ai[0] != 4f)
                {
                    return;
                }
                if (num1281 == 0f)
                {
                    npc.TargetClosest(faceTarget: false);
                    npc.netUpdate = true;
                }
                if (num1281 < 180f)
                {
                    npc.localAI[2] = MathHelper.Lerp(npc.localAI[2], 1f, 0.2f);
                    npc.localAI[1] -= 0.05f;
                    if (npc.localAI[1] < 0f)
                    {
                        npc.localAI[1] = 0f;
                    }
                    npc.velocity *= 0.95f;
                    if (npc.velocity.Length() < 1f)
                    {
                        npc.velocity = Vector2.Zero;
                    }
                    if (!(num1281 >= 60f))
                    {
                        return;
                    }
                    Vector2 center27 = npc.Center;
                    int num1300 = 0;
                    if (num1281 >= 120f)
                    {
                        num1300 = 1;
                    }
                    for (int num1301 = 0; num1301 < 1 + num1300; num1301++)
                    {
                        int num1302 = 229;
                        float num1303 = 0.8f;
                        if (num1301 % 2 == 1)
                        {
                            num1302 = 229;
                            num1303 = 1.65f;
                        }
                        Vector2 vector239 = center27 + ((float)Main.rand.NextDouble() * ((float)Math.PI * 2f)).ToRotationVector2() * vector229 / 2f;
                        int num1304 = Dust.NewDust(vector239 - Vector2.One * 8f, 16, 16, num1302, npc.velocity.X / 2f, npc.velocity.Y / 2f);
                        Main.dust[num1304].velocity = Vector2.Normalize(center27 - vector239) * 3.5f * (10f - (float)num1300 * 2f) / 10f;
                        Main.dust[num1304].noGravity = true;
                        Main.dust[num1304].scale = num1303;
                        Main.dust[num1304].customData = this;
                    }
                }
                else if (num1281 < num1282 - 15f)
                {
                    if (num1281 == 180f && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        npc.TargetClosest(faceTarget: false);
                        Vector2 spinningpoint11 = Main.player[npc.target].Center - npc.Center;
                        spinningpoint11.Normalize();

                        float deathrayDir = -1f;
                        if (spinningpoint11.X < 0f)
                        {
                            deathrayDir = 1f;
                        }

                        // sync deathray directions
                        NPC? eye = Main.npc.FirstOrDefault(n => n.TypeAlive(npc.type) && n.whoAmI != npc.whoAmI && n.ai[0] == npc.ai[0] && n.ai[1] >= npc.ai[1]); // any other eye currently deathraying
                        if (eye != null)
                        {
                            deathrayDir = eye.ai[2].NonZeroSign();
                        }
                            

                        spinningpoint11 = spinningpoint11.RotatedBy((0f - deathrayDir) * ((float)Math.PI * 2f) / 6f);
                        Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center.X, npc.Center.Y, spinningpoint11.X, spinningpoint11.Y, 455, 50, 0f, Main.myPlayer, deathrayDir * ((float)Math.PI * 2f) / 540f, npc.whoAmI);
                        npc.ai[2] = (spinningpoint11.ToRotation() + (float)Math.PI * 3f) * deathrayDir;
                        npc.netUpdate = true;
                    }
                    npc.localAI[1] += 0.05f;
                    if (npc.localAI[1] > 1f)
                    {
                        npc.localAI[1] = 1f;
                    }
                    float num1306 = (npc.ai[2] >= 0f).ToDirectionInt();
                    float num1307 = npc.ai[2];
                    if (num1307 < 0f)
                    {
                        num1307 *= -1f;
                    }
                    num1307 += (float)Math.PI * -3f;
                    num1307 += num1306 * ((float)Math.PI * 2f) / 540f;
                    npc.localAI[0] = num1307;
                    npc.ai[2] = (num1307 + (float)Math.PI * 3f) * num1306;
                }
                else
                {
                    npc.localAI[1] -= 0.07f;
                    if (npc.localAI[1] < 0f)
                    {
                        npc.localAI[1] = 0f;
                    }
                }
            }
        }
    }

    public class MoonLordBodyPart : MoonLord
    {
        public override NPCMatcher CreateMatcher() => new NPCMatcher().MatchTypeRange(NPCID.MoonLordHead, NPCID.MoonLordHand, NPCID.MoonLordLeechBlob);

        public override ClassState GetVulnerabilityState(NPC npc)
        {
            NPC core = FargoSoulsUtil.NPCExists(npc.ai[3], NPCID.MoonLordCore);
            return core == null ? default : core.GetGlobalNPC<MoonLordCore>().VulnerabilityState;
        }

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);

           //if (npc.type == NPCID.MoonLordHead || npc.type == NPCID.MoonLordHand) npc.lifeMax = (int)Math.Round(npc.lifeMax * 0.75f);
        }
    }
}
