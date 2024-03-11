using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

namespace DigitalOpus.MB.Core
{

    public partial class MB3_MeshCombinerSingle : MB3_MeshCombiner
    {
        public class MB_MeshCombinerSingle_BoneProcessor : MB_IMeshCombinerSingle_BoneProcessor
        {
            MB3_MeshCombinerSingle combiner;
            List<MB_DynamicGameObject>[] boneIdx2dgoMap = null;
            HashSet<int> boneIdxsToDelete = new HashSet<int>();
            HashSet<BoneAndBindpose> bonesToAdd = new HashSet<BoneAndBindpose>();
            Dictionary<BoneAndBindpose, int> boneAndBindPose2idx = new Dictionary<BoneAndBindpose, int>();

            Transform[] oldBonesPreviousBake;
            Matrix4x4[] oldBindPosesPreviousBake;

            Transform[] nbones;
            Matrix4x4[] nbindPoses;
            BoneWeight[] nboneWeights;

            //unity won't serialize these
            BoneWeight[] boneWeights = new BoneWeight[0];

            private int _newBonesStartAtIdx;

            private bool _disposed;

            protected void Dispose(bool disposing)
            {
                if (_disposed) return;
                if (disposing)
                {
                    combiner = null;
                    boneIdx2dgoMap = null;
                    boneIdxsToDelete = null;
                    bonesToAdd = null;
                    boneAndBindPose2idx = null;
                    boneWeights = null;

                }

                _disposed = true;
            }

            public void Dispose()
            {
                Dispose(true);
            }

            public int GetNewBonesSize()
            {
                return nbones == null ? 0 : nbones.Length;
            }

            public MB_MeshCombinerSingle_BoneProcessor(MB3_MeshCombinerSingle cm)
            {
                Debug.Assert(cm._meshChannelsCache is MeshChannelsCache, "MB_MeshCombinerSingle_BoneProcessor only works with MeshChannelsCache.");
                combiner = cm;
                oldBonesPreviousBake = combiner.bones;
                oldBindPosesPreviousBake = combiner.bindPoses;
            }

            
            public HashSet<MB3_MeshCombinerSingle.BoneAndBindpose> GetBonesToAdd()
            {
                return bonesToAdd;
            }
            

            public int GetNumBonesToDelete()
            {
                return boneIdxsToDelete.Count;
            }

            private bool _didSetup = false;

            public void BuildBoneIdx2DGOMapIfNecessary(int[] _goToDelete)
            {
                Debug.Assert(!_disposed);
                _didSetup = false;
                if (combiner.settings.renderType == MB_RenderType.skinnedMeshRenderer)
                {
                    if (_goToDelete != null && _goToDelete.Length > 0)
                    {
                        boneIdx2dgoMap = _buildBoneIdx2dgoMap();
                    }

                    for (int i = 0; i < oldBonesPreviousBake.Length; i++)
                    {
                        BoneAndBindpose bn = new BoneAndBindpose(oldBonesPreviousBake[i], oldBindPosesPreviousBake[i]);
                        boneAndBindPose2idx.Add(bn, i);
                        //myBone2idx.Add(combiner.bones[i], i);
                    }

                    _didSetup = true;
                }
            }

            public void RemoveBonesForDgosWeAreDeleting(MB_DynamicGameObject dgo)
            {
                Debug.Assert(!_disposed && _didSetup);
                Debug.Assert(combiner.settings.renderType == MB_RenderType.skinnedMeshRenderer);
                // We could be working with adding and deleting smr body parts from the same rig. Different smrs will share 
                // the same bones. Track if we need to delete a bone or not.
                for (int j = 0; j < dgo.indexesOfBonesUsed.Length; j++)
                {
                    int idxOfUsedBone = dgo.indexesOfBonesUsed[j];
                    List<MB_DynamicGameObject> dgosThatUseBone = boneIdx2dgoMap[idxOfUsedBone];
                    if (dgosThatUseBone.Contains(dgo))
                    {
                        dgosThatUseBone.Remove(dgo);
                        if (dgosThatUseBone.Count == 0)
                        {
                            boneIdxsToDelete.Add(idxOfUsedBone);
                        }
                    }
                }
            }


