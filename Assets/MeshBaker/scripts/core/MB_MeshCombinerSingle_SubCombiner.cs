using System.Collections.Generic;
using System;
using UnityEngine;

namespace DigitalOpus.MB.Core
{
    public partial class MB3_MeshCombinerSingle : MB3_MeshCombiner
    {
        /// <summary>
        /// The SubCombiners are the heart of the MeshCombiner. 
        /// These copy mesh data from source meshes into buffers that can be assigned to the combined mesh.
        /// 
        /// Assumes that all setup validation has been done.
        /// 
        /// Input is:
        ///     List of dgos to be added
        ///     List of dgos in the combined mesh (some of these might be being deleted)
        ///     
        /// Output:
        ///     Updated buffers that are ready to be assigned to the combined mesh.
        /// 
        /// IMPORTANT:
        ///    Some properties of the MeshCombiner like renderType are a "Setting" that is exposed in the inspector.
        ///    DON'T ACCESS THESE DIRECTLY. ACCESS THEM THROUGH THE "settings" property. We might be using a shared 
        ///    settings through the MeshBakerGrouper or MeshBakerSettingsScriptable object.
        /// 
        /// Validation, Error Handling & Dispose
        /// 
        ///     The call stack for AddDeleteGameObjects & UpdateGameObjects should look like:
        /// 
        ///     public MeshCombinerSimple.AddDeleteGameObjects overloads funnel into:
        ///         AddDeleteGameObjectsByID
        ///             _AddToCombined
        ///                 // Create helpers that might need disposal here
        ///                  try
        ///                  {
        ///                       success = __AddToCombined(_goToAdd, _goToDelete, disableRendererInSource, numResultMats, sourceMats2submeshIdx_map, sw);
        ///                                           // set up and validate DGOs in __AddToCombine
        ///                                           // if everything looks good
        ///                                           MeshCombinerSubCombinerLegacy._AddToCombined
        ///                                                  Only worries about copying stuff to buffers, modifying buffers
        ///                                                  Copying to mesh
        ///                  }
        ///                  catch
        ///                  {
        ///                      success = false;
        ///                      throw;
        ///                  }
        ///                  finally
        ///                  {
        ///                      Dispose of NativeArrays and helpers
        ///                  }
        ///                  
        /// </summary>
        public class MB_MeshCombinerSingle_SubCombiner
        {

            public static void instance2Combined_MapAdd(ref Dictionary<GameObject, MB_DynamicGameObject> _instance2combined_map, GameObject gameObjectID, MB_DynamicGameObject dgo)
            {
                _instance2combined_map.Add(gameObjectID, dgo);
            }

            public static void instance2Combined_MapRemove(ref Dictionary<GameObject, MB_DynamicGameObject> _instance2combined_map, GameObject gameObjectID)
            {
                _instance2combined_map.Remove(gameObjectID);
            }

            internal static bool _ShowHideGameObjects(MB3_MeshCombinerSingle c)
            {
                IVertexAndTriangleProcessor vertexAndTriProcessor = c._vertexAndTriProcessor;
                vertexAndTriProcessor.InitShowHide(c);
                return true;
            }


