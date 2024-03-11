using System.Collections.Generic;
using System;
using UnityEngine;

namespace DigitalOpus.MB.Core
{
    public partial class MB3_MeshCombinerSingle : MB3_MeshCombiner
    {
        /// <summary>
        /// This is an abstraction layer between the MeshCombiner and the Mesh.
        /// It exists so that the MeshCombiner can use different APIs to interface with the Mesh.
        /// It needs to serialize its data with the MeshCombiner so it is responsible for managing the MeshCombiner data buffer fields
        /// </summary>
        public struct VertexAndTriangleProcessor : IVertexAndTriangleProcessor
        {
            private bool _disposed;
            private bool _isInitialized;

            internal MB2_LogLevel LOG_LEVEL;

            public MB_MeshVertexChannelFlags channels { get; private set; }
            private Vector3[] verticies;
            private Vector3[] normals;
            private Vector4[] tangents;
            private Color[] colors;
            private Vector2[] uv0s;
            private float[] uvsSliceIdx;
            private Vector2[] uv2s;
            private Vector2[] uv3s;
            private Vector2[] uv4s;
            private Vector2[] uv5s;
            private Vector2[] uv6s;
            private Vector2[] uv7s;
            private Vector2[] uv8s;
            private SerializableIntArray[] submeshTris;

            public void Dispose()
            {
                if (_disposed) return;
                _isInitialized = false;
                channels = MB_MeshVertexChannelFlags.none;
                verticies = null;
                normals = null;
                tangents = null;
                colors = null;
                uv0s = null;
                uvsSliceIdx = null;
                uv2s = null;
                uv3s = null;
                uv4s = null;
                uv5s = null;
                uv6s = null;
                uv7s = null;
                uv8s = null;
                submeshTris = null;
                _disposed = true;
            }

            public bool IsInitialized()
            {
                return _isInitialized;
            }

            public bool IsDisposed()
            {
                return _disposed;
            }

            public void Init(MB3_MeshCombinerSingle combiner, MB_MeshVertexChannelFlags newChannels, int vertexCount, int[] newSubmeshTrisSize, int uvChannelWithExtraParameter, IMeshChannelsCacheTaggingInterface meshChannelsCache, bool loadDataFromCombinedMesh, MB2_LogLevel logLevel)
            {
                if (loadDataFromCombinedMesh)
                {
                    InitFromMeshCombiner(combiner, newChannels, uvChannelWithExtraParameter);
                }
                else
                {
                    Debug.Assert(!_disposed, "MeshDataBuffer was disposed.");
                    Debug.Assert(meshChannelsCache.HasCollectedMeshData(), "Mesh Channels Cache as not collected mesh data.");
                    channels = newChannels;
                    if ((channels & MB_MeshVertexChannelFlags.vertex) != 0u) verticies = new Vector3[vertexCount];
                    if ((channels & MB_MeshVertexChannelFlags.normal) != 0u) normals = new Vector3[vertexCount];
                    if ((channels & MB_MeshVertexChannelFlags.tangent) != 0u) tangents = new Vector4[vertexCount];
                    if ((channels & MB_MeshVertexChannelFlags.colors) != 0u) colors = new Color[vertexCount];
                    if ((channels & MB_MeshVertexChannelFlags.uv0) != 0u) uv0s = new Vector2[vertexCount];
                    if ((channels & MB_MeshVertexChannelFlags.nuvsSliceIdx) != 0u) uvsSliceIdx = new float[vertexCount];
                    if ((channels & MB_MeshVertexChannelFlags.uv2) != 0u) uv2s = new Vector2[vertexCount];
                    if ((channels & MB_MeshVertexChannelFlags.uv3) != 0u) uv3s = new Vector2[vertexCount];
                    if ((channels & MB_MeshVertexChannelFlags.uv4) != 0u) uv4s = new Vector2[vertexCount];
                    if ((channels & MB_MeshVertexChannelFlags.uv5) != 0u) uv5s = new Vector2[vertexCount];
                    if ((channels & MB_MeshVertexChannelFlags.uv6) != 0u) uv6s = new Vector2[vertexCount];
                    if ((channels & MB_MeshVertexChannelFlags.uv7) != 0u) uv7s = new Vector2[vertexCount];
                    if ((channels & MB_MeshVertexChannelFlags.uv8) != 0u) uv8s = new Vector2[vertexCount];

                    submeshTris = new SerializableIntArray[newSubmeshTrisSize.Length];
                    for (int i = 0; i < newSubmeshTrisSize.Length; i++)
                    {
                        submeshTris[i] = new SerializableIntArray(newSubmeshTrisSize[i]);
                    }
                }
                
                _isInitialized = true;
            }

            public void InitShowHide(MB3_MeshCombinerSingle combiner)
            {
                Debug.Assert(!_disposed, "MeshDataBuffer was disposed.");
                channels = MB_MeshVertexChannelFlags.none;
                submeshTris = combiner.submeshTris;
                _isInitialized = true;
            }

            /// <summary>
            /// Loads data from the MeshCombiner serialized buffers into this MeshDataBuffers
            /// </summary>
            public void InitFromMeshCombiner(MB3_MeshCombinerSingle combiner, MB_MeshVertexChannelFlags newChannels, int uvChannelWithExtraParameter)
            {
                Debug.Assert(!_disposed, "MeshDataBuffer was disposed.");
                if (combiner.channelsLastBake != newChannels)
                {
                    if (combiner.channelsLastBake == MB_MeshVertexChannelFlags.none &&
                        combiner.verts.Length > 0)
                    {
                        // This is probably a saved scene that was baked and saved before serialized "channelsLastBake" variable existed.
                        // assume channels have not changed.
                        combiner.channelsLastBake = newChannels;
                    }
                    else
                    {
                        Debug.LogError("Shouldn't change channels between bakes. \n" + combiner.channelsLastBake +
                            " \n" + newChannels);
                    }
                }

                channels = combiner.channelsLastBake;
                if ((channels & MB_MeshVertexChannelFlags.vertex) != MB_MeshVertexChannelFlags.none) { verticies = combiner.verts; }
                if ((channels & MB_MeshVertexChannelFlags.normal) != MB_MeshVertexChannelFlags.none) { normals = combiner.normals; }
                if ((channels & MB_MeshVertexChannelFlags.tangent) != MB_MeshVertexChannelFlags.none) { tangents = combiner.tangents; }
                if ((channels & MB_MeshVertexChannelFlags.colors) != MB_MeshVertexChannelFlags.none) { colors = combiner.colors; }
                if ((channels & MB_MeshVertexChannelFlags.uv0) != MB_MeshVertexChannelFlags.none) { uv0s = combiner.uvs; }
                if ((channels & MB_MeshVertexChannelFlags.nuvsSliceIdx) != MB_MeshVertexChannelFlags.none) { uvsSliceIdx = combiner.uvsSliceIdx; }
                if ((channels & MB_MeshVertexChannelFlags.uv2) != MB_MeshVertexChannelFlags.none) { uv2s = combiner.uv2s; }
                if ((channels & MB_MeshVertexChannelFlags.uv3) != MB_MeshVertexChannelFlags.none) { uv3s = combiner.uv3s; }
                if ((channels & MB_MeshVertexChannelFlags.uv4) != MB_MeshVertexChannelFlags.none) { uv4s = combiner.uv4s; }
                if ((channels & MB_MeshVertexChannelFlags.uv5) != MB_MeshVertexChannelFlags.none) { uv5s = combiner.uv5s; }
                if ((channels & MB_MeshVertexChannelFlags.uv6) != MB_MeshVertexChannelFlags.none) { uv6s = combiner.uv6s; }
                if ((channels & MB_MeshVertexChannelFlags.uv7) != MB_MeshVertexChannelFlags.none) { uv7s = combiner.uv7s; }
                if ((channels & MB_MeshVertexChannelFlags.uv8) != MB_MeshVertexChannelFlags.none) { uv8s = combiner.uv8s; }
                submeshTris = combiner.submeshTris;
                _isInitialized = true;
            }

