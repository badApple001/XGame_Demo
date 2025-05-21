/**************************************************************************    
文　　件：UIStateAssociateWindows.cs
作　　者：
创建时间：2025.05.21
描　　述：UI状态关联窗口
***************************************************************************/

using UnityEngine;
using XGame.UI.Framework;

namespace GameScripts.HeroTeam
{
	public class GameStateAssociateWindowsSetup : MonoBehaviour, IStateAssociateWindowsSetup
    {
        public void SetupStateAssociateWindows()
        {
			GameStateAssociateWindows.AddShowOnEnterWindow<GameScripts.HeroTeam.UI.HeroTeamLogin.UIHeroTeamLogin>(1);
			GameStateAssociateWindows.AddCloseOnLeaveWindow<GameScripts.HeroTeam.UI.HeroTeamLogin.UIHeroTeamLogin>(1);
			GameStateAssociateWindows.AddShowOnEnterWindow<GameScripts.HeroTeam.UI.HeroTeamGame.UIHeroTeamGame>(2);

        }
    }
}
