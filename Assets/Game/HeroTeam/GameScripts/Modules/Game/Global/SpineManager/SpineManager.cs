using System;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

namespace GameScripts.HeroTeam
{
    /// <summary>
    /// SpineManager
    /// 负责 Spine 动画的统一管理，包括实例生成、更新调度、LOD 降级控制和对象池机制。
    ///
    /// 主要职责：
    /// 1. 对 SkeletonAnimation 实例进行对象池管理，避免频繁创建与销毁；
    /// 2. 根据主摄像机与目标之间的距离，动态设定不同更新频率（LOD）；
    /// 3. 支持通过 ScriptableObject 配置参数，如更新间隔、分区数量、距离阈值；
    /// 4. 提供 Spine 实例的租赁与归还接口；
    /// 5. 使用分区轮转更新机制，避免同一帧中更新所有实例造成性能峰值；
    /// 6. 采用真单例模式，确保唯一实例管理，避免资源冲突。
    ///
    /// 使用方式：
    /// - 调用 `SpineManager.Instance.Setup(...)` 进行初始化；
    /// - 使用 `RentSkeletonAnimation()` 租用 Spine 实例；
    /// - 使用 `RemandSkeletonAnimation()` 回收实例；
    /// - 在主循环中调用 `Update()` 以驱动分区轮转和 LOD 调度；
    ///
    /// 注意事项：
    /// - 实例中的 `__GroupId` 和 `__PoolIndex` 为内部池管理字段，请勿手动更改；
    /// - SpineManager 中的资源池默认支持自动清理，但不主动卸载资源；
    /// </summary>
    public class SpineManager
    {
        // 使用真单例方式，避免模板单例带来的静态构造顺序或泛型实例冲突问题
        private SpineManager() { }
        public static SpineManager Instance { private set; get; } = new SpineManager();

        private Camera m_MainCamera;
        private Transform m_trMainCamera;

        /// <summary>
        /// Spine动画代理，用于记录每个实例的运行状态、更新间隔与位置引用等
        /// </summary>
        public class SpineAgent
        {
            public int FrameCounter;       // 帧计数器，用于计算是否需要更新
            public int UpdateInterval;     // 当前更新间隔（帧数）
            public SkeletonAnimation Skeleton;
            public Transform Transform;
            public float PreUpdateTime;    // 上次更新时间
            public int LateUpdateable;     // 是否需要LateUpdate（1表示需要）
        }

        // SpineAgent对象池，用于复用代理实例
        private Stack<SpineAgent> m_stackSpineAgentPool = new Stack<SpineAgent>();

        // SpineAgent分组，每组在一帧中被调度更新，避免性能峰值
        private List<SpineAgent>[] m_AgentGroups;
        private int m_iCurrentGroupIndex = 0;

        // LOD 相关参数
        private float m_fHighDetailDistance = 10f;
        private float m_fMidDetailDistance = 25f;
        private int m_iHighFrequency = 1;   // 高精度，帧间隔为1
        private int m_iMidFrequency = 2;    // 中精度
        private int m_iLowFrequency = 4;    // 低精度
        private bool m_bLateUpdateDirty = false; // LateUpdate脏标记

        // Spine 实例缓存的父节点（隐藏在场景中）
        private Transform m_trSpineCacheRoot;

        // 每种资源路径对应一个对象池
        private Dictionary<string, UnityEngine.Pool.ObjectPool<SkeletonAnimation>> m_dicSpineObjPool = new();
        private Dictionary<string, SkeletonDataAsset> m_dicShareSkeletonDataAssets = new();
        private const string m_szPoolSpineTag = "[Pool Spine]";

        // 外部资源加载委托（支持自定义加载逻辑，如AB或Addressables）
        private LoadResHandler<SkeletonDataAsset> loadResHandler;

        /// <summary>
        /// 初始化 SpineManager，必须调用一次
        /// </summary>
        /// <param name="mainCamera">用于LOD计算的主摄像机</param>
        /// <param name="setting">LOD与分区设置</param>
        /// <param name="loadResHandler">资源加载回调</param>
        public void Setup(Camera mainCamera, SpineManagerLODConfig setting, LoadResHandler<SkeletonDataAsset> loadResHandler)
        {
            m_MainCamera = mainCamera;
            m_trMainCamera = mainCamera.transform;

            m_fHighDetailDistance = setting.highDetailDistance;
            m_fMidDetailDistance = setting.midDetailDistance;
            m_iHighFrequency = setting.highFrequency;
            m_iMidFrequency = setting.midFrequency;
            m_iLowFrequency = setting.lowFrequency;

            m_AgentGroups = new List<SpineAgent>[setting.groupCount];
            for (int i = 0; i < setting.groupCount; i++)
            {
                m_AgentGroups[i] = new();
            }

            m_trSpineCacheRoot = new GameObject("[SpineManager]").transform;
            GameObject.DontDestroyOnLoad(m_trSpineCacheRoot.gameObject);
            m_trSpineCacheRoot.gameObject.SetActive(false);

            this.loadResHandler = loadResHandler;
            Debug.Assert(loadResHandler != null, "SpineManager.Setup 初始化必须配置资源获取句柄");
        }

