using FargowiltasSouls.Assets.Particles;
using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Common.Utilities;
using FargowiltasSouls.Content.BossBars;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Items.Accessories;
using FargowiltasSouls.Content.Items.BossBags;
using FargowiltasSouls.Content.Items.Placables.Relics;
using FargowiltasSouls.Content.Items.Placables.Trophies;
using FargowiltasSouls.Content.Items.Summons;
using FargowiltasSouls.Content.Items.Vanity.Masks;
using FargowiltasSouls.Content.Items.Weapons.Challengers;
using FargowiltasSouls.Core.Systems;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.TrojanSquirrel
{
    public abstract class TrojanSquirrelPart : ModNPC
    {
        
        protected int baseWidth;
        protected int baseHeight;
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            NPCID.Sets.MPAllowedEnemies[Type] = true;

            NPCID.Sets.TrailCacheLength[Type] = 8;
            NPCID.Sets.TrailingMode[Type] = 3;

            NPC.AddDebuffImmunities(
            [
                BuffID.Confused,
                ModContent.BuffType<LethargicBuff>()
            ]);
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            NPC.damage = 24;
            NPC.defense = 2;
            NPC.HitSound = new SoundStyle("FargowiltasSouls/Assets/Sounds/Challengers/Trojan/TrojanHit") with { Variants = [1, 2, 3, 4] , Volume = 0.2f};
            NPC.DeathSound = FargosSoundRegistry.TrojanDeath;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0f;
            NPC.lavaImmune = true;
            NPC.aiStyle = -1;

            if (Main.getGoodWorld)
                NPC.scale += 1;
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            //NPC.damage = (int)(NPC.damage * 0.5f);
            NPC.lifeMax = (int)(NPC.lifeMax * balance);
        }

        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            boundingBox = NPC.Hitbox;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);

            writer.Write(NPC.scale);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);

            NPC.scale = reader.ReadSingle();
        }

        public override void PostAI()
        {
            base.PostAI();

            if (this is TrojanSquirrel)
                NPC.position = NPC.Bottom;
            NPC.width = (int)(baseWidth * NPC.scale);
            NPC.height = (int)(baseHeight * NPC.scale);
            if (this is TrojanSquirrel)
                NPC.Bottom = NPC.position;
        }
    }

    public abstract class TrojanSquirrelLimb : TrojanSquirrelPart
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            NPCID.Sets.NoMultiplayerSmoothingByType[NPC.type] = true;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;

            this.ExcludeFromBestiary();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            NPC.hide = true;
        }

        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCProjectiles.Add(index);
        }

        protected NPC body;

        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);

            if (source is EntitySource_Parent parent && parent.Entity is NPC sourceNPC)
                body = sourceNPC;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);

            writer.Write(body is NPC ? body.whoAmI : -1);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);

            body = FargoSoulsUtil.NPCExists(reader.ReadInt32());
        }

        public override bool PreAI()
        {
            if (body != null)
                body = FargoSoulsUtil.NPCExists(body.whoAmI, ModContent.NPCType<TrojanSquirrel>());

            if (body == null)
            {
                if (FargoSoulsUtil.HostCheck)
                {
                    NPC.life = 0;
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI);
                    NPC.active = false;
                }
                return false;
            }

            return base.PreAI();
        }

        public override bool CheckActive() => false;

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            base.ModifyNPCLoot(npcLoot);

            npcLoot.Add(ItemDropRule.DropNothing());
        }

        public override void FindFrame(int frameHeight)
        {
            base.FindFrame(frameHeight);

            //if (body != null)
            //    NPC.frame = body.frame;
        }
        public bool Trail => body.ai[0] == 0 && body.localAI[0] > 0; //while charging
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return true;
            
            if (body == null)
                return base.PreDraw(spriteBatch, screenPos, drawColor);

            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Npc[NPC.type].Value;
            Rectangle rectangle = NPC.frame;
            Vector2 origin2 = rectangle.Size() / 2f;

            Color color26 = drawColor;
            color26 = NPC.GetAlpha(color26);

            SpriteEffects effects = NPC.direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            if (Trail)
            {
                for (int i = 0; i < NPCID.Sets.TrailCacheLength[Type]; i++) //math.min to safeguard against uncached trail
                {
                    float oldrot = NPC.oldRot[i];
                    Vector2 oldCenter = body.oldPos[i] + body.Size / 2;
                    DrawData oldGlow = new(texture2D13, oldCenter - screenPos + new Vector2(0f, NPC.gfxOffY - 53 * body.scale), new Microsoft.Xna.Framework.Rectangle?(rectangle), color26 * (0.5f / i), oldrot, origin2, NPC.scale, effects, 0);
                    GameShaders.Misc["LCWingShader"].UseColor(Color.Blue).UseSecondaryColor(Color.Black);
                    GameShaders.Misc["LCWingShader"].Apply(oldGlow);
                    oldGlow.Draw(spriteBatch);
                }
            }


            Vector2 center = body.Center;

            Main.EntitySpriteDraw(texture2D13, center - screenPos + new Vector2(0f, NPC.gfxOffY - 53 * body.scale), new Microsoft.Xna.Framework.Rectangle?(rectangle), color26, NPC.rotation, origin2, NPC.scale, effects, 0);

            return false;
        }
    }

    [AutoloadBossHead]
    public class TrojanSquirrel : TrojanSquirrelPart
    {   
        public Vector2 bodyOffset { get; private set; }
        public static float BaseWalkSpeed => 4f;
        string TownNPCName;
        bool hasplayedbreaksound;
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            Main.npcFrameCount[Type] = 5;

            NPCID.Sets.BossBestiaryPriority.Add(NPC.type);

            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                CustomTexturePath = $"FargowiltasSouls/Content/Bosses/TrojanSquirrel/{Name}_Still",
                Position = new Vector2(16 * 4, 16 * 4),
                PortraitPositionXOverride = 16 * 1.5f,
                PortraitPositionYOverride = 16 * 3
            });
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement($"Mods.FargowiltasSouls.Bestiary.{Name}")
            ]);
        }

        public override void SetDefaults()
        {
            base.SetDefaults();

            NPC.lifeMax = 800;

            NPC.width = baseWidth = 100;
            NPC.height = baseHeight = 147; //234

            NPC.value = Item.buyPrice(silver: 75);
            NPC.boss = true;

            /*
            Music = ModLoader.TryGetMod("FargowiltasMusic", out Mod musicMod)
                ? MusicLoader.GetMusicSlot(musicMod, "Assets/Music/TrojanSquirrel") : MusicID.OtherworldlyBoss1;
            SceneEffectPriority = SceneEffectPriority.BossLow;
            */
            NPC.BossBar = ModContent.GetInstance<TrojanSquirrelBossBar>();
        }
        /*
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return base.PreDraw(spriteBatch, screenPos, drawColor);
        }
        */
        public NPC head;
        public NPC arms;
        public int lifeMaxHead;
        public int lifeMaxArms;

        //Used to create a convincing "stepping" animation.
        public static Asset<Texture2D> ArmTexture = ModContent.Request<Texture2D>("FargowiltasSouls/Content/Bosses/TrojanSquirrel/TrojanSquirrelArms", AssetRequestMode.ImmediateLoad);    
        public Rectangle LegFrameAlt = new Rectangle(0, 0, LegTexture.Width() / 5, LegTexture.Height() / LegFrameMax);
        public int LegAltFramecounter;

        public static Asset<Texture2D> LegTexture = ModContent.Request<Texture2D>("FargowiltasSouls/Content/Bosses/TrojanSquirrel/TrojanSquirrelLegs", ReLogic.Content.AssetRequestMode.ImmediateLoad);
        public int LegFrameType;
        public Rectangle LegFrame = new Rectangle(0, 0, LegTexture.Width() / 5, LegTexture.Height() / LegFrameMax);
        public const int LegFrameMax = 7;
        public float LegFramecounter;
        public int AnimationLengthOverride;

        private bool spawned;
        private bool introJump;

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);

            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);
            writer.Write(NPC.localAI[3]);
            writer.Write(head is NPC ? head.whoAmI : -1);
            writer.Write(arms is NPC ? arms.whoAmI : -1);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);

            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[2] = reader.ReadSingle();
            NPC.localAI[3] = reader.ReadSingle();
            head = FargoSoulsUtil.NPCExists(reader.ReadInt32());
            arms = FargoSoulsUtil.NPCExists(reader.ReadInt32());
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (ModContent.TryFind("Fargowiltas", "Squirrel", out ModNPC modNPC))
            {
                int n = NPC.FindFirstNPC(modNPC.Type);
                if (n != -1 && n != Main.maxNPCs)
                {
                    NPC.Bottom = Main.npc[n].Bottom;
                    TownNPCName = Main.npc[n].GivenName;

                    Main.npc[n].life = 0;
                    Main.npc[n].active = false;
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
                }
            }

            //spawnfrag fix
            int p = Player.FindClosest(NPC.Center, 0, 0);
            if (p.IsWithinBounds(Main.maxPlayers))
            {
                Player player = Main.player[p];
                if (player != null && player.active && !player.dead)
                {
                    if (NPC.Distance(player.Center) < 400)
                    {
                        NPC.Center = player.Center - Vector2.UnitX * 1000 * player.direction;
                    }
                };
            }

        }

        private void TileCollision(bool fallthrough = false, bool dropDown = false)
        {
            bool onPlatforms = false;
            for (int i = (int)NPC.position.X; i <= NPC.position.X + NPC.width; i += 16)
            {
                if (Framing.GetTileSafely(new Vector2(i, NPC.Bottom.Y + 2)).TileType == TileID.Platforms)
                {
                    onPlatforms = true;
                    break;
                }
            }

            bool onCollision = Collision.SolidCollision(NPC.position, NPC.width, NPC.height);

            if (dropDown)
            {
                NPC.velocity.Y += 0.5f;
            }
            else if (onCollision || onPlatforms && !fallthrough)
            {
                if (NPC.velocity.Y > 0f)
                    NPC.velocity.Y = 0f;

                if (NPC.velocity.Y > -0.2f)
                    NPC.velocity.Y -= 0.025f;
                else
                    NPC.velocity.Y -= 0.2f;

                if (NPC.velocity.Y < -4f)
                    NPC.velocity.Y = -4f;

                if (Jumping) //landing effects
                {
                    SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/Challengers/Trojan/TrojanJump") with { Variants = [1, 2]}, NPC.Bottom);
                    for (int i = 0; i < 4; i++)
                    {
                        int side = i % 2 == 0 ? 1 : -1;
                        float speed = Main.rand.NextFloat(4, 6);
                        Vector2 vel = (Vector2.UnitX * side * speed).RotatedByRandom(MathHelper.Pi / 11);
                        Gore gore = Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.Bottom - Vector2.UnitY * 10, vel, Main.rand.Next(11, 14), Scale: 2f);
                    }
                    Jumping = false;
                }
            }
            else
            {
                if (NPC.velocity.Y < 0f)
                    NPC.velocity.Y = 0f;

                if (NPC.velocity.Y < 0.1f)
                    NPC.velocity.Y += 0.025f;
                else
                    NPC.velocity.Y += 0.5f;
            }

            if (NPC.velocity.Y > 10f)
                NPC.velocity.Y = 10f;

            Player target = Main.player[NPC.target];
            //anti-leaving soon when terrain above you
            if (onCollision && target != null && target.active && !target.dead && target.Center.Y > NPC.Center.Y + NPC.height * 1.5f && Math.Abs(target.Center.X - NPC.Center.X) < 400)
            {
                NPC.velocity.Y += 3f;
            }
        }

        private void Movement(Vector2 target, bool goFast = false)
        {
            NPC.direction = NPC.spriteDirection = NPC.Center.X < target.X ? 1 : -1;

            if (Math.Abs(target.X - NPC.Center.X) < NPC.width / 2)
            {
                NPC.velocity.X *= 0.9f;
                if (Math.Abs(NPC.velocity.X) < 0.1f)
                    NPC.velocity.X = 0f;
            }
            else
            {
                float maxwalkSpeed = BaseWalkSpeed * NPC.scale;

                if (head == null)
                    maxwalkSpeed *= 1.2f;
                if (arms == null)
                    maxwalkSpeed *= 1.2f;

                if (goFast)
                {
                    maxwalkSpeed *= 3f;
                    if (!WorldSavingSystem.EternityMode)
                        maxwalkSpeed *= 0.75f;
                }
                else if (!WorldSavingSystem.MasochistModeReal)
                {
                    maxwalkSpeed *= 0.75f;

                    if (head != null && head.ai[0] != 0 || arms != null && arms.ai[0] != 0)
                        maxwalkSpeed *= 0.5f;
                }

                if (NPC.dontTakeDamage)
                    maxwalkSpeed *= 0.75f;

                int walkModifier = WorldSavingSystem.EternityMode ? 30 : 40;
                if (WorldSavingSystem.MasochistModeReal || arms == null || head == null)
                    walkModifier = 20;

                if (NPC.direction > 0)
                    NPC.velocity.X = (NPC.velocity.X * walkModifier + maxwalkSpeed) / (walkModifier + 1);
                else
                    NPC.velocity.X = (NPC.velocity.X * walkModifier - maxwalkSpeed) / (walkModifier + 1);
            }

            TileCollision(target.Y > NPC.Bottom.Y, Math.Abs(target.X - NPC.Center.X) < NPC.width / 2 && NPC.Bottom.Y < target.Y);
        }
        public bool Jumping = false;

        #region AI
        public override void AI()
        {
            
            if (!spawned)
            {
                spawned = true;
                introJump = true;

                NPC.TargetClosest(false);

                if (FargoSoulsUtil.HostCheck)
                {
                    head = FargoSoulsUtil.NPCExists(FargoSoulsUtil.NewNPCEasy(NPC.GetSource_FromThis(), NPC.Center, ModContent.NPCType<TrojanSquirrelHead>(), NPC.whoAmI, target: NPC.target));
                    arms = FargoSoulsUtil.NPCExists(FargoSoulsUtil.NewNPCEasy(NPC.GetSource_FromThis(), NPC.Center, ModContent.NPCType<TrojanSquirrelArms>(), NPC.whoAmI, target: NPC.target));
                }
                //drop summon
                EModeUtils.DropSummon(NPC, ModContent.ItemType<SquirrelCoatofArms>(), WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.TrojanSquirrel], ref spawned);

                //start by jumping
                NPC.ai[0] = 1f;
                NPC.ai[3] = 1f;

                LegFrameType = 1;

                for (int i = 0; i < 80; i++)
                {
                    int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Smoke, NPC.velocity.X, NPC.velocity.Y, 50, default, 4f);
                    Main.dust[d].velocity.Y -= 1.5f;
                    Main.dust[d].velocity *= 1.5f;
                    Main.dust[d].noGravity = true;
                }

                FargoSoulsUtil.GrossVanillaDodgeDust(NPC);

                //SoundEngine.PlaySound(SoundID.Roar, Main.player[NPC.target].Center);
            }

            

            //NPC.ai[0] = -1;

            Player player = Main.player[NPC.target];
            NPC.direction = NPC.spriteDirection = NPC.Center.X < player.Center.X ? 1 : -1;

            bool despawn = false;

            switch ((int)NPC.ai[0])
            {
                case 0: //mourning wood movement
                    {
                        
                        Vector2 target = player.Bottom - Vector2.UnitY;
                        if (NPC.localAI[0] > 0) //doing running attack
                        {
                            if (LegFrameType == 1)
                                LegFrameType = 2;
                            NPC.localAI[0] -= 1f;

                            if (NPC.localAI[0] % 10 == 0) //hermes boot clouds
                            {
                                SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/Challengers/Trojan/TrojanFootstep") with {Variants = [1, 2, 3], Volume = 0.5f}, NPC.Bottom);
                                Vector2 vel = (-NPC.velocity).RotatedByRandom(MathHelper.Pi / 11f);
                                vel /= 2;
                                //Gore gore = Gore.NewGoreDirect(player.GetSource_FromThis(), NPC.Bottom - Vector2.UnitY * 10, vel, Main.rand.Next(11, 14), Scale: Main.rand.NextFloat(1.5f, 2f));
                                //gore.timeLeft /= 2;
                            }

                            float distance = NPC.Center.X - target.X;
                            bool passedTarget = Math.Sign(distance) == NPC.localAI[1];
                            if (passedTarget && Math.Abs(distance) > 160)
                                NPC.localAI[0] = 0f;

                            target = new Vector2(NPC.Center.X + 256f * NPC.localAI[1], target.Y);

                            if (NPC.localAI[0] == 0f)
                                NPC.TargetClosest(false);

                            if (WorldSavingSystem.EternityMode && head == null && NPC.localAI[0] % 3 == 0 && FargoSoulsUtil.HostCheck)
                            {
                                int p = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Top.X, NPC.Top.Y, Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-3),
                                    Main.rand.Next(326, 329), FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 0f, Main.myPlayer);
                                if (p != Main.maxProjectiles)
                                    Main.projectile[p].timeLeft = 90;
                            }
                        }
                        else if (!NPC.HasValidTarget || NPC.Distance(player.Center) > 2400)
                        {
                            target = NPC.Center + new Vector2(256f * Math.Sign(NPC.Center.X - player.Center.X), -128);

                            NPC.TargetClosest(false);

                            despawn = true;
                        }

                        if (Math.Abs(NPC.velocity.Y) < 0.05f && NPC.localAI[3] >= 2)
                        {
                            if (NPC.localAI[3] == 2)
                            {
                                NPC.localAI[3] = 0f;
                            }
                            else
                            {
                                NPC.localAI[3] -= 1;
                                NPC.ai[0] = 1f;
                                NPC.ai[3] = 1f;
                            }

                            if (WorldSavingSystem.MasochistModeReal)
                            {
                                SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/Challengers/Trojan/TrojanJump") with { Variants = [1, 2] }, NPC.Bottom);

                                ExplodeAttack();
                            }
                        }

                        bool goFast = despawn || NPC.localAI[0] > 0;
                        Movement(target, goFast);

                        if (arms != null && (NPC.localAI[3] == -1 || NPC.localAI[3] == 1)) //from arms
                            NPC.direction = NPC.spriteDirection = (int)NPC.localAI[3];

                        bool canDoAttacks = WorldSavingSystem.EternityMode && !goFast;
                        if (canDoAttacks) //decide next action
                        {
                            float increment = 1f;
                            if (head == null)
                                increment += 0.75f;
                            if (arms == null)
                                increment += 0.75f;
                            if (WorldSavingSystem.MasochistModeReal)
                                increment += 1f;
                            if (NPC.dontTakeDamage)
                                increment /= 2;

                            if (target.Y > NPC.Top.Y)
                                NPC.ai[1] += increment;
                            else
                                NPC.ai[2] += increment;

                            if (Math.Abs(NPC.velocity.Y) < 0.05f)
                            {
                                //its structured like this to ensure body picks the right attack for the situation after being delayed by head/arms
                                bool canProceed = !(head != null && head.ai[0] != 0) && !(arms != null && arms.ai[0] != 0);

                                int threshold = 300;
                                if (NPC.ai[1] > threshold)
                                {
                                    if (canProceed)
                                    {
                                        NPC.ai[0] = 1f;
                                        NPC.ai[1] = 0f;
                                        //NPC.ai[2] = 0f;
                                        NPC.ai[3] = 0f;
                                        NPC.localAI[0] = 0f;
                                        NPC.netUpdate = true;
                                    }
                                    else
                                    {
                                        NPC.ai[1] -= 10f;
                                    }
                                }

                                if (NPC.ai[2] > threshold)
                                {
                                    if (canProceed)
                                    {
                                        NPC.ai[0] = 1f;
                                        //NPC.ai[1] = 0f;
                                        NPC.ai[2] = 0f;
                                        NPC.ai[3] = 1f;
                                        NPC.localAI[0] = 0f;
                                        NPC.netUpdate = true;
                                    }
                                    else
                                    {
                                        NPC.ai[2] -= 10f;
                                    }
                                }
                            }
                        }
                    }
                    break;

                case 1: //telegraph something
                    {
                        NPC.velocity.X = 0;

                        if (LegFrameType == 1)
                        {
                            LegFrameType = 2;
                            LegFrame.Y = 0;
                        }
                            

                        TileCollision(player.Bottom.Y - 1 > NPC.Bottom.Y, Math.Abs(player.Center.X - NPC.Center.X) < NPC.width / 2 && NPC.Bottom.Y < player.Bottom.Y - 1);

                        int threshold = 105;
                        if (WorldSavingSystem.EternityMode)
                        {
                            if (head == null)
                                threshold -= WorldSavingSystem.MasochistModeReal ? 15 : 10;
                            if (arms == null)
                                threshold -= WorldSavingSystem.MasochistModeReal ? 15 : 10;
                            if (head == null && arms == null)
                                threshold -= WorldSavingSystem.MasochistModeReal ? 35 : 35;
                        }
                        if (WorldSavingSystem.MasochistModeReal || NPC.localAI[3] >= 2)
                            threshold -= 20;

                        AnimationLengthOverride = threshold;
                        AnimationLengthOverride -= 45;
                        AnimationLengthOverride = Math.Max(15, AnimationLengthOverride);

                        if (NPC.ai[3] != 0f) //telegraphing jump
                        {
                            int dir = NPC.localAI[0] % 2 == 0 ? 1 : -1;
                            int maxShake = 8;
                            float shake = dir * maxShake * (NPC.localAI[0] / threshold);
                            NPC.position.X += shake;

                            if (LegFrameType == 3) // run frame; incorrect, can happen on transition sometimes
                            {
                                LegFrameType = 2;
                                LegFrame.Y = 0;
                            }
                        }
                        else // telegraphing run
                        {
                            if (LegFrameType == 4) // jump frame; incorrect
                            {
                                LegFrameType = 2;
                                LegFrame.Y = 0;
                            }
                        }

                        if (++NPC.localAI[0] > threshold)
                        {
                            NPC.localAI[0] = 0f;
                            NPC.netUpdate = true;

                            if (NPC.ai[3] == 0f)
                            {
                                NPC.ai[0] = 0f;

                                NPC.localAI[0] = 300f;
                                NPC.localAI[1] = Math.Sign(player.Center.X - NPC.Center.X);
                                NPC.localAI[2] = player.Center.X;
                            }
                            else
                            {
                                NPC.ai[0] = 2f;
                            }
                        }
                    }
                    break;

                case 2: //jump
                    {
                        
                        const float gravity = 0.4f;
                        float time = WorldSavingSystem.EternityMode && arms == null ? 60f : 90f;

                        if (NPC.localAI[0]++ == 0)
                        {
                            if (LegFrameType == 1)
                                LegFrameType = 2;
                            Vector2 distance = player.Top - NPC.Bottom;

                            if (introJump)
                            {
                                distance.X /= 2;
                                introJump = false;
                            }
                                

                            if (WorldSavingSystem.EternityMode && arms == null)
                            {
                                distance.X += NPC.width * Math.Sign(player.Center.X - NPC.Center.X);

                                if (NPC.localAI[3] < 2)
                                {
                                    NPC.localAI[3] = 2; //flag to stomp again on landing
                                    if (head == null)
                                        NPC.localAI[3] += 2; //flag to do more stomps
                                }

                                ExplodeAttack();
                            }

                            distance.X /= time;
                            distance.Y = distance.Y / time - 0.5f * gravity * time;
                            NPC.velocity = distance;

                            NPC.netUpdate = true;

                            if (arms != null)
                            {
                                SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/Challengers/Trojan/TrojanJump") with { Variants = [1, 2] }, NPC.Bottom);
                            }
                            else
                            {
                                SoundEngine.PlaySound(new SoundStyle("FargowiltasSouls/Assets/Sounds/Challengers/Trojan/TrojanJumpExplosive") with { Variants = [1, 2] }, NPC.Bottom);
                            }
                            

                            for (int i = 0; i < 4; i++)
                            {
                                int side = i % 2 == 0 ? 1 : -1;
                                float speed = Main.rand.NextFloat(4, 6);
                                Vector2 vel = (Vector2.UnitX * side * speed).RotatedByRandom(MathHelper.Pi / 11);
                                Gore gore = Gore.NewGoreDirect(NPC.GetSource_FromThis(), NPC.Bottom - Vector2.UnitY * 10, vel, Main.rand.Next(11, 14), Scale: 2f);
                            }

                            Jumping = true;
                        }
                        else
                        {
                            NPC.velocity.Y += gravity;
                        }

                        if (NPC.localAI[0] > time)
                        {
                            NPC.TargetClosest(false);

                            NPC.velocity.X = Utils.Clamp(NPC.velocity.X, -20, 20);
                            NPC.velocity.Y = Utils.Clamp(NPC.velocity.Y, -10, 10);

                            NPC.ai[0] = 0f;
                            NPC.localAI[0] = 0f;
                            NPC.netUpdate = true;
                        }
                    }
                    break;

                default:
                    NPC.ai[0] = 0;
                    goto case 0;
            }

            if (despawn)
            {
                if (NPC.timeLeft > 60)
                    NPC.timeLeft = 60;
            }
            else
            {
                if (NPC.timeLeft < 600)
                    NPC.timeLeft = 600;
            }

            if (head == null)
            {
                Vector2 pos = NPC.Top;
                pos.X += 2f * 2f * NPC.direction;
                pos.Y += 8;

                int width = 2 * 16;
                int height = 2 * 16;

                pos.X -= width / 2f;
                pos.Y -= height / 2f;

                /*for (int i = 0; i < 3; i++)
                {
                    int d = Dust.NewDust(pos, width, height, DustID.Smoke, NPC.velocity.X, NPC.velocity.Y, 50, default, 2.5f);
                    Main.dust[d].velocity.Y -= 1.5f;
                    Main.dust[d].velocity *= 1.5f;
                    Main.dust[d].noGravity = true;
                }*/

                if (Main.rand.NextBool(2))
                {
                    int d = Dust.NewDust(pos, width, height, DustID.Torch, NPC.velocity.X * 0.9f, NPC.velocity.Y * 0.9f, 100, default, 2f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity.Y -= Main.rand.NextFloat(3f, 8f);
                    Main.dust[d].velocity *= 1.5f;
                }

                if (Main.rand.NextBool(6))
                {
                    Particle p = new SmokeParticle(Main.rand.NextVector2FromRectangle(new Rectangle((int)pos.X, (int)pos.Y, width, height)), NPC.velocity * 0.9f - Vector2.UnitY * 6, Color.Gray, 50, 0.3f, 0.05f, Main.rand.NextFloat(MathF.Tau));
                    p.Spawn();
                }
            }
            else
            {
                lifeMaxHead = head.lifeMax;
                head = FargoSoulsUtil.NPCExists(head.whoAmI, ModContent.NPCType<TrojanSquirrelHead>());
            }

            if (arms == null)
            {
                Vector2 pos = NPC.Center;
                pos.X -= 16f * NPC.direction;
                pos.Y -= 3f * 16f;

                int width = 2 * 16;
                int height = 2 * 16;

                pos.X -= width / 2f;
                pos.Y -= height / 2f;

                /*for (int i = 0; i < 2; i++)
                {
                    int d = Dust.NewDust(pos, width, height, DustID.Smoke, NPC.velocity.X, NPC.velocity.Y, 50, default, 1.5f);
                    Main.dust[d].noGravity = true;
                }*/

                /*if (Main.rand.NextBool(6))
                {
                    int d2 = Dust.NewDust(pos, width, height, DustID.Torch, NPC.velocity.X * 0.4f, NPC.velocity.Y * 0.4f, 100, default, 3f);
                    Main.dust[d2].noGravity = true;
                }*/
            }
            else
            {
                lifeMaxArms = arms.lifeMax;
                arms = FargoSoulsUtil.NPCExists(arms.whoAmI, ModContent.NPCType<TrojanSquirrelArms>());
            }

            /*if (NPC.life < NPC.lifeMax / 2 && Main.rand.NextBool(3))
            {
                int d = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Smoke, NPC.velocity.X, NPC.velocity.Y, 50, default, 4f);
                Main.dust[d].velocity.Y -= 1.5f;
                Main.dust[d].velocity *= 1.5f;
                Main.dust[d].noGravity = true;
            }*/

            if (WorldSavingSystem.EternityMode)
            {
                
                bool wasImmune = NPC.dontTakeDamage;
                NPC.dontTakeDamage = NPC.life < NPC.lifeMax / 2 && (head != null || arms != null);

                if (wasImmune != NPC.dontTakeDamage)
                {
                    for (int i = 0; i < 6; i++)
                        ExplodeDust(NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)));
                }
            }
            else
            {
                NPC.dontTakeDamage = false;
            }

            if (WorldSavingSystem.MasochistModeReal && Main.getGoodWorld && FargoSoulsUtil.HostCheck)
            {
                int[] edibleTiles =
                [
                    TileID.WoodBlock,
                    TileID.AshWood,
                    TileID.BorealWood,
                    TileID.DynastyWood,
                    TileID.LivingWood,
                    TileID.PalmWood,
                    TileID.SpookyWood,
                    TileID.Ebonwood,
                    TileID.Pearlwood,
                    TileID.Shadewood,
                    TileID.Trees,
                    TileID.TreeAsh,
                    TileID.ChristmasTree,
                    TileID.PalmTree,
                    TileID.PineTree,
                    TileID.VanityTreeSakura,
                    TileID.VanityTreeYellowWillow,
                    TileID.LivingMahoganyLeaves
                ];
                for (float x = NPC.position.X; x < NPC.BottomRight.X; x += 16)
                {
                    for (float y = NPC.position.Y; y < NPC.BottomRight.Y; y += 16)
                    {
                        Tile tile = Framing.GetTileSafely(new Vector2(x, y));
                        if (tile != null && edibleTiles.Contains(tile.TileType))
                        {
                            int xCoord = (int)x / 16;
                            int yCoord = (int)y / 16;
                            WorldGen.KillTile(xCoord, yCoord, noItem: true);
                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendTileSquare(-1, xCoord, yCoord, 1);

                            NPC.scale += 0.01f;
                            NPC.netUpdate = true;
                            if (head is NPC)
                            {
                                head.scale += 0.01f;
                                head.netUpdate = true;
                            }
                            if (arms is NPC)
                            {
                                arms.scale += 0.01f;
                                arms.netUpdate = true;
                            }
                        }
                    }
                }
            }

            // to prevent a bug where he played the sound again.
            if (NPC.life < NPC.lifeMax / 2 && hasplayedbreaksound == false && (arms != null || head != null))
            {
                SoundEngine.PlaySound(FargosSoundRegistry.TrojanLegsDeath, NPC.Center);
                hasplayedbreaksound = true;
            }

            SmokeVisuals();
        }
        #endregion

        private void ExplodeAttack()
        {
            if (FargoSoulsUtil.HostCheck)
            {
                float offsetX = NPC.width;
                const float offsetY = 65;
                int max = WorldSavingSystem.MasochistModeReal ? 4 : 2;
                for (int i = -max; i <= max; i++)
                {
                    Projectile p = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Bottom + new Vector2(offsetX * i, -offsetY), Vector2.Zero, ProjectileID.DD2ExplosiveTrapT3Explosion, FargoSoulsUtil.ScaledProjectileDamage(NPC.defDamage), 0, Main.myPlayer);
                    if (p != null)
                    {
                        p.friendly = false;
                        p.hostile = true;
                        p.netUpdate = true;
                    }
                }

            }
        }

        private void SmokeVisuals()
        {
            int headsmokedir = 0;
            int armsmokedir = 0;

            if (NPC.direction == 1)
            {
                headsmokedir = 50;
                armsmokedir = 5;
            }

            if (NPC.direction == -1)
            {
                headsmokedir = 35;
                armsmokedir = 40;
            }
            Vector2 headsmokepos = NPC.Center + new Vector2(headsmokedir, -35);

            Vector2 armsmokepos = NPC.Center + new Vector2(armsmokedir, -35);

            float rotationOffset = -MathHelper.PiOver2 + Main.rand.NextFloatDirection() * 0.51f;

            int smokeAmount = 1;

            if (head == null)
            {
                smokeAmount = 5;
            }

            if (head == null && arms == null)
            {
                smokeAmount = 3;
            }

            if (head == null && arms == null && NPC.life == NPC.lifeMax / 2)
            {
                smokeAmount = 1;
            }

            if (Main.rand.NextBool(smokeAmount) && head == null)
            {
                Particle p = new SmokeParticle(headsmokepos, new Vector2(0, Main.rand.Next(-10, -5)), Color.Gray, 50, 1f, 0.05f, rotationOffset);
                p.Spawn();
            }

            if (Main.rand.NextBool(3) && arms == null && head != null)
            {
                Particle p = new SmokeParticle(armsmokepos, new Vector2(0, Main.rand.Next(-10, -5)), Color.Gray, 50, 0.5f, 0.05f, rotationOffset);
                p.Spawn();
                //Particle p2 = new SmokeParticle(armsmokepos * -1, new Vector2(0, Main.rand.Next(-10, -5)), Color.Gray, 50, 0.5f, 0.05f);
                //p2.Spawn();
            }
        }

        private void ExplodeDust(Vector2 center)
        {
            

            const int width = 32;
            const int height = 32;

            Vector2 pos = center - new Vector2(width, height) / 2f;

            for (int i = 0; i < 20; i++)
            {
                int dust = Dust.NewDust(pos, width, height, DustID.Smoke, 0f, 0f, 100, default, 3f);
                Main.dust[dust].velocity *= 1.4f;
            }

            for (int i = 0; i < 15; i++)
            {
                int dust = Dust.NewDust(pos, width, height, DustID.Torch, 0f, 0f, 100, default, 3.5f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 7f;

                dust = Dust.NewDust(pos, width, height, DustID.Torch, 0f, 0f, 100, default, 1.5f);
                Main.dust[dust].velocity *= 3f;
            }

            float scaleFactor9 = 0.5f;
            for (int j = 0; j < 3; j++)
            {
                int gore = Gore.NewGore(NPC.GetSource_FromThis(), center, default, Main.rand.Next(61, 64));
                Main.gore[gore].velocity *= scaleFactor9;
                Main.gore[gore].velocity.X += 1f;
                Main.gore[gore].velocity.Y += 1f;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            //Main.NewText("AnimationType: " + LegFrameType);
            //Main.NewText("framey: " + LegFrame.Y);
            //Main.NewText("framex: " + LegFrame.X);
            //140 is the width of each X Frame.

            if (Main.netMode == NetmodeID.Server) // mp
                return;

            NPC.frame.X = 0;
            if (++NPC.frameCounter >= 7)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y = 0;
            }
                
            if (NPC.frame.Y >= frameHeight * 5)
                NPC.frame.Y = 0;                

            switch (LegFrameType)
            {   
                
                //Walk Animation
                case 1:
                    {
                        //96 is the width of an X Frame.
                        LegFrame.X = 96 * 3;

                        float increment = 1f / BaseWalkSpeed / NPC.scale * Math.Abs(NPC.velocity.X);
                        // trojan backstepping technology
                        increment *= Math.Sign(NPC.direction * Math.Sign(NPC.velocity.X));

                        LegFramecounter += increment;
                        if (LegFramecounter >= 2.5f)
                        {
                            LegFramecounter = 0;
                            LegFrame.Y += LegFrame.Height;

                            if (LegFrame.Y >= LegFrame.Height * LegFrameMax)
                                LegFrame.Y = 0;
                        }
                        else if (LegFramecounter <= 0)
                        {
                            LegFramecounter = 2.5f;
                            LegFrame.Y -= LegFrame.Height;

                            if (LegFrame.Y < 0)
                                LegFrame.Y = LegFrame.Height * (LegFrameMax - 1);
                        }

                        LegFrameAlt.X = 96 * 3;
                        int frameY = LegFrame.Y / LegFrame.Height;
                        int frameAltY = frameY + (LegFrameMax / 2);
                        frameAltY %= LegFrameMax;
                        LegFrameAlt.Y = frameAltY * LegFrameAlt.Height;

                        /*
                        LegFrameAlt.X = 96 * 3;
                        if (LegFramecounter >= 5)
                        {
                            LegFramecounter = 0;
                            LegFrameAlt.Y += LegFrameAlt.Height;

                            if (LegFrameAlt.Y >= LegFrameAlt.Height * LegFrameMax)
                                LegFrameAlt.Y = 0;
                        }
                        */

                    }
                    break;
                
                //Preparing to either jump, or run.
                case 2:
                    {
                        int animLength = AnimationLengthOverride;
                        int framesPerFrame = 3;
                        if (animLength > 0)
                            framesPerFrame = (int)(animLength / 5f);
                        bool PreparingJump = NPC.ai[0] == 2;
                        bool BeforeJump = (NPC.ai[0] == 1 && NPC.ai[3] != 0f);
                        bool PreparingRun = NPC.ai[0] == 0 || (NPC.ai[0] == 1 && NPC.ai[3] == 0f);

                        LegFrame.X = 0;
                        if (++LegFramecounter >= framesPerFrame)
                        {
                            LegFramecounter = 0;

                            
                            LegFrame.Y += LegFrame.Height;
                            if (LegFrame.Y >= LegFrame.Height * 5)
                            {
                                LegFrame.Y = LegFrame.Height * 5;
                            }


                            if (LegFrame.Y >= LegFrame.Height * 5)
                            {
                                if (PreparingJump)
                                {
                                    LegFrame.X = 96 * 4;
                                    LegFrame.Y = 0;
                                    LegFrameType = 4;
                                }
                                if (PreparingRun)
                                {
                                    LegFrame.X = 96;
                                    LegFrame.Y = 0;
                                    LegFrameType = 3;
                                }
                            }
                                
                        }
                    }
                    break;
                
                //Running
                case 3:
                    {
                        ++LegFramecounter;
                        
                        //Reverse running to move to next stage
                        if (NPC.ai[0] == 0 && NPC.localAI[0] <= 0)
                        {
                            LegFrame.X = 96;
                            if (LegFramecounter >= 16)
                            {
                                LegFramecounter = 0;
                                LegFrame.Y -= LegFrame.Height;
                                if (LegFrame.Y <= 0)
                                {
                                    //LegFrame.X -= 96;
                                    LegFrame.Y = 0;
                                    LegFrameType = 1;                             
                                }
                            } 
                        }
                        else if (LegFrame.X == 96) //Running
                        {
                            if (LegFramecounter >= 12)
                            {
                                LegFramecounter = 0;
                                LegFrame.Y += LegFrame.Height;
                                if (LegFrame.Y >= LegFrame.Height * 2)
                                {
                                    LegFrame.X += 96;
                                    LegFrame.Y = 0;
                                    --LegFramecounter;
                                }
                            }
                        }
                        else if (LegFrame.X == 96 * 2)
                        {
                            if (LegFramecounter >= 7)
                            {
                                LegFramecounter = 0;
                                LegFrame.Y += LegFrame.Height;
                                if (LegFrame.Y >= LegFrame.Height * 2)
                                {
                                    LegFrame.Y = 0;
                                }
                            }
                        }



                    } 
                    break;

                
                //Jumping
                case 4:
                    {
                        if (++LegFramecounter >= 16)
                        {
                            LegFramecounter = 0;
                            LegFrame.Y += LegFrame.Height;
                            if (LegFrame.Y >= LegFrame.Height * 2)
                            {
                                LegFrame.Y = LegFrame.Height * 2;                              
                            }

                            if (!Jumping)
                            {
                                LegFrame.Y = 0;
                                LegFrameType = 1;
                            }
                                
                        }
                    }
                    break;

                default:
                    {
                        LegFrameType = 1;
                    }
                    goto case 1;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
            {
                return true;
            }

            
                
            Texture2D ArmTexture = ModContent.Request<Texture2D>("FargowiltasSouls/Content/Bosses/TrojanSquirrel/TrojanSquirrelArms", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D LegTexture = ModContent.Request<Texture2D>("FargowiltasSouls/Content/Bosses/TrojanSquirrel/TrojanSquirrelLegs", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Texture2D BodyTexture = TextureAssets.Npc[NPC.type].Value;
            Rectangle rectangle = new Rectangle(NPC.frame.X, NPC.frame.Y, NPC.frame.Width / 2, NPC.frame.Height);
            Vector2 origin2 = rectangle.Size() / 2f;
            //NPC.direction = 1;
            //Legs
            Rectangle LegRectangle = LegFrame;
            Vector2 LegOrigin = LegRectangle.Size() / 2f;
            Rectangle LegRectangleAlt = LegFrameAlt;
            Vector2 LegAltOrigin = LegRectangleAlt.Size() / 2f;

            Color color26 = drawColor;
            color26 = NPC.GetAlpha(color26);

            SpriteEffects effects = NPC.direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            if (LegFrame.X == 0)
            {
                bodyOffset = Vector2.Lerp(new(0, 0), new(0, 2), LegFrame.Y / LegFrame.Height);
            }
            else if (LegFrame.X == 96)
            {
                bodyOffset = Vector2.Lerp(new(0, 0), new(0, -6), LegFrame.Y / LegFrame.Height);
            }
            else if (LegFrame.X == 96 * 2)
            {
                bodyOffset = new(0, -6);
                bodyOffset += Vector2.Lerp(new(0, 0), new(0, -1), LegFrame.Y / LegFrame.Height);
            }
            else
            {
                bodyOffset = new(0, 0);
            }

            if (arms != null)
            {
                var trojanArms = arms.As<TrojanSquirrelArms>();
                int animationType = trojanArms.AltArmAnimationType switch
                {
                    0 => 0,
                    2 => 2,
                    _ => trojanArms.ArmsAnimationType
                };
                Rectangle armsFrame = trojanArms.GetFrame(animationType, arms.frame.Height);
                Rectangle ArmRectangle = new(armsFrame.X, armsFrame.Y, armsFrame.Width / 3, armsFrame.Height);
                Vector2 ArmOrigin = ArmRectangle.Size() / 2f;
                Main.EntitySpriteDraw(ArmTexture, NPC.Center - screenPos + new Vector2(NPC.direction < 0 ? 0 : 73f, NPC.gfxOffY + 26 * NPC.scale) + bodyOffset, new Microsoft.Xna.Framework.Rectangle?(ArmRectangle), color26, NPC.rotation, origin2, NPC.scale * 0.9f, effects, 0);
            }

            

            Main.EntitySpriteDraw(LegTexture, NPC.Center - screenPos + new Vector2(NPC.direction < 0 ? -32 : 46f, NPC.gfxOffY + 33 * NPC.scale), new Microsoft.Xna.Framework.Rectangle?(LegFrameType == 1 ? LegRectangleAlt : LegRectangle), color26, NPC.rotation, LegAltOrigin, NPC.scale, effects, 0);

            Main.EntitySpriteDraw(BodyTexture, NPC.Center - screenPos + new Vector2(NPC.direction < 0 ? 40 : -25f, NPC.gfxOffY - 43 * NPC.scale) + bodyOffset, new Microsoft.Xna.Framework.Rectangle?(rectangle), color26, NPC.rotation, origin2, NPC.scale, effects, 0);

            Main.EntitySpriteDraw(LegTexture, NPC.Center - screenPos + new Vector2(NPC.direction < 0 ? 40 : -24f, NPC.gfxOffY + 33 * NPC.scale), new Microsoft.Xna.Framework.Rectangle?(LegRectangle), color26, NPC.rotation, LegOrigin, NPC.scale, effects, 0);

            return false;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                for (int i = 2; i <= 7; i++)
                {
                    Vector2 pos = NPC.position + new Vector2(Main.rand.NextFloat(NPC.width), Main.rand.NextFloat(NPC.height));
                    if (!Main.dedServ)
                        Gore.NewGore(NPC.GetSource_FromThis(), pos, NPC.velocity, ModContent.Find<ModGore>(Mod.Name, $"TrojanSquirrelGore{i}").Type, NPC.scale);
                }
            }
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.LesserHealingPotion;
        }

        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.TrojanSquirrel], -1);

            if (ModContent.TryFind("Fargowiltas", "Squirrel", out ModNPC squrrl) && !NPC.AnyNPCs(squrrl.Type))
            {
                int n = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, squrrl.Type);
                if (n != Main.maxNPCs)
                {
                    Main.npc[n].homeless = true;
                    if (TownNPCName != default)
                        Main.npc[n].GivenName = TownNPCName;
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, n);
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // I have setup the loot placement in this way because 
            // when registering loot for an npc, the bestiary checks for the order of loot registered.
            // For parity with vanilla, the order is as follows: Trophy, Classic Loot, Expert Loot, Master loot.

            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TrojanSquirrelTrophy>(), 10));

            LeadingConditionRule rule = new(new Conditions.NotExpert());

            rule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<TrojanMask>(), 7));

            rule.OnSuccess(ItemDropRule.OneFromOptions(1, ModContent.ItemType<TreeSword>(), ModContent.ItemType<MountedAcornGun>(), ModContent.ItemType<SnowballStaff>(), ModContent.ItemType<KamikazeSquirrelStaff>()));
            rule.OnSuccess(ItemDropRule.OneFromOptions(1,
                ItemID.Squirrel,
                ItemID.SquirrelRed
            //ItemID.SquirrelGold,
            //ItemID.GemSquirrelAmber,
            //ItemID.GemSquirrelAmethyst,
            //ItemID.GemSquirrelDiamond,
            //ItemID.GemSquirrelEmerald,
            //ItemID.GemSquirrelRuby,
            //ItemID.GemSquirrelSapphire,
            //ItemID.GemSquirrelTopaz
            ));
            rule.OnSuccess(ItemDropRule.Common(ItemID.SquirrelHook));
            rule.OnSuccess(ItemDropRule.Common(ItemID.Acorn, 1, 100, 100));
            rule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<SquirrelCharm>()));
            rule.OnSuccess(ItemDropRule.Common(ModContent.Find<ModItem>("Fargowiltas", "LumberJaxe").Type, 10));

            npcLoot.Add(rule);

            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<TrojanSquirrelBag>()));

            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<TrojanSquirrelRelic>()));
        }

        public override void BossHeadSpriteEffects(ref SpriteEffects spriteEffects)
        {
            spriteEffects = NPC.direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        }
    }
}
