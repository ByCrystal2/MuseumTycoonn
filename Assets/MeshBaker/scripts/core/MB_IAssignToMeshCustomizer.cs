using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;

namespace DigitalOpus.MB.Core
{
    public interface IAssignToMeshCustomizer
    {

    }

    public interface IAssignToMeshCustomizer_SimpleAPI : IAssignToMeshCustomizer
    {
        /// <summary>
        /// For customizing data just before it is assigned to a mesh. If using Texture Arrays
        /// this can be used to inject the slice index into a coordinate in the mesh.
        /// </summary>
        void meshAssign_UV0(int channel, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, Mesh mesh, Vector2[] uvs, float[] sliceIndexes);

        /// <summary>
        /// For customizing data just before it is assigned to a mesh. If using Texture Arrays
        /// this can be used to inject the slice index into a coordinate in the mesh.
        /// </summary>
        void meshAssign_UV2(int channel, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, Mesh mesh, Vector2[] uvs, float[] sliceIndexes);

        /// <summary>
        /// For customizing data just before it is assigned to a mesh. If using Texture Arrays
        /// this can be used to inject the slice index into a coordinate in the mesh.
        /// </summary>
        void meshAssign_UV3(int channel, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, Mesh mesh, Vector2[] uvs, float[] sliceIndexes);

        /// <summary>
        /// For customizing data just before it is assigned to a mesh. If using Texture Arrays
        /// this can be used to inject the slice index into a coordinate in the mesh.
        /// </summary>
        void meshAssign_UV4(int channel, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, Mesh mesh, Vector2[] uvs, float[] sliceIndexes);

        /// <summary>
        /// For customizing data just before it is assigned to a mesh. If using Texture Arrays
        /// this can be used to inject the slice index into a coordinate in the mesh.
        /// </summary>
        void meshAssign_UV5(int channel, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, Mesh mesh, Vector2[] uvs, float[] sliceIndexes);

        /// <summary>
        /// For customizing data just before it is assigned to a mesh. If using Texture Arrays
        /// this can be used to inject the slice index into a coordinate in the mesh.
        /// </summary>
        void meshAssign_UV6(int channel, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, Mesh mesh, Vector2[] uvs, float[] sliceIndexes);

        /// <summary>
        /// For customizing data just before it is assigned to a mesh. If using Texture Arrays
        /// this can be used to inject the slice index into a coordinate in the mesh.
        /// </summary>
        void meshAssign_UV7(int channel, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, Mesh mesh, Vector2[] uvs, float[] sliceIndexes);

        /// <summary>
        /// For customizing data just before it is assigned to a mesh. If using Texture Arrays
        /// this can be used to inject the slice index into a coordinate in the mesh.
        /// </summary>
        void meshAssign_UV8(int channel, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, Mesh mesh, Vector2[] uvs, float[] sliceIndexes);

        /// <summary>
        /// For customizing data just before it is assigned to a mesh. If using Texture Arrays
        /// this can be used to inject the slice index into a coordinate in the mesh.
        /// </summary>
        void meshAssign_colors(MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, Mesh mesh, Color[] colors, float[] sliceIndexes);
    }

    public interface IAssignToMeshCustomizer_NativeArrays : IAssignToMeshCustomizer
    {
        /// <summary>
        /// Should return -1 if the extra slice parameter is not stored in a UV channel.
        /// Should return a value from 0 to 7 to indicate which UV channel is being used to store the extra parameter.
        /// </summary>
        int UVchannelWithExtraParameter();

        /// <summary>
        /// For customizing data just before it is assigned to a mesh. If using Texture Arrays
        /// this can be used to inject the slice index into a coordinate in the mesh.
        /// </summary>
        void meshAssign_UV(int channel_0_to_7, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, NativeSlice<Vector3> outUVsInMesh, NativeSlice<float> sliceIndexes);

        void meshAssign_colors(MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, NativeSlice<Color> outUVsInMesh, NativeSlice<float> sliceIndexes);
    }
}
