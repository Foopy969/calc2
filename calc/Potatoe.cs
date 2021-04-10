using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace calc
{
    public class Potatoe
    {
        public static Hazedump Offsets;
        private IntPtr client;
        private IntPtr engine;

        private IntPtr dwClientState;
        public matrix4x4_t dwViewMatrix;

        public User player;
        public Player[]players;

        private Angle viewAngles;
        private Angle anchor;

        public Potatoe()
        {
            if (!File.Exists(@"csgo.json"))
                throw new Exception("Missing offset file", new Exception("Get it from where foopy told u or press the update button"));

            Offsets = JsonConvert.DeserializeObject<Hazedump>(File.ReadAllText(@"csgo.json"));

            if (!Memory.Attatch("csgo"))
                throw new Exception("csgo not found", new Exception("Could not attach to csgo.exe."));

            client = Memory.GetModuleAddress("client.dll");
            engine = Memory.GetModuleAddress("engine.dll");

            if (client == engine)
                throw new Exception("Bad address error", new Exception("Could not hook to client.dll and engine.dll."));

            dwClientState = Memory.Read<IntPtr>(engine + Offsets.signatures.dwClientState);

            if (!ResetPlayers())
                throw new Exception("An error has occurred", new Exception("Could not reset players"));
        }

        public bool ResetPlayers()
        {
            try
            {
                player = GetPlayer();
                players = GetPlayers().ToArray();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public User GetPlayer()
        {
            return new User(Memory.Read<IntPtr>(client + Offsets.signatures.dwLocalPlayer));
        }

        public IEnumerable<Player> GetPlayers()
        {
            for (int i = 0; i < 64; i++)
            {
                IntPtr temp = Memory.Read<IntPtr>(client + Offsets.signatures.dwEntityList + i * 0x10);
                if (temp != IntPtr.Zero && temp != player.Address)
                {
                    yield return new Player(temp);
                }
            }
        }

        public void Get()
        {
            player.Update();

            //find a way to put this in the overlay thread
            foreach (var player in players)
            {
                player.Update();
            }

            dwViewMatrix = Memory.Read<matrix4x4_t>(client + Offsets.signatures.dwViewMatrix);
            viewAngles = Memory.Read<Angle>(dwClientState + Offsets.signatures.dwClientState_ViewAngles);
        }

        public void Set()
        {
            viewAngles.Normalize();
            if (!float.IsNaN(viewAngles.Pitch) && !float.IsNaN(viewAngles.Yaw))
            {
                Memory.Write(dwClientState + Offsets.signatures.dwClientState_ViewAngles, viewAngles);
            }
        }

        public bool AimBot()
        {
            var enemies = players.Where(x => x.Team != player.Team && x.IsAlive());
            if (enemies.Any())
            {
                foreach (var enemy in enemies)
                {
                    enemy.HeadPosition = enemy.GetBonePos(8);
                    enemy.Distance = enemy.CalDistance(player.ViewPosition);
                    enemy.Aim = enemy.GetPitchYaw(player.ViewPosition);
                }

                enemies = enemies.OrderBy(x => Angle.Difference(viewAngles, x.Aim));

                try
                {
                    if (Angle.Difference(viewAngles, enemies.First().Aim) < 0.2)
                    {
                        viewAngles = enemies.First().Aim;
                        return true;
                    }
                }
                catch
                {
                    //ignored
                }
            }
            return false;
        }

        public bool AntiRecoil(bool aimed)
        {
            if (player.m_iShotsFired > 1)
            {
                if (aimed)
                    viewAngles -= player.m_aimPunchAngle * 2;
                else
                    viewAngles += anchor - player.m_aimPunchAngle * 2;
                anchor = player.m_aimPunchAngle * 2;
                return true;
            }
            else
            {
                anchor.Zero();
                return false;
            }
        }
    }
}
