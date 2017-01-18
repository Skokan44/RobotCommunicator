using System;

namespace RobotComunicator
{
    class Location
    {
        protected bool Equals(Location other)
        {
            return X == other.X && Y == other.Y && Direction == other.Direction;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Location) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X;
                hashCode = (hashCode*397) ^ Y;
                hashCode = (hashCode*397) ^ (int) Direction;
                return hashCode;
            }
        }

        public static readonly Location Default = new Location();
        public int X, Y;
        public WorldDirection Direction;

        public Location()
        {
            X = Y = Int32.MaxValue;
            Direction = WorldDirection.None;
        }

        public Location(string toBeParsedString)
        {
            ParseString(toBeParsedString);
            Direction = WorldDirection.None;
        }

        public void ParseString(string toBeParsedString)
        {
            string[] str = toBeParsedString.Split(' ');
            if (str[0] != "OK" || str.Length >3) throw new Exception();
            X = Int32.Parse(str[1]);
            Y = Int32.Parse(str[2]);
        }

        public Location(Location location)
        {
            this.X = location.X;
            Y = location.Y;
            Direction = location.Direction;
        }


        public static bool operator ==(Location x, Location y)
        {
            return x.X == y.X && x.Y == y.Y;
        }

        public static bool operator !=(Location loc, Location other)
        {
            return !(loc == other);
        }

        public void SetFacing(Location original)
        {
            if (original.X == X)
            {
                Direction = original.Y > Y ? WorldDirection.South : WorldDirection.North;
            }
            else
            {
                Direction = original.X > X? WorldDirection.West : WorldDirection.East;
            }
        }
    }
}