            internal static bool _AddToCombined(
                MB3_MeshCombinerSingle c,
                MB_MeshVertexChannelFlags newChannels,
                int totalAddVerts,
                int totalDeleteVerts,
                int numResultMats,
                int totalAddBlendShapes,
                int totalDeleteBlendShapes,
                int[] totalAddSubmeshTris,
                int[] totalDeleteSubmeshTris,
                int[] _goToDelete,
                List<MB_DynamicGameObject> toAddDGOs,
                GameObject[] _goToAdd,
                UVAdjuster_Atlas uvAdjuster,
                ref IVertexAndTriangleProcessor oldMeshData,
                System.Diagnostics.Stopwatch sw
                )
            {
                MB_IMeshCombinerSingle_BoneProcessor boneProcessor = c._boneProcessor;
                MB_MeshCombinerSingle_BlendShapeProcessor blendShapeProcessor = c._blendShapeProcessor;
                IMeshChannelsCacheTaggingInterface meshChannelCache = c._meshChannelsCache;
                MB_IMeshBakerSettings settings = c.settings;
                //ref MBBlendShape[] blendShapes = ref combiner.blendShapes;
                MB2_LogLevel LOG_LEVEL = c.LOG_LEVEL;
                
                MB2_TextureBakeResults textureBakeResults = c.textureBakeResults;
                List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh = c.mbDynamicObjectsInCombinedMesh;
                List<GameObject> objectsInCombinedMesh = c.objectsInCombinedMesh;
                Dictionary<GameObject, MB_DynamicGameObject> _instance2combined_map = c._instance2combined_map;

                int uvChannelWithExtraParameter;
                if (c.settings.assignToMeshCustomizer != null && c.settings.assignToMeshCustomizer is IAssignToMeshCustomizer_NativeArrays)
                {
                    uvChannelWithExtraParameter = ((IAssignToMeshCustomizer_NativeArrays)c.settings.assignToMeshCustomizer).UVchannelWithExtraParameter();
                }
                else
                {
                    uvChannelWithExtraParameter = -1;
                }

                c.db_addDeleteGameObjects_InitFromMeshCombiner.Start();
                int oldMeshDataNumVerts;
                int[] oldSubmeshTrisSize;
                if (!settings.clearBuffersAfterBake && mbDynamicObjectsInCombinedMesh.Count > 0)
                {
                    // There is data in the combined mesh. Initialize a buffer with this old data.
                    oldMeshData.InitFromMeshCombiner(c, newChannels, uvChannelWithExtraParameter);
                    oldMeshDataNumVerts = oldMeshData.GetVertexCount();
                    oldSubmeshTrisSize = oldMeshData.GetTriangleSizes();
                }
                else
                {
                    oldMeshDataNumVerts = 0;
                    oldSubmeshTrisSize = new int[numResultMats];
                }

                c.db_addDeleteGameObjects_InitFromMeshCombiner.Stop();
                c.db_addDeleteGameObjects_Init.Start();
                
                //STEP 2 to allocate new buffers and copy everything over
                int newVertSize = oldMeshDataNumVerts + totalAddVerts - totalDeleteVerts;
                
                int newBlendShapeSize = 0;
                if (settings.doBlendShapes)
                {
                    newBlendShapeSize = c.blendShapes.Length + totalAddBlendShapes - totalDeleteBlendShapes;
                }
                if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Verts adding:" + totalAddVerts + " deleting:" + totalDeleteVerts + " submeshes:" + numResultMats + " blendShapes:" + newBlendShapeSize);

                int[] newSubmeshTrisSize = new int[numResultMats];
                for (int i = 0; i < newSubmeshTrisSize.Length; i++)
                {
                    newSubmeshTrisSize[i] = oldSubmeshTrisSize[i] + totalAddSubmeshTris[i] - totalDeleteSubmeshTris[i];
                    if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("    submesh :" + i + " already contains:" + oldSubmeshTrisSize[i] + " tris to be Added:" + totalAddSubmeshTris[i] + " tris to be Deleted:" + totalDeleteSubmeshTris[i]);
                }

                if (newVertSize >= MBVersion.MaxMeshVertexCount())
                {
                    Debug.LogError("Cannot add objects. Resulting mesh will have more than " + MBVersion.MaxMeshVertexCount() + " vertices. Try using a Multi-MeshBaker component. This will split the combined mesh into several meshes. You don't have to re-configure the MB2_TextureBaker. Just remove the MB2_MeshBaker component and add a MB2_MultiMeshBaker component.");
                    return false;
                }

                //MBBlendShape[] nblendShapes = null;
                IVertexAndTriangleProcessor vertexAndTriProcessor = c._vertexAndTriProcessor;
                
                vertexAndTriProcessor.Init(c, newChannels, newVertSize, newSubmeshTrisSize, uvChannelWithExtraParameter, meshChannelCache, false, c.LOG_LEVEL);

                boneProcessor.AllocateAndSetupSMRDataStructures(toAddDGOs, mbDynamicObjectsInCombinedMesh, newVertSize, c._vertexAndTriProcessor);

                //if (settings.doBlendShapes) nblendShapes = new MBBlendShape[newBlendShapeSize];
                blendShapeProcessor.AllocateBlendShapeArrayIfNecessary(newBlendShapeSize);

                c.db_addDeleteGameObjects_Init.Stop();
                

                if (LOG_LEVEL >= MB2_LogLevel.debug) Debug.Log("Allocating buffers: " + ((MB_MeshVertexChannelFlags) vertexAndTriProcessor.channels) + "  vertexCount:" + newVertSize);

                mbDynamicObjectsInCombinedMesh.Sort();

                //copy existing arrays to narrays gameobj by gameobj omitting deleted ones
                int targBlendShapeIdx = 0;
                int targVidx = 0;
                int[] targSubmeshTidx = new int[numResultMats];
                int triangleIdxAdjustment = 0;

                c.db_addDeleteGameObjects_CopyArraysFromPreviousBakeBuffersToNewBuffers.Start();
                if (!settings.clearBuffersAfterBake && mbDynamicObjectsInCombinedMesh.Count > 0)
                {
                    Debug.Assert(oldMeshData.IsInitialized(), "Should have initialized this earlier.");
                    for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
                    {
                        MB_DynamicGameObject dgo = mbDynamicObjectsInCombinedMesh[i];
                        Debug.Assert(dgo._initialized);
                        if (!dgo._beingDeleted)
                        {
                            if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Copying obj in combined arrays idx:" + i, LOG_LEVEL);

                            vertexAndTriProcessor.CopyArraysFromPreviousBakeBuffersToNewBuffers(dgo, ref oldMeshData, targVidx, triangleIdxAdjustment, targSubmeshTidx, LOG_LEVEL);
                            if (settings.doBlendShapes) 
                            { 
                                blendShapeProcessor.CopyBlendShapesInCurrentMeshIfNecessary(ref targBlendShapeIdx, dgo); 
                            }

                            if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                            {
                                boneProcessor.CopyBoneWeightsFromMeshForDGOsInCombined(dgo, targVidx);
                            }

                            dgo.vertIdx = targVidx;
                            for (int subIdx = 0; subIdx < targSubmeshTidx.Length; subIdx++)
                            {
                                dgo.submeshTriIdxs[subIdx] = targSubmeshTidx[subIdx];
                                targSubmeshTidx[subIdx] += dgo.submeshNumTris[subIdx];
                            }

                            targVidx += dgo.numVerts;
                        }
                        else
                        {
                            if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Not copying obj: " + i, LOG_LEVEL);
                            triangleIdxAdjustment += dgo.numVerts;
                        }
                    }

                    if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                    {
                        boneProcessor.CopyBonesWeAreKeepingToNewBonesArrayAndAdjustBWIndexes(totalDeleteVerts);
                    }

                    //remove objects we are deleting
                    for (int i = mbDynamicObjectsInCombinedMesh.Count - 1; i >= 0; i--)
                    {
                        if (mbDynamicObjectsInCombinedMesh[i]._beingDeleted)
                        {
                            instance2Combined_MapRemove(ref _instance2combined_map, mbDynamicObjectsInCombinedMesh[i].gameObject);
                            objectsInCombinedMesh.RemoveAt(i);
                            mbDynamicObjectsInCombinedMesh.RemoveAt(i);
                        }
                    }
                }

                c.db_addDeleteGameObjects_CopyArraysFromPreviousBakeBuffersToNewBuffers.Stop();
                c.db_addDeleteGameObjects_CopyFromDGOMeshToBuffers.Start();

                if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                {
                    boneProcessor.InsertNewBonesIntoBonesArray();
                }

                //add new
                for (int i = 0; i < toAddDGOs.Count; i++)
                {
                    MB_DynamicGameObject dgo = toAddDGOs[i];
                    Debug.Assert(dgo._initialized);
                    GameObject go = _goToAdd[i];
                    int vertsIdx = targVidx;
                    //				Profile.StartProfile("TestNewNorm");
                    Mesh dgoMesh = dgo._mesh;

                    bool doUpdateBWInfo = false; // Not sure about this. This is false because it this function didn't call _boneProcessor.UpdateGameObjects_UpdateBWIndexes(dgo), but perhaps it should.
                    vertexAndTriProcessor.CopyFromDGOMeshToBuffers(dgo, targVidx, vertexAndTriProcessor.channels, true, doUpdateBWInfo, settings, boneProcessor, targSubmeshTidx,
                        textureBakeResults, uvAdjuster, LOG_LEVEL, meshChannelCache);

                    int numTriSets = dgoMesh.subMeshCount;
                    if (dgo.uvRects.Length < numTriSets)
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Mesh " + dgo.name + " has more submeshes than materials");
                    }
                    else if (dgo.uvRects.Length > numTriSets)
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Mesh " + dgo.name + " has fewer submeshes than materials");
                    }

