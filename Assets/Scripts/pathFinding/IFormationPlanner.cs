

using System.Collections.Generic;
using UnityEngine;

public interface IFormationPlanner
{
    /// <summary>
    /// 生成槽位
    /// </summary>
    /// <param name="center">阵型中心</param>
    /// <param name="count">生成槽位个数</param>
    /// <param name="spacing">槽距</param>
    /// <param name="facing">阵型朝向</param>
    /// <returns></returns>
    IList<Vector3> GenerateSlots(Vector3 center, int count, float spacing, Quaternion facing);
    
    /// <summary>
    /// 将槽位分配给单位
    /// </summary>
    /// <param name="units">单位</param>
    /// <param name="slots">槽位</param>
    /// <param name="keepHistory">是否使用上一次分配的位置</param>
    /// <returns></returns>
    Dictionary<Unit,Vector3> AssignSlots(IList<Unit> units, IList<Vector3> slots, bool keepHistory = true);

    /// <summary>
    /// 判断生成的槽位是否在可行走的区域内
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="projected"></param>
    /// <param name="maxDistance"></param>
    /// <returns></returns>
    bool TryProjectToNavMesh(Vector3 pos, out Vector3 projected, float maxDistance = 1.0f);
}
