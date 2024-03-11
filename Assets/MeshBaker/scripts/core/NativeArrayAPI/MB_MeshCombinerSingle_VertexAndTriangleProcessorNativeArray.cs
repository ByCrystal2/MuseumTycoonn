using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;

namespace DigitalOpus.MB.Core
{
    public partial class MB3_MeshCombinerSingle : MB3_MeshCombiner
    {

#if UNITY_2020_2_OR_NEWER
        /// <summary>
        /// This is an abstraction layer between the MeshCombiner and the Mesh.
        /// It exists so that the MeshCombiner can use different APIs to interface with the Mesh.
        /// It needs to serialize its data with the MeshCombiner so it is responsible for managing the MeshCombiner data buffer fields
        /// 
        /// ABOUT SKINNING DATA
        /// TBH I am puzzled how this works. It would appear as if there are two ways to access the
        /// skinned mesh data.
        /// 
        /// 1) We can set up NativeSlices and access the raw data buffer.
        /// 2) We can use GetBoneWeights() and SetBoneWeights()
        /// 
        /// I think it is safest to use the second and not touch the first.
        /// What I know:
        /// 
        ///      If I create a simple quad mesh, write some boneWeights using SetBoneWeights then:
        ///          If one bone has 7 bones per vertex then these are the attributes:
        ///              3  VertexAttribute: (attr=BlendWeight fmt=UNorm16 dim=4 stream=2)
        ///              4  VertexAttribute: (attr=BlendIndices fmt = UInt16 dim=4 stream=2)
        ///      Notice:  format is UNorm16, UInt16 and dimension is 4 (we have lost three of the weights)
        ///      
        ///      If I create the same quad mesh and all bones have 2 bones per vertex and write using write some boneWeights using SetBoneWeights
        ///         then the mesh has these attributes:
        ///             3  VertexAttribute: (attr=BlendWeight fmt=Float32 dim=2 stream=2)
        ///             4  VertexAttribute: (attr=BlendIndices fmt = UInt32 dim=2 stream=2)
        ///      Notice:  format is now UNorm16, UInt16
        ///      
        ///      If I write bones by assigning boneWeighs using mesh.boneWeights = arrayOfBoneWeights:
        ///         then the mesh has these attributes:
        ///             3  VertexAttribute: (attr=BlendWeight fmt=Float32 dim=2 stream=2)
        ///             4  VertexAttribute: (attr=BlendIndices fmt = UInt32 dim=2 stream=2)
        ///  
        /// The plan:
        ///     Write skinning data using GetBoneWeights and SetBoneWeights.
        ///     Don't bother allocating a buffer for the skinning data at all.
        ///     
        /// </summary>
        public struct VertexAndTriangleProcessorNativeArray : IVertexAndTriangleProcessor
        {
            /*
            [StructLayout(LayoutKind.Sequential)]
            public struct BlendWeightAsNormalizedInt
            {
                public ushort bw0;
                public ushort bw1;
                public ushort bw2;
                public ushort bw3;

                public ushort boneIdx0;
                public ushort boneIdx1;
                public ushort boneIdx2;
                public ushort boneIdx3;

                public static ushort ConvertFloatToNormalizedInt(float v)
                {
                    float max = Mathf.Pow(2f, 16f) - 1f;
                    return (ushort)(v * max);
                }

                public BlendWeightAsFloat Convert()
                {
                    float max = Mathf.Pow(2f, 16f) - 1f;
                    BlendWeightAsFloat bw;
                    bw.boneWeight0 = bw0 / max;
                    bw.boneWeight1 = bw1 / max;
                    bw.boneWeight2 = bw2 / max;
                    bw.boneWeight3 = bw3 / max;
                    return bw;
                }
            }

            public struct BlendWeightAsFloat
            {
                public float boneWeight0;
                public float boneWeight1;
                public float boneWeight2;
                public float boneWeight3;
            }
            */

            private bool _disposed;
            private bool _isInitialized;

            internal MB2_LogLevel LOG_LEVEL;

            public MB_MeshVertexChannelFlags channels { get; private set; }
            
            internal VertexAttributeDescriptor[] vertexAttributes;
            internal bool dataArrayAllocated;
            internal Mesh.MeshDataArray dataArray;
            internal Mesh.MeshData data;

            internal int vertexCount;

            /// <summary>
            /// This is sometimes needed if we reading from a read only mesh and need to modify the verticies.
            /// </summary>
            internal NativeArray<Vector3> verticiesModified;

