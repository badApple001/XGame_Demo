/*******************************************************************
** �ļ���:	MonsterSystem.cs
** ��  Ȩ:	(C) ��������
** ������:	���¼�
** ��  ��:	2024.6.25
** ��  ��:	1.0
** ��  ��:	
** Ӧ  ��:  ����ϵͳ(�������Ĵ��������ٺ��߼��ƶ�)

**************************** �޸ļ�¼ ******************************
** �޸���: 
** ��  ��: 
** ��  ��: 
********************************************************************/

using GameScripts;
using GameScripts.HeroTeam;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XClient.Common;
using XClient.Entity;
using XGame;
using XGame.Entity;
using XGame.Entity.Part;
using XGame.EventEngine;
using XGame.Poolable;
using XGame.Utils;


namespace XClient.Entity
{
    //���ﴴ���ֳ�
    public class CreateMonsterContext
    {
        public int creatureID;
        public int displayID;
        public Vector3 pos;
        public bool isLocalPos;
        public bool faceLeft;
        public Vector3 forward;
        public ulong camp;
        public int baseHP = 1000;
        public int maxHP = 1000;

        //��ʾ��ʽ 
        public CREATURE_VISIBLE_TYPE visibleType = CREATURE_VISIBLE_TYPE.VISIBLE_TYPE_LOCAL;

        //�ѷ���Ӫ
        public List<ulong> listFriendCamps;

        //�з���Ӫ
        public List<ulong> listEnemyCamps;

        //�������
        public Dictionary<int, int> dicSkinData;

        //��Դ·��
        public string resPath;

        public bool isPlayer;

        public void Reset()
        {
            displayID = 0;
            pos = Vector3.zero;
            isLocalPos = false;
            faceLeft = false;
            forward = Vector3.zero;
            camp = (ulong)0;
            baseHP = 1000;
            maxHP = 1000;
            visibleType = CREATURE_VISIBLE_TYPE.VISIBLE_TYPE_LOCAL;

            listFriendCamps = null;
            listEnemyCamps = null;

            dicSkinData = null;
            resPath = "";
            isPlayer = true;
        }
    }

    //���������
    public partial class EntityType
    {
        public readonly static int monsterType = 300; //����ʵ��
    }

    //��������
    public partial class EntityPartType
    {
        public readonly static int entityMovePartType = 300; //�ƶ�����
        public readonly static int entityAIPartType = 301; //AI����
        public readonly static int entityDamagePartType = 302; //�˺�����
        public readonly static int entitySkillPartType = 303; //���ܲ���
        public readonly static int entityMaterialSwitchType = 304; //�����л�����
    }

    public class MonsterSystem : Singleton<MonsterSystem>, IEventExecuteSink
    {
        //�����Ĵ�������
        public static CreateMonsterContext s_CreateContext = new CreateMonsterContext();

        //������Ҫ�����߼��Ķ���
        private Dictionary<ulong, IMonster> m_dicMonster = new Dictionary<ulong, IMonster>();

        //ɾ���б�
        private HashSet<ulong> m_hashWaitDel = new HashSet<ulong>();

        //AI������
        private IAIActionCreator m_AICreator = null;

        //�����޸���
        private IATTRModifier m_attrModifier = null;

        //ʵ����ڵ�
        private Transform entityRoot;

        public Vector3 BossDeathPosition { private set; get; } = Vector3.zero;

        public void RegisterEntityRoot(Transform root)
        {
            entityRoot = root;
        }

        public void DeregisterEntityRoot()
        {
            entityRoot = null;
        }

        public Transform GetEntityRoot()
        {
            return entityRoot;
        }


