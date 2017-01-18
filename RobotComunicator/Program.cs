using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace RobotComunicator
{
    class Program
    {

        static void Main(string[] args)
        {
            StartListening();
        }

        public static void CommunicateWithRobot(Socket handler)
        {
            try
            {
                RobotCommunicator robotCommunicator = new RobotCommunicator(handler);
                if (!robotCommunicator.Autheticicate())
                {
                    return;
                }
                Location loc = robotCommunicator.GetLocation();

                WorldDirection poDirection = loc.Direction;
                if (loc.X > 0) { robotCommunicator.Rotate(WorldDirection.West);poDirection = WorldDirection.West;}
                if (loc.X < 0) { robotCommunicator.Rotate(WorldDirection.East); poDirection = WorldDirection.East;}

                int count = Math.Abs(loc.X);
                for (int i = 0; i < count; i++)
                {
                    robotCommunicator.Move();
                }

                count = Math.Abs(loc.Y);
                if (loc.Y > 0) robotCommunicator.Rotate(WorldDirection.South, poDirection);
                if (loc.Y < 0) robotCommunicator.Rotate(WorldDirection.North, poDirection);
                for (int i = 0; i < count; i++)
                {
                    robotCommunicator.Move();
                }

                robotCommunicator.PickUpMessege();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public static void StartListening()
        {

            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 1234);

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(ipEndPoint);
            listener.Listen(100);
            while (true)
            {
                Socket handler = listener.Accept();
                Thread t = new Thread((() => CommunicateWithRobot(handler)));
                t.Start();
            }
        }
    }
}
