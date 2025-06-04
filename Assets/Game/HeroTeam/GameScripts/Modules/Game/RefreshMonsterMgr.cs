/*******************************************************************
** �ļ���:	RefreshMonsterMgr.cs
** ��  Ȩ:	(C) ��������
** ������:	���¼�
** ��  ��:	2024.7.05
** ��  ��:	1.0
** ��  ��:	
** Ӧ  ��:  ����ˢ�¹�����

**************************** �޸ļ�¼ ******************************
** �޸���: 
** ��  ��: 
** ��  ��: 
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
        //�ҷ�·��·��
        public List<GameObject> listSelfRoute = new List<GameObject>( );

        //�з�·��
        public List<GameObject> listEnemyRoute = new List<GameObject>( );

        //ˢ����
        public MonsterLauncher monsterLauncherMonster;
        public MonsterLauncher monsterLauncherHero;

        //���˵�
        //public MonsterLauncher OtherLauncher;

        //����ļ����б�(��ʱ����)
        private List<int> skillIDs = new List<int>( );

        //��·�ɵ��
        private List<Vector3> m_listSelfRoutePoints = new List<Vector3>( );

        //�з�·�ɵ��
        private List<Vector3> m_listEnemyRoutePoints = new List<Vector3>( );

        //ˢ�ּ��
        private float m_lastCreateTime = 0.0f;

        //�ӳ�ˢ������
        cfg_RefreshLevelMonster m_DelayRefreshCfg;

        //��ˢ���б�
        private List<MonsterRefreshContext> m_listWaitCreateMonsters = new List<MonsterRefreshContext>( );


        // Start is called before the first frame update
        void Start( )
        {

            //����
            //KingAICreator.Instance.Setup( );

            //���������޸���
            //MonsterSystem.Instance.SetAttrModifier( KingAttrModifier.Instance );

            //m_playerIndex<<16| BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER

            //BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER << 32 | GameGlobal.RoleAgent.id >> 32;


            //�����Զ�ˢ����
            if ( null != monsterLauncherMonster )
            {
                monsterLauncherMonster.enabled = false;




                //�����˶����ܴ�����
                monsterLauncherMonster.listFriendCamps.Clear( );
                //monsterLauncher.listFriendCamps.Add( BATTLE_CAMP_DEF.BATTLE_CAMP_PIECE );

                monsterLauncherHero.enabled = false;
                monsterLauncherHero.listFriendCamps.Clear( );
            }


            //��ʼ��·��λ��
            __InitRoutePoints( m_listSelfRoutePoints, listSelfRoute );
            __InitRoutePoints( m_listEnemyRoutePoints, listEnemyRoute );

            //ˢһ�����Ե�
            // RefreshRoundMonster(1, 1, 1, 1);
        }

        // Update is called once per frame
        void Update( )
        {
            __DelayCreateMonster( );
        }

        /// <summary>
        /// ���ٹ���
        /// </summary>
        /// <param name="entMonster"></param>
        public void DestroyMonster( ulong entMonster )
        {
            //GameGlobalEx.EntityManager.DestroyEntity( entMonster );
        }

        //��ˢĳ������
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
                    //�ͷų�������
                    //__CastBornSkill( monster, cfg.bornskillIDs );

                    //�޸�����
                    __SyncAttribute( monster, 0, 0 );

                    return monster.id;
                }
            }

            return 0;
        }

        //��ˢĳ������
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

                    //�޸�����
                    __SyncAttribute(monster, 0, 0);

                    //�ͷų�������
                    __CastBornSkill(monster, cfg.bornskillIDs);
                    return monster.id;
                }
                */
            }
        }

        //ˢ�����б�
        public void RefreshMonster( List<int> listMonster, ulong camp, OnMonsterCreateCallback fnCallback = null )
        {
            int nCount = listMonster.Count;
            for ( int i = 0; i < nCount; ++i )
            {
                RefreshMonster( listMonster[ i ], camp );
            }
        }

        //�ؿ��غϱ��ˢ��
        public void RefreshRoundMonster( int chapter, int level, int round, int order )
        {
            cfg_RefreshLevelMonster cfg = GameGlobal.GameScheme.RefreshLevelMonster_0( chapter, level, round, order );
            if ( null == cfg )
            {
                Debug.LogError( "�����ڵĹؿ����� chapter=" + chapter + ",level=" + level + ",round=" + round + ",order=" + order );
                return;
            }

            // �ȱ����ڴ�ˢ���б�
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

        //ˢ��boss
        public ulong RefreshBoss( int chapter, int level, int round, int order )
        {
            // cfg_RefreshLevelMonster cfg = GameGlobal.GameScheme.RefreshLevelMonster_0( chapter, level, round, order );
            // if ( null == cfg )
            // {
            //     Debug.LogError( "�����ڵĹؿ����� chapter=" + chapter + ",level=" + level + ",round=" + round + ",order=" + order );
            //     return 0;
            // }

            // if ( cfg.iBossID <= 0 )
            // {
            //     Debug.LogError( "��ǰ����û������boss �ؿ����� chapter=" + chapter + ",level=" + level + ",round=" + round + ",order=" + order );
            //     return 0;
            // }

            // if ( cfg.aryBossBornPos.Length < 3 )
            // {
            //     Debug.LogError( "bossλ���������� �ؿ����� chapter=" + chapter + ",level=" + level + ",round=" + round + ",order=" + order );
            //     return 0;
            // }

            // //boss����Ҫ��Ѫ,���ȴ��Ĺ���һ��ˢ��
            // __ProcessDelayMonsterAll( );



            // //ˢ��
            // monsterLauncherMonster.listMonsterIDs.Clear( );
            // monsterLauncherMonster.listMonsterIDs.Add( cfg.iBossID );
            // monsterLauncherMonster.listRefreshParam.Clear( );
            // monsterLauncherMonster.listRefreshParam.Add( 1 );
            // monsterLauncherMonster.paramType = PARAM_TYPE.PARAM_TYPE_COUNT;
            // monsterLauncherMonster.limit_MonsterCount = 1000;
            // monsterLauncherMonster.camp = CampDef.GetLocalCamp( BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER );  //BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER;
            // monsterLauncherMonster.bRandomPos = false;
            // //monsterLauncherMonster.refreshPos = new Vector3( cfg.aryBossBornPos[ 0 ], cfg.aryBossBornPos[ 1 ], cfg.aryBossBornPos[ 2 ] );
            // monsterLauncherMonster.refreshPos = new Vector3( 1.01f, 13.38f );

            // //�����ѷ�
            // monsterLauncherMonster.listFriendCamps.Clear( );
            // //monsterLauncher.listFriendCamps.Add( BATTLE_CAMP_DEF.BATTLE_CAMP_PIECE );
            // //monsterLauncher.listFriendCamps.Add( BATTLE_CAMP_DEF.BATTLE_CAMP_WALL );

            // //��ȡ��ǰ��Ӫ С����Ѫ����ȫ��նɱ
            // int addHP = __SuckMonsterByCamp( cfg, monsterLauncherMonster.camp );

            // Debug.Log( $"Boss��ȡС��Ѫ����{addHP}" );

            // List<IMonster> listMonsters = monsterLauncherMonster.RefreshMonster( );
            // if ( listMonsters.Count > 0 )
            // {
            //     IMonster monster = listMonsters[ 0 ];

            //     //�Ƕ��Ǹ��� MonsterLauncher�ļ�����forward������
            //     //monster.SetRotation( Quaternion.Euler( 0, 180, 0 ) );
            //     //monster.transform.rotation = Quaternion.Euler( 0, 180, 0 );

            //     //�޸�����
            //     __SyncAttribute( monster, 0, addHP );

            //     cfg_Monster cfg_monster = monster.config as cfg_Monster;

            //     //�ͷų�������
            //     //__CastBornSkill( monster, cfg_monster.bornskillIDs );

            //     return monster.id;

            // }

            return 0;
        }


        public ulong RefreshHero( int monsterID, Vector3 pos, ulong camp, List<Vector3> road )
        {
            // if ( monsterLauncherHero != null )
            // {
            //     monsterLauncherHero.listMonsterIDs.Clear( );
            //     monsterLauncherHero.listMonsterIDs.Add( monsterID );
            //     monsterLauncherHero.listRefreshParam.Clear( );
            //     monsterLauncherHero.listRefreshParam.Add( 1 );
            //     monsterLauncherHero.paramType = PARAM_TYPE.PARAM_TYPE_COUNT;
            //     monsterLauncherHero.limit_MonsterCount = 1000;
            //     monsterLauncherHero.bRandomPos = false;
            //     monsterLauncherHero.refreshPos = pos;
            //     monsterLauncherHero.camp = CampDef.GetLocalCamp( camp );
            //     monsterLauncherHero.listFriendCamps.Clear( );
            //     List<IMonster> listMonster = monsterLauncherHero.RefreshMonster( );
            //     if ( listMonster.Count > 0 )
            //     {
            //         IMonster monster = listMonster[ 0 ];
            //         cfg_Monster cfg = monster.config as cfg_Monster;
            //         //�ͷų�������
            //         //__CastBornSkill( monster, cfg.bornskillIDs );

            //         monster.SetRoad( road );

            //         //�޸�����
            //         __SyncAttribute( monster, 0, 0 );

            //         return monster.id;
            //     }
            // }
            return 0;
        }



        //��ȡ�ҷ�·�ɵ�
        public List<Vector3> GetSelfRoutePoints( )
        {
            return m_listSelfRoutePoints;
        }

        //��ȡ�з�·�ɵ�
        public List<Vector3> GetEnemyRoutePoints( )
        {
            return m_listEnemyRoutePoints;
        }

        //���
        public void Clear( )
        {
            m_listWaitCreateMonsters.Clear( );
            m_lastCreateTime = 0;
            m_DelayRefreshCfg = null;
        }


        //��������
        private void __SyncAttribute( IMonster monster, int attributeID, int addHP )
        {

            //��ȡ����ֵ
            cfg_Monster cfgMonster = monster.config as cfg_Monster;
            //int iAttack = cfgMonster.iAttack;
            //int iAttackSpeed = cfgMonster.iAttackSpeed;
            //int baseHP = cfgMonster.baseHP + addHP;
            //int iPhyDefense = cfgMonster.iPhyDefense;
            //int iMagicDefense = cfgMonster.iMagicDefense;
            //float fbaseSpeed = cfgMonster.fbaseSpeed;
            //int iPowerAttackRate = cfgMonster.iPowerAttackRate;
            //int iPowerAttackCoff = cfgMonster.iPowerAttackCoff;

            ////��������
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
            //        Debug.LogError( "�����ڵ�����ID attributeID=" + attributeID );
            //    }
            //}


            //��������
            monster.SetIntAttr( CreatureAttributeDef.ATTACK, cfgMonster.iAttack );
            monster.SetIntAttr( CreatureAttributeDef.HP, cfgMonster.baseHP );

            //�����ƶ��ٶ�
            monster.SetSpeed( 20 );

            //����hp
            monster.SetHPDelta( cfgMonster.baseHP - monster.GetHP( ) );
            int maxHP = monster.GetHP( );
            monster.SetMaxHP( maxHP );

        }

        //���㼼����
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

        //���ˢȡ������
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
                    Debug.LogError( "����ļ����б��͸��ʸ�������Ӧ������" );
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

        //նɱͬ��ӪС��,��ȡѪ��
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
                            //����һ����Ѫ��Ч
                            pos = monster.GetPos( );
                            EffectMgr.Instance( ).PlayEffect( cfg.szSuckEffect, ref pos, 1.5f );
                        }

                        //���ٹ���
                        MonsterSystem.Instance.DestroyMonster( monster.id );
                    }
                }
            }
            return sumHP;
        }


        //�ͷų�������
        private void __CastBornSkill( IMonster monster, int[] skillIDs )
        {
            this.skillIDs.Clear( );
            this.skillIDs.AddRange( skillIDs );
            __CastBornSkill( monster, this.skillIDs );
        }

        //�ͷų�������
        private void __CastBornSkill( IMonster monster, List<int> skillIDs )
        {


            SkillPart skillPart = monster.GetPart<SkillPart>( );
            if ( skillPart != null )
            {
                skillPart.ClearPreConfig( true );

                int nSkillCount = skillIDs.Count;
                //ʹ�ü���
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
            //ˢ��
            monsterLauncherMonster.listMonsterIDs.Clear( );
            monsterLauncherMonster.listMonsterIDs.Add( ctx.monsterID );
            monsterLauncherMonster.listRefreshParam.Clear( );
            monsterLauncherMonster.listRefreshParam.Add( 1 );
            monsterLauncherMonster.paramType = PARAM_TYPE.PARAM_TYPE_COUNT;
            monsterLauncherMonster.limit_MonsterCount = 1000;
            //monsterLauncher.camp = CampDef.GetLocalCamp( BATTLE_CAMP_DEF.BATTLE_CAMP_MONSTER );
            monsterLauncherMonster.bRandomPos = true;

            //�����ѷ�
            monsterLauncherMonster.listFriendCamps.Clear( );
            //monsterLauncher.listFriendCamps.Add( BATTLE_CAMP_DEF.BATTLE_CAMP_PIECE );
            //monsterLauncher.listFriendCamps.Add( BATTLE_CAMP_DEF.BATTLE_CAMP_WALL );

            List<IMonster> listMonsters = monsterLauncherMonster.RefreshMonster( );

            //�������
            __CalclSkills( cfg );

            //�޸����Ժ�ʹ�ü���
            int nMonsterCount = listMonsters.Count;
            IMonster monster = null;
            // KingSkillComponent kingSkillComponent = null;
            int nSkillCount = skillIDs.Count;
            for ( int i = 0; i < nMonsterCount; ++i )
            {
                monster = listMonsters[ i ];

                //�޸�����
                __SyncAttribute( monster, cfg.iAttrModifidID, 0 );

                //�ͷų�������
                __CastBornSkill( monster, skillIDs );

                ctx.fnCallback?.Invoke( monster.id );
            }
        }
    }
}

