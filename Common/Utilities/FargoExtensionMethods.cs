using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.Items.Misc;
using FargowiltasSouls.Content.NPCs.EternityModeNPCs.VanillaEnemies.OOA;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls //lets everything access it without using
{
    public static partial class FargoExtensionMethods
    {
        /// <summary>
        /// Adjusts a TooltipLine to account for prefixes. <br />
        /// Inteded to be used specifically for item names. <br />
        /// This only modifies it in the inventory.
        /// </summary>
        public static TooltipLine ArticlePrefixAdjustment(this TooltipLine itemName, string[] localizationArticles)
        {
            List<string> splitName = itemName.Text.Split(' ').ToList();

            for (int i = 0; i < localizationArticles.Length; i++)
                if (splitName.Remove(localizationArticles[i]))
                {
                    splitName.Insert(0, localizationArticles[i]);
                    break;
                }

            itemName.Text = string.Join(" ", splitName);
            return itemName;
        }
        public static string ArticlePrefixAdjustmentString(this string itemName, string[] localizationArticles)
        {
            List<string> splitName = itemName.Split(' ').ToList();

            for (int i = 0; i < localizationArticles.Length; i++)
                if (splitName.Remove(localizationArticles[i]))
                {
                    splitName.Insert(0, localizationArticles[i]);
                    break;
                }

            itemName = string.Join(" ", splitName);
            return itemName;
        }

        /// <summary>
        /// Uses <see cref="Enumerable.First{TSource}(IEnumerable{TSource}, System.Func{TSource, bool})"/> to find the specified tooltip line. <br />
        /// Returns true if the tooltipLine isn't null and false if it is. <br />
        /// Assumes Terraria as the mod.
        /// </summary>
        public static bool TryFindTooltipLine(this List<TooltipLine> tooltips, string tooltipName, out TooltipLine tooltipLine)
        {
            tooltips.TryFindTooltipLine(tooltipName, "Terraria", out tooltipLine);

            return tooltipLine != null;
        }

        /// <summary>
        /// Uses <see cref="Enumerable.First{TSource}(IEnumerable{TSource}, System.Func{TSource, bool})"/> to find the specified tooltip line. <br />
        /// Returns true if the tooltipLine isn't null and false if it is.
        /// </summary>
        public static bool TryFindTooltipLine(this List<TooltipLine> tooltips, string tooltipName, string tooltipMod, out TooltipLine tooltipLine)
        {
            tooltipLine = tooltips.First(line => line.Name == tooltipName && line.Mod == tooltipMod);

            return tooltipLine != null;
        }

        private static readonly FieldInfo _damageFieldHitInfo =
            typeof(NPC.HitInfo).GetField("_damage", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Null(ref this NPC.HitInfo hitInfo)
        {
            // hitInfo.Damage = 0;

            // https://stackoverflow.com/questions/27226731/getfield-setvalue-doesnt-work
            object unboxedHitInfo = hitInfo;
            _damageFieldHitInfo.SetValue(unboxedHitInfo, 0);
            hitInfo = (NPC.HitInfo)unboxedHitInfo;
            hitInfo.Knockback = 0;
            hitInfo.Crit = false;
            // TODO: should we?
            // hitInfo.HideCombatText = true;
            hitInfo.InstantKill = false;
        }

        public static void Null(ref this NPC.HitModifiers hitModifiers)
        {
            // doesn't work because tModLoader maxes it with 1
            // statModifier = statModifier.Scale(0f);

            // will break if basically any mod registers this hook as well
            hitModifiers.ModifyHitInfo += (ref NPC.HitInfo hitInfo) => hitInfo.Null();
        }

        private static readonly FieldInfo _damageFieldHurtInfo =
            typeof(Player.HurtInfo).GetField("_damage", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Null(ref this Player.HurtInfo hurtInfo)
        {
            object unboxedHurtInfo = hurtInfo;
            _damageFieldHurtInfo.SetValue(unboxedHurtInfo, 0);
            hurtInfo = (Player.HurtInfo)unboxedHurtInfo;
            hurtInfo.Knockback = 0;
        }

        public static void Null(ref this Player.HurtModifiers hurtModifiers)
        {
            // will break if basically any mod registers this hook as well
            hurtModifiers.ModifyHurtInfo += (ref Player.HurtInfo hurtInfo) => hurtInfo.Null();
        }

        public static void AddDebuffImmunities(this NPC npc, List<int> debuffs)
        {
            foreach (int buffType in debuffs)
            {
                NPCID.Sets.SpecificDebuffImmunity[npc.type][buffType] = true;
            }
        }

        public static FargoSoulsGlobalNPC FargoSouls(this NPC npc)
            => npc.GetGlobalNPC<FargoSoulsGlobalNPC>();
        public static EModeGlobalNPC Eternity(this NPC npc)
            => npc.GetGlobalNPC<EModeGlobalNPC>();
        public static EModeDD2GlobalNPC EModeDD2(this NPC npc)
            => npc.GetGlobalNPC<EModeDD2GlobalNPC>();
        public static FargoSoulsGlobalProjectile FargoSouls(this Projectile projectile)
            => projectile.GetGlobalProjectile<FargoSoulsGlobalProjectile>();
        public static EModeGlobalProjectile Eternity(this Projectile projectile)
            => projectile.GetGlobalProjectile<EModeGlobalProjectile>();
        public static FargoSoulsPlayer FargoSouls(this Player player)
            => player.GetModPlayer<FargoSoulsPlayer>();
        public static EModePlayer Eternity(this Player player)
            => player.GetModPlayer<EModePlayer>();
        public static AccessoryEffectPlayer AccessoryEffects(this Player player)
            => player.GetModPlayer<AccessoryEffectPlayer>();
        public static bool ForceEffect<T>(this Player player) where T : AccessoryEffect
        {
            Item item = player.EffectItem<T>();
            if (item == null || item.ModItem == null)
                return false;
            return player.FargoSouls().ForceEffect(item.ModItem);
        }
            
        public static bool Alive(this Player player) => player != null && player.active && !player.dead && !player.ghost;
        public static bool Alive(this Projectile projectile) => projectile != null && projectile.active;
        public static bool Alive(this NPC npc) => npc != null && npc.active;
        public static bool TypeAlive(this Projectile projectile, int type) => projectile.Alive() && projectile.type == type;
        public static bool TypeAlive<T>(this Projectile projectile) where T : ModProjectile => projectile.Alive() && projectile.type == ModContent.ProjectileType<T>();
        public static bool TypeAlive(this NPC npc, int type) => npc.Alive() && npc.type == type;
        public static bool TypeAlive<T>(this NPC npc) where T : ModNPC => npc.Alive() && npc.type == ModContent.NPCType<T>();

        public static bool Hostile(this NPC npc) => !npc.townNPC && !npc.CountsAsACritter && npc.life > 10 && npc.type != NPCID.TargetDummy;

        public static Texture2D GetTexture(this NPC npc) => TextureAssets.Npc[npc.type].Value;
        public static Texture2D GetTexture(this Projectile projectile) => TextureAssets.Projectile[projectile.type].Value;
        public static Vector2 GetDrawPosition(this NPC npc) => npc.Center - Main.screenPosition + Vector2.UnitY * npc.gfxOffY;
        public static Vector2 GetDrawPosition(this Projectile projectile) => projectile.Center - Main.screenPosition + Vector2.UnitY * projectile.gfxOffY;
        public static Rectangle GetDefaultFrame(this Projectile projectile)
        {
            Texture2D texture = projectile.GetTexture();
            int sizeY = texture.Height / Main.projFrames[projectile.type]; //ypos of lower right corner of sprite to draw
            int frameY = projectile.frame * sizeY;
            return new(0, frameY, texture.Width, sizeY);
        }

        /// <summary>
        /// Spawns a projectie from this source NPC. <br></br>
        /// This assumes that the projectile is spawned in AI code, and should be spawned on the server. <br></br>
        /// Use this for spawning enemy/boss projectiles.
        /// </summary>
        public static int SpawnProjectile(this NPC npc, Vector2 position, Vector2 velocity, int Type, int Damage, float KnockBack, int Owner = -1, float ai0 = 0, float ai1 = 0, float ai2 = 0, float localAI0 = 0, float localAI1 = 0, float localAI2 = 0, SoundStyle? spawnSoundEffect = null)
        {
            if (spawnSoundEffect.HasValue)
                SoundEngine.PlaySound(spawnSoundEffect, position);
            if (FargoSoulsUtil.HostCheck)
            {
                int p = Projectile.NewProjectile(npc.GetSource_FromAI(), position, velocity, Type, Damage, KnockBack, Owner, ai0, ai1, ai2);
                if (p.IsWithinBounds(Main.maxProjectiles))
                {
                    if (localAI0 != 0)
                        Main.projectile[p].localAI[0] = localAI0;
                    if (localAI1 != 0)
                        Main.projectile[p].localAI[0] = localAI1;
                    if (localAI2 != 0)
                        Main.projectile[p].localAI[0] = localAI2;
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.SyncProjectile, number: p);
                }
                return p;
            }
            return -1;
        }
        /// <summary>
        /// Spawns a projectie from this source projectile. <br></br>
        /// This assumes that the projectile is spawned in AI code, and should be spawned on the server. <br></br>
        /// Use this for enemy/boss projectiles spawning other projectiles.
        /// </summary>
        public static int SpawnProjectile(this Projectile projectile, Vector2 position, Vector2 velocity, int Type, int Damage, float KnockBack, int Owner = -1, float ai0 = 0, float ai1 = 0, float ai2 = 0, float localAI0 = 0, float localAI1 = 0, float localAI2 = 0, SoundStyle? spawnSoundEffect = null)
        {
            if (spawnSoundEffect.HasValue)
                SoundEngine.PlaySound(spawnSoundEffect, position);
            if (FargoSoulsUtil.HostCheck)
            {
                int p = Projectile.NewProjectile(projectile.GetSource_FromAI(), position, velocity, Type, Damage, KnockBack, Owner, ai0, ai1, ai2);
                if (p.IsWithinBounds(Main.maxProjectiles))
                {
                    if (localAI0 != 0)
                        Main.projectile[p].localAI[0] = localAI0;
                    if (localAI1 != 0)
                        Main.projectile[p].localAI[0] = localAI1;
                    if (localAI2 != 0)
                        Main.projectile[p].localAI[0] = localAI2;
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.SyncProjectile, number: p);
                }
                return p;
            }
            return -1;
        }
        public static Item FindAmmo(this Player player, List <int> ammoID)
        {
            Item ammo = new();
            bool gotammo = false;
            if (ammoID.Contains(AmmoID.None))
                return ammo;
            for (int i = 54; i < 58; i++)
            {
                if (ammoID.Contains(player.inventory[i].ammo) && player.inventory[i].stack > 0)
                {
                    ammo = player.inventory[i];
                    return ammo;
                }
            }
            if (!gotammo)
            {
                for (int j = 0; j < 54; j++)
                {
                    if (ammoID.Contains(player.inventory[j].ammo) && player.inventory[j].stack > 0)
                    {
                        ammo = player.inventory[j];
                        return ammo;
                    }
                }
            }
            return ammo;
        }
        public static NPC GetSourceNPC(this Projectile projectile)
            => projectile.GetGlobalProjectile<A_SourceNPCGlobalProjectile>().sourceNPC;

        public static void SetSourceNPC(this Projectile projectile, NPC npc)
            => projectile.GetGlobalProjectile<A_SourceNPCGlobalProjectile>().sourceNPC = npc;

        public static float ActualClassDamage(this Player player, DamageClass damageClass)
            => player.GetTotalDamage(damageClass).Additive * player.GetTotalDamage(damageClass).Multiplicative;
        public static bool IsWeapon(this Item item)
        {
            return (item.damage > 0 && item.pick == 0 && item.axe == 0 && item.hammer == 0) || item.type == ItemID.CoinGun; // I HATE COIN GUN GRAAAAAAAAAAAAAAAAGHHHHHHHHHHHH
        }
        public static bool IsWeaponWithDamageClass(this Item item)
        {
            return (item.damage > 0 && item.DamageType != DamageClass.Default && item.pick == 0 && item.axe == 0 && item.hammer == 0) || item.type == ItemID.CoinGun;
        }
        /// <summary>
        /// Lower bound is 0 and inclusive, cap is exclusive
        /// </summary>
        public static bool IsWithinBounds(this int index, int cap) => index >= 0 && index < cap;
        /// <summary>
        /// Lower bound is inclusive, higher bound is exclusive
        /// </summary>
        public static bool IsWithinBounds(this int index, int lowerBound, int higherBound) => index >= lowerBound && index < higherBound;
        /// <summary>
        /// Sets the magnitude of the vector. Does not modify the original vector. Defaults to Vector2.UnitY if vector length is 0.
        /// </summary>
        public static Vector2 SetMagnitude(this Vector2 vector, float magnitude) => vector.SafeNormalize(Vector2.UnitY) * magnitude;

        /// <summary>
        /// Returns total crit chance, including class-specific and generic boosts.
        /// This method returns 0 for summon crit if Spider Enchant isn't active and enabled.
        /// This is here because generic boosts like Rage Potion DO increase the total summon crit chance value, even though it's normally not checked!
        /// </summary>
        /// <param name="player"></param>
        /// <param name="damageClass"></param>
        /// <returns></returns>
        public static float ActualClassCrit(this Player player, DamageClass damageClass)
            => (damageClass == DamageClass.Summon || damageClass == DamageClass.SummonMeleeSpeed) && !(player.FargoSouls().MinionCrits)
            ? 0
            : player.GetTotalCritChance(damageClass);

        private static readonly FieldInfo? _critOverrideField =
            typeof(NPC.HitModifiers).GetField("_critOverride", LumUtils.UniversalBindingFlags);
        /// <summary>
        /// Allows tweaking _critOverride directly to get around its restrictions.
        /// True forces a crit, false forces non-crit, null disables forced crit/non-crit behaviour.
        /// Null by default.
        /// </summary>
        /// <param name="modifiers"></param>
        public static void SetCritOverride(ref this NPC.HitModifiers modifiers, bool? _critOverride = null)
        {
            object? unboxedModifiers = modifiers;
            _critOverrideField?.SetValue(unboxedModifiers, _critOverride);
            modifiers = (NPC.HitModifiers)unboxedModifiers;
        }

        public static bool FeralGloveReuse(this Player player, Item item)
            => player.autoReuseGlove && (item.CountsAsClass(DamageClass.Melee) || item.CountsAsClass(DamageClass.SummonMeleeSpeed));

        public static bool CannotUseItems(this Player player) => player.CCed || player.noItems || player.FargoSouls().NoUsingItems > 0 || (player.HeldItem != null && (!ItemLoader.CanUseItem(player.HeldItem, player) || !PlayerLoader.CanUseItem(player, player.HeldItem)));

        public static void Incapacitate(this Player player, bool preventDashing = true)
        {
            player.controlLeft = false;
            player.controlRight = false;
            player.controlJump = false;
            player.controlDown = false;
            player.controlUseItem = false;
            player.controlUseTile = false;
            player.controlHook = false;
            player.releaseHook = true;
            if (player.grapCount > 0)
                player.RemoveAllGrapplingHooks();
            if (player.mount.Active)
                player.mount.Dismount(player);
            player.FargoSouls().NoUsingItems = 2;
            if (preventDashing)
            {
                for (int i = 0; i < 4; i++)
                {
                    player.doubleTapCardinalTimer[i] = 0;
                    player.holdDownCardinalTimer[i] = 0;
                }
            }
            if (player.dashDelay < 10 && preventDashing)
                player.dashDelay = 10;
        }

        public static bool CountsAsClass(this DamageClass damageClass, DamageClass intendedClass)
        {
            return damageClass == intendedClass || damageClass.GetEffectInheritance(intendedClass);
        }

        public static DamageClass ProcessDamageTypeFromHeldItem(this Player player)
        {
            if (player.HeldItem.type == ModContent.ItemType<EternityAdvisor>()) // Prevent advisor shenanigans
                return DamageClass.Default;
            if (player.HeldItem.damage <= 0 || player.HeldItem.pick > 0 || player.HeldItem.axe > 0 || player.HeldItem.hammer > 0)
                return DamageClass.Summon;
            else if (player.HeldItem.DamageType.CountsAsClass(DamageClass.Melee))
                return DamageClass.Melee;
            else if (player.HeldItem.DamageType.CountsAsClass(DamageClass.Ranged))
                return DamageClass.Ranged;
            else if (player.HeldItem.DamageType.CountsAsClass(DamageClass.Magic))
                return DamageClass.Magic;
            else if (player.HeldItem.DamageType.CountsAsClass(DamageClass.Summon))
                return DamageClass.Summon;
            else if (player.HeldItem.DamageType != DamageClass.Generic && player.HeldItem.DamageType != DamageClass.Default)
                return player.HeldItem.DamageType;
            else
                return DamageClass.Summon;
        }

        public static void Animate(this Projectile proj, int ticksPerFrame, int startFrame = 0, int? frames = null)
        {
            frames ??= Main.projFrames[proj.type];

            if (++proj.frameCounter >= ticksPerFrame)
            {
                if (++proj.frame >= startFrame + frames)
                    proj.frame = startFrame;
                proj.frameCounter = 0;
            }
        }

        public static Rectangle ToWorldCoords(this Rectangle rectangle) => new(rectangle.X * 16, rectangle.Y * 16, rectangle.Width * 16, rectangle.Height * 16);

        public static Rectangle ToTileCoords(this Rectangle rectangle) => new(rectangle.X / 16, rectangle.Y / 16, rectangle.Width / 16, rectangle.Height / 16);

        /// <summary>
        /// Checks ProjectileID.Sets.CultistIsResistantTo[projectile.type] for homing.
        /// Additionally, allows manual checking specific projectile behaviours dynamically for homing.
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="player"></param>
        /// <param name="Dynamic"></param>
        /// <returns></returns>
        public static bool IsHoming(this Projectile projectile, Player? player = null, IEntitySource? source = null, bool Dynamic = true)
        {
            switch (projectile.type)
            {
                case ProjectileID.Celeb2Rocket:
                case ProjectileID.Celeb2RocketExplosive:
                case ProjectileID.Celeb2RocketLarge:
                case ProjectileID.Celeb2RocketExplosiveLarge:
                    if (Dynamic && projectile.FargoSouls().SourceItemType == ItemID.Celeb2 && projectile.ai[0] == 3f)
                        return true;
                    break;

                case ProjectileID.FinalFractal: // Zenith // could probably use source null check here and use source first to speed things up but oh well I'll do it later
                    if (Dynamic && player is not null && projectile.FargoSouls().SourceItemType == ItemID.Zenith)
                    {
                        Item heldItem = player.HeldItem;
                        bool noApprentice = false;
                        if (FargoSoulsPlayer.ApprenticeSupportItem is null)
                            noApprentice = true;

                        if (noApprentice && heldItem is not null && player.GetSource_ItemUse_WithPotentialAmmo(heldItem, 0) is EntitySource_ItemUse_WithAmmo)
                        {
                            if (player.itemAnimation <= (int)((float)heldItem.useAnimation * 2f / 3f))
                            {
                                return true;
                            }
                        }
#pragma warning disable CS8604
                        else if (!noApprentice && heldItem is not null && player.GetSource_ItemUse_WithPotentialAmmo(FargoSoulsPlayer.ApprenticeSupportItem, 0) is EntitySource_ItemUse_WithAmmo)
#pragma warning restore CS8604
                        {
                            if (player.itemAnimation <= (int)((float)FargoSoulsPlayer.ApprenticeSupportItem.useAnimation * 2f / 3f))
                            {
                                return true;
                            }
                        }
                    }
                    break;

                default:
                    if (ProjectileID.Sets.CultistIsResistantTo[projectile.type])
                        return true;
                    break;
            }
            return false;
        }
    }
}