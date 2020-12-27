using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using static calc.Potatoe;

namespace calc
{
    public class Player
    {
        private const int MAXSTUDIOBONES = 128;
        private const int BONE_USED_BY_HITBOX = 0x100;

        public int Team { get; set; }
        public IntPtr Address { get; set; }
        public IntPtr BoneMatrix { get; set; }
        public Vector3 HeadPosition { get; set; }
        public Angle Aim { get; set; }
        public double Distance { get; set; }
        public (int, int)[] Skeleton;
        public (Vector3, Vector3)[] SkeletonPos;

        public Player(IntPtr address)
        {
            Address = address;
            Team = Memory.Read<int>(Address + Offsets.netvars.m_iTeamNum);

            BoneMatrix = Memory.Read<IntPtr>(Address + Offsets.netvars.m_dwBoneMatrix);
            Skeleton = GetSkeleton().ToArray();
            SkeletonPos = new (Vector3, Vector3)[Skeleton.Length];
        }

        public void Update()
        {
            for (int i = 0; i < Skeleton.Length; i++)
            {
                SkeletonPos[i] = (GetBonePos(Skeleton[i].Item1), GetBonePos(Skeleton[i].Item2));
            }
        }

        public bool IsAlive()
        {
            return Memory.Read<int>(Address + Offsets.netvars.m_iHealth) > 0 && !Memory.Read<bool>(Address + Offsets.signatures.m_bDormant);
        }

        public IEnumerable<(int, int)> GetSkeleton()
        {
            IntPtr addressStudioHdr = Memory.Read<IntPtr>(Memory.Read<IntPtr>(Address + Offsets.signatures.m_pStudioHdr));
            studiohdr_t studioHdr = Memory.Read<studiohdr_t>(addressStudioHdr);

            for (int i = 0; i < studioHdr.numbones; i++)
            {
                mstudiobone_t bone = Memory.Read<mstudiobone_t>(addressStudioHdr + studioHdr.boneindex + i * Marshal.SizeOf<mstudiobone_t>());

                if ((bone.flags & BONE_USED_BY_HITBOX) == BONE_USED_BY_HITBOX)
                {
                    if (bone.parent >= 0 && bone.parent < studioHdr.numbones)
                    {
                        yield return (i, bone.parent);
                    }
                }
            }
        }

        public unsafe Vector3 GetBonePos(int i)
        {
            matrix3x4_t bones = Memory.Read<matrix3x4_t>(BoneMatrix + 0x30 * i);
            return new Vector3(bones.m[3], bones.m[7], bones.m[11]);
        }

        public double CalDistance(Vector3 Target)
        {
            Vector3 Delta = HeadPosition - Target;
            return Math.Sqrt(Delta.X * Delta.X + Delta.Y * Delta.Y + Delta.Z * Delta.Z);
        }

        public Angle GetPitchYaw(Vector3 Target)
        {
            Vector3 Delta = HeadPosition - Target;
            return new Angle(-Math.Asin(Delta.Z / Distance) * (180 / Math.PI), Math.Atan2(Delta.Y, Delta.X) * (180 / Math.PI));
        }

        public bool GetSpotted(int id)
        {
            return 0 < (Memory.Read<int>(Address + Offsets.netvars.m_bSpottedByMask) & (1 << id));
        }
    }
}
