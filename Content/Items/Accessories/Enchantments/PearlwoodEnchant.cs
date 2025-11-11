using Fargowiltas.Content.Items.Tiles;
using FargowiltasSouls.Common.Graphics.Particles;
using FargowiltasSouls.Content.Projectiles;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler.Content;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Enchantments
{
    public class PearlwoodEnchant : BaseEnchant
    {
        public override Color nameColor => new(173, 154, 95);


        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.rare = ItemRarityID.Pink;
            Item.value = 20000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.AddEffect<PearlwoodEffect>(Item);
            player.AddEffect<PearlwoodStarEffect>(Item);
            player.AddEffect<PearlwoodManaEffect>(Item);
            player.AddEffect<PearlwoodRainbowEffect>(Item);
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.PearlwoodHelmet)
                .AddIngredient(ItemID.PearlwoodBreastplate)
                .AddIngredient(ItemID.PearlwoodGreaves)
                .AddIngredient(ItemID.PearlwoodSword)
                .AddIngredient(ItemID.LightningBug)
                .AddRecipeGroup("FargowiltasSouls:StarfruitOrDragonfruit")

                .AddTile<EnchantedTreeSheet>()
                .Register();
        }
        public override int DamageTooltip(out DamageClass damageClass, out Color? tooltipColor, out int? scaling)
        {
            damageClass = DamageClass.Magic;
            tooltipColor = null;
            scaling = null;
            return PearlwoodEffect.BaseDamage(Main.LocalPlayer);
        }
    }
    public class PearlwoodEffect : AccessoryEffect
    {
        public override Header ToggleHeader => null;
        public static int BaseDamage(Player player) => (int)((player.FargoSouls().ForceEffect<PearlwoodEnchant>() ? 120 : 60) * player.ActualClassDamage(DamageClass.Magic));
        public override void PostUpdateEquips(Player player)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            if (modPlayer.PearlwoodCritDuration > 0)
            {
                modPlayer.PearlwoodCritDuration--;
                if (player.HasEffect<PearlwoodRainbowEffect>())
                {
                    int p = Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, player.velocity, ProjectileID.RainbowBack, 0, 1);
                    if (p != Main.maxProjectiles)
                    {
                        Main.projectile[p].friendly = false;
                        Main.projectile[p].hostile = false;
                        Main.projectile[p].FargoSouls().Rainbow = true;
                        Main.projectile[p].Opacity /= 4;
                    }
                }
            }
                
        }
        public override void OnHitNPCEither(Player player, NPC target, NPC.HitInfo hitInfo, DamageClass damageClass, int baseDamage, Projectile projectile, Item item)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();

            if (modPlayer.PearlwoodCritDuration <= 0)
                return;
        }
        public override void ModifyHitNPCWithProj(Player player, Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            int critChance = (int)player.ActualClassCrit(proj.DamageType);
            PearlwoodCritReroll(player, ref modifiers, critChance, target);
        }
        public override void ModifyHitNPCWithItem(Player player, Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            PearlwoodCritReroll(player, ref modifiers, (int)player.ActualClassCrit(item.DamageType), target);
        }
        public static void PearlwoodCritReroll(Player player, ref NPC.HitModifiers modifiers, int critChance, NPC target)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();

            if (modPlayer.PearlwoodCritDuration <= 0 || critChance <= 0 || critChance >= 100)
                return;

            if (typeof(NPC.HitModifiers).GetField("_critOverride", LumUtils.UniversalBindingFlags)?.GetValue(modifiers) as bool? is not null)
                return;

            int rerolls = modPlayer.ForceEffect<PearlwoodEnchant>() ? 2 : 1;
            for (int i = 0; i < rerolls; i++)
            {
                if (Main.rand.Next(0, 100) < critChance)
                {
                    modifiers.SetCrit();
                    for (int j = 0; j < 7; j++)
                    {
                        Color color = Main.rand.NextFromList(Color.Goldenrod, Color.Pink, Color.Cyan);
                        Particle p = new SmallSparkle(
                            worldPosition: Main.rand.NextVector2FromRectangle(target.Hitbox),
                            velocity: (Main.rand.NextFloat(10, 25) * Vector2.UnitX).RotatedByRandom(MathHelper.TwoPi),
                            drawColor: color,
                            scale: 1f,
                            lifetime: Main.rand.Next(10, 15),
                            rotation: 0,
                            rotationSpeed: Main.rand.NextFloat(-MathHelper.Pi / 8, MathHelper.Pi / 8)
                            );
                        p.Spawn();
                    }
                    break; // no need for this to run twice
                }
            }
        }
        public static void OnPickup(Player player)
        {
            if (!player.HasEffect<PearlwoodEffect>())
                return;
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            modPlayer.PearlwoodCritDuration = 60 * 3;

            if (player.HasEffect<PearlwoodStarEffect>())
            {
                SoundEngine.PlaySound(SoundID.Item105 with { Pitch = -0.3f }, player.Center);
                Vector2 vel = -Vector2.UnitY * 7;
                int nearestNPCID = FargoSoulsUtil.FindClosestHostileNPC(player.Center, 1000, true, true);
                if (nearestNPCID.IsWithinBounds(Main.maxNPCs))
                {
                    NPC nearestNPC = Main.npc[nearestNPCID];
                    if (nearestNPC.Alive())
                        vel = player.DirectionTo(nearestNPC.Center) * 7;
                }
                Projectile.NewProjectile(player.GetSource_Misc("Pearlwood"), player.Center.X, player.Center.Y, vel.X, vel.Y, ProjectileID.FairyQueenMagicItemShot, BaseDamage(player), 0, player.whoAmI, 0f, 0);
            }
        }
    }
    public class PearlwoodStarEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<TimberHeader>();
        public override int ToggleItemType => ModContent.ItemType<PearlwoodEnchant>();
        public override bool ExtraAttackEffect => true;
    }
    public class PearlwoodRainbowEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<TimberHeader>();
        public override int ToggleItemType => ModContent.ItemType<PearlwoodEnchant>();
    }
    public class PearlwoodManaEffect : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<TimberHeader>();
        public override int ToggleItemType => ModContent.ItemType<PearlwoodEnchant>();
        public override void PostUpdateEquips(Player player)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            if (modPlayer.PearlwoodManaCD > 0)
                modPlayer.PearlwoodManaCD--;
        }
        public override void OnHitNPCEither(Player player, NPC target, NPC.HitInfo hitInfo, DamageClass damageClass, int baseDamage, Projectile projectile, Item item)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            if (modPlayer.PearlwoodManaCD <= 0 && !target.immortal)
            {
                modPlayer.PearlwoodManaCD = 60 * 6;

                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    Item.NewItem(player.GetSource_OnHit(target), target.Hitbox, ItemID.ManaCloakStar);
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    var netMessage = Mod.GetPacket();
                    netMessage.Write((byte)FargowiltasSouls.PacketID.RequestPearlwoodStar);
                    netMessage.Write((byte)player.whoAmI);
                    netMessage.Write((byte)target.whoAmI);
                    netMessage.Send();
                }
            }
        }
    }
}
