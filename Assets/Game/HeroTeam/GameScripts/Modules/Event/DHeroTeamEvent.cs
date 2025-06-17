using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameScripts.HeroTeam
{

    /// <summary>
    /// 英雄小队事件定义类
    /// </summary>
    public class DHeroTeamEvent
    {

        /// <summary> HeroTeam事件基准值，从10001开始，10000以下为通用事件 </summary>
        private readonly static ushort EVENT_HEROTEAM_BASE = 10001;

        /// <summary> 本地数据发生变化 </summary>
        public readonly static ushort EVENT_LOCALDATA_CHANGED_CREATE = EVENT_HEROTEAM_BASE++;

        /// <summary> 开始游戏 </summary>
        public readonly static ushort EVENT_START_GAME = EVENT_HEROTEAM_BASE++;

        /// <summary> 相机震动 </summary>
        public readonly static ushort EVENT_CAMERA_SHAKE = EVENT_HEROTEAM_BASE++;

        /// <summary>
        /// Boss血量发生变化
        /// </summary>
        public readonly static ushort EVENT_BOSS_HP_CHANGED = EVENT_HEROTEAM_BASE++;

        /// <summary>
        /// 进入战斗状态
        /// </summary>
        public readonly static ushort EVENT_INTO_FIGHT_STATE = EVENT_HEROTEAM_BASE++;

        /// <summary>
        /// 获胜
        /// </summary>
        public readonly static ushort EVENT_WIN = EVENT_HEROTEAM_BASE++;

        /// <summary>
        /// 刷新排行榜数据
        /// </summary>
        public readonly static ushort EVENT_REFRESH_RANKDATA = EVENT_HEROTEAM_BASE++;

        /// <summary>
        /// 玩家操作摇杆事件: 开始
        /// </summary>
        public readonly static ushort EVENT_JOYSTICK_STARTED = EVENT_HEROTEAM_BASE++;

        /// <summary>
        /// 玩家操作摇杆事件: 拖动
        /// </summary>
        public readonly static ushort EVENT_JOYSTICK_CHANGED = EVENT_HEROTEAM_BASE++;

        /// <summary>
        /// 玩家操作摇杆事件: 结束
        /// </summary>
        public readonly static ushort EVENT_JOYSTICK_ENDED = EVENT_HEROTEAM_BASE++;


        /// <summary>
        /// 激活摇杆
        /// </summary>
        public readonly static ushort EVENT_JOYSTICK_ACTIVE = EVENT_HEROTEAM_BASE++;

        /// <summary>
        /// 团长技能 - 攻击
        /// </summary>
        public readonly static ushort EVENT_LEADER_SKILL_ATTACK = EVENT_HEROTEAM_BASE++;

        /// <summary>
        /// 团长技能 - 散开
        /// </summary>
        public readonly static ushort EVENT_LEADER_SKILL_AVOIDANCE = EVENT_HEROTEAM_BASE++;

        /// <summary>
        /// 团长技能 - 治疗
        /// </summary>
        public readonly static ushort EVENT_LEADER_SKILL_TREAT = EVENT_HEROTEAM_BASE++;

        /// <summary>
        /// 伤害红屏效果
        /// </summary>
        public readonly static ushort EVENT_HARM_RED_SCREEN = EVENT_HEROTEAM_BASE++;

        /// <summary>
        /// 重置游戏
        /// </summary>
        public readonly static ushort EVENT_RESET_GAME = EVENT_HEROTEAM_BASE++;
        /// <summary>
        /// Boss狂暴提示
        /// </summary>
        public readonly static ushort EVENT_BOSS_BERSERK_HINT = EVENT_HEROTEAM_BASE++;

        /// <summary>
        /// 失败
        /// </summary>
        public readonly static ushort EVENT_FAIL = EVENT_HEROTEAM_BASE++;

        /// <summary>
        /// 点击团长技能时
        /// 团长技能还没有冷却
        /// </summary>
        public readonly static ushort EVENT_LEADER_SKILL_NOT_CD = EVENT_HEROTEAM_BASE++;


        /// <summary>
        /// 点击团长技能时
        /// 能力不够
        /// </summary>
        public readonly static ushort EVENT_LEADER_SKILL_NOT_MP = EVENT_HEROTEAM_BASE++;


        /// <summary>
        /// 所有事件的最大ID值
        /// </summary>
        public readonly static ushort EVENT_ALL_MAXID = 30000;

    }


    //////////////////////////事件源类型定义 ///////////////////////////
    ///
    /// <summary>
    /// 事件源类型定义
    /// </summary>
    public class DEventSourceType
    {
        private readonly static byte SOURCE_TYPE_BASE = 0;
        /// <summary> 未知ID </summary>
        public readonly static byte SOURCE_TYPE_UNKNOW = SOURCE_TYPE_BASE++;
        /// <summary> 本地数据事件源 </summary>
        public readonly static byte SOURCE_TYPE_LOCALDATA = SOURCE_TYPE_BASE++;
        /// <summary> 实体事件源 </summary>
        public readonly static byte SOURCE_TYPE_ENTITY = SOURCE_TYPE_BASE++;
        /// <summary> UI事件源 </summary>
        public readonly static byte SOURCE_TYPE_UI = SOURCE_TYPE_BASE++;
        /// <summary> 怪物系统事件源 </summary>
        public readonly static byte SOURCE_TYPE_MONSTERSYSTEAM = SOURCE_TYPE_BASE++;
    };


    /// <summary>
    /// HeroTeam数据变化事件上下文
    /// </summary>
    public class HeroTeamDataChangedEventContext
    {
        private HeroTeamDataChangedEventContext() { }

        /// <summary>
        /// 单例实例
        /// </summary>
        public static HeroTeamDataChangedEventContext Ins { private set; get; } = new HeroTeamDataChangedEventContext();

        ////////////////////////// 事件数据成员 ///////////////////////////
        /// <summary>
        /// 共享数据
        /// </summary>
        public int nShareData;
    }


    /// <summary>
    /// 相机震动事件上下文，包含震动参数配置
    /// </summary>
    /// <remarks>
    /// 封装相机震动效果的参数，如强度、持续时间、随机性等。通过单例Ins访问。
    /// </remarks>
    public class CameraShakeEventContext
    {
        private CameraShakeEventContext() { }
        /// <summary>
        /// 单例实例
        /// </summary>
        public static CameraShakeEventContext Ins { private set; get; } = new CameraShakeEventContext();

        /// <summary>
        /// 震动强度
        /// </summary>
        public float intensity = 0.5f;
        /// <summary>
        /// 震动持续时间
        /// </summary>
        public float duration = 0.5f;
        /// <summary>
        /// 震动频率
        /// </summary>
        public int vibrato = 20;
        /// <summary>
        /// 震动随机性
        /// </summary>
        public float randomness = 90f;
        /// <summary>
        /// 是否淡出
        /// </summary>
        public bool fadeOut = true;
    }


    /// <summary>
    /// Boss血量变化事件上下文，管理Boss当前血量
    /// </summary>
    /// <remarks>
    /// 单例模式，通过Ins访问。Health字段表示Boss当前血量，1.0f为满血。
    /// </remarks>
    public class BossHpEventContext
    {
        private BossHpEventContext() { }
        /// <summary>
        /// 单例实例
        /// </summary>
        public static BossHpEventContext Ins { private set; get; } = new BossHpEventContext();

        /// <summary>
        /// Boss当前血量（1.0f为满血）
        /// </summary>
        public float Health = 1f;
    }

    /// <summary>
    /// 摇杆事件上下文，记录摇杆操作的偏移量
    /// </summary>
    public class JoystickEventContext
    {
        private JoystickEventContext() { }
        /// <summary>
        /// 单例实例
        /// </summary>
        public static JoystickEventContext Ins { private set; get; } = new JoystickEventContext();
        /// <summary>
        /// 摇杆偏移量
        /// </summary>
        public Vector3 delta;
    }
}
