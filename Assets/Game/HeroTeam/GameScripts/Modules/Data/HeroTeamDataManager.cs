using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XGame.Timer;
using XGameEngine.Player;

namespace GameScripts.HeroTeam
{

    /// <summary>
    /// 英雄小队数据管理器，负责管理本地数据的获取、保存、变更监听及定时检测。
    /// </summary>
    [DefaultExecutionOrder( 1000 )]
    public class HeroTeamDataManager : MonoBehaviour, ITimerHandler
    {
        //private IEventEngine m_EventEngine = null;

        private HeroTeamLocalData m_CacheData = null;

        /// <summary>
        /// 获取本地数据对象，懒加载并注册数据变更回调。
        /// </summary>
        public HeroTeamLocalData Data
        {
            get
            {
                if ( null == m_CacheData )
                {
                    m_CacheData = PlayerData.Instance.GetData<HeroTeamLocalData>( nameof( HeroTeamLocalData ), nameof( HeroTeamLocalData ) );
                    PlayerData.Instance.AddManagedData( m_CacheData );
                    m_CacheData.AddDataChangeCallback( OnDataChangeCallback );
                }
                return m_CacheData;
            }
        }

        /// <summary>
        /// 场景加载后自动创建管理器对象并常驻。
        /// </summary>
        [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.AfterSceneLoad )]
        private static void CreateMonoOnSceneLoaded( )
        {
            GameObject.DontDestroyOnLoad( new GameObject( nameof( HeroTeamDataManager ), typeof( HeroTeamDataManager ) ) );
        }

        /// <summary>
        /// 单例实例。
        /// </summary>
        public static HeroTeamDataManager Ins { get; private set; } = null;

        /// <summary>
        /// Mono Awake 生命周期回调，初始化单例。
        /// </summary>
        private void Awake( ) => Ins = this;

        //private void Start( ) => m_EventEngine = XGameComs.Get<IEventEngine>( );

        private List<System.Reflection.FieldInfo> m_DataKeys = new List<System.Reflection.FieldInfo>( );
        private List<object> m_DataValues = new List<object>( );

        /// <summary>
        /// Mono Start 生命周期回调，初始化字段缓存并注册定时器。
        /// </summary>
        private IEnumerator Start( )
        {
            yield return new WaitForSeconds( 1f );
            var type = Data.GetType( );
            var fileds = type.GetFields( );
            foreach ( var filed in fileds )
            {
                m_DataKeys.Add( filed );
                m_DataValues.Add( filed.GetValue( Data ) );
            }
            GameGlobal.Timer.AddTimer( this, typeof( HeroTeamDataManager ).GetHashCode( ), 1, nameof( HeroTeamDataManager ) );
            PlayerData.Instance.SetManagedDataSaveInterval( 1f );
        }

        /// <summary>
        /// 拷贝当前数据对象的所有字段值到缓存。
        /// </summary>
        private void CopyValues( )
        {
            for ( int i = 0; i < m_DataKeys.Count; i++ )
            {
                m_DataValues[ i ] = m_DataKeys[ i ].GetValue( Data );
            }
        }

        /// <summary>
        /// 定时器回调，检测数据是否发生变化，若有变化则标记数据脏并触发变更。
        /// </summary>
        /// <param name="ti">定时器信息</param>
        public void OnTimer( TimerInfo ti )
        {
            for ( int i = 0; i < m_DataKeys.Count; i++ )
            {
                if ( !Equals( m_DataKeys[ i ].GetValue( Data ), m_DataValues[ i ] ) )
                {
                    CopyValues( );
                    Data.MakeDataDirtyAndChange( );
                    break;
                }
            }
        }

        /// <summary>
        /// 数据变更回调，派发刷新UI事件。
        /// </summary>
        private void OnDataChangeCallback( )
        {
            Debug.Log( "HeroTeamDataManager.OnDataChangeCallback" );

            //派发 刷新UI事件
            //m_EventEngine?.FireExecute( DHeroTeamEvent.EVENT_LOCALDATA_CHANGED_CREATE, DEventSourceType.SOURCE_TYPE_LOCALDATA, 0, null );
            GameGlobal.EventEgnine.FireExecute( DHeroTeamEvent.EVENT_LOCALDATA_CHANGED_CREATE, DEventSourceType.SOURCE_TYPE_LOCALDATA, 0, null );
        }

        /// <summary>
        /// 强制保存所有托管数据。
        /// </summary>
        public void ForceSave( )
        {
            m_CacheData.MakeDataDirtyAndChange( );
            PlayerData.Instance.SaveAllManagedData( );
        }


        ///// <summary>
        ///// Mono OnDestroy 生命周期回调，移除定时器。
        ///// </summary>
        //private void OnDisable( )
        //{
        //    GameGlobal.Timer.RemoveTimer( this, typeof( HeroTeamDataManager ).GetHashCode( ) );
        //}
    }

}
