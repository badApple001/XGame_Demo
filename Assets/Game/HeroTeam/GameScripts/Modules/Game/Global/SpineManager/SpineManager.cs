using System;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
// using XGame.Utils;


namespace GameScripts.HeroTeam
{


    /// <summary>
    /// 
    /// SpineManager
    /// 负责Spine动画实例的生成、回收、更新调度和LOD（Level of Detail）管理。
    /// 
    /// 主要功能：
    /// 1. 通过对象池管理SkeletonAnimation实例，减少频繁创建和销毁带来的性能开销。
    /// 2. 按距离分组管理Spine实例，实现不同距离下的降帧（LOD），提升整体性能。
    /// 3. 支持通过GameConfig/SpineManagerSetting配置LOD参数和分组数量。
    /// 4. 提供资源加载委托，支持自定义资源加载方式（如AssetBundle、Addressables等）。
    /// 5. 支持分区轮转更新，避免同一帧内大量Spine实例同时更新造成性能抖动。
    /// 
    /// 使用方式：
    /// - 通过SpineManager.Instance获取单例实例。
    /// - 调用Setup方法初始化主摄像机、配置参数和资源加载委托。
    /// - 通过RentSkeletonAnimation租赁SkeletonAnimation实例，使用完毕后自动回收到对象池。
    /// - 在游戏主循环中调用Update方法，实现分组轮转和LOD调度。
    /// 
    /// 注意事项：
    /// - 不建议手动修改SkeletonAnimation的__GroupId和__PoolIndex属性，避免池管理异常。
    /// - SpineManager为真单例模式，避免多实例带来的资源管理混乱。
    ///  
    /// </summary>
    public class SpineManager  /*: Singleton<SpineManager> */
    {

        //这个类尽量还是保持真单例模式比较好，使用模板创建的单例存在安全隐患。 
        private SpineManager() { }
        public static SpineManager Instance { private set; get; } = new SpineManager();


        private Camera m_MainCamera;
        private Transform m_trMainCamera;

        public class SpineAgent
        {
            public int FrameCounter;
            public int UpdateInterval;
            public SkeletonAnimation Skeleton;
            public Transform Transform;
            public float PreUpdateTime;
            public int LateUpdateable;
        }
        private Stack<SpineAgent> m_stackSpineAgentPool = new Stack<SpineAgent>();
        private List<SpineAgent>[] m_AgentGroups;
        private int m_iCurrentGroupIndex = 0;

        // 调整以下参数实现不同等级的降帧
        private float m_fHighDetailDistance = 10f;
        private float m_fMidDetailDistance = 25f;
        private int m_iHighFrequency = 1;   // 每帧更新
        private int m_iMidFrequency = 2;    // 每2帧更新
        private int m_iLowFrequency = 4;    // 每4帧更新
        private bool m_bLateUpdateDirty = false; //LateUpdate 脏标记

        // Spine实例池子
        private Transform m_trSpineCacheRoot;
        private Dictionary<string, UnityEngine.Pool.ObjectPool<SkeletonAnimation>> m_dicSpineObjPool = new Dictionary<string, UnityEngine.Pool.ObjectPool<SkeletonAnimation>>();
        private Dictionary<string, SkeletonDataAsset> m_dicShareSkeletonDataAssets = new Dictionary<string, SkeletonDataAsset>();
        private const string m_szPoolSpineTag = "[Pool Spine]";

        // 资源Bundle加载器
        private LoadResHandler<SkeletonDataAsset> loadResHandler;

        public void Setup(Camera mainCamera, SpineManagerSetting setting, LoadResHandler<SkeletonDataAsset> loadResHandler)
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
        /// 租赁一个SkeletonAnimation组件
        /// 
        /// var sa = SpineManager.RentSkeletonAnimation("xxx.skel");
        /// sa.transform.SetParent( xxxNode, false );
        /// sa.transform.localPosition = Vector3.zero;
        /// 
        /// </summary>
        /// <param name="szSkeletonDataAssetPath"></param>
        /// <returns></returns>
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
            }
            return pool.Get();
        }

        /// <summary>
        /// 归还 SkeletonAnimation组件
        /// </summary>
        /// <param name="skeleton"></param>
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


        private void Register(SkeletonAnimation skeleton)
        {
            if (skeleton == null) return;

            //直接禁用掉所有更新
            skeleton.enabled = false;

            //均匀分组
            int group = 0;
            int minCount = 9999;
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
            agent.FrameCounter = group;  //避免同帧爆发
            agent.UpdateInterval = m_iHighFrequency;
            agent.PreUpdateTime = TimeUtils.CurrentTime;
            agent.LateUpdateable = 0;
            m_AgentGroups[group].Add(agent);
            skeleton.__GroupId = group; //默认池子
            skeleton.__PoolIndex = m_AgentGroups[group].Count - 1; //默认下标
        }

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
                Debug.LogError($"请不要手动修改 SkeletonAnimation内部的__PoolIndex");
#endif
                //修复 PoolIndex
                //回收
                for (int i = 0; i < pool.Count; i++)
                {
                    if (pool[i].Skeleton == skeleton)
                    {
                        SleepSkeleton(i, pool);
                        return;
                    }
                }

#if UNITY_EDITOR
                Debug.LogError("请不要在外部对m_AgentGroups进行修改处理");
#endif
            }
        }


        //内部 方法，调用前一定要先判断好各种越界情况 再调用
        private void SleepSkeleton(int PoolIndex, List<SpineAgent> pool)
        {
            //回收
            var release_agent = pool[PoolIndex];
            m_stackSpineAgentPool.Push(release_agent);

            //从激活池子里移除
            var swap_agent = pool[pool.Count - 1];
            //不需要检测，每个代理永远都不可能为null，不对外暴露，内部处理
            // if (swap_agent == null)
            //     throw new UnityEngine.UnityException("池子里的元素为null");
            swap_agent.Skeleton.__PoolIndex = PoolIndex;
            pool[PoolIndex] = swap_agent;
            pool.RemoveAt(pool.Count - 1);
        }

        public void Update()
        {
            if (m_MainCamera == null) return;

            List<SpineAgent> group = m_AgentGroups[m_iCurrentGroupIndex];
            Vector3 camPos = m_trMainCamera.position;
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

            // 轮转
            m_iCurrentGroupIndex = (m_iCurrentGroupIndex + 1) % m_AgentGroups.Length;
        }


    }


    [CreateAssetMenu(fileName = "SpineManagerSetting", menuName = "GameConfig/SpineManagerSetting", order = 100)]
    public class SpineManagerSetting : ScriptableObject
    {
        [Header("LOD配置")]
        public float highDetailDistance = 10f;
        public float midDetailDistance = 25f;

        [Header("不同等级下 更新的帧间隔")]
        [Range(1, 16)]
        public int highFrequency = 1;
        [Range(1, 16)]
        public int midFrequency = 2;
        [Range(1, 16)]
        public int lowFrequency = 4;

        [Header("分区激活")]
        [Tooltip("将激活的Spine分为{groupCount}个分区")]
        [Range(1, 8)]
        public int groupCount = 4;
    }

}