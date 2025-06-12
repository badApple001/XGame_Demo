/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIFail.cs <FileHead_AutoGenerate>
Author：陈杰朝
Date：2025.06.12
Description：
***************************************************************************/

using XGame.UI.Framework;
using XClient.Common;
using GameScripts.HeroTeam.UI.HeroTeamGame;

namespace GameScripts.HeroTeam.UI.Fail
{
	public partial class UIFail : UIWindowEx
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
