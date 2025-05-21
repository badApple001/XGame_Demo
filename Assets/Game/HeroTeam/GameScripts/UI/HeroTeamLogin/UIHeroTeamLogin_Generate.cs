/************* <<< UICodeGenerator Version 1.0 >>>  ***********************
File：UIHeroTeamLogin.cs <FileHead_AutoGenerate>
Author：陈杰朝
Date：2025.05.20
Description：
***************************************************************************/

//@Begin_NameSpace
using UnityEngine;
using UnityEngine.UI;
using XGame.UI.Framework;
//@End_NameSpace

namespace GameScripts.HeroTeam.UI.HeroTeamLogin
{

    public partial class UIHeroTeamLogin : UIWindowEx
    {
		public static UIHeroTeamLogin Instance { get; private set; }
	    
		//@Begin_Widget_Variables
		private Button btn_Login = null;
		private InputField input_InputID = null;
		private InputField input_InputPassword = null;
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
            ResPath = "Game/HeroTeam/GameResources/Prefabs/UI/UIHeroTeamLogin.prefab";
        }
		
		protected override void InitWidgets() //@Window 
		{
			btn_Login = Meta.Widgets.GetWidgetComponent<Button>(0);
			input_InputID = Meta.Widgets.GetWidgetComponent<InputField>(1);
			input_InputPassword = Meta.Widgets.GetWidgetComponent<InputField>(2);
			OnInitWidgets();
		} //@End_InitWidgets
		
		protected override void ClearWidgets() //@Window 
		{
			OnClearWidgets();
			btn_Login = null;
			input_InputID = null;
			input_InputPassword = null;

		} //@End_ClearWidgets

		protected override void SubscribeEvents() //@Window
		{
			UnsubscribeEvents();
			btn_Login?.onClick.AddListener(OnBtn_LoginClicked);
			OnSubscribeEvents();
		} //@End_SubscribeEvents
		
		protected override void UnsubscribeEvents() //@Window
		{
			btn_Login?.onClick.RemoveAllListeners();
			OnUnsubscribeEvents();
		} //@End_UnsubscribeEvents
    }
	
	//@<<< FlexItemGenerator >>>
	//@<<< EffectiveListGenerator >>>
	
	
}