            public void AllocateAndSetupSMRDataStructures(List<MB3_MeshCombinerSingle.MB_DynamicGameObject> toAddDGOs, List<MB3_MeshCombinerSingle.MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, int newVertSize, IVertexAndTriangleProcessor vertexAndTriangleProcessor)
            {
                if (combiner.settings.renderType == MB_RenderType.skinnedMeshRenderer)
                {
                    Debug.Assert(!_disposed && _didSetup);
                    _CollectSkinningDataForDGOsInCombinedMesh(toAddDGOs);
                    int newBonesSize = GetNewBonesLength();
                    nbones = new Transform[newBonesSize];
                    nbindPoses = new Matrix4x4[newBonesSize];
                    nboneWeights = new BoneWeight[newVertSize];

                    _newBonesStartAtIdx = oldBindPosesPreviousBake.Length - GetNumBonesToDelete();
                    boneWeights = combiner._mesh.boneWeights;
                    Debug.Assert(boneWeights.Length == combiner._mesh.vertexCount, "Could not retrieve BoneWeight data from mesh");
                }
            }

            public void UpdateGameObjects_ReadBoneWeightInfoFromCombinedMesh()
            {
                if (combiner.settings.renderType == MB_RenderType.skinnedMeshRenderer)
                {
                    Debug.Assert(!_disposed && _didSetup);
                    boneWeights = combiner._mesh.boneWeights;

                    //MeshBaker was baked using old system that had duplicated bones. Upgrade to new system
                    //need to build indexesOfBonesUsed maps for dgos
                    if (combiner.mbDynamicObjectsInCombinedMesh.Count > 0 &&
                        combiner.mbDynamicObjectsInCombinedMesh[0].indexesOfBonesUsed.Length == 0 &&
                        combiner.settings.renderType == MB_RenderType.skinnedMeshRenderer &&
                        boneWeights != null && boneWeights.Length > 0)
                    {
                        for (int i = 0; i < combiner.mbDynamicObjectsInCombinedMesh.Count; i++)
                        {
                            MB_DynamicGameObject dgo = combiner.mbDynamicObjectsInCombinedMesh[i];
                            HashSet<int> idxsOfBonesUsed = new HashSet<int>();
                            for (int j = dgo.vertIdx; j < dgo.vertIdx + dgo.numVerts; j++)
                            {
                                if (boneWeights[j].weight0 > 0f) idxsOfBonesUsed.Add(boneWeights[j].boneIndex0);
                                if (boneWeights[j].weight1 > 0f) idxsOfBonesUsed.Add(boneWeights[j].boneIndex1);
                                if (boneWeights[j].weight2 > 0f) idxsOfBonesUsed.Add(boneWeights[j].boneIndex2);
                                if (boneWeights[j].weight3 > 0f) idxsOfBonesUsed.Add(boneWeights[j].boneIndex3);
                            }
                            dgo.indexesOfBonesUsed = new int[idxsOfBonesUsed.Count];
                            idxsOfBonesUsed.CopyTo(dgo.indexesOfBonesUsed);
                        }

                        if (combiner.LOG_LEVEL >= MB2_LogLevel.debug)
                        {
                            Debug.Log("Baker used old systems that duplicated bones. Upgrading to new system by building indexesOfBonesUsed");
                        }

                    }
                }
            }

            public int GetNewBonesLength()
            {
                if (combiner.settings.renderType == MB_RenderType.skinnedMeshRenderer)
                {
                    return oldBindPosesPreviousBake.Length + bonesToAdd.Count - boneIdxsToDelete.Count;
                }
                else
                {
                    return 0;
                }
            }

            internal void _CollectSkinningDataForDGOsInCombinedMesh(List<MB_DynamicGameObject> objsToAdd)
            {
                {
                    //  First pass -----------
                    for (int i = 0; i < objsToAdd.Count; i++)
                    {
                        MB_DynamicGameObject dgo = objsToAdd[i];
                        CollectBonesToAddForDGO(dgo, MB_Utility.GetRenderer(dgo.gameObject), combiner.settings.smrNoExtraBonesWhenCombiningMeshRenderers);
                    }
                }
            }

