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
        public static readonly byte SOURCE_TYPE_MONSTER = 2;   //怪物事件类型
        public static readonly byte SOURCE_TYPE_PLAYER = 3;    //玩家事件类型
    };


    /// <summary>
    /// HeroTeam数据变化事件
    /// </summary>
    public class HeroTeamDataChangedEventContext
    {
        private HeroTeamDataChangedEventContext( ) { }
        
        public static HeroTeamDataChangedEventContext Ins { private set; get; } = new HeroTeamDataChangedEventContext( );
       
        public int EventKey = typeof( HeroTeamDataChangedEventContext ).GetHashCode();


        ////////////////////////// 事件传递的数据 ///////////////////////////
        public int iShareData;
    }




}