                    if (settings.doBlendShapes)
                    {
                        blendShapeProcessor.CopyBlendShapesForNewMeshIfNecessary(ref targBlendShapeIdx, dgo, dgoMesh, meshChannelCache);
                    }

                    if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                    {
                        boneProcessor.AddBonesToNewBonesArrayAndAdjustBWIndexes1(dgo, vertsIdx);
                    }

                    dgo.vertIdx = targVidx;

                    instance2Combined_MapAdd(ref _instance2combined_map, go, dgo);
                    objectsInCombinedMesh.Add(go);
                    mbDynamicObjectsInCombinedMesh.Add(dgo);

                    targVidx += dgo.numVerts;

                    // Debug.Log("Adding New: " + dgo.gameObject + "  targBoneWeightIdx:" + boneProcessor.targBoneWeightIdx + "  adding: " + dgo.numBoneWeights + "   total: " + (boneProcessor.targBoneWeightIdx + dgo.numBoneWeights));
                    for (int j = 0; j < dgo._tmpSubmeshTris.Length; j++) dgo._tmpSubmeshTris[j] = null;
                    dgo._tmpSubmeshTris = null;

                    if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("Added to combined:" + dgo.name + " verts:" + vertexAndTriProcessor.GetVertexCount() + " bindPoses:" + boneProcessor.GetNewBonesSize(), LOG_LEVEL);
                }

                if (settings.lightmapOption == MB2_LightmapOptions.copy_UV2_unchanged_to_separate_rects)
                {
                    vertexAndTriProcessor.CopyUV2unchangedToSeparateRects(mbDynamicObjectsInCombinedMesh, settings.uv2UnwrappingParamsPackMargin);
                }

                if (LOG_LEVEL >= MB2_LogLevel.debug) MB2_Log.LogDebug("===== _addToCombined completed. Verts in buffer: " + vertexAndTriProcessor.GetVertexCount() + " time(ms): " + sw.ElapsedMilliseconds, LOG_LEVEL);

                c.db_addDeleteGameObjects_CopyFromDGOMeshToBuffers.Stop();
                return true;
            }

