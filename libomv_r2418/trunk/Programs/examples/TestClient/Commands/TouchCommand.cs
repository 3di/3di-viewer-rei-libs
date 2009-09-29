using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace OpenMetaverse.TestClient
{
    public class TouchCommand: Command
    {
        public TouchCommand(TestClient testClient)
		{
			Name = "touch";
			Description = "Attempt to touch a prim with specified UUID";
            Category = CommandCategory.Objects;
		}
		
        public override string Execute(string[] args, UUID fromAgentID)
		{
            UUID target;

            if (args.Length == 0)
            {

                Primitive closest = null;
                double closestDistance = Double.MaxValue;

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
                    Client.Self.Touch(closest.LocalID);
                    return "Touched prim " + closest.ID + " (" + closest.LocalID + "). Distance: " + closestDistance;
                }
                else
                {
                    return "Couldn't find a nearby prim to touch";
                }
            }
            else if (UUID.TryParse(args[0], out target))
            {
                Primitive targetPrim = Client.Network.CurrentSim.ObjectsPrimitives.Find(
                    delegate(Primitive prim)
                    {
                        return prim.ID == target;
                    }
                );

                if (targetPrim != null)
                {
                    Client.Self.Touch(targetPrim.LocalID);
                    return "Touched prim " + targetPrim.LocalID;
                }
            }

            return "Couldn't find a prim to touch with UUID " + args[0];
		}
    }
}
