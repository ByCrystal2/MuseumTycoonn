using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using DigitalOpus.MB.Core;

namespace DigitalOpus.MB.Core
{
    /// <summary>
    /// This MeshAssignCustomizer alters the UV data as it is being assigned to the mesh.
    /// It appends the Texture Array slice index in the UV.z channel.
    /// 
    /// Shaders must be modified to read the slice index from the UV.z channel to use this.
    /// </summary>
    [CreateAssetMenu(fileName = "MeshAssignCustomizerNativeArrayPutSliceIdxInUV0_z", menuName = "Mesh Baker/Assign To Mesh Customizer/NativeArray API Put Slice Index In UV0.z", order = 1)]
    public class CustomizerNativeArrayPutSliceIndexInUV0_z : MB_DefaultMeshAssignCustomizer_NativeArray
    {
        /// <summary>
        /// Should return -1 if the extra slice parameter is not stored in a UV channel.
        /// Should return a value from 0 to 7 to indicate which UV channel is being used to store the extra parameter.
        /// </summary>
        public override int UVchannelWithExtraParameter()
        {
            return 0;
        }

        public override void meshAssign_UV(int channel, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, NativeSlice<Vector3> outUVsInMesh, NativeSlice<float> sliceIndexes)
        {
            if (textureBakeResults.resultType == MB2_TextureBakeResults.ResultType.atlas)
            {
                // Do nothing. uvs is aleady in the mesh.
            }
            else
            {
                {
                    // uvs is a Vector2 but the MeshData uvs will be Vector3
                    if (outUVsInMesh.Length == sliceIndexes.Length)
                    {
                        for (int i = 0; i < outUVsInMesh.Length; i++)
                        {
                            outUVsInMesh[i] = new Vector3(outUVsInMesh[i].x, outUVsInMesh[i].y, sliceIndexes[i]);
                        }

                        // We don't need to assign outUVs to the mesh. They are already in the mesh.
                    }
                    else
                    {
                        Debug.LogError("UV slice buffer was not the same size as the uv buffer");
                    }
                }
            }
        }
    }
}
