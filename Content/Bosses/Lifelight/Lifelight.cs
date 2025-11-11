using FargowiltasSouls.Assets.Textures;
using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Items.BossBags;
using FargowiltasSouls.Content.Items.Pets;
using FargowiltasSouls.Content.Items.Placables.Relics;
using FargowiltasSouls.Content.Items.Placables.Trophies;
using FargowiltasSouls.Content.Items.Summons;
using FargowiltasSouls.Content.Items.Weapons.Challengers;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Core.Systems;
using Luminance.Core.Graphics;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using FargowiltasSouls.Content.Projectiles.Weapons.ChallengerItems;
using FargowiltasSouls.Common.Utilities;
using FargowiltasSouls.Content.Items.Vanity.Masks;

namespace FargowiltasSouls.Content.Bosses.Lifelight
{

    [AutoloadBossHead]
    public class Lifelight : ModNPC, IPixelatedPrimitiveRenderer
    {
        #region Variables
        public override string Texture => FargoAssets.GetAssetString("Content/Bosses/Lifelight", Name);

        public static readonly SoundStyle ScreechSound1 = FargosSoundRegistry.LifelightScreech1 with { Volume = 1.5f};
        public static readonly SoundStyle DashSound1 = FargosSoundRegistry.LifelightDash with { Volume = 1.5f };
        public static readonly SoundStyle DashSound2 = FargosSoundRegistry.LifelightPixieDash with { Volume = 1.5f};
        public static readonly SoundStyle RuneSound1 = FargosSoundRegistry.LifelightRuneSound with { Volume = 1.5f };
        public static readonly SoundStyle TelegraphSound1 = FargosSoundRegistry.LifelightShotPrep with { Volume = 1.5f};

        const int DefaultHeight = 200;
        const int DefaultWidth = 200;

        private bool Charging = false;
        private bool Flying = false;
        private bool CustomRotation = false;

        public bool HitPlayer = false;
        private bool AttackF1;
        private int dustcounter;

        public Vector2 LockVector1 = new(0, 0);
        public Vector2 LockVector2 = new(0, 0);
        public Vector2 LockVector3 = new(0, 0);
        public Vector2 AuraCenter = new(0, 0);

        private double rotspeed = 0;
        public double rot = 0;

        private bool useDR;
        private bool phaseProtectionDR;
        private bool DoAura;

        public bool PhaseOne = true;
        private bool droppedSummon = false;

        int P2Threshold => Main.expertMode ? (int)(NPC.lifeMax * (WorldSavingSystem.EternityMode ? WorldSavingSystem.MasochistModeReal ? 0.75 : 0.66 : 0.5)) : 0;

        // Visual
        private List<Vector4> chunklist = [];
        public const float DefaultRuneDistance = 100;
        public float RuneDistance = DefaultRuneDistance;
        private bool DrawRunes = true;
        public const float DefaultChunkDistance = 65;
        public float ChunkDistance = 1000;


        public int RuneFormation;
        public int OldRuneFormation;
        public int RuneFormationTimer;

        public const int FormationTime = 60;
        public float GunRotation = 0;
        public Vector2 GunCircleCenter(float lerp) => NPC.Center + GunRotation.ToRotationVector2() * DefaultRuneDistance * (RuneFormation == Formations.GunCloser ? 0.15f : 0.9f) * lerp;
        public float FormationLerp
        {
            get
            {
                float lerp = Math.Clamp((float)RuneFormationTimer / FormationTime, 0, 1);
                return lerp;
            }
        }

        public struct Formations
        {
            public const int Circle = 0;
            public const int Shield = 1;
            public const int Gun = 2;
            public const int Scattered = 3;
            public const int Spear = 4;
            public const int GunCloser = 5;
        }

        public Vector2[] CustomRunePositions = new Vector2[RuneCount];
        public int RuneBlinkTimer = 0;

        public int PyramidPhase = 0;
        public int PyramidTimer = 0;
        public const int PyramidAnimationTime = 60;

        float BodyRotation = 0;
        public float RPS = 0.1f;
        private bool Draw = false;

        public int firstblaster = 2;


        //NPC.AI
        public ref float AI_Timer => ref NPC.ai[1];

        //States
        public int State;
        public int AttackCount;
        // AttackCount increments each attack. pattern:
        // 1 - precision
        // 2 - charge
        // 3 - precision
        // 4 - plunge
        // 5 (phase 2 only): strong attack
        // entering phase 2 sets pattern to charge

        public enum States
        {
            // misc
            Opening,
            P1Transition,
            Deathray,
            FTWDesperation,

            // precision
            RuneExpand,
            RuneScatter,
            LifeBlades,
            ShotBarrage,

            // charges
            Charge,
            PixieCharge,

            // plunges
            Plunge,
            RuneSpear,

            // strongs
            CrystallineCongregation,
            // 
            Count
        }
        public List<States> PrecisionAttacks
        {
            get
            {
                List<States> attacks =
                    [
                    States.RuneExpand,
                    States.RuneScatter,
                    States.ShotBarrage
                    ];
                if (!PhaseOne)
                    attacks.Add(States.LifeBlades);
                return attacks;
            }
        }
        /*
        public List<States> ChargeAttacks =>
        [
            States.Charge,
            States.PixieCharge
        ];
        */
        public List<States> ChargeAttacks
        {
            get
            {
                List<States> attacks =
                    [
                        States.Charge,
                        States.PixieCharge,
                        States.RuneSpear
                    ];
                if (!PhaseOne || WorldSavingSystem.MasochistModeReal)
                    attacks.Add(States.Plunge);
                return attacks;
            }
        }
        public List<States> StrongAttacks =>
        [
            States.CrystallineCongregation
        ];
        int[] LastAttack = new int[3];
        #endregion
        #region Standard
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Lifelight");
            Main.npcFrameCount[NPC.type] = 6;
            NPCID.Sets.TrailCacheLength[NPC.type] = 40;
            NPCID.Sets.TrailingMode[NPC.type] = 1;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.NoMultiplayerSmoothingByType[NPC.type] = true;