            public bool CollectBonesToAddForDGO(MB_DynamicGameObject dgo, Renderer r, bool noExtraBonesForMeshRenderers)
            {
                bool success = true;
                Debug.Assert(_didSetup && !_disposed, "Need to setup first.");
                Debug.Assert(combiner.settings.renderType == MB_RenderType.skinnedMeshRenderer);
                // We could be working with adding and deleting smr body parts from the same rig. Different smrs will share 
                // the same bones.

                // Get the bone data that we will be adding from the MeshChannelsCache.

                Debug.Assert(combiner._meshChannelsCache is MeshChannelsCache);
                MeshChannelsCache mcc = (MeshChannelsCache)combiner._meshChannelsCache;
                
                List<Matrix4x4> dgoBindPoses = dgo._tmpSMR_CachedBindposes = mcc.GetBindposes(r, out dgo.isSkinnedMeshWithBones);
                dgo._tmpSMR_CachedBoneWeights = mcc.GetBoneWeights(r, dgo.numVerts, dgo.isSkinnedMeshWithBones);
                
                Transform[] dgoBones = dgo._tmpSMR_CachedBones = combiner._getBones(r, dgo.isSkinnedMeshWithBones);

                /* Pass in length of vertex array and number of bones into data structure */

                for (int i = 0; i < dgoBones.Length; i++)
                {
                    if (dgoBones[i] == null)
                    {
                        Debug.LogError("Source mesh r had a 'null' bone. Bones must not be null: " + r);
                        success = false;
                    }
                }

                if (!success) return success;

                if (noExtraBonesForMeshRenderers)
                {
                    if (MB_Utility.GetRenderer(dgo.gameObject) is MeshRenderer)
                    {
                        // We are visiting a single dgo which is a MeshRenderer.
                        // It may be the child decendant of a bone in another skinned mesh that is being baked or is already in the combined mesh. We need to find that bone if it exists.
                        // We need to check our parent ancestors and search the bone lists of the other dgos being added or previously baked looking for bones that may have been added 
                        Debug.Assert(dgoBones.Length == 1 && dgoBindPoses.Count == 1);
                        //     find and cache the parent bone for this MeshRenderer (it may not be the transform.parent)
                        bool foundBoneParent = false;
                        BoneAndBindpose boneParent = new BoneAndBindpose();
                        {
                            Transform t = dgo.gameObject.transform.parent;
                            while (t != null)
                            {
                                // Look for parent peviously baked in the combined mesh.
                                foreach (BoneAndBindpose b in boneAndBindPose2idx.Keys)
                                {
                                    if (b.bone == t)
                                    {
                                        boneParent = b;
                                        foundBoneParent = true;
                                        break;
                                    }
                                }

                                // Look for parent in something we are adding.
                                foreach (BoneAndBindpose b in bonesToAdd)
                                {
                                    if (b.bone == t)
                                    {
                                        boneParent = b;
                                        foundBoneParent = true;
                                        break;
                                    }
                                }

                                if (foundBoneParent)
                                {
                                    break;
                                }
                                else
                                {
                                    t = t.parent;
                                }
                            }
                        }

                        if (foundBoneParent)
                        {
                            dgoBones[0] = boneParent.bone;
                            dgoBindPoses[0] = boneParent.bindPose;
                        }
                    }
                }

                // The mesh being added may not use all bones on the rig. Find the bones actually used.
                int[] usedBoneIdx2srcMeshBoneIdx;
                {
                    /*
                    HashSet<int> usedBones = new HashSet<int>();
                    for (int j = 0; j < dgoBoneWeights.Length; j++)
                    {
                        usedBones.Add(dgoBoneWeights[j].boneIndex0);
                        usedBones.Add(dgoBoneWeights[j].boneIndex1);
                        usedBones.Add(dgoBoneWeights[j].boneIndex2);
                        usedBones.Add(dgoBoneWeights[j].boneIndex3);
                    }

                    usedBoneIdx2srcMeshBoneIdx = new int[usedBones.Count];
                    usedBones.CopyTo(usedBoneIdx2srcMeshBoneIdx);
                    */
                }

                {
                    usedBoneIdx2srcMeshBoneIdx = new int[dgoBones.Length];
                    for (int i = 0; i < usedBoneIdx2srcMeshBoneIdx.Length; i++) usedBoneIdx2srcMeshBoneIdx[i] = i;
                }

                // For each bone see if it exists in the bones array (with the same bindpose.).
                // We might be baking several skinned meshes on the same rig. We don't want duplicate bones in the bones array.
                for (int i = 0; i < dgoBones.Length; i++)
                {
                    bool foundInBonesList = false;
                    int bidx;
                    int dgoBoneIdx = usedBoneIdx2srcMeshBoneIdx[i];
                    BoneAndBindpose bb = new BoneAndBindpose(dgoBones[dgoBoneIdx], dgoBindPoses[dgoBoneIdx]);
                    if (boneAndBindPose2idx.TryGetValue(bb, out bidx))
                    {
                        if (dgoBones[dgoBoneIdx] == oldBonesPreviousBake[bidx] && 
                            !boneIdxsToDelete.Contains(bidx) &&
                            dgoBindPoses[dgoBoneIdx] == oldBindPosesPreviousBake[bidx])
                        {
                            foundInBonesList = true;
                        }
                    }

                    if (!foundInBonesList)
                    {
                        if (!bonesToAdd.Contains(bb))
                        {
                            bonesToAdd.Add(bb);
                        }
                    }
                }

                dgo._tmpSMR_srcMeshBoneIdx2masterListBoneIdx = usedBoneIdx2srcMeshBoneIdx;

                /* Other way of initialising thing */

             /*   BoneWeightData bwd = new BoneWeightData();

                Mesh mesh = MB_Utility.GetMesh(r.gameObject);  

                NativeArray<byte> bpv = meshChannelCache.GetBonesPerVertex(mesh); //Length is how many vertices 


                int maxbonespervertex = FindMaxBonesPerVertex(bpv, bpv.Length);

                // store bpv in dgo 
                //
               

                bwd.InitBoneWeightData(ref bwd, maxbonespervertex, dgo.numVerts);
  
                BoneWeight[] arr = meshChannelCache.GetBoneWeights(r, dgo.numVerts, dgo.isSkinnedMeshWithBones);


                for (int i = 0; i < dgo.numVerts; i++)
                {
                    
                    bwd.SetBoneWeight(0,i,arr[i].weight0);
                    bwd.SetBoneWeight(1, i, arr[i].weight1);
                    bwd.SetBoneWeight(2, i, arr[i].weight2);
                    bwd.SetBoneWeight(3, i, arr[i].weight3);

                }*/

                return success;
            }

