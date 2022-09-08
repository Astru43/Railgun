using log4net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using Railgun.Projectiles;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using static Humanizer.In;

namespace Railgun.Items {

    public class RailgunPlayer : ModPlayer {
        public float charge = 0.0f;

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)MessageType.RailgunPlayerSync);
            packet.Write((byte)Player.whoAmI);
            packet.Write(charge);
            packet.Send(toWho, fromWho);
        }

        public override void clientClone(ModPlayer clientClone) {
            RailgunPlayer clone = clientClone as RailgunPlayer;
            clone.charge = charge;
        }

        public override void SendClientChanges(ModPlayer clientPlayer) {
            RailgunPlayer clone = clientPlayer as RailgunPlayer;
            if (clone.charge != charge) {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)MessageType.RailgunPlayerChargeUpdate);
                packet.Write((byte)Player.whoAmI);
                packet.Write(charge);
                packet.Send();
            }
        }

    }

    public class Railgun : ModItem {
        internal ILog Logger => Mod.Logger;

        public override void SetStaticDefaults() {
            // DisplayName.SetDefault("railgun"); // By default, capitalization in classnames will add spaces to the display name. You can customize the display name here by uncommenting this line.
            Tooltip.SetDefault("This is a basic modded sword.");
        }

        public override void SetDefaults() {
            Item.damage = 30000;
            Item.DamageType = DamageClass.Magic;

            Item.width = 6;
            Item.height = 16;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 6;
            Item.value = Item.sellPrice(1, 50);
            Item.rare = ItemRarityID.Expert;

            Item.expert = true;
            Item.UseSound = null;
            Item.autoReuse = false;
            Item.noUseGraphic = false;
            Item.noMelee = true;
            Item.channel = true;

            Item.shoot = ItemID.PurificationPowder;
            Item.useAmmo = AmmoID.Coin;
            Item.shootSpeed = 6;
        }

        public override bool MagicPrefix() => true;

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[ModContent.ProjectileType<RailgunHold>()] <= 0;
        public override bool CanConsumeAmmo(Item ammo, Player player) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            damage = player.GetWeaponDamage(Item);

            switch (source.AmmoItemIdUsed) {
            case ItemID.PlatinumCoin:
                var newDamage = damage * 1.5f;
                damage = (int)newDamage;
                break;
            case ItemID.GoldCoin:
                newDamage = damage * 1.25f;
                damage = (int)newDamage;
                break;
            case ItemID.SilverCoin:
                newDamage = damage * 1.1f;
                damage = (int)newDamage;
                break;
            }

            if (Main.myPlayer == player.whoAmI) {
                Projectile.NewProjectile(
                    source, position, velocity,
                    ModContent.ProjectileType<RailgunHold>(),
                    damage, knockback,
                    Main.myPlayer
                );
            }
            return false;
        }
    }

    public class RailgunHold : ModProjectile {
        public override string Texture => $"Railgun/Items/{ModContent.GetModItem(ModContent.ItemType<Railgun>()).Name}";

        private readonly SoundStyle charge = new("Railgun/Items/charge");
        private readonly SoundStyle hold = new("Railgun/Items/hold");
        private readonly SoundStyle shoot = new("Railgun/Items/shoot");
        private SlotId chargeSound;
        private SlotId holdSound;

        private const float AimResponsivness = 1f;

        private int ammoId = ItemID.PlatinumCoin;

        private Player Player => Main.player[Projectile.owner];
        private RailgunPlayer RailgunPlayer => Player.GetModPlayer<RailgunPlayer>();

        private int SoundTimer1 {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private RailgunState State {
            get => (RailgunState)Projectile.ai[1];
            set => Projectile.ai[1] = (int)value;
        }

        public override void SetStaticDefaults() {
            base.SetStaticDefaults();
            Main.projFrames[Projectile.type] = 1;
        }
        public override void SetDefaults() {
            base.SetDefaults();
            Projectile.width = 0;
            Projectile.height = 0;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void OnSpawn(IEntitySource source) {
            if (source is EntitySource_ItemUse_WithAmmo _source) {
                ammoId = _source.AmmoItemIdUsed;
            }
        }

        public override void Kill(int timeLeft) {
            var charge = RailgunPlayer.charge;
            
            if (SoundEngine.TryGetActiveSound(chargeSound, out ActiveSound sound) && SoundTimer1 < 120f)
                sound.Stop();
            if (SoundEngine.TryGetActiveSound(holdSound, out sound))
                sound.Stop();
            if (charge >= .25f)
                SoundEngine.PlaySound(shoot, Projectile.position);
        }

        public override void AI() {
            Player player = Player;
            RailgunPlayer railgunPlayer = RailgunPlayer;
            Vector2 rrp = player.RotatedRelativePoint(player.MountedCenter, true);

            if (SoundTimer1 == 1 && State == RailgunState.Charge) {
                chargeSound = SoundEngine.PlaySound(charge, Projectile.position);
            }
            if (SoundTimer1 == 0 && State == RailgunState.Hold) {
                holdSound = SoundEngine.PlaySound(hold, Projectile.position);
            } else {
                if (SoundEngine.TryGetActiveSound(chargeSound, out ActiveSound sound))
                    sound.Position = Projectile.position;
                if (SoundEngine.TryGetActiveSound(holdSound, out sound))
                    sound.Position = Projectile.position;
            }

            SoundTimer1 += 1;
            if (SoundTimer1 == 132) {
                SoundTimer1 = 0;
                State = RailgunState.Hold;
            } else if (SoundTimer1 == 30 && State == RailgunState.Hold) {
                SoundTimer1 = 0;
            }

            UpdatePlayerVisual(player, rrp);

            if (Main.rand.NextFloat() <= railgunPlayer.charge) {
                Dust dust;
                if (Main.rand.NextBool())
                    dust = Dust.NewDustPerfect(Projectile.position + Projectile.velocity, DustID.Electric,
                        Main.rand.NextVector2Unit() * Main.rand.NextFloat(5f),
                        Scale: .75f
                    );
                else
                    dust = Dust.NewDustPerfect(Projectile.position + Projectile.velocity, DustID.Electric,
                        -Projectile.velocity.RotatedByRandom(MathHelper.Pi / 8) * Main.rand.NextFloat(2f),
                        Scale: .75f
                    );
                dust.noGravity = true;
            }

            if (Main.myPlayer == Projectile.owner) {
                UpdateAim(rrp, player.HeldItem.shootSpeed);

                if (!player.channel || player.CCed || player.noItems) {
                    Projectile.Kill();
                    if (railgunPlayer.charge >= 0.25f) {
                        var damage = Projectile.damage;
                        var scale = (MathF.Log(damage) - MathF.Log(1000)) / (1f - 0.25f);
                        damage = (int)MathF.Round(MathF.Exp(MathF.Log(1000) + scale * (railgunPlayer.charge - 0.25f)));

                        int ammoProjectile = ammoId switch {
                            ItemID.PlatinumCoin => ModContent.ProjectileType<RailgunProjectilePlatinum>(),
                            ItemID.GoldCoin => ModContent.ProjectileType<RailgunProjectileGold>(),
                            ItemID.SilverCoin => ModContent.ProjectileType<RailgunProjectileSilver>(),
                            ItemID.CopperCoin => ModContent.ProjectileType<RailgunProjectileCopper>(),
                            _ => ModContent.ProjectileType<RailgunProjectilePlatinum>(),
                        };
                        player.ConsumeItem(ammoId);

                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(), rrp, Projectile.velocity,
                            ammoProjectile,
                            damage, Projectile.knockBack,
                            Projectile.owner,
                            1f, 0f
                        );
                    }
                    railgunPlayer.charge = 0f;
                } else {
                    railgunPlayer.charge += 0.0084f;
                    if (railgunPlayer.charge > 1f)
                        railgunPlayer.charge = 1f;
                }
            }

        }

        enum RailgunState {
            Charge,
            Hold
        }

        private void UpdateAim(Vector2 source, float speed) {
            Vector2 aim = Vector2.Normalize(Main.MouseWorld - source);
            if (aim.HasNaNs())
                aim = -Vector2.UnitY;

            aim = Vector2.Normalize(Vector2.Lerp(Vector2.Normalize(Projectile.velocity), aim, AimResponsivness));
            aim *= speed;

            if (aim != Projectile.velocity) {
                Projectile.netUpdate = true;
            }

            Projectile.velocity = aim;
        }

        private void UpdatePlayerVisual(Player player, Vector2 handPos) {
            Projectile.Center = handPos;
            Projectile.position += Projectile.velocity * .7f; // .7f = On hand (more or less); .5f = Right besides hand;

            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.spriteDirection = Projectile.direction;

            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;

            player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
        }

        public override bool PreDraw(ref Color lightColor) {
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Texture2D texture = TextureAssets.Projectile[ProjectileID.None].Value;
            //Texture2D texture = TextureAssets.Projectile[Type].Value;
            int frameHeight = texture.Height / Main.projFrames[Type];
            int spriteSheetOffset = frameHeight * Projectile.frame;
            Vector2 sheetInsertPosition = (Projectile.Center + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition).Floor();

            Main.EntitySpriteDraw(texture, sheetInsertPosition, new Rectangle?(new Rectangle(0, spriteSheetOffset, texture.Width, frameHeight)), lightColor, Projectile.rotation, new Vector2(texture.Width / 2f, frameHeight / 2f), Projectile.scale, effects, 1);
            return false;

            //return base.PreDraw(ref lightColor);
        }
    }
}