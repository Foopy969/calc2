using GameOverlay.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static calc.Potatoe;

namespace calc
{
    public class User
    {
        public int Team { get; }
        public IntPtr Address { get; set; }
        public Vector3 ViewPosition { get; set; }
        public Angle AimPunchAngle { get; set; }
        public int ShotsFired { get; set; }

        public User(IntPtr address)
        {
            Address = address;
            Team = Memory.Read<int>(Address + Offsets.netvars.m_iTeamNum);
        }

        public void Update()
        {
            ViewPosition = Memory.Read<Vector3>(Address + Offsets.netvars.m_vecOrigin) + Memory.Read<Vector3>(Address + Offsets.netvars.m_vecViewOffset);

            AimPunchAngle = Memory.Read<Angle>(Address + Offsets.netvars.m_aimPunchAngle);
            ShotsFired = Memory.Read<int>(Address + Offsets.netvars.m_iShotsFired);
        }
    }
}