            private List<MB3_MeshCombinerSingle.MB_DynamicGameObject>[] _buildBoneIdx2dgoMap()
            {
                List<MB3_MeshCombinerSingle.MB_DynamicGameObject>[] boneIdx2dgoMap = new List<MB3_MeshCombinerSingle.MB_DynamicGameObject>[oldBonesPreviousBake.Length];
                for (int i = 0; i < boneIdx2dgoMap.Length; i++) boneIdx2dgoMap[i] = new List<MB3_MeshCombinerSingle.MB_DynamicGameObject>();
                // build the map of bone indexes to objects that use them
                for (int i = 0; i < combiner.mbDynamicObjectsInCombinedMesh.Count; i++)
                {
                    MB3_MeshCombinerSingle.MB_DynamicGameObject dgo = combiner.mbDynamicObjectsInCombinedMesh[i];
                    for (int j = 0; j < dgo.indexesOfBonesUsed.Length; j++)
                    {
                        boneIdx2dgoMap[dgo.indexesOfBonesUsed[j]].Add(dgo);
                    }
                }

                return boneIdx2dgoMap;
            }

            public void CopyBonesWeAreKeepingToNewBonesArrayAndAdjustBWIndexes(int totalDeleteVerts)
            {
                // bones are copied separately because some dgos share bones
                if (boneIdxsToDelete.Count > 0)
                {
                    int[] boneIdxsToDel = new int[boneIdxsToDelete.Count];
                    boneIdxsToDelete.CopyTo(boneIdxsToDel);
                    Array.Sort(boneIdxsToDel);
                    //bones are being moved in bones array so need to do some remapping
                    int[] oldBonesIndex2newBonesIndexMap = new int[oldBonesPreviousBake.Length];
                    int newIdx = 0;
                    int indexInDeleteList = 0;

                    //bones were deleted so we need to rebuild bones and bind poses
                    //and build a map of old bone indexes to new bone indexes
                    //do this by copying old to new skipping ones we are deleting
                    for (int i = 0; i < oldBonesPreviousBake.Length; i++)
                    {
                        if (indexInDeleteList < boneIdxsToDel.Length &&
                            boneIdxsToDel[indexInDeleteList] == i)
                        {
                            //we are deleting this bone so skip its index
                            indexInDeleteList++;
                            oldBonesIndex2newBonesIndexMap[i] = -1;
                        }
                        else
                        {
                            oldBonesIndex2newBonesIndexMap[i] = newIdx;
                            nbones[newIdx] = oldBonesPreviousBake[i];
                            nbindPoses[newIdx] = oldBindPosesPreviousBake[i];
                            newIdx++;
                        }
                    }


                    int numVertKeeping = boneWeights.Length - totalDeleteVerts;
                    {
                        for (int i = 0; i < numVertKeeping; i++)
                        {
                            BoneWeight bw = nboneWeights[i];
                            bw.boneIndex0 = oldBonesIndex2newBonesIndexMap[bw.boneIndex0];
                            bw.boneIndex1 = oldBonesIndex2newBonesIndexMap[bw.boneIndex1];
                            bw.boneIndex2 = oldBonesIndex2newBonesIndexMap[bw.boneIndex2];
                            bw.boneIndex3 = oldBonesIndex2newBonesIndexMap[bw.boneIndex3];
                            nboneWeights[i] = bw;
                        }
                    }

                    /*
                    unsafe
                    {
                        fixed (BoneWeight* boneWeightFirstPtr = &nboneWeights[0])
                        {
                            BoneWeight* boneWeightPtr = boneWeightFirstPtr;
                            for (int i = 0; i < numVertKeeping; i++)
                            {
                                boneWeightPtr->boneIndex0 = oldBonesIndex2newBonesIndexMap[boneWeightPtr->boneIndex0];
                                boneWeightPtr->boneIndex1 = oldBonesIndex2newBonesIndexMap[boneWeightPtr->boneIndex1];
                                boneWeightPtr->boneIndex2 = oldBonesIndex2newBonesIndexMap[boneWeightPtr->boneIndex2];
                                boneWeightPtr->boneIndex3 = oldBonesIndex2newBonesIndexMap[boneWeightPtr->boneIndex3];
                                boneWeightPtr++;
                            }
                        }
                    }
                    */

                    //adjust the bone indexes on the dgos from old to new
                    for (int i = 0; i < combiner.mbDynamicObjectsInCombinedMesh.Count; i++)
                    {
                        MB_DynamicGameObject dgo = combiner.mbDynamicObjectsInCombinedMesh[i];
                        for (int j = 0; j < dgo.indexesOfBonesUsed.Length; j++)
                        {
                            dgo.indexesOfBonesUsed[j] = oldBonesIndex2newBonesIndexMap[dgo.indexesOfBonesUsed[j]];
                        }
                    }
                }
                else
                { //no bones are moving so can simply copy bones from old to new
                    Array.Copy(oldBonesPreviousBake, nbones, oldBonesPreviousBake.Length);
                    Array.Copy(oldBindPosesPreviousBake, nbindPoses, oldBindPosesPreviousBake.Length);
                }
            }