            public int GetVertexCount()
            {
                Debug.Assert(_isInitialized && !_disposed);
                return verticies.Length;
            }

            // Do we still need this? It isn't used anywhere
            public int GetSubmeshCount()
            {
                return submeshTris.Length;
            }

            public void TransferOwnershipOfSerializableBuffersToCombiner(MB3_MeshCombinerSingle c, 
                                                                        MB_MeshVertexChannelFlags channelsToTransfer,  // sometimes we don't transfer all the channels (eg. showHide)
                                                                        BufferDataFromPreviousBake serializableBufferData)
            {
                Debug.Assert(_isInitialized, "MeshDataBuffer is not initialized");
                Debug.Assert(!_disposed, "MeshDataBuffer is Disposed");
                c.channelsLastBake = channels;
                if ((channelsToTransfer & MB_MeshVertexChannelFlags.vertex) != MB_MeshVertexChannelFlags.none) { c.verts = verticies; }
                if ((channelsToTransfer & MB_MeshVertexChannelFlags.normal) != MB_MeshVertexChannelFlags.none) { c.normals = normals; }
                if ((channelsToTransfer & MB_MeshVertexChannelFlags.tangent) != MB_MeshVertexChannelFlags.none) { c.tangents = tangents; }
                if ((channelsToTransfer & MB_MeshVertexChannelFlags.uv0) != MB_MeshVertexChannelFlags.none) { c.uvs = uv0s; }
                if ((channelsToTransfer & MB_MeshVertexChannelFlags.nuvsSliceIdx) != MB_MeshVertexChannelFlags.none) { c.uvsSliceIdx = uvsSliceIdx; }
                if ((channelsToTransfer & MB_MeshVertexChannelFlags.uv2) != MB_MeshVertexChannelFlags.none) { c.uv2s = uv2s; }
                if ((channelsToTransfer & MB_MeshVertexChannelFlags.uv3) != MB_MeshVertexChannelFlags.none) { c.uv3s = uv3s; }
                if ((channelsToTransfer & MB_MeshVertexChannelFlags.uv4) != MB_MeshVertexChannelFlags.none) { c.uv4s = uv4s; }
                if ((channelsToTransfer & MB_MeshVertexChannelFlags.uv5) != MB_MeshVertexChannelFlags.none) { c.uv5s = uv5s; }
                if ((channelsToTransfer & MB_MeshVertexChannelFlags.uv6) != MB_MeshVertexChannelFlags.none) { c.uv6s = uv6s; }
                if ((channelsToTransfer & MB_MeshVertexChannelFlags.uv7) != MB_MeshVertexChannelFlags.none) { c.uv7s = uv7s; }
                if ((channelsToTransfer & MB_MeshVertexChannelFlags.uv8) != MB_MeshVertexChannelFlags.none) { c.uv8s = uv8s; }
                if ((channelsToTransfer & MB_MeshVertexChannelFlags.colors) != MB_MeshVertexChannelFlags.none) { c.colors = colors; }
                c.submeshTris = submeshTris;

                c.bufferDataFromPrevious = serializableBufferData;

                verticies = null;
                normals = null;
                tangents = null;
                uv0s = null;
                uvsSliceIdx = null;
                uv2s = null;
                uv3s = null;
                uv4s = null;
                uv5s = null;
                uv6s = null;
                uv7s = null;
                uv8s = null;
                colors = null;
                submeshTris = null;
                _isInitialized = false;
            }

