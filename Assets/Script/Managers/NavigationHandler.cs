using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.AI;

public class NavigationHandler : MonoBehaviour
{
    public static NavigationHandler instance { get; private set; }
    public List<WayPointRuntime> WayPoints = new List<WayPointRuntime>();

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public List<WayPointRuntime> CreateNavigation(Transform _start, Transform _end)
    {
        List<WayPointRuntime> CurrentNavigation = new List<WayPointRuntime>();
        CurrentNavigation = FindShortestPath(_start, _end);
        CurrentNavigation.Add(new WayPointRuntime() { connections = new List<WayPointRuntime>(), PathName = "Target", Position = _end.position });
        
        if (CurrentNavigation.Count <= 1)
            Debug.LogError("Navigasyon olusturma basarisiz hatlarda bir kopukluk olabilir.");
        
        return CurrentNavigation;
    }

    private WayPointRuntime FindNearestWayPoint(Transform transform)
    {
        WayPointRuntime nearestWayPoint = null;
        float minDistance = float.MaxValue;

        foreach (var wayPoint in WayPoints)
        {
            float distance = Vector3.Distance(transform.position, wayPoint.Position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestWayPoint = wayPoint;
            }
        }

        return nearestWayPoint;
    }

    private List<WayPointRuntime> FindShortestPath(Transform startTransform, Transform endTransform)
    {
        WayPointRuntime startWayPoint = FindNearestWayPoint(startTransform);
        WayPointRuntime endWayPoint = FindNearestWayPoint(endTransform);

        if (startWayPoint == null || endWayPoint == null)
        {
            Debug.LogError("Start or End waypoint is null");
            return null;
        }

        return AStarPathfinding(startWayPoint, endWayPoint);
    }

    private List<WayPointRuntime> AStarPathfinding(WayPointRuntime start, WayPointRuntime end)
    {
        List<WayPointRuntime> openSet = new List<WayPointRuntime> { start };
        HashSet<WayPointRuntime> closedSet = new HashSet<WayPointRuntime>();

        Dictionary<WayPointRuntime, WayPointRuntime> cameFrom = new Dictionary<WayPointRuntime, WayPointRuntime>();
        Dictionary<WayPointRuntime, float> gScore = new Dictionary<WayPointRuntime, float>
        {
            [start] = 0
        };
        Dictionary<WayPointRuntime, float> fScore = new Dictionary<WayPointRuntime, float>
        {
            [start] = Vector3.Distance(start.Position, end.Position)
        };

        while (openSet.Count > 0)
        {
            WayPointRuntime current = GetLowestFScoreNode(openSet, fScore);
            //Debug.Log($"Current Node: {current.PathName} at Position: {current.Position}");

            if (current == end)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var neighborPosition in current.connections)
            {
                WayPointRuntime neighbor = FindWayPointByPosition(neighborPosition.PathName);

                if (neighbor == null)
                {
                    Debug.LogError($"Waypoint {current.PathName} has a null connection or invalid neighbor position.");
                    continue;
                }

                if (closedSet.Contains(neighbor))
                {
                    continue;
                }

                float tentativeGScore = gScore[current] + Vector3.Distance(current.Position, neighbor.Position);

                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                else if (tentativeGScore >= gScore.GetValueOrDefault(neighbor, float.MaxValue))
                {
                    continue;
                }

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + Vector3.Distance(neighbor.Position, end.Position);
            }
        }

        return ReconstructPath(cameFrom, GetFurthestNode(cameFrom, start));
    }

    private WayPointRuntime GetLowestFScoreNode(List<WayPointRuntime> openSet, Dictionary<WayPointRuntime, float> fScore)
    {
        WayPointRuntime lowest = openSet[0];
        float lowestScore = fScore.GetValueOrDefault(lowest, float.MaxValue);

        foreach (var node in openSet)
        {
            float score = fScore.GetValueOrDefault(node, float.MaxValue);
            if (score < lowestScore)
            {
                lowest = node;
                lowestScore = score;
            }
        }

        return lowest;
    }

    private WayPointRuntime GetFurthestNode(Dictionary<WayPointRuntime, WayPointRuntime> cameFrom, WayPointRuntime start)
    {
        WayPointRuntime furthestNode = start;
        float maxDistance = 0;

        foreach (var node in cameFrom.Keys)
        {
            float distance = Vector3.Distance(start.Position, node.Position);
            if (distance > maxDistance)
            {
                furthestNode = node;
                maxDistance = distance;
            }
        }

        return furthestNode;
    }

    private List<WayPointRuntime> ReconstructPath(Dictionary<WayPointRuntime, WayPointRuntime> cameFrom, WayPointRuntime current)
    {
        List<WayPointRuntime> totalPath = new List<WayPointRuntime> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }

        return totalPath;
    }

    private WayPointRuntime FindWayPointByPosition(string pathname)
    {
        foreach (var wayPoint in WayPoints)
        {
            if (wayPoint.PathName == pathname)
            {
                return wayPoint;
            }
        }
        Debug.LogError($"Waypoint with position {pathname} not found in WayPoints list.");
        return null;
    }

