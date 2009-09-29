using System;

namespace OpenMetaverse.TestClient.Commands.Movement
{
    class RightCommand : Command
    {
        public RightCommand(TestClient client)
        {
            Name = "right";
            Description = "Sends the move right command to the server for a single packet or a given number of seconds. Usage: right [seconds [enable_sparse_update (false|true)]]";
            Category = CommandCategory.Movement;
        }

        public override string Execute(string[] args, UUID fromAgentID)
        {
            if (args.Length > 2)
                return "Usage: right [seconds [enable_sparse_update (false|true)]]";

            if (args.Length == 0)
            {
                Client.Self.Movement.SendManualUpdate(AgentManager.ControlFlags.AGENT_CONTROL_LEFT_NEG, Client.Self.Movement.Camera.Position,
                    Client.Self.Movement.Camera.AtAxis, Client.Self.Movement.Camera.LeftAxis, Client.Self.Movement.Camera.UpAxis,
                    Client.Self.Movement.BodyRotation, Client.Self.Movement.HeadRotation, Client.Self.Movement.Camera.Far, AgentManager.AgentFlags.None,
                    AgentManager.AgentState.None, true);
            }
            else
            {
                // Parse the number of seconds
                int duration;
                bool enableSparseUpdate = false;
                if (!Int32.TryParse(args[0], out duration) || (args.Length > 1 && !Boolean.TryParse(args[1], out enableSparseUpdate)))
                    return "Usage: right [seconds [enable_sparse_update (false|true)]]";
                // Convert to milliseconds
                duration *= 1000;

                int start = Environment.TickCount;

                Client.Self.Movement.LeftNeg = true;

                while (Environment.TickCount - start < duration)
                {
                    // The movement timer will do this automatically, but we do it here as an example
                    // and to make sure updates are being sent out fast enough
                    if (!enableSparseUpdate)
                        Client.Self.Movement.SendUpdate(false); 
                    System.Threading.Thread.Sleep(100);
                }

                Client.Self.Movement.LeftNeg = false;
            }

            return "Moved right";
        }
    }
}