            public void InsertNewBonesIntoBonesArray()
            {
                if (combiner.settings.renderType == MB_RenderType.skinnedMeshRenderer)
                {
                    Debug.Assert(!_disposed);
                    boneWeights = nboneWeights;
                    combiner.bindPoses = nbindPoses;
                    combiner.bones = nbones;

                    int bidx = 0;
                    foreach (BoneAndBindpose t in GetBonesToAdd())
                    {
                        Debug.Assert(t.bone != null);
                        int idx = _newBonesStartAtIdx + bidx;
                        nbones[idx] = t.bone;
                        Debug.Assert(nbones[idx] != null, "  bone was null ");
                        nbindPoses[idx] = t.bindPose;
                        bidx++;
                    }
                }
            }

            public void AddBonesToNewBonesArrayAndAdjustBWIndexes1(MB_DynamicGameObject dgo, int vertsIdx)
            {

                Transform[] dgoBones = dgo._tmpSMR_CachedBones;
                List<Matrix4x4> dgoBindPoses = dgo._tmpSMR_CachedBindposes;
                BoneWeight[] dgoBoneWeights = dgo._tmpSMR_CachedBoneWeights;
                int[] srcIndex2combinedIndexMap = new int[dgoBones.Length];
                for (int srcBoneIdx = 0; srcBoneIdx < dgo._tmpSMR_srcMeshBoneIdx2masterListBoneIdx.Length; srcBoneIdx++)
                {
                    for (int combBoneIdx = 0; combBoneIdx < nbones.Length; combBoneIdx++)
                    {
                        if (dgoBones[srcBoneIdx] == nbones[combBoneIdx])
                        {
                            if (dgoBindPoses[srcBoneIdx] == nbindPoses[combBoneIdx])
                            {
                                srcIndex2combinedIndexMap[srcBoneIdx] = combBoneIdx;
                                break;
                            }
                        }
                    }
                }

                //remap the bone weights for this dgo
                //build a list of usedBones, can't trust dgoBones because it contains all bones in the rig
                for (int j = 0; j < dgoBoneWeights.Length; j++)
                {
                    int newVertIdx = vertsIdx + j;
                    nboneWeights[newVertIdx].boneIndex0 = srcIndex2combinedIndexMap[dgoBoneWeights[j].boneIndex0];
                    nboneWeights[newVertIdx].boneIndex1 = srcIndex2combinedIndexMap[dgoBoneWeights[j].boneIndex1];
                    nboneWeights[newVertIdx].boneIndex2 = srcIndex2combinedIndexMap[dgoBoneWeights[j].boneIndex2];
                    nboneWeights[newVertIdx].boneIndex3 = srcIndex2combinedIndexMap[dgoBoneWeights[j].boneIndex3];
                    nboneWeights[newVertIdx].weight0 = dgoBoneWeights[j].weight0;
                    nboneWeights[newVertIdx].weight1 = dgoBoneWeights[j].weight1;
                    nboneWeights[newVertIdx].weight2 = dgoBoneWeights[j].weight2;
                    nboneWeights[newVertIdx].weight3 = dgoBoneWeights[j].weight3;
                }

                // repurposing the _tmpIndexesOfSourceBonesUsed since
                //we don't need it anymore and this saves a memory allocation . remap the indexes that point to source bones to combined bones.
                for (int j = 0; j < dgo._tmpSMR_srcMeshBoneIdx2masterListBoneIdx.Length; j++)
                {
                    dgo._tmpSMR_srcMeshBoneIdx2masterListBoneIdx[j] = srcIndex2combinedIndexMap[dgo._tmpSMR_srcMeshBoneIdx2masterListBoneIdx[j]];
                }
                dgo.indexesOfBonesUsed = dgo._tmpSMR_srcMeshBoneIdx2masterListBoneIdx;
                dgo._tmpSMR_srcMeshBoneIdx2masterListBoneIdx = null;
                dgo._tmpSMR_CachedBones = null;
                dgo._tmpSMR_CachedBindposes = null;
                dgo._tmpSMR_CachedBoneWeights = null;

                //check original bones and bindPoses
                /*
                for (int j = 0; j < dgo.indexesOfBonesUsed.Length; j++) {
                    Transform bone = bones[dgo.indexesOfBonesUsed[j]];
                    Matrix4x4 bindpose = bindPoses[dgo.indexesOfBonesUsed[j]];
                    bool found = false;
                    for (int k = 0; k < dgo._originalBones.Length; k++) {
                        if (dgo._originalBones[k] == bone && dgo._originalBindPoses[k] == bindpose) {
                            found = true;
                        }
                    }
                    if (!found) Debug.LogError("A Mismatch between original bones and bones array. " + dgo.name);
                }
                */
            }

