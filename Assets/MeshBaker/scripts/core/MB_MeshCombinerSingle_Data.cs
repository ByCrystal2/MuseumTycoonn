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

        public enum MeshCreationConditions
        {
            NoMesh,
            CreatedInEditor,
            CreatedAtRuntime,
            AssignedByUser,
        }

        [System.Serializable]
        public struct BufferDataFromPreviousBake
        {
            public int numVertsBaked;
            public Vector3 meshVerticesShift;
            public bool meshVerticiesWereShifted;
        }

        //2D arrays are not serializable but arrays  of arrays are.
        [System.Serializable]
        public class SerializableIntArray
        {
            [SerializeField]
            public int[] data;

            public SerializableIntArray() 
            {
                data = new int[0];
            }

            public SerializableIntArray(int len)
            {
                data = new int[len];
            }
        }

        public struct BoneWeightDataForMesh
        {
            private bool _disposed;
            public bool initialized;

            /// <summary>
            /// We might be getting the NativeArrays from the Mesh using mesh.GetAllBoneWeights & mesh.GetBonesPerVertex. We don't dispose these.
            /// We might be allocating the native arrays using:   new NativeArrays(..., Allocator.XX).   We are responsible for disposing these.
            /// </summary>
            public bool weMustDispose;
#if UNITY_2020_2_OR_NEWER
            public NativeArray<byte> bonesPerVertex;
            public NativeArray<BoneWeight1> boneWeights;
#endif
            public bool[] UsedBoneIdxsInSrcMesh;
            public int numUsedbones;

            internal void Dispose()
            {
                Dispose(true);
            }

            private void Dispose(bool disposing)
            {
                if (_disposed) return;
                _disposed = true;
                initialized = false;
#if UNITY_2020_2_OR_NEWER
                if (bonesPerVertex.IsCreated && weMustDispose) bonesPerVertex.Dispose();
                if (boneWeights.IsCreated && weMustDispose) boneWeights.Dispose();
#endif
            }
        }

        /*
		 Stores information about one source game object that has been added to
		 the combined mesh.  
		*/
        [System.Serializable]
        public class MB_DynamicGameObject : IComparable<MB_DynamicGameObject>
        {
            public int instanceID;
            public GameObject gameObject;
            public string name;
            public int vertIdx;
            public int blendShapeIdx;
            public int numVerts;
            public int numBlendShapes;
            public int numBoneWeights;

            public bool isSkinnedMeshWithBones = false; // it is possible for a skinned mesh to have blend shapes but no bones.

            //distinct list of bones in the bones array
            public int[] indexesOfBonesUsed = new int[0];

            //public Transform[] _originalBones;    //used only for integrity checking
            //public Matrix4x4[] _originalBindPoses; //used only for integrity checking

            public int lightmapIndex = -1;
            public Vector4 lightmapTilingOffset = new Vector4(1f, 1f, 0f, 0f);

            public Vector3 meshSize = Vector3.one; // in world coordinates

            public bool show = true;

            public bool invertTriangles = false;

            /// <summary>
            /// combined mesh will have one submesh per result material
            /// source meshes can have any number of submeshes.They are mapped to a result submesh based on their material
            /// if two different submeshes have the same material they are merged in the same result submesh
            /// </summary>
            // These are result mesh submeshCount comine these into a class.
            public int[] submeshTriIdxs;
            public int[] submeshNumTris;

            /// <summary>
            /// These are source go mesh submeshCount todo combined these into a class.
            /// Maps each submesh in source mesh to a submesh in combined mesh.
            /// </summary>
            public int[] targetSubmeshIdxs;

            /// <summary>
            /// The UVRects in the combinedMaterial atlas.
            /// </summary>
            public Rect[] uvRects;

            /// <summary>
            /// If AllPropsUseSameMatTiling is the rect that was used for sampling the atlas texture from the source texture including both mesh uvTiling and material tiling.
            /// else is the source mesh obUVrect. We don't need to care which.
            /// </summary>
            public Rect[] encapsulatingRect;

            /// <summary>
            /// If AllPropsUseSameMatTiling is the source texture material tiling.
            /// else is 0,0,1,1.  We don't need to care which.
            /// </summary>
            public Rect[] sourceMaterialTiling;

            /// <summary>
            /// The obUVRect for each source mesh submesh;
            /// </summary>
            public Rect[] obUVRects;

            /// <summary>
            /// The index of the texture array slice.
            /// </summary>
            public int[] textureArraySliceIdx;

            public Material[] sourceSharedMaterials;

            // ---------------------- NonSerialized data used internally
            [NonSerialized]
            internal bool _initialized = false;

            [NonSerialized]
            internal bool _beingDeleted = false;

            [NonSerialized]
            internal Mesh _mesh;

            [NonSerialized]
            internal Renderer _renderer;

            // temporary buffers used within a single bake. Not cached between bakes
            // used so we don't have to call GetBones and GetBindposes multiple Times
            [NonSerialized]
            internal SerializableIntArray[] _tmpSubmeshTris;

            // Temporary buffers for Skinned Mesh Renderer baking =========
            [NonSerialized]
            internal Transform[] _tmpSMR_CachedBones;  // 1 per bone in source mesh
            [NonSerialized]
            internal List<Matrix4x4> _tmpSMR_CachedBindposes;  // 1 per bone in source mesh

            [NonSerialized]
            internal BoneAndBindpose[] _tmpSMR_CachedBoneAndBindPose; // 1 per bone in source mesh

            [NonSerialized]
            internal int[] _tmpSMR_srcMeshBoneIdx2masterListBoneIdx; // 1 per bone in source mesh

            [NonSerialized]
            internal BoneWeight[] _tmpSMR_CachedBoneWeights; // used by old API  mesh.boneWeights 

            [NonSerialized]
            internal BoneWeightDataForMesh _tmpSMR_CachedBoneWeightData; // used by new API  mesh.GetAllBoneWeights

            public bool Initialize(bool beingDeleted)
            {
                _initialized = true;
                _beingDeleted = beingDeleted;
                if (!beingDeleted)
                {
                    _mesh = MB_Utility.GetMesh(gameObject);
                    _renderer = MB_Utility.GetRenderer(gameObject);
                    return _mesh != null && _renderer != null;
                } else
                {
                    return true;
                }
            }

            public bool InitializeNew(bool beingDeleted, GameObject go)
            {
                Debug.Assert(go != null);
                Debug.Assert(name == null, "Should only call InitializeNew on a newly created DGO.");
                gameObject = go;
                name = String.Format("{0} {1}", gameObject.ToString(), gameObject.GetInstanceID());
                if (go == null) return false;
                instanceID = gameObject.GetInstanceID();
                return Initialize(beingDeleted);
            }

            public void UnInitialize()
            {
                _initialized = false;
                _beingDeleted = false;
                _mesh = null;
                _renderer = null;
            }

            public int CompareTo(MB_DynamicGameObject b)
            {
                return this.vertIdx - b.vertIdx;
            }
        }

        
        //if baking many instances of the same sharedMesh, want to cache these results rather than grab them multiple times from the mesh 
        public class MeshChannels : IDisposable
        {
            private bool _disposed = false;
            public Vector3[] vertices;
            public Vector3[] normals;
            public Vector4[] tangents;
            public Vector2[] uv0raw;
            public Vector2[] uv0modified;
            public Vector2[] uv2raw;
            public Vector2[] uv2modified;
            public Vector2[] uv3;
            public Vector2[] uv4;
            public Vector2[] uv5;
            public Vector2[] uv6;
            public Vector2[] uv7;
            public Vector2[] uv8;
            public Color[] colors;
            public BoneWeight[] boneWeights;
            public List<Matrix4x4> bindPoses = new List<Matrix4x4>(128);
            public int[] triangles;
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
                vertices = null;
                normals = null;
                tangents = null;
                uv0raw = null;
                uv0modified = null;
                uv2raw = null;
                uv2modified = null;
                uv3 = null;
                uv4 = null;
                uv5 = null;
                uv6 = null;
                uv7 = null;
                uv8 = null;
                colors = null;
                boneWeights = null;
                bindPoses = null;
                triangles = null;
                blendShapes = null;
            }
        }

        [Serializable]
        public class MBBlendShapeFrame
        {
            public float frameWeight;
            public Vector3[] vertices;
            public Vector3[] normals;
            public Vector3[] tangents;
        }

        [Serializable]
        public class MBBlendShape
        {
            public int gameObjectID;
            public GameObject gameObject;
            public string name;
            public int indexInSource;
            public MBBlendShapeFrame[] frames;
        }

        //Used for comparing if skinned meshes use the same bone and bindpose.
        //Skinned meshes must be bound with the same TRS to share a bone.
        public struct BoneAndBindpose
        {
            public Transform bone;
            public Matrix4x4 bindPose;

            public BoneAndBindpose(Transform t, Matrix4x4 bp)
            {
                bone = t;
                bindPose = bp;
            }

            public override bool Equals(object obj)
            {
                if (obj is BoneAndBindpose)
                {
                    if (bone == ((BoneAndBindpose)obj).bone && bindPose == ((BoneAndBindpose)obj).bindPose)
                    {
                        return true;
                    }
                }
                return false;
            }

            public override int GetHashCode()
            {
                //OK if don't check bindPose well because bp should be the same
                return (bone.GetInstanceID() % 2147483647) ^ (int)bindPose[0, 0];
            }
        }

        /// <summary>
        /// Has no methods.
        /// It is used for passing in and and will get typecast.
        /// </summary>
        public interface IMeshChannelsCacheTaggingInterface
        {
            void Dispose();

            bool HasCollectedMeshData();

            void CollectChannelDataForAllMeshesInList(List<MB_DynamicGameObject> toUpdateDGOs, List<MB_DynamicGameObject> toAddDGOs,
                MB_MeshVertexChannelFlags newChannels,
                MB_RenderType renderType,
                bool doBlendShapes);

            MBBlendShape[] GetBlendShapes(Mesh mesh, int instanceID, GameObject gameObject);

            bool hasOutOfBoundsUVs(Mesh m, ref MB_Utility.MeshAnalysisResult mar, int submeshIdx);
        }

        /// <summary>
        /// All getter methods in MeshChannelsCacheNew assume that CollectChannelDataForAllMeshesInList has first been
        /// called and the data is already present and accounted for.
        /// </summary>
        public class MeshChannelsCache : IDisposable, IMeshChannelsCacheTaggingInterface
        {
            MB2_LogLevel LOG_LEVEL;
            MB2_LightmapOptions lightmapOption;
            protected Dictionary<int, MeshChannels> meshID2MeshChannels = new Dictionary<int, MeshChannels>();

            private bool _collectedMeshData;
            private bool _disposed;
            
            public MeshChannelsCache(MB2_LogLevel ll, MB2_LightmapOptions lo)
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
                foreach (MeshChannels mc in meshID2MeshChannels.Values)
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
                Vector2[] uvss = GetUv0Raw(m);
                return MB_Utility.hasOutOfBoundsUVs(uvss, m, ref mar, submeshIdx);
            }
            
            internal Vector3[] GetVertices(Mesh m)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                MeshChannels mc;
                if (!meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc))
                {
                    Debug.LogError("Could not find mesh in the MeshChannelsCache." + m);    
                }
                
                return mc.vertices;
            }
            
            internal Vector3[] GetNormals(Mesh m)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                MeshChannels mc;
                meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc);
                return mc.normals;
            }
            
            internal Vector4[] GetTangents(Mesh m)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                MeshChannels mc;
                meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc);
                return mc.tangents;
            }
            
            internal Vector2[] GetUv0Raw(Mesh m)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                MeshChannels mc;
                meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc);
                return mc.uv0raw;
            }
            
            internal Vector2[] GetUv0Modified(Mesh m)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                MeshChannels mc;
                meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc);
                return mc.uv0modified;
            }
            
            internal Vector2[] GetUv2Modified(Mesh m)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                MeshChannels mc;
                meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc);
                return mc.uv2modified;
            }

            internal Vector2[] GetUVChannel(int channel, Mesh m)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                MeshChannels mc;
                meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc);

                switch(channel)
                {
                    case 0:
                        return mc.uv0raw;
                    case 2:
                        return mc.uv2raw;
                    case 3:
                        return mc.uv3;
                    case 4:
                        return mc.uv4;
                    case 5:
                        return mc.uv5;
                    case 6:
                        return mc.uv6;
                    case 7:
                        return mc.uv7;
                    case 8:
                        return mc.uv8;
                    default:
                        Debug.LogError("Error mesh channel " + channel + " not supported");
                        break;
                }

                return null;
            }
            
            internal Color[] GetColors(Mesh m)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                MeshChannels mc;
                meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc);
                return mc.colors;
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
                        MeshChannels mc = new MeshChannels();
                        meshID2MeshChannels.Add(m.GetInstanceID(), mc);
                        {
                            if (doVerts) mc.vertices = m.vertices;

                            if (doUV0) mc.uv0raw = _getMeshUVs(m); // I think this looks good now. Check with Ian
                            if (doUV2) mc.uv2raw = _getMeshUV2s(m, ref mc.uv2modified); // I think this looks good now. Check with Ian.

                            if (doNormals) mc.normals = _getMeshNormals(m);
                            if (doTangents) mc.tangents = _getMeshTangents(m);
                            if (doUV3) mc.uv3 = MBVersion.GetMeshChannel(3, m, LOG_LEVEL);
                            if (doUV4) mc.uv4 = MBVersion.GetMeshChannel(4, m, LOG_LEVEL);
                            if (doUV5) mc.uv5 = MBVersion.GetMeshChannel(5, m, LOG_LEVEL);
                            if (doUV6) mc.uv6 = MBVersion.GetMeshChannel(6, m, LOG_LEVEL);
                            if (doUV7) mc.uv7 = MBVersion.GetMeshChannel(7, m, LOG_LEVEL);
                            if (doUV8) mc.uv8 = MBVersion.GetMeshChannel(8, m, LOG_LEVEL);
                            if (doColors) mc.colors = _getMeshColors(m);

                            if (renderType == MB_RenderType.skinnedMeshRenderer)
                            {
                                Renderer r = dgo._renderer;
                                bool isSkinnedMeshWithBones;
                                _getBindPoses(r, mc.bindPoses, out isSkinnedMeshWithBones);
// #if UNITY_2020_2_OR_NEWER
//                                // double check with Ian that mc.bindPoses.Length is okay to pass into this method
//                                _getBoneWeightData(ref mc.boneWeightData, r, mc.bindPoses.Length, isSkinnedMeshWithBones);
//#else
                                // double check with Ian that _getBoneWeights should only be called if the render type is a skinnedMeshRenderer
                                mc.boneWeights = _getBoneWeights(r, m.vertexCount, isSkinnedMeshWithBones);
                                //#endif
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

                _collectedMeshData = true;
            }
            
            internal List<Matrix4x4> GetBindposes(Renderer r, out bool isSkinnedMeshWithBones)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                Mesh m = MB_Utility.GetMesh(r.gameObject);
                MeshChannels mc;
                meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc);
                {
                    if (r is SkinnedMeshRenderer &&
                        mc.bindPoses.Count > 0)
                    {
                        isSkinnedMeshWithBones = true;
                    } else
                    {
                        isSkinnedMeshWithBones = false;
                        if (r is SkinnedMeshRenderer) Debug.Assert(m.blendShapeCount > 0, "Skinned Mesh Renderer " + r + " had no bones and no blend shapes");
                    }
                }
                return mc.bindPoses;
            }

            internal BoneWeight[] GetBoneWeights(Renderer r, int numVertsInMeshBeingAdded, bool isSkinnedMeshWithBones)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                Mesh m = MB_Utility.GetMesh(r.gameObject);
                MeshChannels mc;
                meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc);
                return mc.boneWeights;
            }
            

            /*internal int[] GetTriangles(Mesh m)
            {
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out MeshChannels mc);
                return mc.triangles;
            }*/

            public MBBlendShape[] GetBlendShapes(Mesh m, int gameObjectID, GameObject gameObject)
            {
                // Make sure I do the copying part of the setter like it is done in the old GetBlendShapes method
                // We are sure it is there for a reason.
                Debug.Assert(!_disposed && _collectedMeshData, "Mesh Channels Cache has not collected mesh data.");
                MeshChannels mc;
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
                    Vector2[] uvs = GetUv0Raw(m);
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
            private Vector2[] _getMeshUV2s(Mesh m, ref Vector2[] uv2modified)
            {
                Vector2[] uv = m.uv2;

                if (uv.Length == 0)
                {
                    uv2modified = new Vector2[m.vertexCount];
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

            /*
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
                else {
                    Debug.LogError("Could not _getBoneWeights. Object does not have a renderer");
                    return null;
                }
            }

            private void _generateTangents(int[] triangles, Vector3[] verts, Vector2[] uvs, Vector3[] normals, Vector4[] outTangents)
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
    }
}