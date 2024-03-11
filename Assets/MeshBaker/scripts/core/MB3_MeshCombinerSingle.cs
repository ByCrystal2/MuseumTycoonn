using UnityEngine;
using System.Collections.Specialized;
using System;
using System.Collections.Generic;
using Unity.Collections;

namespace DigitalOpus.MB.Core
{
    /// <summary>
    /// Manages a single combined mesh.This class is the core of the mesh combining API.
    /// 
    /// It is not a component so it can be can be instantiated and used like a normal c sharp class.
    /// </summary>
    [System.Serializable]
    public partial class MB3_MeshCombinerSingle : MB3_MeshCombiner
    {
        public System.Diagnostics.Stopwatch db_showHideGameObjects = new System.Diagnostics.Stopwatch();
        public System.Diagnostics.Stopwatch db_addDeleteGameObjects = new System.Diagnostics.Stopwatch();

        public System.Diagnostics.Stopwatch db_addDeleteGameObjects_CollectMeshData = new System.Diagnostics.Stopwatch();

        public System.Diagnostics.Stopwatch db_addDeleteGameObjects_CollectMeshData_a = new System.Diagnostics.Stopwatch();
        public System.Diagnostics.Stopwatch db_addDeleteGameObjects_CollectMeshData_b = new System.Diagnostics.Stopwatch();
        public System.Diagnostics.Stopwatch db_addDeleteGameObjects_CollectMeshData_c = new System.Diagnostics.Stopwatch();

        public System.Diagnostics.Stopwatch db_addDeleteGameObjects_InitFromMeshCombiner = new System.Diagnostics.Stopwatch();
        public System.Diagnostics.Stopwatch db_addDeleteGameObjects_Init = new System.Diagnostics.Stopwatch();
        public System.Diagnostics.Stopwatch db_addDeleteGameObjects_CopyArraysFromPreviousBakeBuffersToNewBuffers = new System.Diagnostics.Stopwatch();
        public System.Diagnostics.Stopwatch db_addDeleteGameObjects_CopyFromDGOMeshToBuffers = new System.Diagnostics.Stopwatch();

        public System.Diagnostics.Stopwatch db_apply = new System.Diagnostics.Stopwatch();
        public System.Diagnostics.Stopwatch db_applyShowHide = new System.Diagnostics.Stopwatch();
        public System.Diagnostics.Stopwatch db_updateGameObjects = new System.Diagnostics.Stopwatch();


        public void StartProfile()
        {
            db_showHideGameObjects.Reset();
            db_addDeleteGameObjects.Reset();
            db_apply.Reset();
            db_applyShowHide.Reset();
            db_updateGameObjects.Reset();
        }
        
