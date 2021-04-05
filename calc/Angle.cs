using System;

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

        public void Absolute()
        {
            if (Yaw < 0)
            {
                Yaw = -Yaw;
            }
            if (Pitch < 0)
            {
                Pitch = -Pitch;
            }
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

        public static double Difference(Angle a, Angle b)
        {
            double pitchCos = Math.Cos(a.Pitch * Math.PI / 180) - Math.Cos(b.Pitch * Math.PI / 180);
            double pitchSin = Math.Sin(a.Pitch * Math.PI / 180) - Math.Sin(b.Pitch * Math.PI / 180);
            double pitchDiff = Math.Sqrt(pitchCos * pitchCos + pitchSin * pitchSin);

            double yawCos = Math.Cos(a.Yaw * Math.PI / 180) - Math.Cos(b.Yaw * Math.PI / 180);
            double yawSin = Math.Sin(a.Yaw * Math.PI / 180) - Math.Sin(b.Yaw * Math.PI / 180);
            double yawDiff = Math.Sqrt(yawCos * yawCos + yawSin * yawSin);

            return Math.Sqrt(pitchDiff * pitchDiff + yawDiff * yawDiff);
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