        //����ϵͳ
        public void Create()
        {
            GameGlobal.EntityWorld.RegisterEntityType<Monster>(EntityType.monsterType);

            GameGlobal.EntityWorld.RegisterEntityPartType<AIPart>(EntityType.monsterType, EntityPartType.entityAIPartType);
            GameGlobal.EntityWorld.RegisterEntityPartType<LightnEffectPart>(EntityType.monsterType, EntityPartType.Lightn);
            GameGlobal.EntityWorld.RegisterEntityPartType<LightnEffectPart>(EntityType.monsterType, EntityPartType.Lightn);
            GameGlobal.EntityWorld.RegisterEntityPartType<SpriteRendererMaterialSwitchPart>(EntityType.monsterType, EntityPartType.entityMaterialSwitchType);

            GameGlobal.EntityWorld.RegisterEntityPartType<MonsterDataPart>(EntityType.monsterType, EntityPartInnerType.Data);
            GameGlobal.EntityWorld.RegisterEntityPartType<EntityMovePart>(EntityType.monsterType, EntityPartType.entityMovePartType);
            //GameGlobal.EntityWorld.RegisterEntityPartType<SkillPart>( EntityType.monsterType, EntityPartType.entitySkillPartType );
            GameGlobal.EntityWorld.RegisterEntityPartType<PrefabPart>(EntityType.monsterType, EntityPartType.Prefab);
            //GameGlobal.EntityWorld.RegisterEntityPartType<SpineSkinPart>(EntityType.monsterType, EntityPartType.Skin);
            //ע��AI��Ϊ����


            IItemPoolManager itemPoolMgr = XGame.XGameComs.Get<IItemPoolManager>();
            //itemPoolMgr.Register<AIMoveAction>();
            //itemPoolMgr.Register<AISkillAction>();
            //itemPoolMgr.Register<AICollisionExplosionAction>();


            //����������Ϣ
            IEventEngine eventEngine = XGameComs.Get<IEventEngine>();
            eventEngine.Subscibe(this, DGlobalEvent.EVENT_ENTITY_DESTROY, XClient.Common.DEventSourceType.SOURCE_TYPE_ENTITY, 0,
                "MonsterSystem:Create"); // FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity);

            eventEngine.Subscibe(this, DHeroTeamEvent.EVENT_WIN, GameScripts.HeroTeam.DEventSourceType.SOURCE_TYPE_ENTITY, 0,
                   "MonsterSystem:Create");
        }

        public void Release()
        {
            /*
            IItemPoolManager itemPoolMgr = XGame.XGameComs.Get<IItemPoolManager>();
            itemPoolMgr.Unregister<AIMoveAction>();
            itemPoolMgr.Unregister<AISkillAction>();
            itemPoolMgr.Unregister<AICollisionExplosionAction>();
            */

            //����������Ϣ
            IEventEngine eventEngine = XGameComs.Get<IEventEngine>();
            eventEngine.UnSubscibe(this, DGlobalEvent.EVENT_ENTITY_DESTROY, GameScripts.HeroTeam.DEventSourceType.SOURCE_TYPE_ENTITY,
                0); // FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity);
        }
        //����������ʱ���������ݵ���,ע��ֻ��Ҫ����Moudleģ����Create����õ�����,Create�д����Ĳ�Ҫ������

        public void Clear()
        {
            foreach (var kv in m_dicMonster)
            {
                DestroyMonster(kv.Key);
            }
            m_dicMonster.Clear();
        }

        //���ˢ�ֵĸ���
        public int GetMonsterCount()
        {
            return m_dicMonster.Count;
        }