#if UNITY_EDITOR
    public List<WayPointData> CoreWayPoints = new List<WayPointData>();
    public float gizmoOffset = 0.1f;

    void FillGizmosFunc()
    {
        CoreWayPoints = FindObjectsByType<WayPointData>(FindObjectsSortMode.InstanceID).ToList();
        WayPoints.Clear();

        int index = 1;
        foreach (var item in CoreWayPoints)
        {
            item.transform.name = "W_" + index;
            index++;
        }

        foreach (var item in CoreWayPoints)
        {
            WayPointRuntime wpR = new();
            wpR.connections = new List<WayPointRuntime>();
            wpR.Position = item.transform.position;
            foreach (var item2 in item.connections)
            {
                WayPointRuntime conwpR = new();
                conwpR.Position = item2.transform.position;
                conwpR.PathName = item2.transform.name;
                wpR.connections.Add(conwpR);
            }
            wpR.PathName = item.transform.name;
            WayPoints.Add(wpR);
        }
    }

    public bool BehaviourToStruct;
    public bool ShowGizmos;
    public bool IncludeWayPoints;
    public bool ShowMeshes;
    public bool HideMeshes;
    public bool SetActiveTrue;
    public bool SetActiveFalse;

    public bool CreateExampleNavigation;
    public bool ClearExampleNavigation;

    public Transform NPC_StartPoint;
    public Transform NPC_Target;

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;

        if (CreateExampleNavigation)
        {
            CreateExampleNavigation = false;
            EditorOnlyTestWaypoint = CreateNavigation(NPC_StartPoint, NPC_Target);
            return;
        }

        if (ClearExampleNavigation)
        {
            ClearExampleNavigation = false;
            EditorOnlyTestWaypoint.Clear();
            return;
        }

        if (BehaviourToStruct)
        {
            BehaviourToStruct = false;
            FillGizmosFunc();
            return;
        }
        
        if (HideMeshes)
        {
            HideMeshes = false;
            HideMeshesFunc(false);
            return;
        }

        if (ShowMeshes)
        {
            ShowMeshes = false;
            HideMeshesFunc(true);
            return;
        }
        
        if (SetActiveTrue)
        {
            SetActiveTrue = false;
            SwitchActivityFunc(true);
            return;
        }
        
        if (SetActiveFalse)
        {
            SetActiveFalse = false;
            SwitchActivityFunc(false);
            return;
        }

        if (ShowGizmos)
        {
            if (EditorOnlyTestWaypoint.Count > 0)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < EditorOnlyTestWaypoint.Count - 1; i++)
                {
                    Gizmos.DrawLine(EditorOnlyTestWaypoint[i].Position + Vector3.up, EditorOnlyTestWaypoint[i + 1].Position + Vector3.up);
                }
                return;
            }

            foreach (var item in WayPoints)
            {
                if (IncludeWayPoints)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(item.Position, 1f);
                }

                int connections = item.connections.Count;
                for (int i = 0; i < connections; i++)
                {
                    if (i == 0)
                        Gizmos.color = Color.red;
                    else if (i == 1)
                        Gizmos.color = Color.blue;
                    else if (i == 2)
                        Gizmos.color = Color.black;
                    else if (i == 3)
                        Gizmos.color = Color.yellow;
                    else if (i == 4)
                        Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(item.Position + Vector3.up * (i / gizmoOffset), item.connections[i].Position + Vector3.up * (i / gizmoOffset));
                }
            }
        }
    }

    void HideMeshesFunc(bool _show)
    {
        foreach (var item in CoreWayPoints)
            item.GetComponent<Renderer>().enabled = _show;
    }
    
    void SwitchActivityFunc(bool _show)
    {
        foreach (var item in CoreWayPoints)
            item.gameObject.SetActive(_show);
    }

    public List<WayPointRuntime> EditorOnlyTestWaypoint = new List<WayPointRuntime>();

#endif

    [System.Serializable]
    public class WayPointRuntime
    {
        public string PathName;
        public List<WayPointRuntime> connections = new List<WayPointRuntime>();
        public Vector3 Position;
    }
}
