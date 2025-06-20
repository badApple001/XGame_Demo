/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIHeroTeamGame.cs <FileHead_AutoGenerate>
Author：陈杰朝
Date：2025.05.20
Description：游戏内的UI
***************************************************************************/

//@Begin_NameSpace
using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
//@End_NameSpace

namespace GameScripts.HeroTeam.UI.HeroTeamGame
{

    public partial class UIHeroTeamGame : UIWindowEx
    {
		public static UIHeroTeamGame Instance { get; private set; }
	    
		//@Begin_Widget_Variables
		private Image img_RedScreen = null;
		private RectTransform tran_TopPanel = null;
		private Image img_UIHp_Boss_Foreground = null;
		private Text text_BossName = null;
		private Text text_UIHp_Boss_Text = null;
		private Text text_GameTime = null;
		private Button btn_BtnFight = null;
		private RectTransform tran_SelectTeam = null;
		private Button btn_Recruit = null;
		private InputField input_TeamName = null;
		private RectTransform tran_CareerRequirement = null;
		private RectTransform tran_JoystickParent = null;
		private Button btn_BtnLeft = null;
		private Button btn_BtnRight = null;
		private RectTransform tran_LeaderSkillPanel = null;
		private Button btn_BtnAttack = null;
		private Button btn_BtnAvoidance = null;
		private Button btn_BtnTreat = null;
		private Text text_FireCost = null;
		private Text text_CostDodge = null;
		private Text text_CostPurify = null;
		private RectTransform tran_MaskSkills = null;
		private Image img_MaskAttack = null;
		private Image img_MaskAvoidance = null;
		private Image img_MaskTreat = null;
		private Image img_SliderMp = null;
		private Text text_TextMp = null;
		private RectTransform tran_ParametersPanel = null;
		private Image img_PropertyPanel = null;
		private Button btn_Collapse = null;
		private RectTransform tran_PropertyContent = null;
		private RectTransform tran_BossRageHintPanel = null;
		//@End_Widget_Variables
		
		protected override void OnSetupOrClearWndInstance(bool isCreate)
		{
			if (isCreate)
				Instance = this;
			else
				Instance = null;
        }
		
        protected override void OnSetupParams()
        {
            ResPath = "Game/HeroTeam/GameResources/Prefabs/UI/UIHeroTeamGame.prefab";
        }
		
		protected override void InitWidgets() //@Window 
		{
			img_RedScreen = Meta.Widgets.GetWidgetComponent<Image>(0);
			tran_TopPanel = Meta.Widgets.GetWidgetComponent<RectTransform>(1);
			img_UIHp_Boss_Foreground = Meta.Widgets.GetWidgetComponent<Image>(2);
			text_BossName = Meta.Widgets.GetWidgetComponent<Text>(3);
			text_UIHp_Boss_Text = Meta.Widgets.GetWidgetComponent<Text>(4);
			text_GameTime = Meta.Widgets.GetWidgetComponent<Text>(5);
			btn_BtnFight = Meta.Widgets.GetWidgetComponent<Button>(6);
			tran_SelectTeam = Meta.Widgets.GetWidgetComponent<RectTransform>(7);
			btn_Recruit = Meta.Widgets.GetWidgetComponent<Button>(8);
			input_TeamName = Meta.Widgets.GetWidgetComponent<InputField>(9);
			tran_CareerRequirement = Meta.Widgets.GetWidgetComponent<RectTransform>(10);
			tran_JoystickParent = Meta.Widgets.GetWidgetComponent<RectTransform>(11);
			btn_BtnLeft = Meta.Widgets.GetWidgetComponent<Button>(12);
			btn_BtnRight = Meta.Widgets.GetWidgetComponent<Button>(13);
			tran_LeaderSkillPanel = Meta.Widgets.GetWidgetComponent<RectTransform>(14);
			btn_BtnAttack = Meta.Widgets.GetWidgetComponent<Button>(15);
			btn_BtnAvoidance = Meta.Widgets.GetWidgetComponent<Button>(16);
			btn_BtnTreat = Meta.Widgets.GetWidgetComponent<Button>(17);
			text_FireCost = Meta.Widgets.GetWidgetComponent<Text>(18);
			text_CostDodge = Meta.Widgets.GetWidgetComponent<Text>(19);
			text_CostPurify = Meta.Widgets.GetWidgetComponent<Text>(20);
			tran_MaskSkills = Meta.Widgets.GetWidgetComponent<RectTransform>(21);
			img_MaskAttack = Meta.Widgets.GetWidgetComponent<Image>(22);
			img_MaskAvoidance = Meta.Widgets.GetWidgetComponent<Image>(23);
			img_MaskTreat = Meta.Widgets.GetWidgetComponent<Image>(24);
			img_SliderMp = Meta.Widgets.GetWidgetComponent<Image>(25);
			text_TextMp = Meta.Widgets.GetWidgetComponent<Text>(26);
			tran_ParametersPanel = Meta.Widgets.GetWidgetComponent<RectTransform>(27);
			img_PropertyPanel = Meta.Widgets.GetWidgetComponent<Image>(28);
			btn_Collapse = Meta.Widgets.GetWidgetComponent<Button>(29);
			tran_PropertyContent = Meta.Widgets.GetWidgetComponent<RectTransform>(30);
			tran_BossRageHintPanel = Meta.Widgets.GetWidgetComponent<RectTransform>(31);
			OnInitWidgets();
		} //@End_InitWidgets
		
