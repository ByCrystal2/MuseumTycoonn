using System.Collections.Generic;
using System;
using UnityEngine;

namespace DigitalOpus.MB.Core
{
    public partial class MB3_MeshCombinerSingle : MB3_MeshCombiner
    {
        public interface IVertexAndTriangleProcessor : IDisposable
        {
            MB_MeshVertexChannelFlags channels { get; }
            bool IsInitialized();
            bool IsDisposed();
            void Init(MB3_MeshCombinerSingle combiner, MB_MeshVertexChannelFlags newChannels, int vertexCount, int[] newSubmeshTrisSize, int uvChannelWithExtraParameter, 
                      IMeshChannelsCacheTaggingInterface meshChannelsCache, bool loadDataFromCombinedMesh, MB2_LogLevel logLevel);
            void InitShowHide(MB3_MeshCombinerSingle combiner);
            void InitFromMeshCombiner(MB3_MeshCombinerSingle combiner, MB_MeshVertexChannelFlags newChannels, int uvChannelWithExtraParameter);
            int GetVertexCount();
            int GetSubmeshCount();
            void TransferOwnershipOfSerializableBuffersToCombiner(MB3_MeshCombinerSingle c, 
                                                                  MB_MeshVertexChannelFlags channelsToTransfer, // sometimes we don't transfer all the channels (eg. showHide) 
                                                                  BufferDataFromPreviousBake serializableBufferData);
            void CopyArraysFromPreviousBakeBuffersToNewBuffers(MB_DynamicGameObject dgo, ref IVertexAndTriangleProcessor iOldBuffers, int destStartVertIdx, int triangleIdxAdjustment, int[] targSubmeshTidx, MB2_LogLevel LOG_LEVEL);
            void CopyFromDGOMeshToBuffers(MB_DynamicGameObject dgo, int destStartVertsIdx,
                    MB_MeshVertexChannelFlags channelsToUpdate, bool updateTris, bool updateBWdata,
                    MB_IMeshBakerSettings settings, MB_IMeshCombinerSingle_BoneProcessor boneProcessor, int[] targSubmeshTidx,
                    MB2_TextureBakeResults textureBakeResults, UVAdjuster_Atlas uvAdjuster, MB2_LogLevel LOG_LEVEL,
                    IMeshChannelsCacheTaggingInterface meshChannelCache);

            void AssignBuffersToMesh(Mesh mesh, MB_IMeshBakerSettings settings,
                MB2_TextureBakeResults textureBakeResults,
                MB_MeshVertexChannelFlags channelsToWriteToMesh,
                bool doWriteTrisToMesh,
                IAssignToMeshCustomizer assignToMeshCustomizer,
                List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh,
                out BufferDataFromPreviousBake serializableBufferData, out SerializableIntArray[] submeshTrisToUse,
                out int numNonZeroLengthSubmeshes);

            void AssignTriangleDataForSubmeshes(Mesh mesh, List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, ref BufferDataFromPreviousBake serializableBufferData, out SerializableIntArray[] submeshTrisToUse, out int numNonZeroLengthSubmeshes);

            void AssignTriangleDataForSubmeshes_ShowHide(Mesh mesh, List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, ref BufferDataFromPreviousBake serializableBufferData, out SerializableIntArray[] submeshTrisToUse, out int numNonZeroLengthSubmeshes);
            
            void CopyUV2unchangedToSeparateRects(List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh,
                float uv2UnwrappingParamsPackMargin);
            int[] GetTriangleSizes();
        }
    }
}