            NPCID.Sets.BossBestiaryPriority.Add(NPC.type);

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers()
            { // Influences how the NPC looks in the Bestiary
                CustomTexturePath = "FargowiltasSouls/Assets/Effects/LifeStar", // If the NPC is multiple parts like a worm, a custom texture for the Bestiary is encouraged.
                Position = new Vector2(0f, 0f),
                PortraitPositionXOverride = 0f,
                PortraitPositionYOverride = 0f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

            NPC.AddDebuffImmunities(
            [
                BuffID.Confused,
                    BuffID.Chilled,
                    BuffID.Suffocation,
                    ModContent.BuffType<LethargicBuff>()
            ]);
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheHallow,
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
                new FlavorTextBestiaryInfoElement($"Mods.FargowiltasSouls.Bestiary.{Name}")
            ]);
        }
        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.lifeMax = 48000;
            NPC.defense = 28;
            NPC.damage = 70;
            NPC.knockBackResist = 0f;
            NPC.width = 200;
            NPC.height = 200;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath7;

            /*Music = ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod)
                ? MusicLoader.GetMusicSlot(musicMod, "Assets/Music/LieflightNoCum") : MusicID.OtherworldlyBoss1;
            SceneEffectPriority = SceneEffectPriority.BossLow;*/

            NPC.value = Item.buyPrice(0, 15);

            NPC.dontTakeDamage = true; //until it Appears in Opening
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * balance);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(State);
            writer.Write(AttackCount);

            writer.Write(rotspeed);

            writer.Write(AttackF1);

            writer.WriteVector2(LockVector1);
            writer.WriteVector2(LockVector2);

            writer.Write(PyramidPhase);
            writer.Write(PyramidTimer);
            writer.Write(RuneFormation);
            writer.Write(RuneFormationTimer);

            for (int i = 0; i < LastAttack.Length; i++)
                writer.Write(LastAttack[i]);

            for (int i = 0; i < CustomRunePositions.Length; i++)
                writer.WriteVector2(CustomRunePositions[i]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            State = reader.ReadInt32();
            AttackCount = reader.ReadInt32();

            rotspeed = reader.ReadDouble();

            AttackF1 = reader.ReadBoolean();

            LockVector1 = reader.ReadVector2();
            LockVector2 = reader.ReadVector2();

            PyramidPhase = reader.ReadInt32();
            PyramidTimer = reader.ReadInt32();
            RuneFormation = reader.ReadInt32();
            RuneFormationTimer = reader.ReadInt32();

            for (int i = 0; i < LastAttack.Length; i++)
                LastAttack[i] = reader.ReadInt32();

            for (int i = 0; i < CustomRunePositions.Length; i++)
                CustomRunePositions[i] = reader.ReadVector2();

        }
        #endregion
        #region AI
        public override void AI()
        {

            //Defaults
            Player Player = Main.player[NPC.target];
            Main.time = 27000; //noon
            Main.dayTime = true;
            Main.LocalPlayer.ZoneHallow = true;
            Main.StopRain();
            Main.cloudAlpha = 0;

            NPC.defense = NPC.defDefense;
            NPC.damage = NPC.defDamage;
            NPC.chaseable = true;
            phaseProtectionDR = false;
            NPC.dontTakeDamage = false;
            Flying = false;
            CustomRotation = false;

            if (RuneFormationTimer >= FormationTime)
                OldRuneFormation = RuneFormation;
            if (RuneFormationTimer <= FormationTime)
                RuneFormationTimer++;
            if (RuneBlinkTimer > 0)
                RuneBlinkTimer--;

            useDR = false;

            if (PhaseOne && NPC.life < P2Threshold)
                phaseProtectionDR = true;

            if (PyramidPhase == 1)
            {
                if (PyramidTimer == PyramidAnimationTime)
                {
                    SoundEngine.PlaySound(SoundID.Item53, NPC.Center);
                    NPC.HitSound = SoundID.Item52;

                    NPC.defense = NPC.defDefense + 100;
                    NPC.netUpdate = true;
                }
                useDR = true;
                ChunkDistance = DefaultChunkDistance * (1 - Math.Min((float)PyramidTimer / PyramidAnimationTime, 1f));
            }
            else if (PyramidPhase == -1)
            {
                if (PyramidTimer == 5)
                {
                    SoundEngine.PlaySound(SoundID.Shatter with { Pitch = -0.5f }, NPC.Center);
                    NPC.HitSound = SoundID.NPCHit4;
                    NPC.defense = NPC.defDefense;
                    NPC.netUpdate = true;
                }
                ChunkDistance = DefaultChunkDistance * Math.Min((float)PyramidTimer / PyramidAnimationTime, 1f);
                if (ChunkDistance == DefaultChunkDistance)
                {
                    PyramidPhase = 0;
                    NPC.netUpdate = true;
                }
            }
            PyramidTimer++;
            //rotation
            BodyRotation += RPS * MathHelper.TwoPi / 60f; //first number is rotations/second
            if (RuneFormation == Formations.Gun || OldRuneFormation == Formations.Gun) // faster
            {
                BodyRotation += FormationLerp * 0.25f * MathHelper.TwoPi / 60f;
            }

            if (State != (int)States.Opening) //do not check during spawn anim
            {
                //Aura
                if (DoAura)
                {
                    if (dustcounter > 5 && (DoAura && State == (int)States.RuneExpand)) //do dust instead of runes during rune expand attack
                    {
                        for (int l = 0; l < 180; l++)
                        {
                            double rad2 = 2.0 * l * (MathHelper.Pi / 180.0);
                            double dustdist2 = 1200.0;
                            int DustX2 = (int)AuraCenter.X - (int)(Math.Cos(rad2) * dustdist2);
                            int DustY2 = (int)AuraCenter.Y - (int)(Math.Sin(rad2) * dustdist2);
                            int DustType = Main.rand.NextFromList(DustID.YellowTorch, DustID.PinkTorch, DustID.UltraBrightTorch);
                            int i = Dust.NewDust(new Vector2(DustX2, DustY2), 1, 1, DustType, Scale: 1.5f);
                            Main.dust[i].noGravity = true;
                        }
                        dustcounter = 0;
                    }
                    dustcounter++;

                    float distance = AuraCenter.Distance(Main.LocalPlayer.Center);
                    float threshold = 1200f;
                    Player player = Main.LocalPlayer;
                    if (player.active && !player.dead && !player.ghost) //pull into arena
                    {
                        if (distance > threshold && distance < threshold * 4f)
                        {
                            if (distance > threshold * 2f)
                            {
                                player.Incapacitate();
                                player.velocity.X = 0f;
                                player.velocity.Y = -0.4f;
                            }

                            Vector2 movement = AuraCenter - player.Center;
                            float difference = movement.Length() - threshold;
                            movement.Normalize();
                            movement *= difference < 17f ? difference : 17f;
                            player.position += movement;

                            for (int i = 0; i < 10; i++)
                            {
                                int DustType = Main.rand.NextFromList(DustID.YellowTorch, DustID.PinkTorch, DustID.UltraBrightTorch);
                                int d = Dust.NewDust(player.position, player.width, player.height, DustType, 0f, 0f, 0, default, 1.25f);
                                Main.dust[d].noGravity = true;
                                Main.dust[d].velocity *= 5f;
                            }
                        }
                    }
                }
                AuraCenter = NPC.Center;

                //Targeting
                if (!Player.active || Player.dead || Player.ghost || NPC.Distance(Player.Center) > 2400)
                {
                    NPC.TargetClosest(false);
                    Player = Main.player[NPC.target];
                    if (!Player.active || Player.dead || Player.ghost || NPC.Distance(Player.Center) > 2400)
                    {
                        if (NPC.timeLeft > 60)
                            NPC.timeLeft = 60;
                        NPC.velocity.Y -= 0.4f;
                        return;
                    }
                }
                NPC.timeLeft = 60;
            }
            DoAura = WorldSavingSystem.MasochistModeReal; //reset aura status

            switch ((States)State)
            {
                case States.Opening:
                    Opening();
                    break;
                case States.P1Transition:
                    P1Transition();
                    break;
                case States.Deathray:
                    LaserSpin();
                    break;
                case States.FTWDesperation:
                    FTWDesperation();
                    break;
                case States.RuneExpand:
                    RuneExpand();
                    break;
                case States.RuneScatter:
                    RuneScatter();
                    break;
                case States.LifeBlades:
                    LifeBlades();
                    break;
                case States.ShotBarrage: 
                    ShotBarrage(); 
                    break;
                case States.Charge:
                    Charge();
                    break;
                case States.PixieCharge:
                    PixieCharge();
                    break;
                case States.Plunge:
                    Plunge();
                    break;
                case States.RuneSpear:
                    RuneSpear();
                    break;
                case States.CrystallineCongregation:
                    CrystallineCongregation();
                    break;
                default:
                    StateReset();
                    break;
            }


            if (!CustomRotation)
            {
                if (Charging) //Charging orientation
                {
                    if (NPC.velocity == Vector2.Zero)
                        NPC.rotation = 0f;
                    else
                        NPC.rotation = NPC.velocity.ToRotation() + MathHelper.Pi / 2;
                    GunRotation = NPC.rotation - MathHelper.PiOver2;
                }
                else if (!Flying) //standard upright orientation
                {
                    NPC.rotation = MathHelper.Lerp(NPC.rotation, 0, 0.09f);
                }
            }

            AI_Timer += 1f;
        }
        #endregion
        #region States

        #region Misc
        public void Opening()
        {
            ref float P1AI_Timer = ref NPC.ai[2];

            if (!NPC.HasValidTarget)
                NPC.TargetClosest(false);

            Player Player = Main.player[NPC.target];
            NPC.position.X = Player.Center.X - NPC.width / 2;
            NPC.position.Y = Player.Center.Y - 490 - NPC.height / 2;
            NPC.alpha = (int)(255 - P1AI_Timer * 17);
            RPS = 0.1f;

            if (AI_Timer == 180)
            {

                if (!Main.dedServ)
                    ScreenShakeSystem.StartShake(10, shakeStrengthDissipationIncrement: 10f / 60);

                EModeUtils.DropSummon(NPC, ModContent.ItemType<CrystallineEffigy>(), WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.Lifelight], ref droppedSummon, WorldSavingSystem.EternityMode);

                SoundEngine.PlaySound(ScreechSound1, NPC.Center);
                SoundEngine.PlaySound(SoundID.Item62, NPC.Center);

                for (int i = 0; i < 150; i++)
                {
                    Vector2 vel = new Vector2(1, 0).RotatedByRandom(MathHelper.Pi * 2) * Main.rand.Next(20);
                    int DustType = Main.rand.NextFromList(DustID.YellowTorch, DustID.PinkTorch, DustID.UltraBrightTorch);
                    Dust.NewDust(NPC.Center, 0, 0, DustType, vel.X, vel.Y, 100, new Color(), 1f);
                }
                Draw = true;
                NPC.dontTakeDamage = false;
            }

            if (chunklist.Count < ChunkCount)
            {

                //generating an even sphere using cartesian coordinates
                float phi = MathHelper.Pi * (float)(Math.Sqrt(5) - 1); //golden angle in radians

                int i = chunklist.Count;
                //for (int i = 0; i < amount; i++)
                //{
                float y = 1 - (2 * ((float)i / (ChunkCount - 1)));
                float theta = phi * i;
                float radius = (float)Math.Sqrt(1 - y * y);
                float x = (float)Math.Cos(theta) * radius;
                float z = (float)Math.Sin(theta) * radius;
                chunklist.Add(new(x, y, z, Main.rand.Next(12) + 1));
                Vector2 pos = NPC.Center + (x * Vector2.UnitX + y * Vector2.UnitY) * ChunkDistance;
                SoundEngine.PlaySound(SoundID.Tink, pos);
                for (int d = 0; d < 3; d++)
                {
                    int dust = Dust.NewDust(pos, 5, 5, DustID.Gold, Scale: 1.5f);
                    Main.dust[dust].velocity = (Main.dust[dust].position - pos) / 10;
                }
                //}
            }
            if (AI_Timer < 180f)
            {
                ChunkDistance = 1000 - ((1000 - DefaultChunkDistance) * ((float)AI_Timer / 180f));
                NPC.dontTakeDamage = true;
            }

            if (AI_Timer >= 240 && chunklist.Count >= ChunkCount)
            {
                ChunkDistance = DefaultChunkDistance; // for good measure
                StateReset();
            }
        }
        public void P1Transition()
        {
            ref float SubAttack = ref NPC.localAI[1];
            Player player = Main.player[NPC.target];


            Charging = false;
            Flying = false;
            useDR = true;
            DoAura = true;
            NPC.velocity *= 0.95f;

            NPC.ai[3] = 0; //no periodic nuke

            void PhaseTransition()
            {
                if (RPS < 0.2f) //speed up rotation
                {
                    RPS += 0.1f / 100;
                }
                else
                {
                    RPS = 0.2f;
                }

                if (AI_Timer < 120)
                    FlyingState();
                else
                    NPC.velocity *= 0.96f;

                if (AI_Timer == 5)
                {
                    SoundEngine.PlaySound(ScreechSound1 with { Pitch = -0.5f }, NPC.Center);
                    RuneFormation = Formations.Circle;
                    RuneFormationTimer = 0;
                }
                if (AI_Timer < 60)
                {
                    //if (AI_Timer % 5 == 0)
                    //SoundEngine.PlaySound(SoundID.Tink, Main.LocalPlayer.Center);

                    Color color = Main.rand.NextFromList(Color.Goldenrod, Color.Pink, Color.Cyan);
                    Particle p = new SmallSparkle(
                        worldPosition: NPC.Center,
                        velocity: (Main.rand.NextFloat(5, 50) * Vector2.UnitX).RotatedByRandom(MathHelper.TwoPi),
                        drawColor: color,
                        scale: 1f,
                        lifetime: Main.rand.Next(20, 80),
                        rotation: 0,
                        rotationSpeed: Main.rand.NextFloat(-MathHelper.Pi / 8, MathHelper.Pi / 8)
                        );
                    p.Spawn();
                    p.Position -= p.Velocity * 4; //implosion
                }
                if (AI_Timer == 60f)
                {
                    SoundEngine.PlaySound(SoundID.Item82 with { Pitch = -0.2f }, Main.LocalPlayer.Center);
                    if (PyramidPhase == 0)
                    {
                        PyramidTimer = 0;
                    }
                    PyramidPhase = 1;
                    NPC.netUpdate = true;

                    for (int i = 0; i < 100; i++)
                    {
                        Color color = Main.rand.NextFromList(Color.Goldenrod, Color.Pink, Color.Cyan);
                        Particle p = new SmallSparkle(
                            worldPosition: NPC.Center,
                            velocity: (Main.rand.NextFloat(5, 50) * Vector2.UnitX).RotatedByRandom(MathHelper.TwoPi),
                            drawColor: color,
                            scale: 1f,
                            lifetime: Main.rand.Next(20, 80),
                            rotation: 0,
                            rotationSpeed: Main.rand.NextFloat(-MathHelper.Pi / 8, MathHelper.Pi / 8)
                            );
                        p.Spawn();
                        //p.Position -= p.Velocity * 4; //implosion
                    }
                }
                int mineAmount = WorldSavingSystem.EternityMode ? 45 : 0;
                if (AI_Timer <= 180 && AI_Timer > 180 - mineAmount)
                {
                    //mine explosion
                    int bombwidth = 22;
                    if (FargoSoulsUtil.HostCheck)
                    {
                        int bombType = ModContent.ProjectileType<LifeTransitionBomb>();
                        //for (int i = 0; i < MineAmount; i++)
                        //{
                        int i = (int)(AI_Timer - (180 - mineAmount));
                        Vector2 FindPos()
                        {
                            float rotation = ((float)i / mineAmount) * MathHelper.TwoPi;
                            float distFrac = Main.rand.NextFloat(1);
                            float modifier = (float)Math.Sin(MathHelper.TwoPi * i / 8f) * 0.3f + 0.9f;
                            distFrac = (float)Math.Pow(distFrac, modifier);
                            float min = NPC.width / 3f;
                            float max = 1200;
                            float distance = MathHelper.Lerp(min, max, distFrac);
                            return NPC.Center + (rotation.ToRotationVector2() * distance);
                        }
                        Vector2 pos = FindPos();
                        const int maxAttempts = 30;
                        for (int attempt = 0; attempt < maxAttempts; attempt++)
                        {
                            pos = FindPos();
                            if (!Main.projectile.Any(p => p.active && p.type == bombType && (Vector2.UnitX * p.ai[1] + Vector2.UnitY * p.ai[2]).Distance(pos) < bombwidth * 1.2f))
                            {
                                break;
                            }
                        }
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, bombType, FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 3f, Main.myPlayer, 0, pos.X, pos.Y);
                        //}
                    }
                }
                if (AI_Timer == 240 - 60 - FormationTime)
                {
                    RuneFormation = Formations.Gun;
                    RuneFormationTimer = 0;
                }
                if (AI_Timer == 240 - 60)
                {

                    SoundEngine.PlaySound(SoundID.Item92 with { Pitch = -0.5f }, NPC.Center);

                    if (!Main.dedServ)
                        ScreenShakeSystem.StartShake(10, shakeStrengthDissipationIncrement: 10f / 60);

                    if (FargoSoulsUtil.HostCheck)
                    {
                        foreach (Projectile p in Main.projectile)
                        {
                            if (p.type == ModContent.ProjectileType<LifeBombExplosion>())
                            {
                                //make them fade
                                p.ai[0] = Math.Max(p.ai[0], LifeBombExplosion.MaxTime - 30);
                                p.netUpdate = true;
                            }
                        }
                    }
                }
                if (AI_Timer < 240)
                {
                    LockVector1 = NPC.DirectionTo(player.Center);
                    GunRotation = LockVector1.ToRotation();
                }
            }

            PhaseTransition();
            //ExpandRunes();
            if (AI_Timer >= 240f)
            {
                LaserSpin(true);
            }

        }
        public void LaserSpin(bool phaseTransition = false)
        {
            ref float RandomDistance = ref NPC.ai[0];
            ref float LaserTimer = ref NPC.localAI[2];
            ref float RotationDirection = ref NPC.localAI[0];

            Player player = Main.player[NPC.target];
            NPC.velocity *= 0.9f;
            HitPlayer = true;
            Flying = false;
            Charging = false;

            //for a starting time, make it fade in, then make it spin faster and faster up to a max speed
            const int fadeintime = 10;
            int endTime = 60 * 4;
            if (WorldSavingSystem.EternityMode)
                endTime += 60;
            if (WorldSavingSystem.MasochistModeReal)
                endTime += 30;

            if (!phaseTransition)
                endTime = 110;

            float start = phaseTransition ? 240 : 220;

            if (AI_Timer < start - FormationTime)
                AI_Timer = start - FormationTime;

            if (AI_Timer < 280f)
            {
                if (AI_Timer < start)
                {
                    LockVector1 = NPC.DirectionTo(player.Center);
                    if (PyramidPhase == 0)
                    {
                        PyramidTimer = 0;
                        NPC.netUpdate = true;
                    }
                    PyramidPhase = 1;

                    if (RuneFormation != Formations.Gun)
                    {
                        RuneFormationTimer = 0;
                        NPC.netUpdate = true;
                    }
                    RuneFormation = Formations.Gun;

                }
                else if (AI_Timer == start)
                {
                    LockVector1 = NPC.DirectionTo(player.Center);
                    RotationDirection = Math.Sign(FargoSoulsUtil.RotationDifference(NPC.DirectionTo(player.Center + player.velocity), NPC.DirectionTo(player.Center)));
                    if (RotationDirection == 0)
                        RotationDirection = 1;
                    NPC.netUpdate = true;
                    if (FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<BloomLine>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 0f, Main.myPlayer, -1, NPC.whoAmI);

                    if (!phaseTransition)
                    {
                        if (WorldSavingSystem.MasochistModeReal)
                        {
                            SoundEngine.PlaySound(SoundID.Item91, NPC.Center);
                            if (FargoSoulsUtil.HostCheck)
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.DirectionTo(player.Center).RotatedBy(MathF.PI * 0.75f * (Main.rand.NextBool() ? 1 : -1)) * 10f, ModContent.ProjectileType<LifeNuke>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 3f, Main.myPlayer, 32f, 0);
                        }
                    }
                }
                else
                {
                    Vector2 lockon = NPC.DirectionTo(player.Center).RotatedBy(-RotationDirection * MathF.PI * 0.15f);
                    LockVector1 = Vector2.Lerp(LockVector1, lockon, 0.2f);
                }
                GunRotation = LockVector1.RotatedBy(rot).ToRotation();
                return;
            }


            if (AttackF1)
            {
                AttackF1 = false;

                //string extra = !phaseTransition ? "Short" : "";
                SoundEngine.PlaySound((!phaseTransition ? FargosSoundRegistry.LifelightDeathrayShort : FargosSoundRegistry.LifelightDeathray) with { Volume = 3f }, NPC.Center);
                if (FargoSoulsUtil.HostCheck)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, LockVector1,
                                    ModContent.ProjectileType<LifelightDeathray>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage, 1.25f), 3f, Main.myPlayer, 0, NPC.whoAmI, endTime);
                }
                NPC.velocity.X = 0;
                NPC.velocity.Y = 0;
                Flying = false;

                float pyramidRot = LockVector1.RotatedBy(rot).ToRotation();
                Vector2 PV = NPC.SafeDirectionTo(player.Center);
                Vector2 LV = pyramidRot.ToRotationVector2();
                float anglediff = (float)(Math.Atan2(PV.Y * LV.X - PV.X * LV.Y, LV.X * PV.X + LV.Y * PV.Y)); //real
                RotationDirection = Math.Sign(anglediff);


                NPC.netUpdate = true;
                rotspeed = 0;
                rot = 0;
            }
            GunRotation = LockVector1.RotatedBy(rot).ToRotation();
            if (RotationDirection == 0)
            {
                RotationDirection = 1;
                NPC.netUpdate = true;
            }
            if (LaserTimer >= fadeintime && LaserTimer < endTime)
            {
                float maxSpeed = 1.3f;
                if (rotspeed < maxSpeed)
                {
                    float increment = maxSpeed / 100f;
                    if (!phaseTransition)
                        increment = maxSpeed / 35f;
                    if (WorldSavingSystem.MasochistModeReal)
                        increment *= 1.2f;
                    rotspeed += increment;
                }
                else
                {
                    rotspeed = maxSpeed;
                }
                rot += RotationDirection * MathHelper.Pi / 180 * rotspeed;

            }

            LaserTimer++;

            if (LaserTimer >= endTime && PyramidPhase == 1)
            {
                foreach (Projectile p in Main.projectile)
                {
                    if (p.type == ModContent.ProjectileType<LifeBombExplosion>())
                    {
                        //make them fade
                        p.ai[0] = Math.Max(p.ai[0], LifeBombExplosion.MaxTime - 30);
                        p.netUpdate = true;
                    }
                    //kill deathray, just to be sure
                    if (p.type == ModContent.ProjectileType<LifelightDeathray>())
                    {
                        p.Kill();
                    }
                }

                PyramidPhase = -1;
                PyramidTimer = 0;
                NPC.netUpdate = true;

                RuneFormation = Formations.Circle;
                RuneFormationTimer = 0;
            }
            if (LaserTimer > endTime)
            {
                rotspeed = 0;
                rot = 0;
                if (phaseTransition)
                {
                    GoPhase2();
                }
                else
                {
                    HitPlayer = false;
                    StateReset();
                }
                return;
            }
        }
        public void GoPhase2()
        {
            PhaseOne = false;
            HitPlayer = false;
            NPC.netUpdate = true;
            NPC.TargetClosest(true);
            AttackF1 = true;
            AI_Timer = 0f;
            NPC.ai[2] = 0f;
            NPC.ai[3] = 0f;
            NPC.ai[0] = 0f;
            AttackCount = 1;
            StateReset();
        }

        public void FlyingState(float speedModifier = 1f, bool flyfast = false, Vector2? flyPosition = null, bool orient = true)
        {
            ref float speedVar = ref NPC.localAI[3];
            Flying = true;

            //basically, create a smooth transition when using different speedMod values
            float accel = 0.5f / 30f;
            if (speedVar < speedModifier)
            {
                speedVar += accel;
                if (speedVar > speedModifier)
                    speedVar = speedModifier;
            }
            if (speedVar > speedModifier)
            {
                speedVar -= accel;
                if (speedVar < speedModifier)
                    speedVar = speedModifier;
            }
            speedModifier = speedVar;

            Player Player = Main.player[NPC.target];
            //flight AI
            float flySpeed = 0f;
            float inertia = 10f;
            Vector2 AbovePlayer = new(Player.Center.X, Player.Center.Y - 300f);

            if (flyPosition.HasValue)
                AbovePlayer = flyPosition.Value;

            if (NPC.velocity == Vector2.Zero)
            {
                NPC.velocity = NPC.SafeDirectionTo(AbovePlayer) * 1f;
            }

            // new smart accel
            //NPC.velocity = FargoSoulsUtil.SmartAccel(NPC.Center, AbovePlayer, NPC.velocity, 0.25f, 1f);
            //if (NPC.velocity.Length() > 20f)
            //    NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 20f;

            
            bool Close = Math.Abs(AbovePlayer.Y - NPC.Center.Y) < 32f && Math.Abs(AbovePlayer.X - NPC.Center.X) < 160f;
            if (!Close && NPC.Distance(AbovePlayer) < 500f)
            {
                flySpeed = 9f;
                if (!flyfast)
                {
                    Vector2 flyabovePlayer3 = NPC.SafeDirectionTo(AbovePlayer) * flySpeed;
                    NPC.velocity = (NPC.velocity * (inertia - 1f) + flyabovePlayer3) / inertia;
                }
            }
            if (NPC.velocity == Vector2.Zero)
            {
                NPC.velocity = NPC.SafeDirectionTo(AbovePlayer) * 1f;
            }
            if (NPC.Distance(AbovePlayer) > 360f)
            {
                flySpeed = NPC.Distance(AbovePlayer) / 35f;
                flyfast = true;
                Vector2 flyabovePlayer2 = NPC.SafeDirectionTo(AbovePlayer) * flySpeed;
                NPC.velocity = (NPC.velocity * (inertia - 1f) + flyabovePlayer2) / inertia;
            }
            if (flyfast && (NPC.Distance(AbovePlayer) < 100f || NPC.Distance(Player.Center) < 100f))
            {
                Vector2 flyabovePlayer = NPC.SafeDirectionTo(AbovePlayer) * flySpeed;
                NPC.velocity = flyabovePlayer;
            }
            

            //orientation
            if (orient)
            {
                if (NPC.velocity.ToRotation() > MathHelper.Pi)
                {
                    NPC.rotation = 0f - MathHelper.Pi * NPC.velocity.X * speedModifier / 100;
                }
                else
                {
                    NPC.rotation = 0f + MathHelper.Pi * NPC.velocity.X * speedModifier / 100;
                }
            }

            NPC.position -= NPC.velocity * (1f - speedModifier);
        }
        #endregion

        #region Attacks
        public void RuneExpand()
        {

            ref float ExtraShots = ref NPC.ai[3];
            ref float RandomAngle = ref NPC.ai[2];

            Player Player = Main.player[NPC.target];
            //let projectiles access
            NPC.localAI[0] = RuneDistance;
            NPC.localAI[1] = BodyRotation;
            NPC.localAI[2] = RuneCount;

            int ExpandTime = 120;
            if (!PhaseOne)
                ExpandTime = 120;
            int AttackDuration = 5; //change this depending on phase
            if (!PhaseOne)
                AttackDuration = 30;

            if (NPC.Distance(Player.Center) > 2000)
                FlyingState(1.5f);
            else
                NPC.velocity *= 0.95f;

            if (RuneFormation != Formations.Circle)
            {
                RuneFormation = Formations.Circle;
                RuneFormationTimer = 0;
            }

            if (RuneFormationTimer < FormationTime)
            {
                AI_Timer--;
                return;
            }

            if (AttackF1)
            {
                AttackF1 = false;
                NPC.netUpdate = true;
                SoundEngine.PlaySound(RuneSound1, NPC.Center);
                if (PyramidPhase == 0)
                {
                    PyramidTimer = 0;
                }
                PyramidPhase = 1;

                //invisible rune hitbox
                for (int i = 0; i < RuneCount; i++)
                {
                    float runeRot = (float)(BodyRotation + Math.PI * 2 / RuneCount * i);
                    Vector2 runePos = NPC.Center + runeRot.ToRotationVector2() * RuneDistance;
                    //DrawRunes = false;
                    NPC.netUpdate = true;

                    if (FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), runePos, Vector2.Zero, ModContent.ProjectileType<LifeRuneHitbox>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 3f, Main.myPlayer, NPC.whoAmI, i);

                }
                    
                //decrease size to size of triangle
                NPC.position = NPC.Center;
                NPC.Size = new Vector2(DefaultChunkDistance * 2, DefaultChunkDistance * 2);
                NPC.Center = NPC.position;

                if (PhaseOne)
                    LockVector1 = -Vector2.UnitY;
                else
                    LockVector1 = NPC.DirectionTo(Player.Center);

            }


            if (AI_Timer < ExpandTime) //expand
            {
                if (AI_Timer == ExpandTime - 150)
                {
                    if (WorldSavingSystem.MasochistModeReal)
                    {
                        SoundEngine.PlaySound(SoundID.Item91, NPC.Center);
                        if (FargoSoulsUtil.HostCheck)
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.DirectionTo(Player.Center).RotatedBy(MathF.PI * 0.4f * (Main.rand.NextBool() ? 1 : -1)) * 10f, ModContent.ProjectileType<LifeNuke>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 3f, Main.myPlayer, 32f, 0);
                    }
                }
                if (AI_Timer < 30)
                {
                    if (NPC.Distance(Player.Center) > 250)
                        FlyingState(0.3f, false, Player.Center + Player.DirectionTo(NPC.Center) * 250);

                }
                float expandlerp = AI_Timer / ExpandTime;
                RuneDistance = MathHelper.SmoothStep(DefaultRuneDistance, 1000, MathF.Pow(expandlerp, 2));
                /*
                if (WorldSavingSystem.MasochistModeReal)
                {
                    RuneDistance = Math.Min((float)(100 + Math.Pow(AI_Timer / 5, 2)), 1200);
                }
                else
                {
                    RuneDistance = (float)(100 + Math.Pow(AI_Timer / 5, 2));
                }
                */
                RPS += 0.0005f;
            }
            if (!PhaseOne) // p2 shots
            {
                LockVector1 = LockVector1.RotateTowards(NPC.DirectionTo(Player.Center).ToRotation(), 0.02f);

                Vector2 tipPos = NPC.Center + LockVector1.SafeNormalize(Vector2.UnitX) * 40;
                if (AI_Timer >= ExpandTime - 120)
                {
                    if (AI_Timer % 5 == 0)
                    {
                        float scale = 6f * (AI_Timer - (ExpandTime - 120f)) / 60f;
                        if (AI_Timer > ExpandTime + AttackDuration + 30)
                            scale = 6f * 1 - (AI_Timer - ExpandTime + AttackDuration + 30 / 60f);

                        scale = MathHelper.Clamp(scale, 0, 6);

                        Particle p = new ExpandingBloomParticle(tipPos, NPC.velocity, Color.Cyan, Vector2.One * scale, Vector2.One * scale, 10);
                        p.Spawn();
                    }
                }

                if (AI_Timer >= ExpandTime - 60 && AI_Timer <= ExpandTime + AttackDuration + 30)
                {
                    if (AI_Timer % 24 == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item12, NPC.Center);
                        if (FargoSoulsUtil.HostCheck)
                        {
                            float ProjectileSpeed = 12f;
                            Vector2 vel = LockVector1.SafeNormalize(Vector2.UnitX) * ProjectileSpeed;
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), tipPos, vel, ModContent.ProjectileType<LifeWave>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
                        }
                    }
                }
            }

            if (AI_Timer == ExpandTime + AttackDuration) //noise
            {
                SoundEngine.PlaySound(RuneSound1, NPC.Center);
            }
            if (AI_Timer >= ExpandTime + AttackDuration) //retract
            {
                HitPlayer = false; //stop dealing contact damage (anti-cheese)
                RuneDistance = MathHelper.SmoothStep(1000, DefaultRuneDistance, (AI_Timer - ExpandTime - AttackDuration) / ExpandTime);
                /*
                if (WorldSavingSystem.MasochistModeReal)
                {
                    RuneDistance = Math.Min((float)(DefaultRuneDistance + Math.Pow((ExpandTime - (AI_Timer - ExpandTime - AttackDuration)) / 5, 2)), 1200);
                }
                else
                {
                    RuneDistance = (float)(DefaultRuneDistance + Math.Pow((ExpandTime - (AI_Timer - ExpandTime - AttackDuration)) / 5, 2));
                }
                */
                RPS -= 0.0005f;
            }
            if (AI_Timer >= ExpandTime + AttackDuration + ExpandTime)
            {
                RuneDistance = DefaultRuneDistance; //make sure

                //kill rune hitboxes
                if (FargoSoulsUtil.HostCheck)
                {
                    foreach (Projectile p in Main.projectile)
                    {
                        if (p.type == ModContent.ProjectileType<LifeRuneHitbox>())
                        {
                            p.Kill();
                        }
                    }
                }
                DrawRunes = true;
                NPC.netUpdate = true;

                PyramidPhase = -1;
                PyramidTimer = 0;

                //revert size
                NPC.position = NPC.Center;
                NPC.Size = new Vector2(DefaultWidth, DefaultHeight);
                NPC.Center = NPC.position;

                Flying = true;
                StateReset();
            }
        }
        public void RuneScatter()
        {
            Player Player = Main.player[NPC.target];

            int startup = FormationTime + (WorldSavingSystem.MasochistModeReal ? 40 : WorldSavingSystem.EternityMode ? 40 : 50);

            int explosionTime = 60;

            if (AttackF1)
            {
                AttackF1 = false;
                
                NPC.netUpdate = true;

                RuneFormation = Formations.Scattered;
                RuneFormationTimer = 0;

                for (int i = 0; i < CustomRunePositions.Length; i++)
                {
                    Vector2 offset = Vector2.Zero;
                    if (i == CustomRunePositions.Length - 1 && !CustomRunePositions.Any(p => p.Distance(Player.Center) < 100))
                    {
                        CustomRunePositions[i] = Player.Center + Vector2.UnitX.RotatedBy(MathF.Tau * i / RuneCount).RotatedByRandom(MathF.PI * 0.1f) * 200f;
                        break;
                    }
                    for (int attempts = 0; attempts < 100; attempts++)
                    {
                        bool valid = true;
                        offset = Vector2.UnitX.RotatedBy(MathF.Tau * i / RuneCount).RotatedByRandom(MathF.PI * 0.1f) * Main.rand.NextFloat(100f, 450f);

                        if ((Player.Center + offset).Distance(Player.Center) < Player.height * 0.7f) // NEVER place on top of player
                        {
                            valid = false;
                            attempts--;
                        }
                        else
                        {
                            for (int j = 0; j < CustomRunePositions.Length; j++)
                            {
                                if (i != j && offset.Distance(CustomRunePositions[j] - Player.Center) < 100)
                                {
                                    valid = false;
                                    break;
                                }

                            }
                        }
                        if (valid)
                        {
                            CustomRunePositions[i] = Player.Center + offset;
                            break;
                        }
                            
                    }
                    if (offset.Length() < 120f)
                        offset += offset.SafeNormalize(Vector2.Zero) * 100;
                    CustomRunePositions[i] = Player.Center + offset;
                }

                if ((WorldSavingSystem.EternityMode && !PhaseOne) || WorldSavingSystem.MasochistModeReal)
                {
                    SoundEngine.PlaySound(SoundID.Item91, NPC.Center);
                    if (FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.DirectionTo(Player.Center).RotatedBy(MathF.PI * 0.4f * (Main.rand.NextBool() ? 1 : -1)) * 10f, ModContent.ProjectileType<LifeNuke>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 3f, Main.myPlayer, 32f, 0);
                }
            }

            FlyingState(0.2f);

            if (WorldSavingSystem.EternityMode && AI_Timer < startup)
            {
                for (int i = 0; i < CustomRunePositions.Length; i++)
                {
                    CustomRunePositions[i] += Player.velocity;
                }
            }
            if (AI_Timer == FormationTime) // make runes blink right before getting hitbox
            {
                RuneBlinkTimer = 40;
                SoundEngine.PlaySound(SoundID.MaxMana, NPC.Center);
            }
            if (AI_Timer > FormationTime)
            {
                for (int i = 0; i < CustomRunePositions.Length; i++)
                {
                    Vector2 dif = CustomRunePositions[i] - Player.Center;
                    if (dif.LengthSquared() < Math.Pow(Player.height * 0.8f, 2))
                    {
                        CustomRunePositions[i] += dif.SafeNormalize(Vector2.UnitX) * 6;
                    }
                }
            }
                
            if (AI_Timer == startup) // put hitboxes and eventual explosion
            {
                SoundEngine.PlaySound(RuneSound1, NPC.Center);

                NPC.netUpdate = true;
                for (int i = 0; i < RuneCount; i++)
                {
                    Vector2 runePos = CustomRunePositions[i];

                    if (FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), runePos, Vector2.Zero, ModContent.ProjectileType<LifeRuneExplosion>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 3f, Main.myPlayer, NPC.whoAmI, i, ai2: explosionTime);
                }
            }
            /*
            if (AI_Timer == startup + explosionTime) // explosion
            {

            }
            */
            if (AI_Timer >= startup + explosionTime)
                DrawRunes = true;
            if (AI_Timer > startup + explosionTime + 10)
            {
                if (!PhaseOne)
                {
                    GoToDeathray();
                    return;
                }
                if (RuneFormation != Formations.Circle)
                {
                    RuneFormation = Formations.Circle;
                    RuneFormationTimer = 0;
                }
                else if (RuneFormationTimer >= FormationTime)
                {
                    StateReset();
                }
            }
        }
        public void LifeBlades()
        {
            ref float TeleportX = ref NPC.localAI[0];
            ref float TeleportY = ref NPC.localAI[1];
            ref float RandomAngle = ref NPC.ai[3];

            Player Player = Main.player[NPC.target];
            if (AttackF1)
            {
                AttackF1 = false;
                NPC.netUpdate = true;

                RuneFormation = Formations.Circle;
                RuneFormationTimer = 0;
            }
            Vector2 desiredPos = Player.Center + Player.DirectionTo(NPC.Center) * 320;
            FlyingState(0.55f, false, desiredPos);

            //new homing swords:
            const int endTime = 487;
            if (AI_Timer == 70)
            {
                SoundEngine.PlaySound(SoundID.Item71, NPC.Center);
                int swords = WorldSavingSystem.MasochistModeReal ? 6 : WorldSavingSystem.EternityMode ? 4 : 2;
                for (int i = 0; i < swords; i++)
                {
                    int randSide = i % 2 == 0 ? 1 : -1;
                    float randRot = Main.rand.NextFloat(-MathHelper.Pi / 6, MathHelper.Pi / 6);
                    //float extraVel = i;
                    Vector2 offset1 = (NPC.SafeDirectionTo(Player.Center) * (6f + 3f * i)).RotatedBy(MathHelper.PiOver2 * randSide + randRot);
                    if (FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, -offset1, ModContent.ProjectileType<LifelightLifeblade>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 3f, Main.myPlayer, 0, ai1: NPC.target);
                }
                /*
                if (WorldSavingSystem.EternityMode && !PhaseOne && AI_Timer >= 70 * 4) // chance to interrupt in emode
                {
                    if (Main.rand.NextBool(4))
                        AI_Timer = endTime + 1;
                }
                */
            }
            float extraTime = PhaseOne || !WorldSavingSystem.EternityMode ? 100 : -20;
            if (AI_Timer > endTime + extraTime)
            {
                foreach (Projectile p in Main.projectile)
                {
                    if (p.type == ModContent.ProjectileType<LifelightLifeblade>())
                    {
                        float extra = PhaseOne || !WorldSavingSystem.EternityMode ? 0 : 50;
                        p.localAI[0] = 1200 - extra;
                    }
                }
                Flying = true;
                if (!PhaseOne)
                {
                    GoToDeathray();
                    return;
                }
                StateReset();
            }
        }
        public void ShotBarrage()
        {
            ref float ShotTimer = ref NPC.localAI[1];
            ref float P1AI_Timer = ref NPC.ai[2];

            P1AI_Timer++;

            FlyingState(0.2f);

            Player Player = Main.player[NPC.target];
            if (AttackF1)
            {
                AttackF1 = false;
                NPC.netUpdate = true;
                ShotTimer = 0;
                //Rampup = 1;
                RuneFormation = Formations.GunCloser;
                RuneFormationTimer = 0;
                SoundEngine.PlaySound(TelegraphSound1, NPC.Center);
                GunRotation = NPC.SafeDirectionTo(Player.Center).ToRotation();

                if ((WorldSavingSystem.EternityMode && !PhaseOne) || WorldSavingSystem.MasochistModeReal)
                {
                    SoundEngine.PlaySound(SoundID.Item91, NPC.Center);
                    if (FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.DirectionTo(Player.Center).RotatedBy(MathF.PI * 0.4f * (Main.rand.NextBool() ? 1 : -1)) * 10f, ModContent.ProjectileType<LifeNuke>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 3f, Main.myPlayer, 32f, 0);
                }
            }
            GunRotation = GunRotation.ToRotationVector2().RotateTowards(NPC.SafeDirectionTo(Player.Center).ToRotation(), 0.1f).ToRotation();

            if (P1AI_Timer > FormationTime && AI_Timer < 220f)
            {
                int threshold = WorldSavingSystem.MasochistModeReal ? 5 : 6;
                float rampupTimer = P1AI_Timer - FormationTime;
                if (true) //(rampupTimer < 60f)
                    threshold += 8;
                if (++ShotTimer % threshold == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item12, NPC.Center);

                    float finalSpreadOffset = MathHelper.Pi / (WorldSavingSystem.MasochistModeReal ? 7 : WorldSavingSystem.EternityMode ? 6 : 5);
                    float startOffset = (MathHelper.Pi - finalSpreadOffset) * 0.3f;
                    const int timeToFocus = 60;

                    float rampRatio = (float)Math.Min(1f, ShotTimer / timeToFocus);
                    float rotationToUse = finalSpreadOffset + startOffset * (float)Math.Cos(MathHelper.PiOver2 * rampRatio);

                    Vector2 vel = NPC.SafeDirectionTo(Player.Center);
                    float length = 8f + 6f * rampRatio;
                    vel *= length;

                    int projType = ModContent.ProjectileType<LifeHomingProjSmall>(); //length > 8f ? ModContent.ProjectileType<LifeHomingProjSmall>() : ModContent.ProjectileType<LifeProjSmall>();

                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 finalVel = vel.RotatedBy(rotationToUse * i );
                        if (FargoSoulsUtil.HostCheck)
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), GunCircleCenter(0) + finalVel * 2, finalVel, projType, FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 0f, Main.myPlayer);
                    }
                }
            }
            float end = WorldSavingSystem.EternityMode ? 170f : 200f;

            if (AI_Timer >= end - 30)
            {
                if (!PhaseOne)
                {
                    GoToDeathray();
                    return;
                }
            }
            if (AI_Timer >= end)
            {

                StateReset();
            }
        }

        public void Charge()
        {
            ref float SwordSide = ref NPC.ai[3];
            ref float AttackCount = ref NPC.ai[2];

            ref float TeleportX = ref NPC.localAI[0];
            ref float TeleportY = ref NPC.localAI[1];
            ref float HasTeleported = ref NPC.localAI[2];

            Player Player = Main.player[NPC.target];
            int StartTime = (WorldSavingSystem.MasochistModeReal ? 60 : WorldSavingSystem.EternityMode ? 70 : 90);

            if (AttackF1)
            {
                LockVector3 = NPC.Center;
                AttackF1 = false;
                NPC.netUpdate = true;
                SoundEngine.PlaySound(ScreechSound1, NPC.Center);

                RuneFormationTimer = 0;
                RuneFormation = Formations.Shield;

                GunRotation = NPC.DirectionTo(Player.Center).ToRotation();
                SwordSide = Main.rand.NextBool() ? 1 : -1;
                HasTeleported = 0;

                AI_Timer = -20;
            }
            Flying = false;
            
            HitPlayer = true;
            AuraCenter = LockVector3; //lock arena in place during charges

            float chargecount = PhaseOne ? 3f : 4f;

            float chargeSpeed = 22f;
            Vector2 chargeatPlayer = NPC.SafeDirectionTo(Player.Center) * chargeSpeed;

            if (AI_Timer < StartTime * 0.5f && AttackCount == 0)
            {
                Vector2 desiredPos = Player.Center + Player.DirectionTo(NPC.Center) * 450;
                FlyingState(1f, false, desiredPos, orient: false);
            }


            if (AttackCount < chargecount)
            { 
                if (AI_Timer == StartTime - 30 && AttackCount == 0 && WorldSavingSystem.EternityMode) // bomb
                {
                    SoundEngine.PlaySound(SoundID.Item91, NPC.Center);
                    if (FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.DirectionTo(Player.Center).RotatedBy(MathF.PI * 0.2f * (Main.rand.NextBool() ? 1 : -1)) * 10f, ModContent.ProjectileType<LifeNuke>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 3f, Main.myPlayer, 32f, 0);
                }

                if (AI_Timer == StartTime + 1) // sfx
                {
                    SoundEngine.PlaySound(DashSound1, NPC.Center);
                    bool earlyTeleport = WorldSavingSystem.MasochistModeReal && Main.rand.NextBool(3);
                    if (!PhaseOne && HasTeleported == 0 && AttackCount > 0 && (AttackCount == 2 || earlyTeleport))
                        HasTeleported = 1; // prepare for teleport during next charge
                    NPC.netUpdate = true;
                }

                Flying = false;
                Charging = false;
                CustomRotation = true;

                if (NPC.velocity.ToRotation() > MathHelper.Pi)
                {
                    NPC.rotation = 0f - MathHelper.Pi * NPC.velocity.X * 1f / 100;
                }
                else
                {
                    NPC.rotation = 0f + MathHelper.Pi * NPC.velocity.X * 1f / 100;
                }

                if (AI_Timer < StartTime)
                {
                    GunRotation = MathHelper.Lerp(GunRotation, NPC.DirectionTo(Player.Center).ToRotation(), 0.1f * AI_Timer / StartTime);
                }
                else
                    GunRotation = NPC.velocity.ToRotation();
                

                if (AI_Timer > StartTime * 0.5f) // movement
                {
                    float speedmod = 1.2f;

                    float progress = (AI_Timer - StartTime) / StartTime;
                    if (AI_Timer < StartTime || progress > 0.4f)
                    {
                        NPC.velocity *= 0.94f;
                        speedmod /= 6f;
                    }
                    NPC.velocity += NPC.DirectionTo(Player.Center) * speedmod;
                    NPC.velocity.ClampLength(0, 35);
                }

                // p2 tp and double charge
                if (!PhaseOne && HasTeleported == 1)
                {

                    float tpTelegraph = 40;
                    float leeway = 5;
                    float timeOffset = tpTelegraph + leeway;

                    float endTime = StartTime * 2;

                    if (AI_Timer >= endTime - timeOffset && AI_Timer <= endTime - timeOffset + 20)
                    {
                        if (AI_Timer == endTime - timeOffset)
                            LockVector1 = Player.velocity.SafeNormalize(Vector2.UnitX) * 500;
                        LockVector1 = LockVector1.RotateTowards(Player.velocity.SafeNormalize(Vector2.UnitX).ToRotation(), 0.05f);

                        Vector2 TpPos = Player.Center + LockVector1;

                        TeleportX = TpPos.X; //exposing so proj can access
                        TeleportY = TpPos.Y;
                        if (AI_Timer == endTime - timeOffset)
                        {

                            NPC.netUpdate = true;

                            //SoundEngine.PlaySound(SoundID.Item91, NPC.Center);
                            if (FargoSoulsUtil.HostCheck)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromThis(), TpPos, Vector2.Zero, ModContent.ProjectileType<LifeTpTelegraph>(), 0, 0f, Main.myPlayer, -tpTelegraph, NPC.whoAmI);
                                //if (WorldSavingSystem.MasochistModeReal)
                                    //Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, NPC.DirectionTo(Player.Center).RotatedBy(MathF.PI * 0.2f * (Main.rand.NextBool() ? 1 : -1)) * 10f, ModContent.ProjectileType<LifeNuke>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 3f, Main.myPlayer, 32f, 0);
                            }
                                
                        }
                    }

                    if (AI_Timer == endTime - leeway)
                    {
                        NPC.Center = new(TeleportX, TeleportY);
                        for (int i = 0; i < NPC.oldPos.Length; i++)
                            NPC.oldPos[i] = NPC.Center;
                        NPC.velocity = NPC.DirectionTo(Player.Center) * 0.1f;
                        SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                        NPC.netUpdate = true;

                        AI_Timer = StartTime * 0.5f;
                        AttackCount++;

                        HasTeleported = 2; // can't do it again
                        return;
                    }
                }

                if (AI_Timer == StartTime * 2) // reset
                {
                    NPC.netUpdate = true;
                    AI_Timer = AttackCount == chargecount - 1 ? 0 : StartTime;
                    if (AttackCount == chargecount - 1)
                    {
                        if (RuneFormation != Formations.Circle)
                        {
                            RuneFormation = Formations.Circle;
                            RuneFormationTimer = 0;
                        }
                    }
                    AttackCount += 1f;
                }
            }
            
            if (AttackCount >= chargecount)
            {
                NPC.velocity *= 0.96f;
                Vector2 desiredPos = Player.Center + Player.DirectionTo(NPC.Center) * 250;
                FlyingState(1f, false, desiredPos, orient: true);

                NPC.velocity.X = 0f;
                NPC.velocity.Y = 0f;
                HitPlayer = false;
                Flying = true;
                Charging = false;
                StateReset();
            }
        }
        public void PixieCharge()
        {
            ref float ChargeCounter = ref NPC.ai[2];
            ref float RandomOffset = ref NPC.ai[3];

            ref float TeleportX = ref NPC.localAI[0];
            ref float TeleportY = ref NPC.localAI[1];

            Player Player = Main.player[NPC.target];
            int StartTime = (WorldSavingSystem.MasochistModeReal ? 80 : WorldSavingSystem.EternityMode ? 90 : 100);

            if (AttackF1)
            {
                Flying = true;
                AttackF1 = false;
                NPC.netUpdate = true;
                RandomOffset = 0;
                SoundEngine.PlaySound(ScreechSound1, NPC.Center);
                NPC.rotation = NPC.DirectionTo(Player.Center).ToRotation() + MathHelper.PiOver2;
                LockVector3 = NPC.Center;

                RuneFormationTimer = 0;
                RuneFormation = Formations.Shield;
            }
            AuraCenter = LockVector3;

            Flying = false;
            Charging = false;
            CustomRotation = true;

            if (NPC.velocity.ToRotation() > MathHelper.Pi)
            {
                NPC.rotation = 0f - MathHelper.Pi * NPC.velocity.X * 1f / 100;
            }
            else
            {
                NPC.rotation = 0f + MathHelper.Pi * NPC.velocity.X * 1f / 100;
            }

            if (AI_Timer < StartTime)
            {
                Vector2 desiredPos = Player.Center + Player.DirectionTo(NPC.Center) * 550;
                FlyingState(1f, false, desiredPos, orient: false);
                GunRotation = MathHelper.Lerp(GunRotation, NPC.DirectionTo(Player.Center).ToRotation(), 0.1f * AI_Timer / StartTime);
            }
            else
            {
                GunRotation = NPC.velocity.ToRotation();
            }

            if (AI_Timer == StartTime - 5)
            {
                LockVector1 = Player.Center;
                NPC.netUpdate = true;
            }
            const int ChargeCD = 60 + 8; // compensation for accel changes


            // p2 tp and double charge
            if (!PhaseOne && ChargeCounter == 0)
            {
                float tpTelegraph = 40;
                float leeway = 5;
                float timeOffset = tpTelegraph + leeway;

                if (AI_Timer >= StartTime + ChargeCD - timeOffset && AI_Timer <= StartTime + ChargeCD - timeOffset + 20)
                {
                    if (AI_Timer == StartTime + ChargeCD - timeOffset)
                        LockVector1 = Player.velocity.SafeNormalize(Vector2.UnitX) * 500;
                    LockVector1 = LockVector1.RotateTowards(Player.velocity.SafeNormalize(Vector2.UnitX).ToRotation(), 0.05f);

                    Vector2 TpPos = Player.Center + LockVector1;

                    TeleportX = TpPos.X; //exposing so proj can access
                    TeleportY = TpPos.Y;
                    if (AI_Timer == StartTime + ChargeCD - timeOffset)
                    {
                        NPC.netUpdate = true;

                        if (FargoSoulsUtil.HostCheck)
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), TpPos, Vector2.Zero, ModContent.ProjectileType<LifeTpTelegraph>(), 0, 0f, Main.myPlayer, -tpTelegraph, NPC.whoAmI);
                    }
                }

                if (AI_Timer == StartTime + ChargeCD - leeway)
                {
                    NPC.Center = new(TeleportX, TeleportY);

                    for (int i = 0; i < NPC.oldPos.Length; i++)
                        NPC.oldPos[i] = NPC.Center;
                    NPC.velocity = NPC.DirectionTo(Player.Center) * 0.1f;
                    SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                    NPC.netUpdate = true;

                    AI_Timer = StartTime * 0.5f;
                    ChargeCounter++;
                    return;
                }
            }

            if (AI_Timer >= StartTime + ChargeCD)
            {
                NPC.velocity *= 0.97f;
                Vector2 desiredPos = Player.Center + Player.DirectionTo(NPC.Center) * 250;
                FlyingState(1f, false, desiredPos, orient: true);

                foreach (Projectile p in Main.projectile.Where(p => p.Alive()))
                {
                    if (p.type == ModContent.ProjectileType<LifeHomingProj>())
                        p.ai[2] = 1;
                }
                Flying = true;
                Charging = false;
                StateReset();
                return;
            }

            Vector2 atPlayer = NPC.SafeDirectionTo(Player.Center);

            if (AI_Timer == StartTime) //charge
            {
                SoundEngine.PlaySound(DashSound1, NPC.Center);

                float initialSpeed = 10f;
                NPC.velocity = atPlayer * initialSpeed;
                NPC.netUpdate = true;
            }
            if (AI_Timer > StartTime && AI_Timer < StartTime + 55)
            {
                float modifier = WorldSavingSystem.EternityMode ? WorldSavingSystem.MasochistModeReal ? 1.4f : 1.2f : 0.5f;
                NPC.velocity += atPlayer * modifier;
                NPC.velocity = NPC.velocity.ClampLength(0, 25f);
            }
            if (AI_Timer % 5 == 0 && AI_Timer > StartTime) //fire pixies during charges
            {
                SoundEngine.PlaySound(SoundID.Item25, NPC.Center);
                if (FargoSoulsUtil.HostCheck)
                {
                    float knockBack10 = 3f;
                    Vector2 shootoffset4 = Vector2.Normalize(NPC.velocity).RotatedBy(RandomOffset) * 5f;
                    RandomOffset = (float)(Main.rand.Next(-30, 30) * (MathHelper.Pi / 180.0)); //change random offset after so game has time to sync
                    float ai0 = 0;
                    if (!WorldSavingSystem.MasochistModeReal)
                    {
                        ai0 = -30;
                        shootoffset4 /= 2;
                    }

                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, shootoffset4, ModContent.ProjectileType<LifeHomingProj>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), knockBack10, Main.myPlayer, ai0, NPC.whoAmI);
                }
            }
            NPC.velocity *= 0.99f;
        }

        public void Plunge()
        {
            ref float TeleportX = ref NPC.localAI[0];
            ref float TeleportY = ref NPC.localAI[1];
            ref float PlungeCount = ref NPC.ai[2];

            Player Player = Main.player[NPC.target];
            int StartTime = (WorldSavingSystem.MasochistModeReal ? 60 : WorldSavingSystem.EternityMode ? 70 : 80);
            float tpTelegraph = 40;

            if (AttackF1)
            {
                LockVector3 = NPC.Center;
                AttackF1 = false;
                NPC.netUpdate = true;
                Flying = false;

                SoundEngine.PlaySound(ScreechSound1, NPC.Center);

                RuneFormation = Formations.Shield;
                RuneFormationTimer = 0;
            }
            AuraCenter = LockVector3;

            Vector2 TpPos = new(Player.Center.X, Player.Center.Y - 400f);

            TeleportX = TpPos.X; //exposing so proj can access
            TeleportY = TpPos.Y;

            if (AI_Timer == 1)
            {
                LockVector2 = new Vector2(Player.Center.X, Player.Center.Y - 400f);
            }
            if (AI_Timer == StartTime - 15 - tpTelegraph && FargoSoulsUtil.HostCheck)
            {

                Projectile.NewProjectile(NPC.GetSource_FromThis(), TpPos, Vector2.Zero, ModContent.ProjectileType<LifeTpTelegraph>(), 0, 0f, Main.myPlayer, -tpTelegraph, NPC.whoAmI);
                /*
                if (WorldSavingSystem.MasochistModeReal)
                {
                    //below wall telegraph
                    for (int i = 0; i < 60; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(LockVector2.X - 1500, LockVector2.Y + 400 + 500 + 60 * i), Vector2.Zero, ModContent.ProjectileType<BloomLine>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 0f, Main.myPlayer, 0, 0);
                    }
                }
                */
            }
            if (AI_Timer < StartTime - 15 && PlungeCount <= 0)
                NPC.velocity *= 0.95f;

            if (AI_Timer == StartTime - 15)
            {
                Flying = false;
                Charging = true;
                NPC.Center = TpPos;
                for (int i = 0; i < NPC.oldPos.Length; i++)
                    NPC.oldPos[i] = NPC.Center;
                NPC.velocity.X = 0f;
                NPC.velocity.Y = 0.1f;
                SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                NPC.netUpdate = true;
            }
            GunRotation = NPC.rotation - MathHelper.PiOver2;
            if (AI_Timer == StartTime)
            {
                SoundEngine.PlaySound(DashSound1, NPC.Center);
                HitPlayer = true;
                float chargeSpeed2 = 50f;
                NPC.velocity.Y = chargeSpeed2;
                NPC.netUpdate = true;
                //below wall
                /*
                if (WorldSavingSystem.MasochistModeReal)
                {
                    if (FargoSoulsUtil.HostCheck)
                    {
                        for (int i = 0; i < 120; i++)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(LockVector2.X - 1200, LockVector2.Y + 600 + 500 + 30 * i), new Vector2(60, 0), ModContent.ProjectileType<LifeProjLarge>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 3f, Main.myPlayer);
                        }
                        for (int i = 0; i < 120; i++)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(LockVector2.X + 1200, LockVector2.Y + 600 + 500 + 30 * i), new Vector2(-60, 0), ModContent.ProjectileType<LifeProjLarge>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 3f, Main.myPlayer);
                        }
                    }
                }
                */

                if (FargoSoulsUtil.HostCheck && WorldSavingSystem.EternityMode)
                {
                    Vector2 shootdown = new(0f, 10f);
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, NPC.Center);
                    const int shards = 15;
                    for (int k = 0; k <= shards; k++)
                    {
                        Vector2 vel = Vector2.UnitY * NPC.velocity.Y;
                        vel += shootdown / 2;
                        vel *= 0.65f;
                        float frac = ((float)k / shards);
                        float maxAccel = 0.5f;
                        float accel = -maxAccel + (maxAccel * 2 * frac);

                        float yaccel = 1.2f + 0.008f * MathF.Pow(MathF.Abs(k - (float)(shards / 2f)), 2f);

                        float speedMod = 0.7f;

                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel * speedMod, ModContent.ProjectileType<LifeNeggravProj>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 3f, Main.myPlayer, ai1: yaccel * speedMod, ai2: accel * speedMod);

                        int j = k - 1;
                        float shards2 = shards - 1;
                        frac = ((float)j / shards2);

                        float maxAccel2 = 0.35f;
                        float accel2 = -maxAccel2 + (maxAccel2 * 2 * frac);
                        yaccel = 1.2f + 0.008f * MathF.Pow(MathF.Abs(j - (float)(shards2 / 2f)), 2f);
                        if (k >= shards - 1)
                            continue;

                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, vel * speedMod, ModContent.ProjectileType<LifeNeggravProj>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 3f, Main.myPlayer, ai1: 0.9f * yaccel * speedMod, ai2: accel2 * speedMod);
                    }
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, NPC.Center);
                }
            }
            if (AI_Timer >= StartTime)
            {
                HitPlayer = true;
                NPC.velocity *= 0.96f;
            }
            if (AI_Timer == StartTime + 50 && PlungeCount < 1f && WorldSavingSystem.MasochistModeReal && !PhaseOne)
            {
                AI_Timer = 0f;
                PlungeCount += 1f;
                NPC.netUpdate = true;
            }
            float neutralTime = 100;
            if (AI_Timer == StartTime + neutralTime)
            {
                RuneFormation = Formations.Circle;
                RuneFormationTimer = 0;
                NPC.netUpdate = true;
            }
            if (AI_Timer > StartTime + neutralTime) // fly back up
            {
                //Vector2 desiredPos = Player.Center + Player.DirectionTo(NPC.Center) * 200;
                float offset = NPC.Center.X - Player.Center.X;
                offset = MathHelper.Clamp(offset, -200, 200);
                Vector2 desiredPos = new(Player.Center.X + offset, Player.Center.Y - 300f);
                FlyingState(2f, false, desiredPos, orient: true);

                HitPlayer = false;
                Flying = true;
                Charging = false;
                
            }
            if (AI_Timer >= StartTime + neutralTime + FormationTime + 10)
            {
                HitPlayer = false;
                StateReset();
            }
        }

        public void RuneSpear()
        {
            Player player = Main.player[NPC.target];

            int StartTime = WorldSavingSystem.EternityMode ? 75 : 100;
            float stabTime = 60;
            float AttackTime = stabTime + 20;

            // phase 2
            float middleTime = -35;
            float throwTelegraphTime = 50;
            float throwTime = 30;
            float retractTime = 65;
            float extraTime = 10;

            if (!PhaseOne)
                AttackTime += middleTime + throwTelegraphTime + throwTime + retractTime + extraTime;

            if (AttackF1)
            {
                LockVector3 = NPC.Center;
                AttackF1 = false;
                NPC.netUpdate = true;
                Flying = true;

                SoundEngine.PlaySound(ScreechSound1, NPC.Center);

                RuneFormation = Formations.Spear;
                RuneFormationTimer = 0;

                GunRotation = NPC.DirectionTo(player.Center).ToRotation();

            }
            AuraCenter = LockVector3;

            NPC.damage = 0; // (int)(NPC.defDamage * 0.75f);

            if (AI_Timer < StartTime) // prep, get in position
            {
                HitPlayer = false;
                float directionToPlayer = NPC.HorizontalDirectionTo(player.Center);

                Vector2 desiredPos = player.Center - Vector2.UnitY * 200 - Vector2.UnitX * directionToPlayer * 240;
                FlyingState(1.5f, false, desiredPos, orient: true);

                GunRotation = NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2; // MathHelper.Lerp(GunRotation, NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2, 0.4f);

                Vector2 forward = (GunRotation - MathHelper.PiOver2).ToRotationVector2();
                LockVector1 = forward.RotatedBy(-directionToPlayer * MathHelper.PiOver2) * ChunkDistance - forward * 100;

                //triangulation to find spear direction
                LockVector2 = (NPC.Center + LockVector1).DirectionTo(player.Center);

                if (AI_Timer == FormationTime)
                {
                    float extraMinus = PhaseOne ? 0 : retractTime;
                    if (FargoSoulsUtil.HostCheck)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<LifeRunespearHitbox>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 0f, Main.myPlayer, NPC.whoAmI, AttackTime + (StartTime - FormationTime) - extraMinus);
                    
                    if (WorldSavingSystem.MasochistModeReal)
                    {
                        SoundEngine.PlaySound(SoundID.Item91, NPC.Center);
                        if (FargoSoulsUtil.HostCheck)
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, (Vector2.UnitY - NPC.DirectionTo(player.Center)).SafeNormalize(Vector2.UnitY) * 10f, ModContent.ProjectileType<LifeNuke>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 3f, Main.myPlayer, 32f, 0);
                    }
                }

            }
            else // rest of attack
            {
                if (AI_Timer == StartTime)
                    SoundEngine.PlaySound(DashSound1, NPC.Center);

                NPC.velocity *= 0.965f;
                float rotationIncrement = WorldSavingSystem.EternityMode ? 0.007f : 0f;
                NPC.velocity = NPC.velocity.RotateTowards(NPC.DirectionTo(player.Center).ToRotation(), rotationIncrement);
                GunRotation = GunRotation.ToRotationVector2().RotateTowards(NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2, rotationIncrement).ToRotation();

                float attackTimer = AI_Timer - StartTime;

                HitPlayer = false;
                if (attackTimer < stabTime)
                {
                    HitPlayer = true;
                    float stabModifier = 1f - (attackTimer / stabTime);
                    stabModifier = MathF.Sqrt(stabModifier);

                    LockVector2 = LockVector2.RotateTowards((NPC.Center + LockVector1).DirectionTo(player.Center).ToRotation(), rotationIncrement * 2f * stabModifier);

                    LockVector1 += LockVector2 * 2f * stabModifier;
                }

                if (!PhaseOne)
                {
                    float throwSpeed = 21f;

                    if (attackTimer > stabTime + middleTime)
                    {
                        float throwTelegraphTimer = attackTimer - (stabTime + middleTime);
                        //if (throwTelegraphTimer == 1)
                        //SoundEngine.PlaySound(ScreechSound1, NPC.Center);
                        if (throwTelegraphTimer < throwTelegraphTime)
                        {
                            HitPlayer = false;
                            GunRotation = MathHelper.Lerp(GunRotation, NPC.DirectionTo(player.Center).ToRotation() + MathHelper.PiOver2, 1f);

                            Vector2 forward = (GunRotation - MathHelper.PiOver2).ToRotationVector2();

                            bool predictive = false; // WorldSavingSystem.MasochistModeReal;
                            Vector2 aim = predictive ? FargoSoulsUtil.PredictiveAim(NPC.Center + LockVector1, player.Center, player.velocity, throwSpeed) : (NPC.Center + LockVector1).DirectionTo(player.Center);

                            float direction = MathF.Sign(aim.X);

                            float pullback = 175f;
                            if (throwTelegraphTimer / throwTelegraphTime > 0.8f)
                                pullback = MathHelper.Lerp(175f, -50f, ((throwTelegraphTimer / throwTelegraphTime) - 0.8f) / 0.2f);
                            Vector2 desiredVector1 = forward.RotatedBy(-direction * MathHelper.PiOver2) * ChunkDistance - forward * pullback;
                            Vector2 desiredVector2 = aim.SafeNormalize(Vector2.UnitX);

                            LockVector1 = Vector2.Lerp(LockVector1, desiredVector1, 0.1f);
                            //triangulation to find spear direction
                            LockVector2 = LockVector2.RotateTowards(desiredVector2.ToRotation(), 0.07f);


                            Vector2 desiredPos = player.Center + player.DirectionTo(NPC.Center) * 300f;
                            FlyingState(3f, false, desiredPos, orient: true);
                        }
                        else
                        {
                            //Vector2 desiredPos = player.Center + player.DirectionTo(NPC.Center) * 150f;
                            //FlyingState(0.5f, false, desiredPos, orient: true);

                            float throwTimer = throwTelegraphTimer - throwTelegraphTime;
                            if (throwTimer < throwTime)
                            {
                                HitPlayer = true;
                                if (throwTimer == 1)
                                    SoundEngine.PlaySound(DashSound1 with { Pitch = -0.2f }, NPC.Center);
                                LockVector1 += LockVector2 * throwSpeed;

                                //if (WorldSavingSystem.MasochistModeReal)
                                    //LockVector2 = LockVector2.RotateTowards((NPC.Center + LockVector1).DirectionTo(player.Center).ToRotation(), 0.01f);
                            }
                            else
                            {
                                float retractTimer = throwTimer - throwTime;
                                if (retractTimer <= 0)
                                {
                                    SoundEngine.PlaySound(RuneSound1, NPC.Center);

                                    RuneFormation = Formations.Scattered;
                                    RuneFormationTimer = FormationTime;

                                    Vector2 spearHilt = NPC.Center + LockVector1;
                                    Vector2 spearDirection = LockVector2;

                                    float totalLength = LifeRunespearHitbox.Length;
                                    float spearRunes = RuneCount;// - 2;

                                    for (int i = 0; i < RuneCount; i++)
                                    {
                                        CustomRunePositions[i] = spearHilt + spearDirection * (totalLength / spearRunes) * i;
                                        if (FargoSoulsUtil.HostCheck)
                                            Projectile.NewProjectile(NPC.GetSource_FromThis(), CustomRunePositions[i], Vector2.Zero, ModContent.ProjectileType<LifeRuneRetractHitbox>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 3f, Main.myPlayer, NPC.whoAmI, i, ai2: retractTime + 10);
                                    }
                                    NPC.netUpdate = true;

                                    foreach (Projectile proj in Main.projectile.Where(p => p.TypeAlive<LifeRunespearHitbox>()))
                                        proj.Kill();


                                }
                                else
                                {
                                    float progress = retractTimer / retractTime;

                                    for (int i = 0; i < RuneCount; i++)
                                    {
                                        //float drawRot = (float)(BodyRotation + Math.PI * 2 / RuneCount * i);
                                        //Vector2 circlePos = NPC.Center + drawRot.ToRotationVector2() * RuneDistance;


                                        if (NPC.Distance(CustomRunePositions[i]) < RuneDistance + 20)
                                        {
                                            float drawRot = (float)(BodyRotation + Math.PI * 2 / RuneCount * i);
                                            Vector2 circlePos = NPC.Center + drawRot.ToRotationVector2() * RuneDistance;
                                            CustomRunePositions[i] = Vector2.Lerp(CustomRunePositions[i], circlePos, 0.1f);
                                        }
                                        else
                                        {
                                            int side = i % 2 == 0 ? 1 : -1;
                                            Vector2 dir = NPC.DirectionTo(CustomRunePositions[i]);

                                            float runeLerp = (i + 1) / (float)RuneCount;
                                            float mod = 0.5f + MathF.Pow(runeLerp, 3f);
                                            float mod2 = 1f + runeLerp * 0.2f;

                                            float vel = throwSpeed - progress * throwSpeed * 2 * mod2;

                                            CustomRunePositions[i] += dir * vel;
                                            CustomRunePositions[i] += dir.RotatedBy(MathHelper.PiOver2 * side) * 24f * mod * (0.3f + progress);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
            }   
                
            Flying = false;

            // little stab charge
            int lerpTime = 10;
            if (AI_Timer >= StartTime && AI_Timer <= StartTime + lerpTime)
            {
                float progress = (float)(AI_Timer - StartTime) / lerpTime;
                NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Lerp(LockVector2, NPC.DirectionTo(player.Center), 0.5f) * 22, progress);
            }

            // reset
            if (AI_Timer >= StartTime + AttackTime)
            {
                HitPlayer = false;
                StateReset();
            }
        }
        
        public void CrystallineCongregation()
        {
            ref float SlurpTimer = ref NPC.ai[2];
            ref float BurpTimer = ref NPC.ai[3];

            Player Player = Main.player[NPC.target];
            if (AttackF1)
            {
                //only do attack when in range
                //this had bugs and is currently disabled, may be changed in the future
                //update: this has been replaced with not doing the attack in the first place if too far away (conditional attacks baybeee)

                //Vector2 targetPos = Player.Center;
                //targetPos.Y -= 16 * 15;
                //if (NPC.Distance(targetPos) < 18 * 10 || WorldSavingSystem.MasochistModeReal)

                AttackF1 = false;
                NPC.netUpdate = true;

                RuneFormation = Formations.Circle;
                RuneFormationTimer = 0;
            }

            Vector2 desiredPos = Player.Center + Player.DirectionTo(NPC.Center) * 360;
            float speed = 0.5f;
            if (NPC.Distance(Player.Center) > 800)
                speed = 1f;
            FlyingState(speed, false, desiredPos);
            if (AI_Timer > 240f)
                speed *= 0.4f;

            if (AI_Timer <= 240f)
            {
                float delay = 12f;
                if (WorldSavingSystem.MasochistModeReal)
                    delay = 10f;
                if (AI_Timer % delay == 0)
                {
                    float predict = 160;
                    if (WorldSavingSystem.MasochistModeReal)
                        predict = 200;
                    Vector2 predictedPos = Player.Center + Player.velocity * predict;
                    float distance = MathF.Max(NPC.Distance(Player.Center), NPC.Distance(Player.Center + Player.velocity * 45));
                    Vector2 dir = NPC.DirectionTo(predictedPos);
                    //float dif = FargoSoulsUtil.RotationDifference(dir, NPC.DirectionTo(Player.Center));
                    //dir = dir.RotatedBy(-Math.Sign(dif) * MathHelper.PiOver2 * 0.1f);
                    Vector2 crystalPos = NPC.Center + dir * (distance + 350) + Main.rand.NextVector2Circular(90f, 90f);
                    SoundEngine.PlaySound(SoundID.Item101, crystalPos);
                    if (FargoSoulsUtil.HostCheck)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), crystalPos, Vector2.Zero, ModContent.ProjectileType<LifeSlurp>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 3f, Main.myPlayer, 0, NPC.whoAmI);
                    }
                }
            }
            if (AI_Timer > 240f && AI_Timer < 360 && AI_Timer % 40 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item43 with { Volume = 1f, MaxInstances = 10 }, NPC.Center);
                if (FargoSoulsUtil.HostCheck)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<GlowRingHollow>(), 0, 0f, Main.myPlayer, 11, 100, ai2: NPC.whoAmI);
            }
            if (AI_Timer == 360) // burst
            {
                int count = 0;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.TypeAlive<LifeSlurp>() && p.As<LifeSlurp>().lifelight.whoAmI == NPC.whoAmI)
                    {
                        p.ai[1] = 1000 - count * 2;
                        p.ai[2] = count;
                        count++;
                    }
                }
            }
            int endTime = 430;

            /*
            if (AI_Timer > 300f && AI_Timer < endTime)
            {
                if (BurpTimer > 15f)
                {
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, NPC.Center);
                    BurpTimer = 0f;
                }
                BurpTimer += 1f;
            }

            if (!WorldSavingSystem.MasochistModeReal && AI_Timer < 120)
            {
                SlurpTimer -= 0.5f;
                BurpTimer -= 0.5f;
            }
            */

            if (AI_Timer >= endTime)
            {
                StateReset();
            }
        }
        public void FTWDesperation()
        {
            ref float RandomFloat = ref NPC.ai[0];

            Player Player = Main.player[NPC.target];

            NPC.chaseable = false;

            if (Main.LocalPlayer.active && !Main.LocalPlayer.dead && !Main.LocalPlayer.ghost && NPC.Distance(Main.LocalPlayer.Center) < 3000)
            {
                if (Main.LocalPlayer.grapCount > 0)
                    Main.LocalPlayer.RemoveAllGrapplingHooks();

                Main.LocalPlayer.controlUseItem = false;
                Main.LocalPlayer.controlUseTile = false;
                Main.LocalPlayer.FargoSouls().NoUsingItems = 2;
            }
            if (AttackF1)
            {
                AttackF1 = false;
                NPC.netUpdate = true;
                Flying = true;
            }

            FlyingState();

            //for (int i = 0; i < Main.musicFade.Length; i++) //shut up music
            //    if (Main.musicFade[i] > 0f)
            //        Main.musicFade[i] -= 1f / 60;
            const int InitTime = 120;

            if (AI_Timer == 0 && FargoSoulsUtil.HostCheck) // cage size is 600x600, 300 from center, 25 projectiles per side, 24x24 each
            {
                Projectile.NewProjectile(NPC.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<LifeCageTelegraph>(), 0, 0f, Main.myPlayer, ai1: Player.whoAmI);
            }
            if (AI_Timer == InitTime)
            {
                SoundEngine.PlaySound(SoundID.DD2_DefenseTowerSpawn, Player.Center);
                for (int i = 0; i < 26; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        if (FargoSoulsUtil.HostCheck)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(Player.Center.X - 300 + 600 * j, Player.Center.Y - 300 + 24 * i), Vector2.Zero, ModContent.ProjectileType<LifeCageProjectile>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, j);
                            Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(Player.Center.X - 300 + 24 * i, Player.Center.Y - 300 + 600 * j), Vector2.Zero, ModContent.ProjectileType<LifeCageProjectile>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, 2 + j);
                        }
                    }
                }
                /*if (FargoSoulsUtil.HostCheck) //bars
                {
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<LifeCageBars>(), 0, 0, Main.myPlayer);
                }*/
                LockVector1 = Player.Center;
                NPC.netUpdate = true;
            }

            if (AI_Timer > InitTime) //make sure to teleport any player outside the cage inside
            {
                if (Main.LocalPlayer.active && (Math.Abs(Main.LocalPlayer.Center.X - LockVector1.X) > 320 || Math.Abs(Main.LocalPlayer.Center.Y - LockVector1.Y) > 320) && Main.LocalPlayer.active && (Math.Abs(Main.LocalPlayer.Center.X - LockVector1.X) < 1500 || Math.Abs(Main.LocalPlayer.Center.Y - LockVector1.Y) < 1500))
                {
                    Main.LocalPlayer.position = LockVector1;
                }
            }
            #region GridShots (removed)
            const int Attack1Start = InitTime + 40;
            const int Attack1End = Attack1Start;
            #endregion
            #region BulletHell
            //start of shooting attack: cum god fires a nuke or two straight up while he shoots slow shots straight down from him
            const int Attack2Time = 25;
            const int Attack2Start = Attack1End + 60;
            const int Attack2End = Attack2Start + 60 * 8;
            int time2 = (int)AI_Timer - Attack2Start;

            if (AI_Timer > Attack2Start && time2 % (Attack2Time * 3) + 1 == 1 && AI_Timer < Attack2End) //cum nuke up
            {
                if (FargoSoulsUtil.HostCheck)
                    for (int i = 0; i < 2; i++)
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(-4 + 8 * i, -2f), ModContent.ProjectileType<LifeNuke>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, 24f);
            }
            if (AI_Timer > Attack2Start && time2 % (Attack2Time * 2) + 1 == 1 && AI_Timer < Attack2End) //fire shots down
            {
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, NPC.Center);
                if (FargoSoulsUtil.HostCheck)
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(0, 2.5f), ModContent.ProjectileType<LifeProjLarge>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer);
            }
            #endregion

            #region Blaster1
            //GASTER BLASTER 1
            const int Attack3Time = 90;
            const int Attack3Start = Attack2End + 60;
            const int Attack3End = Attack3Start + Attack3Time * 8;
            int time5 = (int)AI_Timer - Attack3Start;
            if (AI_Timer >= Attack3Start && time5 % Attack3Time + 1 == 1 && AI_Timer < Attack3End) // get random angle
            {
                RandomFloat = Main.rand.Next(-90, 90);
                NPC.netUpdate = true;
            }
            if (AI_Timer >= Attack3Start && time5 % Attack3Time + 1 == Attack3Time && AI_Timer < Attack3End) // spawn blasters
            {
                Vector2 aim = new(0, 450);
                if (firstblaster < 1 || firstblaster > 1)
                    SoundEngine.PlaySound(SoundID.Item92, NPC.Center);
                for (int i = 0; i <= 12; i++)
                {
                    if (FargoSoulsUtil.HostCheck && (firstblaster < 1 || firstblaster > 1))
                    {
                        Vector2 vel = -Vector2.Normalize(aim).RotatedBy(i * MathHelper.Pi / 6 + MathHelper.ToRadians(NPC.ai[0]));
                        float ai0 = vel.ToRotation();
                        Projectile.NewProjectile(NPC.GetSource_FromThis(), LockVector1 + aim.RotatedBy(i * MathHelper.Pi / 6 + MathHelper.ToRadians(NPC.ai[0])), Vector2.Zero, ModContent.ProjectileType<LifeBlaster>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, ai0, firstblaster);
                    }
                }
                if (firstblaster > 0)
                    firstblaster -= 1;
                NPC.netUpdate = true;

            }
            #endregion
            #region Blaster2
            //GASTER BLASTER 2 FINAL BIG SPIN FINAL CUM GOD DONE DUN DEAL
            const int Attack4Time = 4;
            const int Attack4Start = Attack3End + 90;
            const int Attack4End = Attack4Start + 180 * 5; //2 seconds per rotation
            int time6 = (int)AI_Timer - Attack4Start;
            if (AI_Timer >= Attack4Start && time6 == 0) // reset NPC.ai[0]
            {
                RandomFloat = 0;
                NPC.netUpdate = true;
                LockVector2 = Player.Center;
            }

            if (AI_Timer > Attack4Start && time5 % Attack4Time == Attack4Time - 1 && AI_Timer < Attack4End) // spawn blasters. 1 every 4th frame, 2 seconds per rotation, 45 total
            {
                SoundEngine.PlaySound(SoundID.Item92, NPC.Center);
                Vector2 aim = (Vector2.Normalize(LockVector2 - LockVector1) * 550).RotatedBy(MathHelper.PiOver2);
                if (FargoSoulsUtil.HostCheck)
                {
                    Vector2 vel = -Vector2.Normalize(aim).RotatedBy(RandomFloat * MathHelper.Pi / 18);
                    float ai0 = vel.ToRotation();
                    Projectile.NewProjectile(NPC.GetSource_FromThis(), LockVector1 + aim.RotatedBy(RandomFloat * MathHelper.Pi / 18), Vector2.Zero, ModContent.ProjectileType<LifeBlaster>(), FargoSoulsUtil.ScaledProjectileDamage(NPC.damage), 3f, Main.myPlayer, ai0);
                }
                NPC.netUpdate = true;
                RandomFloat += 1;
            }
            #endregion
            #region End
            int end = Attack4End + 120;
            if (AI_Timer >= end)
            {
                NPC.dontTakeDamage = false;
                SoundEngine.PlaySound(SoundID.ScaryScream, NPC.Center);
            }
            if (AI_Timer >= end && AI_Timer % 10 == 0)
            {
                for (int i = 0; i < 50; i++)
                {
                    int DustType = Main.rand.NextFromList(DustID.YellowTorch, DustID.PinkTorch, DustID.UltraBrightTorch);
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustType);
                }

                NPC.position = NPC.position + new Vector2(Main.rand.Next(-60, 60), Main.rand.Next(-60, 60));
                NPC.netUpdate = true;
                SoundEngine.PlaySound(SoundID.Item84, NPC.Center);
            }
            if (AI_Timer == end + 90)
            {
                AI_Timer = -200;
                NPC.life = 0;
                NPC.checkDead();
                //there was dialogue here before
            }
            #endregion
        }
        #endregion
        #endregion
        #region Overrides
        public override bool CheckDead()
        {
            if (!Main.getGoodWorld)
                return base.CheckDead();
            if ((States)State == States.FTWDesperation && AI_Timer < -100)
                return base.CheckDead();
            NPC.life = 1;
            NPC.active = true;
            return false;
        }
        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {
            if (phaseProtectionDR)
                modifiers.FinalDamage /= 4f;
            else if (useDR)
                modifiers.FinalDamage /= 2.5f;
        }
        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            boundingBox = NPC.Hitbox;
        }
        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (projectile.type == ModContent.ProjectileType<DecrepitAirstrikeNuke>() || projectile.type == ModContent.ProjectileType<DecrepitAirstrikeNukeSplinter>())
            {
                modifiers.FinalDamage *= 0.75f;
            }
        }

        #region Hitbox
        public override bool CanHitPlayer(Player target, ref int CooldownSlot)
        {
            if (HitPlayer)
            {
                Vector2 boxPos = target.position;
                Vector2 boxDim = target.Size;
                return Collides(boxPos, boxDim);
            }
            return false;
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            if (WorldSavingSystem.EternityMode)
            {
                target.AddBuff(ModContent.BuffType<SmiteBuff>(), 60 * 4);
            }
        }
        public override bool CanHitNPC(NPC target)
        {
            if (HitPlayer)
            {
                Vector2 boxPos = target.position;
                Vector2 boxDim = target.Size;
                return Collides(boxPos, boxDim);
            }
            return false;
        }
        public bool Collides(Vector2 boxPos, Vector2 boxDim)
        {
            //circular hitbox-inator
            Vector2 ellipseDim = NPC.Size;
            Vector2 ellipseCenter = NPC.position + 0.5f * new Vector2(NPC.width, NPC.height);

            if (PyramidPhase == 1)
                ellipseDim /= 2;

            float x = 0f; //ellipse center
            float y = 0f; //ellipse center
            if (boxPos.X > ellipseCenter.X)
            {
                x = boxPos.X - ellipseCenter.X; //left corner
            }
            else if (boxPos.X + boxDim.X < ellipseCenter.X)
            {
                x = boxPos.X + boxDim.X - ellipseCenter.X; //right corner
            }
            if (boxPos.Y > ellipseCenter.Y)
            {
                y = boxPos.Y - ellipseCenter.Y; //top corner
            }
            else if (boxPos.Y + boxDim.Y < ellipseCenter.Y)
            {
                y = boxPos.Y + boxDim.Y - ellipseCenter.Y; //bottom corner
            }
            float a = ellipseDim.X / 2f;
            float b = ellipseDim.Y / 2f;

            return x * x / (a * a) + y * y / (b * b) < 1; //point collision detection
        }
        #endregion
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0 && CheckDead())
            {
                for (int i = 0; i < 400; i++)
                {
                    Color color = Main.rand.NextFromList(Color.Goldenrod, Color.Pink, Color.Cyan);
                    Particle p = new SmallSparkle(
                        worldPosition: NPC.Center,
                        velocity: (Main.rand.NextFloat(5, 50) * Vector2.UnitX).RotatedByRandom(MathHelper.TwoPi),
                        drawColor: color,
                        scale: 1f,
                        lifetime: Main.rand.Next(20, 80),
                        rotation: 0,
                        rotationSpeed: Main.rand.NextFloat(-MathHelper.Pi / 8, MathHelper.Pi / 8)
                        );
                    p.Spawn();
                    p.Position -= p.Velocity * 4; //implosion
                }
                return;
            }
            else
            {
                if (NPC.GetLifePercent() < (float)chunklist.Count / (float)ChunkCount && State != (int)States.Opening && PyramidPhase == 0) //not during opening or pyramid attacks
                {
                    if (chunklist.Count <= 0)
                    {
                        return;
                    }
                    int i = Main.rand.Next(chunklist.Count);
                    Vector4 chunk = chunklist[i];
                    Vector2 pos = chunk.X * Vector2.UnitX * ChunkDistance + chunk.Y * Vector2.UnitY * ChunkDistance;// + chunk.Z * Vector3.UnitZ;
                    if (!Main.dedServ)
                        Gore.NewGore(NPC.GetSource_FromThis(), NPC.Center + pos, NPC.velocity, ModContent.Find<ModGore>(Mod.Name, $"ShardGold{chunk.W}").Type, NPC.scale);
                    chunklist.RemoveAt(i);
                    SoundEngine.PlaySound(SoundID.Tink, NPC.Center);
                }
            }
        }
        public const int ChunkCount = 10 * 5;
        public const int RuneCount = 12;
        const int ChunkSpriteCount = 12;
        internal struct Rune
        {
            public Rune(Vector3 center, int index, float scale, float rotation)
            {
                Center = center;
                Index = index;
                Scale = scale;
                Rotation = rotation;
            }
            public Vector3 Center;
            public int Index;
            public float Scale;
            public float Rotation;
        }
        List<Rune> PostdrawRunes = [];
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            const float ChunkRotationSpeed = MathHelper.TwoPi * (1f / 360);

            Vector2 drawCenter = NPC.Center - screenPos;

            for (int i = 0; i < chunklist.Count; i++)
            {
                chunklist[i] = RotateByMatrix(chunklist[i], ChunkRotationSpeed, Vector3.UnitX);
                chunklist[i] = RotateByMatrix(chunklist[i], -ChunkRotationSpeed / 2, Vector3.UnitZ);
                chunklist[i] = RotateByMatrix(chunklist[i], -ChunkRotationSpeed / 4, Vector3.UnitY);

            }
            chunklist.Sort(delegate (Vector4 x, Vector4 y)
            {
                return Math.Sign(x.Z - y.Z);
            });
            foreach (Vector4 chunk in chunklist.Where(pos => pos.Z <= 0))
            {
                DrawChunk(chunk, spriteBatch, drawColor);
            }
            List<Rune> PredrawRunes = [];
            PostdrawRunes = [];
            if (Draw || NPC.IsABestiaryIconDummy)
            {
                if (DrawRunes)
                {

                    for (int i = 0; i < RuneCount; i++)
                    {

                        float drawRot = (float)(BodyRotation + Math.PI * 2 / RuneCount * i);

                        Vector2 circlePos = drawCenter + drawRot.ToRotationVector2() * RuneDistance;


                        float runeRotation = drawRot + MathHelper.PiOver2;
                        Vector2 drawPos;
                        float runeScale = NPC.scale;
                        float runeLerp = (drawRot % MathF.Tau) / MathF.Tau;
                        float z = 0;

                        Vector2 GetFormationPosition(int formation)
                        {
                            switch (formation)
                            {
                                case Formations.Shield:
                                    {
                                        float rotation = GunRotation;

                                        float rotationForward = rotation;
                                        Vector2 spearPoint = drawCenter + rotationForward.ToRotationVector2() * DefaultRuneDistance * 1.2f;
                                        float spearRuneLerp = runeLerp - 0.5f;
                                        Vector2 spearOffset = Vector2.Zero;
                                        const float SpearAngle = MathF.PI * 0.13f;
                                        float spearLength = NPC.width * 1.2f;
                                        z = Math.Abs(spearRuneLerp);
                                        spearOffset = -(rotation - (SpearAngle * MathHelper.Lerp(1f, 3f, MathF.Pow(Math.Abs(spearRuneLerp), 1.5f)) * Math.Sign(spearRuneLerp))).ToRotationVector2() * spearLength * Math.Abs(spearRuneLerp);
                                        Vector2 spearPos = spearPoint + spearOffset;

                                        return spearPos;
                                    }
                                case Formations.Gun:
                                case Formations.GunCloser:
                                    {
                                        float gunRot = drawRot;
                                        Vector2 circleCenter = drawCenter + GunCircleCenter(1) - NPC.Center; // remove duplicate NPC.Center
                                        float circleRadius = 90;
                                        if (formation == Formations.GunCloser)
                                            circleRadius += 40;
                                        float deformMult = MathF.Pow(Math.Abs(gunRot.ToRotationVector2().X), 1);
                                        circleRadius *= MathHelper.Lerp(1, 0.6f, deformMult); // circle deformation
                                        Vector2 runeCircleOffset = (GunRotation + gunRot).ToRotationVector2() * circleRadius;
                                        Vector2 gunPos = circleCenter + runeCircleOffset;

                                        float rot = (gunRot + MathF.PI / 2) % MathF.Tau;
                                        float scaleMult = (rot.ToRotationVector2().Y + 1) / 2;
                                        runeScale *= MathHelper.Lerp(1.3f, 0.7f, scaleMult);
                                        z = scaleMult;

                                        return gunPos;
                                    }
                                case Formations.Scattered:
                                    {
                                        runeRotation = MathHelper.SmoothStep(runeRotation, 0, FormationLerp);
                                        return CustomRunePositions[i] - screenPos;
                                    }
                                case Formations.Spear:
                                    {
                                        Vector2 spearHilt = drawCenter + LockVector1;
                                        Vector2 spearDirection = LockVector2;

                                        float totalLength = LifeRunespearHitbox.Length;
                                        float spearRunes = RuneCount;// - 2;
                                        Vector2 spearPos = spearHilt + spearDirection * (totalLength / spearRunes) * i;
                                        /*
                                        if (i < 2)
                                        {
                                            float dir = i == 0 ? 1 : -1;
                                            spearPos = spearHilt + spearDirection * (totalLength / spearRunes) * (spearRunes + 0.2f) + spearDirection.RotatedBy(MathHelper.PiOver2 * dir) * 14;
                                        }
                                        */

                                        return spearPos;
                                    }
                                default:
                                    {
                                        return circlePos;
                                    }
                            };
                        }
                        Vector2 previousPos = GetFormationPosition(OldRuneFormation);
                        Vector2 newPos = GetFormationPosition(RuneFormation);
                        drawPos = Vector2.SmoothStep(previousPos, newPos, FormationLerp);

                        if (z <= 0)
                        {
                            PredrawRunes.Add(new(new Vector3(drawPos.X, drawPos.Y, z), i, runeScale, runeRotation));
                        }
                        else
                        {
                            PostdrawRunes.Add(new(new Vector3(drawPos.X, drawPos.Y, z), i, runeScale, runeRotation));
                        }
                    }
                }
                if (PredrawRunes.Count != 0)
                {
                    PredrawRunes.Sort(delegate (Rune x, Rune y)
                    {
                        return Math.Sign(x.Center.Z - y.Center.Z);
                    });
                    foreach (Rune rune in PredrawRunes)
                    {
                        DrawRune(rune, spriteBatch, drawColor);
                    }
                }

                //draw arena runes
                if (DoAura)
                {
                    const int AuraRuneCount = 48;
                    for (int i = 0; i < AuraRuneCount; i++)
                    {
                        float rotation = (RuneFormation == Formations.Gun || OldRuneFormation == Formations.Gun) ? BodyRotation / MathHelper.Lerp(1, 3, FormationLerp) : BodyRotation;
                        float drawRot = (float)(rotation + Math.PI * 2 / AuraRuneCount * i);
                        AtlasTexture RuneTexture = GetTexture("FargowiltasSouls." + $"Rune{(i % RuneCount) + 1}");
                        Vector2 drawPos = AuraCenter + drawRot.ToRotationVector2() * (1100 + RuneDistance) - screenPos;
                        float RuneRotation = drawRot + MathHelper.PiOver2;

                        //rune glow
                        for (int j = 0; j < AuraRuneCount; j++)
                        {
                            Vector2 afterimageOffset = (MathHelper.TwoPi * j / 12f).ToRotationVector2() * 1f;
                            Color glowColor;

                            if (i % 3 == 0) //cyan
                                glowColor = new Color(0f, 1f, 1f, 0f) * 0.7f;
                            else if (i % 3 == 1) //yellow
                                glowColor = new Color(1f, 1f, 0f, 0f) * 0.7f;
                            else //pink
                                glowColor = new Color(1, 192 / 255f, 203 / 255f, 0f) * 0.7f;

                            Utilities.Draw(spriteBatch, RuneTexture, drawPos + afterimageOffset, null, glowColor, RuneRotation, RuneTexture.Size * 0.5f, new Vector2(NPC.scale), SpriteEffects.None);
                        }                       
                        Utilities.Draw(spriteBatch, RuneTexture, drawPos, null, Color.White, RuneRotation, new Vector2(RuneTexture.Width / 2, RuneTexture.Height / 2), new Vector2(NPC.scale), SpriteEffects.None);
                    }
                }

                //draw wings
                //draws 4 things: 2 upper wings, 2 lower wings
                if (ChunkDistance > 3)
                {
                    AtlasTexture wingUtexture = AtlasManager.GetTexture("FargowiltasSouls.Lifelight_WingUpper");
                    AtlasTexture wingLtexture = AtlasManager.GetTexture("FargowiltasSouls.Lifelight_WingLower");
                    Vector2 wingdrawPos = NPC.Center - screenPos;
                    int currentFrame = NPC.frame.Y;
                    int wingUHeight = wingUtexture.Height / Main.npcFrameCount[NPC.type];
                    Rectangle wingURectangle = new(0, currentFrame * wingUHeight, wingUtexture.Width, wingUHeight);
                    int wingLHeight = wingLtexture.Height / Main.npcFrameCount[NPC.type];
                    Rectangle wingLRectangle = new(0, currentFrame * wingLHeight, wingLtexture.Width, wingLHeight);
                    Vector2 wingUOrigin = new(wingUtexture.Width / 2, wingUtexture.Height / 2 / Main.npcFrameCount[NPC.type]);
                    Vector2 wingLOrigin = new(wingLtexture.Width / 2, wingLtexture.Height / 2 / Main.npcFrameCount[NPC.type]);

                    float distance = ChunkDistance;
                    if (RuneFormation == Formations.Shield || OldRuneFormation == Formations.Shield)
                        distance /= MathHelper.Lerp(1, 1.5f, MathHelper.Clamp((float)RuneFormationTimer / FormationTime, 0, 1));

                    for (int i = -1; i < 2; i += 2)
                    {
                        float wingLRotation = NPC.rotation - MathHelper.PiOver2 + MathHelper.ToRadians(110 * i);
                        float wingURotation = NPC.rotation - MathHelper.PiOver2 + MathHelper.ToRadians(70 * i);
                        SpriteEffects flip = i == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
                     
                        Utilities.Draw(spriteBatch, wingUtexture, wingdrawPos + wingURotation.ToRotationVector2() * (distance + 30), wingURectangle, drawColor, wingURotation, wingUOrigin, new Vector2(NPC.scale), flip);
                        Utilities.Draw(spriteBatch, wingLtexture, wingdrawPos + wingLRotation.ToRotationVector2() * (distance + 30), wingLRectangle, drawColor, wingLRotation, wingLOrigin, new Vector2(NPC.scale), flip);
                    }
                }


            }
            return false;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) //DRAW STAR
        {

            Vector2 drawCenter = NPC.Center - screenPos;
            if (Main.LocalPlayer.gravDir < 0)
            {
                drawCenter.Y = Main.screenHeight - drawCenter.Y;
            }
            //if ((SpritePhase > 1 || !Draw) && !NPC.IsABestiaryIconDummy) //star
            if (ChunkDistance > 20)
            {

                spriteBatch.UseBlendState(BlendState.Additive);


                Texture2D star = ModContent.Request<Texture2D>("FargowiltasSouls/Assets/Effects/LifeStar", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Rectangle rect = new(0, 0, star.Width, star.Height);
                float scale = 0.45f * Main.rand.NextFloat(1f, 2.5f);
                Vector2 origin = new(star.Width / 2 + scale, star.Height / 2 + scale);

                Vector2 pos = drawCenter;
                if (NPC.IsABestiaryIconDummy)
                {
                    pos += Vector2.UnitX * 85 + Vector2.UnitY * 48;
                }
                spriteBatch.Draw(star, pos, new Rectangle?(rect), Color.HotPink, 0, origin, scale, SpriteEffects.None, 0);
                DrawData starDraw = new(star, pos, new Rectangle?(rect), Color.HotPink, 0, origin, scale, SpriteEffects.None, 0);
                GameShaders.Misc["LCWingShader"].UseColor(Color.HotPink).UseSecondaryColor(Color.HotPink);
                GameShaders.Misc["LCWingShader"].Apply(new DrawData?());
                starDraw.Draw(spriteBatch);


                spriteBatch.ResetToDefault();
            }
            foreach (Vector4 chunk in chunklist.Where(pos => pos.Z > 0))
            {
                DrawChunk(chunk, spriteBatch, drawColor);
            }
            if (PyramidPhase != 0) //draw pyramid
            {
                float pyramidRot = LockVector1.RotatedBy(rot + MathHelper.PiOver2).ToRotation();
                if (PyramidPhase == 1 && PyramidTimer > PyramidAnimationTime)
                {
                    AtlasTexture pyramid = AtlasManager.GetTexture("FargowiltasSouls.PyramidFull");
                    Rectangle pyramidRect = new(0, 0, pyramid.Width, pyramid.Height);
                    Vector2 pyramidOrigin = pyramidRect.Size() / 2;
                    //spriteBatch.Draw(origin: pyramidOrigin, texture: pyramid.Atlas.Texture.Value, position: NPC.Center - screenPos, sourceRectangle: pyramidRect, color: drawColor, rotation: pyramidRot, scale: NPC.scale, effects: SpriteEffects.None, layerDepth: 0f);
                    Utilities.Draw(spriteBatch, pyramid, NPC.Center - screenPos, pyramidRect, drawColor, pyramidRot, pyramidOrigin, new Vector2(NPC.scale), SpriteEffects.None);
                }
                else
                {
                    AtlasTexture[] pyramidp = new AtlasTexture[4];
                    Rectangle[] rects = new Rectangle[4];
                    Vector2[] origins = new Vector2[4];
                    Vector2[] offsets = new Vector2[4];

                    pyramidp[0] = AtlasManager.GetTexture("FargowiltasSouls.Pyramid_U");
                    pyramidp[1] = AtlasManager.GetTexture("FargowiltasSouls.Pyramid_L");
                    pyramidp[2] = AtlasManager.GetTexture("FargowiltasSouls.Pyramid_R");
                    pyramidp[3] = AtlasManager.GetTexture("FargowiltasSouls.Pyramid_D");
                    float progress = 0;
                    if (PyramidPhase > 0)
                    {
                        progress = 1 - Math.Min((float)PyramidTimer / PyramidAnimationTime, 1f);
                    }
                    else if (PyramidPhase < 0)
                    {
                        progress = Math.Min((float)PyramidTimer * 4 / PyramidAnimationTime, 1f);
                    }
                    float expansion = progress;
                    byte alpha = (byte)(255 * (1 - progress));
                    offsets[0] = new Vector2(0, -15) * expansion + new Vector2(0, -20); //top
                    offsets[1] = new Vector2(-12.5f, 3) * expansion + new Vector2(-25, 10); //left
                    offsets[2] = new Vector2(12.5f, 3) * expansion + new Vector2(25, 10); //right
                    offsets[3] = new Vector2(0, 10) * expansion + new Vector2(0, 20);  //bottom

                    Color color = drawColor;
                    color.A = alpha;


                    for (int i = 0; i < 4; i++)
                    {
                        rects[i] = new Rectangle(0, 0, pyramidp[i].Width, pyramidp[i].Height);
                        origins[i] = pyramidp[i].Size / 2;
                        offsets[i] = offsets[i].RotatedBy(pyramidRot);


                        //spriteBatch.Draw(origin: origins[i], texture: pyramidp[i].Atlas.Texture.Value, position: NPC.Center + offsets[i] - screenPos, sourceRectangle: rects[i], color: color, rotation: pyramidRot, scale: NPC.scale, effects: SpriteEffects.None, layerDepth: 0f);
                        Utilities.Draw(spriteBatch, pyramidp[i], NPC.Center + offsets[i] - screenPos, rects[i], color, pyramidRot, origins[i], new Vector2(NPC.scale), SpriteEffects.None);
                    }
                }
            }
            if (PostdrawRunes.Any())
            {
                PostdrawRunes.Sort(delegate (Rune x, Rune y)
                {
                    return Math.Sign(x.Center.Z - y.Center.Z);
                });
                foreach (Rune rune in PostdrawRunes)
                {
                    DrawRune(rune, spriteBatch, drawColor);
                }
            }
        }
        AtlasTexture GetTexture(string name) => AtlasManager.GetTexture(name);
            //Main.zenithWorld || (Main.getGoodWorld && ((States)State == States.FTWDesperation
            //|| (States)State == States.P1Transition
            //|| (States)State == States.Opening))
            //? "FargowiltasSouls.Lifelight_Full" : name);
        private void DrawRune(Rune rune, SpriteBatch spriteBatch, Color drawColor)
        {
            int i = rune.Index;
            AtlasTexture RuneTexture = GetTexture("FargowiltasSouls." + $"Rune{i + 1}");
            Vector2 drawPos = new(rune.Center.X, rune.Center.Y);
            //rune glow
            for (int j = 0; j < 12; j++)
            {
                Vector2 afterimageOffset = (MathHelper.TwoPi * j / 12f).ToRotationVector2() * 1f * rune.Scale;
                Color glowColor;

                if (i % 3 == 0) //cyan
                    glowColor = new Color(0f, 1f, 1f, 0f) * 0.7f;
                else if (i % 3 == 1) //yellow
                    glowColor = new Color(1f, 1f, 0f, 0f) * 0.7f;
                else //pink
                    glowColor = new Color(1, 192 / 255f, 203 / 255f, 0f) * 0.7f;

                //Main.spriteBatch.Draw(RuneTexture.Atlas.Texture.Value, drawPos + afterimageOffset, null, glowColor, rune.Rotation, RuneTexture.Size * 0.5f, rune.Scale, SpriteEffects.None, 0f);
                Utilities.Draw(spriteBatch, RuneTexture, drawPos + afterimageOffset, null, glowColor, rune.Rotation, RuneTexture.Size * 0.5f, new Vector2(rune.Scale), SpriteEffects.None);
            }
            Color color = Color.White;
            if (RuneBlinkTimer > 0)
            {
                float modifier = RuneBlinkTimer / 40f;
                color *= modifier;
                
                Texture2D ringTexture = FargoAssets.GetTexture2D("Content/Projectiles", "GlowRing").Value;
                Rectangle ringrect = new(0, 0, ringTexture.Width, ringTexture.Height);
                Vector2 ringorigin = ringrect.Size() / 2f;
                spriteBatch.Draw(origin: ringorigin, texture: ringTexture, position: drawPos, sourceRectangle: ringrect, color: color, rotation: rune.Rotation, scale: rune.Scale * 0.4f, effects: SpriteEffects.None, layerDepth: 0f);
            }
            //spriteBatch.Draw(origin: new Vector2(RuneTexture.Width / 2, RuneTexture.Height / 2), texture: RuneTexture.Atlas.Texture.Value, position: drawPos, sourceRectangle: null, color: color, rotation: rune.Rotation, scale: rune.Scale, effects: SpriteEffects.None, layerDepth: 0f);
            Utilities.Draw(spriteBatch, RuneTexture, drawPos, null, color, rune.Rotation, new Vector2(RuneTexture.Width / 2, RuneTexture.Height / 2), new Vector2(rune.Scale), SpriteEffects.None);
        }
        private void DrawChunk(Vector4 chunk, SpriteBatch spriteBatch, Color drawColor)
        {
            if (ChunkDistance <= 20)
            {
                return;
            }
            Vector3 pos = chunk.X * Vector3.UnitX + chunk.Y * Vector3.UnitY + chunk.Z * Vector3.UnitZ;
            float scale = 0.3f * pos.Z;

            byte alpha = (byte)(150 + (100f * pos.Z));


            Color color = drawColor;
            color.A = alpha;
            Vector2 drawCenter = NPC.Center - Main.screenPosition;
            float distance = ChunkDistance;
            if (RuneFormation == Formations.Shield || OldRuneFormation == Formations.Shield)
                distance /= MathHelper.Lerp(1, 1.5f, MathHelper.Clamp((float)RuneFormationTimer / FormationTime, 0, 1));
            Vector2 chunkOffset = pos.X * Vector2.UnitX * distance + pos.Y * Vector2.UnitY * distance;
            Vector2 drawPos = drawCenter + chunkOffset;
            AtlasTexture ChunkTexture = GetTexture("FargowiltasSouls." + $"ShardGold{chunk.W}");
            //spriteBatch.Draw(origin: new Vector2(ChunkTexture.Width / 2, ChunkTexture.Height / 2), texture: ChunkTexture.Atlas.Texture.Value, position: drawPos, sourceRectangle: null, color: color, rotation: 0, scale: NPC.scale + scale, effects: SpriteEffects.None, layerDepth: 0f);
            Utilities.Draw(spriteBatch, ChunkTexture, drawPos, null, color, 0, new Vector2(ChunkTexture.Width / 2, ChunkTexture.Height / 2), new Vector2(NPC.scale + scale), SpriteEffects.None);
        }

        public float WidthFunction(float completionRatio)
        {
            float baseWidth = NPC.scale * 20;
            return MathHelper.SmoothStep(baseWidth, 3.5f, completionRatio);
        }

        public Color ColorFunction(float completionRatio)
        {
            const float LerpTime = 60;
            float timer = (Main.GameUpdateCount % (LerpTime * 3)) / LerpTime;
            timer += completionRatio * 2;
            Color color = timer switch
            {
                _ when timer <= 1 => Color.Lerp(Color.Cyan, Color.Goldenrod, timer),
                _ when timer > 1 && timer <= 2 => Color.Lerp(Color.Goldenrod, Color.DeepPink, timer - 1),
                _ => Color.Lerp(Color.DeepPink, Color.Cyan, timer - 2)
            };
            return Color.Lerp(color, Color.Transparent, completionRatio * FormationLerp) * 0.7f;
        }
        public PixelationPrimitiveLayer LayerToRenderTo => PixelationPrimitiveLayer.BeforeNPCs;
        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            if (RuneFormation != Formations.Shield)
                return;
            ManagedShader shader = ShaderManager.GetShader("FargowiltasSouls.BlobTrail");
            FargoSoulsUtil.SetTexture1(FargoAssets.FadedStreak.Value);
            PrimitiveRenderer.RenderTrail(NPC.oldPos, new(WidthFunction, ColorFunction, _ => NPC.Size * 0.5f, Pixelate: true, Shader: shader), 44);
        }

        public override void FindFrame(int frameHeight)
        {
            double fpf = 60 / 10; //  60/fps
            NPC.spriteDirection = NPC.direction;
            NPC.frameCounter += 1;
            NPC.frameCounter %= Main.npcFrameCount[NPC.type] * fpf;
            NPC.frame.Y = (int)(NPC.frameCounter / fpf);
        }
        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.Lifelight], -1);
        }
        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // I have setup the loot placement in this way because 
            // when registering loot for an npc, the bestiary checks for the order of loot registered.
            // For parity with vanilla, the order is as follows: Trophy, Classic Loot, Expert Loot, Master loot.

            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LifelightTrophy>(), 10));

            LeadingConditionRule rule = new(new Conditions.NotExpert());

            rule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<LifelightMask>(), 7));

            rule.OnSuccess(ItemDropRule.OneFromOptions(1, ModContent.ItemType<EnchantedLifeblade>(), ModContent.ItemType<Lightslinger>(), ModContent.ItemType<CrystallineCongregation>(), ModContent.ItemType<KamikazePixieStaff>()));
            rule.OnSuccess(ItemDropRule.Common(ItemID.SoulofLight, 1, 1, 3));
            rule.OnSuccess(ItemDropRule.Common(ItemID.PixieDust, 1, 15, 25));

            npcLoot.Add(rule);

            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<LifelightBag>()));

            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<LifelightRelic>()));

            npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<LifelightMasterPet>(), 4));
        }
        #endregion
        #region Help Methods

        public void ResetVariables()
        {
            AI_Timer = 0f;
            NPC.ai[2] = 0f;
            NPC.ai[3] = 0f;
            NPC.ai[0] = 0f;
            NPC.localAI[0] = 0;
            NPC.localAI[1] = 0;
            NPC.localAI[2] = 0;
            AttackF1 = true;
            NPC.netUpdate = true;
        }
        public void StateReset()
        {
            NPC.TargetClosest(true);
            
            ChooseState();
            ResetVariables();

        }
        public void GoToDeathray()
        {
            if (!WorldSavingSystem.EternityMode)
            {
                StateReset();
                return;
            }
            NPC.TargetClosest(true);
            ResetVariables();
            State = (int)States.Deathray;
        }


        public void ChooseState()
        {
            NPC.netUpdate = true;

            AttackCount++;
            int max = (!PhaseOne && WorldSavingSystem.EternityMode) ? 7 : 4;
            if (AttackCount > max)
                AttackCount = 1;

            var doableStates = new List<States>(AttackCount switch
            {
                2 => ChargeAttacks,
                4 => ChargeAttacks,
                6 => ChargeAttacks,
                7 => StrongAttacks,
                _ => PrecisionAttacks
            });

            if (PhaseOne && NPC.life < P2Threshold)
                State = (int)States.P1Transition;
            else if (Main.getGoodWorld && NPC.life < 1000)
                State = (int)States.FTWDesperation;
            else
            {
                if (FargoSoulsUtil.HostCheck)
                {
                    int i = AttackCount switch
                    {
                        2 => 0,
                        4 => 0,
                        6 => 0,
                        7 => 1,
                        _ => 2
                    };
                    if (LastAttack[i] >= 0 && doableStates.Count > 1)
                        doableStates.Remove((States)LastAttack[i]);
                    if (State == (int)States.RuneSpear)
                        doableStates.Remove(States.RuneScatter); // incompatible; both use scatter formation
                    State = (int)Main.rand.NextFromCollection(doableStates);
                    LastAttack[i] = State;

                    //State = (int)States.Plunge;
                }
            }
        }

        private static Vector4 RotateByMatrix(Vector4 obj, float radians, Vector3 axis)
        {
            Vector3 vector = obj.X * Vector3.UnitX + obj.Y * Vector3.UnitY + obj.Z * Vector3.UnitZ;
            Matrix rotationMatrix;
            if (axis == Vector3.UnitX)
            {
                rotationMatrix = Matrix.CreateRotationX(radians);
            }
            else if (axis == Vector3.UnitY)
            {
                rotationMatrix = Matrix.CreateRotationY(radians);
            }
            else
            {
                rotationMatrix = Matrix.CreateRotationZ(radians);
            }
            vector = Vector3.Transform(vector, rotationMatrix);
            obj.X = vector.X;
            obj.Y = vector.Y;
            obj.Z = vector.Z;
            return obj;
        }
        #endregion
    }
}
