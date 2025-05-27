using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGame;
using XGame.Asset;
using XGame.Utils;

namespace GameScripts.HeroTeam
{

    /// <summary>
    /// 
    /// 子弹的工厂类
    /// 
    /// Recycle回收
    /// Get<T> T可以是Bullet的任意派生类 你可以在派生类是实现各色各样的子弹技能
    /// 
    /// </summary>
    public class BulletManager : Singleton<BulletManager>
    {

        private List<IBullet> m_ActiveBullets = new List<IBullet>();
        // private Stack<IBullet> m_FreeBulletPools = new Stack<IBullet>();
        private Transform m_trActiveRoot, m_trHidddenRoot;
        private Dictionary<Type, GameObject> m_PrefabDict = new Dictionary<Type, GameObject>();
        private Dictionary<Type, LinkedList<IBullet>> m_BulletPool = new Dictionary<Type, LinkedList<IBullet>>();
        private Dictionary<GameObject, List<GameObject>> m_PrefabPool = new Dictionary<GameObject, List<GameObject>>();
        public void Setup(Transform activeRoot, Transform hiddenRoot)
        {
            m_trActiveRoot = activeRoot;
            m_trHidddenRoot = hiddenRoot;

            //目前通用，后续通过配置类分池子
            //Bullet后期只做为一个容器
            //子弹长啥样子在 <Bullet>的派生类 初始化的时候去设置
            // var resPath = "Game/HeroTeam/GameResources/Prefabs/Game/Bullet.prefab";
            // var resLoader = XGameComs.Get<IGAssetLoader>();
            // resLoader.LoadRes<GameObject>(resPath, 0, this);
        }

        public void Update()
        {
            IBullet bullet = null;
            for (int i = m_ActiveBullets.Count - 1; i >= 0; i--)
            {
                bullet = m_ActiveBullets[i];
                if (bullet.CheckCollision())
                {
                    m_ActiveBullets[i] = m_ActiveBullets[m_ActiveBullets.Count - 1];
                    m_ActiveBullets[m_ActiveBullets.Count - 1] = null;
                    m_ActiveBullets.RemoveAt(m_ActiveBullets.Count - 1);
                    Recycle(bullet);
                    continue;
                }
                bullet.Fly();
            }
        }

        public void Recycle(IBullet bullet)
        {
            bullet.ClearState();
            bullet.GetTr().SetParent(m_trHidddenRoot, false);

            // m_FreeBulletPools.Push(bullet);
            var type = bullet.GetType();
            if (m_BulletPool.TryGetValue(type, out var pool))
            {
                pool.AddLast(new LinkedListNode<IBullet>(bullet));
            }
            else
            {
                Debug.LogError($"没有注册的池子: {type}");
            }
        }

        public IBullet Get<T>(cfg_HeroTeamBullet cfg) where T : Bullet, new()
        {
            IBullet bullet = null;
            Type type = typeof(T);
            if (!m_BulletPool.TryGetValue(type, out var pool))
            {
                pool = new LinkedList<IBullet>();
            }
            if (pool.Count == 0)
            {
                if (!m_PrefabDict.TryGetValue(type, out var pref))
                {
                    var resLoader = XGameComs.Get<IGAssetLoader>();
                    uint handle = 0;
                    pref = (GameObject)resLoader.LoadResSync<GameObject>(cfg.szResPath, out handle);
                    m_PrefabDict.Add(type, pref);
                }
                var inst = GameObject.Instantiate(pref);
                bullet = new T();
                bullet.Init(inst);
                bullet.SetConfig(cfg);
            }
            else
            {
                bullet = pool.Last.Value;
            }
            bullet.GetTr().SetParent(m_trActiveRoot, false);
            m_ActiveBullets.Add(bullet);
            return bullet;
        }

        public void ShowEffect(GameObject prefab, Vector3 pos, float duration = 1f)
        {
            if (!m_PrefabPool.TryGetValue(prefab, out var pool))
            {
                pool = new List<GameObject>();
            }
            GameObject ins = null;
            if (pool.Count == 0)
            {
                ins = GameObject.Instantiate(prefab, pos, Quaternion.identity, m_trActiveRoot);
            }
            else
            {
                ins = pool[pool.Count - 1];
                ins.transform.position = pos;
                ins.transform.SetParent(m_trActiveRoot, false);
                pool.RemoveAt(pool.Count - 1);
            }
            GameManager.instance.OpenCoroutine(DelayRecycleGameObject(ins, duration, pool));
        }

        private IEnumerator DelayRecycleGameObject(GameObject obj, float duration, List<GameObject> pool)
        {
            yield return new WaitForSeconds(duration);
            obj.transform.SetParent(m_trHidddenRoot, false);
            pool.Add(obj);
        }



        // public void OnLoadAssetSuccess(Object obj, uint nResKey, object ud = null)
        // {
        //     m_BulletPrefab = (GameObject)obj;
        // }

        public void OnLoadAssetFail(uint nResKey, object userData = null)
        {
            Debug.Log($"加载资源失败: {nResKey}");
        }
    }
}