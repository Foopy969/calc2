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
        public Angle m_aimPunchAngle { get; set; }
        public int m_iShotsFired { get; set; }

        public User(IntPtr address)
        {
            Address = address;
            Team = Memory.Read<int>(Address + Offsets.netvars.m_iTeamNum);
        }

        public void Update()
        {
            ViewPosition = Memory.Read<Vector3>(Address + Offsets.netvars.m_vecOrigin) + Memory.Read<Vector3>(Address + Offsets.netvars.m_vecViewOffset);

            m_aimPunchAngle = Memory.Read<Angle>(Address + Offsets.netvars.m_aimPunchAngle);
            m_iShotsFired = Memory.Read<int>(Address + Offsets.netvars.m_iShotsFired);
        }
    }
}
