using System;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    class PreferenceCommand : Command
    {
        public PreferenceCommand(TestClient testClient)
        {
            Name = "preference";
            Description = "Send generic message to set client preferences. Usage: preference message [...]";
            Category = CommandCategory.TestClient;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length == 0)
                return "Usage: preference message";

            for (int i = 0; i < args.Length; i++)
            {
                string message = args[i];
                GenericMessagePacket packet = new GenericMessagePacket();
                packet.AgentData.AgentID = Client.Self.AgentID;
                packet.AgentData.SessionID = Client.Self.SessionID;
                packet.AgentData.TransactionID = UUID.Zero;
                packet.MethodData.Invoice = UUID.Zero;
                packet.MethodData.Method = Utils.StringToBytes("ClientPreference");
                packet.ParamList = new GenericMessagePacket.ParamListBlock[1];
                packet.ParamList[0] = new GenericMessagePacket.ParamListBlock();
                packet.ParamList[0].Parameter = Utils.StringToBytes(message);
                Client.Network.SendPacket(packet, Client.Network.CurrentSim);
            }
            return String.Format("Set preference [{0}]", String.Join(",",args));
        }
    }
}