            internal NativeSlice<Vector3> verticies;
            internal NativeSlice<Vector3> normals;
            internal NativeSlice<Vector4> tangents;
            internal NativeSlice<Color> colors;
            internal NativeSlice<Vector2> uv0s;
            internal NativeSlice<Vector2> uv2s;
            internal NativeSlice<Vector2> uv3s;
            internal NativeSlice<Vector2> uv4s;
            internal NativeSlice<Vector2> uv5s;
            internal NativeSlice<Vector2> uv6s;
            internal NativeSlice<Vector2> uv7s;
            internal NativeSlice<Vector2> uv8s;

            // We don't store any skinned mesh channels because I am currently using the
            // SetBoneWeights  and  GetBoneWeights API. Instead of writing to the mesh-skinning data buffer directly.
            // public  NativeSlice<BlendWeightAsNormalizedInt> boneWeightsAsNormalizedUShorts;

            // for texture arrays so we can put the slice in the extra index
            internal NativeSlice<float> uvsSliceIdx;
            internal NativeSlice<Vector3> uvsWithExtraIndex;

            private SerializableIntArray[] submeshTris;

            internal NativeArray<ushort> triangleBuffer;

            // Buffers
            internal int bufferStride_0;
            internal int bufferStride_1;
            internal int bufferStride_2;

            internal Type rawSliceSizerType_0; // This is a type the size of the buffer stride
            internal Type rawSliceSizerType_1; // This is a type the size of the buffer stride
            // internal Type rawSliceSizerType_2; // This is a type the size of the buffer stride

            internal object rawSliceVertexStream_0;
            internal object rawSliceVertexStream_1;
            // internal object rawSliceVertexStream_2;

