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
using GameScripts.HeroTeam.UI.Win;

namespace GameScripts.HeroTeam.UI.HeroTeamGame
{
    public partial class UIHeroTeamGame : UIWindowEx, IEventExecuteSink
    {
 
        protected override void OnUpdateUI()
        {

        }

        //@<<< ExecuteEventHandlerGenerator >>>
        //@<<< ButtonFuncGenerator >>>
        private void OnBtn_BtnFightClicked() //@Window 
        {
            btn_BtnFight.gameObject.SetActive(false);
            GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_START_GAME, DEventSourceType.SOURCE_TYPE_UI, 0, null);

            var cg = tran_TopPanel.GetComponent<CanvasGroup>();
            cg.gameObject.SetActive(true);
            cg.alpha = 0f;
            cg.DOFade(1, 1f);
        }

        protected override void OnSubscribeEvents()
        {
            base.OnSubscribeEvents();
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_BOSS_HP_CHANGED, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "UIHeroTeamGame:OnSubscribeEvents");
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_WIN, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "UIHeroTeamGame:OnSubscribeEvents");

        }

        protected override void OnUnsubscribeEvents()
        {
            base.OnUnsubscribeEvents();
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_BOSS_HP_CHANGED, DEventSourceType.SOURCE_TYPE_ENTITY, 0);
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_WIN, DEventSourceType.SOURCE_TYPE_ENTITY, 0);

        }

        public void OnExecute(ushort wEventID, byte bSrcType, uint dwSrcID, object pContext)
        {
            if (wEventID == DHeroTeamEvent.EVENT_BOSS_HP_CHANGED)
            {
                float health = Mathf.Clamp01(BossHpEventContext.Ins.Health);
                var size = img_UIHp_Boss_Foreground.rectTransform.sizeDelta;
                img_UIHp_Boss_Foreground.rectTransform.DOKill();
                var t = img_UIHp_Boss_Foreground.rectTransform.DOSizeDelta(new Vector2(783.8218f * health, 70.585f), 0.5f);
                t.SetEase(Ease.OutCirc);
                text_UIHp_Boss_Text.text = string.Format("{0:P}", health);
            }
            else if (wEventID == DHeroTeamEvent.EVENT_WIN)
            {
                //3秒后显示结束胜利页面
                GameManager.instance.AddTimer(3f, () =>
                {
                    UIWindowManager.Instance.ShowWindow<UIWin>();
                });
            }
        }
    }

    //@<<< EffectiveListGenerator >>>
    //@<<< FlexItemGenerator >>>


}
