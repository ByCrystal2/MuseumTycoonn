using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;

namespace DigitalOpus.MB.Core
{
    public class MB_DefaultMeshAssignCustomizer_NativeArray : ScriptableObject, IAssignToMeshCustomizer_NativeArrays
    {
        public virtual int UVchannelWithExtraParameter()
        {
            return -1; 
        }

        public virtual void meshAssign_UV(int channel, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, NativeSlice<Vector3> outUVsInMesh, NativeSlice<float> sliceIndexes)
        {
            // Don't need to assign to mesh. uvs are already in the mesh.
        }

        public virtual void meshAssign_colors(MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, NativeSlice<Color> outUVsInMesh, NativeSlice<float> sliceIndexes)
        {

        }

        /*
                public virtual void meshAssign_UV5(int channel, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, Mesh mesh, Vector2[] uvs, float[] sliceIndexes)
                {
        #if UNITY_2018_2_OR_NEWER
                    Debug.Assert(channel == 5);
                    mesh.uv5 = uvs;
        #endif
                }

                public virtual void meshAssign_UV6(int channel, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, Mesh mesh, Vector2[] uvs, float[] sliceIndexes)
                {
        #if UNITY_2018_2_OR_NEWER
                    Debug.Assert(channel == 6);
                    mesh.uv6 = uvs;
        #endif
                }

                public virtual void meshAssign_UV7(int channel, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, Mesh mesh, Vector2[] uvs, float[] sliceIndexes)
                {
        #if UNITY_2018_2_OR_NEWER
                    Debug.Assert(channel == 7);
                    mesh.uv7 = uvs;
        #endif
                }

                public virtual void meshAssign_UV8(int channel, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, Mesh mesh, Vector2[] uvs, float[] sliceIndexes)
                {
        #if UNITY_2018_2_OR_NEWER
                    Debug.Assert(channel == 8);
                    mesh.uv8 = uvs;
        #endif
                }

                public virtual void meshAssign_colors(MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, Mesh mesh, Color[] colors, float[] sliceIndexes)
                {
                    mesh.colors = colors;
                }

                public static void DefaultDelegateAssignMeshColors(MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults,
                            Mesh mesh, Color[] colors, float[] sliceIndexes)
                {
                    mesh.colors = colors;
                }
        */
    }
}
