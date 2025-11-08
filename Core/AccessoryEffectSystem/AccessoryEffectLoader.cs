using FargowiltasSouls.Content.Items;
using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Content.UI;
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace FargowiltasSouls.Core.AccessoryEffectSystem
{
    public static class AccessoryEffectLoader
    {
        public static List<AccessoryEffect> AccessoryEffects = [];
        internal static void Register(AccessoryEffect effect)
        {
            effect.Index = AccessoryEffects.Count;
            AccessoryEffects.Add(effect);

            ToggleLoader.RegisterToggle(new Toggle(effect, effect.Mod.Name));

        }
        /// <summary>
        /// Adds the effect to the player. Lasts one frame. 
        /// Returns whether the effect was successfully added or not (it's not added if it's blocked by, for example, the toggle)
        /// </summary>
        public static bool AddEffect<T>(this Player player, Item item) where T : AccessoryEffect
        {
            AccessoryEffect effect = ModContent.GetInstance<T>();
            AccessoryEffectPlayer effectPlayer = player.AccessoryEffects();
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            effectPlayer.EquippedEffects[effect.Index] = true;

            if (effectPlayer.EffectItems[effect.Index] == null || item.ModItem is BaseEnchant) // enchants take priority
                effectPlayer.EffectItems[effect.Index] = item;

            if (effectPlayer.DeactivatedEffects[effect.Index])
                return false;

            if (effect.HasToggle)
            {
                if (modPlayer.MutantPresence && (effect.MutantsPresenceAffects || effect.MinionEffect))
                    return false;

                if (effect.MinionEffect)
                {
                    if (modPlayer.GalacticMinionsDeactivated)
                    {
                        effectPlayer.DeactivatedEffects[effect.Index] = true;
                        modPlayer.DeactivatedMinionEffectCount++;
                        return false;
                    }
                    if (modPlayer.Toggler_MinionsDisabled)
                        return false;
                }
                if (effect.ExtraAttackEffect && modPlayer.Toggler_ExtraAttacksDisabled)
                    return false;

                if (effect.ExtraJumpEffect && player.wingTimeMax > 0 && modPlayer.Toggler_ExtraJumpsDisabled)
                    return false;

                SoulsItem soulsItem = item != null && item.ModItem is SoulsItem si ? si : null;
                if (!player.GetToggleValue(effect, true))
                {
                    if (soulsItem != null)
                        soulsItem.HasDisabledEffects = ClientConfig.Instance.ItemDisabledTooltip;
                    return false;
                }
                if (soulsItem != null)
                    soulsItem.HasDisabledEffects = ClientConfig.Instance.ItemDisabledTooltip && AccessoryEffects.Any(e => !player.GetToggleValue(e, true) && e.EffectItem(player) == item);
            }

            if (!effectPlayer.ActiveEffects[effect.Index])
            {
                effectPlayer.ActiveEffects[effect.Index] = true;
                return true;
            }
            return false;
        }
        public static bool HasEffect<T>(this Player player) where T : AccessoryEffect => player.HasEffect(ModContent.GetInstance<T>());
        public static bool HasEffect(this Player player, AccessoryEffect accessoryEffect) => player.AccessoryEffects().ActiveEffects[accessoryEffect.Index];
        public static Item EffectItem<T>(this Player player) where T : AccessoryEffect => player.AccessoryEffects().EffectItems[ModContent.GetInstance<T>().Index];
        public static bool HasEffectEnchant<T>(this Player player) where T : AccessoryEffect => player.HasEffect<T>() && player.EffectItem<T>()?.ModItem is BaseEnchant;
        public static IEntitySource GetSource_EffectItem<T>(this Player player) where T : AccessoryEffect => ModContent.GetInstance<T>().GetSource_EffectItem(player);
        public static T GetEffect<T>() where T : AccessoryEffect => ModContent.GetInstance<T>();
        public static AccessoryEffect GetEffect(string internalName) => ModContent.Find<AccessoryEffect>(internalName);
        /// <summary>
        /// If this is a bound Active Skill, returns the key it's bound to. <br></br>
        /// Otherwise, returns null.
        /// </summary>
        public static ModKeybind GetKeybind<T>(FargoSoulsPlayer soulsPlayer) where T : AccessoryEffect
        {
            ModKeybind keybind = null;
            for (int i = 0; i < soulsPlayer.ActiveSkills.Length; i++)
            {
                if (soulsPlayer.ActiveSkills[i] is T)
                {
                    keybind = FargowiltasSouls.ActiveSkillKeys[i];
                    break;
                }
            }
            return keybind;
        }
        /// <summary>
        /// Gets the value of the cooldown tied to this accessory effect.
        /// </summary>
        public static float GetCooldown<T>(this Player player) where T : AccessoryEffect => player.AccessoryEffects().Cooldowns[ModContent.GetInstance<T>().Index];
        /// <summary>
        /// Sets the value of the cooldown tied to this accessory effect.
        /// </summary>
        public static void SetCooldown<T>(this Player player, float value) where T : AccessoryEffect => player.AccessoryEffects().Cooldowns[ModContent.GetInstance<T>().Index] = value;
        /// <summary>
        /// Adds the value amount to the cooldown tied to this accessory effect. Can be positive or negative.
        /// </summary>
        public static void IncrementCooldown<T>(this Player player, float value) where T : AccessoryEffect => player.AccessoryEffects().Cooldowns[ModContent.GetInstance<T>().Index] += value;
        /// <summary>
        /// Adds the value amount to the cooldown tied to this accessory effect, until it reaches a goal value. <br></br>
        /// Most commonly used to decrement the cooldown to 0. <br></br>
        /// Returns true if the value was incremented, and false if it has reached the goal.
        /// </summary>
        public static bool IncrementCooldownTowards<T>(this Player player, float value, float goalValue) where T : AccessoryEffect
        {
            if (value == 0) // why are we here
                return false;
            var accs = player.AccessoryEffects();
            var index = ModContent.GetInstance<T>().Index;
            if (value > 0) // increment
            {
                if (accs.Cooldowns[index] < goalValue)
                {
                    accs.Cooldowns[index] += value;
                    return true;
                } 
                else
                    accs.Cooldowns[index] = goalValue;
            }
            else // decrement
            {
                if (accs.Cooldowns[index] > goalValue)
                {
                    accs.Cooldowns[index] += value;
                    return true;
                }
                else
                    accs.Cooldowns[index] = goalValue;
            }
            return false;
        }
    }
}
