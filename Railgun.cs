using Terraria;
using Railgun.Items;
using System.IO;
using Terraria.ModLoader;
using Terraria.ID;

namespace Railgun {
    internal enum MessageType : byte {
        RailgunPlayerSync,
        RailgunPlayerChargeUpdate,
    }
    
    public class Railgun : Mod {


        public override void HandlePacket(BinaryReader reader, int whoAmI) {
            MessageType type = (MessageType)reader.ReadByte();
            switch (type) {
            case MessageType.RailgunPlayerSync:
                byte playerNumber = reader.ReadByte();
                RailgunPlayer player = Main.player[playerNumber].GetModPlayer<RailgunPlayer>();
                player.charge = reader.ReadSingle();
                break;
            case MessageType.RailgunPlayerChargeUpdate:
                playerNumber = reader.ReadByte();
                player = Main.player[playerNumber].GetModPlayer<RailgunPlayer>();
                player.charge = reader.ReadSingle();

                if (Main.netMode == NetmodeID.Server) {
                    var packet = GetPacket();
                    packet.Write((byte)type);
                    packet.Write((byte)playerNumber);
                    packet.Write(player.charge);
                    packet.Send(-1, playerNumber);
                }

                break;
            default:
                Logger.WarnFormat("Unknow message type {0}", type);
                break;
            }
        }

    }
}