using System.Collections.Generic;
using UnityEngine;
using XGame;
using XGame.Asset;
using XGame.FrameUpdate;
using XGame.Utils;

namespace GameScripts.HeroTeam
{

    public class GameEffectManager : MonoSingleton<GameEffectManager>, IFrameUpdateSink
    {

        private readonly Dictionary<string, TransformPool> m_transformPool = new Dictionary<string, TransformPool>();
        private readonly Dictionary<Transform, TransformPool> m_transformPoolByTransform = new Dictionary<Transform, TransformPool>();
        private Transform m_trActiveRoot, m_trHiddenRoot;

        // 定义一个内部结构体用于管理回收任务，并使用对象池复用
        private class EffectRecycleTask
        {
            public float EndTime;
            public TransformPool Pool;
            public Transform EffectTransform;

            public void Reset()
            {
                EndTime = 0;
                Pool = null;
                EffectTransform = null;
            }
        }

        private List<EffectRecycleTask> m_recycleTasks = new List<EffectRecycleTask>();
        private Stack<EffectRecycleTask> m_taskPool = new Stack<EffectRecycleTask>();

        private EffectRecycleTask GetTask()
        {
            return m_taskPool.Count > 0 ? m_taskPool.Pop() : new EffectRecycleTask();
        }

        private void ReleaseTask(EffectRecycleTask task)
        {
            task.Reset();
            m_taskPool.Push(task);
        }


        public void Setup(Transform activeRoot, Transform hiddenRoot)
        {
            m_trActiveRoot = activeRoot;
            m_trHiddenRoot = hiddenRoot;


            var frameUpdMgr = XGameComs.Get<IFrameUpdateManager>();
            frameUpdMgr.RegUpdate(this, EnUpdateType.Update, "GameEffectManager.Update");
        }

        private TransformPool GetTransformPool(string resPath)
        {
            if (m_transformPool.TryGetValue(resPath, out TransformPool pool))
            {
                return pool;
            }

            var resLoader = XGameComs.Get<IGAssetLoader>();
            uint handle = 0;
            var prefab = (GameObject)resLoader.LoadResSync<GameObject>(resPath, out handle);
            Debug.Assert(prefab != null, $"LoadResSync Fail, path: {resPath}");

            return GetTransformPool(prefab);
        }

        private TransformPool GetTransformPool(GameObject prefab)
        {
            string resPath = prefab.name;
            if (m_transformPool.TryGetValue(resPath, out TransformPool pool))
            {
                return pool;
            }

            pool = new TransformPool(prefab, m_trActiveRoot, m_trHiddenRoot);
            m_transformPool[resPath] = pool;
            return pool;
        }

        public Transform ShowEffect(string resPath, float duration = 1f)
        {
            return ShowEffect(resPath, Vector3.zero, Quaternion.identity, duration);
        }

        public Transform ShowEffect(string resPath, Vector3 position, Quaternion rotation = default(Quaternion), float duration = 1f)
        {
            var pool = GetTransformPool(resPath);
            return ShowEffect(pool, position, rotation, duration);
        }

        public Transform ShowEffect(GameObject prefab, Vector3 position, Quaternion rotation = default(Quaternion), float duration = 1f)
        {
            var pool = GetTransformPool(prefab);
            return ShowEffect(pool, position, rotation, duration);
        }


        public Transform ShowEffect(TransformPool pool, Vector3 position, Quaternion rotation = default(Quaternion), float duration = 1f)
        {
            var effectTransform = pool.Get(position, rotation);
            if (effectTransform != null && duration > 0)
            {
                var task = GetTask();
                task.EndTime = Time.time + duration;
                task.Pool = pool;
                task.EffectTransform = effectTransform;
                m_recycleTasks.Add(task);
            }
            m_transformPoolByTransform[effectTransform] = pool;
            return effectTransform;
        }

        public void ReleaseEffect(Transform trEffect)
        {
            var task = m_recycleTasks.Find(task => task.EffectTransform == trEffect);
            if (task != null)
            {
                task.EndTime = 0;
            }
        }

        // 在Update中统一处理回收
        public void OnFrameUpdate()
        {
            float now = Time.time;
            for (int i = m_recycleTasks.Count - 1; i >= 0; i--)
            {
                var task = m_recycleTasks[i];
                if (now >= task.EndTime)
                {
                    if (task.EffectTransform != null)
                    {
                        task.Pool.Release(task.EffectTransform);
                    }
                    m_recycleTasks.RemoveAt(i);
                    ReleaseTask(task);
                }
            }
        }

        public void Release()
        {
            foreach (var t in m_recycleTasks)
            {
                if (t.EffectTransform != null)
                {
                    t.Pool.Release(t.EffectTransform);
                }
                ReleaseTask(t);
            }
            m_recycleTasks.Clear();

            // if (m_trActiveRoot != null)
            // {
            //     foreach (Transform child in m_trActiveRoot)
            //     {
            //         if (child != null && m_transformPoolByTransform.TryGetValue(child, out TransformPool pool))
            //         {
            //             pool.Release(child);
            //         }
            //     }
            // }
        }
    }

}