            public void Dispose()
            {
                if (_disposed) return;
                _isInitialized = false;
                channels = MB_MeshVertexChannelFlags.none;

                if (dataArrayAllocated)
                {
                    dataArray.Dispose();
                    dataArrayAllocated = false;
                }

                if (verticiesModified.IsCreated)
                {
                    verticiesModified.Dispose();
                }

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

            /// <param name="uvChannelWithExtraParameter"> is for texture arrays </param>
            public void Init(MB3_MeshCombinerSingle combiner,
                             MB_MeshVertexChannelFlags newChannels, int vertexCount, int[] newSubmeshTrisSize, int uvChannelWithExtraParameter, 
                             IMeshChannelsCacheTaggingInterface meshChannelsCache,
                             bool loadDataFromCombinedMesh,
                             MB2_LogLevel logLevel)
            {
                Debug.Assert(!_disposed, "MeshDataBuffer was disposed.");
                if (meshChannelsCache != null) Debug.Assert(meshChannelsCache.HasCollectedMeshData(), "Mesh Channels Cache has not collected mesh data.");
                channels = newChannels;

                LOG_LEVEL = logLevel;

                int numVertexChannels = 0;
                int numUVchannels = 0;
                int numSkinChannels = 0;
                {
                    // Count the channels.
                    if ((channels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex) numVertexChannels++;
                    if ((channels & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal) numVertexChannels++;
                    if ((channels & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent) numVertexChannels++;
                    
                    if ((channels & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors) numUVchannels++;
                    if ((channels & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0) numUVchannels++;
                    if ((channels & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2) numUVchannels++;
                    if ((channels & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3) numUVchannels++;
                    if ((channels & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4) numUVchannels++;
                    if ((channels & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5) numUVchannels++;
                    if ((channels & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6) numUVchannels++;
                    if ((channels & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7) numUVchannels++;
                    if ((channels & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8) numUVchannels++;

                    if ((channels & MB_MeshVertexChannelFlags.blendWeight) == MB_MeshVertexChannelFlags.blendWeight) numSkinChannels++;
                    if ((channels & MB_MeshVertexChannelFlags.blendIndices) == MB_MeshVertexChannelFlags.blendIndices) numSkinChannels++;
                }

                // Order is important. Must be:
                // position, normal, tangent, color, texcoord0 .... texcord7, blendweight, blendindicies
                vertexAttributes = new VertexAttributeDescriptor[numVertexChannels + numUVchannels + numSkinChannels];
                MB_MeshCombinerSingle_MeshNativeArrayHelper.Init(channels, vertexAttributes, ref this, vertexCount, newSubmeshTrisSize, uvChannelWithExtraParameter);

                if (loadDataFromCombinedMesh)
                {
                    //System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    //sb.AppendLine("Copying old data from combined mesh.");
                    Debug.Assert(combiner._mesh.vertexCount == vertexCount);
                    Debug.Assert(combiner.bufferDataFromPrevious.numVertsBaked == vertexCount);
                    Debug.Assert(combiner.channelsLastBake == channels);
                    submeshTris = combiner.submeshTris;

                    // Get readable data from mesh.
                    VertexAndTriangleProcessorNativeArray oldData = new VertexAndTriangleProcessorNativeArray();
                    oldData.InitFromMeshCombiner(combiner, channels, -1);

                    //sb.AppendLine(" bufferStride_0 " + oldData.bufferStride_0);
                    //sb.AppendLine(" bufferStride_1 " + oldData.bufferStride_1);
                    //sb.AppendLine(" bufferStride_2 " + oldData.bufferStride_2);
                    
                    // Copy data from combined mesh to writable mesh data.
                    if (oldData.bufferStride_0 > 0)
                    {
                        MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyFrom(rawSliceVertexStream_0, rawSliceSizerType_0,
                                                                                        oldData.rawSliceVertexStream_0, oldData.rawSliceSizerType_0);
                    }

                    if (oldData.bufferStride_1 > 0)
                    {
                        MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyFrom(rawSliceVertexStream_1, rawSliceSizerType_1,
                                                                                        oldData.rawSliceVertexStream_1, oldData.rawSliceSizerType_1);
                    }

                    /*
                    if (oldData.bufferStride_2 > 0)
                    {
                        MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyFrom(rawSliceVertexStream_2, rawSliceSizerType_2,
                                                                                        oldData.rawSliceVertexStream_2, oldData.rawSliceSizerType_2);
                    }
                    */

                    {
                        if (oldData.data.indexFormat == IndexFormat.UInt16)
                        {
                            NativeArray<ushort> indexBufferOld = oldData.data.GetIndexData<ushort>();
                            data.SetIndexBufferParams(indexBufferOld.Length, IndexFormat.UInt16);
                            NativeArray<ushort> indexBufferNew = data.GetIndexData<ushort>();
                            indexBufferNew.CopyFrom(indexBufferOld);
                            data.subMeshCount = oldData.data.subMeshCount;
                            for (int i = 0; i < data.subMeshCount; i++)
                            {
                                SubMeshDescriptor submesh = oldData.data.GetSubMesh(i);
                                data.SetSubMesh(i, submesh);
                            }
                        }
                        else
                        {
                            NativeArray<uint> indexBufferOld = oldData.data.GetIndexData<uint>();
                            data.SetIndexBufferParams(indexBufferOld.Length, IndexFormat.UInt32);
                            NativeArray<uint> indexBufferNew = data.GetIndexData<uint>();
                            indexBufferNew.CopyFrom(indexBufferOld);
                            data.subMeshCount = oldData.data.subMeshCount;
                            for (int i = 0; i < data.subMeshCount; i++)
                            {
                                SubMeshDescriptor submesh = oldData.data.GetSubMesh(i);
                                data.SetSubMesh(i, submesh);
                            }
                        }

                        //sb.AppendLine("Copying index data numSubmeshes: " + data.subMeshCount);
                    }
                    
                    //Debug.Log(sb.ToString());
                    
                    
                    oldData.Dispose();
                }
                else
                {
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
                // Setup writable mesh data.
                channels = combiner.channelsLastBake;
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

                Debug.Assert(newChannels == combiner.channelsLastBake, "Channels have changed since last bake. This is not supported when data is not cleared between bakes." +
                    " Is Clear Buffers After Bake unchecked?");

                channels = combiner.channelsLastBake;

                dataArray = Mesh.AcquireReadOnlyMeshData(combiner._mesh);
                dataArrayAllocated = true;
                data = dataArray[0];

                if (LOG_LEVEL >= MB2_LogLevel.debug)
                {
                    string s = "Vertex attributes in combined mesh: ";
                    for (int vertAttrIdx = 0; vertAttrIdx < combiner._mesh.vertexAttributeCount; vertAttrIdx++)
                    {
                        VertexAttributeDescriptor vertAttr = combiner._mesh.GetVertexAttribute(vertAttrIdx);
                        s += "\n    " + (vertAttrIdx + "  VertexAttribute: " + vertAttr);
                    }

                    Debug.Log(s);
                }

                int strideVertexBuffer, strideUVbuffer;
                MB_MeshCombinerSingle_MeshNativeArrayHelper.CalcStride(channels, uvChannelWithExtraParameter, out strideVertexBuffer, out strideUVbuffer);
                MB_MeshCombinerSingle_MeshNativeArrayHelper.SetupNativeSlices(ref this, strideVertexBuffer, strideUVbuffer, uvChannelWithExtraParameter);

                {
                    if (combiner.bufferDataFromPrevious.meshVerticiesWereShifted)
                    {
                        // The verticies in the combined mesh from the previous bake were shifted. We need to unshift theses.
                        // We need to allocate a temporary verticies buffer because the  MeshData one is read only.
                        verticiesModified = new NativeArray<Vector3>(verticies.Length, Allocator.Temp);
                        Vector3 shift = combiner.bufferDataFromPrevious.meshVerticesShift;
                        for (int i = 0; i < verticies.Length; i++)
                        {
                            verticiesModified[i] = verticies[i] + shift;
                        }

                        verticies = verticiesModified.Slice<Vector3>();
                    }
                }

                submeshTris = combiner.submeshTris;
                _isInitialized = true;
            }

            public void ApplyDataBufferToMesh(Mesh m)
            {
                Debug.Assert(_isInitialized && !_disposed);
                // This happens in Apply()
                data.subMeshCount = 1;
                data.SetSubMesh(0, new SubMeshDescriptor(0, triangleBuffer.Length));
                Mesh.ApplyAndDisposeWritableMeshData(dataArray, m);
                dataArrayAllocated = false;
                m.RecalculateBounds();
            }

            public int GetVertexCount()
            {
                Debug.Assert(_isInitialized && !_disposed);
                Debug.Assert(vertexCount == verticies.Length);
                return verticies.Length;
            }

            // Do we still need this? It isn't used anywhere
            public int GetSubmeshCount()
            {
                return submeshTris.Length;
            }

            public void TransferOwnershipOfSerializableBuffersToCombiner(MB3_MeshCombinerSingle c, MB_MeshVertexChannelFlags channelsToTransfer, BufferDataFromPreviousBake serializableBufferData)
            {
                Debug.Assert(_isInitialized, "MeshDataBuffer is not initialized");
                Debug.Assert(!_disposed, "MeshDataBuffer is Disposed");
                c.channelsLastBake = channels;

                c.bufferDataFromPrevious = serializableBufferData;

                /*
                if ((channels & MB_MeshVertexChannelFlags.vertex) != MB_MeshVertexChannelFlags.none) { c.verts = verticies; }
                if ((channels & MB_MeshVertexChannelFlags.normal) != MB_MeshVertexChannelFlags.none) { c.normals = normals; }
                if ((channels & MB_MeshVertexChannelFlags.tangent) != MB_MeshVertexChannelFlags.none) { c.tangents = tangents; }
                if ((channels & MB_MeshVertexChannelFlags.uv0) != MB_MeshVertexChannelFlags.none) { c.uvs = uv0s; }
                if ((channels & MB_MeshVertexChannelFlags.nuvsSliceIdx) != MB_MeshVertexChannelFlags.none) { c.uvsSliceIdx = uvsSliceIdx; }
                if ((channels & MB_MeshVertexChannelFlags.uv2) != MB_MeshVertexChannelFlags.none) { c.uv2s = uv2s; }
                if ((channels & MB_MeshVertexChannelFlags.uv3) != MB_MeshVertexChannelFlags.none) { c.uv3s = uv3s; }
                if ((channels & MB_MeshVertexChannelFlags.uv4) != MB_MeshVertexChannelFlags.none) { c.uv4s = uv4s; }
                if ((channels & MB_MeshVertexChannelFlags.uv5) != MB_MeshVertexChannelFlags.none) { c.uv5s = uv5s; }
                if ((channels & MB_MeshVertexChannelFlags.uv6) != MB_MeshVertexChannelFlags.none) { c.uv6s = uv6s; }
                if ((channels & MB_MeshVertexChannelFlags.uv7) != MB_MeshVertexChannelFlags.none) { c.uv7s = uv7s; }
                if ((channels & MB_MeshVertexChannelFlags.uv8) != MB_MeshVertexChannelFlags.none) { c.uv8s = uv8s; }
                if ((channels & MB_MeshVertexChannelFlags.colors) != MB_MeshVertexChannelFlags.none) { c.colors = colors; }
                */

                c.submeshTris = submeshTris;

                // verticies = null;
                // normals = null;
                // tangents = null;
                // uv0s = null;
                // uvsSliceIdx = null;
                // uv2s = null;
                // uv3s = null;
                // uv4s = null;
                // uv5s = null;
                // uv6s = null;
                // uv7s = null;
                // uv8s = null;
                // colors = null;
                submeshTris = null;
                _isInitialized = false;
            }

            /// <summary>
            /// If we are adding one mesh to a combined mesh, it is more efficient to copy data from existing buffers rather than read raw data from Meshes and re-process it.
            /// </summary>
            public void CopyArraysFromPreviousBakeBuffersToNewBuffers(MB_DynamicGameObject dgo, ref IVertexAndTriangleProcessor iOldBuffers, int destStartVertIdx, int triangleIdxAdjustment, int[] targSubmeshTidx, MB2_LogLevel LOG_LEVEL)
            {
                Debug.Assert(_isInitialized && !_disposed);
                VertexAndTriangleProcessorNativeArray oldBuffers = (VertexAndTriangleProcessorNativeArray)iOldBuffers;
                int srcStartVertIdx = dgo.vertIdx;
                int numVerts = dgo.numVerts;
                if ((channels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex) { MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy<Vector3>(oldBuffers.verticies, srcStartVertIdx, verticies, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal) { MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy<Vector3>(oldBuffers.normals, srcStartVertIdx, normals, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent) { MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy<Vector4>(oldBuffers.tangents, srcStartVertIdx, tangents, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0) { MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy<Vector2>(oldBuffers.uv0s, srcStartVertIdx, uv0s, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.nuvsSliceIdx) == MB_MeshVertexChannelFlags.nuvsSliceIdx) { MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy<float>(oldBuffers.uvsSliceIdx, srcStartVertIdx, uvsSliceIdx, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2) { MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy<Vector2>(oldBuffers.uv2s, srcStartVertIdx, uv2s, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3) { MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy<Vector2>(oldBuffers.uv3s, srcStartVertIdx, uv3s, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4) { MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy<Vector2>(oldBuffers.uv4s, srcStartVertIdx, uv4s, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5) { MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy<Vector2>(oldBuffers.uv5s, srcStartVertIdx, uv5s, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6) { MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy<Vector2>(oldBuffers.uv6s, srcStartVertIdx, uv6s, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7) { MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy<Vector2>(oldBuffers.uv7s, srcStartVertIdx, uv7s, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8) { MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy<Vector2>(oldBuffers.uv8s, srcStartVertIdx, uv8s, destStartVertIdx, numVerts); }
                if ((channels & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors) { MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy<Color>(oldBuffers.colors, srcStartVertIdx, colors, destStartVertIdx, numVerts); }

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
                MeshChannelsCache_NativeArray meshChannelCache = (MeshChannelsCache_NativeArray) meshChannelCacheParam;
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
                        NativeArray<Vector3> dgoMeshVerts = new NativeArray<Vector3>();
                        NativeArray<Vector3> dgoMeshNorms = new NativeArray<Vector3>();
                        NativeArray<Vector4> dgoMeshTans = new NativeArray<Vector4>();
                        if (doVerts) dgoMeshVerts = meshChannelCache.GetVerticiesAsNativeArray(dgo._mesh);
                        if (doNorm) dgoMeshNorms = meshChannelCache.GetNormalsAsNativeArray(dgo._mesh);
                        if (doTan) dgoMeshTans = meshChannelCache.GetTangentsAsNativeArray(dgo._mesh);

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
                    NativeArray<Vector2> dgoMeshNuv3s = meshChannelCache.GetUVChannelAsNativeArray(3, dgo._mesh);
                    // dgoMeshNuv3s.CopyTo(uv3s, destStartVertsIdx);
                    MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyTo(dgoMeshNuv3s, uv3s, destStartVertsIdx);
                }

                if (((channels & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4) &&
                    ((channelsToUpdate & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4))
                {
                    NativeArray<Vector2> dgoMeshNuv4s = meshChannelCache.GetUVChannelAsNativeArray(4, dgo._mesh);
                    // dgoMeshNuv4s.CopyTo(uv4s, destStartVertsIdx);
                    MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyTo(dgoMeshNuv4s, uv4s, destStartVertsIdx);
                }

                if (((channels & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5) &&
                    ((channelsToUpdate & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5))
                {
                    NativeArray<Vector2> dgoMeshNuv5s = meshChannelCache.GetUVChannelAsNativeArray(5, dgo._mesh);
                    // dgoMeshNuv5s.CopyTo(uv5s, destStartVertsIdx);
                    MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyTo(dgoMeshNuv5s, uv5s, destStartVertsIdx);
                }

                if (((channels & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6) &&
                    ((channelsToUpdate & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6))
                {
                    NativeArray<Vector2> dgoMeshNuv6s = meshChannelCache.GetUVChannelAsNativeArray(6, dgo._mesh);
                    // dgoMeshNuv6s.CopyTo(uv6s, destStartVertsIdx);
                    MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyTo(dgoMeshNuv6s, uv6s, destStartVertsIdx);
                }

                if (((channels & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7) &&
                    ((channelsToUpdate & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7))
                {
                    NativeArray<Vector2> dgoMeshNuv7s = meshChannelCache.GetUVChannelAsNativeArray(7, dgo._mesh);
                    // dgoMeshNuv7s.CopyTo(uv7s, destStartVertsIdx);
                    MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyTo(dgoMeshNuv7s, uv7s, destStartVertsIdx);
                }

                if (((channels & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8) &&
                    ((channelsToUpdate & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8))
                {
                    NativeArray<Vector2> dgoMeshNuv8s = meshChannelCache.GetUVChannelAsNativeArray(8, dgo._mesh);
                    // dgoMeshNuv8s.CopyTo(uv8s, destStartVertsIdx);
                    MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyTo(dgoMeshNuv8s, uv8s, destStartVertsIdx);
                }

                if (((channels & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors) &&
                    ((channelsToUpdate & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors))
                {
                    NativeArray<Color> dgoMeshNcolors = meshChannelCache.GetColorsAsNativeArray(dgo._mesh);
                    // dgoMeshNcolors.CopyTo(colors, destStartVertsIdx);
                    MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyTo(dgoMeshNcolors, colors, destStartVertsIdx);
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
                    //NativeSlice<Vector3> verts2Write;
                    AdjustVertsToWriteAccordingToPivotPositionIfNecessary(settings.pivotLocationType, settings.renderType, settings.clearBuffersAfterBake, settings.pivotLocation, out serializableBufferData);
                    // mesh.vertices = verts2Write;
                }
                else
                {
                    serializableBufferData.numVertsBaked = data.vertexCount;
                    serializableBufferData.meshVerticesShift = Vector3.zero;
                    serializableBufferData.meshVerticiesWereShifted = false;
                }

                if (assignToMeshCustomizer != null)
                {
                    Debug.Assert(assignToMeshCustomizer is IAssignToMeshCustomizer_NativeArrays, "Assign to mesh customizer must implement: IAssignToMeshCustomizer_NativeArrays");
                    IAssignToMeshCustomizer_NativeArrays customizerNA = (IAssignToMeshCustomizer_NativeArrays)assignToMeshCustomizer;
                    
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0) customizerNA.meshAssign_UV(0, settings, textureBakeResults, uvsWithExtraIndex, uvsSliceIdx);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2) customizerNA.meshAssign_UV(1, settings, textureBakeResults, uvsWithExtraIndex, uvsSliceIdx);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3) customizerNA.meshAssign_UV(2, settings, textureBakeResults, uvsWithExtraIndex, uvsSliceIdx);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4) customizerNA.meshAssign_UV(3, settings, textureBakeResults, uvsWithExtraIndex, uvsSliceIdx);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5) customizerNA.meshAssign_UV(4, settings, textureBakeResults, uvsWithExtraIndex, uvsSliceIdx);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6) customizerNA.meshAssign_UV(5, settings, textureBakeResults, uvsWithExtraIndex, uvsSliceIdx);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7) customizerNA.meshAssign_UV(6, settings, textureBakeResults, uvsWithExtraIndex, uvsSliceIdx);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8) customizerNA.meshAssign_UV(7, settings, textureBakeResults, uvsWithExtraIndex, uvsSliceIdx);
                    if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors) customizerNA.meshAssign_colors(settings, textureBakeResults, colors, uvsSliceIdx);
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

                Mesh.ApplyAndDisposeWritableMeshData(dataArray, mesh);
                dataArrayAllocated = false;
            }

            /// <summary>
            /// This is very similar to AssignTriangleDataForSubmeshes_ShowHide. They do the same thing but:
            ///     AssignTriangleDataForSubmeshes operates on a MeshData
            ///     AssignTriangleDataForSubmeshes_ShowHide operates on a Mesh directly.
            /// </summary>
            public void AssignTriangleDataForSubmeshes(Mesh mmesh, List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, ref BufferDataFromPreviousBake serializableBufferData, out SerializableIntArray[] submeshTrisToUse, out int numNonZeroLengthSubmeshes)
            {
                submeshTrisToUse = GetSubmeshTrisWithShowHideApplied(mbDynamicObjectsInCombinedMesh);

                //submeshes with zero length tris cause error messages. must exclude these
                int numTotalIndexes;
                numNonZeroLengthSubmeshes = _NumNonZeroLengthSubmeshTris(submeshTrisToUse, out numTotalIndexes);

                {
                    IndexFormat indexFormat;
                    if (numTotalIndexes > System.UInt16.MaxValue)
                    {
                        indexFormat = IndexFormat.UInt32;
                    }
                    else
                    {
                        indexFormat = IndexFormat.UInt16;
                    }

                    data.SetIndexBufferParams(numTotalIndexes, indexFormat);
                    if (indexFormat == IndexFormat.UInt16)
                    {
                        int submeshIdx = 0;
                        int indexStart = 0;
                        NativeArray<ushort> newIndexes = data.GetIndexData<ushort>();
                        for (int i = 0; i < submeshTrisToUse.Length; i++)
                        {
                            if (submeshTrisToUse[i].data.Length != 0)
                            {
                                SerializableIntArray submesh = submeshTrisToUse[i];
                                for (int k = 0; k < submesh.data.Length; k++)
                                {
                                    newIndexes[indexStart + k] = (ushort)submesh.data[k];
                                }

                                submeshIdx++;
                                indexStart += submesh.data.Length;
                            }
                        }
                        
                        // Don't dispose index data because its owned by MeshData
                    }
                    else
                    {
                        int submeshIdx = 0;
                        int indexStart = 0;
                        NativeArray<uint> newIndexes = data.GetIndexData<uint>();
                        for (int i = 0; i < submeshTrisToUse.Length; i++)
                        {
                            if (submeshTrisToUse[i].data.Length != 0)
                            {
                                SerializableIntArray submesh = submeshTrisToUse[i];
                                for (int k = 0; k < submesh.data.Length; k++)
                                {
                                    newIndexes[indexStart + k] = (uint)submesh.data[k];
                                }

                                submeshIdx++;
                                indexStart += submesh.data.Length;
                            }
                        }

                        // Don't dispose index data because its owned by MeshData
                    }

                    data.subMeshCount = numNonZeroLengthSubmeshes;
                }

                {
                    int submeshIdx = 0;
                    int indexStart = 0;
                    for (int i = 0; i < submeshTrisToUse.Length; i++)
                    {
                        if (submeshTrisToUse[i].data.Length != 0)
                        {
                            SerializableIntArray submesh = submeshTrisToUse[i];
                            SubMeshDescriptor submeshInfo = new SubMeshDescriptor(indexStart, submesh.data.Length);
                            data.SetSubMesh(submeshIdx, submeshInfo);
                            submeshIdx++;
                            indexStart += submesh.data.Length;
                        }
                    }
                }
            }

            public void AssignTriangleDataForSubmeshes_ShowHide(Mesh mesh, List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, ref BufferDataFromPreviousBake serializableBufferData, out SerializableIntArray[] submeshTrisToUse, out int numNonZeroLengthSubmeshes)
            {
                submeshTrisToUse = GetSubmeshTrisWithShowHideApplied(mbDynamicObjectsInCombinedMesh);

                //submeshes with zero length tris cause error messages. must exclude these
                int numTotalIndexes;
                numNonZeroLengthSubmeshes = _NumNonZeroLengthSubmeshTris(submeshTrisToUse, out numTotalIndexes);

                {
                    IndexFormat indexFormat;
                    if (numTotalIndexes > System.UInt16.MaxValue)
                    {
                        indexFormat = IndexFormat.UInt32;
                    }
                    else
                    {
                        indexFormat = IndexFormat.UInt16;
                    }
                    
                    mesh.subMeshCount = 1; // if we don't do this we get warnings about submesh overlaps as we assign submeshes.

                    mesh.SetIndexBufferParams(numTotalIndexes, indexFormat);
                    if (indexFormat == IndexFormat.UInt16)
                    {
                        int submeshIdx = 0;
                        int indexStart = 0;
                        NativeArray<ushort> newIndexes = new NativeArray<ushort>(numTotalIndexes, Allocator.Temp);
                        for (int i = 0; i < submeshTrisToUse.Length; i++)
                        {
                            if (submeshTrisToUse[i].data.Length != 0)
                            {
                                SerializableIntArray submesh = submeshTrisToUse[i];
                                for (int k = 0; k < submesh.data.Length; k++)
                                {
                                    newIndexes[indexStart + k] = (ushort)submesh.data[k];
                                }

                                submeshIdx++;
                                indexStart += submesh.data.Length;
                            }
                        }

                        mesh.SetIndexBufferData(newIndexes, 0, 0, newIndexes.Length, MeshUpdateFlags.DontValidateIndices);
                        if (newIndexes.IsCreated) newIndexes.Dispose();
                    } else
                    {
                        int submeshIdx = 0;
                        int indexStart = 0;
                        NativeArray<uint> newIndexes = new NativeArray<uint>(numTotalIndexes, Allocator.Temp);
                        for (int i = 0; i < submeshTrisToUse.Length; i++)
                        {
                            if (submeshTrisToUse[i].data.Length != 0)
                            {
                                SerializableIntArray submesh = submeshTrisToUse[i];
                                for (int k = 0; k < submesh.data.Length; k++)
                                {
                                    newIndexes[indexStart + k] = (uint) submesh.data[k];
                                }

                                submeshIdx++;
                                indexStart += submesh.data.Length;
                            }
                        }

                        mesh.SetIndexBufferData(newIndexes, 0, 0, newIndexes.Length, MeshUpdateFlags.DontValidateIndices);
                        if (newIndexes.IsCreated) newIndexes.Dispose();
                    }
                    
                    mesh.subMeshCount = numNonZeroLengthSubmeshes;
                }

                {
                    int submeshIdx = 0;
                    int indexStart = 0;
                    for (int i = 0; i < submeshTrisToUse.Length; i++)
                    {
                        if (submeshTrisToUse[i].data.Length != 0)
                        {
                            SerializableIntArray submesh = submeshTrisToUse[i];
                            SubMeshDescriptor submeshInfo = new SubMeshDescriptor(indexStart, submesh.data.Length);
                            mesh.SetSubMesh(submeshIdx, submeshInfo);
                            submeshIdx++;
                            indexStart += submesh.data.Length;
                        }
                    }
                }
            }

            private void AdjustVertsToWriteAccordingToPivotPositionIfNecessary(MB_MeshPivotLocation pivotLocationType, MB_RenderType renderType, bool clearBuffersAfterBake, Vector3 pivotLocation_wld, out BufferDataFromPreviousBake serializableBufferData)
            {
                Debug.Assert(_isInitialized && !_disposed);
                Debug.Assert((channels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex);
                serializableBufferData.numVertsBaked = data.vertexCount;
                //verts2Write = verticies;
                if (verticies.Length > 0)
                {
                    if (renderType == MB_RenderType.skinnedMeshRenderer)
                    {
                        serializableBufferData.meshVerticesShift = Vector3.zero;
                        serializableBufferData.meshVerticiesWereShifted = false;
                    }
                    else if (pivotLocationType == MB_MeshPivotLocation.worldOrigin)
                    {
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

                        for (int i = 0; i < verticies.Length; i++)
                        {
                            verticies[i] = verticies[i] - center;
                        }

                        serializableBufferData.meshVerticesShift = center;
                        serializableBufferData.meshVerticiesWereShifted = true;
                    }
                    else
                    {
                        Debug.LogError("Unsupported Pivot Location Type: " + pivotLocationType);
                        serializableBufferData.meshVerticesShift = Vector3.zero;
                        serializableBufferData.meshVerticiesWereShifted = false;
                    }
                }
                else
                {
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

            private void _copyAndAdjustUVsFromMesh(MB2_TextureBakeResults tbr, MB_DynamicGameObject dgo, Mesh mesh, int uvChannel, int vertsIdx, NativeSlice<Vector2> uvsOut, NativeSlice<float> uvsSliceIdx, MeshChannelsCache_NativeArray meshChannelsCache, MB2_LogLevel LOG_LEVEL,
                MB2_TextureBakeResults textureBakeResults)
            {
                Debug.Assert(dgo.sourceSharedMaterials != null && dgo.sourceSharedMaterials.Length == dgo.targetSubmeshIdxs.Length,
                    "sourceSharedMaterials array was a different size than the targetSubmeshIdxs. Was this old data that is being updated? " + dgo.sourceSharedMaterials.Length);
                NativeArray<Vector2> nuvs = meshChannelsCache.GetUVChannelAsNativeArray(uvChannel, mesh);

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

            private void _CopyAndAdjustUV2FromMesh(MB_IMeshBakerSettings settings, MeshChannelsCache_NativeArray meshChannelsCache, MB_DynamicGameObject dgo, int vertsIdx, MB2_LogLevel LOG_LEVEL)
            {
                NativeArray<Vector2> dgoUV2s = meshChannelsCache.GetUVChannelAsNativeArray(2, dgo._mesh);

                if (settings.lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping)
                { //has a lightmap
                  // the lightmapTilingOffset is always 1,1,0,0 for all objects
                  //lightMap index is always 1
                    if (dgoUV2s == null || dgoUV2s.Length == 0)
                    {
                        NativeArray<Vector2> dgoUVs = meshChannelsCache.GetUVChannelAsNativeArray(0, dgo._mesh);
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
                            dgoUV2s = meshChannelsCache.GetUv2ModifiedAsNativeArray(dgo._mesh);
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
                        dgoUV2s = meshChannelsCache.GetUv2ModifiedAsNativeArray(dgo._mesh);
                    }
                    MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyTo(dgoUV2s, uv2s, vertsIdx);
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
                                NativeArray<Vector3> dgoMeshVerts, NativeArray<Vector3> dgoMeshNorms, NativeArray<Vector4> dgoMeshTans,
                                NativeSlice<Vector3> verticies, NativeSlice<Vector3> normals, NativeSlice<Vector4> tangents)
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
                                NativeSlice<Vector3> dgoMeshVerts, NativeSlice<Vector3> dgoMeshNorms, NativeSlice<Vector4> dgoMeshTans,
                                NativeSlice<Vector3> verticies, NativeSlice<Vector3> normals, NativeSlice<Vector4> tangents)
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
                                NativeSlice<Vector3> dgoMeshVerts_local, NativeSlice<Vector3> dgoMeshNorms_local, NativeSlice<Vector4> dgoMeshTans_local,
                                NativeSlice<Vector3> verticies, NativeSlice<Vector3> normals, NativeSlice<Vector4> tangents)
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
                                NativeSlice<Vector3> dgoMeshVerts_local, NativeSlice<Vector3> dgoMeshNorms_local, NativeSlice<Vector4> dgoMeshTans_local,
                                NativeSlice<Vector3> verticies, NativeSlice<Vector3> normals, NativeSlice<Vector4> tangents)
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
#endif
    }
}
