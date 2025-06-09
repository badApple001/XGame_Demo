/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIHeroTeamGame.cs <FileHead_AutoGenerate>
Author：陈杰朝
Date：2025.05.20
Description：游戏内的UI
***************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
using XGame.EventEngine;
using XClient.Common;
using DG.Tweening;
using GameScripts.HeroTeam.UI.Win;
using System.Text;
using EasyMobileInput;
using System.Collections.Generic;

namespace GameScripts.HeroTeam.UI.HeroTeamGame
{
    /// <summary>
    /// 英雄小队游戏主UI窗口
    /// </summary>
    public partial class UIHeroTeamGame : UIWindowEx, IEventExecuteSink
    {

        // 仇恨列表根节点
        private Transform m_trHateListRoot;
        // 伤害列表根节点
        private Transform m_trHarmListRoot;
        // 治疗列表根节点
        private Transform m_trCuringListRoot;
        //摇杆
        private Joystick m_Joystick;
        // 是否展开属性面板
        private bool m_OpenCollapsed = false;
        //下次可以使用技能的时间
        private Dictionary<string, float> m_dicNextCanUseSkillTime = new Dictionary<string, float>();


        /// <summary>
        /// 刷新UI时调用
        /// </summary>
        protected override void OnUpdateUI()
        {
            // 此处可添加UI刷新逻辑
        }

        /// <summary>
        /// 初始化UI组件
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();

            // 获取属性面板下的各个排行榜根节点
            var propertyPanel = tran_PropertyContent;
            m_trHateListRoot = propertyPanel.GetChild(0);
            m_trHarmListRoot = propertyPanel.GetChild(1);
            m_trCuringListRoot = propertyPanel.GetChild(2);

            // 创建排行榜条目模板（最多24个）
            void CreateTemplate(Transform root)
            {
                var pref = root.GetChild(1).gameObject;
                for (int i = 0; i < 24; i++) GameObject.Instantiate(pref, root);
            }
            CreateTemplate(m_trHateListRoot);
            CreateTemplate(m_trHarmListRoot);
            CreateTemplate(m_trCuringListRoot);
        }

        private void SetButtonCD(UnityEngine.UI.Button btn, float cd)
        {
            btn.interactable = false;
            var maskImg = btn.transform.GetChild(0).GetComponent<Image>();
            maskImg.gameObject.SetActive(true);
            maskImg.DOKill();
            maskImg.fillAmount = 1f;
            maskImg.DOFillAmount(0f, cd).OnComplete(() =>
            {
                btn.interactable = true;
                maskImg.gameObject.SetActive(false);
            });
        }


        /// <summary>
        /// 刷新单个排行榜条目
        /// </summary>
        /// <param name="index">条目索引</param>
        /// <param name="trItem">条目Transform</param>
        /// <param name="monster">怪物数据</param>
        /// <param name="sb">字符串构建器</param>
        /// <param name="func">伤害转换函数（可选）</param>
        private void RefreshRankItem(int index, Transform trItem, ISpineCreature monster, ref StringBuilder sb, HarmConverterFunc func = null)
        {

            //转换伤害/治疗
            int realValue = func != null ? func(monster) : monster.GetTotalHarm();
            trItem.SafeSetActive(true);
            // if (realValue == 0) return;


            var slider = (RectTransform)trItem.GetChild(0);
            var harmText = slider.GetChild(0).GetComponent<Text>();
            var rankText = trItem.GetChild(1).GetComponent<Text>();

            // 计算进度条长度
            var nSliderSizeX = Mathf.Clamp01(realValue * 1f / 40000) * 100f;
            var size = slider.sizeDelta;
            size.x = 140 + nSliderSizeX;
            slider.DOKill();
            slider.DOSizeDelta(size, 0.5f);

            // 设置排名文本
            sb.Clear();
            sb.Append(index + 1);
            sb.Append(".");
            sb.Append(monster.name);
            rankText.text = sb.ToString();

            // 设置伤害/治疗数值文本
            sb.Clear();
            sb.Append(realValue);
            harmText.text = sb.ToString();
        }

        /// <summary>
        /// 伤害转换函数委托
        /// </summary>
        private delegate int HarmConverterFunc(ISpineCreature monster);

        /// <summary>
        /// 伤害转换实现：如果是贤者职业则返回0，否则返回总伤害
        /// </summary>
        private int HarmConverter(ISpineCreature monster) => monster.GetHeroCls() == HeroClassDef.SAGE ? 0 : monster.GetTotalHarm();

        /// <summary>
        /// 治疗转换实现 ：如果非贤者职业返回0
        /// </summary>
        /// <param name="monster"></param>
        /// <returns></returns>
        private int CuringConverter(ISpineCreature monster) => monster.GetHeroCls() != HeroClassDef.SAGE ? 0 : monster.GetTotalHarm();

        /// <summary>
        /// 仇恨转换实现：返回怪物的仇恨值
        /// </summary>
        private int HateConverter(ISpineCreature monster) => monster.GetHatred();

