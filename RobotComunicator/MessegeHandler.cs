using System;
using System.Net.Sockets;
using System.Text;

namespace RobotComunicator
{
    class MessegeHandler
    {
        #region Const definitions
        //Server side strings
        public const string END = "\r\n";
        public const string SERVER_USER = "100 LOGIN" + END;
        public const string SERVER_PASSWORD = "101 PASSWORD" + END;
        public const string SERVER_MOVE = "102 MOVE" + END;
        public const string SERVER_TURN_LEFT = "103 TURN LEFT" + END;
        public const string SERVER_TURN_RIGHT = "104 TURN RIGHT" + END;
        public const string SERVER_PICK_UP = "105 GET MESSAGE" + END;
        public const string SERVER_OK = "200 OK" + END;
        public const string SERVER_LOGIN_FAILED = "300 LOGIN FAILED" + END;
        public const string SERVER_SYNTAX_ERROR = "301 SYNTAX ERROR" + END;
        public const string SERVER_LOGIC_ERROR = "302 LOGIC ERROR" + END;

        //Client side strings
        public const string CLIENT_RECHARGING = "RECHARGING";
        public const string CLIENT_FULL_POWER = "FULL POWER";

        public const int TIMEOUT = 1000;
        public const int TIMEOUT_RECHARGING = 5000;
        #endregion


        private Socket handler;
        private const int BufferLength = 100;
        private string stringBuffer = "";
        private bool _isRecharging;

        public bool IsRecharging
        {
            get { return _isRecharging; }
            private set
            {
                _isRecharging = value;
                handler.ReceiveTimeout = value? TIMEOUT_RECHARGING : TIMEOUT;
            }
        }


        public MessegeHandler(Socket handler)
        {
            this.handler = handler;
            handler.ReceiveTimeout = handler.SendTimeout = TIMEOUT;
            _isRecharging = false;
        }

        public string GetMessege(int maxLength)
        {
            while (true)
            {
                int index = stringBuffer.IndexOf(END, StringComparison.CurrentCulture);
                while (index == -1)
                {
                    byte[] buffer = new byte[BufferLength];
                    try
                    {
                        handler.Receive(buffer);
                    }
                    catch (SocketException exception)
                    {
                        Console.WriteLine(exception.Message);
                        handler.Close();
                        throw new NonValidMessegeException("TIMEOUT");
                    }
                    stringBuffer = stringBuffer + Encoding.ASCII.GetString(buffer);
                    stringBuffer = stringBuffer.Replace("\0", "");
                    index = stringBuffer.IndexOf(END, StringComparison.CurrentCulture);
                    if ( index == -1 && ( stringBuffer.Length >= maxLength ||
                                          stringBuffer.Length == maxLength -1 && stringBuffer[stringBuffer.Length - 1] != '\r')
                        )
                    {
                        AbortConnection(SERVER_SYNTAX_ERROR);
                        throw new NonValidMessegeException("SYNTAX ERROR");
                    }
                }
                string ret = stringBuffer.Substring(0, index);

                Console.WriteLine(ret);
                stringBuffer = stringBuffer.Substring(index + 2);
                if (ret.Equals(CLIENT_RECHARGING,StringComparison.Ordinal) && !IsRecharging)
                {
                    IsRecharging = true;
                    string temp = GetMessege(12);
                    if (temp != CLIENT_FULL_POWER)
                    {
                        AbortConnection(SERVER_LOGIC_ERROR);
                        throw new NonValidMessegeException(SERVER_LOGIC_ERROR);
                    }
                    IsRecharging = false;
                    return GetMessege(maxLength);
                }

                if (ret.Length > maxLength - 2)
                {
                    AbortConnection(SERVER_SYNTAX_ERROR);
                    throw new NonValidMessegeException("SYNTAX ERROR");
                }
                return ret;
            }
        }

        public void AbortConnection(string messege)
        {
            Send(messege);
            handler.Close();
        }

        public void Send(string messege)
        {

            handler.Send(Encoding.ASCII.GetBytes(messege));
        }
    }
}