/*******************************************************************
** 文件名:	RefreshMonsterMgr.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.7.05
** 版  本:	1.0
** 描  述:	
** 应  用:  怪物刷新管理器

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XClient.Entity;
using XGame.Entity;
using XGame;
using XGame.Utils;
using XClient.LightEffect;
using XGame.Poolable;

namespace GameScripts.Monster
{
    public delegate void OnMonsterCreateCallback( ulong entID );

    internal class MonsterRefreshContext : LitePoolableObject
    {
        public int monsterID;
        public OnMonsterCreateCallback fnCallback;

        protected override void OnRecycle( )
        {
            fnCallback = null;
            monsterID = 0;
        }
    }

    public class RefreshMonsterMgr : MonoBehaviourEX<RefreshMonsterMgr>
    {
        //我放路由路点
        public List<GameObject> listSelfRoute = new List<GameObject>( );

        //敌方路点
        public List<GameObject> listEnemyRoute = new List<GameObject>( );

        //刷怪器
        public MonsterLauncher monsterLauncherMonster;
        public MonsterLauncher monsterLauncherHero;

        //别人的
        //public MonsterLauncher OtherLauncher;

        //随机的技能列表(临时对象)
        private List<int> skillIDs = new List<int>( );

        //我路由点表
        private List<Vector3> m_listSelfRoutePoints = new List<Vector3>( );

        //敌方路由点表
        private List<Vector3> m_listEnemyRoutePoints = new List<Vector3>( );

        //刷怪间隔
        private float m_lastCreateTime = 0.0f;

        //延迟刷新配置
        cfg_RefreshLevelMonster m_DelayRefreshCfg;

        //待刷怪列表
        private List<MonsterRefreshContext> m_listWaitCreateMonsters = new List<MonsterRefreshContext>( );


        // Start is called before the first frame update
        void Start( )
        {

            //设置
            //KingAICreator.Instance.Setup( );

            //设置属性修改器
            //MonsterSystem.Instance.SetAttrModifier( KingAttrModifier.Instance );

            //m_playerIndex<<16| BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER

            //BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER << 32 | GameGlobal.RoleAgent.id >> 32;


            //屏蔽自动刷怪物
            if ( null != monsterLauncherMonster )
            {
                monsterLauncherMonster.enabled = false;




                //所有人都不能打棋子
                monsterLauncherMonster.listFriendCamps.Clear( );
                //monsterLauncher.listFriendCamps.Add( BATTLE_CAMP_DEF.BATTLE_CAMP_PIECE );

                monsterLauncherHero.enabled = false;
                monsterLauncherHero.listFriendCamps.Clear( );
            }


            //初始化路由位置
            __InitRoutePoints( m_listSelfRoutePoints, listSelfRoute );
            __InitRoutePoints( m_listEnemyRoutePoints, listEnemyRoute );

            //刷一波测试的
            // RefreshRoundMonster(1, 1, 1, 1);
        }

        // Update is called once per frame
        void Update( )
        {
            __DelayCreateMonster( );
        }

        /// <summary>
        /// 销毁怪物
        /// </summary>
        /// <param name="entMonster"></param>
        public void DestroyMonster( ulong entMonster )
        {
            //GameGlobalEx.EntityManager.DestroyEntity( entMonster );
        }

        //单刷某个怪物
        public ulong RefreshMonster( int monsterID, Vector3 pos, ulong camp )
        {
            if ( monsterLauncherMonster != null )
            {
                monsterLauncherMonster.listMonsterIDs.Clear( );
                monsterLauncherMonster.listMonsterIDs.Add( monsterID );
                monsterLauncherMonster.listRefreshParam.Clear( );
                monsterLauncherMonster.listRefreshParam.Add( 1 );
                monsterLauncherMonster.paramType = PARAM_TYPE.PARAM_TYPE_COUNT;
                monsterLauncherMonster.limit_MonsterCount = 1000;
                monsterLauncherMonster.bRandomPos = false;
                monsterLauncherMonster.refreshPos = pos;
                //monsterLauncher.camp = CampDef.GetLocalCamp( camp );
                monsterLauncherMonster.listFriendCamps.Clear( );
                List<IMonster> listMonster = monsterLauncherMonster.RefreshMonster( );
                if ( listMonster.Count > 0 )
                {
                    IMonster monster = listMonster[ 0 ];
                    cfg_Monster cfg = monster.config as cfg_Monster;
                    //释放出生技能
                    //__CastBornSkill( monster, cfg.bornskillIDs );

                    //修改属性
                    __SyncAttribute( monster, 0, 0 );

                    return monster.id;
                }
            }

            return 0;
        }

        //单刷某个怪物
        public void RefreshMonster( int monsterID, ulong camp, OnMonsterCreateCallback fnCallback = null )
        {
            if ( monsterLauncherMonster != null )
            {
                var context = LitePoolableObject.Instantiate<MonsterRefreshContext>( );
                context.monsterID = monsterID;
                context.fnCallback = fnCallback;
                m_listWaitCreateMonsters.Add( context );

                /*
                monsterLauncher.listMonsterIDs.Clear();
                monsterLauncher.listMonsterIDs.Add(monsterID);
                monsterLauncher.listRefreshParam.Clear();
                monsterLauncher.listRefreshParam.Add(1);
                monsterLauncher.paramType = PARAM_TYPE.PARAM_TYPE_COUNT;
                monsterLauncher.limit_MonsterCount = 1000;
                monsterLauncher.bRandomPos = true;
                monsterLauncher.camp = CampDef.GetLocalCamp(camp);
                monsterLauncher.listFriendCamps.Clear();
                List<IMonster> listMonster = monsterLauncher.RefreshMonster();
                if (listMonster.Count > 0)
                {
                    IMonster monster = listMonster[0];
                    cfg_Monster cfg = monster.config as cfg_Monster;

                    //修改属性
                    __SyncAttribute(monster, 0, 0);

                    //释放出生技能
                    __CastBornSkill(monster, cfg.bornskillIDs);
                    return monster.id;
                }
                */
            }
        }

        //刷怪物列表
        public void RefreshMonster( List<int> listMonster, ulong camp, OnMonsterCreateCallback fnCallback = null )
        {
            int nCount = listMonster.Count;
            for ( int i = 0; i < nCount; ++i )
            {
                RefreshMonster( listMonster[ i ], camp );
            }
        }

        //关卡回合变更刷怪
        public void RefreshRoundMonster( int chapter, int level, int round, int order )
        {
            cfg_RefreshLevelMonster cfg = GameGlobal.GameScheme.RefreshLevelMonster_0( chapter, level, round, order );
            if ( null == cfg )
            {
                Debug.LogError( "不存在的关卡配置 chapter=" + chapter + ",level=" + level + ",round=" + round + ",order=" + order );
                return;
            }

            // 先保存在待刷新列表
            int nLen = cfg.aryMonster.Length;
            for ( int i = 0; i < nLen; ++i )
            {
                int nMonsterID = cfg.aryMonster[ i ];
                int nCount = cfg.aryMonsterCount[ i ];
                for ( int j = 0; j < nCount; ++j )
                {
                    RefreshMonster( nMonsterID, 0, null );
                }
            }

            m_DelayRefreshCfg = cfg;

        }

        //刷新boss
        public ulong RefreshBoss( int chapter, int level, int round, int order )
        {
            cfg_RefreshLevelMonster cfg = GameGlobal.GameScheme.RefreshLevelMonster_0( chapter, level, round, order );
            if ( null == cfg )
            {
                Debug.LogError( "不存在的关卡配置 chapter=" + chapter + ",level=" + level + ",round=" + round + ",order=" + order );
                return 0;
            }

            if ( cfg.iBossID <= 0 )
            {
                Debug.LogError( "当前波次没有配置boss 关卡配置 chapter=" + chapter + ",level=" + level + ",round=" + round + ",order=" + order );
                return 0;
            }

            if ( cfg.aryBossBornPos.Length < 3 )
            {
                Debug.LogError( "boss位置配置有误 关卡配置 chapter=" + chapter + ",level=" + level + ",round=" + round + ",order=" + order );
                return 0;
            }

            //boss出生要吸血,将等待的怪物一体刷新
            __ProcessDelayMonsterAll( );



            //刷怪
            monsterLauncherMonster.listMonsterIDs.Clear( );
            monsterLauncherMonster.listMonsterIDs.Add( cfg.iBossID );
            monsterLauncherMonster.listRefreshParam.Clear( );
            monsterLauncherMonster.listRefreshParam.Add( 1 );
            monsterLauncherMonster.paramType = PARAM_TYPE.PARAM_TYPE_COUNT;
            monsterLauncherMonster.limit_MonsterCount = 1000;
            monsterLauncherMonster.camp = CampDef.GetLocalCamp( BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER );  //BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER;
            monsterLauncherMonster.bRandomPos = false;
            //monsterLauncherMonster.refreshPos = new Vector3( cfg.aryBossBornPos[ 0 ], cfg.aryBossBornPos[ 1 ], cfg.aryBossBornPos[ 2 ] );
            monsterLauncherMonster.refreshPos = new Vector3( 1.01f, 13.38f );

            //设置友方
            monsterLauncherMonster.listFriendCamps.Clear( );
            //monsterLauncher.listFriendCamps.Add( BATTLE_CAMP_DEF.BATTLE_CAMP_PIECE );
            //monsterLauncher.listFriendCamps.Add( BATTLE_CAMP_DEF.BATTLE_CAMP_WALL );

            //获取当前阵营 小兵的血量，全部斩杀
            int addHP = __SuckMonsterByCamp( cfg, monsterLauncherMonster.camp );

            Debug.Log( $"Boss吸取小怪血量：{addHP}" );

            List<IMonster> listMonsters = monsterLauncherMonster.RefreshMonster( );
            if ( listMonsters.Count > 0 )
            {
                IMonster monster = listMonsters[ 0 ];

                //角度是根据 MonsterLauncher的加载器forward来设置
                //monster.SetRotation( Quaternion.Euler( 0, 180, 0 ) );
                //monster.transform.rotation = Quaternion.Euler( 0, 180, 0 );

                //修改属性
                __SyncAttribute( monster, 0, addHP );

                cfg_Monster cfg_monster = monster.config as cfg_Monster;

                //释放出生技能
                //__CastBornSkill( monster, cfg_monster.bornskillIDs );

                return monster.id;

            }

            return 0;
        }


        public ulong RefreshHero( int monsterID, Vector3 pos, ulong camp, List<Vector3> road )
        {
            if ( monsterLauncherHero != null )
            {
                monsterLauncherHero.listMonsterIDs.Clear( );
                monsterLauncherHero.listMonsterIDs.Add( monsterID );
                monsterLauncherHero.listRefreshParam.Clear( );
                monsterLauncherHero.listRefreshParam.Add( 1 );
                monsterLauncherHero.paramType = PARAM_TYPE.PARAM_TYPE_COUNT;
                monsterLauncherHero.limit_MonsterCount = 1000;
                monsterLauncherHero.bRandomPos = false;
                monsterLauncherHero.refreshPos = pos;
                monsterLauncherHero.camp = CampDef.GetLocalCamp( camp );
                monsterLauncherHero.listFriendCamps.Clear( );
                List<IMonster> listMonster = monsterLauncherHero.RefreshMonster( );
                if ( listMonster.Count > 0 )
                {
                    IMonster monster = listMonster[ 0 ];
                    cfg_Monster cfg = monster.config as cfg_Monster;
                    //释放出生技能
                    //__CastBornSkill( monster, cfg.bornskillIDs );

                    monster.SetRoad( road );

                    //修改属性
                    __SyncAttribute( monster, 0, 0 );

                    return monster.id;
                }
            }
            return 0;
        }



        //获取我方路由点
        public List<Vector3> GetSelfRoutePoints( )
        {
            return m_listSelfRoutePoints;
        }

        //获取敌方路由点
        public List<Vector3> GetEnemyRoutePoints( )
        {
            return m_listEnemyRoutePoints;
        }

        //清除
        public void Clear( )
        {
            m_listWaitCreateMonsters.Clear( );
            m_lastCreateTime = 0;
            m_DelayRefreshCfg = null;
        }


        //设置属性
        private void __SyncAttribute( IMonster monster, int attributeID, int addHP )
        {

            //获取属性值
            cfg_Monster cfgMonster = monster.config as cfg_Monster;
            //int iAttack = cfgMonster.iAttack;
            //int iAttackSpeed = cfgMonster.iAttackSpeed;
            //int baseHP = cfgMonster.baseHP + addHP;
            //int iPhyDefense = cfgMonster.iPhyDefense;
            //int iMagicDefense = cfgMonster.iMagicDefense;
            //float fbaseSpeed = cfgMonster.fbaseSpeed;
            //int iPowerAttackRate = cfgMonster.iPowerAttackRate;
            //int iPowerAttackCoff = cfgMonster.iPowerAttackCoff;

            ////缩放属性
            //if ( attributeID > 0 )
            //{
            //    cfg_Attribute cfgAttr = GameGlobal.GameScheme.Attribute_0( attributeID );
            //    if ( null != cfgAttr )
            //    {
            //        iAttack = ( iAttack * cfgAttr.iAttackCoff ) / 1000;
            //        iAttackSpeed = iAttackSpeed * cfgAttr.iAttackSpeedCoff / 1000;
            //        baseHP = baseHP * cfgAttr.iHPCoff / 1000;
            //        iPhyDefense = iPhyDefense * cfgAttr.iPhyCoff / 1000;
            //        iMagicDefense = iMagicDefense * cfgAttr.iMagicCoff / 1000;
            //        fbaseSpeed = fbaseSpeed * cfgAttr.iSpeedCoff / 1000;

            //        iPowerAttackRate = iPowerAttackRate * cfgAttr.iPowerRateCoff / 1000;
            //        iPowerAttackCoff = iPowerAttackCoff * cfgAttr.iPowerAttackModifiedCoff / 1000;

            //    }
            //    else
            //    {
            //        Debug.LogError( "不存在的属性ID attributeID=" + attributeID );
            //    }
            //}


            //设置属性
            monster.SetIntAttr( CreatureAttributeDef.ATTACK, cfgMonster.iAttack );
            monster.SetIntAttr( CreatureAttributeDef.HP, cfgMonster.baseHP );

            //设置移动速度
            monster.SetSpeed( 20 );

            //设置hp
            monster.SetHPDelta( cfgMonster.baseHP - monster.GetHP( ) );
            int maxHP = monster.GetHP( );
            monster.SetMaxHP( maxHP );

        }

        //计算技能组
        private void __CalclSkills( cfg_RefreshLevelMonster cfg )
        {
            skillIDs.Clear( );
            int skillID = __RandmonOneSkill( cfg.arySkillID1, cfg.arySkillWeight1 );
            if ( skillID > 0 )
            {
                skillIDs.Add( skillID );
            }

            skillID = __RandmonOneSkill( cfg.arySkillID2, cfg.arySkillWeight2 );
            if ( skillID > 0 )
            {
                skillIDs.Add( skillID );
            }

            skillID = __RandmonOneSkill( cfg.arySkillID3, cfg.arySkillWeight3 );
            if ( skillID > 0 )
            {
                skillIDs.Add( skillID );
            }
        }

        //随机刷取个技能
        private int __RandmonOneSkill( int[] skills, int[] weights )
        {
            int nCount = skills.Length;
            if ( nCount == 0 )
            {
                return 0;

            }

            int sumProp = __SumWeight( weights );
            int curSum = 0;
            int prop = Random.Range( 0, sumProp );
            for ( int i = 0; i < nCount; ++i )
            {
                curSum += weights[ i ];
                if ( curSum > prop )
                {
                    if ( skills.Length > i )
                    {
                        return skills[ i ];
                    }
                    Debug.LogError( "随机的技能列表和概率个数不对应，请检查" );
                    break;
                }
            }

            return 0;
        }

        public int __SumWeight( int[] weights )
        {
            int sum = 0;
            int len = weights.Length;
            for ( int i = 0; i < len; ++i )
            {
                sum += weights[ i ];
            }

            return sum;
        }

        //斩杀同阵营小兵,获取血量
        private List<ulong> listCamp = new List<ulong>( );
        private List<IEntity> listEntity = new List<IEntity>( );
        private int __SuckMonsterByCamp( cfg_RefreshLevelMonster cfg, ulong camp )
        {
            listCamp.Clear( );
            listCamp.Add( camp );
            Vector3 pos = this.transform.position;
            //Vector3 forward = this.transform.forward;
            // List<IDReco> listReco = IDRecoEntityMgr.Instance.GetIDRecoByCamp(listCamp, EntityType.monsterType, ref pos, ref forward, REGION_TYPE.REGION_SHAPE_CIRCLE, 100000, 0);
            //int nCount = listReco.Count;
            IEntityManager manager = GameGlobal.EntityWorld.Default;
            listEntity.Clear( );
            manager.GetEntityByType( EntityType.monsterType, listEntity );
            int nCount = listEntity.Count;


            IMonster monster = null;
            IEntity entity = null;
            int sumHP = 0;
            for ( int i = 0; i < nCount; ++i )
            {
                entity = listEntity[ i ];// manager.GetEntity(listReco[i].entID);
                if ( entity != null )
                {
                    monster = entity as IMonster;
                    if ( null != monster && monster.GetCamp( ) == camp )
                    {
                        sumHP += monster.GetHP( );

                        if ( string.IsNullOrEmpty( cfg.szSuckEffect ) == false )
                        {
                            //播放一个吸血特效
                            pos = monster.GetPos( );
                            EffectMgr.Instance( ).PlayEffect( cfg.szSuckEffect, ref pos, 1.5f );
                        }

                        //销毁怪物
                        MonsterSystem.Instance.DestroyMonster( monster.id );
                    }
                }
            }
            return sumHP;
        }


        //释放出生技能
        private void __CastBornSkill( IMonster monster, int[] skillIDs )
        {
            this.skillIDs.Clear( );
            this.skillIDs.AddRange( skillIDs );
            __CastBornSkill( monster, this.skillIDs );
        }

        //释放出生技能
        private void __CastBornSkill( IMonster monster, List<int> skillIDs )
        {


            SkillPart skillPart = monster.GetPart<SkillPart>( );
            if ( skillPart != null )
            {
                skillPart.ClearPreConfig( true );

                int nSkillCount = skillIDs.Count;
                //使用技能
                for ( int j = 0; j < nSkillCount; ++j )
                {
                    skillPart.DoAttack( skillIDs[ j ] );
                }
            }

        }

        private void __InitRoutePoints( List<Vector3> listPoints, List<GameObject> listRouteObjects )
        {
            listPoints.Clear( );
            int count = listRouteObjects.Count;
            for ( int i = 0; i < count; ++i )
            {
                if ( null != listRouteObjects[ i ] )
                {
                    listPoints.Add( listRouteObjects[ i ].transform.position );
                }

            }
        }


        private void __DelayCreateMonster( )
        {
            int nCount = m_listWaitCreateMonsters.Count;
            if ( nCount == 0 )
            {
                return;
            }

            float curTime = Time.realtimeSinceStartup;
            if ( curTime - m_lastCreateTime < m_DelayRefreshCfg.fCreateInterval )
            {
                return;
            }


            m_lastCreateTime = curTime;
            MonsterRefreshContext ctx = m_listWaitCreateMonsters[ nCount - 1 ];
            m_listWaitCreateMonsters.RemoveAt( nCount - 1 );
            __CreateMonster( ctx, m_DelayRefreshCfg );
            ctx.Release( );
        }


        private void __ProcessDelayMonsterAll( )
        {
            int nCount = m_listWaitCreateMonsters.Count;
            for ( int i = 0; i < nCount; ++i )
            {
                MonsterRefreshContext ctx = m_listWaitCreateMonsters[ i ];
                __CreateMonster( ctx, m_DelayRefreshCfg );
                ctx.Release( );
            }

            m_listWaitCreateMonsters.Clear( );
        }


        private void __CreateMonster( MonsterRefreshContext ctx, cfg_RefreshLevelMonster cfg )
        {
            //刷怪
            monsterLauncherMonster.listMonsterIDs.Clear( );
            monsterLauncherMonster.listMonsterIDs.Add( ctx.monsterID );
            monsterLauncherMonster.listRefreshParam.Clear( );
            monsterLauncherMonster.listRefreshParam.Add( 1 );
            monsterLauncherMonster.paramType = PARAM_TYPE.PARAM_TYPE_COUNT;
            monsterLauncherMonster.limit_MonsterCount = 1000;
            //monsterLauncher.camp = CampDef.GetLocalCamp( BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER );
            monsterLauncherMonster.bRandomPos = true;

            //设置友方
            monsterLauncherMonster.listFriendCamps.Clear( );
            //monsterLauncher.listFriendCamps.Add( BATTLE_CAMP_DEF.BATTLE_CAMP_PIECE );
            //monsterLauncher.listFriendCamps.Add( BATTLE_CAMP_DEF.BATTLE_CAMP_WALL );

            List<IMonster> listMonsters = monsterLauncherMonster.RefreshMonster( );

            //随机技能
            __CalclSkills( cfg );

            //修改属性和使用技能
            int nMonsterCount = listMonsters.Count;
            IMonster monster = null;
            // KingSkillComponent kingSkillComponent = null;
            int nSkillCount = skillIDs.Count;
            for ( int i = 0; i < nMonsterCount; ++i )
            {
                monster = listMonsters[ i ];

                //修改属性
                __SyncAttribute( monster, cfg.iAttrModifidID, 0 );

                //释放出生技能
                __CastBornSkill( monster, skillIDs );

                ctx.fnCallback?.Invoke( monster.id );
            }
        }
    }
}

