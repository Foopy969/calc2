using System;

namespace calc
{
    public struct Angle
    {
        static double toRadian = Math.PI / 180;

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
            double aPitchRadian = a.Pitch * toRadian;
            double bPitchRadian = b.Pitch * toRadian;

            double aYawRadian = a.Yaw * toRadian;
            double bYawRadian = b.Yaw * toRadian;

            double pitchCos = Math.Cos(aPitchRadian) - Math.Cos(bPitchRadian);
            double pitchSin = Math.Sin(aPitchRadian) - Math.Sin(bPitchRadian);

            double yawCos = Math.Cos(aYawRadian) - Math.Cos(bYawRadian);
            double yawSin = Math.Sin(aYawRadian) - Math.Sin(bYawRadian);

            return Math.Sqrt(Math.Abs(pitchCos * pitchCos + pitchSin * pitchSin) + Math.Abs(yawCos * yawCos + yawSin * yawSin));
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
