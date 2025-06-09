using UnityEngine;
using XClient.Common;
using XGame.FlowText;
using XGame.Utils;
namespace GameScripts.HeroTeam
{

    public enum FlowTextType
    {
        Normal = 101,
        CriticalHit = 102,
        Treat = 120
    }

    public class FlowTextManager : Singleton<FlowTextManager>
    {
        private Camera m_Cam = Camera.main;
        public void ShowFlowText(FlowTextType type, string content, Vector3 worldPos)
        {
            var cam = m_Cam;
            var screenPos = cam.WorldToScreenPoint(worldPos);
            var data = new FlowTextContext()
            {
                content = content,
                startPosition = GameGlobal.FlowTextManager.ScreenPositionToLayerLocalPosition((int)type, screenPos, cam),
            };

            GameGlobal.FlowTextManager.AddFlowText((int)type, data);
        }
    }

}