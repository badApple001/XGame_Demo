/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIHeroTeamGame.cs <FileHead_AutoGenerate>
Author：陈杰朝
Date：2025.05.20
Description：游戏内的UI
***************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
using XGame.UI.Framework.Flex;
using XGame.UI.Framework.EffList;
using XGame.EventEngine;
using XClient.Common;
using DG.Tweening;

namespace GameScripts.HeroTeam.UI.HeroTeamGame
{
    public partial class UIHeroTeamGame : UIWindowEx
    {
        protected override void OnUpdateUI( )
        {

        }

        //@<<< ExecuteEventHandlerGenerator >>>
        //@<<< ButtonFuncGenerator >>>
        private void OnBtn_BtnFightClicked( ) //@Window 
        {
            btn_BtnFight.gameObject.SetActive( false );
            GameGlobal.EventEgnine.FireExecute( DHeroTeamEvent.EVENT_START_BATTLE, DEventSourceType.SOURCE_TYPE_UI, 0, null );

            var cg = tran_TopPanel.GetComponent<CanvasGroup>( );
            cg.gameObject.SetActive( true );
            cg.alpha = 0f;
            cg.DOFade( 1, 1f );


        }


    }

    //@<<< EffectiveListGenerator >>>
    //@<<< FlexItemGenerator >>>


}
