using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameScripts.HeroTeam
{


    public class DHeroTeamEvent
    {

        /// <summary> HeroTeam事件 从10001开始， 10000以内是公共事件 </summary>
        public readonly static ushort EVENT_HEROTEAM_BASE = 10001;

        /// <summary> 本地数据发生了变化 </summary>
        public readonly static ushort EVENT_LOCALDATA_CHANGED_CREATE = EVENT_HEROTEAM_BASE++;

        /// <summary> 开始游戏 </summary>
        public readonly static ushort EVENT_START_GAME = EVENT_HEROTEAM_BASE++;

        /// <summary> 相机震动 </summary>
        public readonly static ushort EVENT_CAMERA_SHAKE = EVENT_HEROTEAM_BASE++;
        
        /// <summary>
        /// Boss血量发生了变化
        /// </summary>
        public readonly static ushort EVENT_BOSS_HP_CHANGED = EVENT_HEROTEAM_BASE++;

        /// <summary>
        /// 对话结束进入战斗状态
        /// </summary>
        public readonly static ushort EVENT_INTO_FIGHT_CHANGED = EVENT_HEROTEAM_BASE++;


        //最大值
        public const ushort EVENT_ALL_MAXID = 30000;

    }


    //////////////////////////发送源类型 ///////////////////////////
    ///
    /// 发送源类型
    public class DEventSourceType
    {
        public static readonly byte SOURCE_TYPE_UNKNOW = 0;    // 类型ID根
        public static readonly byte SOURCE_TYPE_LOCALDATA = 1;    //本地数据事件类型
        public static readonly byte SOURCE_TYPE_ENTITY = 2;//实体事件类型
        public static readonly byte SOURCE_TYPE_UI = 2;//UI事件类型
    };


    /// <summary>
    /// HeroTeam数据变化事件
    /// </summary>
    public class HeroTeamDataChangedEventContext
    {
        private HeroTeamDataChangedEventContext( ) { }

        public static HeroTeamDataChangedEventContext Ins { private set; get; } = new HeroTeamDataChangedEventContext( );

        ////////////////////////// 事件传递的数据 ///////////////////////////
        public int nShareData;
    }

   
    /// <summary>
    /// Provides a context for configuring and managing camera shake events.
    /// </summary>
    /// <remarks>This class encapsulates parameters for camera shake effects, such as intensity, duration, and
    /// randomness. It is implemented as a singleton, accessible via the <see cref="Ins"/> property.</remarks>
    public class CameraShakeEventContext
    {
        private CameraShakeEventContext( ) { }
        public static CameraShakeEventContext Ins { private set; get; } = new CameraShakeEventContext( );

        public float intensity = 0.5f;
        public float duration = 0.5f;
        public int vibrato = 20;
        public float randomness = 90f;
        public bool fadeOut = true;
    }


    /// <summary>
    /// Provides a context for managing and accessing the health of a boss entity in the game.
    /// </summary>
    /// <remarks>This class is a singleton, and its instance can be accessed through the <see cref="Ins"/>
    /// property. The <see cref="Health"/> field represents the current health of the boss, where 1.0f typically
    /// indicates full health.</remarks>
    public class BossHpEventContext
    {
        private BossHpEventContext( ) { }
        public static BossHpEventContext Ins { private set; get; } = new BossHpEventContext( );

        public float Health = 1f;
    }

}
