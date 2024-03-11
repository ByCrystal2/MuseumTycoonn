using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Collections;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using UnityEngine.Profiling;
using DigitalOpus.MB.Core;

namespace DigitalOpus.MB.Core
{
    public partial class MB3_MeshCombinerSingle : MB3_MeshCombiner
    {
#if UNITY_2020_2_OR_NEWER
        /// <summary>
        /// These "SIZER"  structs are designed to be a certain size in memory
        /// We can use them to allocate blocks of memory the right size to hold
        /// A certain number of bytes.
        /// 
        /// We will use these structs instead of structs like:
        /// 
        /// public struct PositionAndNormal
        /// {
        ///     public Vector3 position;
        ///     public Vector3 normal;
        /// }
        /// 
        /// public struct PositionAndColor
        /// {
        ///     public Vector3 position;
        ///     public Vector3 color;
        /// }
        /// 
        /// public unsafe struct SIZER_20
        /// {
        ///     public fixed byte data[20];
        /// }
        /// 
        /// These structs have a size of 20 bytes which is the same as the size of SIZER_20
        /// We can use SIZER_20 in place of a huge number of channel-permutation-structs.
        /// 
        /// </summary>
        public struct MB_MeshCombinerSingle_MeshNativeArrayHelper
        {
            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_4 { public fixed byte data[4]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_8 { public fixed byte data[8]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_12 { public fixed byte data[12]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_16 { public fixed byte data[16]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_20 { public fixed byte data[20]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_24 { public fixed byte data[24]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_28 { public fixed byte data[28]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_32 { public fixed byte data[32]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_36 { public fixed byte data[36]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_40 { public fixed byte data[40]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_44 { public fixed byte data[44]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_48 { public fixed byte data[48]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_52 { public fixed byte data[52]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_56 { public fixed byte data[56]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_60 { public fixed byte data[60]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_64 { public fixed byte data[64]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_68 { public fixed byte data[68]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_72 { public fixed byte data[72]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_76 { public fixed byte data[72]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_80 { public fixed byte data[80]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_84 { public fixed byte data[84]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_88 { public fixed byte data[88]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_92 { public fixed byte data[92]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_96 { public fixed byte data[96]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_100 { public fixed byte data[100]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_104 { public fixed byte data[104]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_108 { public fixed byte data[108]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_112 { public fixed byte data[112]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_116 { public fixed byte data[116]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_120 { public fixed byte data[120]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_124 { public fixed byte data[124]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_128 { public fixed byte data[128]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_132 { public fixed byte data[132]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_136 { public fixed byte data[136]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_140 { public fixed byte data[140]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_144 { public fixed byte data[144]; }
            
            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_148 { public fixed byte data[148]; }

            [StructLayout(LayoutKind.Sequential)]
            public unsafe struct SIZER_152 { public fixed byte data[152]; }

            /// <summary>
            /// This array can look up a type that is a certain number of bytes in size.
            /// For example   Type t = _TypeForStride[12]      // t will be 12 bytes in size.
            /// We only need types for multiples of 4 since all channels are built on floats
            /// and a float has four bytes.
            /// </summary>
            static Type[] _TypeForStride = new Type[]
            {
            null,             null,             null,             null,             // 0,1,2,3
            typeof(SIZER_4),  null,             null,             null,             // 4,5,6,7
            typeof(SIZER_8),  null,             null,             null,             // 8,9,10,11
            typeof(SIZER_12), null,             null,             null,
            typeof(SIZER_16), null,             null,             null,

            typeof(SIZER_20), null,             null,             null,
            typeof(SIZER_24), null,             null,             null,
            typeof(SIZER_28), null,             null,             null,
            typeof(SIZER_32), null,             null,             null,
            typeof(SIZER_36), null,             null,             null,

            typeof(SIZER_40), null,             null,             null,
            typeof(SIZER_44), null,             null,             null,
            typeof(SIZER_48), null,             null,             null,
            typeof(SIZER_52), null,             null,             null,
            typeof(SIZER_56), null,             null,             null,

            typeof(SIZER_60), null,             null,             null,
            typeof(SIZER_64), null,             null,             null,
            typeof(SIZER_68), null,             null,             null,
            typeof(SIZER_72), null,             null,             null,
            typeof(SIZER_76), null,             null,             null,

            typeof(SIZER_80), null,             null,             null,
            typeof(SIZER_84), null,             null,             null,
            typeof(SIZER_88), null,             null,             null,
            typeof(SIZER_92), null,             null,             null,
            typeof(SIZER_96), null,             null,             null,

            typeof(SIZER_100), null,             null,             null,
            typeof(SIZER_104), null,             null,             null,
            typeof(SIZER_108), null,             null,             null,
            typeof(SIZER_112), null,             null,             null,
            typeof(SIZER_116), null,             null,             null,

            typeof(SIZER_120), null,             null,             null,
            typeof(SIZER_124), null,             null,             null,
            typeof(SIZER_128), null,             null,             null,
            typeof(SIZER_132), null,             null,             null,
            typeof(SIZER_136), null,             null,             null,

            typeof(SIZER_140), null,             null,             null,
            typeof(SIZER_144), null,             null,             null,
            typeof(SIZER_148), null,             null,             null,
            typeof(SIZER_152)

            };

            [UnityEngine.Scripting.Preserve]
            public void _ENSURE_IL2CPP_CREATES_NECESSARY_CODE(ref Mesh.MeshData m)
            {
                Debug.LogError("This should never be called directly. It is only here " +
                    "to ensure these methodes are" +
                    " generated by the il2cpp compiler and not stripped so that they " +
                    "can be found by reflection.");
                {
                    var na = m.GetVertexData<SIZER_4>(0);
                    var slicer = new NativeSlice<SIZER_4>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_8>(0);
                    var slicer = new NativeSlice<SIZER_8>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_12>(0);
                    var slicer = new NativeSlice<SIZER_12>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_16>(0);
                    var slicer = new NativeSlice<SIZER_16>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_20>(0);
                    var slicer = new NativeSlice<SIZER_20>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_24>(0);
                    var slicer = new NativeSlice<SIZER_24>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_28>(0);
                    var slicer = new NativeSlice<SIZER_28>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_32>(0);
                    var slicer = new NativeSlice<SIZER_32>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_36>(0);
                    var slicer = new NativeSlice<SIZER_36>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_40>(0);
                    var slicer = new NativeSlice<SIZER_40>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_44>(0);
                    var slicer = new NativeSlice<SIZER_44>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_48>(0);
                    var slicer = new NativeSlice<SIZER_48>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_52>(0);
                    var slicer = new NativeSlice<SIZER_52>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_56>(0);
                    var slicer = new NativeSlice<SIZER_56>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                // =======================================

                {
                    var na = m.GetVertexData<SIZER_60>(0);
                    var slicer = new NativeSlice<SIZER_60>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_64>(0);
                    var slicer = new NativeSlice<SIZER_64>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_68>(0);
                    var slicer = new NativeSlice<SIZER_68>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_72>(0);
                    var slicer = new NativeSlice<SIZER_72>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_76>(0);
                    var slicer = new NativeSlice<SIZER_76>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                // =======================================
                {
                    var na = m.GetVertexData<SIZER_80>(0);
                    var slicer = new NativeSlice<SIZER_80>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_84>(0);
                    var slicer = new NativeSlice<SIZER_84>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_88>(0);
                    var slicer = new NativeSlice<SIZER_88>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_92>(0);
                    var slicer = new NativeSlice<SIZER_92>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_96>(0);
                    var slicer = new NativeSlice<SIZER_96>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                // =======================================

                {
                    var na = m.GetVertexData<SIZER_100>(0);
                    var slicer = new NativeSlice<SIZER_100>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_104>(0);
                    var slicer = new NativeSlice<SIZER_104>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_108>(0);
                    var slicer = new NativeSlice<SIZER_108>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_112>(0);
                    var slicer = new NativeSlice<SIZER_112>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_116>(0);
                    var slicer = new NativeSlice<SIZER_116>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                // =======================================

                {
                    var na = m.GetVertexData<SIZER_120>(0);
                    var slicer = new NativeSlice<SIZER_120>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_124>(0);
                    var slicer = new NativeSlice<SIZER_124>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_128>(0);
                    var slicer = new NativeSlice<SIZER_128>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_132>(0);
                    var slicer = new NativeSlice<SIZER_132>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_136>(0);
                    var slicer = new NativeSlice<SIZER_136>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_140>(0);
                    var slicer = new NativeSlice<SIZER_140>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_144>(0);
                    var slicer = new NativeSlice<SIZER_144>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_148>(0);
                    var slicer = new NativeSlice<SIZER_148>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                {
                    var na = m.GetVertexData<SIZER_152>(0);
                    var slicer = new NativeSlice<SIZER_152>(na);
                    slicer.SliceWithStride<Vector2>(0);
                    slicer.SliceWithStride<Vector3>(0);
                    slicer.SliceWithStride<Vector4>(0);
                    slicer.SliceWithStride<Color32>(0); // Not sure about the right type for this
                }

                // =======================================
            }

            public Mesh.MeshDataArray dataArray;
            public Mesh.MeshData data;
            public int vertexCount;

            public static int CalcStride(MB_MeshVertexChannelFlags channels, int uvChannelWithExtraParameter,
                out int strideVertexBuffer, out int strideUVbuffer)
            {
                strideVertexBuffer = 0;
                strideUVbuffer = 0;
                
                {
                    if ((channels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex)
                    {
                        strideVertexBuffer += sizeof(float) * 3;
                    }
                    else
                    {
                        Debug.Assert(false, "Verticies must always be included.");
                    }

                    if ((channels & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal)
                    {
                        strideVertexBuffer += sizeof(float) * 3;
                    }

                    if ((channels & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent)
                    {
                        strideVertexBuffer += sizeof(float) * 4;
                    }

                    if ((channels & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors)
                    {
                        strideUVbuffer += sizeof(float) * 4;
                    }

                    if ((channels & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0)
                    {
                        strideUVbuffer += sizeof(float) * 2;
                    }

                    if ((channels & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2)
                    {
                        strideUVbuffer += sizeof(float) * 2;
                    }

                    if ((channels & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3)
                    {
                        strideUVbuffer += sizeof(float) * 2;
                    }

                    if ((channels & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4)
                    {
                        strideUVbuffer += sizeof(float) * 2;
                    }

                    if ((channels & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5)
                    {
                        strideUVbuffer += sizeof(float) * 2;
                    }

                    if ((channels & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6)
                    {
                        strideUVbuffer += sizeof(float) * 2;
                    }

                    if ((channels & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7)
                    {
                        strideUVbuffer += sizeof(float) * 2;
                    }

                    if ((channels & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8)
                    {
                        strideUVbuffer += sizeof(float) * 2;
                    }

                    if ((channels & MB_MeshVertexChannelFlags.blendWeight) == MB_MeshVertexChannelFlags.blendWeight)
                    {
                        Debug.Assert((channels & MB_MeshVertexChannelFlags.blendIndices) == MB_MeshVertexChannelFlags.blendIndices);
                        //strideSkinBuffer += sizeof(ushort) * 8; // for blendWeights and blendIndicies
                    }

                    if (uvChannelWithExtraParameter >= 0)
                    {
                        strideUVbuffer += sizeof(float);
                    }
                }
                
                int stride = strideVertexBuffer + strideUVbuffer /*+ strideSkinBuffer */;
                return stride;
            }

            public static void Init(MB_MeshVertexChannelFlags channels,
                                    VertexAttributeDescriptor[] vertexAttributes,
                                    ref MB3_MeshCombinerSingle.VertexAndTriangleProcessorNativeArray nativeSlices,
                                    int vertexCount, int[] submeshCount, int uvChannelWithExtraParameter)
            {
                Profiler.BeginSample("MeshDataAPI_Allocate");
                // This should happen once in AddDeleteGameObjects or Update

                int strideVertexBuffer, strideUVbuffer;
                CalcStride(channels, uvChannelWithExtraParameter, out strideVertexBuffer, out strideUVbuffer);

                int numBuffers = 0;
                int vertexStream = 0;
                int uvStream = 1;
                int skinStream = 2;
                {
                    // number the streams.
                    int nextStreamNum = 0;
                    vertexStream = nextStreamNum;
                    numBuffers++;
                    nextStreamNum++;
                    if (strideUVbuffer > 0)
                    {
                        uvStream = nextStreamNum;
                        nextStreamNum++;
                        numBuffers++;
                    }
                    /*
                    if (strideSkinBuffer > 0)
                    {
                        skinStream = nextStreamNum;
                        nextStreamNum++;
                        numBuffers++;
                    }
                    */
                }

                // Order is important. Must be:
                // position, normal, tangent, color, texcoord0 .... texcord7, blendweight, blendindicies
                int vertIdx = 0;
                {
                    {
                        
                        if ((channels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex)
                        {
                            vertexAttributes[vertIdx] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, vertexStream);
                            vertIdx++;
                        }
                        else
                        {
                            Debug.Assert(false, "Verticies must always be included.");
                        }

                        if ((channels & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal)
                        {
                            vertexAttributes[vertIdx] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, vertexStream);
                            vertIdx++;
                        }

                        if ((channels & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent)
                        {
                            vertexAttributes[vertIdx] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, vertexStream);
                            vertIdx++;
                        }
                    }

                    {
                        if ((channels & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors)
                        {
                            vertexAttributes[vertIdx] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4, uvStream);
                            vertIdx++;
                        }

                        if ((channels & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0)
                        {
                            int size = uvChannelWithExtraParameter == 0 ? 3 : 2;
                            vertexAttributes[vertIdx] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, size, uvStream);
                            vertIdx++;
                        }

                        if ((channels & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2)
                        {
                            int size = uvChannelWithExtraParameter == 1 ? 3 : 2;
                            vertexAttributes[vertIdx] = new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, size, uvStream);
                            vertIdx++;
                        }

                        if ((channels & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3)
                        {
                            int size = uvChannelWithExtraParameter == 2 ? 3 : 2;
                            vertexAttributes[vertIdx] = new VertexAttributeDescriptor(VertexAttribute.TexCoord2, VertexAttributeFormat.Float32, size, uvStream);
                            vertIdx++;
                        }

                        if ((channels & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4)
                        {
                            int size = uvChannelWithExtraParameter == 3 ? 3 : 2;
                            vertexAttributes[vertIdx] = new VertexAttributeDescriptor(VertexAttribute.TexCoord3, VertexAttributeFormat.Float32, size, uvStream);
                            vertIdx++;
                        }

                        if ((channels & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5)
                        {
                            int size = uvChannelWithExtraParameter == 4 ? 3 : 2;
                            vertexAttributes[vertIdx] = new VertexAttributeDescriptor(VertexAttribute.TexCoord4, VertexAttributeFormat.Float32, size, uvStream);
                            vertIdx++;
                        }

                        if ((channels & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6)
                        {
                            int size = uvChannelWithExtraParameter == 5 ? 3 : 2;
                            vertexAttributes[vertIdx] = new VertexAttributeDescriptor(VertexAttribute.TexCoord5, VertexAttributeFormat.Float32, size, uvStream);
                            vertIdx++;
                        }

                        if ((channels & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7)
                        {
                            int size = uvChannelWithExtraParameter == 6 ? 3 : 2;
                            vertexAttributes[vertIdx] = new VertexAttributeDescriptor(VertexAttribute.TexCoord6, VertexAttributeFormat.Float32, size, uvStream);
                            vertIdx++;
                        }

                        if ((channels & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8)
                        {
                            int size = uvChannelWithExtraParameter == 7 ? 3 : 2;
                            vertexAttributes[vertIdx] = new VertexAttributeDescriptor(VertexAttribute.TexCoord7, VertexAttributeFormat.Float32, size, uvStream);
                            vertIdx++;
                        }
                    }
                    
                    
                    {

                        if ((channels & MB_MeshVertexChannelFlags.blendWeight) == MB_MeshVertexChannelFlags.blendWeight)
                        {
                            Debug.Assert((channels & MB_MeshVertexChannelFlags.blendIndices) == MB_MeshVertexChannelFlags.blendIndices);
                            vertexAttributes[vertIdx] = new VertexAttributeDescriptor(VertexAttribute.BlendWeight, VertexAttributeFormat.UNorm16, 4, skinStream);
                            vertIdx++;

                            vertexAttributes[vertIdx] = new VertexAttributeDescriptor(VertexAttribute.BlendIndices, VertexAttributeFormat.UInt16, 4, skinStream);
                            vertIdx++;
                        }
                    }
                    
                }

                AllocateWriteableMeshData(ref nativeSlices, vertexAttributes, vertexCount, numBuffers);
                SetupNativeSlices(ref nativeSlices, strideVertexBuffer, strideUVbuffer, uvChannelWithExtraParameter);
                nativeSlices.triangleBuffer = nativeSlices.data.GetIndexData<ushort>();
                Profiler.EndSample();
            }

            public static void AllocateWriteableMeshData(ref MB3_MeshCombinerSingle.VertexAndTriangleProcessorNativeArray nativeSlices,
                VertexAttributeDescriptor[] channels,
                int vertexCount, int numBuffers)
            {
                nativeSlices.dataArray = Mesh.AllocateWritableMeshData(1);
                nativeSlices.dataArrayAllocated = true;
                nativeSlices.data = nativeSlices.dataArray[0];

                if (nativeSlices.LOG_LEVEL >= MB2_LogLevel.debug)
                {
                    String s;
                    s = "Allocating VertexChannels for combined mesh: ";
                    for (int i = 0; i < channels.Length; i++)
                    {
                        s += "\n   " + channels[i];
                    }
                    Debug.Log(s);
                }

                nativeSlices.data.SetVertexBufferParams(vertexCount, channels);
            
                //Debug.Assert(numBuffers == nativeSlices.data.vertexBufferCount, "numBuffers not same: " + numBuffers + "  " + nativeSlices.data.vertexBufferCount);

                // Don't do the triangles here. We do it in apply because some submeshes might have zero length tris.

                //nativeSlices.data.subMeshCount = submeshSizes.Length;

                /*
                for (int i = 0; i < submeshSizes.Length; i++)
                {
                    IndexFormat format = IndexFormat.UInt16;
                    if (submeshSizes[i] >= ushort.MaxValue)
                    {
                        format = IndexFormat.UInt32;
                    }

                    Debug.Assert(i == 0, "Needs fixed if not first submesh.");
                    SubMeshDescriptor smd = new SubMeshDescriptor(0, (int)submeshSizes[i]);
                    nativeSlices.data.SetSubMesh(i, smd, MeshUpdateFlags.Default);
                    //nativeSlices.data.SetIndexBufferParams((int)submeshSizes[i], format);
                }
                */
            }


            /// <summary>
            /// This method uses Reflection so it is confusing to see what is going on. It is doing this:
            /// 
            ///     struct VertsNormsTansUVs
            ///     {
            ///        public Vector3 position;
            ///        ....
            ///     }
            ///     
            ///         Type SizeStruct60 = _TypeForStride[vertexDataStride];
            ///         NativeArray<VertsNormsTansUVs> rawMeshData = nativeSlices.data.GetVertexData<VertsNormsTansUVs>();;
            ///         NativeSlice<VertsNormsTansUVs> slicer = new NativeSlice<VertsNormsTansUVs>(rawMeshData);
            ///         NativeSlice<Vector3> verts = slicer.SliceWithStride<Vector3>(offsetVerts);
            ///         NativeSlice<Vector3> norms = slicer.SliceWithStride<Vector3>(offsetNorms);
            ///         NativeSlice<Vector4> tans = slicer.SliceWithStride<Vector4>(offsetTans);
            ///         NativeSlice<Vector2> uv0 = slicer.SliceWithStride<Vector2>(offsetUV0);
            ///         ....
            ///
            ///  The above code is simple and straightforward, but we need a different struct for each possible permutation of channels.
            ///  Instead we could use a struct of the right size and the first few lines would look like this:
            ///
            ///         int vertexDataStride = sizeof(Vector3) + sizeof(Vector3) + sizeof(Vector4) + sizeof(Vector2); // verts, norms, tans, uvs
            ///         Type SizeStruct = _TypeForStride[vertexDataStride];
            ///         //  SizeStruct size in bytes matches VertsNormsTansUVs
            ///         NativeArray<SizeStruct> rawMeshData = nativeSlices.data.GetVertexData<SizeStruct>();;
            ///         NativeSlice<SizeStruct> slicer = new NativeSlice<SizeStruct>(rawMeshData);
            ///         ...
            ///
            /// We need to use Reflection because we don't know the vertexDataStride. vertexDataStride will vary depending
            /// on which channels are checked. The SizeStruct will be different depending on the channels. We need to use
            /// Reflection.
            /// 
            /// </summary>
            public static unsafe void SetupNativeSlices(ref MB3_MeshCombinerSingle.VertexAndTriangleProcessorNativeArray nativeSlices,
                int strideVertexData, int strideUVdata, /*int strideSkinData,*/ int uvChannelWithExtraParameter)
            {
                ref Mesh.MeshData m = ref nativeSlices.data;

                // {
                //     Debug.Log("   SetupNativeSlices: " + tSizerVertex + "  stride:" + strideVertexData + "   blockSize: " + (strideVertexData * m.vertexCount) + "  uvChannelWithExtraParameter:" + uvChannelWithExtraParameter);
                // }

                // Unity likes to use three different buffers:
                //     buffer/stream 0 :  postion, normal, tangent
                //     buffer/stream 1 :  colors and uv channels
                //     buffer/stream 2 :  boneWeights
                nativeSlices.bufferStride_0 = strideVertexData;
                nativeSlices.bufferStride_1 = strideUVdata;
                //nativeSlices.bufferStride_2 = strideSkinData;

                int streamIdx = 0;
                {
                    Type tSizerVertex = nativeSlices.rawSliceSizerType_0 = _TypeForStride[strideVertexData];
                    Debug.Assert(tSizerVertex != null, "Did not have a sizer struct for stride.");

                    System.Reflection.MethodInfo methodGetVertexData = m.GetType().GetMethod("GetVertexData", new Type[] { typeof(int) });
                    System.Reflection.MethodInfo genericGetVertexData = methodGetVertexData.MakeGenericMethod(tSizerVertex);
                    object rawDataArray = genericGetVertexData.Invoke(m, new object[] { streamIdx });

                    {
                        Type nst = typeof(NativeSlice<>);
                        Type genericNst = nst.MakeGenericType(new Type[] { tSizerVertex });
                        nativeSlices.rawSliceVertexStream_0 = Activator.CreateInstance(genericNst, new object[] { rawDataArray });
                    }

                    int vertexCount = (int)nativeSlices.rawSliceVertexStream_0.GetType().GetProperty("Length").GetValue(nativeSlices.rawSliceVertexStream_0, null);
                    nativeSlices.vertexCount = vertexCount;

                    System.Reflection.MethodInfo method = nativeSlices.rawSliceVertexStream_0.GetType().GetMethod("SliceWithStride", new Type[] { typeof(int) });

                    {
                        int offset = 0;
                        if ((nativeSlices.channels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex)
                        {
                            // NativeSlice<Vector3> verts = slicer.SliceWithStride<Vector3>(offsetVerts);
                            System.Reflection.MethodInfo generic = method.MakeGenericMethod(typeof(Vector3));
                            nativeSlices.verticies = (NativeSlice<Vector3>)generic.Invoke(nativeSlices.rawSliceVertexStream_0, new object[] { offset });
                            offset += sizeof(Vector3);
                        }

                        if ((nativeSlices.channels & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal)
                        {
                            // NativeSlice<Vector3> norms = slicer.SliceWithStride<Vector3>(offsetVerts);
                            System.Reflection.MethodInfo generic = method.MakeGenericMethod(typeof(Vector3));
                            nativeSlices.normals = (NativeSlice<Vector3>)generic.Invoke(nativeSlices.rawSliceVertexStream_0, new object[] { offset });
                            offset += sizeof(Vector3);
                        }

                        if ((nativeSlices.channels & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent)
                        {
                            // NativeSlice<Vector4> tangents = slicer.SliceWithStride<Vector4>(offsetVerts);
                            System.Reflection.MethodInfo generic = method.MakeGenericMethod(typeof(Vector4));
                            nativeSlices.tangents = (NativeSlice<Vector4>)generic.Invoke(nativeSlices.rawSliceVertexStream_0, new object[] { offset });
                            offset += sizeof(Vector4);
                        }
                    }
                }

                if (strideUVdata > 0)
                {
                    streamIdx++;
                    Type tSizerUV = nativeSlices.rawSliceSizerType_1 = _TypeForStride[strideUVdata];
                    System.Reflection.MethodInfo methodGetVertexData = m.GetType().GetMethod("GetVertexData", new Type[] { typeof(int) });
                    System.Reflection.MethodInfo genericGetVertexData = methodGetVertexData.MakeGenericMethod(tSizerUV);
                    object rawDataArray = genericGetVertexData.Invoke(m, new object[] { streamIdx });

                    {
                        Type nst = typeof(NativeSlice<>);
                        Type genericNst = nst.MakeGenericType(new Type[] { tSizerUV });
                        nativeSlices.rawSliceVertexStream_1 = Activator.CreateInstance(genericNst, new object[] { rawDataArray });
                    }

                    {
                        System.Reflection.MethodInfo method = nativeSlices.rawSliceVertexStream_1.GetType().GetMethod("SliceWithStride", new Type[] { typeof(int) });
                        
                        int offset = 0;
                        if ((nativeSlices.channels & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors)
                        {
                            System.Reflection.MethodInfo generic = method.MakeGenericMethod(typeof(Color));
                            nativeSlices.colors = (NativeSlice<Color>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                            offset += sizeof(Color);
                        }

                        if ((nativeSlices.channels & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0)
                        {
                            System.Reflection.MethodInfo generic = method.MakeGenericMethod(typeof(Vector2));
                            nativeSlices.uv0s = (NativeSlice<Vector2>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                            if (uvChannelWithExtraParameter == 0)
                            {
                                generic = method.MakeGenericMethod(typeof(Vector3));
                                nativeSlices.uvsWithExtraIndex = (NativeSlice<Vector3>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                                generic = method.MakeGenericMethod(typeof(float));
                                offset += sizeof(Vector2);
                                nativeSlices.uvsSliceIdx = (NativeSlice<float>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                                offset += sizeof(float);
                            }
                            else
                            {
                                offset += sizeof(Vector2);
                            }
                        }

                        if ((nativeSlices.channels & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2)
                        {
                            System.Reflection.MethodInfo generic = method.MakeGenericMethod(typeof(Vector2));
                            nativeSlices.uv2s = (NativeSlice<Vector2>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                            if (uvChannelWithExtraParameter == 1)
                            {
                                generic = method.MakeGenericMethod(typeof(Vector3));
                                nativeSlices.uvsWithExtraIndex = (NativeSlice<Vector3>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                                generic = method.MakeGenericMethod(typeof(float));
                                offset += sizeof(Vector2);
                                nativeSlices.uvsSliceIdx = (NativeSlice<float>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                                offset += sizeof(float);
                            }
                            else
                            {
                                offset += sizeof(Vector2);
                            }
                        }

                        if ((nativeSlices.channels & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3)
                        {
                            System.Reflection.MethodInfo generic = method.MakeGenericMethod(typeof(Vector2));
                            nativeSlices.uv3s = (NativeSlice<Vector2>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                            if (uvChannelWithExtraParameter == 2)
                            {
                                generic = method.MakeGenericMethod(typeof(Vector3));
                                nativeSlices.uvsWithExtraIndex = (NativeSlice<Vector3>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                                generic = method.MakeGenericMethod(typeof(float));
                                offset += sizeof(Vector2);
                                nativeSlices.uvsSliceIdx = (NativeSlice<float>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                                offset += sizeof(float);
                            }
                            else
                            {
                                offset += sizeof(Vector2);
                            }
                        }

                        if ((nativeSlices.channels & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4)
                        {
                            System.Reflection.MethodInfo generic = method.MakeGenericMethod(typeof(Vector2));
                            nativeSlices.uv4s = (NativeSlice<Vector2>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                            if (uvChannelWithExtraParameter == 3)
                            {
                                generic = method.MakeGenericMethod(typeof(Vector3));
                                nativeSlices.uvsWithExtraIndex = (NativeSlice<Vector3>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                                generic = method.MakeGenericMethod(typeof(float));
                                offset += sizeof(Vector2);
                                nativeSlices.uvsSliceIdx = (NativeSlice<float>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                                offset += sizeof(float);
                            }
                            else
                            {
                                offset += sizeof(Vector2);
                            }
                        }

                        if ((nativeSlices.channels & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5)
                        {
                            System.Reflection.MethodInfo generic = method.MakeGenericMethod(typeof(Vector2));
                            nativeSlices.uv5s = (NativeSlice<Vector2>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                            if (uvChannelWithExtraParameter == 4)
                            {
                                generic = method.MakeGenericMethod(typeof(Vector3));
                                nativeSlices.uvsWithExtraIndex = (NativeSlice<Vector3>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                                generic = method.MakeGenericMethod(typeof(float));
                                offset += sizeof(Vector2);
                                nativeSlices.uvsSliceIdx = (NativeSlice<float>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                                offset += sizeof(float);
                            }
                            else
                            {
                                offset += sizeof(Vector2);
                            }
                        }

                        if ((nativeSlices.channels & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6)
                        {
                            System.Reflection.MethodInfo generic = method.MakeGenericMethod(typeof(Vector2));
                            nativeSlices.uv6s = (NativeSlice<Vector2>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                            if (uvChannelWithExtraParameter == 5)
                            {
                                generic = method.MakeGenericMethod(typeof(Vector3));
                                nativeSlices.uvsWithExtraIndex = (NativeSlice<Vector3>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                                generic = method.MakeGenericMethod(typeof(float));
                                offset += sizeof(Vector2);
                                nativeSlices.uvsSliceIdx = (NativeSlice<float>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                                offset += sizeof(float);
                            }
                            else
                            {
                                offset += sizeof(Vector2);
                            }
                        }

                        if ((nativeSlices.channels & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7)
                        {
                            System.Reflection.MethodInfo generic = method.MakeGenericMethod(typeof(Vector2));
                            nativeSlices.uv7s = (NativeSlice<Vector2>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                            if (uvChannelWithExtraParameter == 6)
                            {
                                generic = method.MakeGenericMethod(typeof(Vector3));
                                nativeSlices.uvsWithExtraIndex = (NativeSlice<Vector3>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                                generic = method.MakeGenericMethod(typeof(float));
                                offset += sizeof(Vector2);
                                nativeSlices.uvsSliceIdx = (NativeSlice<float>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                                offset += sizeof(float);
                            }
                            else
                            {
                                offset += sizeof(Vector2);
                            }
                        }

                        if ((nativeSlices.channels & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8)
                        {
                            System.Reflection.MethodInfo generic = method.MakeGenericMethod(typeof(Vector2));
                            nativeSlices.uv8s = (NativeSlice<Vector2>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                            if (uvChannelWithExtraParameter == 7)
                            {
                                generic = method.MakeGenericMethod(typeof(Vector3));
                                nativeSlices.uvsWithExtraIndex = (NativeSlice<Vector3>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                                generic = method.MakeGenericMethod(typeof(float));
                                offset += sizeof(Vector2);
                                nativeSlices.uvsSliceIdx = (NativeSlice<float>)generic.Invoke(nativeSlices.rawSliceVertexStream_1, new object[] { offset });
                                offset += sizeof(float);
                            }
                            else
                            {
                                offset += sizeof(Vector2);
                            }
                        }
                    }
                }

                // We don't do skinned data here because we are using SetBoneWeights and GetBoneWeights API 
                // This API seems to switch the data format of the channels mysteriously. 
                /*
                if (strideSkinData > 0)
                {
                    streamIdx++;
                    Type tSizerSkin = nativeSlices.rawSliceSizerType_2 = _TypeForStride[strideSkinData];
                    System.Reflection.MethodInfo methodGetVertexData = m.GetType().GetMethod("GetVertexData", new Type[] { typeof(int) });
                    System.Reflection.MethodInfo genericGetVertexData = methodGetVertexData.MakeGenericMethod(tSizerSkin);
                    object rawDataArray = genericGetVertexData.Invoke(m, new object[] { streamIdx });

                    {
                        Type nst = typeof(NativeSlice<>);
                        Type genericNst = nst.MakeGenericType(new Type[] { tSizerSkin });
                        nativeSlices.rawSliceVertexStream_2 = Activator.CreateInstance(genericNst, new object[] { rawDataArray });
                    }

                    {
                        int offset = 0;
                        System.Reflection.MethodInfo method = nativeSlices.rawSliceVertexStream_2.GetType().GetMethod("SliceWithStride", new Type[] { typeof(int) });
                        if ((nativeSlices.channels & MB_MeshVertexChannelFlags.blendWeight) == MB_MeshVertexChannelFlags.blendWeight)
                        {
                            Debug.Assert((nativeSlices.channels & MB_MeshVertexChannelFlags.blendIndices) == MB_MeshVertexChannelFlags.blendIndices);
                            System.Reflection.MethodInfo generic = method.MakeGenericMethod(typeof(VertexAndTriangleProcessorNativeArray.BlendWeightAsNormalizedInt));

                            nativeSlices.boneWeightsAsNormalizedUShorts = (NativeSlice<VertexAndTriangleProcessorNativeArray.BlendWeightAsNormalizedInt>)generic.Invoke(nativeSlices.rawSliceVertexStream_2, new object[] { offset });
                            // offset += sizeof(VertexAndTriangleProcessorNativeArray.BlendWeightAsNormalizedInt);
                        }
                    }
                }
                */
            }
            
            public static void NativeSliceCopyFrom(object toHereSlice, Type toHereSizerType, object fromHereSlice, Type fromHereSizerType)
            {
                Debug.Assert(toHereSizerType.Equals(fromHereSizerType));
                Type nst = typeof(NativeSlice<>);
                Type genericNst = nst.MakeGenericType(new Type[] { fromHereSizerType });
                System.Reflection.MethodInfo methodCopyFrom = genericNst.GetMethod("CopyFrom", new Type[] { genericNst });
                methodCopyFrom.Invoke(toHereSlice, new object[] { fromHereSlice });
            }

            public static void NativeSliceCopy<T>(NativeSlice<T> srcArray, int srcStartIdx, NativeSlice<T> destArray, int destStartIdx, int length) where T : struct
            {
                NativeSlice<T> src = srcArray.Slice(srcStartIdx, length);
                NativeSlice<T> ddd = destArray.Slice(destStartIdx, length);
                ddd.CopyFrom(src);
            }

            public static void NativeSliceCopyTo<T>(NativeSlice<T> srcArray, NativeSlice<T> destArray, int destStartIdx) where T : struct
            {
                NativeSlice<T> ddd = destArray.Slice(destStartIdx, srcArray.Length);
                ddd.CopyFrom(srcArray);
            }
        }
#endif
    }
}
