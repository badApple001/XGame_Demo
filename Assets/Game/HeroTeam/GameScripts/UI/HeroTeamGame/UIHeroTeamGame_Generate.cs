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
		private RectTransform tran_TopPanel = null;
		private Image img_UIHp_Boss_Foreground = null;
		private Text text_BossName = null;
		private Text text_UIHp_Boss_Text = null;
		private Button btn_BtnFight = null;
		private RectTransform tran_BottomPanel = null;
		private Image img_PropertyPanel = null;
		private Button btn_Collapse = null;
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
			tran_TopPanel = Meta.Widgets.GetWidgetComponent<RectTransform>(0);
			img_UIHp_Boss_Foreground = Meta.Widgets.GetWidgetComponent<Image>(1);
			text_BossName = Meta.Widgets.GetWidgetComponent<Text>(2);
			text_UIHp_Boss_Text = Meta.Widgets.GetWidgetComponent<Text>(3);
			btn_BtnFight = Meta.Widgets.GetWidgetComponent<Button>(4);
			tran_BottomPanel = Meta.Widgets.GetWidgetComponent<RectTransform>(5);
			img_PropertyPanel = Meta.Widgets.GetWidgetComponent<Image>(6);
			btn_Collapse = Meta.Widgets.GetWidgetComponent<Button>(7);
			OnInitWidgets();
		} //@End_InitWidgets
		
		protected override void ClearWidgets() //@Window 
		{
			OnClearWidgets();
			tran_TopPanel = null;
			img_UIHp_Boss_Foreground = null;
			text_BossName = null;
			text_UIHp_Boss_Text = null;
			btn_BtnFight = null;
			tran_BottomPanel = null;
			img_PropertyPanel = null;
			btn_Collapse = null;

		} //@End_ClearWidgets

		protected override void SubscribeEvents() //@Window
		{
			UnsubscribeEvents();
			btn_BtnFight?.onClick.AddListener(OnBtn_BtnFightClicked);
			btn_Collapse?.onClick.AddListener(OnBtn_CollapseClicked);
			OnSubscribeEvents();
		} //@End_SubscribeEvents
		
		protected override void UnsubscribeEvents() //@Window
		{
			btn_BtnFight?.onClick.RemoveAllListeners();
			btn_Collapse?.onClick.RemoveAllListeners();
			OnUnsubscribeEvents();
		} //@End_UnsubscribeEvents
    }
	
	//@<<< FlexItemGenerator >>>
	//@<<< EffectiveListGenerator >>>
	
	
}