            public void UpdateGameObjects_UpdateBWIndexes(MB_DynamicGameObject dgo)
            {
                Transform[] rendererBones = MBVersion.GetBones(dgo._renderer, dgo.isSkinnedMeshWithBones);
                //only does BoneWeights. Used to do Bones and BindPoses but it doesn't make sence.
                //if updating Bones and Bindposes should remove and re-add
                Debug.Assert(dgo._initialized);
                
                Debug.Assert(combiner._meshChannelsCache is MeshChannelsCache);
                MeshChannelsCache mcc = (MeshChannelsCache)combiner._meshChannelsCache;

                BoneWeight[] bws = mcc.GetBoneWeights(dgo._renderer, dgo.numVerts, dgo.isSkinnedMeshWithBones);
                //Transform[] bs = _getBones(r, dgo.isSkinnedMeshWithBones);
                //assumes that the bones and boneweights have not been reeordered
                int bwIdx = dgo.vertIdx; //the index in the verts array
                bool switchedBonesDetected = false;
                for (int i = 0; i < bws.Length; i++)
                {
                    if (rendererBones[bws[i].boneIndex0] != oldBonesPreviousBake[boneWeights[bwIdx].boneIndex0])
                    {
                        switchedBonesDetected = true;
                        break;
                    }
                    boneWeights[bwIdx].weight0 = bws[i].weight0;
                    boneWeights[bwIdx].weight1 = bws[i].weight1;
                    boneWeights[bwIdx].weight2 = bws[i].weight2;
                    boneWeights[bwIdx].weight3 = bws[i].weight3;
                    bwIdx++;
                }
                if (switchedBonesDetected)
                {
                    Debug.LogError("Detected that some of the boneweights reference different bones than when initial added. Boneweights must reference the same bones " + dgo.name);
                }
            }

            public void CopyVertsNormsTansToBuffers(MB_DynamicGameObject dgo, MB_IMeshBakerSettings settings, int vertsIdx, NativeSlice<Vector3> nnorms, NativeSlice<Vector4> ntangs, NativeSlice<Vector3> nverts, NativeSlice<Vector3> normals, NativeSlice<Vector4> tangents, NativeSlice<Vector3> verts)
            {
                Debug.LogError("The simple bone processor doesn't use this.");
            }