        /// <summary>
        /// 刷新排行榜数据
        /// </summary>
        /// <param name="pContext">排行榜上下文数据</param>
        private void RefreshRank(RefreshRankContext pContext)
        {
            Debug.Log("刷新列表");
            var arrCuringRank = pContext.arrCuringRank;
            var arrHateRank = pContext.arrHateRank;
            var arrHarmRank = pContext.arrHarmRank;
            var sb = new StringBuilder();

            // 刷新仇恨排行榜
            for (int i = 0; i < m_trHateListRoot.childCount - 1; i++)
            {
                var trItem = m_trHateListRoot.GetChild(i + 1);
                if (i < arrHateRank.Count)
                    RefreshRankItem(i, trItem, arrHateRank[i], ref sb, HateConverter);
                else
                    trItem.SafeSetActive(false);
            }

            // 刷新伤害排行榜
            for (int i = 0; i < m_trHarmListRoot.childCount - 1; i++)
            {
                var trItem = m_trHarmListRoot.GetChild(i + 1);
                if (i < arrHarmRank.Count)
                    RefreshRankItem(i, trItem, arrHarmRank[i], ref sb, HarmConverter);
                else
                    trItem.SafeSetActive(false);
            }

            // 刷新治疗排行榜
            for (int i = 0; i < m_trCuringListRoot.childCount - 1; i++)
            {
                var trItem = m_trCuringListRoot.GetChild(i + 1);
                if (i < arrCuringRank.Count)
                    RefreshRankItem(i, trItem, arrCuringRank[i], ref sb, CuringConverter);
                else
                    trItem.SafeSetActive(false);
            }
        }


