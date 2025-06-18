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
using System.Text;
using EasyMobileInput;
using Game;
using System;
using System.Collections;
using GameScripts.HeroTeam.UI.Win;
using GameScripts.HeroTeam.UI.Fail;
using UnityEngine.EventSystems;

namespace GameScripts.HeroTeam.UI.HeroTeamGame
{

    public class BossRageEventContext
    {
        public static BossRageEventContext Instance { private set; get; } = new BossRageEventContext();
        public bool showHintWindow = false;
        public float bossCDScale = 1f;
    }


    /// <summary>
    /// 团长别开腔 主UI窗口
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
        /// <summary>
        /// 游戏时间
        /// </summary>
        private int m_nGameTime;

        private Sequence m_FlashTween;

        private bool m_bShowBossBerserkWindows = false;

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

            Debug.Log("####### UIHeroTeamGame.OnInitialize");

            m_bShowBossBerserkWindows = false;

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



            //注册技能
            var skillProperty = LeaderManager.Instance.GetLeaderSkillProperty(LeaderSkillType.Fire);
            skillProperty.BindAction(() => btn_BtnAttack.image.color = Color.white, () => btn_BtnAttack.image.color = Color.gray);

            var skillProperty2 = LeaderManager.Instance.GetLeaderSkillProperty(LeaderSkillType.Dodge);
            skillProperty2.BindAction(() => btn_BtnAvoidance.image.color = Color.white, () => btn_BtnAvoidance.image.color = Color.gray);

            var skillProperty3 = LeaderManager.Instance.GetLeaderSkillProperty(LeaderSkillType.Purify);
            skillProperty3.BindAction(() => btn_BtnTreat.image.color = Color.white, () => btn_BtnTreat.image.color = Color.gray);

            LeaderManager.Instance.BindMpChangedEvent((int mp, int maxMap) =>
            {
                text_TextMp.text = $"{mp}/{maxMap}";
                if (img_SliderMp != null)
                {
                    img_SliderMp.rectTransform.DOKill();
                    img_SliderMp.rectTransform.DOSizeDelta(new Vector2(400f * mp / maxMap, 50), 0.8f);
                }
            });



            //注册Trigger事件
            var leftTrigger = btn_BtnLeft.GetComponent<EventTrigger>();
            var left_touch_begin_callback = new EventTrigger.TriggerEvent();
            left_touch_begin_callback.AddListener(OnTouchLeftButtonBegin);
            var left_touch_end_callback = new EventTrigger.TriggerEvent();
            left_touch_end_callback.AddListener(OnTouchLeftButtonEnd);