            public void CopyVertsNormsTansToBuffers(MB_DynamicGameObject dgo, MB_IMeshBakerSettings settings, int vertsIdx, Vector3[] nnorms, Vector4[] ntangs, Vector3[] nverts, Vector3[] normals, Vector4[] tangents, Vector3[] verts)
            {
                bool isMeshRenderer = dgo._renderer is MeshRenderer;
                if (settings.smrNoExtraBonesWhenCombiningMeshRenderers &&
                    isMeshRenderer &&
                    dgo._tmpSMR_CachedBones[0] != dgo.gameObject.transform // bone may not have a parent ancestor that is a bone
                    )
                {
                    // transform all the verticies, norms and tangents into the parent bone's local space (adjusted by the parent bone's bind pose).
                    // there should be only one bone and bind pose for a mesh renderer dgo. 
                    // The bone and bind pose should be the parent-bone's NOT the MeshRenderers.
                    Matrix4x4 l2parentMat = dgo._tmpSMR_CachedBindposes[0].inverse * dgo._tmpSMR_CachedBones[0].worldToLocalMatrix * dgo.gameObject.transform.localToWorldMatrix;

                    // Similar to local2world but with translation removed and we are using the inverse transpose.
                    // We use this for normals and tangents because it handles scaling correctly.
                    Matrix4x4 l2parentRotScale = l2parentMat;
                    l2parentRotScale[0, 3] = l2parentRotScale[1, 3] = l2parentRotScale[2, 3] = 0f;
                    l2parentRotScale = l2parentRotScale.inverse.transpose;

                    //can't modify the arrays we get from the cache because they will be modified multiple times if the same mesh is being added multiple times.
                    for (int j = 0; j < dgo._mesh.vertexCount; j++)
                    {
                        int vIdx = vertsIdx + j;
                        if (verts != null)
                        {
                            verts[vertsIdx + j] = l2parentMat.MultiplyPoint3x4(nverts[j]);
                        }

                        if (settings.doNorm && nnorms != null)
                        {
                            normals[vIdx] = l2parentRotScale.MultiplyPoint3x4(nnorms[j]).normalized;
                        }

                        if (settings.doTan && ntangs != null)
                        {
                            float w = ntangs[j].w; //need to preserve the w value
                            tangents[vIdx] = l2parentRotScale.MultiplyPoint3x4(((Vector3)ntangs[j])).normalized;
                            tangents[vIdx].w = w;
                        }
                    }
                }
                else
                {
                    if (settings.doNorm && nnorms != null) nnorms.CopyTo(normals, vertsIdx);
                    if (settings.doTan && ntangs != null) ntangs.CopyTo(tangents, vertsIdx);
                    if (verts != null) nverts.CopyTo(verts, vertsIdx);
                }
            }

            public void DisposeOfTemporarySMRData()
            {
                if (boneIdxsToDelete != null) boneIdxsToDelete.Clear();
                if (boneAndBindPose2idx != null) boneAndBindPose2idx.Clear();
                boneIdxsToDelete = null;
                boneAndBindPose2idx = null;
                boneIdx2dgoMap = null;
            }

            public void CopyBoneWeightsFromMeshForDGOsInCombined(MB_DynamicGameObject dgo, int targVidx)
            {
                Array.Copy(boneWeights, dgo.vertIdx, nboneWeights, targVidx, dgo.numVerts); 
            }

            public void ApplySMRdataToMeshToBuffer()
            {
                
            }

            public void ApplySMRdataToMesh(MB3_MeshCombinerSingle combiner, Mesh mesh)
            {
                Debug.Assert(!_disposed);
                mesh.bindposes = combiner.bindPoses;
                mesh.boneWeights = boneWeights;
                
                /*
                {
                    string s = "bindPoses:\n";
                    for (int i = 0; i < combiner.bindPoses.Length; i++)
                    {
                        s += combiner.bindPoses[i] + "\n";
                    }

                    Debug.Log(s);
                    s = "boneWeights: " + boneWeights.Length + " \n";
                    for (int i = 0; i < boneWeights.Length && i < 20; i++)
                    {
                        s += MB_Utility.BoneWeightToString( boneWeights[i]) + "\n";
                    }

                    Debug.Log(s);
                }
                */
            }

            public bool GetCachedSMRMeshData(MB_DynamicGameObject dgo)
            {
                return true;
            }


