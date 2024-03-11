﻿using System.Collections.Generic;
using UnityEngine;
using System;

namespace DigitalOpus.MB.Core
{
    public partial class MB3_MeshCombinerSingle : MB3_MeshCombiner
    {
        internal class MB_MeshCombinerSingle_BlendShapeProcessor
        {
            MB3_MeshCombinerSingle combiner;
            MBBlendShape[] nblendShapes = null;

            private bool _disposed;

            protected void Dispose(bool disposing)
            {
                if (_disposed) return;
                if (disposing)
                {
                    combiner = null;
                    nblendShapes = null;
                }

                _disposed = true;
            }

            public void Dispose()
            {
                Dispose(true);
            }

            public MB_MeshCombinerSingle_BlendShapeProcessor(MB3_MeshCombinerSingle cm)
            {
                combiner = cm;
            }

            /// <summary>
            /// Get the blend shapes from the source mesh
            /// </summary>
            public static MBBlendShape[] GetBlendShapes(Mesh m, int gameObjectID, GameObject gameObject, Dictionary<int, MeshChannels> meshID2MeshChannels)
            {
                if (MBVersion.GetMajorVersion() > 5 ||
                (MBVersion.GetMajorVersion() == 5 && MBVersion.GetMinorVersion() >= 3))
                {

                    MeshChannels mc;
                    if (!meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out mc))
                    {
                        mc = new MeshChannels();
                        meshID2MeshChannels.Add(m.GetInstanceID(), mc);
                    }
                    if (mc.blendShapes == null)
                    {
                        MBBlendShape[] shapes = new MBBlendShape[m.blendShapeCount];
                        int arrayLen = m.vertexCount;
                        for (int shapeIdx = 0; shapeIdx < shapes.Length; shapeIdx++)
                        {
                            MBBlendShape shape = shapes[shapeIdx] = new MBBlendShape();
                            shape.frames = new MBBlendShapeFrame[MBVersion.GetBlendShapeFrameCount(m, shapeIdx)];
                            shape.name = m.GetBlendShapeName(shapeIdx);
                            shape.indexInSource = shapeIdx;
                            shape.gameObjectID = gameObjectID;
                            shape.gameObject = gameObject;
                            for (int frameIdx = 0; frameIdx < shape.frames.Length; frameIdx++)
                            {
                                MBBlendShapeFrame frame = shape.frames[frameIdx] = new MBBlendShapeFrame();
                                frame.frameWeight = MBVersion.GetBlendShapeFrameWeight(m, shapeIdx, frameIdx);
                                frame.vertices = new Vector3[arrayLen];
                                frame.normals = new Vector3[arrayLen];
                                frame.tangents = new Vector3[arrayLen];
                                MBVersion.GetBlendShapeFrameVertices(m, shapeIdx, frameIdx, frame.vertices, frame.normals, frame.tangents);
                            }
                        }
                        mc.blendShapes = shapes;
                        return mc.blendShapes;
                    }
                    else
                    { //copy cached blend shapes from same mesh assiged to a different gameObjectID
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
                }
                else
                {
                    return new MBBlendShape[0];
                }
            }

