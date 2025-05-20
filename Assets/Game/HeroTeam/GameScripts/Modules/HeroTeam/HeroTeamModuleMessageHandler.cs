/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File：HeroTeam.cs 
Author：陈杰朝
Date：2025.05.20
Description：#Desc#
***************************************************************************/

using minigame;
using gamepol;
using XClient.Common;
using XClient.Net;
using XClient.RPC;
using XGame.LitJson;

namespace GameScripts.HeroTeam.HeroTeam
{
    public class HeroTeamModuleMessageHandler :  ModuleMessageHandler<HeroTeamModule>
    {
        protected override void OnSetupHandlers()
        {
			var desc = GetType().Name;
			//@SetupMessageHandlerGenerator
        }
		
        //@ReceiveMessageHandlerGenerator

        //@SendMessageHandlerGenerator
    }
}
