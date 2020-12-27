namespace calc
{
    public struct Angle
    {
        public float Pitch { get; set; }
        public float Yaw { get; set; }

        public Angle(float _Pitch, float _Yaw)
        {
            Pitch = _Pitch;
            Yaw = _Yaw;
        }

        public Angle(double _Pitch, double _Yaw)
        {
            Pitch = (float)_Pitch;
            Yaw = (float)_Yaw;
        }

        public void Normalize()
        {
            if (Yaw < -180)
            {
                Yaw += 360;
            }
            else if (Yaw > 180)
            {
                Yaw -= 360;
            }

            if (Pitch > 89)
            {
                Pitch = 89;
            }
            else if (Pitch < -89)
            {
                Pitch = -89;
            }
        }

        public void Zero()
        {
            Yaw = 0;
            Pitch = 0;
        }

        public static Angle operator +(Angle a, Angle b)
            => new Angle(a.Pitch + b.Pitch, a.Yaw + b.Yaw);

        public static Angle operator -(Angle a, Angle b)
            => new Angle(a.Pitch - b.Pitch, a.Yaw - b.Yaw);

        public static Angle operator *(Angle a, float b)
            => new Angle(a.Pitch * b, a.Yaw * b);
    }
}