            internal void ApplyBlendShapeFramesToMeshAndBuildMap(int newVertCount)
            {
                // int vertCount = combiner.GetVertexCount();
                Renderer _targetRenderer = combiner._targetRenderer;
                Mesh _mesh = combiner._mesh;

                if (MBVersion.GetMajorVersion() > 5 ||
                                    (MBVersion.GetMajorVersion() == 5 && MBVersion.GetMinorVersion() >= 3))
                {
                    if (combiner.blendShapes.Length != nblendShapes.Length) combiner.blendShapes = new MBBlendShape[nblendShapes.Length];
                    Vector3[] targVerts = new UnityEngine.Vector3[newVertCount];
                    Vector3[] targNorms = new UnityEngine.Vector3[newVertCount];
                    Vector3[] targTans = new UnityEngine.Vector3[newVertCount];

                    ((SkinnedMeshRenderer)_targetRenderer).sharedMesh = null;

                    MBVersion.ClearBlendShapes(_mesh);
                    for (int bsIdx = 0; bsIdx < nblendShapes.Length; bsIdx++)
                    {
                        MBBlendShape blendShape = nblendShapes[bsIdx];
                        MB_DynamicGameObject dgo = combiner.instance2Combined_MapGet(blendShape.gameObject);
                        if (dgo != null)
                        {
                            int destIdx = dgo.vertIdx;
                            for (int frmIdx = 0; frmIdx < blendShape.frames.Length; frmIdx++)
                            {
                                MBBlendShapeFrame frame = blendShape.frames[frmIdx];
                                Array.Copy(frame.vertices, 0, targVerts, destIdx, frame.vertices.Length);
                                Array.Copy(frame.normals, 0, targNorms, destIdx, frame.normals.Length);
                                Array.Copy(frame.tangents, 0, targTans, destIdx, frame.tangents.Length);
                                MBVersion.AddBlendShapeFrame(_mesh, _ConvertBlendShapeNameToOutputName(blendShape.name) + blendShape.gameObjectID, frame.frameWeight, targVerts, targNorms, targTans);
                                // We re-use these arrays restore them to zero
                                _ZeroArray(targVerts, destIdx, frame.vertices.Length);
                                _ZeroArray(targNorms, destIdx, frame.normals.Length);
                                _ZeroArray(targTans, destIdx, frame.tangents.Length);
                            }
                        }
                        else
                        {
                            Debug.LogError("InstanceID in blend shape that was not in instance2combinedMap");
                        }
                        combiner.blendShapes[bsIdx] = blendShape;
                    }

                    //this is necessary to get the renderer to refresh its data about the blendshapes.
                    ((SkinnedMeshRenderer)_targetRenderer).sharedMesh = null;
                    ((SkinnedMeshRenderer)_targetRenderer).sharedMesh = _mesh;

                    // Add the map to the target renderer.
                    if (combiner.settings.doBlendShapes)
                    {
                        MB_BlendShape2CombinedMap mapComponent = _targetRenderer.GetComponent<MB_BlendShape2CombinedMap>();
                        if (mapComponent == null) mapComponent = _targetRenderer.gameObject.AddComponent<MB_BlendShape2CombinedMap>();
                        SerializableSourceBlendShape2Combined map = mapComponent.GetMap();
                        _BuildSrcShape2CombinedMap(combiner, map, nblendShapes);
                    }
                }
            }


            public void AllocateBlendShapeArrayIfNecessary(int nBlendShapeSize)
            {
                if (combiner.settings.doBlendShapes)
                {
                    nblendShapes = new MBBlendShape[nBlendShapeSize];
                }
            }

            public void AssignNewBlendShapesToCombinerIfNecessary()
            {
                if (combiner.settings.doBlendShapes)
                {
                    combiner.blendShapes = nblendShapes;
                }
            }

            public void CopyBlendShapesInCurrentMeshIfNecessary(ref int targBlendShapeIdx, MB_DynamicGameObject dgo)
            {
                if (combiner.settings.doBlendShapes) 
                {
                    Array.Copy(combiner.blendShapes, dgo.blendShapeIdx, nblendShapes, targBlendShapeIdx, dgo.numBlendShapes);
                    dgo.blendShapeIdx = targBlendShapeIdx;
                    targBlendShapeIdx += dgo.numBlendShapes;
                }
            }

            public void CopyBlendShapesForNewMeshIfNecessary(ref int targBlendShapeIdx, MB_DynamicGameObject dgo, Mesh mesh, IMeshChannelsCacheTaggingInterface meshChannelCache)
            {
                if (combiner.settings.doBlendShapes)
                {
                    int blendShapeIdx = targBlendShapeIdx;
                    MBBlendShape[] blendShapesFromMesh = meshChannelCache.GetBlendShapes(mesh, dgo.instanceID, dgo.gameObject);
                    blendShapesFromMesh.CopyTo(nblendShapes, blendShapeIdx);
                    dgo.blendShapeIdx = targBlendShapeIdx;
                    targBlendShapeIdx += blendShapesFromMesh.Length;
                }
            }

