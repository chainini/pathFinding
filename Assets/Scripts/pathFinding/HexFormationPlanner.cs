using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HexFormationPlanner : IFormationPlanner
{
    public IList<Vector3> GenerateSlots(Vector3 center, int count, float spacing, Quaternion facing)
    {
        var result = new List<Vector3>(count);
        if(count <= 0) return result;
        
        result.Add(center);
        if(result.Count >= count) return result;

        int ring = 1;
        Vector3[] dir =
        {
            new Vector3(1, 0, 0), new Vector3(0.5f, 0, Mathf.Sqrt(3) / 2f),
            new Vector3(-0.5f, 0, Mathf.Sqrt(3) / 2f), new Vector3(-1, 0, 0),
            new Vector3(-0.5f, 0, -Mathf.Sqrt(3) / 2f), new Vector3(0.5f, 0, -Mathf.Sqrt(3) / 2f),
        };

        while (result.Count < count)
        {
            int maxInRing = 6 * ring;
            int remaining = count - result.Count;
            int toPlace = Mathf.Min(maxInRing, remaining);
            
            float angleStep = 360f / toPlace;
            float startAngle = 0f;

            for (int i = 0; i < toPlace; i++)
            {
                float angle = angleStep + i * angleStep;
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * spacing * ring,
                    0,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * spacing * ring
                );
                Vector3 pos = center + facing * offset;
                result.Add(pos);
            }
            
            
            // Vector3 p = center + (facing * (dir[5] * spacing)) * ring;
            // for (int side = 0; side < 6 && result.Count < count; side++)
            // {
            //     Vector3 step = facing * (dir[side] * spacing);
            //     for (int i = 0; i < ring && result.Count < count; i++)
            //     {
            //         p += step;
            //         result.Add(p);
            //     }
            // }

            ring++;
        }
        
        return result;
    }

    
    private readonly Dictionary<Unit, Vector3> _lastAssign = new Dictionary<Unit, Vector3>();
    public Dictionary<Unit, Vector3> AssignSlots(IList<Unit> units, IList<Vector3> slots, bool keepHistory = true)
    {
        var result = new Dictionary<Unit, Vector3>(units.Count);
        if(units.Count == 0 || slots.Count == 0) return result;
        
        var userdSlots = new HashSet<Vector3>();

        if (keepHistory)
        {
            foreach (var u in units)
            {
                if (_lastAssign.TryGetValue(u, out var lastSlot))
                {
                    Vector3? matchingSlot = null;
                    float minDist = float.MaxValue;

                    foreach (var slot in slots)
                    {
                        float dist = Vector3.Distance(lastSlot, slot);
                        if (dist < minDist && dist < 0.5f)
                        {
                            minDist = dist;
                            matchingSlot = slot;
                        }

                        if (matchingSlot.HasValue && !userdSlots.Contains(matchingSlot.Value))
                        {
                            result[u] = matchingSlot.Value;
                            userdSlots.Add(matchingSlot.Value);
                        }
                    }
                }
            }
        }
        
        var remainingUnits = new List<Unit>();
        foreach (var u in units)
        {
            if (!result.ContainsKey(u))
            {
                remainingUnits.Add(u);
            }
        }

        if (remainingUnits.Count > 0)
        {
            var unitDistances = new List<(Unit unit, float totalDistance, List<(Vector3 slot, float distance)> slotDistances)>();

            foreach (var u in remainingUnits)
            {
                var slotDistances = new List<(Vector3 slot, float distance)>();
                float totalDistance = 0;

                foreach (var slot in slots)
                {
                    if (!userdSlots.Contains(slot))
                    {
                        float dist = Vector3.Distance(u.transform.position, slot);
                        slotDistances.Add((slot, dist));
                        totalDistance += dist;
                    }
                }

                if (slotDistances.Count > 0)
                {
                    slotDistances.Sort((a, b) => a.distance.CompareTo(b.distance));
                    unitDistances.Add((u, totalDistance, slotDistances));
                }
            }
            
            unitDistances.Sort((a,b) => b.totalDistance.CompareTo(a.totalDistance));

            foreach (var (unit, totalDist, slotDistance) in unitDistances)
            {
                foreach (var (slot, distance) in slotDistance)
                {
                    if (!userdSlots.Contains(slot))
                    {
                        result[unit] = slot;
                        userdSlots.Add(slot);
                        break;
                    }
                }
            }
        }
        
        _lastAssign.Clear();
        foreach (var kv in result)
        {
            _lastAssign[kv.Key] = kv.Value;
        }
        
        return result;
        
        
        
        // var candidates = new List<(float cost, Unit u, int slotIndex)>(units.Count * slots.Count);
        // float penalty = keepHistory ? 1.5f : 0f;
        // for (int ui = 0; ui < units.Count; ui++)
        // {
        //     var u = units[ui];
        //     for (int si = 0; si < slots.Count; si++)
        //     {
        //         float d = Vector3.Distance(u.transform.position, slots[si]);
        //         if (keepHistory && _lastAssign.TryGetValue(u, out var last) &&
        //             (last - slots[si]).sqrMagnitude > 0.0001f)
        //         {
        //             d += penalty;
        //         }
        //         candidates.Add((d,u,si));
        //     }
        // }
        //
        // candidates.Sort((a, b) => a.cost.CompareTo(b.cost));
        //
        // var userdSlots = new HashSet<Unit>();
        // foreach (var c in candidates)
        // {
        //     if(result.ContainsKey(c.u)) continue;
        //     if(userdSlots.Contains(c.slotIndex)) continue;
        //     result[c.u] = slots[c.slotIndex];
        //     userdSlots.Add(c.slotIndex);
        //     if (result.Count == units.Count || userdSlots.Count == slots.Count) break;
        // }
        //
        // _lastAssign.Clear();
        // foreach (var kv in result)
        // {
        //     _lastAssign[kv.Key] = kv.Value;
        // }
        //
        // return result;
    }

    public bool TryProjectToNavMesh(Vector3 pos, out Vector3 projected, float maxDistance = 1)
    {
        //#if UNITY_ENGINE
	        UnityEngine.AI.NavMeshHit hit;
	        if (UnityEngine.AI.NavMesh.SamplePosition(pos, out hit, maxDistance, UnityEngine.AI.NavMesh.AllAreas))
	        {
		        projected = hit.position;
		        return true;
	        }
        //#endif
        projected = pos;
        return false;
    }
}
