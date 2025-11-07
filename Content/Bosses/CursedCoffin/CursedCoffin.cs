using FargowiltasSouls.Content.BossBars;
using FargowiltasSouls.Content.Buffs.Eternity;
using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Content.Items.Accessories;
using FargowiltasSouls.Content.Items.BossBags;
using FargowiltasSouls.Content.Items.Placables.Relics;
using FargowiltasSouls.Content.Items.Placables.Trophies;
using FargowiltasSouls.Content.Items.Vanity.Masks;
using FargowiltasSouls.Content.Items.Weapons.Challengers;
using FargowiltasSouls.Core.Systems;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace FargowiltasSouls.Content.Bosses.CursedCoffin
{
    [AutoloadBossHead]
    public partial class CursedCoffin : ModNPC
    {
        public const bool Enabled = true;
        public override bool IsLoadingEnabled(Mod mod) => Enabled;

        #region Variables
        float DrawcodeOpacity = 0f;

        private int Phase = 0;
		private bool Attacking = true;
		private bool ExtraTrail = false;

		public int MashTimer = 15;

		public int Frame = 0;

		public Vector2 LockVector1 = Vector2.Zero;

		private int LastAttackChoice { get; set; }

        //NPC.ai[] overrides
        public float Timer
        {
            get => StateMachine.StateStack.Count != 0 ? StateMachine.CurrentState.Time : 0;
            set 
            {
                if (StateMachine.StateStack.Count != 0)
                    StateMachine.CurrentState.Time = (int)value;
            }
        }
        /// <summary>
        /// Setting this to a number except 0 immediately forces the SpiritGrabPunish state.
		/// This happens when the Spirit grabs a player.
        /// </summary>
        public ref float ForceGrabPunish => ref NPC.ai[1];
        public ref float AI2 => ref NPC.ai[2];
		public ref float AI3 => ref NPC.ai[3];
        /// <summary>
        /// Increments every time an attack sequence ends.
        /// Attacks 0-2 ghostless.
        /// Attack 3 is spawning ghost.
        /// Attack 4-5 accompanied by ghost. 
        /// 6th attack is gem rain, then repeat cycle.
        /// </summary>
        public ref float AttackCounter => ref NPC.localAI[0];

        public bool Enraged => false; // NPC.GetLifePercent() <= 0.2f && WorldSavingSystem.EternityMode; // disabled because it occasionally threw stack overflow errors. fix that if readding this

		public Vector2 MaskCenter() => NPC.Center - Vector2.UnitY * NPC.height * NPC.scale / 4;

		private static readonly Color glowColor = Color.Purple with { A = 0 };//new(224, 196, 252, 0);
        public static Color GlowColor => Main.npc.Any(n => n.TypeAlive<CursedCoffin>() && n.As<CursedCoffin>().Enraged) ? Color.Lerp(Color.Red, glowColor, 0.75f) : glowColor;

        #endregion
        #region Standard
        public override void SetStaticDefaults()
		{
			Main.npcFrameCount[NPC.type] = 4;
			NPCID.Sets.TrailCacheLength[NPC.type] = 18; //decrease later if not needed
			NPCID.Sets.TrailingMode[NPC.type] = 1;
			NPCID.Sets.MPAllowedEnemies[Type] = true;

			NPCID.Sets.BossBestiaryPriority.Add(NPC.type);
			NPC.AddDebuffImmunities(
            [
                BuffID.Confused,
				BuffID.Chilled,
				BuffID.Suffocation,
				ModContent.BuffType<LethargicBuff>(),
				ModContent.BuffType<TimeFrozenBuff>()
			]);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.UndergroundDesert,
                //BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.any,
                new FlavorTextBestiaryInfoElement($"Mods.FargowiltasSouls.Bestiary.{Name}")
			});

        }
		public static int BaseHP => 2500;
        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.lifeMax = BaseHP;
            NPC.defense = 10;
            NPC.damage = 31;
            NPC.knockBackResist = 0f;
            NPC.width = 90;
            NPC.height = 150;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit54; 
            NPC.DeathSound = SoundID.NPCDeath6;

            /*Music = ModLoader.TryGetMod("FargowiltasMusic", out Mod _)
                ? MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Silent") : MusicID.OtherworldlyBoss1;

            SceneEffectPriority = SceneEffectPriority.BossLow;*/

            NPC.BossBar = ModContent.GetInstance<CursedCoffinBossBar>();

			NPC.value = Item.buyPrice(0, 2);

        }
        public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
        {
            if (NPC.rotation != 0)
            {
				int centerX = npcHitbox.X + (npcHitbox.Width / 2);
                int centerY = npcHitbox.Y + (npcHitbox.Height / 2);

				float angle = NPC.rotation % MathF.Tau;
				float incline = MathF.Abs(MathF.Sin(angle));
				npcHitbox.Height = (int)(MathHelper.Lerp(NPC.height, NPC.width, incline) * NPC.scale);
                npcHitbox.Width = (int)(MathHelper.Lerp(NPC.width, NPC.height, incline) * NPC.scale);

				npcHitbox.X = (int)(centerX - (npcHitbox.Width / 2));
                npcHitbox.Y = (int)(centerY - (npcHitbox.Height / 2));
            }
            return base.ModifyCollisionData(victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox);
        }
        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {
			if (StateMachine.StateStack.Count != 0 && StateMachine.CurrentState.Identifier == BehaviorStates.YouCantEscape)
				modifiers.Null();
            if (StateMachine.StateStack.Count != 0 && StateMachine.CurrentState.Identifier == BehaviorStates.PhaseTransition)
                modifiers.FinalDamage *= 0.25f;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
			if (StateMachine.StateStack.Count == 0 || (StateMachine.CurrentState.Identifier != BehaviorStates.SlamWShockwave && StateMachine.CurrentState.Identifier != BehaviorStates.WavyShotSlam))
				return false;
			if (StateMachine.CurrentState.Identifier == BehaviorStates.SlamWShockwave && NPC.velocity.Y <= 0)
				return false;
            return base.CanHitPlayer(target, ref cooldownSlot);
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (WorldSavingSystem.MasochistModeReal)
            {
                target.AddBuff(BuffID.Cursed, 60);
            }
        }
        public Rectangle TopHitbox()
        {
            return new((int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height / 3);
        }

        /*
        public Rectangle MaskHitbox()
        {
            Vector2 maskCenter = MaskCenter();
            int maskRadius = 24;
            return new((int)(maskCenter.X - maskRadius * NPC.scale), (int)(maskCenter.Y - maskRadius * NPC.scale), maskRadius * 2, maskRadius * 2);
        }
        */
        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * balance);
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);
            writer.Write(NPC.localAI[3]);
            writer.Write7BitEncodedInt(LastAttackChoice);
            writer.Write7BitEncodedInt(Phase);
            writer.Write(Timer);
            writer.WriteVector2(LockVector1);

            var stateStack = (StateMachine?.StateStack ?? new()).ToList();
            writer.Write(stateStack.Count);
            for (int i = stateStack.Count - 1; i >= 0; i--)
                writer.Write((byte)stateStack[i].Identifier);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			NPC.localAI[0] = reader.ReadSingle();
			NPC.localAI[1] = reader.ReadSingle();
			NPC.localAI[2] = reader.ReadSingle();
			NPC.localAI[3] = reader.ReadSingle();
			LastAttackChoice = reader.Read7BitEncodedInt();
            Phase = reader.Read7BitEncodedInt();
			Timer = reader.ReadSingle();
            LockVector1 = reader.ReadVector2();

			StateMachine.StateStack.Clear();

            int stateStackCount = reader.ReadInt32();
            for (int i = 0; i < stateStackCount; i++)
                StateMachine.StateStack.Push(StateMachine.StateRegistry[(BehaviorStates)reader.ReadByte()]);
		}
		#endregion

		#region Overrides
		public override void HitEffect(NPC.HitInfo hit)
		{
			//TODO: gore

			
            if (NPC.life <= 0)
            {
                /*
                for (int i = 1; i <= 4; i++)
                {
                    Vector2 pos = NPC.position + new Vector2(Main.rand.NextFloat(NPC.width), Main.rand.NextFloat(NPC.height));
                    if (!Main.dedServ)
                        Gore.NewGore(NPC.GetSource_FromThis(), pos, NPC.velocity, ModContent.Find<ModGore>(Mod.Name, $"BaronGore{i}").Type, NPC.scale);
                }
                */
                for (int i = 0; i < 100; i++)
                {
                    Vector2 inVel = Main.rand.NextVector2Circular(0.65f, 1f);
                    int p = Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center + inVel * 40, inVel * 10f, ModContent.ProjectileType<CoffinDarkSouls>(), 0, 0, Main.myPlayer, NPC.whoAmI , -0.135f);
                    if (p.IsWithinBounds(Main.maxProjectiles))
                    {
                        Main.projectile[p].Opacity = 0.4f;
                    }

                }
            }
            
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (NPC.IsABestiaryIconDummy)
            {
                if (Main.getGoodWorld)
                {
                    Texture2D whitecoffin = ModContent.Request<Texture2D>(Texture + "_FTW").Value;
                    spriteBatch.Draw(whitecoffin, NPC.position - screenPos, null, NPC.GetAlpha(drawColor), 0f, Vector2.Zero, NPC.scale, SpriteEffects.None, 0f);
                    return false;
                }
                return true;
            }
				
            if (DrawcodeOpacity < 1f)
                DrawcodeOpacity += 0.025f;

            Texture2D bodytexture = Terraria.GameContent.TextureAssets.Npc[NPC.type].Value;
			Vector2 drawPos = NPC.Center - screenPos;
			SpriteEffects spriteEffects = NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Vector2 origin = new Vector2(bodytexture.Width / 2, bodytexture.Height / 2 / Main.npcFrameCount[NPC.type]);

            for (int i = 0; i < (ExtraTrail ? NPCID.Sets.TrailCacheLength[NPC.type] : NPCID.Sets.TrailCacheLength[NPC.type] / 4); i++)
            {
                Vector2 value4 = NPC.oldPos[i];
                int oldFrame = Frame;
                Rectangle oldRectangle = new(0, oldFrame * bodytexture.Height / Main.npcFrameCount[NPC.type], bodytexture.Width, bodytexture.Height / Main.npcFrameCount[NPC.type]);
                DrawData oldGlow = new(bodytexture, value4 + NPC.Size / 2f - screenPos + new Vector2(0, NPC.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(oldRectangle), GlowColor * (0.5f / i) * DrawcodeOpacity, NPC.rotation, origin, NPC.scale, spriteEffects, 0);
                GameShaders.Misc["LCWingShader"].UseColor(Color.Blue).UseSecondaryColor(Color.Black);
                GameShaders.Misc["LCWingShader"].Apply(oldGlow);
                oldGlow.Draw(spriteBatch);
            }
            bool spirit = Main.npc.Any(p => p.TypeAlive<CursedSpirit>());
            if (!spirit)
            {
                for (int j = 0; j < 12; j++)
                {
                    float spinOffset = (Main.GameUpdateCount * 0.001f * j) % 12;
                    float magnitude = 3f + ((j % 5) * 3f * MathF.Sin(Main.GameUpdateCount * MathHelper.TwoPi / (10 + ((j - 6f) * 28f))));
                    Vector2 afterimageOffset = (MathHelper.TwoPi * (j + spinOffset) / 12f).ToRotationVector2() * magnitude * NPC.scale;
                    Color glowColor = GlowColor;

                    spriteBatch.Draw(bodytexture, drawPos + afterimageOffset, NPC.frame, glowColor * DrawcodeOpacity, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
                }
            }

            spriteBatch.Draw(bodytexture, drawPos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);

            if (!spirit)
            {
                float shakeFactor = 1;
                if (StateMachine.StateStack.Count != 0 && StateMachine.CurrentState.Identifier == BehaviorStates.PhaseTransition)
                    shakeFactor = 3 + 5 * (Timer / 60);
                Texture2D glowTexture = ModContent.Request<Texture2D>(Texture + "_MaskGlow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Color glowColor = GlowColor;
                int glowTimer = (int)(Main.GlobalTimeWrappedHourly * 60) % 60;
                DrawData oldGlow = new(glowTexture, drawPos + Main.rand.NextVector2Circular(shakeFactor, shakeFactor), NPC.frame, glowColor * DrawcodeOpacity * (0.75f + 0.25f * MathF.Sin(MathF.Tau * glowTimer / 60f)), NPC.rotation, new Vector2(bodytexture.Width / 2, bodytexture.Height / 2 / Main.npcFrameCount[NPC.type]), NPC.scale, spriteEffects, 0);
                GameShaders.Misc["LCWingShader"].UseColor(Color.Purple).UseSecondaryColor(Color.Black);
                GameShaders.Misc["LCWingShader"].Apply(oldGlow);
                oldGlow.Draw(spriteBatch);
            }

            return false;
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.spriteDirection = NPC.direction;
			NPC.frame.Y = frameHeight * Frame;
		}

		public override void OnKill()
		{
			NPC.SetEventFlagCleared(ref WorldSavingSystem.DownedBoss[(int)WorldSavingSystem.Downed.CursedCoffin], -1);
		}

		public override void BossLoot(ref string name, ref int potionType)
		{
			potionType = ItemID.HealingPotion;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
            // I have setup the loot placement in this way because 
            // when registering loot for an npc, the bestiary checks for the order of loot registered.
            // For parity with vanilla, the order is as follows: Trophy, Classic Loot, Expert Loot, Master loot.

            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CursedCoffinTrophy>(), 10));

            LeadingConditionRule rule = new(new Conditions.NotExpert());

            rule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<CoffinMask>(), 7));

            //rule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<SoulLantern>()));

            rule.OnSuccess(ItemDropRule.OneFromOptions(1, ModContent.ItemType<SisypheanFist>(), ModContent.ItemType<SpiritLongbow>(), ModContent.ItemType<GildedSceptre>(), ModContent.ItemType<EgyptianFlail>()));
            // gems
            rule.OnSuccess(ItemDropRule.Common(ItemID.Amethyst, 3, 2, 4));
            rule.OnSuccess(ItemDropRule.Common(ItemID.Topaz, 4, 2, 4));
            rule.OnSuccess(ItemDropRule.Common(ItemID.Sapphire, 4, 2, 3));
            rule.OnSuccess(ItemDropRule.Common(ItemID.Emerald, 5, 1, 3));
            rule.OnSuccess(ItemDropRule.Common(ItemID.Ruby, 5, 1, 2));
            rule.OnSuccess(ItemDropRule.Common(ItemID.Amber, 3, 2, 6));
            rule.OnSuccess(ItemDropRule.Common(ItemID.Diamond, 7, 1, 1));

            rule.OnSuccess(ItemDropRule.Common(ItemID.PharaohsMask, 4));
            rule.OnSuccess(ItemDropRule.Common(ItemID.PharaohsRobe, 4));

            npcLoot.Add(rule);

            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<CursedCoffinBag>()));

            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<CursedCoffinRelic>()));
        }
		#endregion
	}
}