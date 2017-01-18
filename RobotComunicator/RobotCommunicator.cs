using System;
using System.Linq;
using System.Net.Sockets;

namespace RobotComunicator
{
    class RobotCommunicator
    {
        private MessegeHandler messegeHandler;
        private string _user;
        private Location _location;

        public RobotCommunicator(Socket handler)
        {
            _location = Location.Default;
            messegeHandler = new MessegeHandler(handler);
        }

        public bool Autheticicate()
        {
            messegeHandler.Send(MessegeHandler.SERVER_USER);
            _user = messegeHandler.GetMessege(100);
            messegeHandler.Send(MessegeHandler.SERVER_PASSWORD);
            int passwd = _user.Sum(o => (int) o);
            try
            {
                string msg = messegeHandler.GetMessege(7);
                if (msg.Contains(" "))
                {
                    messegeHandler.AbortConnection(MessegeHandler.SERVER_LOGIN_FAILED);
                    return false;
                }
                int sentPassword = Int32.Parse(msg);
                if (passwd != sentPassword)
                {
                    messegeHandler.AbortConnection(MessegeHandler.SERVER_LOGIN_FAILED);
                    return false;
                }
            }
            catch (Exception)
            {
                messegeHandler.AbortConnection(MessegeHandler.SERVER_SYNTAX_ERROR);
                return false;
            }
            messegeHandler.Send(MessegeHandler.SERVER_OK);
            
            return true;
        }

        public Location GetLocation()
        {
            if (_location == Location.Default)
            {
                Move();
                Location temp = new Location(_location);
                Move();
                _location.SetFacing(temp);
            }
            return _location;
        }

        public Location Move()
        {
            Location newLocation;
            do
            {
                messegeHandler.Send(MessegeHandler.SERVER_MOVE);
                try
                {
                    newLocation = new Location(messegeHandler.GetMessege(12));
                }
                catch (Exception ex )
                {
                    messegeHandler.AbortConnection(MessegeHandler.SERVER_SYNTAX_ERROR);
                    throw new NonValidMessegeException(MessegeHandler.SERVER_SYNTAX_ERROR);
                }

            } while (newLocation == _location);
            _location = newLocation;
            return _location;
        }

        public void Rotate(WorldDirection pointMeTo, WorldDirection currentlyPointingTo)
        {
            _location.Direction = currentlyPointingTo;
            Rotate(pointMeTo);
        }

        public void Rotate(WorldDirection direction)
        {
            int count = _location.Direction - direction;
            if (count < 0)
            {
                while (count < 0)
                {
                    count++;
                    messegeHandler.Send(MessegeHandler.SERVER_TURN_RIGHT);
                    _location.ParseString(messegeHandler.GetMessege(12));
                }
            }
            else
            {
                while (count > 0)
                {
                    count --;
                    messegeHandler.Send(MessegeHandler.SERVER_TURN_LEFT);
                    _location.ParseString(messegeHandler.GetMessege(12));
                }
            }
            _location.Direction = direction;
        }

        public void PickUpMessege()
        {
            messegeHandler.Send(MessegeHandler.SERVER_PICK_UP);
            messegeHandler.GetMessege(100);
            messegeHandler.AbortConnection(MessegeHandler.SERVER_OK);
        }
    }
}