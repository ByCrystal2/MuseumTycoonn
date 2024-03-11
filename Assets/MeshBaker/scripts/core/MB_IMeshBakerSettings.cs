using UnityEngine;

namespace DigitalOpus.MB.Core
{
    public enum MB_MeshCombineAPIType
    {
        simpleMeshAPI,
        betaNativeArrayAPI
    }
    
    public interface MB_IMeshBakerSettingsHolder
    {
        MB_IMeshBakerSettings GetMeshBakerSettings();

        /// <summary>
        /// A settings holder could store settings in one of several places. We can't return a 
        /// SerializedProperty because that would be editor only. Instead we return the parameters
        /// needed to construct a serialized property.
        /// </summary>
        void GetMeshBakerSettingsAsSerializedProperty(out string propertyName, out UnityEngine.Object targetObj);
    }

    public interface MB_IMeshBakerSettings
    {
        bool doBlendShapes { get; set; }
        bool doCol { get; set; }
        bool doNorm { get; set; }
        bool doTan { get; set; }
        bool doUV { get; set; }
        bool doUV3 { get; set; }
        bool doUV4 { get; set; }
        bool doUV5 { get; set; }
        bool doUV6 { get; set; }
        bool doUV7 { get; set; }
        bool doUV8 { get; set; }
        MB2_LightmapOptions lightmapOption { get; set; }
        float uv2UnwrappingParamsHardAngle { get; set; }
        float uv2UnwrappingParamsPackMargin { get; set; }
        bool optimizeAfterBake { get; set; }
        MB_MeshPivotLocation pivotLocationType { get; set; }
        Vector3 pivotLocation { get; set; }
        bool clearBuffersAfterBake { get; set; }
        MB_RenderType renderType { get; set; }
        bool smrNoExtraBonesWhenCombiningMeshRenderers { get; set; }

        bool smrMergeBlendShapesWithSameNames { get; set; }

        IAssignToMeshCustomizer assignToMeshCustomizer { get; set; }

        MB_MeshCombineAPIType meshAPI { get; set; }
    }

    public static class MeshBakerSettingsUtility
    {
        public static MB_MeshVertexChannelFlags GetMeshChannelsAsFlags(MB_IMeshBakerSettings settings, bool doVerts, bool uvsSliceIdx_w)
        {
            MB_MeshVertexChannelFlags outFlags = (doVerts ? MB_MeshVertexChannelFlags.vertex : MB_MeshVertexChannelFlags.none) |
                            (settings.doNorm ? MB_MeshVertexChannelFlags.normal : MB_MeshVertexChannelFlags.none) |
                            (settings.doTan ? MB_MeshVertexChannelFlags.tangent : MB_MeshVertexChannelFlags.none) |
                            (settings.doCol ? MB_MeshVertexChannelFlags.colors : MB_MeshVertexChannelFlags.none) |
                            (settings.doUV ? MB_MeshVertexChannelFlags.uv0 : MB_MeshVertexChannelFlags.none) |
                            (uvsSliceIdx_w ? MB_MeshVertexChannelFlags.nuvsSliceIdx : MB_MeshVertexChannelFlags.none) |
                            (DoUV2getDataFromSourceMeshes(ref settings) ? MB_MeshVertexChannelFlags.uv2 : MB_MeshVertexChannelFlags.none) |
                            (settings.doUV3 ? MB_MeshVertexChannelFlags.uv3 : MB_MeshVertexChannelFlags.none) |
                            (settings.doUV4 ? MB_MeshVertexChannelFlags.uv4 : MB_MeshVertexChannelFlags.none) |
                            (settings.doUV5 ? MB_MeshVertexChannelFlags.uv5 : MB_MeshVertexChannelFlags.none) |
                            (settings.doUV6 ? MB_MeshVertexChannelFlags.uv6 : MB_MeshVertexChannelFlags.none) |
                            (settings.doUV7 ? MB_MeshVertexChannelFlags.uv7 : MB_MeshVertexChannelFlags.none) |
                            (settings.doUV8 ? MB_MeshVertexChannelFlags.uv8 : MB_MeshVertexChannelFlags.none) |
                            (settings.renderType == MB_RenderType.skinnedMeshRenderer ? MB_MeshVertexChannelFlags.blendWeight : MB_MeshVertexChannelFlags.none) |
                            (settings.renderType == MB_RenderType.skinnedMeshRenderer ? MB_MeshVertexChannelFlags.blendIndices : MB_MeshVertexChannelFlags.none);
            return outFlags;
        }

        /// <summary>
        /// UV2 channel has different ways to process based on: MB_IMeshBakerSettings
        /// Some of these options generate a completely new channel in the combined mesh. In these cases we don't want
        /// to get data from the sourceMeshes.
        /// </summary>
        public static bool DoUV2getDataFromSourceMeshes(ref MB_IMeshBakerSettings settings)
        {
            // For some UV2 settings like generateNewUV2Layout, there is no point in doing any copying data from source meshes from buffers because everything is getting re-generated
            bool result = (settings.lightmapOption == MB2_LightmapOptions.copy_UV2_unchanged) || 
                          (settings.lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping) || 
                          (settings.lightmapOption == MB2_LightmapOptions.copy_UV2_unchanged_to_separate_rects);
            return result;
        }
    }
}