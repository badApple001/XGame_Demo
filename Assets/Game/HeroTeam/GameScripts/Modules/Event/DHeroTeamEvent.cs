using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameScripts.HeroTeam
{


    public class DHeroTeamEvent
    {

        /// <summary> HeroTeam�¼� ��10001��ʼ�� 10000�����ǹ����¼� </summary>
        private readonly static ushort EVENT_HEROTEAM_BASE = 10001;

        /// <summary> �������ݷ����˱仯 </summary>
        public readonly static ushort EVENT_LOCALDATA_CHANGED_CREATE = EVENT_HEROTEAM_BASE++;

        /// <summary> ��ʼ��Ϸ </summary>
        public readonly static ushort EVENT_START_GAME = EVENT_HEROTEAM_BASE++;

        /// <summary> ����� </summary>
        public readonly static ushort EVENT_CAMERA_SHAKE = EVENT_HEROTEAM_BASE++;

        /// <summary>
        /// BossѪ�������˱仯
        /// </summary>
        public readonly static ushort EVENT_BOSS_HP_CHANGED = EVENT_HEROTEAM_BASE++;

        /// <summary>
        /// �Ի���������ս��״̬
        /// </summary>
        public readonly static ushort EVENT_INTO_FIGHT_STATE = EVENT_HEROTEAM_BASE++;

        /// <summary>
        /// 胜利
        /// </summary>
        public readonly static ushort EVENT_WIN = EVENT_HEROTEAM_BASE++;

        /// <summary>
        /// 刷新排行榜数据
        /// </summary>
        public readonly static ushort EVENT_REFRESH_RANKDATA = EVENT_HEROTEAM_BASE++;

        //���ֵ
        public readonly static ushort EVENT_ALL_MAXID = 30000;

    }


    //////////////////////////����Դ���� ///////////////////////////
    ///
    /// ����Դ����
    public class DEventSourceType
    {
        private readonly static byte SOURCE_TYPE_BASE = 0;
        public readonly static byte SOURCE_TYPE_UNKNOW = SOURCE_TYPE_BASE++;    // ����ID��
        public readonly static byte SOURCE_TYPE_LOCALDATA = SOURCE_TYPE_BASE++;    //���������¼�����
        public readonly static byte SOURCE_TYPE_ENTITY = SOURCE_TYPE_BASE++;//ʵ���¼�����
        public readonly static byte SOURCE_TYPE_UI = SOURCE_TYPE_BASE++;//UI�¼�����
        public readonly static byte SOURCE_TYPE_MONSTERSYSTEAM = SOURCE_TYPE_BASE++;
    };


    /// <summary>
    /// HeroTeam���ݱ仯�¼�
    /// </summary>
    public class HeroTeamDataChangedEventContext
    {
        private HeroTeamDataChangedEventContext() { }

        public static HeroTeamDataChangedEventContext Ins { private set; get; } = new HeroTeamDataChangedEventContext();

        ////////////////////////// �¼����ݵ����� ///////////////////////////
        public int nShareData;
    }


    /// <summary>
    /// Provides a context for configuring and managing camera shake events.
    /// </summary>
    /// <remarks>This class encapsulates parameters for camera shake effects, such as intensity, duration, and
    /// randomness. It is implemented as a singleton, accessible via the <see cref="Ins"/> property.</remarks>
    public class CameraShakeEventContext
    {
        private CameraShakeEventContext() { }
        public static CameraShakeEventContext Ins { private set; get; } = new CameraShakeEventContext();

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
        private BossHpEventContext() { }
        public static BossHpEventContext Ins { private set; get; } = new BossHpEventContext();

        public float Health = 1f;
    }

}
