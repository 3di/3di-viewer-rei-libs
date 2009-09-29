using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class SitCommand: Command
    {
        public SitCommand(TestClient testClient)
		{
			Name = "sit";
			Description = "Attempt to sit on the closest prim";
            Category = CommandCategory.Movement;
		}
			
        public override string Execute(string[] args, UUID fromAgentID)
		{
            Primitive closest = null;
		    double closestDistance = Double.MaxValue;
            bool emitSitPacket = false;

            if (args.Length != 0 && (args.Length != 1 || !Boolean.TryParse(args[0], out emitSitPacket)))
            {
                return "Usage: sit [emit_sit_packet (false|true)]";
            }
            
            Client.Network.CurrentSim.ObjectsPrimitives.ForEach(
                delegate(Primitive prim)
                {
                    float distance = Vector3.Distance(Client.Self.SimPosition, prim.Position);

                    if (closest == null || distance < closestDistance)
                    {
                        closest = prim;
                        closestDistance = distance;
                    }
                }
            );

            if (closest != null)
            {
                Client.Self.RequestSit(closest.ID, Vector3.Zero);
                if (emitSitPacket)
                {
                    Client.Self.Sit();
                }
                return "Sat on " + closest.ID + " (" + closest.LocalID + "). Distance: " + closestDistance;
            }
            else
            {
                return "Couldn't find a nearby prim to sit on";
            }
		}
    }
}
