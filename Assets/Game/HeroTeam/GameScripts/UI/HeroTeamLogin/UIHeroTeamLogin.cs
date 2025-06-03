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
using System;
using XGameEngine.Player;
using XClient.Game;
using System.Collections.Generic;

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
            GameGlobalEx.LoginModule.Login();
            Debug.Log(FromUnixTimestamp(HeroTeamDataManager.Ins.Data.LastLoginTimestamps).ToString());
            HeroTeamDataManager.Ins.Data.LastLoginTimestamps = ToUnixTimestamp(DateTime.Now);

            // Debug.Log(HeroTeamDataManager.Ins.Data.testString);
            // var tmpArr = new List<int>()
            // {
            //     1,2,3
            // };
            // string json = JsonUtility.ToJson(tmpArr);
            // HeroTeamDataManager.Ins.Data.testString = json;
        }

        public static long ToUnixTimestamp(DateTime dateTime)
        {
            // 确保时间为 UTC
            DateTime utcTime = dateTime.ToUniversalTime();

            // Unix 时间戳起点
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // 计算总秒数
            return (long)(utcTime - epoch).TotalSeconds;
        }

        public static DateTime FromUnixTimestamp(long timestamp)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(timestamp);
        }

    }

    //@<<< EffectiveListGenerator >>>
    //@<<< FlexItemGenerator >>>


}