            /// <summary>
            /// If we are adding one mesh to a combined mesh, it is more efficient to copy data from existing buffers rather than read raw data from Meshes and re-process it.
            /// </summary>
            public void CopyArraysFromPreviousBakeBuffersToNewBuffers(MB_DynamicGameObject dgo, ref IVertexAndTriangleProcessor iOldBuffers, int destStartVertIdx, int triangleIdxAdjustment, int[] targSubmeshTidx, MB2_LogLevel LOG_LEVEL)
            {
                Debug.Assert(_isInitialized && !_disposed);
                VertexAndTriangleProcessor oldBuffers = (VertexAndTriangleProcessor) iOldBuffers;
                int srcStartVertIdx = dgo.vertIdx;
                int numVerts = dgo.numVerts;
                if ((channels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex) { Array.Copy(oldBuffers.verticies, srcStartVertIdx, verticies, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal) { Array.Copy(oldBuffers.normals, srcStartVertIdx, normals, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent) { Array.Copy(oldBuffers.tangents, srcStartVertIdx, tangents, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0) { Array.Copy(oldBuffers.uv0s, srcStartVertIdx, uv0s, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.nuvsSliceIdx) == MB_MeshVertexChannelFlags.nuvsSliceIdx) { Array.Copy(oldBuffers.uvsSliceIdx, srcStartVertIdx, uvsSliceIdx, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2) { Array.Copy(oldBuffers.uv2s, srcStartVertIdx, uv2s, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3) { Array.Copy(oldBuffers.uv3s, srcStartVertIdx, uv3s, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4) { Array.Copy(oldBuffers.uv4s, srcStartVertIdx, uv4s, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5) { Array.Copy(oldBuffers.uv5s, srcStartVertIdx, uv5s, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6) { Array.Copy(oldBuffers.uv6s, srcStartVertIdx, uv6s, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7) { Array.Copy(oldBuffers.uv7s, srcStartVertIdx, uv7s, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8) { Array.Copy(oldBuffers.uv8s, srcStartVertIdx, uv8s, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors) { Array.Copy(oldBuffers.colors, srcStartVertIdx, colors, destStartVertIdx, numVerts); }

                //adjust triangles, then copy them over
                for (int subIdx = 0; subIdx < submeshTris.Length; subIdx++)
                {
                    int[] oldFromPrevBakeSubmeshTris = oldBuffers.submeshTris[subIdx].data;
                    int submeshStartIdx = dgo.submeshTriIdxs[subIdx];
                    int submeshNumTris = dgo.submeshNumTris[subIdx];
                    if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("    Adjusting submesh triangles submesh:" + subIdx + " startIdx:" + submeshStartIdx + " num:" + submeshNumTris + " nsubmeshTris:" + submeshTris.Length + " targSubmeshTidx:" + targSubmeshTidx.Length, LOG_LEVEL);
                    for (int triIdx = submeshStartIdx; triIdx < submeshStartIdx + submeshNumTris; triIdx++)
                    {
                        oldFromPrevBakeSubmeshTris[triIdx] = oldFromPrevBakeSubmeshTris[triIdx] - triangleIdxAdjustment;
                    }
                    Array.Copy(oldFromPrevBakeSubmeshTris, submeshStartIdx, submeshTris[subIdx].data, targSubmeshTidx[subIdx], submeshNumTris);
                }
            }

            public void CopyFromDGOMeshToBuffers(MB_DynamicGameObject dgo, int destStartVertsIdx,
                    MB_MeshVertexChannelFlags channelsToUpdate, bool updateTris, bool updateBWdata,
                    MB_IMeshBakerSettings settings, MB_IMeshCombinerSingle_BoneProcessor boneProcessor, int[] targSubmeshTidx,
                    MB2_TextureBakeResults textureBakeResults, UVAdjuster_Atlas uvAdjuster, MB2_LogLevel LOG_LEVEL,
                    IMeshChannelsCacheTaggingInterface meshChannelCacheParam)
            {
                MeshChannelsCache meshChannelCache = (MeshChannelsCache)meshChannelCacheParam;
                {
                    // Similar to local2world but with translation removed and we are using the inverse transpose.
                    // We use this for normals and tangents because it handles scaling correctly.
                    bool doVerts = ((channels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex) &&
                                   ((channelsToUpdate & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex);
                    bool doNorm = ((channels & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal) &&
                                  ((channelsToUpdate & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal);
                    bool doTan = ((channels & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent) &&
                                  ((channelsToUpdate & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent);
                    if (doVerts || doNorm || doTan)
                    {
                        //can't modify the arrays we get from the cache because they will be modified multiple times if the same mesh is being added multiple times.
                        Vector3[] dgoMeshVerts = null;
                        Vector3[] dgoMeshNorms = null;
                        Vector4[] dgoMeshTans = null;
                        if (doVerts) dgoMeshVerts = meshChannelCache.GetVertices(dgo._mesh);
                        if (doNorm) dgoMeshNorms = meshChannelCache.GetNormals(dgo._mesh);
                        if (doTan) dgoMeshTans = meshChannelCache.GetTangents(dgo._mesh);

                        if (settings.renderType != MB_RenderType.skinnedMeshRenderer)
                        {
                            _LocalToWorld(dgo.gameObject.transform, 
                                            doNorm, doTan, destStartVertsIdx, 
                                            dgoMeshVerts, dgoMeshNorms, dgoMeshTans, 
                                            verticies, normals, tangents);
                        }
                        else
                        {
                            //for skinned meshes leave in bind pose
                            boneProcessor.CopyVertsNormsTansToBuffers(dgo, settings, destStartVertsIdx, dgoMeshNorms, dgoMeshTans, dgoMeshVerts, normals, tangents, verticies);
                        }
                    }
                }

                if (((channels & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0) &&
                    ((channelsToUpdate & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0))
                {
                    _copyAndAdjustUVsFromMesh(textureBakeResults, dgo, dgo._mesh, 0, destStartVertsIdx, uv0s, uvsSliceIdx, meshChannelCache, LOG_LEVEL, textureBakeResults);
                }

                if (((channels & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2) &&
                    ((channelsToUpdate & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2))
                {
                    _CopyAndAdjustUV2FromMesh(settings, meshChannelCache, dgo, destStartVertsIdx, LOG_LEVEL);
                }

                if (((channels & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3) &&
                    ((channelsToUpdate & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3))
                {
                    Vector2[] dgoMeshNuv3s = meshChannelCache.GetUVChannel(3, dgo._mesh);
                    dgoMeshNuv3s.CopyTo(uv3s, destStartVertsIdx);
                }

                if (((channels & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4) &&
                    ((channelsToUpdate & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4))
                {
                    Vector2[] dgoMeshNuv4s = meshChannelCache.GetUVChannel(4, dgo._mesh);
                    dgoMeshNuv4s.CopyTo(uv4s, destStartVertsIdx);
                }

                if (((channels & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5) &&
                    ((channelsToUpdate & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5))
                {
                    Vector2[] dgoMeshNuv5s = meshChannelCache.GetUVChannel(5, dgo._mesh);
                    dgoMeshNuv5s.CopyTo(uv5s, destStartVertsIdx);
                }

                if (((channels & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6) &&
                    ((channelsToUpdate & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6))
                {
                    Vector2[] dgoMeshNuv6s = meshChannelCache.GetUVChannel(6, dgo._mesh);
                    dgoMeshNuv6s.CopyTo(uv6s, destStartVertsIdx);
                }

                if (((channels & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7) &&
                    ((channelsToUpdate & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7))
                {
                    Vector2[] dgoMeshNuv7s = meshChannelCache.GetUVChannel(7, dgo._mesh);
                    dgoMeshNuv7s.CopyTo(uv7s, destStartVertsIdx);
                }

                if (((channels & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8) &&
                    ((channelsToUpdate & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8))
                {
                    Vector2[] dgoMeshNuv8s = meshChannelCache.GetUVChannel(8, dgo._mesh);
                    dgoMeshNuv8s.CopyTo(uv8s, destStartVertsIdx);
                }

                if (((channels & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors) &&
                    ((channelsToUpdate & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors))
                {
                    Color[] dgoMeshNcolors = meshChannelCache.GetColors(dgo._mesh);
                    dgoMeshNcolors.CopyTo(colors, destStartVertsIdx);
                }

                if (updateBWdata)
                {
                    boneProcessor.UpdateGameObjects_UpdateBWIndexes(dgo);
                }

                if (updateTris)
                {
                    for (int combinedSubMeshIdx = 0; combinedSubMeshIdx < targSubmeshTidx.Length; combinedSubMeshIdx++)
                    {
                        dgo.submeshTriIdxs[combinedSubMeshIdx] = targSubmeshTidx[combinedSubMeshIdx];
                    }

                    for (int j = 0; j < dgo._tmpSubmeshTris.Length; j++)
                    {
                        // Triangles from source mesh use vertex indexes for source mesh, these need to be updated
                        // because verts were probably copied to different index positions
                        int[] submeshTris = dgo._tmpSubmeshTris[j].data;
                        if (destStartVertsIdx != 0)
                        {
                            for (int k = 0; k < submeshTris.Length; k++)
                            {
                                submeshTris[k] = submeshTris[k] + destStartVertsIdx;
                            }
                        }

                        if (dgo.invertTriangles)
                        {
                            //need to reverse winding order
                            for (int k = 0; k < submeshTris.Length; k += 3)
                            {
                                int tmp = submeshTris[k];
                                submeshTris[k] = submeshTris[k + 1];
                                submeshTris[k + 1] = tmp;
                            }
                        }

                        int submeshIdx = dgo.targetSubmeshIdxs[j];
                        submeshTris.CopyTo(this.submeshTris[submeshIdx].data, targSubmeshTidx[submeshIdx]);
                        dgo.submeshNumTris[submeshIdx] += submeshTris.Length;
                        targSubmeshTidx[submeshIdx] += submeshTris.Length;
                    }
                }
            }

            public void AssignBuffersToMesh(Mesh mesh, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults,
                MB_MeshVertexChannelFlags channelsToWriteToMesh,
                bool doWriteTrisToMesh,
                IAssignToMeshCustomizer assignToMeshCustomizer, List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh,
                out BufferDataFromPreviousBake serializableBufferData, out SerializableIntArray[] submeshTrisToUse, out int numNonZeroLengthSubmeshes)
            {
                Debug.Assert(_isInitialized && !_disposed);
                if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex)
                {
                    Vector3[] verts2Write;
                    AdjustVertsToWriteAccordingToPivotPositionIfNecessary(settings.pivotLocationType, settings.renderType, settings.clearBuffersAfterBake, settings.pivotLocation, out serializableBufferData, out verts2Write);
                    mesh.vertices = verts2Write;
                } else
                {
                    serializableBufferData.numVertsBaked = mesh.vertexCount;
                    serializableBufferData.meshVerticesShift = Vector3.zero;
                    serializableBufferData.meshVerticiesWereShifted = false;
                }

                if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal) mesh.normals = normals;
                if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent) mesh.tangents = tangents;

                if (assignToMeshCustomizer != null)
                {
                    Debug.Assert(assignToMeshCustomizer is IAssignToMeshCustomizer_SimpleAPI);
                    IAssignToMeshCustomizer_SimpleAPI assignToMeshCustomizerSimple = (IAssignToMeshCustomizer_SimpleAPI)assignToMeshCustomizer; 
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0) assignToMeshCustomizerSimple.meshAssign_UV0(0, settings, textureBakeResults, mesh, uv0s, uvsSliceIdx);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2) assignToMeshCustomizerSimple.meshAssign_UV2(2, settings, textureBakeResults, mesh, uv2s, uvsSliceIdx);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3) assignToMeshCustomizerSimple.meshAssign_UV3(3, settings, textureBakeResults, mesh, uv3s, uvsSliceIdx);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4) assignToMeshCustomizerSimple.meshAssign_UV4(4, settings, textureBakeResults, mesh, uv4s, uvsSliceIdx);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5) assignToMeshCustomizerSimple.meshAssign_UV5(5, settings, textureBakeResults, mesh, uv5s, uvsSliceIdx);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6) assignToMeshCustomizerSimple.meshAssign_UV6(6, settings, textureBakeResults, mesh, uv6s, uvsSliceIdx);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7) assignToMeshCustomizerSimple.meshAssign_UV7(7, settings, textureBakeResults, mesh, uv7s, uvsSliceIdx);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8) assignToMeshCustomizerSimple.meshAssign_UV8(8, settings, textureBakeResults, mesh, uv8s, uvsSliceIdx);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors) assignToMeshCustomizerSimple.meshAssign_colors(settings, textureBakeResults, mesh, colors, uvsSliceIdx);
                }
                else
                {
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0) MBVersion.MeshAssignUVChannel(0, mesh, uv0s);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2) MBVersion.MeshAssignUVChannel(2, mesh, uv2s);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3) MBVersion.MeshAssignUVChannel(3, mesh, uv3s);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4) MBVersion.MeshAssignUVChannel(4, mesh, uv4s);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5) MBVersion.MeshAssignUVChannel(5, mesh, uv5s);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6) MBVersion.MeshAssignUVChannel(6, mesh, uv6s);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7) MBVersion.MeshAssignUVChannel(7, mesh, uv7s);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8) MBVersion.MeshAssignUVChannel(8, mesh, uv8s);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors) mesh.colors = colors;
                }

                if (doWriteTrisToMesh)
                {
                    AssignTriangleDataForSubmeshes(mesh, mbDynamicObjectsInCombinedMesh, ref serializableBufferData, out submeshTrisToUse, out numNonZeroLengthSubmeshes);
                }
                else
                {
                    submeshTrisToUse = null;
                    numNonZeroLengthSubmeshes = -1;
                }
            }

            public void AssignTriangleDataForSubmeshes(Mesh mesh, List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, ref BufferDataFromPreviousBake serializableBufferData, out SerializableIntArray[] submeshTrisToUse, out int numNonZeroLengthSubmeshes)
            {
                submeshTrisToUse = GetSubmeshTrisWithShowHideApplied(mbDynamicObjectsInCombinedMesh);

                //submeshes with zero length tris cause error messages. must exclude these
                int numTotalIndexes = 0;
                numNonZeroLengthSubmeshes = _NumNonZeroLengthSubmeshTris(submeshTrisToUse, out numTotalIndexes);

                mesh.subMeshCount = numNonZeroLengthSubmeshes;
                int submeshIdx = 0;
                for (int i = 0; i < submeshTrisToUse.Length; i++)
                {
                    if (submeshTrisToUse[i].data.Length != 0)
                    {
                        mesh.SetTriangles(submeshTrisToUse[i].data, submeshIdx);
                        submeshIdx++;
                    }
                }
            }

            public void AssignTriangleDataForSubmeshes_ShowHide(Mesh mesh, List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, ref BufferDataFromPreviousBake serializableBufferData, out SerializableIntArray[] submeshTrisToUse, out int numNonZeroLengthSubmeshes)
            {
                AssignTriangleDataForSubmeshes(mesh, mbDynamicObjectsInCombinedMesh, ref serializableBufferData, out submeshTrisToUse, out numNonZeroLengthSubmeshes);
            }

            private void AdjustVertsToWriteAccordingToPivotPositionIfNecessary(MB_MeshPivotLocation pivotLocationType, MB_RenderType renderType, bool clearBuffersAfterBake, Vector3 pivotLocation_wld, out BufferDataFromPreviousBake serializableBufferData, out Vector3[] verts2Write)
            {
                Debug.Assert(_isInitialized && !_disposed);
                Debug.Assert((channels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex);
                verts2Write = verticies;
                serializableBufferData.numVertsBaked = verticies.Length;
                if (verticies.Length > 0)
                {
                    if (renderType == MB_RenderType.skinnedMeshRenderer)
                    {
                        serializableBufferData.numVertsBaked = verticies.Length;
                        serializableBufferData.meshVerticesShift = Vector3.zero;
                        serializableBufferData.meshVerticiesWereShifted = false;
                    }
                    else if (pivotLocationType == MB_MeshPivotLocation.worldOrigin)
                    {
                        serializableBufferData.numVertsBaked = verticies.Length;
                        serializableBufferData.meshVerticesShift = Vector3.zero;
                        serializableBufferData.meshVerticiesWereShifted = false;
                    }
                    else if (pivotLocationType == MB_MeshPivotLocation.boundsCenter ||
                             pivotLocationType == MB_MeshPivotLocation.customLocation)
                    {
                        Vector3 center;
                        if (pivotLocationType == MB_MeshPivotLocation.boundsCenter)
                        {
                            Vector3 max = verticies[0], min = verticies[0];
                            for (int i = 1; i < verticies.Length; i++)
                            {
                                Vector3 v = verticies[i];
                                if (max.x < v.x) max.x = v.x;
                                if (max.y < v.y) max.y = v.y;
                                if (max.z < v.z) max.z = v.z;
                                if (min.x > v.x) min.x = v.x;
                                if (min.y > v.y) min.y = v.y;
                                if (min.z > v.z) min.z = v.z;
                            }

                            center = (max + min) * .5f;
                        }
                        else
                        {
                            center = pivotLocation_wld;
                        }

                        if (!clearBuffersAfterBake)
                        {
                            // Vertex data copied from source meshes to meshCombiner.verts is in world-coordinates. We are going to shift vertex  
                            // positions here so that they are be relative to a new origin point. This is effectively a different coordinate system.
                            // We want to use a new buffer for these vertex positions because
                            // the data in the original buffer is serialized and the next time we access it we expect the data to be in world-coordinates not
                            // the new shifted coordinate system.
                            verts2Write = new Vector3[verticies.Length];
                        }

                        for (int i = 0; i < verticies.Length; i++)
                        {
                            verts2Write[i] = verticies[i] - center;
                        }

                        serializableBufferData.numVertsBaked = verticies.Length;
                        serializableBufferData.meshVerticesShift = center;
                        serializableBufferData.meshVerticiesWereShifted = true;
                    }
                    else
                    {
                        Debug.LogError("Unsupported Pivot Location Type: " + pivotLocationType);
                        serializableBufferData.numVertsBaked = verticies.Length;
                        serializableBufferData.meshVerticesShift = Vector3.zero;
                        serializableBufferData.meshVerticiesWereShifted = false;
                    }
                }
                else
                {
                    serializableBufferData.numVertsBaked = verticies.Length;
                    serializableBufferData.meshVerticesShift = Vector3.zero;
                    serializableBufferData.meshVerticiesWereShifted = false;
                }
            }

            private static int _NumNonZeroLengthSubmeshTris(SerializableIntArray[] subTris, out int numIndexes)
            {
                numIndexes = 0;
                int numNonZeroLength = 0;
                for (int i = 0; i < subTris.Length; i++) 
                { 
                    if (subTris[i].data.Length > 0)
                    {
                        numNonZeroLength++;
                        numIndexes += subTris[i].data.Length;
                    }
                }

                return numNonZeroLength;
            }

            private void _copyAndAdjustUVsFromMesh(MB2_TextureBakeResults tbr, MB_DynamicGameObject dgo, Mesh mesh, int uvChannel, int vertsIdx, Vector2[] uvsOut, float[] uvsSliceIdx, MeshChannelsCache meshChannelsCache, MB2_LogLevel LOG_LEVEL, MB2_TextureBakeResults textureBakeResults)
            {
                Debug.Assert(dgo.sourceSharedMaterials != null && dgo.sourceSharedMaterials.Length == dgo.targetSubmeshIdxs.Length,
                    "sourceSharedMaterials array was a different size than the targetSubmeshIdxs. Was this old data that is being updated? " + dgo.sourceSharedMaterials.Length);
                Vector2[] nuvs = meshChannelsCache.GetUVChannel(uvChannel, mesh);

                int[] done = new int[nuvs.Length]; //use this to track uvs that have already been adjusted don't adjust twice
                for (int l = 0; l < done.Length; l++) done[l] = -1;
                bool triangleArraysOverlap = false;

                //Rect uvRectInSrc = new Rect (0f,0f,1f,1f);
                //need to address the UVs through the submesh indexes because
                //each submesh has a different UV index
                bool doTextureArray = tbr.resultType == MB2_TextureBakeResults.ResultType.textureArray;
                for (int srcSubmeshIdx = 0; srcSubmeshIdx < dgo.targetSubmeshIdxs.Length; srcSubmeshIdx++)
                {
                    int[] srcSubTris;
                    if (dgo._tmpSubmeshTris != null)
                    {
                        srcSubTris = dgo._tmpSubmeshTris[srcSubmeshIdx].data;
                    }
                    else
                    {
                        srcSubTris = mesh.GetTriangles(srcSubmeshIdx);
                    }

                    float slice = dgo.textureArraySliceIdx[srcSubmeshIdx];
                    int resultSubmeshIdx = dgo.targetSubmeshIdxs[srcSubmeshIdx];

                    if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log(String.Format("Build UV transform for mesh {0} submesh {1} encapsulatingRect {2}",
                                dgo.name, srcSubmeshIdx, dgo.encapsulatingRect[srcSubmeshIdx]));

                    bool considerUVs = textureBakeResults.GetConsiderMeshUVs(resultSubmeshIdx, dgo.sourceSharedMaterials[srcSubmeshIdx]);
                    Rect rr = MB3_TextureCombinerMerging.BuildTransformMeshUV2AtlasRect(
                            considerUVs,
                            dgo.uvRects[srcSubmeshIdx],
                            dgo.obUVRects == null || dgo.obUVRects.Length == 0 ? new Rect(0, 0, 1, 1) : dgo.obUVRects[srcSubmeshIdx],
                            dgo.sourceMaterialTiling[srcSubmeshIdx],
                            dgo.encapsulatingRect[srcSubmeshIdx]);

                    for (int srcSubTriIdx = 0; srcSubTriIdx < srcSubTris.Length; srcSubTriIdx++)
                    {
                        int srcVertIdx = srcSubTris[srcSubTriIdx];
                        if (done[srcVertIdx] == -1)
                        {
                            done[srcVertIdx] = srcSubmeshIdx; //prevents a uv from being adjusted twice. Same vert can be on more than one submesh.
                            Vector2 nuv = nuvs[srcVertIdx]; //don't modify nuvs directly because it is cached and we might be re-using
                                                            //if (textureBakeResults.fixOutOfBoundsUVs) {
                                                            //uvRectInSrc can be larger than (out of bounds uvs) or smaller than 0..1
                                                            //this transforms the uvs so they fit inside the uvRectInSrc sample box 

                            // scale, shift to fit in atlas rect
                            nuv.x = rr.x + nuv.x * rr.width;
                            nuv.y = rr.y + nuv.y * rr.height;
                            int idx = vertsIdx + srcVertIdx;
                            uvsOut[idx] = nuv;
                            if (doTextureArray)
                            {
                                uvsSliceIdx[idx] = slice;
                            }
                        }
                        if (done[srcVertIdx] != srcSubmeshIdx)
                        {
                            triangleArraysOverlap = true;
                        }
                    }
                }
                if (triangleArraysOverlap)
                {
                    if (LOG_LEVEL >= MB2_LogLevel.warn)
                        Debug.LogWarning(dgo.name + "has submeshes which share verticies. Adjusted uvs may not map correctly in combined atlas.");
                }

                if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log(string.Format("_copyAndAdjustUVsFromMesh copied {0} verts", nuvs.Length));
            }

            private void _CopyAndAdjustUV2FromMesh(MB_IMeshBakerSettings settings, MeshChannelsCache meshChannelsCache, MB_DynamicGameObject dgo, int vertsIdx, MB2_LogLevel LOG_LEVEL)
            {
                Vector2[] dgoUV2s = meshChannelsCache.GetUVChannel(2, dgo._mesh);

                if (settings.lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping)
                { //has a lightmap
                  // the lightmapTilingOffset is always 1,1,0,0 for all objects
                  //lightMap index is always 1
                    if (dgoUV2s == null || dgoUV2s.Length == 0)
                    {
                        Vector2[] dgoUVs = meshChannelsCache.GetUVChannel(0, dgo._mesh);
                        if (dgoUVs != null && dgoUVs.Length > 0)
                        {
                            dgoUV2s = dgoUVs;
                        }
                        else
                        {
#if UNITY_EDITOR
                            Debug.LogError("Mesh " + dgo._mesh + " has no uv2 or uvs. Generating garbage UVs. Every UV = .5, .5");
#endif
                            if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Mesh " + dgo._mesh + " didn't have uv2s. Generating uv2s.");
                            dgoUV2s = meshChannelsCache.GetUv2Modified(dgo._mesh);
                        }
                    }

                    Vector2 uvscale2;
                    Vector4 lightmapTilingOffset = dgo.lightmapTilingOffset;
                    Vector2 uvscale = new Vector2(lightmapTilingOffset.x, lightmapTilingOffset.y);
                    Vector2 uvoffset = new Vector2(lightmapTilingOffset.z, lightmapTilingOffset.w);
                    for (int j = 0; j < dgoUV2s.Length; j++)
                    {
                        uvscale2.x = uvscale.x * dgoUV2s[j].x;
                        uvscale2.y = uvscale.y * dgoUV2s[j].y;
                        uv2s[vertsIdx + j] = uvoffset + uvscale2;
                    }
                    if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log("_copyAndAdjustUV2FromMesh copied and modify for preserve current lightmapping " + dgoUV2s.Length);
                }
                else
                {
                    if (dgoUV2s == null || dgoUV2s.Length == 0)
                    {
#if UNITY_EDITOR
                        Debug.LogError("Mesh " + dgo._mesh + " has no uv2s. Generating garbage uv2s. Every UV = .5, .5");
#endif
                        if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Mesh " + dgo._mesh + " didn't have uv2s. Generating uv2s.");
                        if (settings.lightmapOption == MB2_LightmapOptions.copy_UV2_unchanged_to_separate_rects && (dgoUV2s == null || dgoUV2s.Length == 0))
                        {
                            Debug.LogError("Mesh " + dgo._mesh + " did not have a UV2 channel. Nothing to copy when trying to copy UV2 to separate rects. " +
                              "The combined mesh will not lightmap properly. Try using generate new uv2 layout.");
                        }
                        dgoUV2s = meshChannelsCache.GetUv2Modified(dgo._mesh);
                    }
                    dgoUV2s.CopyTo(uv2s, vertsIdx);
                    if (LOG_LEVEL >= MB2_LogLevel.trace)
                    {
                        Debug.Log("_copyAndAdjustUV2FromMesh copied without modifying " + dgoUV2s.Length);
                    }
                }
            }

            public void CopyUV2unchangedToSeparateRects(List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, float uv2UnwrappingParamsPackMargin)
            {
                int uv2Padding = Mathf.CeilToInt(MB2_TexturePacker.MAX_ATLAS_SIZE * uv2UnwrappingParamsPackMargin);
                if (uv2Padding < 1) uv2Padding = 1;
                List<Vector2> uv2AtlasSizes = new List<Vector2>(mbDynamicObjectsInCombinedMesh.Count);
                float[] uv2ScaleInLightmap = new float[mbDynamicObjectsInCombinedMesh.Count];
                Rect[] uv2UsedRects = new Rect[mbDynamicObjectsInCombinedMesh.Count];
                float totalSizeScaleInLightmap = 0f;

                // Get the size in the lightmap for each dgo.
                {
                    for (int dgoIdx = 0; dgoIdx < mbDynamicObjectsInCombinedMesh.Count; dgoIdx++)
                    {
                        MB_DynamicGameObject dgo = mbDynamicObjectsInCombinedMesh[dgoIdx];
                        float scaleInLightmap = 1f;
                        if (Application.isEditor) // This functionality only exists in the editor
                        {
                            if (dgo._renderer is MeshRenderer)
                            {
                                MeshRenderer r = (MeshRenderer)dgo._renderer;
                                scaleInLightmap = MBVersion.GetScaleInLightmap(r);
                                if (scaleInLightmap <= 0f)
                                {
                                    scaleInLightmap = 1f;
                                }
                            }
                        }

                        float scaleRenderBounds = dgo.meshSize.magnitude;
                        uv2ScaleInLightmap[dgoIdx] = scaleInLightmap * scaleRenderBounds;
                        totalSizeScaleInLightmap += uv2ScaleInLightmap[dgoIdx];
                    }
                }

                // Normalize sizes so all rectangles sum to approx 1. Then muliply by MAX_ATLAS_SIZE which is
                // a large number so that we don't get round-to-pixel effects.
                for (int dgoIdx = 0; dgoIdx < uv2ScaleInLightmap.Length; dgoIdx++)
                {
                    MB_DynamicGameObject dgo = mbDynamicObjectsInCombinedMesh[dgoIdx];
                    int endIdx = dgo.vertIdx + dgo.numVerts;

                    float minx, maxx, miny, maxy;
                    minx = maxx = uv2s[dgo.vertIdx].x;
                    miny = maxy = uv2s[dgo.vertIdx].y;
                    for (int vertIdx = dgo.vertIdx; vertIdx < endIdx; vertIdx++)
                    {
                        if (uv2s[vertIdx].x < minx) minx = uv2s[vertIdx].x;
                        if (uv2s[vertIdx].x > maxx) maxx = uv2s[vertIdx].x;
                        if (uv2s[vertIdx].y < miny) miny = uv2s[vertIdx].y;
                        if (uv2s[vertIdx].y > maxy) maxy = uv2s[vertIdx].y;
                    }

                    uv2UsedRects[dgoIdx] = new Rect(minx, miny, maxx - minx, maxy - miny);
                    uv2ScaleInLightmap[dgoIdx] /= totalSizeScaleInLightmap;
                    Vector2 usedSize = new Vector2(uv2UsedRects[dgoIdx].width, uv2UsedRects[dgoIdx].height);
                    Vector2 atlasRectSize = usedSize * (uv2ScaleInLightmap[dgoIdx] * MB2_TexturePacker.MAX_ATLAS_SIZE);
                    uv2AtlasSizes.Add(atlasRectSize);
                }

                // Run texture packer on these rects.
                MB2_TexturePackerRegular tp = new MB2_TexturePackerRegular();
                tp.atlasMustBePowerOfTwo = false;
                AtlasPackingResult[] uv2Rects = tp.GetRects(uv2AtlasSizes, MB2_TexturePacker.MAX_ATLAS_SIZE, MB2_TexturePacker.MAX_ATLAS_SIZE, uv2Padding);
                AtlasPackingResult atlas = uv2Rects[0];
                for (int dgoIdx = 0; dgoIdx < mbDynamicObjectsInCombinedMesh.Count; dgoIdx++)
                {
                    MB_DynamicGameObject dgo = mbDynamicObjectsInCombinedMesh[dgoIdx];
                    int endIdx = dgo.vertIdx + dgo.numVerts;
                    Rect uv2UsedRect = uv2UsedRects[dgoIdx];

                    //  Scale and shift UV2s to fit the rect in the atlas
                    Rect rInAtlas = atlas.rects[dgoIdx];
                    for (int vertexIdx = dgo.vertIdx; vertexIdx < endIdx; vertexIdx++)
                    {
                        Vector2 uv2;
                        uv2.x = ((uv2s[vertexIdx].x - uv2UsedRect.x) / uv2UsedRect.width) * rInAtlas.width + rInAtlas.x;
                        uv2.y = ((uv2s[vertexIdx].y - uv2UsedRect.y) / uv2UsedRect.height) * rInAtlas.height + rInAtlas.y;
                        uv2s[vertexIdx] = uv2;
                    }

                    //  uv2s are currently stretched to fill entire 0..1 range on the X and Y dimensions
                    //  but the atlas in pixels may not be square (eg. 700 pixels by 200 pixels).
                    //  This distorts the aspect ratio of the source meshes.
                    //  Squash them in one dimension to the correct aspect ratio.
                    //  (eg. we would squash x dimension by 200 / 700  to correct the aspect ratio)
                    if (atlas.atlasX != atlas.atlasY)
                    {
                        if (atlas.atlasX < atlas.atlasY)
                        {
                            // squash X so that uv2s (0..1 space) and atlas (texels) have consistent aspect ratio
                            float scaleX = ((float)atlas.atlasX) / ((float)atlas.atlasY);
                            for (int vertexIdx = dgo.vertIdx; vertexIdx < endIdx; vertexIdx++)
                            {
                                Vector2 uv2 = uv2s[vertexIdx];
                                uv2.x *= scaleX;
                                uv2s[vertexIdx] = uv2;
                            }
                        }
                        else
                        {
                            // squash Y so that uv2s (0..1 space) and atlas (texels) have consistent aspect ratio
                            float scaleY = ((float)atlas.atlasY) / ((float)atlas.atlasX);
                            for (int vertexIdx = dgo.vertIdx; vertexIdx < endIdx; vertexIdx++)
                            {
                                Vector2 uv2 = uv2s[vertexIdx];
                                uv2.y *= scaleY;
                                uv2s[vertexIdx] = uv2;
                            }
                        }
                    }
                }
            }

            private SerializableIntArray[] GetSubmeshTrisWithShowHideApplied(List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh)
            {
                bool containsHiddenObjects = false;
                for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
                {
                    if (mbDynamicObjectsInCombinedMesh[i].show == false)
                    {
                        containsHiddenObjects = true;
                        break;
                    }
                }

                if (containsHiddenObjects)
                {
                    int[] newLengths = new int[submeshTris.Length];
                    SerializableIntArray[] newSubmeshTris = new SerializableIntArray[submeshTris.Length];
                    for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
                    {
                        MB_DynamicGameObject dgo = mbDynamicObjectsInCombinedMesh[i];
                        if (dgo.show)
                        {
                            for (int j = 0; j < dgo.submeshNumTris.Length; j++)
                            {
                            	
                                newLengths[j] += dgo.submeshNumTris[j];
                            }
                        }
                    }

                    for (int i = 0; i < newSubmeshTris.Length; i++)
                    {
                        newSubmeshTris[i] = new SerializableIntArray(newLengths[i]);
                    }

                    int[] idx = new int[newSubmeshTris.Length];
                    for (int dgoIdx = 0; dgoIdx < mbDynamicObjectsInCombinedMesh.Count; dgoIdx++)
                    {
                        MB_DynamicGameObject dgo = mbDynamicObjectsInCombinedMesh[dgoIdx];
                        if (dgo.show)
                        {
                            for (int submeshIdx = 0; submeshIdx < submeshTris.Length; submeshIdx++)
                            { //for each submesh
                                int[] triIdxs = submeshTris[submeshIdx].data;
                                int startIdx = dgo.submeshTriIdxs[submeshIdx];
                                int endIdx = startIdx + dgo.submeshNumTris[submeshIdx];
                                for (int triIdx = startIdx; triIdx < endIdx; triIdx++)
                                {
                                    newSubmeshTris[submeshIdx].data[idx[submeshIdx]] = triIdxs[triIdx];
                                    idx[submeshIdx] = idx[submeshIdx] + 1;
                                }
                            }
                        }
                    }

                    return newSubmeshTris;
                }
                else
                {
                    return submeshTris;
                }
            }

            public int[] GetTriangleSizes()
            {
                int[] oldSubmeshTrisSize = new int[submeshTris.Length];
                for (int i = 0; i < submeshTris.Length; i++)
                {
                    oldSubmeshTrisSize[i] = submeshTris[i].data.Length;
                }

                return oldSubmeshTrisSize;
            }

            private void _LocalToWorld(Transform t, bool doNorm, bool doTan, int destStartVertsIdx,
                                Vector3[] dgoMeshVerts, Vector3[] dgoMeshNorms, Vector4[] dgoMeshTans,
                                Vector3[] verticies, Vector3[] normals, Vector4[] tangents)
            {
                Vector3 scale = t.lossyScale;
                if (scale == Vector3.one)
                {
                    _LocalToWorld_TR(t.rotation, t.position, doNorm, doTan, destStartVertsIdx,
                        dgoMeshVerts, dgoMeshNorms, dgoMeshTans,
                        verticies, normals, tangents);
                } 
                else if (scale.x > Mathf.Epsilon && scale.y > Mathf.Epsilon && scale.z > Mathf.Epsilon)
                {
                    Matrix4x4 wld_X_local = t.localToWorldMatrix;
                    _LocalToWorldMatrix_TRS(ref wld_X_local, 
                                            doNorm, doTan, destStartVertsIdx,
                                            dgoMeshVerts, dgoMeshNorms, dgoMeshTans,
                                            verticies, normals, tangents);
                }
                else
                {
                    _LocalToWorld_TRS(t.rotation, t.position, t.lossyScale, doNorm, doTan, destStartVertsIdx,
                        dgoMeshVerts, dgoMeshNorms, dgoMeshTans,
                        verticies, normals, tangents);
                }
            }

            /// <summary>
            /// Faster but doesn't work when scale has zero component
            /// </summary>
            private static void _LocalToWorldMatrix_TRS(ref Matrix4x4 wld_X_local,
                                bool doNorm, bool doTan, int destStartVertsIdx,
                                Vector3[] dgoMeshVerts, Vector3[] dgoMeshNorms, Vector4[] dgoMeshTans,
                                Vector3[] verticies, Vector3[] normals, Vector4[] tangents)
            {
                Matrix4x4 l2wRotScale = Matrix4x4.zero;

                if (doNorm || doTan)
                {
                    l2wRotScale = wld_X_local;
                    l2wRotScale[0, 3] = l2wRotScale[1, 3] = l2wRotScale[2, 3] = 0f;
                    // if scale is zero than inverse is impossible (determinate is zero) 
                    l2wRotScale = l2wRotScale.inverse.transpose;
                }

                for (int dgoVertIdx = 0; dgoVertIdx < dgoMeshVerts.Length; dgoVertIdx++)
                {
                    int destVertIdx = destStartVertsIdx + dgoVertIdx;
                    verticies[destVertIdx] = wld_X_local.MultiplyPoint3x4(dgoMeshVerts[dgoVertIdx]);
                    if (doNorm)
                    {
                        normals[destVertIdx] = l2wRotScale.MultiplyPoint3x4(dgoMeshNorms[dgoVertIdx]).normalized;
                    }

                    if (doTan)
                    {
                        float w = dgoMeshTans[dgoVertIdx].w; //need to preserve the w value
                        Vector4 tt;
                        tt = l2wRotScale.MultiplyPoint3x4(((Vector3)dgoMeshTans[dgoVertIdx])).normalized;
                        tt.w = w;
                        tangents[destVertIdx] = tt;
                    }
                }
            }

            /// <summary>
            /// Optimized version that ommits scale
            /// </summary>
            private static void _LocalToWorld_TR(Quaternion wld_Rot_local, Vector3 position_wld, 
                                bool doNorm, bool doTan, int destStartVertsIdx,
                                Vector3[] dgoMeshVerts_local, Vector3[] dgoMeshNorms_local, Vector4[] dgoMeshTans_local,
                                Vector3[] verticies, Vector3[] normals, Vector4[] tangents)
            {
                for (int dgoVertIdx = 0; dgoVertIdx < dgoMeshVerts_local.Length; dgoVertIdx++)
                {
                    int destVertIdx = destStartVertsIdx + dgoVertIdx;
                    {
                        Vector3 v = dgoMeshVerts_local[dgoVertIdx];

                        // rotate
                        v = wld_Rot_local * v;

                        // translate
                        v += position_wld;
                        verticies[destVertIdx] = v;
                    }

                    if (doNorm)
                    {
                        Vector3 n = dgoMeshNorms_local[dgoVertIdx];
                        n = wld_Rot_local * n;
                        normals[destVertIdx] = n;
                    }

                    if (doTan)
                    {
                        Vector3 t = dgoMeshTans_local[dgoVertIdx];
                        float w = dgoMeshTans_local[dgoVertIdx].w;
                        t = wld_Rot_local * t;
                        Vector4 tt = t;
                        tt.w = w;
                        tangents[destVertIdx] = tt;
                    }
                }
            }

            /// <summary>
            /// Most correct but slowest version.
            /// </summary>
            private static void _LocalToWorld_TRS(Quaternion wld_Rot_local, Vector3 position_wld, Vector3 scale,
                                bool doNorm, bool doTan, int destStartVertsIdx,
                                Vector3[] dgoMeshVerts_local, Vector3[] dgoMeshNorms_local, Vector4[] dgoMeshTans_local,
                                Vector3[] verticies, Vector3[] normals, Vector4[] tangents)
            {
                Vector3 scaleInverted = Vector3.one;
                if (doNorm || doTan)
                {
                    // for normals and tangents that are scaled, we want to scale by 1/scaleComponent instead of scale.
                    scaleInverted.x = scale.x < Mathf.Epsilon ? 0f : 1f / scale.x;
                    scaleInverted.y = scale.y < Mathf.Epsilon ? 0f : 1f / scale.y;
                    scaleInverted.z = scale.z < Mathf.Epsilon ? 0f : 1f / scale.z;
                }

                for (int dgoVertIdx = 0; dgoVertIdx < dgoMeshVerts_local.Length; dgoVertIdx++)
                {
                    int destVertIdx = destStartVertsIdx + dgoVertIdx;
                    {
                        Vector3 v = dgoMeshVerts_local[dgoVertIdx];

                        // scale
                        v.x = v.x * scale.x;
                        v.y = v.y * scale.y;
                        v.z = v.z * scale.z;

                        // rotate
                        v = wld_Rot_local * v;

                        // translate
                        v += position_wld;
                        verticies[destVertIdx] = v;
                    }

                    if (doNorm)
                    {
                        Vector3 n = dgoMeshNorms_local[dgoVertIdx];
                        n.x = n.x * scaleInverted.x;
                        n.y = n.y * scaleInverted.y;
                        n.z = n.z * scaleInverted.z;
                        n = wld_Rot_local * n;
                        n.Normalize();
                        normals[destVertIdx] = n;
                    }

                    if (doTan)
                    {
                        Vector3 t = dgoMeshTans_local[dgoVertIdx];
                        float w = dgoMeshTans_local[dgoVertIdx].w;
                        t.x = t.x * scaleInverted.x;
                        t.y = t.y * scaleInverted.y;
                        t.z = t.z * scaleInverted.z;
                        t = wld_Rot_local * t;
                        t.Normalize();
                        tangents[destVertIdx] = new Vector4(t.x, t.y, t.z, w);
                    }
                }
            }
        }
    }
}
