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
        const double RAD2DEGREE = 180 / Math.PI;
        const int MAXSTUDIOBONES = 128;
        const int BONE_USED_BY_HITBOX = 0x100;

        IntPtr address;
        IntPtr boneMatrix;

        public (int, int)[] SkeletonIdx;
        public Vector3[] SkeletonPos;
        public float Rank;

        public int Team { get; set; }

        public Player(IntPtr _address)
        {
            address = _address;
            Team = Memory.Read<int>(address + Offsets.netvars.m_iTeamNum);

            boneMatrix = Memory.Read<IntPtr>(address + Offsets.netvars.m_dwBoneMatrix);
            SkeletonIdx = GetSkeleton().ToArray();
            SkeletonPos = new Vector3[MAXSTUDIOBONES];
        }

        public void Update()
        {
            for (int i = 0; i < MAXSTUDIOBONES; i++)
            {
                SkeletonPos[i] = GetBonePos(i);
            }
        }

        public bool IsAlive()
        {
            return Memory.Read<int>(address + Offsets.netvars.m_iHealth) > 0 && !Memory.Read<bool>(address + Offsets.signatures.m_bDormant);
        }

        public IEnumerable<(int, int)> GetSkeleton()
        {
            IntPtr addressStudioHdr = Memory.Read<IntPtr>(Memory.Read<IntPtr>(address + Offsets.signatures.m_pStudioHdr));
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
            matrix3x4_t bones = Memory.Read<matrix3x4_t>(boneMatrix + 0x30 * i);
            return new Vector3(bones.m[3], bones.m[7], bones.m[11]);
        }

        public float GetRank(Vector3 position, Vector3 vector)
        {
            Rank = Vector3.Dot(vector, Vector3.Normalize(SkeletonPos[8] - position));
            return Rank;
        }

        public Angle GetAim(Vector3 target)
        {
            Vector3 delta = SkeletonPos[8] - target;
            return new Angle(-Math.Asin(delta.Z / delta.Length()) * RAD2DEGREE, Math.Atan2(delta.Y, delta.X) * RAD2DEGREE);
        }
    }
}