            public static bool _UpdateGameObjects(MB3_MeshCombinerSingle combiner, List<MB_DynamicGameObject> dgosToUpdate, 
                                            MB_MeshVertexChannelFlags newChannels,
                                            bool updateVertices, bool updateNormals, bool updateTangents,
                                            bool updateUV, bool updateUV2, bool updateUV3, bool updateUV4, bool updateUV5, bool updateUV6, bool updateUV7, bool updateUV8,
                                            bool updateColors, bool updateSkinningInfo,
                                            UVAdjuster_Atlas uVAdjuster, MB2_LogLevel LOG_LEVEL)
            {
                IMeshChannelsCacheTaggingInterface meshChannelCache = combiner._meshChannelsCache;
                MB_IMeshBakerSettings settings = combiner.settings;
                IVertexAndTriangleProcessor vertexDataInCombiner = combiner._vertexAndTriProcessor;

                int uvChannelWithExtraParameter;
                if (combiner.settings.assignToMeshCustomizer != null && combiner.settings.assignToMeshCustomizer is IAssignToMeshCustomizer_NativeArrays)
                {
                    uvChannelWithExtraParameter = ((IAssignToMeshCustomizer_NativeArrays) combiner.settings.assignToMeshCustomizer).UVchannelWithExtraParameter();
                }
                else
                {
                    uvChannelWithExtraParameter = -1;
                }

                vertexDataInCombiner.Init(combiner, newChannels, combiner._mesh.vertexCount, new int[0], uvChannelWithExtraParameter, combiner._meshChannelsCache, true, LOG_LEVEL);
                
                if (settings.renderType == MB_RenderType.skinnedMeshRenderer &&
                    updateSkinningInfo)
                {
                    combiner._boneProcessor.UpdateGameObjects_ReadBoneWeightInfoFromCombinedMesh();
                }

                MB_MeshVertexChannelFlags channelsToUpdate = (updateVertices ? MB_MeshVertexChannelFlags.vertex : 0u) |
                    (updateNormals ? MB_MeshVertexChannelFlags.normal : MB_MeshVertexChannelFlags.none) |
                    (updateTangents ? MB_MeshVertexChannelFlags.tangent : MB_MeshVertexChannelFlags.none) |
                    (updateColors ? MB_MeshVertexChannelFlags.colors : MB_MeshVertexChannelFlags.none) |
                    (updateUV ? MB_MeshVertexChannelFlags.uv0 : MB_MeshVertexChannelFlags.none) |
                    (updateUV2 ? MB_MeshVertexChannelFlags.uv2 : MB_MeshVertexChannelFlags.none) |
                    (updateUV3 ? MB_MeshVertexChannelFlags.uv3 : MB_MeshVertexChannelFlags.none) |
                    (updateUV4 ? MB_MeshVertexChannelFlags.uv4 : MB_MeshVertexChannelFlags.none) |
                    (updateUV5 ? MB_MeshVertexChannelFlags.uv5 : MB_MeshVertexChannelFlags.none) |
                    (updateUV6 ? MB_MeshVertexChannelFlags.uv6 : MB_MeshVertexChannelFlags.none) |
                    (updateUV7 ? MB_MeshVertexChannelFlags.uv7 : MB_MeshVertexChannelFlags.none) |
                    (updateUV8 ? MB_MeshVertexChannelFlags.uv8 : MB_MeshVertexChannelFlags.none); // This is a subset of channels in the combined mesh.
                
                for (int i = 0; i < dgosToUpdate.Count; i++)
                {
                    MB_DynamicGameObject dgo = dgosToUpdate[i];
                    bool doUpdateBWInfo = settings.renderType == MB_RenderType.skinnedMeshRenderer && updateSkinningInfo;
                    vertexDataInCombiner.CopyFromDGOMeshToBuffers(dgo, dgo.vertIdx, channelsToUpdate, false, doUpdateBWInfo, settings, combiner._boneProcessor, null, combiner.textureBakeResults, uVAdjuster, LOG_LEVEL, meshChannelCache);
                    dgo.UnInitialize();
                }

				combiner._bakeStatus = MeshCombiningStatus.readyForApply;
                if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                {
                    ((SkinnedMeshRenderer)combiner.targetRenderer).sharedMesh = null;
                    ((SkinnedMeshRenderer)combiner.targetRenderer).sharedMesh = combiner._mesh;
                }

                return true;
            }


            public static bool Apply(MB3_MeshCombinerSingle combiner, GenerateUV2Delegate uv2GenerationMethod)
            {
                MB_IMeshBakerSettings settings = combiner.settings;
                bool doBones = false;
                if (settings.renderType == MB_RenderType.skinnedMeshRenderer) doBones = true;
                return Apply(combiner, true, true, settings.doNorm, settings.doTan,
                    settings.doUV, MeshBakerSettingsUtility.DoUV2getDataFromSourceMeshes(ref settings), settings.doUV3, settings.doUV4, settings.doUV5, settings.doUV6, settings.doUV7, settings.doUV8,
                    settings.doCol, doBones, settings.doBlendShapes, false, uv2GenerationMethod);
            }

