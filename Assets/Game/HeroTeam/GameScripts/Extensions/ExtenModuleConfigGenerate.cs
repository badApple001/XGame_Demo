/************* <<< ModuleCodeGenerator Version 1.0 >>>  *************************
File: ModuleConfigGenerate.cs 
Author: 2025.05.20
Date: 2024.06.11
Description: 自动生成模块配置，请不要手动修改
***************************************************************************/

using XClient.Common;

namespace GameScripts.HeroTeam
{
    /// <summary>
    /// 模块ID声明
    /// </summary>
    public partial class DModuleIDEx
    {
		public static int GEN_CUR_EX = 24;     
        public static readonly int MODULE_ID_HEROTEAM =  ++GEN_CUR_EX;     //模块 - 英雄小队
		//@AppendDeclareModuleID
    }
	
	/// <summary>
    /// 全局对象扩展
    /// </summary>
    public class GameGlobalEx : GameGlobal
    {
        public static HeroTeam.IHeroTeamModule HeroTeam = null;
		//@AppendDeclareModuleInstanceVars
    }

    /// <summary>
    /// 模块实例变量
    /// </summary>
    public static class ExtenModulesSetupGenerate
    {
        public static void Setup(IModule[] modules)
        {
			GameGlobalEx.HeroTeam = Setup<HeroTeam.HeroTeamModule>(DModuleIDEx.MODULE_ID_HEROTEAM, modules);
			//@AppendSetupModuleInstanceVar
        }

        public static void Clear()
        {
			GameGlobalEx.HeroTeam = null;
			//@AppendClearModuleInstanceVar
        }

        private static T Setup<T>(int moduleID, IModule[] modules) where T : class, IModule, new()
        {
            T module = new T();
            modules[moduleID] = module;
            return module;
        }
    }
}