        /// <summary>
        /// 租赁一个SkeletonAnimation实例（会自动从池中复用或创建）
        /// </summary>
        public SkeletonAnimation RentSkeletonAnimation(string szSkeletonDataAssetPath)
        {
            if (!m_dicSpineObjPool.TryGetValue(szSkeletonDataAssetPath, out var pool))
            {
                if (!m_dicShareSkeletonDataAssets.TryGetValue(szSkeletonDataAssetPath, out var dataAsset))
                {
                    dataAsset = loadResHandler(szSkeletonDataAssetPath);
                    Debug.Assert(dataAsset != null, $"LoadResSync Fail, path: {szSkeletonDataAssetPath}");
                    m_dicShareSkeletonDataAssets.Add(szSkeletonDataAssetPath, dataAsset);
                    dataAsset.__InstancePoolKey = szSkeletonDataAssetPath;
                }

                pool = new UnityEngine.Pool.ObjectPool<SkeletonAnimation>(() =>
                {
                    var obj = new GameObject(m_szPoolSpineTag);
                    obj.transform.SetParent(m_trSpineCacheRoot, false);
                    return SkeletonAnimation.AddToGameObject(obj, dataAsset);
                }, on_get_obj =>
                {
                    Register(on_get_obj);
                }, on_release_obj =>
                {
                    UnRegister(on_release_obj);
                    on_release_obj.transform.SetParent(m_trSpineCacheRoot);
                }, on_destory_obj =>
                {
                    GameObject.Destroy(on_destory_obj.gameObject);
                }, true, 0);

                m_dicSpineObjPool[szSkeletonDataAssetPath] = pool;
            }

            return pool.Get();
        }

        /// <summary>
        /// 将SkeletonAnimation归还回对象池
        /// </summary>
        public void RemandSkeletonAnimation(SkeletonAnimation skeleton)
        {
            if (skeleton == null || skeleton.skeletonDataAsset == null)
            {
#if UNITY_EDITOR
                Debug.LogError("尝试归还无效的骨骼");
#endif
                return;
            }

            if (m_dicSpineObjPool.TryGetValue(skeleton.skeletonDataAsset.__InstancePoolKey, out var pool))
            {
                pool.Release(skeleton);
            }
        }

        /// <summary>
        /// 注册新租出的SkeletonAnimation到轮转分组中
        /// </summary>
        private void Register(SkeletonAnimation skeleton)
        {
            if (skeleton == null) return;

            skeleton.enabled = false;

            // 找到最少的分组，平衡负载
            int group = 0;
            int minCount = int.MaxValue;
            for (int i = 0; i < m_AgentGroups.Length; i++)
            {
                int tmpCount = m_AgentGroups[i].Count;
                if (tmpCount < minCount)
                {
                    minCount = tmpCount;
                    group = i;
                }
            }

            SpineAgent agent = m_stackSpineAgentPool.Count == 0 ? new SpineAgent() : m_stackSpineAgentPool.Pop();
            agent.Skeleton = skeleton;
            agent.Transform = skeleton.transform;
            agent.FrameCounter = group;
            agent.UpdateInterval = m_iHighFrequency;
            agent.PreUpdateTime = TimeUtils.CurrentTime;
            agent.LateUpdateable = 0;

            m_AgentGroups[group].Add(agent);
            skeleton.__GroupId = group;
            skeleton.__PoolIndex = m_AgentGroups[group].Count - 1;
        }

