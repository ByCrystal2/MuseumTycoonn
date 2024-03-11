using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Rendering;

namespace DigitalOpus.MB.Core
{
    /// <summary>
    /// Manages a single combined mesh.This class is the core of the mesh combining API.
    /// 
    /// It is not a component so it can be can be instantiated and used like a normal c sharp class.
    /// </summary>
    public partial class MB3_MeshCombinerSingle : MB3_MeshCombiner
    {
#if UNITY_2020_2_OR_NEWER
        /// <summary>
        /// All getter methods in MeshChannelsCacheNew assume that CollectChannelDataForAllMeshesInList has first been
        /// called and the data is already present and accounted for.
        /// 
        /// Originally I wrote a version of this that uses  Mesh.AcquireReadOnlyMeshData( allMeshes ) to get the mesh data for all meshes at once.
        /// Unfortunatly it was much slower than calling mesh.verticies, mesh.normals etc...
        /// </summary>
        public class MeshChannelsCache_NativeArray : IDisposable, IMeshChannelsCacheTaggingInterface
        {
            
            MB2_LogLevel LOG_LEVEL;
            MB2_LightmapOptions lightmapOption;
            protected Dictionary<int, MeshChannelsNativeArray> meshID2MeshChannels = new Dictionary<int, MeshChannelsNativeArray>();

            private bool _collectedMeshData;
            private bool _disposed;

            public MeshChannelsCache_NativeArray(MB2_LogLevel ll, MB2_LightmapOptions lo)
            {
                LOG_LEVEL = ll;
                lightmapOption = lo;
            }

            public void Dispose()
            {
                Dispose(true);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (_disposed) return;
                foreach (MeshChannelsNativeArray mc in meshID2MeshChannels.Values)
                {
                    mc.Dispose();
                }

                _collectedMeshData = false;
                _disposed = true;
            }

            public bool HasCollectedMeshData()
            {
                return _collectedMeshData;
            }

            public bool hasOutOfBoundsUVs(Mesh m, ref MB_Utility.MeshAnalysisResult mar, int submeshIdx)
            {
                NativeArray<Vector2> uvss = GetUv0RawAsNativeArray(m);
                return MB_Utility.hasOutOfBoundsUVs(uvss, m, ref mar, submeshIdx);
            }

            internal NativeArray<Vector3> GetVerticiesAsNativeArray(Mesh m)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                MeshChannelsNativeArray mc;
                if (!meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc))
                {
                    Debug.LogError("Could not find mesh in the MeshChannelsCache." + m);
                }

                return mc.vertcies_NativeArray;
            }