            public bool DB_CheckIntegrity()
            {
                if (combiner.settings.renderType == MB_RenderType.skinnedMeshRenderer)
                {

                    for (int i = 0; i < combiner.mbDynamicObjectsInCombinedMesh.Count; i++)
                    {
                        MB_DynamicGameObject dgo = combiner.mbDynamicObjectsInCombinedMesh[i];
                        HashSet<int> usedBonesWeights = new HashSet<int>();
                        HashSet<int> usedBonesIndexes = new HashSet<int>();
                        for (int j = dgo.vertIdx; j < dgo.vertIdx + dgo.numVerts; j++)
                        {
                            usedBonesWeights.Add(boneWeights[j].boneIndex0);
                            usedBonesWeights.Add(boneWeights[j].boneIndex1);
                            usedBonesWeights.Add(boneWeights[j].boneIndex2);
                            usedBonesWeights.Add(boneWeights[j].boneIndex3);
                        }
                        for (int j = 0; j < dgo.indexesOfBonesUsed.Length; j++)
                        {
                            usedBonesIndexes.Add(dgo.indexesOfBonesUsed[j]);
                        }

                        usedBonesIndexes.ExceptWith(usedBonesWeights);
                        if (usedBonesIndexes.Count > 0)
                        {
                            Debug.LogError("The bone indexes were not the same. " + usedBonesWeights.Count + " " + usedBonesIndexes.Count);
                        }
                        for (int j = 0; j < dgo.indexesOfBonesUsed.Length; j++)
                        {
                            if (j < 0 || j > oldBonesPreviousBake.Length)
                                Debug.LogError("Bone index was out of bounds.");
                        }
                        if (dgo.indexesOfBonesUsed.Length < 1)
                            Debug.Log("DGO had no bones");

                        Debug.Assert(dgo.targetSubmeshIdxs.Length == dgo.uvRects.Length, "Array length mismatch targetSubmeshIdxs, uvRects");
                        Debug.Assert(dgo.targetSubmeshIdxs.Length == dgo.sourceSharedMaterials.Length, "Array length mismatch targetSubmeshIdxs, uvRects");
                        Debug.Assert(dgo.targetSubmeshIdxs.Length == dgo.encapsulatingRect.Length, "Array length mismatch targetSubmeshIdxs, uvRects");
                        Debug.Assert(dgo.targetSubmeshIdxs.Length == dgo.sourceMaterialTiling.Length, "Array length mismatch targetSubmeshIdxs, uvRects");
                        Debug.Assert(dgo.targetSubmeshIdxs.Length == dgo.obUVRects.Length, "Array length mismatch targetSubmeshIdxs, uvRects");
                    }
                }

                return true;
            }
        }

        public override void UpdateSkinnedMeshApproximateBounds()
        {
            UpdateSkinnedMeshApproximateBoundsFromBounds();
        }

        public override void UpdateSkinnedMeshApproximateBoundsFromBones()
        {
            if (outputOption == MB2_OutputOptions.bakeMeshAssetsInPlace)
            {
                if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Can't UpdateSkinnedMeshApproximateBounds when output type is bakeMeshAssetsInPlace");
                return;
            }
            if (bones.Length == 0)
            {
                if (GetVertexCount() > 0) if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("No bones in SkinnedMeshRenderer. Could not UpdateSkinnedMeshApproximateBounds.");
                return;
            }
            if (_targetRenderer == null)
            {
                if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Target Renderer is not set. No point in calling UpdateSkinnedMeshApproximateBounds.");
                return;
            }
            if (!_targetRenderer.GetType().Equals(typeof(SkinnedMeshRenderer)))
            {
                if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Target Renderer is not a SkinnedMeshRenderer. No point in calling UpdateSkinnedMeshApproximateBounds.");
                return;
            }
            UpdateSkinnedMeshApproximateBoundsFromBonesStatic(bones, (SkinnedMeshRenderer)targetRenderer);
        }

        public override void UpdateSkinnedMeshApproximateBoundsFromBounds()
        {
            if (outputOption == MB2_OutputOptions.bakeMeshAssetsInPlace)
            {
                if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Can't UpdateSkinnedMeshApproximateBoundsFromBounds when output type is bakeMeshAssetsInPlace");
                return;
            }
            if (GetVertexCount() == 0 || mbDynamicObjectsInCombinedMesh.Count == 0)
            {
                if (GetVertexCount() > 0) if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Nothing in SkinnedMeshRenderer. CoulddoBlendShapes not UpdateSkinnedMeshApproximateBoundsFromBounds.");
                return;
            }
            if (_targetRenderer == null)
            {
                if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Target Renderer is not set. No point in calling UpdateSkinnedMeshApproximateBoundsFromBounds.");
                return;
            }
            if (!_targetRenderer.GetType().Equals(typeof(SkinnedMeshRenderer)))
            {
                if (LOG_LEVEL >= MB2_LogLevel.warn) Debug.LogWarning("Target Renderer is not a SkinnedMeshRenderer. No point in calling UpdateSkinnedMeshApproximateBoundsFromBounds.");
                return;
            }

            UpdateSkinnedMeshApproximateBoundsFromBoundsStatic(objectsInCombinedMesh, (SkinnedMeshRenderer)targetRenderer);
        }
    }
}
