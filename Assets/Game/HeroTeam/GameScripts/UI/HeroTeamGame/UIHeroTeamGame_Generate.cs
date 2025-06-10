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
		private Button btn_BtnFight = null;
		private RectTransform tran_JoystickParent = null;
		private RectTransform tran_LeaderSkillPanel = null;
		private Button btn_BtnAvoidance = null;
		private Button btn_BtnAttack = null;
		private Button btn_BtnTreat = null;
		private RectTransform tran_ParametersPanel = null;
		private Image img_PropertyPanel = null;
		private Button btn_Collapse = null;
		private RectTransform tran_PropertyContent = null;
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
			btn_BtnFight = Meta.Widgets.GetWidgetComponent<Button>(5);
			tran_JoystickParent = Meta.Widgets.GetWidgetComponent<RectTransform>(6);
			tran_LeaderSkillPanel = Meta.Widgets.GetWidgetComponent<RectTransform>(7);
			btn_BtnAvoidance = Meta.Widgets.GetWidgetComponent<Button>(8);
			btn_BtnAttack = Meta.Widgets.GetWidgetComponent<Button>(9);
			btn_BtnTreat = Meta.Widgets.GetWidgetComponent<Button>(10);
			tran_ParametersPanel = Meta.Widgets.GetWidgetComponent<RectTransform>(11);
			img_PropertyPanel = Meta.Widgets.GetWidgetComponent<Image>(12);
			btn_Collapse = Meta.Widgets.GetWidgetComponent<Button>(13);
			tran_PropertyContent = Meta.Widgets.GetWidgetComponent<RectTransform>(14);
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
			btn_BtnFight = null;
			tran_JoystickParent = null;
			tran_LeaderSkillPanel = null;
			btn_BtnAvoidance = null;
			btn_BtnAttack = null;
			btn_BtnTreat = null;
			tran_ParametersPanel = null;
			img_PropertyPanel = null;
			btn_Collapse = null;
			tran_PropertyContent = null;

		} //@End_ClearWidgets

		protected override void SubscribeEvents() //@Window
		{
			UnsubscribeEvents();
			btn_BtnFight?.onClick.AddListener(OnBtn_BtnFightClicked);
			btn_BtnAvoidance?.onClick.AddListener(OnBtn_BtnAvoidanceClicked);
			btn_BtnAttack?.onClick.AddListener(OnBtn_BtnAttackClicked);
			btn_BtnTreat?.onClick.AddListener(OnBtn_BtnTreatClicked);
			btn_Collapse?.onClick.AddListener(OnBtn_CollapseClicked);
			OnSubscribeEvents();
		} //@End_SubscribeEvents
		
		protected override void UnsubscribeEvents() //@Window
		{
			btn_BtnFight?.onClick.RemoveAllListeners();
			btn_BtnAvoidance?.onClick.RemoveAllListeners();
			btn_BtnAttack?.onClick.RemoveAllListeners();
			btn_BtnTreat?.onClick.RemoveAllListeners();
			btn_Collapse?.onClick.RemoveAllListeners();
			OnUnsubscribeEvents();
		} //@End_UnsubscribeEvents
    }
	
	//@<<< FlexItemGenerator >>>
	//@<<< EffectiveListGenerator >>>
	
	
}