            /// <summary>
            /// The source blend shape may have parts that should be stripped away.
            /// Use this method to strip away the unused parts.
            /// </summary>
            private static string _ConvertBlendShapeNameToOutputName(string bs)
            {
                // remove everything before the final '.'
                string[] nameParts = bs.Split('.');
                string lastPart = nameParts[nameParts.Length - 1];

                return lastPart;
            }

            internal void ApplyBlendShapeFramesToMeshAndBuildMap_MergeBlendShapesWithTheSameName(int newVertCount)
            {
                // int vertCount = combiner.GetVertexCount();
                Renderer _targetRenderer = combiner._targetRenderer;
                Mesh _mesh = combiner._mesh;

                if (MBVersion.GetMajorVersion() > 5 ||
                                    (MBVersion.GetMajorVersion() == 5 && MBVersion.GetMinorVersion() >= 3))
                {
                    Vector3[] targVerts = new UnityEngine.Vector3[newVertCount];
                    Vector3[] targNorms = new UnityEngine.Vector3[newVertCount];
                    Vector3[] targTans = new UnityEngine.Vector3[newVertCount];

                    MBVersion.ClearBlendShapes(_mesh);

                    // Group source that share the same blendShapeName
                    bool numFramesError = false;
                    Dictionary<string, List<MBBlendShape>> shapeName2objs = new Dictionary<string, List<MBBlendShape>>();
                    {
                        for (int i = 0; i < nblendShapes.Length; i++)
                        {
                            MBBlendShape blendShape = nblendShapes[i];
                            string blendShapeName = _ConvertBlendShapeNameToOutputName(blendShape.name);
                            List<MBBlendShape> dgosUsingBlendShape;
                            if (!shapeName2objs.TryGetValue(blendShapeName, out dgosUsingBlendShape))
                            {
                                dgosUsingBlendShape = new List<MBBlendShape>();
                                shapeName2objs.Add(blendShapeName, dgosUsingBlendShape);
                            }

                            dgosUsingBlendShape.Add(blendShape);
                            if (dgosUsingBlendShape.Count > 1)
                            {
                                if (dgosUsingBlendShape[0].frames.Length != blendShape.frames.Length)
                                {
                                    Debug.LogError("BlendShapes with the same name must have the same number of frames.");
                                    numFramesError = true;
                                }
                            }
                        }
                    }

                    if (numFramesError) return;

                    if (combiner.blendShapes.Length != nblendShapes.Length) combiner.blendShapes = new MBBlendShape[shapeName2objs.Keys.Count];

                    int bsInCombinedIdx = 0;
                    foreach (string shapeName in shapeName2objs.Keys)
                    {
                        List<MBBlendShape> groupOfSrcObjs = shapeName2objs[shapeName];
                        MBBlendShape firstBlendShape = groupOfSrcObjs[0];
                        int numFrames = firstBlendShape.frames.Length;
                        int db_numVertsAdded = 0;
                        int db_numObjsAdded = 0;
                        string db_vIdx = "";

                        for (int frmIdx = 0; frmIdx < numFrames; frmIdx++)
                        {
                            float firstFrameWeight = firstBlendShape.frames[frmIdx].frameWeight;

                            for (int dgoIdx = 0; dgoIdx < groupOfSrcObjs.Count; dgoIdx++)
                            {
                                MBBlendShape blendShape = groupOfSrcObjs[dgoIdx];
                                MB_DynamicGameObject dgo = combiner.instance2Combined_MapGet(blendShape.gameObject);
                                int destIdx = dgo.vertIdx;
                                Debug.Assert(blendShape.frames.Length == numFrames);
                                MBBlendShapeFrame frame = blendShape.frames[frmIdx];
                                Debug.Assert(frame.frameWeight == firstFrameWeight);
                                Array.Copy(frame.vertices, 0, targVerts, destIdx, frame.vertices.Length);
                                Array.Copy(frame.normals, 0, targNorms, destIdx, frame.normals.Length);
                                Array.Copy(frame.tangents, 0, targTans, destIdx, frame.tangents.Length);
                                if (frmIdx == 0)
                                {
                                    db_numVertsAdded += frame.vertices.Length;
                                    db_vIdx += blendShape.gameObject.name + " " + destIdx + ":" + (destIdx + frame.vertices.Length) + ", ";
                                }
                            }

                            db_numObjsAdded += groupOfSrcObjs.Count;
                            MBVersion.AddBlendShapeFrame(_mesh, shapeName, firstFrameWeight, targVerts, targNorms, targTans);

                            // We re-use these arrays restore them to zero
                            _ZeroArray(targVerts, 0, targVerts.Length);
                            _ZeroArray(targNorms, 0, targNorms.Length);
                            _ZeroArray(targTans, 0, targTans.Length);
                        }

                        combiner.blendShapes[bsInCombinedIdx] = firstBlendShape;
                        bsInCombinedIdx++;
                    }



                    //this is necessary to get the renderer to refresh its data about the blendshapes.
                    ((SkinnedMeshRenderer)_targetRenderer).sharedMesh = null;
                    ((SkinnedMeshRenderer)_targetRenderer).sharedMesh = _mesh;

                    // Add the map to the target renderer.
                    if (combiner.settings.doBlendShapes)
                    {
                        MB_BlendShape2CombinedMap mapComponent = _targetRenderer.GetComponent<MB_BlendShape2CombinedMap>();
                        if (mapComponent == null) mapComponent = _targetRenderer.gameObject.AddComponent<MB_BlendShape2CombinedMap>();
                        SerializableSourceBlendShape2Combined map = mapComponent.GetMap();
                        _BuildSrcShape2CombinedMap(combiner, map, combiner.blendShapes);
                    }
                }
            }

