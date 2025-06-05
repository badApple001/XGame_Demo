using System;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XClient.Entity;
using XGame;
using XGame.Entity;
using XGame.Entity.Part;
using XGame.EventEngine;
using XGame.FrameUpdate;
using XGame.Poolable;
using XGame.Utils;

namespace GameScripts.HeroTeam
{
    /// <summary>
    /// 创建Actor时的上下文信息
    /// </summary>
    public class CreateActorContext
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        // public static CreateActorContext Instance { private set; get; } = new CreateActorContext();
        private CreateActorContext() { }
        private static CreateActorContext _instance = null;
        public static CreateActorContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CreateActorContext();
                }
                else
                {
                    _instance.Clear();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Actor配置ID
        /// </summary>
        public int nActorCfgID;
        /// <summary>
        /// Actor所属阵营
        /// </summary>
        public int nCamp;
        /// <summary>
        /// 父节点
        /// </summary>
        public Transform parent;
        /// <summary>
        /// 出生位置
        /// </summary>
        public Vector3 worldPos;
        /// <summary>
        /// 出生的朝向
        /// </summary>
        public Vector3 forward;

        /// <summary>
        /// 需要挂的mono
        /// </summary>
        public List<Type> MonoTypes = new List<Type>();

        private void Clear()
        {
            nActorCfgID = 0;
            nCamp = 0;
            parent = null;
            worldPos = Vector3.zero;
            forward = Vector3.forward;
            MonoTypes.Clear();
        }
    }

    /// <summary>
    /// 实体类型定义
    /// </summary>
    public partial class EntityType
    {
        private readonly static int EntityTypeBase = 400;

        /// <summary>
        /// Actor模型
        /// </summary>
        public readonly static int Actor = EntityTypeBase++;
    }

    public partial class EntityPartType : EntityPartInnerType
    {
        private readonly static int EntityPartBase = 400;

        /// <summary>
        /// SkeletonRuntimeSortPart
        /// </summary>
        public readonly static int SkeletonRuntimeSortPart = EntityPartBase++;

        /// <summary>
        /// 移动目标部件
        /// </summary>
        public readonly static int TargetMoverPart = EntityPartBase++;
        public readonly static int SkelForwardPart = EntityPartBase++;
    }

    /// <summary>
    /// Actor阵营更新处理管道接口
    /// </summary>
    public interface IActorCampUpdateProcessPipe
    {
        /// <summary>
        /// 更新指定阵营的Actor列表
        /// </summary>
        void Update(int camp, List<IActor> actors);
    }

    /// <summary>
    /// Actor阵营更新处理管道的包装类（用于对象池）
    /// </summary>
    public class ActorCampUpdateProcessPipe : IPoolable
    {
        /// <summary>
        /// 管道实现
        /// </summary>
        public IActorCampUpdateProcessPipe pipe;
        /// <summary>
        /// 阵营编号
        /// </summary>
        public int camp;

        public bool Create()
        {
            return true;
        }

        public void Init(object context = null)
        {

        }

        public void Release()
        {
            pipe = null;
            camp = 0;
        }

        public void Reset()
        {

        }
    }

    /// <summary>
    /// Actor系统，负责管理所有角色的生命周期、分组、更新等
    /// </summary>
    public class ActorManager : Singleton<ActorManager>, IEventExecuteSink, IFrameUpdateSink
    {
        /// <summary>
        /// Actor实体的根节点
        /// </summary>
        private Transform m_trEntityRoot;

        /// <summary>
        /// Actor分组，key为阵营，value为该阵营下的所有Actor
        /// </summary>
        private Dictionary<int, List<IActor>> m_dicGroupActor = new Dictionary<int, List<IActor>>();

        /// <summary>
        /// 通过entId获取对象
        /// </summary>
        private Dictionary<ulong, IActor> m_dicEntIdActor = new Dictionary<ulong, IActor>();

        /// <summary>
        /// 角色列表缓存 用于高效遍历
        /// </summary>
        private List<IActor> m_arrActiveActor = new List<IActor>();

        /// <summary>
        /// 需要刷新激活的角色列表
        /// </summary>
        private bool m_NeedRefreshActiveActorList = false;

        /// <summary>
        /// 等待删除的角色列表
        /// </summary>
        private List<ulong> m_arrWaitDeleteActor = new List<ulong>();

        /// <summary>
        /// 角色阵营更新管线 ： 采用管线处理，让业务逻辑从这里剥离出去，降低耦合
        /// </summary>
        private List<ActorCampUpdateProcessPipe> m_arrActorCampUpdateProcessPipes = new List<ActorCampUpdateProcessPipe>();

        /// <summary>
        /// 保持统一的EntityManager
        /// </summary>
        private IEntityManager m_EntityManager;

        /// <summary>
        /// 缓存一份Boss的死亡的位置
        /// </summary>
        public Vector3 BossDeathPosition { private set; get; } = Vector3.zero;

        /// <summary>
        /// 角色死亡处理回调
        /// </summary>
        public Action<IActor> ActorDieHandler;

        /// <summary>
        /// 系统初始化
        /// </summary>
        public void Setup(Transform entityRoot = null)
        {
            m_EntityManager = GameGlobal.EntityWorld.Local;
            RegisterEntityRoot(entityRoot);

            // 注册Actor相关的实体类型和部件
            GameGlobal.EntityWorld.RegisterEntityType<Actor>(EntityType.Actor);
            GameGlobal.EntityWorld.RegisterEntityPartType<PrefabPart>(EntityType.Actor, EntityPartType.Prefab);
            GameGlobal.EntityWorld.RegisterEntityPartType<ActorDataPart>(EntityType.Actor, EntityPartType.Data);
            GameGlobal.EntityWorld.RegisterEntityPartType<ActorSkelRTSortPart>(EntityType.Actor, EntityPartType.SkeletonRuntimeSortPart);
            GameGlobal.EntityWorld.RegisterEntityPartType<ActorTargetMoverPart>(EntityType.Actor, EntityPartType.TargetMoverPart);
            GameGlobal.EntityWorld.RegisterEntityPartType<ActorSkelForwardPart>(EntityType.Actor, EntityPartType.SkelForwardPart);

            // 注册帧更新
            var frameUpdMgr = XGameComs.Get<IFrameUpdateManager>();
            frameUpdMgr.RegUpdate(this, EnUpdateType.Update, "ActorSystem.Update");

            // 注册对象池
            XGameComs.Get<IItemPoolManager>().Register<ActorCampUpdateProcessPipe>(16);


            // 订阅胜利事件
            // GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_WIN, GameScripts.HeroTeam.DEventSourceType.SOURCE_TYPE_ENTITY, 0,
            //        "ActorSystem:Create");
        }

        /// <summary>
        /// 释放系统资源
        /// </summary>
        public void Release()
        {
            // GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_WIN, GameScripts.HeroTeam.DEventSourceType.SOURCE_TYPE_ENTITY, 0);
            // 清理所有Actor
            foreach (var actor in m_dicEntIdActor.Values)
            {
                m_EntityManager.DestroyEntity(actor.id);
            }
            m_dicEntIdActor.Clear();

            // 清理分组
            foreach (var actors in m_dicGroupActor.Values)
            {
                actors.Clear();
            }
            m_dicGroupActor.Clear();

            // 清理激活Actor列表
            m_arrActiveActor.Clear();

            // 清理待删除Actor列表
            m_arrWaitDeleteActor.Clear();

            // 清理阵营更新管道
            IItemPoolManager itemPoolMgr = XGame.XGameComs.Get<IItemPoolManager>();
            foreach (var pipePacker in m_arrActorCampUpdateProcessPipes)
            {
                itemPoolMgr.Push(pipePacker);
            }
            m_arrActorCampUpdateProcessPipes.Clear();

            // 释放EntityManager引用
            m_EntityManager = null;

            // 清理死亡回调
            ActorDieHandler = null;

            // 清理Boss死亡位置
            BossDeathPosition = Vector3.zero;

            var frameUpdMgr = XGameComs.Get<IFrameUpdateManager>();
            frameUpdMgr.UnregUpdate(this, EnUpdateType.Update);
        }

        /// <summary>
        /// 注册阵营更新处理管道
        /// </summary>
        public void RegisterActorCampUpdateProcessPipe(int camp, IActorCampUpdateProcessPipe pipe)
        {
            IItemPoolManager itemPoolMgr = XGame.XGameComs.Get<IItemPoolManager>();
            ActorCampUpdateProcessPipe pipePacker = itemPoolMgr.Pop<ActorCampUpdateProcessPipe>(this);
            pipePacker.pipe = pipe;
            pipePacker.camp = camp;
            m_arrActorCampUpdateProcessPipes.Add(pipePacker);
        }

        /// <summary>
        /// 注销阵营更新处理管道
        /// </summary>
        public void UnRegisterActorCampUpdateProcessPipe(int camp, IActorCampUpdateProcessPipe pipe)
        {
            var packer = m_arrActorCampUpdateProcessPipes.Find(packer => packer.camp == camp && packer.pipe == pipe);
            if (null != packer)
            {
                m_arrActorCampUpdateProcessPipes.Remove(packer);
                IItemPoolManager itemPoolMgr = XGame.XGameComs.Get<IItemPoolManager>();
                itemPoolMgr.Push(packer);
            }
        }

        /// <summary>
        /// 注册实体根节点
        /// </summary>
        public void RegisterEntityRoot(Transform root)
        {
            m_trEntityRoot = root;
        }

        /// <summary>
        /// 获取实体根节点
        /// </summary>
        public Transform GetEntityRoot()
        {
            return m_trEntityRoot;
        }

        /// <summary>
        /// 创建Actor
        /// </summary>
        public IActor CreateActor(CreateActorContext ctx)
        {
            NetEntityShareInitContext.instance.Reset();
            NetEntityShareInitContext.instance.localInitContext = ctx;

#if UNITY_EDITOR
            Type type = Type.GetType("GameScripts.HeroTeam.ActorPartInspector");
            if (type != null) ctx.MonoTypes.Add(type);
#endif
            if (ctx.MonoTypes.Count > 0)
            {
                List<Type> removeTypes = new List<Type>();
                ctx.MonoTypes.ForEach(t =>
                {
                    if (!t.IsSubclassOf(typeof(Component)))
                    {
                        Debug.LogError($"{type.FullName} 不是继承自 Component 的类型。");
                        removeTypes.Add(t);
                    }
                });

                //线上兜底
                if (removeTypes.Count > 0)
                {
                    foreach (var t in removeTypes)
                    {
                        ctx.MonoTypes.Remove(t);
                    }
                }
            }

            if (null == ctx.parent) ctx.parent = m_trEntityRoot;
            ulong entId = GameGlobal.Role.entityIDGenerator.Next();
            IActor actor =
                m_EntityManager.CreateEntity(EntityType.Actor, entId, ctx.nActorCfgID,
                        NetEntityShareInitContext.instance) as
                    IActor;
            if (!m_dicGroupActor.TryGetValue(ctx.nCamp, out var actors))
            {
                actors = new List<IActor>();
                m_dicGroupActor.Add(ctx.nCamp, actors);
            }
            actors.Add(actor);
            m_dicEntIdActor[entId] = actor;
            m_NeedRefreshActiveActorList = true;
            return actor;
        }

        /// <summary>
        /// 标记怪物待销毁
        /// </summary>
        public void DestroyActor(ulong entId)
        {
            if (!m_arrWaitDeleteActor.Contains(entId))
            {
                m_arrWaitDeleteActor.Add(entId);
            }
        }


        /// <summary>
        /// 标记怪物待销毁
        /// </summary>
        public void DestroyActor(IActor actor) => DestroyActor(actor.id);

        /// <summary>
        /// 内部销毁Actor
        /// </summary>
        private void DestoryActorInternal(ulong entId)
        {
            if (m_dicEntIdActor.TryGetValue(entId, out var ent))
            {
                m_dicEntIdActor.Remove(entId);
                if (m_dicGroupActor.TryGetValue(ent.GetCamp(), out var actors))
                {
                    actors.Remove(ent);
                }
            }
            m_EntityManager.DestroyEntity(entId);
        }

        /// <summary>
        /// 刷新激活的Actor列表
        /// </summary>
        private void RefreshArrActors()
        {
            m_arrActiveActor.Clear();
            foreach (var kvp in m_dicGroupActor)
            {
                foreach (var a in kvp.Value)
                {
                    m_arrActiveActor.Add(a);
                }
            }
        }

        /// <summary>
        /// 事件回调
        /// </summary>
        public void OnExecute(ushort wEventID, byte bSrcType, uint dwSrcID, object pContext)
        {

        }

        /// <summary>
        /// 帧更新回调
        /// </summary>
        public void OnFrameUpdate()
        {
            // 处理待删除Actor
            if (m_arrWaitDeleteActor.Count > 0)
            {
                m_arrWaitDeleteActor.ForEach(entId => DestoryActorInternal(entId));
                m_arrWaitDeleteActor.Clear();
                m_NeedRefreshActiveActorList = true;
            }

            // 刷新激活Actor列表
            if (m_NeedRefreshActiveActorList)
            {
                m_NeedRefreshActiveActorList = false;
                RefreshArrActors();
            }

            // 遍历所有激活Actor，执行更新逻辑
            IActor actor;
            for (int i = 0; i < m_arrActiveActor.Count; i++)
            {
                actor = m_arrActiveActor[i];
                if (actor == null) continue;
                actor.OnUpdate();

                if (actor.IsDie())
                {
                    DestroyActor(actor.id);
                    // 角色死亡回调，必须注册
                    ActorDieHandler(actor);
                }
            }

            // 执行所有阵营更新管道
            for (int i = 0; i < m_arrActorCampUpdateProcessPipes.Count; i++)
            {
                var pipePacker = m_arrActorCampUpdateProcessPipes[i];
                if (m_dicGroupActor.TryGetValue(pipePacker.camp, out List<IActor> actors))
                {
                    pipePacker.pipe.Update(pipePacker.camp, actors);
                }
            }
        }


        /// <summary>
        /// 获取指定的阵营的所有角色
        /// </summary>
        /// <param name="camp"></param>
        /// <returns></returns>
        public List<IActor> GetActorsByCamp(int camp)
        {
            if (m_dicGroupActor.TryGetValue(camp, out List<IActor> actors))
            {
                return actors;
            }
            return null;
        }


    }
}