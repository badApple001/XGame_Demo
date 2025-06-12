/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIFail.cs <FileHead_AutoGenerate>
Author：陈杰朝
Date：2025.06.12
Description：
***************************************************************************/

//@Begin_NameSpace
using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
//@End_NameSpace

namespace GameScripts.HeroTeam.UI.Fail
{

    public partial class UIFail : UIWindowEx
    {
		public static UIFail Instance { get; private set; }
	    
		//@Begin_Widget_Variables
		private Text text_Title = null;
		private Button btn_ReturnHome = null;
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
            ResPath = "Game/HeroTeam/GameResources/Prefabs/UI/UIFail.prefab";
        }
		
		protected override void InitWidgets() //@Window 
		{
			text_Title = Meta.Widgets.GetWidgetComponent<Text>(0);
			btn_ReturnHome = Meta.Widgets.GetWidgetComponent<Button>(1);
			OnInitWidgets();
		} //@End_InitWidgets
		
		protected override void ClearWidgets() //@Window 
		{
			OnClearWidgets();
			text_Title = null;
			btn_ReturnHome = null;

		} //@End_ClearWidgets

		protected override void SubscribeEvents() //@Window
		{
			UnsubscribeEvents();
			btn_ReturnHome?.onClick.AddListener(OnBtn_ReturnHomeClicked);
			OnSubscribeEvents();
		} //@End_SubscribeEvents
		
		protected override void UnsubscribeEvents() //@Window
		{
			btn_ReturnHome?.onClick.RemoveAllListeners();
			OnUnsubscribeEvents();
		} //@End_UnsubscribeEvents
    }
	
	//@<<< FlexItemGenerator >>>
	//@<<< EffectiveListGenerator >>>
	
	
}