        public void PrintProfileInfo()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Timings  " + (settings.meshAPI == MB_MeshCombineAPIType.betaNativeArrayAPI ? "  newMeshAPI " : " oldMeshAPI"));
            sb.AppendLine("db_showHideGameObjects " + db_showHideGameObjects.Elapsed.Seconds);
            sb.AppendLine("db_addDeleteGameObjects " + db_addDeleteGameObjects.Elapsed.Seconds);
            sb.AppendLine("db_apply " + db_apply.Elapsed.Seconds);
            sb.AppendLine("db_applyShowHide " + db_applyShowHide.Elapsed.Seconds);
            sb.AppendLine("db_updateGameObjects " + db_updateGameObjects.Elapsed.Seconds);
            Debug.Log(sb.ToString());
        }
        
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed()) return;
            base.Dispose(disposing);
            if (_boneProcessor != null)
            {
                _boneProcessor.DisposeOfTemporarySMRData();
                _boneProcessor.Dispose();
                _boneProcessor = null;
            }

            if (_blendShapeProcessor != null)
            {
                _blendShapeProcessor.Dispose();
                _blendShapeProcessor = null;
            }

            if (_meshChannelsCache != null)
            {
                _meshChannelsCache.Dispose();
                _meshChannelsCache = null;
            }

            if (_vertexAndTriProcessor != null)
            {
                _vertexAndTriProcessor.Dispose();
            }
        }

        public override MB2_TextureBakeResults textureBakeResults
        {
            set
            {
                if (GetVertexCount() > 0 && _textureBakeResults != value && _textureBakeResults != null)
                {
                    if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("If Texture Bake Result is changed then objects currently in combined mesh may be invalid.");
                }
                _textureBakeResults = value;
            }
        }

        public override MB_RenderType renderType
        {
            set
            {
                if (value == MB_RenderType.skinnedMeshRenderer && _renderType == MB_RenderType.meshRenderer)
                {
                    if (GetVertexCount() > 0 && (bones == null || bones.Length == 0)) Debug.LogError("Can't set the render type to SkinnedMeshRenderer without clearing the mesh first. Try deleting the CombinedMesh scene object.");
                }
                _renderType = value;
            }
        }

        public override GameObject resultSceneObject
        {
            set
            {
                if (_resultSceneObject != value && _resultSceneObject != null)
                {
                    _targetRenderer = null;
                    if (_mesh != null && LOG_LEVEL >= MB2_LogLevel.warn)
                    {
                        Debug.LogWarning("Result Scene Object was changed when this mesh baker component had a reference to a mesh. If mesh is being used by another object make sure to reset the mesh to none before baking to avoid overwriting the other mesh.");
                    }
                }
                _resultSceneObject = value;
            }
        }

        //this contains object instances that have been added to the combined mesh through AddDelete
        [SerializeField]
        protected List<GameObject> objectsInCombinedMesh = new List<GameObject>();

        [SerializeField]
        int lightmapIndex = -1;

        [SerializeField]
        List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh = new List<MB_DynamicGameObject>();
        Dictionary<GameObject, MB_DynamicGameObject> _instance2combined_map = new Dictionary<GameObject, MB_DynamicGameObject>();

        /// <summary>
        /// The channels that we baked last bake.
        /// </summary>
        [SerializeField]
        MB_MeshVertexChannelFlags channelsLastBake;

        // ======================== These fields are owned/managed by MeshDataBuffers
        // They are here because we need them to be serialized with he MeshCombiner.
        // If we are using the mesh-NativeArrays-API then we don't use these
        public int GetVertexCount()
        {
            return verts.Length;
        }

        /*
        public int GetNumSubmeshes()
        {
            return _meshDataBuffer.GetSubmeshCountFromSerializedDataInCombiner(this);
        }
        */

        [SerializeField]
        Vector3[] verts = new Vector3[0];
        [SerializeField]
        Vector3[] normals = new Vector3[0];
        [SerializeField]
        Vector4[] tangents = new Vector4[0];
        [SerializeField]
        Vector2[] uvs = new Vector2[0];
        [SerializeField]
        float[] uvsSliceIdx = new float[0];
        [SerializeField]
        Vector2[] uv2s = new Vector2[0];
        [SerializeField]
        Vector2[] uv3s = new Vector2[0];
        [SerializeField]
        Vector2[] uv4s = new Vector2[0];

        [SerializeField]
        Vector2[] uv5s = new Vector2[0];
        [SerializeField]
        Vector2[] uv6s = new Vector2[0];
        [SerializeField]
        Vector2[] uv7s = new Vector2[0];
        [SerializeField]
        Vector2[] uv8s = new Vector2[0];

        [SerializeField]
        Color[] colors = new Color[0];

        [SerializeField]
        SerializableIntArray[] submeshTris = new SerializableIntArray[0];
        // End fields owned by MeshDataBuffers ================

        [SerializeField]
        Matrix4x4[] bindPoses = new Matrix4x4[0];
        [SerializeField]
        Transform[] bones = new Transform[0];
        [SerializeField]
        internal MBBlendShape[] blendShapes = new MBBlendShape[0];

        internal BufferDataFromPreviousBake bufferDataFromPrevious;

        /// <summary>
        /// Needed for mesh disposal. We want to dispose of meshes that were created at runtime in a scene.
        /// However it is very important that we NOT dispose of meshes that were baked in the editor and saved in the scene or as assets in the project.
        /// </summary>
        [SerializeField]
        MeshCreationConditions _meshBirth = MeshCreationConditions.NoMesh;

        [SerializeField]
        Mesh _mesh;

        internal IVertexAndTriangleProcessor _vertexAndTriProcessor;
        protected MB_IMeshCombinerSingle_BoneProcessor _boneProcessor;
        internal MB_MeshCombinerSingle_BlendShapeProcessor _blendShapeProcessor;
        protected IMeshChannelsCacheTaggingInterface _meshChannelsCache;

        //used if user passes null in as parameter to AddOrDelete
        GameObject[] empty = new GameObject[0];
        int[] emptyIDs = new int[0];
        
        MB_DynamicGameObject instance2Combined_MapGet(GameObject gameObjectID)
        {
            return _instance2combined_map[gameObjectID];
        }

        void instance2Combined_MapAdd(GameObject gameObjectID, MB_DynamicGameObject dgo)
        {
            _instance2combined_map.Add(gameObjectID, dgo);
        }

        void instance2Combined_MapRemove(GameObject gameObjectID)
        {
            _instance2combined_map.Remove(gameObjectID);
        }

        bool instance2Combined_MapTryGetValue(GameObject gameObjectID, out MB_DynamicGameObject dgo)
        {
            return _instance2combined_map.TryGetValue(gameObjectID, out dgo);
        }

        int instance2Combined_MapCount()
        {
            return _instance2combined_map.Count;
        }

        void instance2Combined_MapClear()
        {
            _instance2combined_map.Clear();
        }

        bool instance2Combined_MapContainsKey(GameObject gameObjectID)
        {
            return _instance2combined_map.ContainsKey(gameObjectID);
        }

        bool InstanceID2DGO(int instanceID, out MB_DynamicGameObject dgoGameObject)
        {
            for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
            {
                if (mbDynamicObjectsInCombinedMesh[i].instanceID == instanceID)
                {
                    dgoGameObject = mbDynamicObjectsInCombinedMesh[i];
                    return true;
                }
            }

            dgoGameObject = null;
            return false;
        }

        public override int GetNumObjectsInCombined()
        {
            return mbDynamicObjectsInCombinedMesh.Count;
        }

        public override List<GameObject> GetObjectsInCombined()
        {
            List<GameObject> outObs = new List<GameObject>();
            outObs.AddRange(objectsInCombinedMesh);
            return outObs;
        }

        public Mesh GetMesh()
        {
            if (_mesh == null)
            {
                _mesh = _NewMesh();
            }
            return _mesh;
        }

        public MeshCreationConditions SetMesh(Mesh m)
        {
            if (m == null)
            {
                _meshBirth = MeshCreationConditions.NoMesh;
            }
            else
            {
                _meshBirth = MeshCreationConditions.AssignedByUser;
            }
           
            _mesh = m;
            return _meshBirth;
        }

        public Transform[] GetBones()
        {
            return bones;
        }

        public override int GetLightmapIndex()
        {
            if (settings.lightmapOption == MB2_LightmapOptions.generate_new_UV2_layout || settings.lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping)
            {
                return lightmapIndex;
            }
            else {
                return -1;
            }
        }

        public override int GetNumVerticesFor(GameObject go)
        {
            return GetNumVerticesFor(go.GetInstanceID());
        }

        public override int GetNumVerticesFor(int instanceID)
        {
            MB_DynamicGameObject dgo = null;
            InstanceID2DGO(instanceID, out dgo);
            if (dgo != null)
            {
                return dgo.numVerts;
            }
            else {
                return -1;
            }
        }

        bool _Initialize(int numResultMats)
        {
            if (mbDynamicObjectsInCombinedMesh.Count == 0)
            {
                lightmapIndex = -1;
            }

            if (_mesh == null)
            {
                if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("_initialize Creating new Mesh");
                _mesh = GetMesh();
            }

            if (instance2Combined_MapCount() != mbDynamicObjectsInCombinedMesh.Count)
            {
                //build the instance2Combined map
                instance2Combined_MapClear();
                for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
                {
                    if (mbDynamicObjectsInCombinedMesh[i] != null)
                    {
                        if (mbDynamicObjectsInCombinedMesh[i].gameObject == null)
                        {
                            Debug.LogError("This MeshBaker contains information from a previous bake that is incomlete. It may have been baked by a previous version of Mesh Baker. If you are trying to update/modify a previously baked combined mesh. Try doing the original bake.");
                            return false;
                        }

                        instance2Combined_MapAdd(mbDynamicObjectsInCombinedMesh[i].gameObject, mbDynamicObjectsInCombinedMesh[i]);
                    }
                }
            }

            /*
            if (objectsInCombinedMesh.Count == 0)
            {
                if (GetNumSubmeshes() != numResultMats)
                {
                    submeshTris = new SerializableIntArray[numResultMats];
                    for (int i = 0; i < submeshTris.Length; i++) submeshTris[i] = new SerializableIntArray(0);
                }
            }
            */

			if (LOG_LEVEL >= MB2_LogLevel.trace) {
				Debug.Log (String.Format ("_initialize numObjsInCombined={0}", mbDynamicObjectsInCombinedMesh.Count));
			}

            return true;
        }

        bool _collectMaterialTriangles(Mesh m, MB_DynamicGameObject dgo, Material[] sharedMaterials, OrderedDictionary sourceMats2submeshIdx_map)
        {
            //everything here applies to the source object being added
            int numTriMeshes = m.subMeshCount;
            if (sharedMaterials.Length < numTriMeshes) numTriMeshes = sharedMaterials.Length;
            dgo._tmpSubmeshTris = new SerializableIntArray[numTriMeshes];
            dgo.targetSubmeshIdxs = new int[numTriMeshes];
            for (int i = 0; i < numTriMeshes; i++)
            {
                if (_textureBakeResults.doMultiMaterial || _textureBakeResults.resultType == MB2_TextureBakeResults.ResultType.textureArray)
                {
                    if (!sourceMats2submeshIdx_map.Contains(sharedMaterials[i]))
                    {
                        Debug.LogError("Object " + dgo.name + " has a material that was not found in the result materials maping. " + sharedMaterials[i]);
                        return false;
                    }
                    dgo.targetSubmeshIdxs[i] = (int)sourceMats2submeshIdx_map[sharedMaterials[i]];
                }
                else {
                    dgo.targetSubmeshIdxs[i] = 0;
                }
                dgo._tmpSubmeshTris[i] = new SerializableIntArray();
                dgo._tmpSubmeshTris[i].data = m.GetTriangles(i);
                if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Collecting triangles for: " + dgo.name + " submesh:" + i + " maps to submesh:" + dgo.targetSubmeshIdxs[i] + " added:" + dgo._tmpSubmeshTris[i].data.Length, LOG_LEVEL);
            }
            return true;
        }

        // if adding many copies of the same mesh want to cache obUVsResults
        bool _collectOutOfBoundsUVRects2(Mesh m, MB_DynamicGameObject dgo, Material[] sharedMaterials, OrderedDictionary sourceMats2submeshIdx_map, Dictionary<int, MB_Utility.MeshAnalysisResult[]> meshAnalysisResults)
        {
            if (_textureBakeResults == null)
            {
                Debug.LogError("Need to bake textures into combined material");
                return false;
            }

            MB_Utility.MeshAnalysisResult[] res;
            if (!meshAnalysisResults.TryGetValue(m.GetInstanceID(), out res))
            {
                // Process the mesh and cache the result.
                int numSrcSubMeshes = m.subMeshCount;
                res = new MB_Utility.MeshAnalysisResult[numSrcSubMeshes];
                for (int submeshIdx = 0; submeshIdx < numSrcSubMeshes; submeshIdx++)
                {
                    _meshChannelsCache.hasOutOfBoundsUVs(m, ref res[submeshIdx], submeshIdx);
                }

                meshAnalysisResults.Add(m.GetInstanceID(), res);
            }
            
            int numUsedSrcSubMeshes = sharedMaterials.Length;
            if (numUsedSrcSubMeshes > m.subMeshCount) numUsedSrcSubMeshes = m.subMeshCount;
            dgo.obUVRects = new Rect[numUsedSrcSubMeshes];
            
            // We might have fewer sharedMaterials than submeshes in the mesh.
            for (int submeshIdx = 0; submeshIdx < numUsedSrcSubMeshes; submeshIdx++)
            {
                int idxInResultMats = dgo.targetSubmeshIdxs[submeshIdx];
                if (_textureBakeResults.GetConsiderMeshUVs(idxInResultMats, sharedMaterials[submeshIdx]))
                {
                    dgo.obUVRects[submeshIdx] = res[submeshIdx].uvRect;
                }
            }

            return true;
        }

        bool _validateTextureBakeResults()
        {
            if (_textureBakeResults == null)
            {
                Debug.LogError("Texture Bake Results is null. Can't combine meshes.");
                return false;
            }
            if (_textureBakeResults.materialsAndUVRects == null || _textureBakeResults.materialsAndUVRects.Length == 0)
            {
                Debug.LogError("Texture Bake Results has no materials in material to sourceUVRect map. Try baking materials. Can't combine meshes. " +
                    "If you are trying to combine meshes without combining materials, try removing the Texture Bake Result.");
                return false;
            }

            if (_textureBakeResults.NumResultMaterials() == 0)
            {
                Debug.LogError("Texture Bake Results has no result materials. Try baking materials. Can't combine meshes.");
                return false;
            }

            return true;
        }

        internal bool _ShowHide(GameObject[] goToShow, GameObject[] goToHide)
        {
            if (goToShow == null) goToShow = empty;
            if (goToHide == null) goToHide = empty;
            //calculate amount to hide
            int numResultMats = _textureBakeResults.NumResultMaterials();
            if (!_Initialize(numResultMats))
            {
                return false;
            }

            // Check that objects we are hiding are in the combined mesh.
            for (int i = 0; i < goToHide.Length; i++)
            {
                if (!instance2Combined_MapContainsKey(goToHide[i]))
                {
                    if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Trying to hide an object " + goToHide[i] + " that is not in combined mesh. Did you initially bake with 'clear buffers after bake' enabled?");
                    return false;
                }
            }

            // Check that objects we are showing are in the combined mesh.
            for (int i = 0; i < goToShow.Length; i++)
            {
                if (!instance2Combined_MapContainsKey(goToShow[i]))
                {
                    if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Trying to show an object " + goToShow[i] + " that is not in combined mesh. Did you initially bake with 'clear buffers after bake' enabled?");
                    return false;
                }
            }

            // input is valid so set the show/hide flags on the DGOs
            for (int i = 0; i < goToHide.Length; i++)
            {
                _instance2combined_map[goToHide[i]].show = false;
            }

            for (int i = 0; i < goToShow.Length; i++)
            {
                _instance2combined_map[goToShow[i]].show = true;
            }

            if (_vertexAndTriProcessor != null && !_vertexAndTriProcessor.IsDisposed())
            {
                _vertexAndTriProcessor.Dispose();
            }
            
            bool doNativeArrays = _UseNativeArrayAPIorNot();
            _vertexAndTriProcessor = Create_VertexAndTriangleProcessor(doNativeArrays);
            if (LOG_LEVEL >= MB2_LogLevel.debug)
            {
                if (doNativeArrays)
                {
                    Debug.Log("using NativeArray mesh API");
                }
                else
                {
                    Debug.Log("using simple mesh API");
                }
            }
            
            bool success = false;
            try
            {
                success = MB_MeshCombinerSingle_SubCombiner._ShowHideGameObjects(this);
                if (success)
                {
                    _bakeStatus = MeshCombiningStatus.readyForApply;
                }
            }
            catch
            {
                success = false;
                throw;
            }
            finally
            {

            }

            return success;
        }

        /// <summary>
        /// This level is for some basic validation and setup.
        /// Helpers that need need cleanup/disposal are created and cleanedup/disposed at this level.
        /// </summary>
        internal bool _AddToCombined(GameObject[] goToAdd, int[] goToDelete, bool disableRendererInSource)
        {
            System.Diagnostics.Stopwatch sw = null;
            if (LOG_LEVEL >= MB2_LogLevel.debug)
            {
                sw = new System.Diagnostics.Stopwatch();
                sw.Start();
            }
            GameObject[] _goToAdd;
            int[] _goToDelete;
            if (!_validateTextureBakeResults()) return false;
            if (!ValidateTargRendererAndMeshAndResultSceneObj()) return false;

            if (outputOption != MB2_OutputOptions.bakeMeshAssetsInPlace &&
                settings.renderType == MB_RenderType.skinnedMeshRenderer)
            {
                if (_targetRenderer == null || !(_targetRenderer is SkinnedMeshRenderer))
                {
                    Debug.LogError("Target renderer must be set and must be a SkinnedMeshRenderer");
                    return false;
                }
            }

            if (settings.doBlendShapes && settings.renderType != MB_RenderType.skinnedMeshRenderer)
            {
                Debug.LogError("If doBlendShapes is set then RenderType must be skinnedMeshRenderer.");
                return false;
            }

            if (goToAdd == null) _goToAdd = empty;
            else _goToAdd = (GameObject[])goToAdd.Clone();
            if (goToDelete == null) _goToDelete = emptyIDs;
            else _goToDelete = (int[])goToDelete.Clone();
            if (_mesh == null) DestroyMesh(); //cleanup maps and arrays

            int numResultMats = _textureBakeResults.NumResultMaterials();
            if (!_Initialize(numResultMats))
            {
                return false;
            }

            if (_mesh.vertexCount > 0 && _instance2combined_map.Count == 0)
            {
                Debug.LogWarning("There were vertices in the combined mesh but nothing in the MeshBaker buffers. If you are trying to bake in the editor and modify at runtime, make sure 'Clear Buffers After Bake' is unchecked.");
            }

            if (LOG_LEVEL >= MB2_LogLevel.debug)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("==== Calling _addToCombined objs adding:" + _goToAdd.Length + " objs deleting:" + _goToDelete.Length + " fixOutOfBounds:" + textureBakeResults.DoAnyResultMatsUseConsiderMeshUVs() + " doMultiMaterial:" + textureBakeResults.doMultiMaterial + " disableRenderersInSource:" + disableRendererInSource);

                if (LOG_LEVEL >= MB2_LogLevel.trace)
                {
                    for (int i = 0; i < _goToAdd.Length; i++)
                    {
                        sb.AppendLine("    adding obj[" + i + "]=" + _goToAdd[i]);
                    }

                    HashSet<int> deleteSet = new HashSet<int>(_goToDelete);
                    for (int i = 0; i < objectsInCombinedMesh.Count; i++)
                    {
                        bool deleteMe = deleteSet.Contains(objectsInCombinedMesh[i].gameObject.GetInstanceID());
                        if (!deleteMe)
                        {
                            sb.AppendLine("    keeping in combined:" + objectsInCombinedMesh[i]);
                        } else
                        {
                            sb.AppendLine("    deleting in combined:" + objectsInCombinedMesh[i]);
                        }
                    }
                }

                Debug.Log(sb);
            }

            //backward compatibility set up resultMaterials if it is blank
            if (_textureBakeResults.NumResultMaterials() == 0)
            {
                Debug.LogError("No resultMaterials in this TextureBakeResults. Try baking textures.");
                return false;
            }

            if (!settings.clearBuffersAfterBake && mbDynamicObjectsInCombinedMesh.Count > 0)
            {
                if (_mesh == null)
                {
                    Debug.LogError("Trying to add and delete to a combined mesh that was previously baked but the mesh is null.");
                    return false;
                }
                
                if (_mesh.vertexCount != bufferDataFromPrevious.numVertsBaked)
                {
                    Debug.LogError("Trying to add and delete to a combined mesh that was previously baked but the mesh vertex count is different. " + _mesh.vertexCount + " != " + bufferDataFromPrevious.numVertsBaked);
                    return false;
                }
            }

            OrderedDictionary sourceMats2submeshIdx_map = BuildSourceMatsToSubmeshIdxMap(numResultMats);
            if (sourceMats2submeshIdx_map == null)
            {
                return false;
            }

            bool createWchannelForUVs = settings.doUV && (textureBakeResults.resultType == MB2_TextureBakeResults.ResultType.textureArray);
            MB_MeshVertexChannelFlags newChannels = MeshBakerSettingsUtility.GetMeshChannelsAsFlags(settings, true, createWchannelForUVs);
            if (!settings.clearBuffersAfterBake && 
                (channelsLastBake != MB_MeshVertexChannelFlags.none) &&  // this could be an old combiner that was baked before the channelsLastBake flag was added.
                mbDynamicObjectsInCombinedMesh.Count > 0)
            {
                if (channelsLastBake != newChannels)
                {
                    Debug.LogError("There is data in the combined mesh and channels have changed since previous bake. Can't bake:\n channelsLastBake:" + channelsLastBake + "\n channels current bake: " + newChannels);
                    return false;
                }
            }

            // Create helpers
            // we are often adding the same sharedMesh many times. Only want to grab the results once and cache them
            if (_vertexAndTriProcessor != null && !_vertexAndTriProcessor.IsDisposed())
            {
                _vertexAndTriProcessor.Dispose();
            }

            bool doNativeArrays = _UseNativeArrayAPIorNot();
            IVertexAndTriangleProcessor oldMeshData;
            _meshChannelsCache = Create_MeshChannelsCache(doNativeArrays, LOG_LEVEL, settings.lightmapOption); // new MeshChannelsCache_NativeArray();
            _vertexAndTriProcessor = Create_VertexAndTriangleProcessor(doNativeArrays); //new VertexAndTriangleProcessorNativeArray();
            oldMeshData = Create_VertexAndTriangleProcessor(doNativeArrays); //new VertexAndTriangleProcessorNativeArray();
            _blendShapeProcessor = new MB_MeshCombinerSingle_BlendShapeProcessor(this);
            _boneProcessor = Create_BoneProcessor(doNativeArrays);
            if (LOG_LEVEL >= MB2_LogLevel.debug)
            {
                if (doNativeArrays)
                {
                    Debug.Log("using NativeArray mesh API");
                }
                else
                {
                    Debug.Log("using simple mesh API");
                }
            }

            bool success = false;
            try
            {
                success = __AddToCombined(_goToAdd, _goToDelete, disableRendererInSource, numResultMats, sourceMats2submeshIdx_map, ref oldMeshData, newChannels, sw);
            }
            catch
            {
                success = false;
                throw;
            }
            finally
            {
                // Cleanup helpers
                _meshChannelsCache.Dispose();
                _boneProcessor.DisposeOfTemporarySMRData();
                oldMeshData.Dispose();
                for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
                {
                    MB_DynamicGameObject dgo = mbDynamicObjectsInCombinedMesh[i];
                    dgo.UnInitialize();
                }
            }

            return success;
        }

        /// <summary>
        /// This level is for setting up and validating DGOs for each source renderer.
        /// </summary>
        internal bool __AddToCombined(GameObject[] _goToAdd, int[] _goToDelete, bool disableRendererInSource, int numResultMats, OrderedDictionary sourceMats2submeshIdx_map, ref IVertexAndTriangleProcessor oldMeshData, MB_MeshVertexChannelFlags newChannels, System.Diagnostics.Stopwatch sw)
        {
            //STEP 1 update our internal description of objects being added and deleted keep track of changes to buffer sizes as we do.
            UVAdjuster_Atlas uvAdjuster = new UVAdjuster_Atlas(textureBakeResults, LOG_LEVEL);
            
            //First pass create list of DGOs for toAdd and validate that they have renderers and meshes
            List<MB_DynamicGameObject> toAddDGOs = new List<MB_DynamicGameObject>();
            
            for (int i = 0; i < _goToAdd.Length; i++)
            {
                // if not already in mesh or we are deleting and re-adding in same operation
                if (!instance2Combined_MapContainsKey(_goToAdd[i]) || Array.FindIndex<int>(_goToDelete, o => o == _goToAdd[i].GetInstanceID()) != -1)
                {
                    MB_DynamicGameObject dgo = new MB_DynamicGameObject();
                    dgo.InitializeNew(false, _goToAdd[i]);
                    if (dgo._renderer == null)
                    {
                        Debug.LogError("Object " + dgo.gameObject.name + " does not have a Renderer");
                        _goToAdd[i] = null;
                        return false;
                    }

                    Material[] sharedMaterials = dgo._renderer.sharedMaterials;
                    if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log(String.Format("Getting {0} shared materials for {1}", sharedMaterials.Length, dgo.gameObject));
                    if (sharedMaterials == null)
                    {
                        Debug.LogError("Object " + dgo.name + " does not have a Renderer");
                        _goToAdd[i] = null;
                        return false;
                    }

                    Mesh m = dgo._mesh;
                    if (m == null)
                    {
                        Debug.LogError("Object " + dgo.gameObject.name + " MeshFilter or SkinnedMeshRenderer had no mesh");
                        _goToAdd[i] = null;
                        return false;
                    }
                    else if (MBVersion.IsRunningAndMeshNotReadWriteable(m))
                    {
                        Debug.LogError("Object " + dgo.gameObject.name + " Mesh Importer has read/write flag set to 'false'. This needs to be set to 'true' in order to read data from this mesh.");
                        _goToAdd[i] = null;
                        return false;
                    }

                    if (sharedMaterials.Length > m.subMeshCount)
                    {
                        // The extra materials do nothing but could cause bugs.
                        Array.Resize(ref sharedMaterials, m.subMeshCount);
                    }
                    if (_goToAdd[i] != null)
                    {
                        toAddDGOs.Add(dgo);
                        dgo.name = String.Format("{0} {1}", _goToAdd[i].ToString(), _goToAdd[i].GetInstanceID());
                        dgo.instanceID = _goToAdd[i].GetInstanceID();
                        dgo.gameObject = _goToAdd[i];
                        dgo.numVerts = m.vertexCount;
                        dgo.sourceSharedMaterials = sharedMaterials;
                    }
                }
                else
                {
                    if (LOG_LEVEL >= MB2_LogLevel.warn)
                        Debug.LogWarning("Object " + _goToAdd[i].name +
                                         " has already been added. This MeshBaker may have been baked previously with 'Clear Buffers After Bake' unchecked." +
                                         " You can clear the buffers by checking 'Clear Buffers After Bake' and baking. If you want to update a combined mesh by baking several times, you should uncheck 'Clear Buffers After Bake'.");
                    _goToAdd[i] = null;
                }
            }
            
            // This is where we assign the meshChannelCache full of data from the meshes in toAddDGOs and mbDynamicObjectsInCombinedMesh.
            for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
            {
                if (!mbDynamicObjectsInCombinedMesh[i]._beingDeleted)
                {
                    MB_DynamicGameObject dgo = mbDynamicObjectsInCombinedMesh[i];
                    if (!dgo.Initialize(false))
                    {
                        Debug.LogError("Object " + dgo.gameObject.name + " does not have a Renderer");
                        return false;
                    }
                }
            }

            db_addDeleteGameObjects_CollectMeshData.Start();
            db_addDeleteGameObjects_CollectMeshData_a.Start();
            {
                
                _meshChannelsCache.CollectChannelDataForAllMeshesInList(mbDynamicObjectsInCombinedMesh, toAddDGOs, newChannels, settings.renderType, settings.doBlendShapes);
                
            }
            db_addDeleteGameObjects_CollectMeshData_a.Stop();

            // Vist objects we are deleting and collect info for them
            int totalDeleteVerts = 0;
            int[] totalDeleteSubmeshTris = new int[numResultMats];
            int totalDeleteBlendShapes = 0;
            
            //in order to decide if a bone can be deleted need to know which dgos use it so build a map
            _boneProcessor.BuildBoneIdx2DGOMapIfNecessary(_goToDelete);
            for (int i = 0; i < _goToDelete.Length; i++)
            {
                MB_DynamicGameObject dgo = null;
                InstanceID2DGO(_goToDelete[i], out dgo);
                if (dgo != null)
                {
                    dgo.Initialize(true);
                    totalDeleteVerts += dgo.numVerts;
                    totalDeleteBlendShapes += dgo.numBlendShapes;
                    if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                    {
                        // boneProcessor.FindBonesToDelete(dgo);
                        _boneProcessor.RemoveBonesForDgosWeAreDeleting(dgo);

                    }

                    for (int j = 0; j < dgo.submeshNumTris.Length; j++)
                    {
                        totalDeleteSubmeshTris[j] += dgo.submeshNumTris[j];
                    }
                }
                else
                {
                    if (LOG_LEVEL >= MB2_LogLevel.warn)
                        Debug.LogWarning("Trying to delete an object that is not in combined mesh");
                }
            }

            // Visit objects we are updating/keeping
            db_addDeleteGameObjects_CollectMeshData_b.Start();
            for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
            {
                if (!mbDynamicObjectsInCombinedMesh[i]._beingDeleted)
                {
                    MB_DynamicGameObject dgo = mbDynamicObjectsInCombinedMesh[i];

                    if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                    {
                        if (!_boneProcessor.GetCachedSMRMeshData(dgo))
                        {
                            Debug.LogError("Object " + dgo.gameObject.name + " could not retrieve skinning data");
                            return false;
                        }
                    }
                }
            }
            db_addDeleteGameObjects_CollectMeshData_b.Stop();

            db_addDeleteGameObjects_CollectMeshData.Stop();
            
            // Now add
            Dictionary<int, MB_Utility.MeshAnalysisResult[]> meshAnalysisResultsCache = new Dictionary<int, MB_Utility.MeshAnalysisResult[]>(); //cache results

            int totalAddVerts = 0;
            int[] totalAddSubmeshTris = new int[numResultMats];
            int totalAddBlendShapes = 0;
            
            for (int i = 0; i < toAddDGOs.Count; i++)
            {
                MB_DynamicGameObject dgo = toAddDGOs[i];
                Mesh m = dgo._mesh;
                Material[] sharedMaterials = dgo.sourceSharedMaterials;
                if (!uvAdjuster.MapSharedMaterialsToAtlasRects(sharedMaterials, false, m, _meshChannelsCache,
                        meshAnalysisResultsCache, sourceMats2submeshIdx_map, dgo.gameObject, dgo))
                {
                    _goToAdd[i] = null;
                    return false;
                }
                
                if (_goToAdd[i] != null)
                {
                    if (settings.doBlendShapes)
                    {
                        dgo.numBlendShapes = m.blendShapeCount;
                    }

                    Renderer r = dgo._renderer;
                    if (lightmapIndex == -1)
                    {
                        lightmapIndex = r.lightmapIndex; //initialize	
                    }

                    if (settings.lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping)
                    {
                        if (lightmapIndex != r.lightmapIndex)
                        {
                            if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Object " + dgo.gameObject.name + " has a different lightmap index. Lightmapping will not work.");
                        }

                        if (!MBVersion.GetActive(dgo.gameObject))
                        {
                            if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Object " + dgo.gameObject.name + " is inactive. Can only get lightmap index of active objects.");
                        }

                        if (r.lightmapIndex == -1)
                        {
                            if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Object " + dgo.gameObject.name + " does not have an index to a lightmap.");
                        }
                    }

                    dgo.lightmapIndex = r.lightmapIndex;
                    dgo.lightmapTilingOffset = MBVersion.GetLightmapTilingOffset(r);
                    if (!_collectMaterialTriangles(m, dgo, sharedMaterials, sourceMats2submeshIdx_map))
                    {
                        return false;
                    }

                    dgo.meshSize = r.bounds.size;
                    dgo.submeshNumTris = new int[numResultMats];
                    dgo.submeshTriIdxs = new int[numResultMats];
                    dgo.sourceSharedMaterials = sharedMaterials;

                    bool doAnyResultsUseConsiderMeshUVs = textureBakeResults.DoAnyResultMatsUseConsiderMeshUVs();
                    if (doAnyResultsUseConsiderMeshUVs)
                    {
                        if (!_collectOutOfBoundsUVRects2(m, dgo, sharedMaterials, sourceMats2submeshIdx_map,
                                meshAnalysisResultsCache))
                        {
                            return false;
                        }
                    }

                    if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                    {
                        db_addDeleteGameObjects_CollectMeshData.Start();
                        db_addDeleteGameObjects_CollectMeshData_c.Start();
                        if (!_boneProcessor.GetCachedSMRMeshData(dgo))
                        {
                            return false;
                        }
                        db_addDeleteGameObjects_CollectMeshData_c.Stop();
                        db_addDeleteGameObjects_CollectMeshData.Stop();
                    }

                    if (settings.assignToMeshCustomizer != null)
                    {
                        if (_UseNativeArrayAPIorNot())
                        {
                            if (!(settings.assignToMeshCustomizer is IAssignToMeshCustomizer_NativeArrays))
                            {
                                Debug.LogError("Assign To Mesh Customizer must implement IAssignToMeshCustomizer_NativeArrays");
                                return false;
                            }
                        } else
                        {
                            if (!(settings.assignToMeshCustomizer is IAssignToMeshCustomizer_SimpleAPI))
                            {
                                Debug.LogError("Assign To Mesh Customizer must implemennt IAssignToMeshCustomizer_SimpleAPI");
                                return false;
                            }
                        }
                    }

                    totalAddVerts += dgo.numVerts;
                    totalAddBlendShapes += dgo.numBlendShapes;
                    for (int j = 0; j < dgo._tmpSubmeshTris.Length; j++)
                    {
                        totalAddSubmeshTris[dgo.targetSubmeshIdxs[j]] += dgo._tmpSubmeshTris[j].data.Length;
                    }

                    dgo.invertTriangles = IsMirrored(dgo.gameObject.transform.localToWorldMatrix);

                    Debug.Assert(dgo.targetSubmeshIdxs.Length == dgo.uvRects.Length, "Array length mismatch targetSubmeshIdxs, uvRects");
                    Debug.Assert(dgo.targetSubmeshIdxs.Length == dgo.sourceSharedMaterials.Length, "Array length mismatch targetSubmeshIdxs, uvRects");
                    Debug.Assert(dgo.targetSubmeshIdxs.Length == dgo.encapsulatingRect.Length, "Array length mismatch targetSubmeshIdxs, uvRects");
                    Debug.Assert(dgo.targetSubmeshIdxs.Length == dgo.sourceMaterialTiling.Length, "Array length mismatch targetSubmeshIdxs, uvRects");
                    if (doAnyResultsUseConsiderMeshUVs) Debug.Assert(dgo.targetSubmeshIdxs.Length == dgo.obUVRects.Length, "Array length mismatch targetSubmeshIdxs, uvRects");
                }
            }

            for (int i = 0; i < _goToAdd.Length; i++)
            {
                if (_goToAdd[i] != null && disableRendererInSource)
                {
                    MB_Utility.DisableRendererInSource(_goToAdd[i]);
                    if (LOG_LEVEL == MB2_LogLevel.trace) Debug.Log("Disabling renderer on " + _goToAdd[i].name + " id=" + _goToAdd[i].GetInstanceID());
                }
            }

            // STEP 2 to allocate new buffers and copy everything over
            bool success = MB_MeshCombinerSingle_SubCombiner._AddToCombined(this, newChannels,
                        totalAddVerts, totalDeleteVerts, numResultMats, totalAddBlendShapes, totalDeleteBlendShapes, 
                        totalAddSubmeshTris, totalDeleteSubmeshTris,
                        _goToDelete, toAddDGOs, _goToAdd, 
                        uvAdjuster, ref oldMeshData, sw);
            
            if (success)
            {
                _bakeStatus = MeshCombiningStatus.readyForApply;
            }

            return success;
        }

        Transform[] _getBones(Renderer r, bool isSkinnedMeshWithBones)
        {
            return MBVersion.GetBones(r, isSkinnedMeshWithBones);
        }

        public override bool Apply(GenerateUV2Delegate uv2GenerationMethod)
        {
            db_apply.Start();
            bool ret = MB_MeshCombinerSingle_SubCombiner.Apply(this, uv2GenerationMethod);
            db_apply.Stop();
            return ret;
        }

        public virtual void ApplyShowHide()
        {
            db_applyShowHide.Start();
            if (_validationLevel >= MB2_ValidationLevel.quick && !ValidateTargRendererAndMeshAndResultSceneObj()) return;
            MB_MeshCombinerSingle_SubCombiner.ApplyShowHide(this);
            db_applyShowHide.Stop();
        }

        public override bool Apply(bool triangles,
                          bool vertices,
                          bool normals,
                          bool tangents,
                          bool uvs,
                          bool uv2,
                          bool uv3,
                          bool uv4,
                          bool colors,
                          bool bones = false,
                          bool blendShapesFlag = false,
                          GenerateUV2Delegate uv2GenerationMethod = null)
        {
            db_apply.Start();
            bool ret = MB_MeshCombinerSingle_SubCombiner.Apply(this, triangles, vertices, normals, tangents, uvs, uv2, uv3, uv4, colors, bones, blendShapesFlag, uv2GenerationMethod);
            db_apply.Stop();
            return ret;
        }

        public override bool Apply(bool triangles,
                          bool vertices,
                          bool normals,
                          bool tangents,
                          bool uvs,
                          bool uv2,
                          bool uv3,
                          bool uv4,
                          bool uv5,
                          bool uv6,
                          bool uv7,
                          bool uv8,
                          bool colors,
                          bool bones = false,
                          bool blendShapesFlag = false,
                          GenerateUV2Delegate uv2GenerationMethod = null)
        {
            db_apply.Start();
            bool ret = MB_MeshCombinerSingle_SubCombiner.Apply(this, triangles, vertices, normals, tangents, uvs, uv2, uv3, uv4, uv5, uv6, uv7, uv8, colors, bones, blendShapesFlag, false, uv2GenerationMethod);
            db_apply.Stop();
            return ret;
        }

        public override bool UpdateGameObjects(GameObject[] gos, bool recalcBounds,
                                        bool updateVertices, bool updateNormals, bool updateTangents,
                                        bool updateUV, bool updateUV2, bool updateUV3, bool updateUV4,
                                        bool updateColors, bool updateSkinningInfo)
        {
            db_updateGameObjects.Start();
            bool ret = _UpdateGameObjects(gos, recalcBounds, updateVertices, updateNormals, updateTangents, updateUV, updateUV2, updateUV3, updateUV4,
                                        false, false, false, false, updateColors, updateSkinningInfo);
            db_updateGameObjects.Stop();
            return ret;
        }

        public override bool UpdateGameObjects(GameObject[] gos, bool recalcBounds,
                                        bool updateVertices, bool updateNormals, bool updateTangents,
                                        bool updateUV, bool updateUV2, bool updateUV3, bool updateUV4,
                                        bool updateUV5, bool updateUV6, bool updateUV7, bool updateUV8,
                                        bool updateColors, bool updateSkinningInfo)
        {
            db_updateGameObjects.Start();
            bool ret = _UpdateGameObjects(gos, recalcBounds, updateVertices, updateNormals, updateTangents, updateUV, updateUV2, updateUV3, updateUV4,
                                        updateUV5, updateUV6, updateUV7, updateUV8, updateColors, updateSkinningInfo);
            db_updateGameObjects.Stop();
            return ret;
        }

        internal bool _UpdateGameObjects(GameObject[] gos, bool recalcBounds,
                                        bool updateVertices, bool updateNormals, bool updateTangents,
                                        bool updateUV, bool updateUV2, bool updateUV3, bool updateUV4, bool updateUV5, bool updateUV6, bool updateUV7, bool updateUV8,
                                        bool updateColors, bool updateSkinningInfo)
        {
            if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("UpdateGameObjects called on " + gos.Length + " objects.");
            int numResultMats = 1;
            if (textureBakeResults.doMultiMaterial) numResultMats = textureBakeResults.NumResultMaterials();

            if (!_Initialize(numResultMats))
            {
                return false;
            }

            bool createWchannelForUVs = settings.doUV && (textureBakeResults.resultType == MB2_TextureBakeResults.ResultType.textureArray);
            MB_MeshVertexChannelFlags newChannels = MeshBakerSettingsUtility.GetMeshChannelsAsFlags(settings, true, createWchannelForUVs);
            if (channelsLastBake != newChannels)
            {
                Debug.LogError("Channels changed since previous bake. Can't Update GameObjects.");
                return false;
            }

            if (_bakeStatus != MeshCombiningStatus.preAddDeleteOrUpdate)
            {
                Debug.LogError("Bake Status of combiner was not 'preAddDeleteOrUpdate'. This can happen if AddDeleteGameObjects or UpdateGameObjects is called" +
                    " twice without calling Apply. You can call 'ClearBuffers' to reset the combiner.");
                return false;
            }

            if (_mesh.vertexCount > 0 && _instance2combined_map.Count == 0)
            {
                Debug.LogWarning("There were vertices in the combined mesh but nothing in the MeshBaker buffers. If you are trying to bake in the editor and modify at runtime, make sure 'Clear Buffers After Bake' is unchecked.");
            }

            if (settings.assignToMeshCustomizer != null)
            {
                if (_UseNativeArrayAPIorNot())
                {
                    if (!(settings.assignToMeshCustomizer is IAssignToMeshCustomizer_NativeArrays))
                    {
                        Debug.LogError("Assign To Mesh Customizer must implement IAssignToMeshCustomizer_NativeArrays");
                        return false;
                    }
                }
                else
                {
                    if (!(settings.assignToMeshCustomizer is IAssignToMeshCustomizer_SimpleAPI))
                    {
                        Debug.LogError("Assign To Mesh Customizer must implemennt IAssignToMeshCustomizer_SimpleAPI");
                        return false;
                    }
                }
            }

            UVAdjuster_Atlas uvAdjuster = null;
            OrderedDictionary sourceMats2submeshIdx_map = null;
            Dictionary<int, MB_Utility.MeshAnalysisResult[]> meshAnalysisResultsCache = null;
            if (updateUV)
            {
                sourceMats2submeshIdx_map = BuildSourceMatsToSubmeshIdxMap(numResultMats);
                if (sourceMats2submeshIdx_map == null)
                {
                    return false;
                }

                uvAdjuster = new UVAdjuster_Atlas(textureBakeResults, LOG_LEVEL);
                meshAnalysisResultsCache = new Dictionary<int, MB_Utility.MeshAnalysisResult[]>();
            }

            if (_vertexAndTriProcessor != null && !_vertexAndTriProcessor.IsDisposed())
            {
                _vertexAndTriProcessor.Dispose();
            }


            _blendShapeProcessor = new MB_MeshCombinerSingle_BlendShapeProcessor(this);
            bool doNativeArrays = _UseNativeArrayAPIorNot();
            _meshChannelsCache = Create_MeshChannelsCache(doNativeArrays, LOG_LEVEL, settings.lightmapOption); // new MeshChannelsCache_NativeArray();
            _vertexAndTriProcessor = Create_VertexAndTriangleProcessor(doNativeArrays); //new VertexAndTriangleProcessorNativeArray();
            _boneProcessor = Create_BoneProcessor(doNativeArrays);
            if (LOG_LEVEL >= MB2_LogLevel.debug)
            {
                if (doNativeArrays)
                {
                    Debug.Log("using NativeArray mesh API");
                }
                else
                {
                    Debug.Log("using simple mesh API");
                }
            }

            bool success = true;
            try
            {
                success = success && __UpdateGameObjects(gos, recalcBounds, newChannels, updateVertices, updateNormals, updateTangents, updateUV, updateUV2, updateUV3, updateUV4, updateUV5, updateUV6, updateUV7, updateUV8, updateColors, updateSkinningInfo,
                        meshAnalysisResultsCache, sourceMats2submeshIdx_map, uvAdjuster);
            }
            catch
            {
                success = false;
                throw;
            }
            finally
            {
                _meshChannelsCache.Dispose();
                _boneProcessor.DisposeOfTemporarySMRData();
                for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
                {
                    MB_DynamicGameObject dgo = mbDynamicObjectsInCombinedMesh[i];
                    if (dgo._initialized)
                    {
                        dgo.UnInitialize();
                    }
                }
            }

            return success;
        }

        private bool __UpdateGameObjects(GameObject[] gos, bool recalcBounds, MB_MeshVertexChannelFlags newChannels, bool updateVertices, bool updateNormals, bool updateTangents,
                                        bool updateUV, bool updateUV2, bool updateUV3, bool updateUV4, bool updateUV5, bool updateUV6, bool updateUV7, bool updateUV8,
                                        bool updateColors, bool updateSkinningInfo, 
                                        Dictionary<int, MB_Utility.MeshAnalysisResult[]> meshAnalysisResultsCache,
                                        OrderedDictionary sourceMats2submeshIdx_map, UVAdjuster_Atlas uVAdjuster)
        {
            List<MB_DynamicGameObject> objsToUpdate = new List<MB_DynamicGameObject>();
            for (int i = 0; i < gos.Length; i++)
            {
                MB_DynamicGameObject dgo = _instance2combined_map[gos[i]];
                if (!dgo.Initialize(false))
                {
                    Debug.LogError("Object " + dgo.name + " could not be initialized");
                    return false;
                }

                objsToUpdate.Add(dgo);
                if (dgo._mesh == null)
                {
                    Debug.LogError("Object " + dgo.name + " had no renderer");
                    return false;
                }

                if (dgo._renderer == null)
                {
                    Debug.LogError("Object " + dgo.name + " had no renderer");
                    return false;
                }

                Mesh mesh = dgo._mesh;
                if (dgo.numVerts != mesh.vertexCount)
                {
                    Debug.LogError("Object " + dgo.gameObject.name + " source mesh has been modified since being added. To update it must have the same number of verts");
                    return false;
                }
            }

            // This is where we assign the meshChannelCache full of data from the meshes in toAddDGOs and mbDynamicObjectsInCombinedMesh.
            for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
            {
                Debug.Assert(!mbDynamicObjectsInCombinedMesh[i]._beingDeleted, "There should be not deleted objects in mbDynamicObjectsInCombinedMesh when UpdateGameObjects is called");
                if (!mbDynamicObjectsInCombinedMesh[i]._beingDeleted)
                {
                    MB_DynamicGameObject dgo = mbDynamicObjectsInCombinedMesh[i];
                    if (!dgo.Initialize(false))
                    {
                        Debug.LogError("Object " + dgo.gameObject.name + " does not have a Renderer");
                        return false;
                    }
                }
            }

            {
                _meshChannelsCache.CollectChannelDataForAllMeshesInList(mbDynamicObjectsInCombinedMesh, objsToUpdate, newChannels, settings.renderType, settings.doBlendShapes);
            }
            
            for (int i = 0; i < gos.Length; i++)
            {
                MB_DynamicGameObject dgo = _instance2combined_map[gos[i]];
                if (settings.doUV && updateUV)
                {
                    // updating UVs is a bit more complicated because most likely the user has changed
                    // the material on the source mesh which is why they are calling update. We need to
                    // find the UV rect for this.
                    Material[] sharedMaterials = dgo._renderer.sharedMaterials;
                    if (!uVAdjuster.MapSharedMaterialsToAtlasRects(sharedMaterials, true, dgo._mesh, _meshChannelsCache, meshAnalysisResultsCache, sourceMats2submeshIdx_map, dgo.gameObject, dgo))
                    {
                        return false;
                    }
                }
            }

            _boneProcessor.BuildBoneIdx2DGOMapIfNecessary(null);

            bool success = MB_MeshCombinerSingle_SubCombiner._UpdateGameObjects(this, objsToUpdate, 
                                        newChannels,
                                        updateVertices, updateNormals, updateTangents,
                                        updateUV, updateUV2, updateUV3, updateUV4, updateUV5, updateUV6, updateUV7, updateUV8,
                                        updateColors, updateSkinningInfo,
                                        uVAdjuster, LOG_LEVEL);

            if (success && recalcBounds)
            {
                _mesh.RecalculateBounds();
            }

            return success;
        }

        public bool ShowHideGameObjects(GameObject[] toShow, GameObject[] toHide)
        {
            db_showHideGameObjects.Start();
            if (textureBakeResults == null)
            {
                Debug.LogError("TextureBakeResults must be set.");
                return false;
            }
            bool ret = _ShowHide(toShow, toHide);
            db_showHideGameObjects.Stop();
            return ret;
        }

        public override bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource = true)
        {
            db_addDeleteGameObjects.Start();
            int[] delInstanceIDs = null;
            if (deleteGOs != null)
            {
                delInstanceIDs = new int[deleteGOs.Length];
                for (int i = 0; i < deleteGOs.Length; i++)
                {
                    if (deleteGOs[i] == null)
                    {
                        Debug.LogError("The " + i + "th object on the list of objects to delete is 'Null'");
                    }
                    else {
                        delInstanceIDs[i] = deleteGOs[i].GetInstanceID();
                    }
                }
            }
            bool ret = AddDeleteGameObjectsByID(gos, delInstanceIDs, disableRendererInSource);
            db_addDeleteGameObjects.Stop();
            return ret;
        }

        public override bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOinstanceIDs, bool disableRendererInSource)
        {
            db_addDeleteGameObjects.Start();
            //			Profile.StartProfile("AddDeleteGameObjectsByID");
            if (validationLevel > MB2_ValidationLevel.none)
            {
                //check for duplicates
                if (gos != null)
                {
                    for (int i = 0; i < gos.Length; i++)
                    {
                        if (gos[i] == null)
                        {
                            Debug.LogError("The " + i + "th object on the list of objects to combine is 'None'. Use Command-Delete on Mac OS X; Delete or Shift-Delete on Windows to remove this one element.");
                            return false;
                        }
                        if (validationLevel >= MB2_ValidationLevel.robust)
                        {
                            for (int j = i + 1; j < gos.Length; j++)
                            {
                                if (gos[i] == gos[j])
                                {
                                    Debug.LogError("GameObject " + gos[i] + " appears twice in list of game objects to add");
                                    return false;
                                }
                            }
                        }
                    }
                }
                if (deleteGOinstanceIDs != null && validationLevel >= MB2_ValidationLevel.robust)
                {
                    for (int i = 0; i < deleteGOinstanceIDs.Length; i++)
                    {
                        for (int j = i + 1; j < deleteGOinstanceIDs.Length; j++)
                        {
                            if (deleteGOinstanceIDs[i] == deleteGOinstanceIDs[j])
                            {
                                Debug.LogError("GameObject " + deleteGOinstanceIDs[i] + "appears twice in list of game objects to delete");
                                return false;
                            }
                        }
                    }
                }
            }

            if (_bakeStatus != MeshCombiningStatus.preAddDeleteOrUpdate)
            {
                Debug.LogError("Bake Status of combiner was not 'preAddDeleteOrUpdate'. This can happen if AddDeleteGameObjects or UpdateGameObjects is called" +
                    " twice without calling Apply. You can call 'ClearBuffers' to reset the combiner.");
                return false;
            }

            if (_usingTemporaryTextureBakeResult && gos != null && gos.Length > 0)
            {
                MB_Utility.Destroy(_textureBakeResults);
                _textureBakeResults = null;
                _usingTemporaryTextureBakeResult = false;
            }

            //create a temporary _textureBakeResults if needed 
            if (_textureBakeResults == null && gos != null && gos.Length > 0 && gos[0] != null)
            {
                if (!_CreateTemporaryTextrueBakeResult(gos, GetMaterialsOnTargetRenderer()))
                {
                    return false;
                }
            }

            BuildSceneMeshObject(gos);


            if (!_AddToCombined(gos, deleteGOinstanceIDs, disableRendererInSource))
            {
                Debug.LogError("Failed to add/delete objects to combined mesh");
                return false;
            }

            //			Profile.EndProfile("AddDeleteGameObjectsByID");
            //			Profile.PrintResults();
            db_addDeleteGameObjects.Stop();
            return true;
        }

        public override bool CombinedMeshContains(GameObject go)
        {
            return objectsInCombinedMesh.Contains(go);
        }

        public override void ClearBuffers()
        {
            bones = new Transform[0];
            bindPoses = new Matrix4x4[0];
            blendShapes = new MBBlendShape[0];
            // blendShapesInCombined = new MBBlendShape[0];
            mbDynamicObjectsInCombinedMesh.Clear();
            objectsInCombinedMesh.Clear();
            if (_vertexAndTriProcessor != null && !_vertexAndTriProcessor.IsDisposed())
            {
                _vertexAndTriProcessor.Dispose();
            }

            // WILL NEED A CHECK HERE TO DETERMINE WHICH VERTEXANDTRIANGLEPROCESSOR TO USE
            _vertexAndTriProcessor = Create_VertexAndTriangleProcessor(_UseNativeArrayAPIorNot());
            {
                // These arrays in the combiner are actually owned by the MeshDataBuffers
                // Terrible design, but we need to put them in MeshCombiner because:
                //     We need them to be serialized with the MeshCombiner
                //     Many users have serialized data with these fields. We need that data preserved.
                verts = new Vector3[0];
                normals = new Vector3[0];
                tangents = new Vector4[0];
                uvs = new Vector2[0];
                uvsSliceIdx = new float[0];
                uv2s = new Vector2[0];
                uv3s = new Vector2[0];
                uv4s = new Vector2[0];
                uv5s = new Vector2[0];
                uv6s = new Vector2[0];
                uv7s = new Vector2[0];
                uv8s = new Vector2[0];
                colors = new Color[0];
                submeshTris = new SerializableIntArray[0];
            }

            if (submeshTris != null)
            {
                for (int i = 0; i < submeshTris.Length; i++)
                {
                    if (submeshTris[i].data == null)
                    {
                        submeshTris[i].data = new int[0];
                    }
                    else if (submeshTris[i].data.Length > 0)
                    {
                        submeshTris[i].data = new int[0];
                    }
                }

                submeshTris = null;
            }

            instance2Combined_MapClear();
            if (_usingTemporaryTextureBakeResult)
            {
                MB_Utility.Destroy(_textureBakeResults);
                _textureBakeResults = null;
                _usingTemporaryTextureBakeResult = false;
            }

            _bakeStatus = MeshCombiningStatus.preAddDeleteOrUpdate;

            if (LOG_LEVEL >= MB2_LogLevel.trace) MB2_Log.LogDebug("ClearBuffers called");
        }

        private Mesh _NewMesh()
        {
            if (Application.isPlaying)
            {
                _meshBirth = MeshCreationConditions.CreatedAtRuntime;
            } else {
                _meshBirth = MeshCreationConditions.CreatedInEditor;
            }
            Mesh m = new Mesh();
            
            return m;
        }

        /*
		 * Empties all channels and clears the mesh
		 */
        public override void ClearMesh()
        {
            if (_mesh != null)
            {
                MBVersion.MeshClear(_mesh, false);
            }
            else {
                _mesh = _NewMesh();
            }
            ClearBuffers();
        }

        public override void ClearMesh(MB2_EditorMethodsInterface editorMethods)
        {
            ClearMesh();
        }

        internal override void _DisposeRuntimeCreated()
        {
            if (Application.isPlaying)
            {
                if (_meshBirth == MeshCreationConditions.CreatedAtRuntime)
                {
                    if (!MBVersion.IsAssetInProject(_mesh))
                    {
                        GameObject.Destroy(_mesh);
                    } 
                }
                else if (_meshBirth == MeshCreationConditions.AssignedByUser)
                {
                    // This mesh was assigned by a user. The user is responsible for destroying it.
                    _mesh = null;
                }

                ClearBuffers();
            }
        }

        /// <summary>
        /// Empties all channels, destroys the mesh and replaces it with a new mesh
        /// </summary>
        public override void DestroyMesh()
        {
            if (_mesh != null)
            {
                if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Destroying Mesh");
                MB_Utility.Destroy(_mesh);
                _meshBirth = MeshCreationConditions.NoMesh;
            }

            ClearBuffers();
        }

        public override void DestroyMeshEditor(MB2_EditorMethodsInterface editorMethods)
        {
            if (_mesh != null && editorMethods != null && !Application.isPlaying)
            {
                if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Destroying Mesh");
                editorMethods.Destroy(_mesh);
            }

            ClearBuffers();
        }

        public bool ValidateTargRendererAndMeshAndResultSceneObj()
        {
            if (_resultSceneObject == null)
            {
                if (_LOG_LEVEL >= MB2_LogLevel.error) Debug.LogError("Result Scene Object was not set.");
                return false;
            }
            else {
                if (_targetRenderer == null)
                {
                    if (_LOG_LEVEL >= MB2_LogLevel.error) Debug.LogError("Target Renderer was not set.");
                    return false;
                }
                else {
                    if (_resultSceneObject != null && _targetRenderer.transform.parent != _resultSceneObject.transform)
                    {
                        if (_LOG_LEVEL >= MB2_LogLevel.error) Debug.LogError("Target Renderer game object is not a child of Result Scene Object.");
                        return false;
                    }
                    if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                    {
                        if (!(_targetRenderer is SkinnedMeshRenderer))
                        {
                            if (_LOG_LEVEL >= MB2_LogLevel.error) Debug.LogError("Render Type is skinned mesh renderer but Target Renderer is not.");
                            return false;
                        }
                        /*
                        if (((SkinnedMeshRenderer)_targetRenderer).sharedMesh != _mesh)
                        {
                            if (_LOG_LEVEL >= MB2_LogLevel.error) Debug.LogError("Target renderer mesh is not equal to mesh.");
                            return false;
                        }
                        */
                    }
                    if (settings.renderType == MB_RenderType.meshRenderer)
                    {
                        if (!(_targetRenderer is MeshRenderer))
                        {
                            if (_LOG_LEVEL >= MB2_LogLevel.error) Debug.LogError("Render Type is mesh renderer but Target Renderer is not.");
                            return false;
                        }
                        MeshFilter mf = _targetRenderer.GetComponent<MeshFilter>();
                        if (_mesh != mf.sharedMesh)
                        {
                            if (_LOG_LEVEL >= MB2_LogLevel.error) Debug.LogError("Target renderer mesh is not equal to mesh.");
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        OrderedDictionary BuildSourceMatsToSubmeshIdxMap(int numResultMats)
        {
            OrderedDictionary sourceMats2submeshIdx_map = new OrderedDictionary();
            //build the sourceMats to submesh index map
            for (int resultMatIdx = 0; resultMatIdx < numResultMats; resultMatIdx++)
            {
                List<Material> sourceMats = _textureBakeResults.GetSourceMaterialsUsedByResultMaterial(resultMatIdx);
                for (int j = 0; j < sourceMats.Count; j++)
                {
                    if (sourceMats[j] == null)
                    {
                        Debug.LogError("Found null material in source materials for combined mesh materials " + resultMatIdx);
                        return null;
                    }

                    if (!sourceMats2submeshIdx_map.Contains(sourceMats[j]))
                    {
                        sourceMats2submeshIdx_map.Add(sourceMats[j], resultMatIdx);
                    }
                }
            }

            return sourceMats2submeshIdx_map;
        }

        internal Renderer BuildSceneHierarchPreBake(MB3_MeshCombinerSingle mom, GameObject root, Mesh m, bool createNewChild = false, GameObject[] objsToBeAdded = null)
        {
            if (mom._LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log("Building Scene Hierarchy createNewChild=" + createNewChild);
            GameObject meshGO;
            MeshFilter mf = null;
            MeshRenderer mr = null;
            SkinnedMeshRenderer smr = null;
            Transform mt = null;
            if (root == null)
            {
                Debug.LogError("root was null.");
                return null;
            }
            if (mom.textureBakeResults == null)
            {
                Debug.LogError("textureBakeResults must be set.");
                return null;
            }
            if (root.GetComponent<Renderer>() != null)
            {
                Debug.LogError("root game object cannot have a renderer component");
                return null;
            }
            if (!createNewChild)
            {
                //try to find an existing child
                if (mom.targetRenderer != null && mom.targetRenderer.transform.parent == root.transform)
                {
                    mt = mom.targetRenderer.transform; //good setup
                }
                else
                {
                    Renderer[] rs = (Renderer[])root.GetComponentsInChildren<Renderer>(true);
                    if (rs.Length == 1)
                    {
                        if (rs[0].transform.parent != root.transform)
                        {
                            Debug.LogError("Target Renderer is not an immediate child of Result Scene Object. Try using a game object with no children as the Result Scene Object..");
                        }
                        mt = rs[0].transform;
                    }
                }
            }
            if (mt != null && mt.parent != root.transform)
            { //target renderer must be a child of root
                mt = null;
            }
            if (mt == null)
            {
                meshGO = new GameObject(mom.name + "-mesh");
                meshGO.transform.parent = root.transform;
                mt = meshGO.transform;
            }
            mt.parent = root.transform;
            meshGO = mt.gameObject;
            if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
            {
                MeshRenderer r = meshGO.GetComponent<MeshRenderer>();
                if (r != null) MB_Utility.Destroy(r);
                MeshFilter f = meshGO.GetComponent<MeshFilter>();
                if (f != null) MB_Utility.Destroy(f);
                smr = meshGO.GetComponent<SkinnedMeshRenderer>();
                if (smr == null) smr = meshGO.AddComponent<SkinnedMeshRenderer>();
            }
            else
            {
                SkinnedMeshRenderer r = meshGO.GetComponent<SkinnedMeshRenderer>();
                if (r != null) MB_Utility.Destroy(r);
                mf = meshGO.GetComponent<MeshFilter>();
                if (mf == null) mf = meshGO.AddComponent<MeshFilter>();
                mr = meshGO.GetComponent<MeshRenderer>();
                if (mr == null) mr = meshGO.AddComponent<MeshRenderer>();
            }
            if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
            {
                smr.bones = mom.GetBones();
                bool origVal = smr.updateWhenOffscreen;
                smr.updateWhenOffscreen = true;
                smr.updateWhenOffscreen = origVal;
            }

            _ConfigureSceneHierarch(mom, root, mr, mf, smr, m, objsToBeAdded);

            if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
            {
                return smr;
            }
            else
            {
                return mr;
            }
        }

        /*
         could be building for a multiMeshBaker or a singleMeshBaker, targetRenderer will be a scene object.
        */
        /*
        public static void BuildPrefabHierarchy(MB3_MeshCombinerSingle mom, GameObject instantiatedPrefabRoot, Mesh m, bool createNewChild = false, GameObject[] objsToBeAdded = null)
        {
            SkinnedMeshRenderer smr = null;
            MeshRenderer mr = null;
            MeshFilter mf = null;
            GameObject meshGO = new GameObject(mom.name + "-mesh");
            meshGO.transform.parent = instantiatedPrefabRoot.transform;
            Transform mt = meshGO.transform;
            
            mt.parent = instantiatedPrefabRoot.transform;
            meshGO = mt.gameObject;
            if (mom.settings.renderType == MB_RenderType.skinnedMeshRenderer)
            {
                MeshRenderer r = meshGO.GetComponent<MeshRenderer>();
                if (r != null) MB_Utility.Destroy(r);
                MeshFilter f = meshGO.GetComponent<MeshFilter>();
                if (f != null) MB_Utility.Destroy(f);
                smr = meshGO.GetComponent<SkinnedMeshRenderer>();
                if (smr == null) smr = meshGO.AddComponent<SkinnedMeshRenderer>();
            }
            else
            {
                SkinnedMeshRenderer r = meshGO.GetComponent<SkinnedMeshRenderer>();
                if (r != null) MB_Utility.Destroy(r);
                mf = meshGO.GetComponent<MeshFilter>();
                if (mf == null) mf = meshGO.AddComponent<MeshFilter>();
                mr = meshGO.GetComponent<MeshRenderer>();
                if (mr == null) mr = meshGO.AddComponent<MeshRenderer>();
            }
            if (mom.settings.renderType == MB_RenderType.skinnedMeshRenderer)
            {
                smr.bones = mom.GetBones();
                bool origVal = smr.updateWhenOffscreen;
                smr.updateWhenOffscreen = true;
                smr.updateWhenOffscreen = origVal;
                smr.sharedMesh = m;

                MB_BlendShape2CombinedMap srcMap = mom._targetRenderer.GetComponent<MB_BlendShape2CombinedMap>();
                if (srcMap != null)
                {
                    MB_BlendShape2CombinedMap targMap = meshGO.GetComponent<MB_BlendShape2CombinedMap>();
                    if (targMap == null) targMap = meshGO.AddComponent<MB_BlendShape2CombinedMap>();
                    targMap.srcToCombinedMap = srcMap.srcToCombinedMap;
                    for (int i = 0; i < targMap.srcToCombinedMap.combinedMeshTargetGameObject.Length; i++)
                    {
                        targMap.srcToCombinedMap.combinedMeshTargetGameObject[i] = meshGO;
                    }
                }
                
            }

            _ConfigureSceneHierarch(mom, instantiatedPrefabRoot, mr, mf, smr, m, objsToBeAdded);
            
            //First try to get the materials from the target renderer. This is because the mesh may have fewer submeshes than number of result materials if some of the submeshes had zero length tris.
            //If we have just baked then materials on the target renderer will be correct wheras materials on the textureBakeResult may not be correct.
            if (mom.targetRenderer != null)
            {
                Material[] sharedMats = new Material[mom.targetRenderer.sharedMaterials.Length];
                for (int i = 0; i < sharedMats.Length; i++)
                {
                    sharedMats[i] = mom.targetRenderer.sharedMaterials[i];
                }
                if (mom.settings.renderType == MB_RenderType.skinnedMeshRenderer)
                {
                    smr.sharedMaterial = null;
                    smr.sharedMaterials = sharedMats;
                }
                else
                {
                    mr.sharedMaterial = null;
                    mr.sharedMaterials = sharedMats;
                }
            }
        }
        */

        private static void _ConfigureSceneHierarch(MB3_MeshCombinerSingle mom, GameObject root, MeshRenderer mr, MeshFilter mf, SkinnedMeshRenderer smr, Mesh m, GameObject[] objsToBeAdded = null)
        {
            //assumes everything is set up correctly
            GameObject meshGO;
            if (mom.settings.renderType == MB_RenderType.skinnedMeshRenderer)
            {
                meshGO = smr.gameObject;
                //smr.sharedMesh = m; can't assign mesh for skinned mesh until it has skinning information
                smr.lightmapIndex = mom.GetLightmapIndex();
             
            }
            else {
                meshGO = mr.gameObject;
                mf.sharedMesh = m;
                mom._SetLightmapIndexIfPreserveLightmapping(mr); 
            }
            if (mom.settings.lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping || mom.settings.lightmapOption == MB2_LightmapOptions.generate_new_UV2_layout)
            {
                meshGO.isStatic = true;
            }

            //set layer and tag of combined object if all source objs have same layer
            if (objsToBeAdded != null && objsToBeAdded.Length > 0 && objsToBeAdded[0] != null)
            {
                bool tagsAreSame = true;
                bool layersAreSame = true;
                string tag = objsToBeAdded[0].tag;
                int layer = objsToBeAdded[0].layer;
                for (int i = 0; i < objsToBeAdded.Length; i++)
                {
                    if (objsToBeAdded[i] != null)
                    {
                        if (!objsToBeAdded[i].tag.Equals(tag)) tagsAreSame = false;
                        if (objsToBeAdded[i].layer != layer) layersAreSame = false;
                    }
                }
                if (tagsAreSame)
                {
                    root.tag = tag;
                    meshGO.tag = tag;
                }
                if (layersAreSame)
                {
                    root.layer = layer;
                    meshGO.layer = layer;
                }
            }
        }
        /// <summary>
        /// If the option to preserve current lightmapping is enabled, we use
        /// this function to add the MB_PreserveLightMapData script to the gameobject 
        /// to ensure that the lightmap index and lightmap scale offset are set to prevent the lightmap
        /// data from being lost. 
        /// </summary>
        /// <param name="tr"></param>
        private void _SetLightmapIndexIfPreserveLightmapping(Renderer tr)
        {
            tr.lightmapIndex = GetLightmapIndex();
            tr.lightmapScaleOffset = new Vector4(1, 1, 0, 0);
            if (settings.lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping)
            {
                MB_PreserveLightmapData script = tr.gameObject.GetComponent<MB_PreserveLightmapData>();
                if (script == null)
                {
                    script = tr.gameObject.AddComponent<MB_PreserveLightmapData>();
                }
                script.lightmapIndex = GetLightmapIndex();
                script.lightmapScaleOffset = new Vector4(1, 1, 0, 0);
            }
        }

        public void BuildSceneMeshObject(GameObject[] gos = null, bool createNewChild = false)
        {
            if (_resultSceneObject == null)
            {
                _resultSceneObject = new GameObject("CombinedMesh-" + name);
            }

            _targetRenderer = BuildSceneHierarchPreBake(this, _resultSceneObject, GetMesh(), createNewChild, gos);

        }

        //tests if a matrix has been mirrored
        bool IsMirrored(Matrix4x4 tm)
        {
            Vector3 x = tm.GetRow(0);
            Vector3 y = tm.GetRow(1);
            Vector3 z = tm.GetRow(2);
            x.Normalize(); y.Normalize(); z.Normalize();
            float an = Vector3.Dot(Vector3.Cross(x, y), z);
            return an >= 0 ? false : true;
        }

        public override void CheckIntegrity()
        {
            if (!MB_Utility.DO_INTEGRITY_CHECKS) return;

            if (_boneProcessor != null)
            {
                _boneProcessor.DB_CheckIntegrity();
            }

            //check blend shapes
            if (settings.doBlendShapes)
            {
                if (settings.renderType != MB_RenderType.skinnedMeshRenderer)
                {
                    Debug.LogError("Blend shapes can only be used with skinned meshes.");
                }
            }
        }

        public override List<Material> GetMaterialsOnTargetRenderer()
        {
            List<Material> outMats = new List<Material>();
            if (_targetRenderer != null)
            {
                outMats.AddRange(_targetRenderer.sharedMaterials);
            }
            return outMats;
        }

        bool _UseNativeArrayAPIorNot()
        {
#if UNITY_2020_2_OR_NEWER            
            if (settings.meshAPI == MB_MeshCombineAPIType.betaNativeArrayAPI)
            {
                return true;
            }
            else
            {
                return false;
            }
#else
            Debug.Assert(settings.meshAPI == MB_MeshCombineAPIType.simpleMeshAPI);
            return false;
#endif
        }

        public MB_IMeshCombinerSingle_BoneProcessor Create_BoneProcessor(bool doNativeArrays)
        {
            MB_IMeshCombinerSingle_BoneProcessor boneProcessor;
            if (doNativeArrays)
            {
#if UNITY_2020_2_OR_NEWER
                boneProcessor = new MB_MeshCombinerSingle_BoneProcessorNewAPI(this);
#else
                boneProcessor = new MB_MeshCombinerSingle_BoneProcessor(this);
#endif
            }
            else
            {
                boneProcessor = new MB_MeshCombinerSingle_BoneProcessor(this);
            }

            return boneProcessor;
        }
        
        public static IVertexAndTriangleProcessor Create_VertexAndTriangleProcessor(bool doNativeArrays)
        {
            IVertexAndTriangleProcessor _vertexAndTriangleProcessor = null;
            if (doNativeArrays)
            {
#if UNITY_2020_2_OR_NEWER
                _vertexAndTriangleProcessor = new VertexAndTriangleProcessorNativeArray();
#else
                _vertexAndTriangleProcessor = new VertexAndTriangleProcessor();
#endif
            }
            else
            {
                _vertexAndTriangleProcessor = new VertexAndTriangleProcessor();
            }
            
            return _vertexAndTriangleProcessor;
        }
        
        public static IMeshChannelsCacheTaggingInterface Create_MeshChannelsCache(bool doNativeArrays, MB2_LogLevel LOG_LEVEL, MB2_LightmapOptions lightmapOption)
        {
            IMeshChannelsCacheTaggingInterface _meshChannelsCache = null;
            if (doNativeArrays)
            {
#if UNITY_2020_2_OR_NEWER
                _meshChannelsCache = new MeshChannelsCache_NativeArray(LOG_LEVEL, lightmapOption);
#else
                _meshChannelsCache = new MeshChannelsCache(LOG_LEVEL, lightmapOption);
#endif
            }
            else
            {
                _meshChannelsCache = new MeshChannelsCache(LOG_LEVEL, lightmapOption);
            }

            return _meshChannelsCache;
        }
    }
}