            leftTrigger.triggers.Add(new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerDown,
                callback = left_touch_begin_callback
            });
            leftTrigger.triggers.Add(new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerUp,
                callback = left_touch_end_callback
            });
            var rightTrigger = btn_BtnRight.GetComponent<EventTrigger>();
            var right_touch_begin_callback = new EventTrigger.TriggerEvent();
            right_touch_begin_callback.AddListener(OnTouchRightButtonBegin);
            var right_touch_end_callback = new EventTrigger.TriggerEvent();
            right_touch_end_callback.AddListener(OnTouchRightButtonEnd);

            rightTrigger.triggers.Add(new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerDown,
                callback = right_touch_begin_callback
            });
            rightTrigger.triggers.Add(new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerUp,
                callback = right_touch_end_callback
            });

            //蓝量消耗
            var cfg = GameGlobal.GameScheme.HeroTeamLeaderConfig(0);
            text_CostDodge.text = cfg.iMpCost_Dodge.ToString();
            text_FireCost.text = cfg.iMpCost_Fire.ToString();
            text_CostPurify.text = cfg.iMpCost_Purify.ToString();
        }


        private void OnTouchLeftButtonBegin(BaseEventData data)
        {
            OnJoystickInputStarted();
            OnJoystickInputChanged(Vector3.zero, Vector3.left);
        }

        private void OnTouchLeftButtonEnd(BaseEventData data)
        {
            OnJoystickInputEnded();
        }

        private void OnTouchRightButtonBegin(BaseEventData data)
        {
            OnJoystickInputStarted();
            OnJoystickInputChanged(Vector3.zero, Vector3.right);
        }

        private void OnTouchRightButtonEnd(BaseEventData data)
        {
            OnJoystickInputEnded();
        }

        private IEnumerator GameTimeUpdate()
        {

            while (true)
            {
                if (m_nGameTime == 20)
                {

                    var pContext = BossRageEventContext.Instance;
                    pContext.showHintWindow = true;
                    pContext.bossCDScale = 1f / (1f + 1f);
                    GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_BOSS_BERSERK_HINT, DEventSourceType.SOURCE_TYPE_ENTITY, 0, pContext);
                    StartLastSecondsEffect();
                }

                if (m_nGameTime == 60)
                {
                    var pContext = BossRageEventContext.Instance;
                    pContext.showHintWindow = false;
                    pContext.bossCDScale = 1f / (1 + 0.5f);
                    text_GameTime.color = Color.yellow;
                    GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_BOSS_BERSERK_HINT, DEventSourceType.SOURCE_TYPE_ENTITY, 0, pContext);
                }

                if (m_nGameTime >= 0)
                {
                    text_GameTime.text = m_nGameTime.ToString();
                    m_nGameTime--;
                }
                else
                {
                    break;
                }
                yield return new WaitForSeconds(1);
            }
            StopAllEffects();
            GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_FAIL, 0, 0, null);
        }

        private void StartLastSecondsEffect()
        {
            // 缩放跳动
            text_GameTime.transform.DOScale(1.2f, 0.2f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutQuad);

            // 颜色闪烁 + 透明闪烁（红白交替）
            m_FlashTween = DOTween.Sequence()
                .Append(text_GameTime.DOColor(Color.red, 0.3f))
                .Append(text_GameTime.DOFade(0.3f, 0.2f))
                .Append(text_GameTime.DOColor(Color.white, 0.3f))
                .Append(text_GameTime.DOFade(1f, 0.2f))
                .SetLoops(-1);
        }

        private void StopAllEffects()
        {
            text_GameTime.transform.DOKill();
            m_FlashTween?.Kill();

            Color defaultCountdownTextColor;
            ColorUtility.TryParseHtmlString("#00BFF3", out defaultCountdownTextColor);
            text_GameTime.color = defaultCountdownTextColor;
            text_GameTime.transform.localScale = Vector3.one;
        }

        private void SetMaskCD(Image maskImg, float cd)
        {
            maskImg.DOKill();
            maskImg.fillAmount = 1f;
            maskImg.DOFillAmount(0f, cd);
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
        private void OnBtn_BtnLeftClicked() //@Window 
        {

        }

        private void OnBtn_BtnRightClicked() //@Window 
        {

        }

        //散开
        private void OnBtn_BtnAvoidanceClicked() //@Window 
        {

            var skill = LeaderManager.Instance.GetLeaderSkillProperty(LeaderSkillType.Dodge);
            if (!skill.HasCD())
            {
                //没有CD
                GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_LEADER_SKILL_NOT_CD, 0, 0, null);
                return;
            }


            if (!skill.enoughMp)
            {
                //能量不足
                GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_LEADER_SKILL_NOT_MP, 0, 0, null);
                return;
            }

            skill.Invoke();
            SetMaskCD(img_MaskAvoidance, skill.CD);
            // GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_LEADER_SKILL_AVOIDANCE, DEventSourceType.SOURCE_TYPE_UI, 0, null);
        }

        //攻击
        private void OnBtn_BtnAttackClicked() //@Window 
        {
            var skill = LeaderManager.Instance.GetLeaderSkillProperty(LeaderSkillType.Fire);
            if (!skill.HasCD())
            {
                //没有CD
                GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_LEADER_SKILL_NOT_CD, 0, 0, null);
                return;
            }


            if (!skill.enoughMp)
            {
                //能量不足
                GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_LEADER_SKILL_NOT_MP, 0, 0, null);
                return;
            }

            skill.Invoke();
            SetMaskCD(img_MaskAttack, skill.CD);

            // GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_LEADER_SKILL_ATTACK, DEventSourceType.SOURCE_TYPE_UI, 0, null);
        }

        //治疗
        private void OnBtn_BtnTreatClicked() //@Window 
        {
            var skill = LeaderManager.Instance.GetLeaderSkillProperty(LeaderSkillType.Purify);
            if (!skill.HasCD())
            {
                //没有CD
                GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_LEADER_SKILL_NOT_CD, 0, 0, null);
                return;
            }


            if (!skill.enoughMp)
            {
                //能量不足
                GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_LEADER_SKILL_NOT_MP, 0, 0, null);
                return;
            }

            skill.Invoke();
            SetMaskCD(img_MaskTreat, skill.CD);
            // GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_LEADER_SKILL_TREAT, DEventSourceType.SOURCE_TYPE_UI, 0, null);
        }

        /// <summary>
        /// 折叠/展开属性面板按钮点击事件
        /// </summary>
        private void OnBtn_CollapseClicked() //@Window 
        {
            m_OpenCollapsed = !m_OpenCollapsed;

            float height = m_OpenCollapsed ? 1000 : 65f;
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
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_HARM_RED_SCREEN, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "UIHeroTeamGame:OnSubscribeEvents");
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_RESET_GAME, DEventSourceType.SOURCE_TYPE_UI, 0, "UIHeroTeamGame:OnSubscribeEvents");
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_BOSS_BERSERK_HINT, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "UIHeroTeamGame:OnSubscribeEvents");
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_FAIL, 0, 0, "UIHeroTeamGame:OnSubscribeEvents");

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
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_HARM_RED_SCREEN, DEventSourceType.SOURCE_TYPE_ENTITY, 0);
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_RESET_GAME, DEventSourceType.SOURCE_TYPE_UI, 0);
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_BOSS_BERSERK_HINT, DEventSourceType.SOURCE_TYPE_ENTITY, 0);
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_FAIL, 0, 0);

            //游戏摇杆事件移除
            if (null != m_Joystick)
            {
                var joystick = m_Joystick;
                joystick.OnInputChanged -= OnJoystickInputChanged;
                joystick.OnInputStarted -= OnJoystickInputStarted;
                joystick.OnInputEnded -= OnJoystickInputEnded;
            }
        }

        IEnumerator DelayCall(float delaySeconds, Action callback)
        {
            yield return new WaitForSeconds(delaySeconds);
            try
            {
                callback();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
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

                StopAllEffects();

                tran_TopPanel.gameObject.SetActive(false);

                // 5秒后显示胜利页面
                GameRoots.Instance.StartCoroutine(DelayCall(5f, () =>
                {
                    UIWindowManager.Instance.ShowWindow<UIWin>();
                }));
            }
            else if (wEventID == DHeroTeamEvent.EVENT_FAIL)
            {
                StopAllEffects();

                tran_TopPanel.gameObject.SetActive(false);

                // 5秒后显示胜利页面
                GameRoots.Instance.StartCoroutine(DelayCall(5f, () =>
                {
                    UIWindowManager.Instance.ShowWindow<UIFail>();
                }));
            }
            else if (DHeroTeamEvent.EVENT_START_GAME == wEventID)
            {
                // 开始游戏时折叠属性面板
                // OnBtn_CollapseClicked();
                tran_ParametersPanel.gameObject.SetActive(true);
            }
            else if (DHeroTeamEvent.EVENT_JOYSTICK_ACTIVE == wEventID)
            {
                //激活摇杆
                tran_JoystickParent.gameObject.SetActive(true);
                tran_LeaderSkillPanel.gameObject.SetActive(true);

                //激活游戏倒计时
                m_nGameTime = GameManager.Instance.GetCurrentLevelConfig().iGameTime;
                GameManager.Instance.OpenCoroutine(GameTimeUpdate());

                //技能CD 
                // TODO: 后续由技能管理器来处理
                // m_dicNextCanUseSkillTime["Attack"] = Time.time + 5;
                // m_dicNextCanUseSkillTime["Avoidance"] = Time.time + 5;
                // m_dicNextCanUseSkillTime["Treat"] = Time.time + 5;
                // SetButtonCD(btn_BtnAttack, 5);
                // SetButtonCD(btn_BtnAvoidance, 5);
                // SetButtonCD(btn_BtnTreat, 5);
            }
            else if (DHeroTeamEvent.EVENT_REFRESH_RANKDATA == wEventID && pContext is RefreshRankContext ctx)
            {
                // 刷新排行榜
                RefreshRank(ctx);
            }
            else if (DHeroTeamEvent.EVENT_HARM_RED_SCREEN == wEventID)
            {
                OnReciveHarm();
            }
            else if (DHeroTeamEvent.EVENT_RESET_GAME == wEventID)
            {
                OnResetGame();
            }
            else if (DHeroTeamEvent.EVENT_BOSS_BERSERK_HINT == wEventID)
            {
                if (BossRageEventContext.Instance.showHintWindow && !m_bShowBossBerserkWindows)
                {
                    m_bShowBossBerserkWindows = true;

                    var cg = tran_BossRageHintPanel.GetComponent<CanvasGroup>();
                    cg.gameObject.SetActive(true);

                    DOTween.Sequence().Append(cg.DOFade(0.95f, 0.4f).SetUpdate(false)).AppendInterval(2f).Append(cg.DOFade(0, 0.4f).SetUpdate(false)).AppendCallback(() =>
                    {
                        cg.gameObject.SetActive(false);
                        Time.timeScale = 1f;
                    }).SetUpdate(true).SetAutoKill(true);
                    Time.timeScale = 0.01f;
                }
            }
        }

        private void OnResetGame()
        {

            //重置倒计时
            StopAllEffects();
            m_nGameTime = GameManager.Instance.GetCurrentLevelConfig().iGameTime;
            text_GameTime.text = m_nGameTime.ToString();

            //重置排行榜
            var pContext = RefreshRankContext.Ins;
            pContext.arrCuringRank.Clear();
            pContext.arrHarmRank.Clear();
            pContext.arrHateRank.Clear();
            RefreshRank(pContext);

            tran_ParametersPanel.gameObject.SetActive(false);
            tran_JoystickParent.gameObject.SetActive(false);
            tran_LeaderSkillPanel.gameObject.SetActive(false);
            btn_BtnFight.gameObject.SetActive(true);

            var pContext2 = BossHpEventContext.Ins;
            pContext2.Health = 1f;
            GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_BOSS_HP_CHANGED, GameScripts.HeroTeam.DEventSourceType.SOURCE_TYPE_ENTITY, 0, pContext2);

            float height = 65f;
            img_PropertyPanel.rectTransform.DOKill();
            var size = img_PropertyPanel.rectTransform.sizeDelta;
            size.y = height;
            img_PropertyPanel.rectTransform.sizeDelta = size; //(size, 1.0f).SetEase(Ease.OutQuad);
        }

        private void OnReciveHarm()
        {

            if (img_RedScreen.SafeGetActiveSelf()) return;
            img_RedScreen.gameObject.BetterSetActive(true);
            var cg = img_RedScreen.GetComponent<CanvasGroup>();
            cg.DOKill();
            cg.alpha = 0f;
            cg.DOFade(0.43f, 0.5f).SetLoops(4, LoopType.Yoyo).OnComplete(() => img_RedScreen.gameObject.BetterSetActive(false)).SetAutoKill(true);
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
