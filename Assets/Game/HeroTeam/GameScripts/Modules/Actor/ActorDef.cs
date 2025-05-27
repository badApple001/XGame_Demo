using UnityEngine;
using XGame.EventEngine;


namespace GameScripts.HeroTeam
{
    /// <summary>
    /// 
    /// 轻量化战斗单位
    /// 
    /// 由配置表+Entity+Fsm管理
    /// 
    /// 配置表提供 每个角色不同的技能，不同的CD，生命，攻击，等属性
    /// Enity管理 每个角色的生物周期属性，负责创建和销毁
    /// Fsm辅助处理 角色不同状态下的行为,包括动画
    /// </summary>
    public partial class Actor : MonoBehaviour, IActor, IEventExecuteSink
    {

        
        /// <summary>
        /// 已经冷却了的技能
        /// </summary>
        public const string Machine_blackBoard_CooldownSkill = "CooldownSkill";


    }

}