		protected override void ClearWidgets() //@Window 
		{
			OnClearWidgets();
			img_RedScreen = null;
			tran_TopPanel = null;
			img_UIHp_Boss_Foreground = null;
			text_BossName = null;
			text_UIHp_Boss_Text = null;
			text_GameTime = null;
			btn_BtnFight = null;
			tran_SelectTeam = null;
			btn_Recruit = null;
			input_TeamName = null;
			tran_CareerRequirement = null;
			tran_JoystickParent = null;
			btn_BtnLeft = null;
			btn_BtnRight = null;
			tran_LeaderSkillPanel = null;
			btn_BtnAttack = null;
			btn_BtnAvoidance = null;
			btn_BtnTreat = null;
			text_FireCost = null;
			text_CostDodge = null;
			text_CostPurify = null;
			tran_MaskSkills = null;
			img_MaskAttack = null;
			img_MaskAvoidance = null;
			img_MaskTreat = null;
			img_SliderMp = null;
			text_TextMp = null;
			tran_ParametersPanel = null;
			img_PropertyPanel = null;
			btn_Collapse = null;
			tran_PropertyContent = null;
			tran_BossRageHintPanel = null;

		} //@End_ClearWidgets

		protected override void SubscribeEvents() //@Window
		{
			UnsubscribeEvents();
			btn_BtnFight?.onClick.AddListener(OnBtn_BtnFightClicked);
			btn_Recruit?.onClick.AddListener(OnBtn_RecruitClicked);
			btn_BtnLeft?.onClick.AddListener(OnBtn_BtnLeftClicked);
			btn_BtnRight?.onClick.AddListener(OnBtn_BtnRightClicked);
			btn_BtnAttack?.onClick.AddListener(OnBtn_BtnAttackClicked);
			btn_BtnAvoidance?.onClick.AddListener(OnBtn_BtnAvoidanceClicked);
			btn_BtnTreat?.onClick.AddListener(OnBtn_BtnTreatClicked);
			btn_Collapse?.onClick.AddListener(OnBtn_CollapseClicked);
			OnSubscribeEvents();
		} //@End_SubscribeEvents
		
		protected override void UnsubscribeEvents() //@Window
		{
			btn_BtnFight?.onClick.RemoveAllListeners();
			btn_Recruit?.onClick.RemoveAllListeners();
			btn_BtnLeft?.onClick.RemoveAllListeners();
			btn_BtnRight?.onClick.RemoveAllListeners();
			btn_BtnAttack?.onClick.RemoveAllListeners();
			btn_BtnAvoidance?.onClick.RemoveAllListeners();
			btn_BtnTreat?.onClick.RemoveAllListeners();
			btn_Collapse?.onClick.RemoveAllListeners();
			OnUnsubscribeEvents();
		} //@End_UnsubscribeEvents
    }
	
	//@<<< FlexItemGenerator >>>
	//@<<< EffectiveListGenerator >>>
	
	
}
