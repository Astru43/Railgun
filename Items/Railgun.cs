using log4net;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Railgun.Items {
    public class Railgun : ModItem {
        public override void SetStaticDefaults() {
            // DisplayName.SetDefault("railgun"); // By default, capitalization in classnames will add spaces to the display name. You can customize the display name here by uncommenting this line.
            Tooltip.SetDefault("This is a basic modded sword.");
        }

        public override void SetDefaults() {
            Item.damage = 5000;
            Item.DamageType = DamageClass.Generic;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6;
            Item.value = 10000;
            Item.rare = ItemRarityID.Purple;
            Item.UseSound = SoundID.Item122;
            Item.autoReuse = true;

            Item.shoot = 10;
            Item.useAmmo = AmmoID.Coin;
            Item.shootSpeed = 32;
            Item.channel = true;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player) {
            if (ammo.type == ItemID.PlatinumCoin)
                return false;
            return base.CanConsumeAmmo(ammo, player);
        }

        ILog Logger => Mod.Logger;

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            var origDamage = damage;
            base.ModifyShootStats(player, ref position, ref velocity, ref type, ref damage, ref knockback);
            Logger.Debug(damage + "/" + origDamage);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {

            switch (source.AmmoItemIdUsed) {
            case ItemID.PlatinumCoin:
                damage = (int)Math.Round(damage * 0.5f) + damage;
                break;
            }

            Logger.Debug(damage);
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }

    }
}