/**************************************************************************    
文　　件：UIStateAssociateWindows.cs
作　　者：
创建时间：2025.05.20
描　　述：UI状态关联窗口
***************************************************************************/

using UnityEngine;
using XGame.UI.Framework;

namespace GameScripts.Tank
{
	public class GameStateAssociateWindowsSetup : MonoBehaviour, IStateAssociateWindowsSetup
    {
        public void SetupStateAssociateWindows()
        {
			GameStateAssociateWindows.AddShowOnEnterWindow<GameScripts.Tank.UI.TankLogin.UITankLogin>(1);
			GameStateAssociateWindows.AddCloseOnLeaveWindow<GameScripts.Tank.UI.TankLogin.UITankLogin>(1);
			GameStateAssociateWindows.AddShowOnEnterWindow<GameScripts.Tank.UI.TankNavs.UITankNavs>(2);

        }
    }
}
