using System;
using System.Collections.Generic;
using UnityEngine;
using XGame;
using XGame.Asset;

namespace GameScripts.HeroTeam
{

    /// <summary>
    /// TransformPool
    /// 通用Transform对象池，负责Transform的复用与回收，
    /// 仅提供基础的对象池逻辑，不关心对象类型与标记，
    /// 由上层脚本决定回收与获取的对象类型和管理方式。
    /// </summary>
    public class TransformPool
    {
        // 私有构造，禁止外部直接 new
        private TransformPool() { }

        // 对象池栈
        private Stack<Transform> m_stackPool = new Stack<Transform>();
        // 已回收对象集合，用于防止重复回收
        private HashSet<Transform> m_setPooled = new HashSet<Transform>();
        // 活跃对象的父节点
        private Transform m_trActiveRoot;
        // 隐藏对象的父节点
        private Transform m_trHiddenRoot;
        // 预制体对象
        private GameObject m_Prefab;

        /// <summary>
        /// 通过资源路径构造对象池
        /// </summary>
        /// <param name="szResPath">资源路径</param>
        /// <param name="activeRoot">活跃节点父节点</param>
        /// <param name="hiddenRoot">隐藏节点父节点</param>
        public TransformPool(string szResPath, Transform activeRoot, Transform hiddenRoot)
        {
            var resLoader = XGameComs.Get<IGAssetLoader>();
            uint handle = 0;
            var prefab = (GameObject)resLoader.LoadResSync<GameObject>(szResPath, out handle);
            Debug.Assert(prefab != null, $"LoadResSync Fail, path: {szResPath}");
            CreateInternal(prefab, activeRoot, hiddenRoot);
        }

        /// <summary>
        /// 通过已有预制体构造对象池
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="activeRoot">活跃节点父节点</param>
        /// <param name="hiddenRoot">隐藏节点父节点</param>
        public TransformPool(GameObject prefab, Transform activeRoot, Transform hiddenRoot)
        {
            CreateInternal(prefab, activeRoot, hiddenRoot);
        }

        /// <summary>
        /// 静态工厂方法，创建TransformPool
        /// </summary>
        public static TransformPool Create(GameObject prefab, Transform activeRoot, Transform hiddenRoot)
        {
            return new TransformPool(prefab, activeRoot, hiddenRoot);
        }

        /// <summary>
        /// 内部初始化方法
        /// </summary>
        private void CreateInternal(GameObject prefab, Transform activeRoot, Transform hiddenRoot)
        {
            m_Prefab = prefab;
            m_trActiveRoot = activeRoot;
            m_trHiddenRoot = hiddenRoot;

            // 确保父节点激活状态
            if (!m_trActiveRoot.SafeGetActiveSelf()) m_trActiveRoot.SafeSetActive(true);
            if (m_trHiddenRoot.SafeGetActiveSelf()) m_trHiddenRoot.SafeSetActive(false);
        }

        /// <summary>
        /// 从池中获取一个Transform对象
        /// </summary>
        /// <param name="pos">目标位置</param>
        /// <param name="rotation">目标旋转</param>
        /// <returns>Transform对象</returns>
        public Transform Get(Vector3 pos, Quaternion rotation)
        {
            if (m_stackPool.Count > 0)
            {
                var tr = m_stackPool.Pop();
                tr.SetParent(m_trActiveRoot, false);
                tr.SetPositionAndRotation(pos, rotation);
                m_setPooled.Remove(tr);
                return tr;
            }
            else
            {
                var instance = GameObject.Instantiate(m_Prefab, pos, rotation, m_trActiveRoot);
                return instance.transform;
            }
        }

        /// <summary>
        /// 回收一个Transform对象到池中
        /// </summary>
        /// <param name="target">要回收的Transform</param>
        public void Release(Transform target)
        {
            if (m_setPooled.Contains(target))
            {
                Debug.LogWarning("Trying to release an object that has already been released to the pool.");
                return;
            }
            else
            {
                m_setPooled.Add(target);
                m_stackPool.Push(target);
                target.SetParent(m_trHiddenRoot);
            }
        }

    }
}