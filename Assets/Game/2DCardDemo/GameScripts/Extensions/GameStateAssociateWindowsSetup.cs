/**************************************************************************    
文　　件：UIStateAssociateWindows.cs
作　　者：
创建时间：2025.05.20
描　　述：UI状态关联窗口
***************************************************************************/

using UnityEngine;
using XGame.UI.Framework;

namespace GameScripts.CardDemo
{
	public class GameStateAssociateWindowsSetup : MonoBehaviour, IStateAssociateWindowsSetup
    {
        public void SetupStateAssociateWindows()
        {
			GameStateAssociateWindows.AddShowOnEnterWindow<GameScripts.CardDemo.UI.Login.UILogin>(1);
			GameStateAssociateWindows.AddCloseOnLeaveWindow<GameScripts.CardDemo.UI.Login.UILogin>(1);
			GameStateAssociateWindows.AddShowOnEnterWindow<GameScripts.CardDemo.UI.Main.UIMain>(2);
			GameStateAssociateWindows.AddShowOnEnterWindow<GameScripts.CardDemo.UI.MainNavs.UIMainNavs>(2);

        }
    }
}