        /// <summary>
        /// 从更新分组中移除SkeletonAnimation，回收到代理池中
        /// </summary>
        private void UnRegister(SkeletonAnimation skeleton)
        {
            if (skeleton == null) return;

#if UNITY_EDITOR
            Debug.Assert(skeleton.__GroupId != -1, "请不要手动修改 SkeletonAnimation内部的__GroupId");
            Debug.Assert(skeleton.__PoolIndex != -1, "请不要手动修改 SkeletonAnimation内部的__PoolIndex");

#endif

            var pool = m_AgentGroups[skeleton.__GroupId];
            if (skeleton.__PoolIndex >= 0 && skeleton.__PoolIndex < pool.Count && pool[skeleton.__PoolIndex].Skeleton == skeleton)
            {
                SleepSkeleton(skeleton.__PoolIndex, pool);
            }
            else
            {

#if UNITY_EDITOR
                Debug.LogWarning($"请不要手动修改 SkeletonAnimation内部的__PoolIndex");
#endif

                for (int i = 0; i < pool.Count; i++)
                {
                    if (pool[i].Skeleton == skeleton)
                    {
                        SleepSkeleton(i, pool);
                        return;
                    }
                }

#if UNITY_EDITOR
                Debug.LogError("未能从分组中正确回收Skeleton，可能__PoolIndex索引错误或外部干预！");
#endif
            }
        }

        /// <summary>
        /// 执行实际回收，将SkeletonAnimation从激活组中移除
        /// </summary>
        private void SleepSkeleton(int poolIndex, List<SpineAgent> pool)
        {
            var release_agent = pool[poolIndex];
            m_stackSpineAgentPool.Push(release_agent);

            var swap_agent = pool[pool.Count - 1];
            swap_agent.Skeleton.__PoolIndex = poolIndex;
            pool[poolIndex] = swap_agent;
            pool.RemoveAt(pool.Count - 1);
        }

        /// <summary>
        /// 在每帧中调用，驱动轮转更新与LOD分发
        /// </summary>
        public void Update()
        {
            if (m_MainCamera == null) return;

            List<SpineAgent> group = m_AgentGroups[m_iCurrentGroupIndex];
            Vector3 camPos = GetScreenCenterProjectionOnZPlane();
            m_bLateUpdateDirty = false;

            foreach (var agent in group)
            {
                float dist = Vector3.Distance(camPos, agent.Transform.position);

                if (dist < m_fHighDetailDistance)
                    agent.UpdateInterval = m_iHighFrequency;
                else if (dist < m_fMidDetailDistance)
                    agent.UpdateInterval = m_iMidFrequency;
                else
                    agent.UpdateInterval = m_iLowFrequency;

                agent.FrameCounter++;
                if (agent.FrameCounter >= agent.UpdateInterval)
                {
                    agent.FrameCounter = 0;
                    float deltaTime = TimeUtils.CurrentTime - agent.PreUpdateTime;
                    agent.PreUpdateTime = TimeUtils.CurrentTime;
                    agent.Skeleton.UpdateSkeleton(deltaTime);
                    agent.LateUpdateable = 1;
                    m_bLateUpdateDirty = true;
                }
            }

            if (m_bLateUpdateDirty)
            {
                foreach (var agent in group)
                {
                    if (agent.LateUpdateable == 1)
                    {
                        agent.LateUpdateable = 0;
                        agent.Skeleton.LateUpdate();
                    }
                }
            }

            // 分组轮转更新
            m_iCurrentGroupIndex = (m_iCurrentGroupIndex + 1) % m_AgentGroups.Length;
        }

        //这块根据你自己的项目 自行去处理
        //我这个项目默认的是2D视角
        private Vector3 GetScreenCenterProjectionOnZPlane()
        {
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector3 origin = m_MainCamera.ScreenToWorldPoint(new Vector3(screenCenter.x, screenCenter.y, m_MainCamera.nearClipPlane));
            Vector3 direction = m_trMainCamera.forward;

            if (Mathf.Approximately(direction.z, 0f))
                return origin; // 避免除以 0

            float t = -origin.z / direction.z;

            Vector3 hitPoint = origin + direction * t;

#if UNITY_EDITOR
            //绘制中心线
            Debug.DrawLine(origin, hitPoint, Color.blue);
#endif


            return hitPoint;
        }


        //暂时用于面板上的Debug调试系统    
        public SpineAgent GetSpineAgent(SkeletonAnimation skeleton)
        {
            if (skeleton == null) return null;
            var pool = m_AgentGroups[skeleton.__GroupId];
            if (skeleton.__PoolIndex >= 0 && skeleton.__PoolIndex < pool.Count && pool[skeleton.__PoolIndex].Skeleton == skeleton)
            {
                return pool[skeleton.__PoolIndex];
            }
            else
            {
                for (int i = 0; i < pool.Count; i++)
                {
                    if (pool[i].Skeleton == skeleton)
                    {
                        return pool[i];
                    }
                }
            }
            return null;
        }
    }
}


