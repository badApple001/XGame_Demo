using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 扇形区域 均匀生成方块
/// </summary>
public class SectorCellGenerator
{
    public struct Cell
    {
        public Rect Rect;
        public Vector2 Center => Rect.center;
    }

    private List<Cell> m_ValidCells = new List<Cell>();

    public void Init(
        Vector2 center,
        float minRadius,
        float maxRadius,
        float sectorAngleDeg,
        float sectorDirectionDeg,
        Vector2 cellSize,
        float angleStepDeg)
    {
        m_ValidCells.Clear();

        float halfAngle = sectorAngleDeg / 2f;
        float startAngle = -halfAngle;
        float endAngle = halfAngle;

        for (float angle = startAngle; angle < endAngle; angle += angleStepDeg)
        {
            float angleRad = (angle + sectorDirectionDeg) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

            for (float r = minRadius; r < maxRadius; r += cellSize.y)
            {
                Vector2 cellCenter = center + dir * r;
                Rect cellRect = new Rect(
                    cellCenter - cellSize / 2f,
                    cellSize
                );

                // 检查该矩形是否完全在扇形内
                if (IsRectInSector(cellRect, center, minRadius, maxRadius, startAngle, endAngle, sectorDirectionDeg))
                {
                    m_ValidCells.Add(new Cell { Rect = cellRect });
                }
            }
        }
    }

    public bool HasCell() => m_ValidCells.Count > 0;

    public Vector2 GetRandomPoint()
    {
        if (m_ValidCells.Count == 0)
            return Vector2.zero;

        int idx = Random.Range(0, m_ValidCells.Count);
        var cell = m_ValidCells[idx];

        // 在 Cell 内随机一个点
        float x = Random.Range(cell.Rect.xMin, cell.Rect.xMax);
        float y = Random.Range(cell.Rect.yMin, cell.Rect.yMax);
        return new Vector2(x, y);
    }

    // 判定一个矩形是否在扇形区域中（近似：判断4个角都在内）
    private bool IsRectInSector(Rect rect, Vector2 center, float minR, float maxR, float startAngle, float endAngle, float directionDeg)
    {
        Vector2[] corners = new Vector2[4]
        {
            new Vector2(rect.xMin, rect.yMin),
            new Vector2(rect.xMax, rect.yMin),
            new Vector2(rect.xMin, rect.yMax),
            new Vector2(rect.xMax, rect.yMax),
        };

        foreach (var pt in corners)
        {
            Vector2 dir = pt - center;
            float dist = dir.magnitude;

            if (dist < minR || dist > maxR)
                return false;

            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float relativeAngle = Mathf.DeltaAngle(directionDeg, ang);

            if (relativeAngle < startAngle || relativeAngle > endAngle)
                return false;
        }

        return true;
    }

    public List<Cell> ____GetCells() => m_ValidCells;
}