        /// <summary>
        /// ��ȡһ����Ӫ������Monster
        /// </summary>
        /// <param name="camp"></param>
        /// <returns></returns>
        public List<IMonster> GetMonstersByCamp(ulong camp)
        {
            var enumerator = (IEnumerator<IMonster>)m_dicMonster.Values.GetEnumerator();
            List<IMonster> list = new List<IMonster>();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.GetCamp() == camp)
                {
                    list.Add(enumerator.Current);
                }
            }
            return list;
        }


        /// <summary>
        /// ��ȡ�з���Ӫ������Monster
        /// </summary>
        /// <param name="camp"></param>
        /// <returns></returns>
        public List<IMonster> GetMonstersNotEqulCamp(ulong camp)
        {
            var enumerator = (IEnumerator<IMonster>)m_dicMonster.Values.GetEnumerator();
            List<IMonster> list = new List<IMonster>();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.GetCamp() != camp)
                {
                    list.Add(enumerator.Current);
                }
            }
            return list;
        }


        //����
        public IMonster CreateMonster(CreateMonsterContext createContext)
        {
            NetEntityShareInitContext.instance.Reset();
            NetEntityShareInitContext.instance.localInitContext = createContext;

            //����һ��ΨһID
            ulong entId = GameGlobal.Role.entityIDGenerator.Next();
            IMonster monster =
                GameGlobal.EntityWorld.Local.CreateEntity(EntityType.monsterType, entId, createContext.displayID,
                        NetEntityShareInitContext.instance) as
                    IMonster;

            m_dicMonster.Add(entId, monster);
            return monster;
        }

        //����
        public void DestroyMonster(ulong entId)
        {
            //Debug.LogError("DestroyMonster:" + entId);
            GameGlobal.EntityWorld.Local.DestroyEntity(entId);
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        public void Update()
        {
            //ɾ������
            int nCount = m_hashWaitDel.Count;
            if (nCount > 0)
            {
                foreach (ulong id in m_hashWaitDel)
                {
                    m_dicMonster.Remove(id);
                }

                m_hashWaitDel.Clear();
            }

            //�ƶ��߼�����

            foreach (IMonster monster in m_dicMonster.Values)
            {
                if (null != monster)
                {
                    monster.OnUpdate();

                    if (monster.IsDie())
                    {
                        if (monster.IsBoos())
                        {
                            //记录一下boss的死亡位置
                            BossDeathPosition = monster.GetTr().position;
                            GameGlobal.EventEgnine.FireExecute(GameScripts.HeroTeam.DHeroTeamEvent.EVENT_WIN, GameScripts.HeroTeam.DEventSourceType.SOURCE_TYPE_ENTITY, 0, null);
                        }
                        DestroyMonster(monster.id);
                        m_hashWaitDel.Add(monster.id);
                        //MonsterDieEffectMgr.Instance.AddMonster(monster.id);
                        continue;
                    }
                }
            }

            //��ֲ���
            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    foreach (IMonster monster in m_dicMonster.Values)
            //    {
            //        DestroyMonster(monster.id);
            //    }
            //
            //    m_dicMonster.Clear();
            //    Debug.Log("�ո��������");
            //}
        }


        public void OnExecute(ushort wEventID, byte bSrcType, uint dwSrcID, object pContext)
        {

            if (DHeroTeamEvent.EVENT_WIN == wEventID)
            {
                foreach (IMonster monster in m_dicMonster.Values)
                {
                    if (null != monster)
                    {
                        if (!monster.IsDie())
                        {
                            // if (((cfg_Monster)monster.config).HeroClass > HeroClassDef.WARRIOR)
                            // {
                            ////离boss比较远的玩家 跑到boss实体附近去捡装备
                            var prefab = monster.GetPart<PrefabPart>();
                            if (prefab != null)
                            {
                                if (prefab.transform.TryGetComponent<Actor>(out var actor))
                                {
                                    actor.Switch2State<ActorWinState>();
                                }
                            }
                            // }
                        }
                    }
                }
            }
            else if (DGlobalEvent.EVENT_ENTITY_DESTROY == wEventID)
            {
                IMonster monster = pContext as IMonster;
                if (null != monster)
                {
                    ulong entID = monster.id;
                    if (m_dicMonster.ContainsKey(entID))
                    {
                        //m_dicMonster[entID] = null;
                        m_hashWaitDel.Add(entID);
                    }
                }
            }
        }

        //��ȡAI������
        public IAIActionCreator GetAICreator()
        {
            return m_AICreator;
        }

        //����AI������
        public void SetAICreator(IAIActionCreator creator)
        {
            m_AICreator = creator;
        }

        //��ȡ�����޸���
        public IATTRModifier GetAttrModified()
        {
            return m_attrModifier;
        }

        //���������޸���
        public void SetAttrModifier(IATTRModifier modifier)
        {
            m_attrModifier = modifier;
        }

        //����ת��
        public Vector3 WorldPositionToBattlePosition(Vector3 worldPosition, bool isMyBattle)
        {

            return worldPosition;

            //return m_attrModifier.WorldPositionToBattlePosition(worldPosition, isMyBattle);
        }
    }
}