            private static void _BuildSrcShape2CombinedMap(MB3_MeshCombinerSingle combiner, SerializableSourceBlendShape2Combined map, MBBlendShape[] bs)
            {
                MBBlendShape[] blendShapesInCombined = combiner.blendShapes;
                Renderer _targetRenderer = combiner._targetRenderer;
                if (combiner._mesh != null && 
                    (combiner._mesh.blendShapeCount != combiner.blendShapes.Length))
                {
                    Debug.LogError("Blend shapes in combiner did not match blend shapes in mesh. Map will probably be invalid.");
                }

                Debug.Assert(_targetRenderer.gameObject != null, "Target Renderer was null.");
                GameObject[] srcGameObjects = new GameObject[bs.Length];
                int[] srcBlendShapeIdxs = new int[bs.Length];
                GameObject[] targGameObjects = new GameObject[bs.Length];
                int[] targBlendShapeIdxs = new int[bs.Length];
                for (int i = 0; i < blendShapesInCombined.Length; i++)
                {
                    srcGameObjects[i] = blendShapesInCombined[i].gameObject;
                    srcBlendShapeIdxs[i] = blendShapesInCombined[i].indexInSource;
                    targGameObjects[i] = _targetRenderer.gameObject;
                    targBlendShapeIdxs[i] = i;
                }

                map.SetBuffers(srcGameObjects, srcBlendShapeIdxs, targGameObjects, targBlendShapeIdxs);
            }

            /*
            [System.Obsolete("BuildSourceBlendShapeToCombinedIndexMap is deprecated. The map will be now be attached to the combined SkinnedMeshRenderer object as the MB_BlendShape2CombinedMap Component.")]
            public override Dictionary<MBBlendShapeKey, MBBlendShapeValue> BuildSourceBlendShapeToCombinedIndexMap()
            {
                if (_targetRenderer == null) return new Dictionary<MBBlendShapeKey, MBBlendShapeValue>();
                MB_BlendShape2CombinedMap mapComponent = _targetRenderer.GetComponent<MB_BlendShape2CombinedMap>();
                if (mapComponent == null) return new Dictionary<MBBlendShapeKey, MBBlendShapeValue>();
                return mapComponent.srcToCombinedMap.GenerateMapFromSerializedData();
            }
            */

            static void _ZeroArray(Vector3[] arr, int idx, int length)
            {
                int bound = idx + length;
                for (int i = idx; i < bound; i++)
                {
                    arr[i] = Vector3.zero;
                }
            }
        }
    }
}
