/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIHeroTeamLogin.cs <FileHead_AutoGenerate>
Author：陈杰朝
Date：2025.05.20
Description：
***************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
using XGame.UI.Framework.Flex;
using XGame.UI.Framework.EffList;

namespace GameScripts.HeroTeam.UI.HeroTeamLogin
{
    public partial class UIHeroTeamLogin : UIWindowEx
    {
        protected override void OnUpdateUI()
        {
        }
		
		//@<<< ExecuteEventHandlerGenerator >>>
		//@<<< ButtonFuncGenerator >>>
		private void OnBtn_LoginClicked() //@Window 
		{
            GameGlobalEx.LoginModule.Login( );
        }

    }
	
	//@<<< EffectiveListGenerator >>>
	//@<<< FlexItemGenerator >>>
	
	
}