            public static bool Apply(MB3_MeshCombinerSingle combiner, bool triangles,
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
                return Apply(combiner, triangles, vertices, normals, tangents,
                    uvs, uv2, uv3, uv4,
                    false, false, false, false,
                    colors, bones, blendShapesFlag, false, uv2GenerationMethod);
            }

            internal static bool Apply(MB3_MeshCombinerSingle combiner, bool triangles,
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
                         bool suppressClearMesh = false,
                         GenerateUV2Delegate uv2GenerationMethod = null)
            {
                MB2_LogLevel LOG_LEVEL = combiner.LOG_LEVEL;
                MB2_ValidationLevel _validationLevel = combiner._validationLevel;
                MB_IMeshBakerSettings settings = combiner.settings;

                {
                    bool error = false;
                    // Validation
                    if (bones && combiner._boneProcessor == null)
                    {
                        Debug.LogError("Apply was called with 'bones = true', but the meshCombiner did not contain valid bone data. Was AddDelete(...), Update(...) or ShowHide() called with 'renderType = skinnedMeshRenderer'?");
                        error = true;
                    }

                    if (_validationLevel >= MB2_ValidationLevel.quick && !combiner.ValidateTargRendererAndMeshAndResultSceneObj())
                    {
                        error = true;
                    }

                    if (combiner._bakeStatus != MeshCombiningStatus.readyForApply)
                    {
                        Debug.LogError("Apply was called when combiner was not in 'readyForApply' state. Did you call AddDelete(), Update() or ShowHide()");
                        error = true;
                    }

                    if (combiner._vertexAndTriProcessor != null && 
                        combiner._vertexAndTriProcessor.IsDisposed() && 
                        combiner._vertexAndTriProcessor.IsInitialized())
                    {
                        Debug.LogError("Apply was called with bad meshDataBuffer");
                        error = true;
                    }

                    if (error) return false;
                }

                Mesh mesh = combiner._mesh;
                Renderer targetRenderer = combiner.targetRenderer;
                MB2_TextureBakeResults _textureBakeResults = combiner._textureBakeResults;
                MB2_TextureBakeResults textureBakeResults = combiner.textureBakeResults;



                System.Diagnostics.Stopwatch sw = null;
                if (LOG_LEVEL >= MB2_LogLevel.debug)
                {
                    sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                }

                if (mesh != null)
                {
                    {
                        IVertexAndTriangleProcessor vertexAndTriProcessor = combiner._vertexAndTriProcessor;
                        if (LOG_LEVEL >= MB2_LogLevel.trace)
                        {
                            Debug.Log(String.Format("Apply called:\n" +
                                " tri={0}\n vert={1}\n norm={2}\n tan={3}\n uv={4}\n col={5}\n uv3={6}\n uv4={7}\n uv2={8}\n bone={9}\n blendShape{10}\n meshID={11}\n",
                                triangles, vertices, normals, tangents, uvs, colors, uv3, uv4, uv2, bones, blendShapesFlag, mesh.GetInstanceID()));
                        }

                        // If ApplyShowHide we don't want to clear the mesh.
                        if (!suppressClearMesh &&
                            (triangles || mesh.vertexCount != vertexAndTriProcessor.GetVertexCount()))
                        {
                            bool justClearTriangles = triangles && !vertices && !normals && !tangents && !uvs && !colors && !uv3 && !uv4 && !uv2 && !bones;
                            MBVersion.SetMeshIndexFormatAndClearMesh(mesh, vertexAndTriProcessor.GetVertexCount(), vertices, justClearTriangles);
                        }

                        MB_MeshVertexChannelFlags channelsToWriteToMesh = MB_MeshVertexChannelFlags.none;

                        bool doWriteTrisToMesh = false;
                        if (vertices)
                        {
                            Debug.Assert((vertexAndTriProcessor.channels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex);
                            // Vector3 targetRenderPosition_wld;
                            // meshDataBuffers.AdjustVertsToWriteAccordingToPivotPositionIfNecessary(settings.pivotLocationType, settings.renderType, settings.clearBuffersAfterBake, settings.pivotLocation, out targetRenderPosition_wld, out verts2Write);

                            channelsToWriteToMesh = channelsToWriteToMesh | MB_MeshVertexChannelFlags.vertex;
                            // mesh.vertices = verts2Write;
                        }

                        if (triangles && _textureBakeResults)
                        {
                            if (_textureBakeResults == null)
                            {
                                Debug.LogError("Texture Bake Result was not set.");
                            }
                            else
                            {
                                doWriteTrisToMesh = true;
                            }
                        }

                        if (normals)
                        {
                            Debug.Assert((vertexAndTriProcessor.channels & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal);
                            if (settings.doNorm)
                            {
                                channelsToWriteToMesh = channelsToWriteToMesh | MB_MeshVertexChannelFlags.normal;
                                //_mesh.normals = meshDataBuffers.nnormals;
                            }
                            else { Debug.LogError("normal flag was set in Apply but MeshBaker didn't generate normals"); }
                        }

                        if (tangents)
                        {
                            Debug.Assert((vertexAndTriProcessor.channels & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent);
                            if (settings.doTan)
                            {
                                channelsToWriteToMesh = channelsToWriteToMesh | MB_MeshVertexChannelFlags.tangent;
                                //_mesh.tangents = meshDataBuffers.ntangents; 
                            }
                            else { Debug.LogError("tangent flag was set in Apply but MeshBaker didn't generate tangents"); }
                        }
                        if (colors)
                        {
                            if (settings.doCol)
                            {
                                Debug.Assert((vertexAndTriProcessor.channels & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors);

                                channelsToWriteToMesh = channelsToWriteToMesh | MB_MeshVertexChannelFlags.colors;

                            }
                            else { Debug.LogError("color flag was set in Apply but MeshBaker didn't generate colors"); }
                        }
                        if (uvs)
                        {
                            if (settings.doUV)
                            {
                                Debug.Assert((vertexAndTriProcessor.channels & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0);
                                channelsToWriteToMesh = channelsToWriteToMesh | MB_MeshVertexChannelFlags.uv0;
                            }
                            else { Debug.LogError("uv flag was set in Apply but MeshBaker didn't generate uvs"); }
                        }
                        if (uv2)
                        {
                            if (MeshBakerSettingsUtility.DoUV2getDataFromSourceMeshes(ref settings))
                            {
                                Debug.Assert((vertexAndTriProcessor.channels & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2);
                                channelsToWriteToMesh = channelsToWriteToMesh | MB_MeshVertexChannelFlags.uv2;
                            }
                            else { Debug.LogError("uv2 flag was set in Apply but lightmapping option was set to " + settings.lightmapOption); }
                        }
                        if (uv3)
                        {
                            if (settings.doUV3)
                            {
                                Debug.Assert((vertexAndTriProcessor.channels & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3);
                                channelsToWriteToMesh = channelsToWriteToMesh | MB_MeshVertexChannelFlags.uv3;
                            }
                            else { Debug.LogError("uv3 flag was set in Apply but MeshBaker didn't generate uv3s"); }
                        }

                        if (uv4)
                        {
                            if (settings.doUV4)
                            {
                                Debug.Assert((vertexAndTriProcessor.channels & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4);
                                channelsToWriteToMesh = channelsToWriteToMesh | MB_MeshVertexChannelFlags.uv4;
                            }
                            else { Debug.LogError("uv4 flag was set in Apply but MeshBaker didn't generate uv4s"); }
                        }

                        if (uv5)
                        {
                            if (settings.doUV5)
                            {
                                Debug.Assert((vertexAndTriProcessor.channels & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5);
                                channelsToWriteToMesh = channelsToWriteToMesh | MB_MeshVertexChannelFlags.uv5;
                            }
                            else { Debug.LogError("uv5 flag was set in Apply but MeshBaker didn't generate uv5s"); }
                        }

                        if (uv6)
                        {
                            if (settings.doUV6)
                            {
                                Debug.Assert((vertexAndTriProcessor.channels & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6);
                                channelsToWriteToMesh = channelsToWriteToMesh | MB_MeshVertexChannelFlags.uv6;
                            }
                            else { Debug.LogError("uv6 flag was set in Apply but MeshBaker didn't generate uv6s"); }
                        }

                        if (uv7)
                        {
                            if (settings.doUV7)
                            {
                                Debug.Assert((vertexAndTriProcessor.channels & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7);
                                channelsToWriteToMesh = channelsToWriteToMesh | MB_MeshVertexChannelFlags.uv7;
                            }
                            else { Debug.LogError("uv7 flag was set in Apply but MeshBaker didn't generate uv7s"); }
                        }

                        if (uv8)
                        {
                            if (settings.doUV8)
                            {
                                Debug.Assert((vertexAndTriProcessor.channels & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8);
                                channelsToWriteToMesh = channelsToWriteToMesh | MB_MeshVertexChannelFlags.uv8;
                            }
                            else { Debug.LogError("uv8 flag was set in Apply but MeshBaker didn't generate uv8s"); }
                        }

                        if (bones)
                        {
                            combiner._boneProcessor.ApplySMRdataToMeshToBuffer();
                        }

                        {
                            SerializableIntArray[] submeshTrisToUse;
                            int numNonZeroLengthSubmeshes;
                            BufferDataFromPreviousBake serializableBufferData;
                            vertexAndTriProcessor.AssignBuffersToMesh(mesh, settings, textureBakeResults,
                                channelsToWriteToMesh, doWriteTrisToMesh, settings.assignToMeshCustomizer,
                                combiner.mbDynamicObjectsInCombinedMesh, out serializableBufferData, out submeshTrisToUse, out numNonZeroLengthSubmeshes);
                            vertexAndTriProcessor.TransferOwnershipOfSerializableBuffersToCombiner(combiner, vertexAndTriProcessor.channels, serializableBufferData);
                            vertexAndTriProcessor.Dispose();

                            if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex)
                            {
                                targetRenderer.transform.position = serializableBufferData.meshVerticesShift;
                            }

                            if (doWriteTrisToMesh)
                            {
                                _UpdateMaterialsOnTargetRenderer(combiner.textureBakeResults, combiner.targetRenderer, submeshTrisToUse, numNonZeroLengthSubmeshes);
                            }
                        }
                    }

                    bool do_generate_new_UV2_layout = false;
                    {
                        // Generate new UV2 layout stuff.
                        if (settings.renderType != MB_RenderType.skinnedMeshRenderer && settings.lightmapOption == MB2_LightmapOptions.generate_new_UV2_layout)
                        {
                            if (uv2GenerationMethod != null)
                            {
                                uv2GenerationMethod(mesh, settings.uv2UnwrappingParamsHardAngle, settings.uv2UnwrappingParamsPackMargin);
                                if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log("generating new UV2 layout for the combined mesh ");
                            }
                            else
                            {
                                Debug.LogError("No GenerateUV2Delegate method was supplied. UV2 cannot be generated.");
                            }
                            do_generate_new_UV2_layout = true;
                        }
                        else if (settings.renderType == MB_RenderType.skinnedMeshRenderer && settings.lightmapOption == MB2_LightmapOptions.generate_new_UV2_layout)
                        {
                            if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("UV2 cannot be generated for SkinnedMeshRenderer objects.");
                        }
                        if (settings.renderType != MB_RenderType.skinnedMeshRenderer && settings.lightmapOption == MB2_LightmapOptions.generate_new_UV2_layout && do_generate_new_UV2_layout == false)
                        {
                            Debug.LogError("Failed to generate new UV2 layout. Only works in editor.");
                        }
                    }

                    if (bones)
                    {
                        combiner._boneProcessor.ApplySMRdataToMesh(combiner, mesh);
                        combiner._boneProcessor.Dispose();
                        combiner._boneProcessor = null;
                    }

                    if (blendShapesFlag)
                    {
                        combiner._blendShapeProcessor.AssignNewBlendShapesToCombinerIfNecessary();
                        if (settings.smrMergeBlendShapesWithSameNames)
                        {
                            combiner._blendShapeProcessor.ApplyBlendShapeFramesToMeshAndBuildMap_MergeBlendShapesWithTheSameName(combiner._mesh.vertexCount);
                        }
                        else
                        {
                            combiner._blendShapeProcessor.ApplyBlendShapeFramesToMeshAndBuildMap(combiner._mesh.vertexCount);
                        }

                        combiner._blendShapeProcessor.Dispose();
                        combiner._blendShapeProcessor = null;
                    }

                    if (triangles || vertices)
                    {
                        if (LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log("recalculating bounds on mesh.");
                        mesh.RecalculateBounds();
                    }

                    if (settings.optimizeAfterBake && !Application.isPlaying)
                    {
                        MBVersion.OptimizeMesh(mesh);
                    }

                    combiner._SetLightmapIndexIfPreserveLightmapping(targetRenderer);

                    if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                    {
                        if (combiner._mesh.vertexCount == 0)
                        {
                            if (LOG_LEVEL >= MB2_LogLevel.debug)
                            {
                                Debug.Log(" combined mesh had zero vertices. Disabling combined SkinnedMeshRenderer.");
                            }
                            //disable mesh renderer to avoid skinning warning
                            targetRenderer.enabled = false;
                        }
                        else
                        {
                            targetRenderer.enabled = true;
                            SkinnedMeshRenderer smr = (SkinnedMeshRenderer)targetRenderer;

                            //needed so that updating local bounds will take affect
                            bool uwos = smr.updateWhenOffscreen;
                            smr.updateWhenOffscreen = true;
                            smr.updateWhenOffscreen = uwos;

                            // Needed because it appears that the SkinnedMeshRenderer caches stuff when the mesh is assigned.
                            // It updates its cache on assignment. In 2019.4.28+ it appears that a check was added so that if the same mesh is assigned to the SMR then the update is skipped. 
                            // Generates errors (and mesh is invisible): d3d11: buffer size can not be zero
                            smr.sharedMesh = null;
                            smr.sharedMesh = mesh;

                            // {
                            //    Debug.Log("Assigning combiner.bones to smr.bones: " + combiner.bones.Length);
                            //    for (int i = 0; i < combiner.bones.Length; i++)
                            //    {
                            //        Debug.Log(i + "      " + combiner.bones[i]);
                            //    }
                            // }

                            smr.bones = combiner.bones;

                            if (LOG_LEVEL >= MB2_LogLevel.debug)
                            {
                                Debug.Log(" Applying bones and mesh to SkinnedMeshRenderer component  numbones: " + combiner.bones.Length);
                            }

                            MB3_MeshCombinerSingle.UpdateSkinnedMeshApproximateBoundsFromBoundsStatic(combiner.objectsInCombinedMesh, smr);
                        }
                    }

                    combiner._boneProcessor = null;
                }
                else
                {
                    Debug.LogError("Need to add objects to this meshbaker before calling Apply or ApplyAll");
                }
                if (LOG_LEVEL >= MB2_LogLevel.debug)
                {
                    Debug.Log("Apply Complete time: " + sw.ElapsedMilliseconds + " vertices: " + mesh.vertexCount);
                }

                combiner._bakeStatus = MeshCombiningStatus.preAddDeleteOrUpdate;

                if (settings.clearBuffersAfterBake)
                {
                    combiner.ClearBuffers();
                }

                return true;
            }

            public static bool ApplyShowHide(MB3_MeshCombinerSingle combiner)
            {
                MB_IMeshBakerSettings settings = combiner.settings;
                Renderer targetRenderer = combiner.targetRenderer;

                {
                        bool error = false;

                        if (combiner._bakeStatus != MeshCombiningStatus.readyForApply)
                        {
                            Debug.LogError("Apply was called when combiner was not in 'readyForApply' state. Did you call AddDelete(), Update() or ShowHide()");
                            error = true;
                        }

                        if (combiner._vertexAndTriProcessor != null &&
                            combiner._vertexAndTriProcessor.IsDisposed() &&
                            combiner._vertexAndTriProcessor.IsInitialized())
                        {
                            Debug.LogError("Apply was called with bad meshDataBuffer");
                            error = true;
                        }

                        if (error) return false;
                }

                {
                    IVertexAndTriangleProcessor vertexAndTriProcessor = combiner._vertexAndTriProcessor;
                    SerializableIntArray[] submeshTrisToUse;
                    int numNonZeroLengthSubmeshes;
                    BufferDataFromPreviousBake serializableBufferData = combiner.bufferDataFromPrevious;
                    vertexAndTriProcessor.AssignTriangleDataForSubmeshes_ShowHide(combiner._mesh, combiner.mbDynamicObjectsInCombinedMesh, ref serializableBufferData, out submeshTrisToUse, out numNonZeroLengthSubmeshes);
                    vertexAndTriProcessor.TransferOwnershipOfSerializableBuffersToCombiner(combiner, MB_MeshVertexChannelFlags.none, serializableBufferData);
                    vertexAndTriProcessor.Dispose();

                    _UpdateMaterialsOnTargetRenderer(combiner.textureBakeResults, combiner.targetRenderer, submeshTrisToUse, numNonZeroLengthSubmeshes);
                }

                if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
                {
                    if (combiner._mesh.vertexCount == 0)
                    {
                        if (combiner.LOG_LEVEL >= MB2_LogLevel.debug)
                        {
                            Debug.Log(" combined mesh had zero vertices. Disabling combined SkinnedMeshRenderer.");
                        }
                        //disable mesh renderer to avoid skinning warning
                        targetRenderer.enabled = false;
                    }
                    else
                    {
                        targetRenderer.enabled = true;
                        SkinnedMeshRenderer smr = (SkinnedMeshRenderer)targetRenderer;

                        //needed so that updating local bounds will take affect
                        bool uwos = smr.updateWhenOffscreen;
                        smr.updateWhenOffscreen = true;
                        smr.updateWhenOffscreen = uwos;

                        // Needed because it appears that the SkinnedMeshRenderer caches stuff when the mesh is assigned.
                        // It updates its cache on assignment. In 2019.4.28+ it appears that a check was added so that if the same mesh is assigned to the SMR then the update is skipped. 
                        // Generates errors (and mesh is invisible): d3d11: buffer size can not be zero
                        smr.sharedMesh = null;
                        smr.sharedMesh = combiner._mesh;

                        // {
                        //    Debug.Log("Assigning combiner.bones to smr.bones: " + combiner.bones.Length);
                        //    for (int i = 0; i < combiner.bones.Length; i++)
                        //    {
                        //        Debug.Log(i + "      " + combiner.bones[i]);
                        //    }
                        // }

                        smr.bones = combiner.bones;

                        if (combiner.LOG_LEVEL >= MB2_LogLevel.debug)
                        {
                            Debug.Log(" Applying bones and mesh to SkinnedMeshRenderer component  numbones: " + combiner.bones.Length);
                        }

                        MB3_MeshCombinerSingle.UpdateSkinnedMeshApproximateBoundsFromBoundsStatic(combiner.objectsInCombinedMesh, smr);
                    }
                }

                if (combiner.LOG_LEVEL >= MB2_LogLevel.trace) Debug.Log("ApplyShowHide");
                return true;
            }
        }

        private static void _UpdateMaterialsOnTargetRenderer(MB2_TextureBakeResults textureBakeResults, Renderer targetRenderer, SerializableIntArray[] subTris, int numNonZeroLengthSubmeshTris)
        {
            //zero length triangle arrays in mesh cause errors. have excluded these sumbeshes so must exclude these materials
            if (subTris.Length != textureBakeResults.NumResultMaterials()) Debug.LogError("Mismatch between number of submeshes and number of result materials " + subTris.Length + " " + textureBakeResults.NumResultMaterials());
            Material[] resMats = new Material[numNonZeroLengthSubmeshTris];
            int submeshIdx = 0;
            for (int i = 0; i < subTris.Length; i++)
            {
                if (subTris[i].data.Length > 0)
                {
                    resMats[submeshIdx] = textureBakeResults.GetCombinedMaterialForSubmesh(i);
                    submeshIdx++;
                }
            }
            targetRenderer.materials = resMats;
        }
    }
}
