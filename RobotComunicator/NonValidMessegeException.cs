using System;

namespace RobotComunicator
{
    class NonValidMessegeException : Exception
    {
        public NonValidMessegeException(string message) : base(message)
        {
        }
    }
}