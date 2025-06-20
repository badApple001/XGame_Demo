using UnityEngine;
using UnityEditor;

namespace GameScripts.HeroTeam
{


    [CustomEditor(typeof(CalcHeroGridCoord)), CanEditMultipleObjects]
    public class CalcHeroGridCoordEditor : Editor
    {
        Vector3 m_v3OriginPos = Vector3.zero;
        Vector3 m_v3OrginOffset = Vector3.zero;
        private void OnEnable()
        {
            CalcHeroGridCoord _target = (target as CalcHeroGridCoord);
            m_v3OriginPos = _target.transform.position;
        }

        private void OnSceneGUI()
        {
            CalcHeroGridCoord _target = (target as CalcHeroGridCoord);

            if (_target == null || (_target != null && !_target.m_bEnableEditor)) return;

            m_v3OrginOffset = _target.m_v3OrginOffset;
            // DrawSolid(_target.__LongRangeAttackerColor, _target.__LongRangeAttackerRadius);
            // DrawSolid(_target.__LavaColor, _target.__LavaRadius);
            // // DrawSolid(_target.__CloseCombatantColor, _target.__CloseCombatantRadius);
            DrawFanShape2D(
                m_v3OrginOffset + m_v3OriginPos,
                Vector3.right,
                _target.HunterScope[2],
                _target.HunterScope[1],
                _target.HunterScope[0],
                new Color(0.3f, 1, 0.6f, 0.6f), Color.black);

            DrawFanShape2D(
                m_v3OrginOffset + m_v3OriginPos,
                Vector3.right,
                _target.TankScope[2],
                _target.TankScope[1],
                _target.TankScope[0],
                new Color(1, 1, 1, 0.6f), Color.black);

            DrawFanShape2D(
                  m_v3OrginOffset + m_v3OriginPos,
                  Vector3.right,
                  _target.WarriorScope[2],
                  _target.WarriorScope[1],
                  _target.WarriorScope[0],
                  new Color(0, 0, 0, 0.6f), Color.black);


            DrawSolid(new Color(1, 0, 0, 0.8f), _target.BossCollision);


            var cells = _target.tankCellSector.____GetCells();
            foreach (var cell in cells)
            {
                Handles.DrawWireCube(cell.Center, cell.Rect.size);
            }

            cells = _target.warriorCellSector.____GetCells();
            foreach (var cell in cells)
            {
                Handles.DrawWireCube(cell.Center, cell.Rect.size);
            }

            cells = _target.hunterCellSector.____GetCells();
            foreach (var cell in cells)
            {
                Handles.DrawWireCube(cell.Center, cell.Rect.size);
            }

            // DrawFanShape(Vector3.zero, Vector3.down, 30f, 60, 270, Color.white, Color.white);

            DrawSceneCameraViewBorder();
        }


        public void DrawFanShape2D(Vector3 center, Vector3 forward, float radius, float angle, float start_angle, Color fill_color, Color line_color)
        {
            Vector3 normal = Vector3.forward; // 沿 Z 轴竖直

            // 起始向量
            Quaternion startRot = Quaternion.AngleAxis(-angle / 2 + start_angle, normal);
            Vector3 from = startRot * forward;

            Handles.color = fill_color;
            Handles.DrawSolidArc(center, normal, from, angle, radius);

            Handles.color = line_color;
            Vector3 to = Quaternion.AngleAxis(angle / 2 + start_angle, normal) * forward;
            Handles.DrawLine(center, center + from * radius);
            Handles.DrawLine(center, center + to * radius);
            Handles.DrawWireArc(center, normal, from, angle, radius);
        }

        /// <summary>
        /// 绘制一个扇形区域（Scene 视图专用）
        /// </summary>
        /// <param name="center">扇形中心点</param>
        /// <param name="forward">面朝方向</param>
        /// <param name="radius">半径</param>
        /// <param name="angle">扇形角度（单位：度）</param>
        /// <param name="start_angle">起始角度（相对于 forward 的偏移，单位：度）</param>
        /// <param name="fill_color">填充颜色（带透明）</param>
        /// <param name="line_color">边缘线颜色</param>
        public void DrawFanShape(Vector3 center, Vector3 forward, float radius, float angle, float start_angle, Color fill_color, Color line_color)
        {
            Vector3 normal = Vector3.up; // 可根据需要更改
            Quaternion startRot = Quaternion.Euler(0, 0, -angle / 2 + start_angle);
            Vector3 from = startRot * forward;

            Handles.color = fill_color;
            Handles.DrawSolidArc(center, normal, from, angle, radius);

            Handles.color = line_color;
            Vector3 to = Quaternion.Euler(0, 0, angle / 2 + start_angle) * forward;
            Handles.DrawLine(center, center + from * radius);
            Handles.DrawLine(center, center + to * radius);
            Handles.DrawWireArc(center, normal, from, angle, radius);
        }

        public void DrawSolid(Color color, float radius)
        {
            Handles.color = color;
            Handles.DrawSolidDisc(m_v3OriginPos + m_v3OrginOffset, Vector3.forward, radius);
        }



        public void DrawSceneCameraViewBorder()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Handles.color = Color.yellow;

            float z = cam.orthographic ? (0 - cam.transform.position.z) : cam.farClipPlane;

            Vector3[] corners = new Vector3[4];
            corners[0] = cam.ViewportToWorldPoint(new Vector3(0, 0, z)); // Bottom-left
            corners[1] = cam.ViewportToWorldPoint(new Vector3(1, 0, z)); // Bottom-right
            corners[2] = cam.ViewportToWorldPoint(new Vector3(1, 1, z)); // Top-right
            corners[3] = cam.ViewportToWorldPoint(new Vector3(0, 1, z)); // Top-left

            for (int i = 0; i < 4; i++)
            {
                Handles.DrawLine(corners[i], corners[(i + 1) % 4]);
            }
        }
    }

}