        //@<<< ExecuteEventHandlerGenerator >>>
        //@<<< ButtonFuncGenerator >>>
        //散开
        private void OnBtn_BtnAvoidanceClicked() //@Window 
        {
            if (m_dicNextCanUseSkillTime["Avoidance"] > Time.time)
            {
                return;
            }
            m_dicNextCanUseSkillTime["Avoidance"] = Time.time + 5;
            SetButtonCD(btn_BtnAvoidance, 5);

            GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_LEADER_SKILL_AVOIDANCE, DEventSourceType.SOURCE_TYPE_UI, 0, null);
        }

        //攻击
        private void OnBtn_BtnAttackClicked() //@Window 
        {
            if (m_dicNextCanUseSkillTime["Attack"] > Time.time)
            {
                return;
            }
            m_dicNextCanUseSkillTime["Attack"] = Time.time + 5;
            SetButtonCD(btn_BtnAttack, 5);


            GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_LEADER_SKILL_ATTACK, DEventSourceType.SOURCE_TYPE_UI, 0, null);
        }

        //治疗
        private void OnBtn_BtnTreatClicked() //@Window 
        {
            if (m_dicNextCanUseSkillTime["Treat"] > Time.time)
            {
                return;
            }
            m_dicNextCanUseSkillTime["Treat"] = Time.time + 5;
            SetButtonCD(btn_BtnTreat, 5);

            GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_LEADER_SKILL_TREAT, DEventSourceType.SOURCE_TYPE_UI, 0, null);

        }

        /// <summary>
        /// 折叠/展开属性面板按钮点击事件
        /// </summary>
        private void OnBtn_CollapseClicked() //@Window 
        {
            m_OpenCollapsed = !m_OpenCollapsed;

            float height = m_OpenCollapsed ? 2000f : 65f;
            img_PropertyPanel.rectTransform.DOKill();
            var size = img_PropertyPanel.rectTransform.sizeDelta;
            size.y = height;
            img_PropertyPanel.rectTransform.DOSizeDelta(size, 1.0f).SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// 开始战斗按钮点击事件
        /// </summary>
        private void OnBtn_BtnFightClicked() //@Window 
        {
            btn_BtnFight.gameObject.SetActive(false);
            GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_START_GAME, DEventSourceType.SOURCE_TYPE_UI, 0, null);

            var cg = tran_TopPanel.GetComponent<CanvasGroup>();
            cg.gameObject.SetActive(true);
            cg.alpha = 0f;
            cg.DOFade(1, 1f);
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        protected override void OnSubscribeEvents()
        {
            base.OnSubscribeEvents();

            //注册事件
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_BOSS_HP_CHANGED, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "UIHeroTeamGame:OnSubscribeEvents");
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_WIN, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "UIHeroTeamGame:OnSubscribeEvents");
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_START_GAME, DEventSourceType.SOURCE_TYPE_UI, 0, "UIHeroTeamGame:OnSubscribeEvents");
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_REFRESH_RANKDATA, DEventSourceType.SOURCE_TYPE_MONSTERSYSTEAM, 0, "UIHeroTeamGame:OnSubscribeEvents");
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_JOYSTICK_ACTIVE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "UIHeroTeamGame:OnSubscribeEvents");

            //游戏摇杆事件注册
            var joystick = tran_JoystickParent.GetComponentInChildren<Joystick>();
            if (null != joystick)
            {
                m_Joystick = joystick;
                joystick.OnInputChanged += OnJoystickInputChanged;
                joystick.OnInputStarted += OnJoystickInputStarted;
                joystick.OnInputEnded += OnJoystickInputEnded;
            }
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        protected override void OnUnsubscribeEvents()
        {
            base.OnUnsubscribeEvents();


            //移除事件
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_BOSS_HP_CHANGED, DEventSourceType.SOURCE_TYPE_ENTITY, 0);
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_WIN, DEventSourceType.SOURCE_TYPE_ENTITY, 0);
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_START_GAME, DEventSourceType.SOURCE_TYPE_UI, 0);
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_REFRESH_RANKDATA, DEventSourceType.SOURCE_TYPE_MONSTERSYSTEAM, 0);
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_JOYSTICK_ACTIVE, DEventSourceType.SOURCE_TYPE_ENTITY, 0);

            //游戏摇杆事件移除
            if (null != m_Joystick)
            {
                var joystick = m_Joystick;
                joystick.OnInputChanged -= OnJoystickInputChanged;
                joystick.OnInputStarted -= OnJoystickInputStarted;
                joystick.OnInputEnded -= OnJoystickInputEnded;
            }
        }

        /// <summary>
        /// 事件回调处理
        /// </summary>
        /// <param name="wEventID">事件ID</param>
        /// <param name="bSrcType">事件源类型</param>
        /// <param name="dwSrcID">事件源ID</param>
        /// <param name="pContext">事件上下文</param>
        public void OnExecute(ushort wEventID, byte bSrcType, uint dwSrcID, object pContext)
        {
            if (wEventID == DHeroTeamEvent.EVENT_BOSS_HP_CHANGED)
            {
                // BOSS血量变化，刷新血条
                float health = Mathf.Clamp01(BossHpEventContext.Ins.Health);
                var size = img_UIHp_Boss_Foreground.rectTransform.sizeDelta;
                img_UIHp_Boss_Foreground.rectTransform.DOKill();
                var t = img_UIHp_Boss_Foreground.rectTransform.DOSizeDelta(new Vector2(783.8218f * health, 70.585f), 0.5f);
                t.SetEase(Ease.OutCirc);
                text_UIHp_Boss_Text.text = string.Format("{0:P}", health);
            }
            else if (wEventID == DHeroTeamEvent.EVENT_WIN)
            {
                // 3秒后显示胜利页面
                GameManager.Instance.AddTimer(3f, () =>
                {
                    UIWindowManager.Instance.ShowWindow<UIWin>();
                });
            }
            else if (DHeroTeamEvent.EVENT_START_GAME == wEventID)
            {
                // 开始游戏时折叠属性面板
                // OnBtn_CollapseClicked();
                tran_ParametersPanel.gameObject.SetActive(true);
            }
            else if (DHeroTeamEvent.EVENT_JOYSTICK_ACTIVE == wEventID)
            {
                tran_JoystickParent.gameObject.SetActive(true);
                tran_LeaderSkillPanel.gameObject.SetActive(true);



                //技能CD 
                // TODO: 后续由技能管理器来处理
                m_dicNextCanUseSkillTime["Attack"] = Time.time + 5;
                m_dicNextCanUseSkillTime["Avoidance"] = Time.time + 5;
                m_dicNextCanUseSkillTime["Treat"] = Time.time + 5;
                SetButtonCD(btn_BtnAttack, 5);
                SetButtonCD(btn_BtnAvoidance, 5);
                SetButtonCD(btn_BtnTreat, 5);
            }
            else if (DHeroTeamEvent.EVENT_REFRESH_RANKDATA == wEventID && pContext is RefreshRankContext ctx)
            {
                // 刷新排行榜
                RefreshRank(ctx);
            }
        }


        private void OnJoystickInputChanged(Vector3 old, Vector3 current)
        {
            // Debug.Log("joystick drag: " + current.ToString());

            var pContext = JoystickEventContext.Ins;
            pContext.delta = current;
            GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_JOYSTICK_CHANGED, DEventSourceType.SOURCE_TYPE_UI, 0, pContext);
        }

        private void OnJoystickInputStarted()
        {
            Debug.Log("joystick started");

            var pContext = JoystickEventContext.Ins;
            GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_JOYSTICK_STARTED, DEventSourceType.SOURCE_TYPE_UI, 0, pContext);

        }

        private void OnJoystickInputEnded()
        {
            Debug.Log("joystick ended");

            var pContext = JoystickEventContext.Ins;
            GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_JOYSTICK_ENDED, DEventSourceType.SOURCE_TYPE_UI, 0, pContext);

        }


    }
    //@<<< EffectiveListGenerator >>>
    //@<<< FlexItemGenerator >>>

}
