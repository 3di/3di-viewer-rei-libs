using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class SitOnCommand : Command
    {
        public SitOnCommand(TestClient testClient)
        {
            Name = "siton";
            Description = "Attempt to sit on a particular prim, with specified UUID";
            Category = CommandCategory.Movement;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            bool emitSitPacket = true;

            if (args.Length != 1 && (args.Length != 2 || !Boolean.TryParse(args[1], out emitSitPacket)))
            {
                return "Usage: siton UUID [emit_sit_packet (false|true)]";
            }

            UUID target;

            if (UUID.TryParse(args[0], out target))
            {
                Primitive targetPrim = Client.Network.CurrentSim.ObjectsPrimitives.Find(
                    delegate(Primitive prim)
                    {
                        return prim.ID == target;
                    }
                );

                if (targetPrim != null)
                {
                    Client.Self.RequestSit(targetPrim.ID, Vector3.Zero);
                    if (emitSitPacket)
                    {
                        Client.Self.Sit();
                    }
                    return "Requested to sit on prim " + targetPrim.ID.ToString() +
                        " (" + targetPrim.LocalID + ")";
                }
            }

            return "Couldn't find a prim to sit on with UUID " + args[0];
        }
    }
}