            internal NativeArray<Vector3> GetNormalsAsNativeArray(Mesh m)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                MeshChannelsNativeArray mc;
                if (!meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc))
                {
                    Debug.LogError("Could not find mesh in the MeshChannelsCache." + m);
                }

                return mc.normals_NativeArray;
            }

            internal NativeArray<Vector4> GetTangentsAsNativeArray(Mesh m)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                MeshChannelsNativeArray mc;
                if (!meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc))
                {
                    Debug.LogError("Could not find mesh in the MeshChannelsCache." + m);
                }

                return mc.tangents_NativeArray;
            }

            internal NativeArray<Vector2> GetUv0RawAsNativeArray(Mesh m)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                MeshChannelsNativeArray mc;
                meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc);
                return mc.uv0raw_NativeArray;
            }

            internal NativeArray<Vector2> GetUv0ModifiedAsNativeArray(Mesh m)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                MeshChannelsNativeArray mc;
                meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc);
                if (!mc.uv0modified_NativeArray.IsCreated)
                {
                    mc.uv0modified_NativeArray = new NativeArray<Vector2>(mc.vertcies_NativeArray.Length, Allocator.Temp);
                }

                return mc.uv0modified_NativeArray;
            }

            internal NativeArray<Vector2> GetUv2ModifiedAsNativeArray(Mesh m)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                MeshChannelsNativeArray mc;
                meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc);
                if (!mc.uv2modified_NativeArray.IsCreated)
                {
                    mc.uv2modified_NativeArray = new NativeArray<Vector2>(mc.vertcies_NativeArray.Length, Allocator.Temp);
                }

                return mc.uv2modified_NativeArray;
            }

            internal NativeArray<Vector2> GetUVChannelAsNativeArray(int channel, Mesh m)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                MeshChannelsNativeArray mc;
                meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc);

                switch (channel)
                {
                    case 0:
                        return mc.uv0raw_NativeArray;
                    case 2:
                        return mc.uv2raw_NativeArray;
                    case 3:
                        return mc.uv3_NativeArray;
                    case 4:
                        return mc.uv4_NativeArray;
                    case 5:
                        return mc.uv5_NativeArray;
                    case 6:
                        return mc.uv6_NativeArray;
                    case 7:
                        return mc.uv7_NativeArray;
                    case 8:
                        return mc.uv8_NativeArray;
                    default:
                        Debug.LogError("Error mesh channel " + channel + " not supported");
                        break;
                }

                return new NativeArray<Vector2>();
            }

            internal NativeArray<Color> GetColorsAsNativeArray(Mesh m)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                MeshChannelsNativeArray mc;
                meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc);
                return mc.colors_NativeArray;
            }

            public void CollectChannelDataForAllMeshesInList(List<MB_DynamicGameObject> toUpdateDGOs, List<MB_DynamicGameObject> toAddDGOs,
                MB_MeshVertexChannelFlags newChannels,
                MB_RenderType renderType,
                bool doBlendShapes)
            {
                bool doVerts = ((newChannels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex);
                bool doNormals = ((newChannels & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal);
                bool doTangents = ((newChannels & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent);
                bool doUV0 = ((newChannels & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0);
                bool doUV2 = ((newChannels & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2);
                bool doUV3 = ((newChannels & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3);
                bool doUV4 = ((newChannels & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4);
                bool doUV5 = ((newChannels & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5);
                bool doUV6 = ((newChannels & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6);
                bool doUV7 = ((newChannels & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7);
                bool doUV8 = ((newChannels & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8);
                bool doColors = ((newChannels & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors);

                List<MB_DynamicGameObject> toUpdateAndAddDGOs = new List<MB_DynamicGameObject>();
                toUpdateAndAddDGOs.AddRange(toUpdateDGOs);
                toUpdateAndAddDGOs.AddRange(toAddDGOs);

                for (int i = 0; i < toUpdateAndAddDGOs.Count; i++)
                {
                    MB_DynamicGameObject dgo = toUpdateAndAddDGOs[i];
                    Mesh m = dgo._mesh;
                    if (!meshID2MeshChannels.ContainsKey(m.GetInstanceID()))
                    {
                        MeshChannelsNativeArray mc = new MeshChannelsNativeArray();
                        meshID2MeshChannels.Add(m.GetInstanceID(), mc);
                        {
                            if (doVerts)  mc.vertcies_NativeArray = new NativeArray<Vector3>(m.vertices, Allocator.Temp);

                            if (doUV0) mc.uv0raw_NativeArray = new NativeArray<Vector2>(_getMeshUVs(m), Allocator.Temp); // I think this looks good now. Check with Ian
                            if (doUV2) mc.uv2raw_NativeArray = new NativeArray<Vector2>(_getMeshUV2s(m, ref mc.uv2modified_NativeArray), Allocator.Temp); // I think this looks good now. Check with Ian.
                            
                            if (doNormals) mc.normals_NativeArray = new NativeArray<Vector3>(_getMeshNormals(m), Allocator.Temp);
                            if (doTangents) mc.tangents_NativeArray = new NativeArray<Vector4>(_getMeshTangents(m), Allocator.Temp);
                            if (doUV3) mc.uv3_NativeArray = new NativeArray<Vector2>(MBVersion.GetMeshChannel(3, m, LOG_LEVEL), Allocator.Temp);
                            if (doUV4) mc.uv4_NativeArray = new NativeArray<Vector2>(MBVersion.GetMeshChannel(4, m, LOG_LEVEL), Allocator.Temp);
                            if (doUV5) mc.uv5_NativeArray = new NativeArray<Vector2>(MBVersion.GetMeshChannel(5, m, LOG_LEVEL), Allocator.Temp);
                            if (doUV6) mc.uv6_NativeArray = new NativeArray<Vector2>(MBVersion.GetMeshChannel(6, m, LOG_LEVEL), Allocator.Temp);
                            if (doUV7) mc.uv7_NativeArray = new NativeArray<Vector2>(MBVersion.GetMeshChannel(7, m, LOG_LEVEL), Allocator.Temp);
                            if (doUV8) mc.uv8_NativeArray = new NativeArray<Vector2>(MBVersion.GetMeshChannel(8, m, LOG_LEVEL), Allocator.Temp);
                            if (doColors) mc.colors_NativeArray = new NativeArray<Color>(_getMeshColors(m), Allocator.Temp);

                            {
                                if (renderType == MB_RenderType.skinnedMeshRenderer)
                                {
                                    bool isSkinnedMeshWithBones = false;
                                    Renderer r = dgo._renderer;
                                    if (mc.bindPoses == null || mc.bindPoses.Count == 0)
                                    {
                                        _getBindPoses(r, mc.bindPoses, out isSkinnedMeshWithBones);

#if UNITY_2020_2_OR_NEWER
                                        // double check with Ian that mc.bindPoses.Length is okay to pass into this method
                                        _getBoneWeightData(ref mc.boneWeightData, r, mc.bindPoses.Count, isSkinnedMeshWithBones);
#else
                                        // double check with Ian that _getBoneWeights should only be called if the render type is a skinnedMeshRenderer
                                        mc.boneWeights = _getBoneWeights(r, m.vertexCount, isSkinnedMeshWithBones);
#endif
                                    }

                                    if (doBlendShapes)
                                    {
                                        MBBlendShape[] shapes = new MBBlendShape[m.blendShapeCount];
                                        int arrayLen = m.vertexCount;
                                        for (int shapeIdx = 0; shapeIdx < shapes.Length; shapeIdx++)
                                        {
                                            MBBlendShape shape = shapes[shapeIdx] = new MBBlendShape();
                                            shape.frames =
                                                new MBBlendShapeFrame[MBVersion.GetBlendShapeFrameCount(m, shapeIdx)];
                                            shape.name = m.GetBlendShapeName(shapeIdx);
                                            shape.indexInSource = shapeIdx;
                                            shape.gameObjectID = dgo.instanceID;
                                            shape.gameObject = dgo.gameObject;
                                            for (int frameIdx = 0; frameIdx < shape.frames.Length; frameIdx++)
                                            {
                                                MBBlendShapeFrame frame = shape.frames[frameIdx] = new MBBlendShapeFrame();
                                                frame.frameWeight =
                                                    MBVersion.GetBlendShapeFrameWeight(m, shapeIdx, frameIdx);
                                                frame.vertices = new Vector3[arrayLen];
                                                frame.normals = new Vector3[arrayLen];
                                                frame.tangents = new Vector3[arrayLen];
                                                MBVersion.GetBlendShapeFrameVertices(m, shapeIdx, frameIdx, frame.vertices,
                                                    frame.normals, frame.tangents);
                                            }
                                        }

                                        mc.blendShapes = shapes;
                                    }
                                }
                            }
                        }
                        
                    }
                }

                _collectedMeshData = true;
            }

            internal List<Matrix4x4> GetBindposes(Renderer r, out bool isSkinnedMeshWithBones)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                Mesh m = MB_Utility.GetMesh(r.gameObject);
                MeshChannelsNativeArray mc;
                meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc);
                {
                    if (r is SkinnedMeshRenderer &&
                        mc.bindPoses.Count > 0)
                    {
                        isSkinnedMeshWithBones = true;
                    }
                    else
                    {
                        isSkinnedMeshWithBones = false;
                        if (r is SkinnedMeshRenderer) Debug.Assert(m.blendShapeCount > 0, "Skinned Mesh Renderer " + r + " had no bones and no blend shapes");
                    }
                }
                return mc.bindPoses;
            }

            // two arguments in this method are unnecessary, so they will get refactored out when MeshChannelsCache gets replaced by MeshChannelsCacheNew
            internal BoneWeightDataForMesh GetBoneWeightData(Renderer r, int numbones, bool isSkinnedMeshWithBones)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                Mesh m = MB_Utility.GetMesh(r.gameObject);
                MeshChannelsNativeArray mc;
                meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc);
                return mc.boneWeightData;
            }


            public MBBlendShape[] GetBlendShapes(Mesh m, int gameObjectID, GameObject gameObject)
            {
                // Make sure I do the copying part of the setter like it is done in the old GetBlendShapes method
                // We are sure it is there for a reason.
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                MeshChannelsNativeArray mc;
                meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc);
                MBBlendShape[] shapes = new MBBlendShape[mc.blendShapes.Length];
                for (int i = 0; i < shapes.Length; i++)
                {
                    shapes[i] = new MBBlendShape();
                    shapes[i].name = mc.blendShapes[i].name;
                    shapes[i].indexInSource = mc.blendShapes[i].indexInSource;
                    shapes[i].frames = mc.blendShapes[i].frames;
                    shapes[i].gameObjectID = gameObjectID;
                    shapes[i].gameObject = gameObject;
                }
                return shapes;
            }

            private Color[] _getMeshColors(Mesh m)
            {
                Color[] cs = m.colors;
                if (cs.Length == 0)
                {
                    if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Mesh " + m + " has no colors. Generating");
                    if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Mesh " + m + " didn't have colors. Generating an array of white colors");
                    cs = new Color[m.vertexCount];
                    for (int i = 0; i < cs.Length; i++) { cs[i] = Color.white; }
                }
                return cs;
            }

            private Vector3[] _getMeshNormals(Mesh m)
            {
                Vector3[] ns = m.normals;
                if (ns.Length == 0)
                {
                    if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Mesh " + m + " has no normals. Generating");
                    if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Mesh " + m + " didn't have normals. Generating normals.");
                    Mesh tempMesh = (Mesh)GameObject.Instantiate(m);
                    tempMesh.RecalculateNormals();
                    ns = tempMesh.normals;
                    MB_Utility.Destroy(tempMesh);
                }
                return ns;
            }

            private Vector4[] _getMeshTangents(Mesh m)
            {
                Vector4[] ts = m.tangents;
                if (ts.Length == 0)
                {
                    if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Mesh " + m + " has no tangents. Generating");
                    if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Mesh " + m + " didn't have tangents. Generating tangents.");
                    Vector3[] verts = m.vertices;
                    NativeArray<Vector2> uvs = GetUv0Raw(m);
                    Vector3[] norms = _getMeshNormals(m);
                    ts = new Vector4[m.vertexCount];
                    for (int i = 0; i < m.subMeshCount; i++)
                    {
                        int[] tris = m.GetTriangles(i);
                        _generateTangents(tris, verts, uvs, norms, ts);
                    }
                }
                return ts;
            }

            private Vector2 _HALF_UV = new Vector2(.5f, .5f);
            private Vector2[] _getMeshUVs(Mesh m)
            {
                Vector2[] uv = m.uv;
                if (uv.Length == 0)
                {

                    uv = new Vector2[m.vertexCount];
                    for (int i = 0; i < uv.Length; i++) { uv[i] = _HALF_UV; }
                }
                return uv;
            }

            //has a second parameter, return two arrays 
            private Vector2[] _getMeshUV2s(Mesh m, ref NativeArray<Vector2> uv2modified)
            {
                Vector2[] uv = m.uv2;

                if (uv.Length == 0)
                {
                    uv2modified = new NativeArray<Vector2>(m.vertexCount, Allocator.TempJob);
                    for (int i = 0; i < uv2modified.Length; i++) { uv2modified[i] = _HALF_UV; }
                }
                return uv;
            }

            private static void _getBindPoses(Renderer r, List<Matrix4x4> poses, out bool isSkinnedMeshWithBones)
            {
                poses.Clear();
                isSkinnedMeshWithBones = r is SkinnedMeshRenderer;
                if (r is SkinnedMeshRenderer)
                {
                    Mesh m = MB_Utility.GetMesh(r.gameObject);
                    m.GetBindposes(poses);
                    if (poses.Count == 0)
                    {
                        if (m.blendShapeCount > 0)
                        {
                            isSkinnedMeshWithBones = false;
                        }
                        else
                        {
                            Debug.LogError("Skinned mesh " + r + " had no bindposes AND no blend shapes");
                        }
                    }
                }

                if (r is MeshRenderer ||
                   (r is SkinnedMeshRenderer && !isSkinnedMeshWithBones)) // It is possible for a skinned mesh to have blend shapes but no bones. These need to be treated like MeshRenderer meshes.
                {
                    poses.Clear();
                    poses.Add(Matrix4x4.identity);
                }

                if (poses == null ||
                    poses.Count == 0)
                {
                    Debug.LogError("Could not _getBindPoses. Object does not have a renderer");
                }
            }

            private static void _getBoneWeightData(ref BoneWeightDataForMesh bwd, Renderer r, int numBones, bool isSkinnedMeshWithBones)
            {
#if UNITY_2020_2_OR_NEWER
                Debug.Assert(!bwd.bonesPerVertex.IsCreated, "Should only create native arrays once.");
                Debug.Assert(!bwd.boneWeights.IsCreated, "Should only create native arrays once.");
                if (isSkinnedMeshWithBones)
                {
                    Mesh m = ((SkinnedMeshRenderer)r).sharedMesh;
                    bwd.initialized = true;
                    bwd.weMustDispose = false;
                    bwd.bonesPerVertex = m.GetBonesPerVertex();
                    bwd.boneWeights = m.GetAllBoneWeights();
                }
                else if (r is MeshRenderer ||
                    (r is SkinnedMeshRenderer && !isSkinnedMeshWithBones)) // It is possible for a skinned mesh to have blend shapes but no bones. These need to be treated like MeshRenderer meshes
                {
                    Mesh m = MB_Utility.GetMesh(r.gameObject);
                    bwd.initialized = true;
                    bwd.weMustDispose = true;
                    bwd.boneWeights = new NativeArray<BoneWeight1>(m.vertexCount, Allocator.Temp);
                    bwd.bonesPerVertex = new NativeArray<byte>(m.vertexCount, Allocator.Temp);
                    BoneWeight1 bw = new BoneWeight1();
                    bw.boneIndex = 0;
                    bw.weight = 1f;
                    for (int i = 0; i < m.vertexCount; i++)
                    {
                        bwd.bonesPerVertex[i] = 1;
                        bwd.boneWeights[i] = bw;
                    }
                }
                else
                {
                    Debug.LogError("Could not _getBoneWeights. Object does not have a renderer");
                }

                bwd.UsedBoneIdxsInSrcMesh = new bool[numBones];
                for (int i = 0; i < bwd.boneWeights.Length; i++)
                {
                    bwd.UsedBoneIdxsInSrcMesh[bwd.boneWeights[i].boneIndex] = true;
                }

                bwd.numUsedbones = 0;
                for (int i = 0; i < bwd.UsedBoneIdxsInSrcMesh.Length; i++)
                {
                    if (bwd.UsedBoneIdxsInSrcMesh[i]) bwd.numUsedbones++;
                }
#endif
            }

            internal NativeArray<Vector2> GetUv0Raw(Mesh m)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                MeshChannelsNativeArray mc;
                meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc);
                return mc.uv0raw_NativeArray;
            }

            /*
            private static void _getBoneWeightData(ref BoneWeightDataForMesh bwd, Renderer r, int numBones, bool isSkinnedMeshWithBones)
            {
#if UNITY_2020_2_OR_NEWER
                Debug.Assert(!bwd.bonesPerVertex.IsCreated, "Should only create native arrays once.");
                Debug.Assert(!bwd.boneWeights.IsCreated, "Should only create native arrays once.");

                Debug.Log("REMOVE  Getting bone weight data.");

                if (isSkinnedMeshWithBones)
                {
                    Mesh m = ((SkinnedMeshRenderer)r).sharedMesh;
                    bwd.initialized = true;
                    bwd.weMustDispose = false;
                    bwd.bonesPerVertex = m.GetBonesPerVertex();
                    bwd.boneWeights = m.GetAllBoneWeights();
                }
                else if (r is MeshRenderer ||
                    (r is SkinnedMeshRenderer && !isSkinnedMeshWithBones)) // It is possible for a skinned mesh to have blend shapes but no bones. These need to be treated like MeshRenderer meshes
                {
                    Mesh m = MB_Utility.GetMesh(r.gameObject);
                    bwd.initialized = true;
                    bwd.weMustDispose = true;
                    bwd.boneWeights = new NativeArray<BoneWeight1>(m.vertexCount, Allocator.Temp);
                    bwd.bonesPerVertex = new NativeArray<byte>(m.vertexCount, Allocator.Temp);
                    BoneWeight1 bw = new BoneWeight1();
                    bw.boneIndex = 0;
                    bw.weight = 1f;
                    for (int i = 0; i < m.vertexCount; i++)
                    {
                        bwd.bonesPerVertex[i] = 1;
                        bwd.boneWeights[i] = bw;
                    }
                }
                else
                {
                    Debug.LogError("Could not _getBoneWeights. Object does not have a renderer");
                }

                bwd.UsedBoneIdxsInSrcMesh = new bool[numBones];
                for (int i = 0; i < bwd.boneWeights.Length; i++)
                {
                    bwd.UsedBoneIdxsInSrcMesh[bwd.boneWeights[i].boneIndex] = true;
                }

                bwd.numUsedbones = 0;
                for (int i = 0; i < bwd.UsedBoneIdxsInSrcMesh.Length; i++)
                {
                    if (bwd.UsedBoneIdxsInSrcMesh[i]) bwd.numUsedbones++;
                }
#endif
            }
            */

            private static BoneWeight[] _getBoneWeights(Renderer r, int numVertsInMeshBeingAdded, bool isSkinnedMeshWithBones)
            {
                if (isSkinnedMeshWithBones)
                {
                    return ((SkinnedMeshRenderer)r).sharedMesh.boneWeights;
                }
                else if (r is MeshRenderer ||
                    (r is SkinnedMeshRenderer && !isSkinnedMeshWithBones)) // It is possible for a skinned mesh to have blend shapes but no bones. These need to be treated like MeshRenderer meshes
                {
                    BoneWeight bw = new BoneWeight();
                    bw.boneIndex0 = bw.boneIndex1 = bw.boneIndex2 = bw.boneIndex3 = 0;
                    bw.weight0 = 1f;
                    bw.weight1 = bw.weight2 = bw.weight3 = 0f;
                    BoneWeight[] bws = new BoneWeight[numVertsInMeshBeingAdded];
                    for (int i = 0; i < bws.Length; i++) bws[i] = bw;
                    return bws;
                }
                else
                {
                    Debug.LogError("Could not _getBoneWeights. Object does not have a renderer");
                    return null;
                }
            }

            private void _generateTangents(int[] triangles, Vector3[] verts, NativeArray<Vector2> uvs, Vector3[] normals, Vector4[] outTangents)
            {
                int triangleCount = triangles.Length;
                int vertexCount = verts.Length;

                Vector3[] tan1 = new Vector3[vertexCount];
                Vector3[] tan2 = new Vector3[vertexCount];

                for (int a = 0; a < triangleCount; a += 3)
                {
                    int i1 = triangles[a + 0];
                    int i2 = triangles[a + 1];
                    int i3 = triangles[a + 2];

                    Vector3 v1 = verts[i1];
                    Vector3 v2 = verts[i2];
                    Vector3 v3 = verts[i3];

                    Vector2 w1 = uvs[i1];
                    Vector2 w2 = uvs[i2];
                    Vector2 w3 = uvs[i3];

                    float x1 = v2.x - v1.x;
                    float x2 = v3.x - v1.x;
                    float y1 = v2.y - v1.y;
                    float y2 = v3.y - v1.y;
                    float z1 = v2.z - v1.z;
                    float z2 = v3.z - v1.z;

                    float s1 = w2.x - w1.x;
                    float s2 = w3.x - w1.x;
                    float t1 = w2.y - w1.y;
                    float t2 = w3.y - w1.y;

                    float rBot = (s1 * t2 - s2 * t1);
                    if (rBot == 0f)
                    {
                        Debug.LogError("Could not compute tangents. All UVs need to form a valid triangles in UV space. If any UV triangles are collapsed, tangents cannot be generated.");
                        return;
                    }
                    float r = 1.0f / rBot;

                    Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                    Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                    tan1[i1] += sdir;
                    tan1[i2] += sdir;
                    tan1[i3] += sdir;

                    tan2[i1] += tdir;
                    tan2[i2] += tdir;
                    tan2[i3] += tdir;
                }


                for (int a = 0; a < vertexCount; ++a)
                {
                    Vector3 n = normals[a];
                    Vector3 t = tan1[a];

                    Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
                    outTangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
                    outTangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
                }
            }
        }

        //if baking many instances of the same sharedMesh, want to cache these results rather than grab them multiple times from the mesh 
        public class MeshChannelsNativeArray : IDisposable
        {
            private bool _disposed = false;

            public NativeArray<Vector3> vertcies_NativeArray;
            public NativeArray<Vector3> normals_NativeArray;
            public NativeArray<Vector4> tangents_NativeArray;
            public NativeArray<Color> colors_NativeArray;
            public NativeArray<Vector2> uv0raw_NativeArray;
            public NativeArray<Vector2> uv0modified_NativeArray;
            public NativeArray<Vector2> uv2raw_NativeArray;
            public NativeArray<Vector2> uv2modified_NativeArray;
            public NativeArray<Vector2> uv3_NativeArray;
            public NativeArray<Vector2> uv4_NativeArray;
            public NativeArray<Vector2> uv5_NativeArray;
            public NativeArray<Vector2> uv6_NativeArray;
            public NativeArray<Vector2> uv7_NativeArray;
            public NativeArray<Vector2> uv8_NativeArray;

            /// <summary>
            /// In unity 2021 we can only get bindposes using a List.
            /// In unity 2022 we can use a NativeArray.
            /// </summary>
            public List<Matrix4x4> bindPoses = new List<Matrix4x4>(128);
            public BoneWeightDataForMesh boneWeightData;

            public MBBlendShape[] blendShapes;

            public void Dispose()
            {
                Dispose(true);
            }

            public bool IsDisposed()
            {
                return _disposed;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (_disposed) return;
                _disposed = true;
                boneWeightData.Dispose();

                if (vertcies_NativeArray.IsCreated) { vertcies_NativeArray.Dispose(); }
                if (normals_NativeArray.IsCreated) { normals_NativeArray.Dispose(); }
                if (tangents_NativeArray.IsCreated) { tangents_NativeArray.Dispose(); }
                if (colors_NativeArray.IsCreated) { colors_NativeArray.Dispose(); }

                if (uv0raw_NativeArray.IsCreated) { uv0raw_NativeArray.Dispose(); }
                if (uv0modified_NativeArray.IsCreated) { uv0modified_NativeArray.Dispose(); }

                if (uv2raw_NativeArray.IsCreated) { uv2raw_NativeArray.Dispose(); }
                if (uv2modified_NativeArray.IsCreated) { uv2modified_NativeArray.Dispose(); }

                if (uv3_NativeArray.IsCreated) { uv3_NativeArray.Dispose(); }
                if (uv4_NativeArray.IsCreated) { uv4_NativeArray.Dispose(); }
                if (uv5_NativeArray.IsCreated) { uv5_NativeArray.Dispose(); }
                if (uv6_NativeArray.IsCreated) { uv6_NativeArray.Dispose(); }
                if (uv7_NativeArray.IsCreated) { uv7_NativeArray.Dispose(); }
                if (uv8_NativeArray.IsCreated) { uv8_NativeArray.Dispose(); }
            }
        }
#endif 
    }
}