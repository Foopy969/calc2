using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace calc
{
    public class Potatoe
    {
        public static Hazedump Offsets;
        private IntPtr client;
        private IntPtr engine;

        private IntPtr ClientState;
        private Angle viewAngles;
        private Angle anchor;

        public matrix4x4_t ViewMatrix;
        public User player;
        public Player[] players;


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

            ClientState = Memory.Read<IntPtr>(engine + Offsets.signatures.dwClientState);

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

            foreach (var player in players)
            {
                player.Update();
            }

            ViewMatrix = Memory.Read<matrix4x4_t>(client + Offsets.signatures.dwViewMatrix);
            viewAngles = Memory.Read<Angle>(ClientState + Offsets.signatures.dwClientState_ViewAngles);
        }

        public void Set()
        {
            viewAngles.Normalize();
            if (!float.IsNaN(viewAngles.Pitch) && !float.IsNaN(viewAngles.Yaw))
            {
                Memory.Write(ClientState + Offsets.signatures.dwClientState_ViewAngles, viewAngles);
            }
        }

        public bool AimBot()
        {
            var enemies = players.Where(x => x.Team != player.Team && x.IsAlive());
            if (enemies.Any())
            {
                try
                {
                    viewAngles = enemies.OrderBy(x => x.GetRank(player.ViewPosition, viewAngles.ToVector3())).Last().GetAim(player.ViewPosition);
                    return true;
                }
                catch { }
            }
            return false;
        }

        public bool AntiRecoil(bool aimed)
        {
            if (player.ShotsFired > 1)
            {
                if (aimed)
                    viewAngles -= player.AimPunchAngle * 2;
                else
                    viewAngles += anchor - player.AimPunchAngle * 2;
                anchor = player.AimPunchAngle * 2;
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
