/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIWin.cs <FileHead_AutoGenerate>
Author：陈杰朝
Date：2025.05.28
Description：胜利页面
***************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
using XGame.UI.Framework.Flex;
using XGame.UI.Framework.EffList;
using XClient.Common;
using GameScripts.HeroTeam.UI.HeroTeamGame;

namespace GameScripts.HeroTeam.UI.Win
{
	public partial class UIWin : UIWindowEx
	{
		protected override void OnUpdateUI()
		{
		}

		//@<<< ExecuteEventHandlerGenerator >>>
		//@<<< ButtonFuncGenerator >>>
		private void OnBtn_ReturnHomeClicked() //@Window 
		{
			Close();
		}

		protected override void AfterClose()
		{
			UIWindowManager.Instance.ShowWindow<UIHeroTeamGame>();
			GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_RESET_GAME, DEventSourceType.SOURCE_TYPE_UI, 0, null);
		}
	}

	//@<<< EffectiveListGenerator >>>
	//@<<< FlexItemGenerator >>>


}
