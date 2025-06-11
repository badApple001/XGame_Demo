using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGame;
using XGame.Asset;
using XGame.FrameUpdate;

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
    public class BulletManager : MonoSingleton<BulletManager>, IFrameUpdateSink
    {

        private List<IBullet> m_ActiveBullets = new List<IBullet>();
        private Transform m_trActiveRoot, m_trHidddenRoot;
        private Dictionary<int, UnityEngine.Pool.ObjectPool<Bullet>> m_BulletPool = new Dictionary<int, UnityEngine.Pool.ObjectPool<Bullet>>();
        private Dictionary<int, GameObject> m_PrefabDict = new Dictionary<int, GameObject>();

        // private Stack<IBullet> m_FreeBulletPools = new Stack<IBullet>();

        // private Dictionary<GameObject, List<GameObject>> m_PrefabPool = new Dictionary<GameObject, List<GameObject>>();

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

            var frameUpdMgr = XGameComs.Get<IFrameUpdateManager>();
            frameUpdMgr.RegUpdate(this, EnUpdateType.Update, "BulletManager.Update");
        }


        public void OnFrameUpdate()
        {
            IBullet bullet = null;
            for (int i = m_ActiveBullets.Count - 1; i >= 0; i--)
            {
                bullet = m_ActiveBullets[i];
                if (bullet.IsExpired())
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

            // m_FreeBulletPools.Push(bullet);
            // if (m_BulletPool.TryGetValue(bullet.GetPoolId(), out var pool))
            // {
            //     // Debug.Log($"池子: {bullet.GetPoolId()}");
            //     pool.AddLast(new LinkedListNode<IBullet>(bullet));
            // }
            // else
            // {
            //     Debug.LogError($"没有注册的池子: {bullet.GetPoolId()}");
            // }
            // int hashCode = bullet.GetType().GetHashCode();
            int hashCode = bullet.GetPoolId();
            if (m_BulletPool.TryGetValue(hashCode, out var pool))
            {
                pool.Release(bullet as Bullet);
            }
            else
            {
                string ero_msg = $"Pool undefined: {bullet.GetType().FullName}";
                Debug.LogError(ero_msg);
#if UNITY_EDITOR
                throw new System.Exception(ero_msg);
#endif
            }
        }

        public T Get<T>(cfg_HeroTeamBullet cfg, Vector3 newPos) where T : Bullet, new()
        {
            // int hashCode = typeof(T).GetHashCode();
            int hashCode = cfg.iID;
            if (!m_BulletPool.TryGetValue(hashCode, out var pool))
            {
                //获取模型预设
                if (!m_PrefabDict.TryGetValue(cfg.iID, out var pref))
                {
                    var resLoader = XGameComs.Get<IGAssetLoader>();
                    uint handle = 0;
                    pref = (GameObject)resLoader.LoadResSync<GameObject>(cfg.szResPath, out handle);
                    m_PrefabDict.Add(cfg.iID, pref);
                }

                //创建对象池
                pool = new UnityEngine.Pool.ObjectPool<Bullet>(() =>
                {
                    var creat_blt = new T();
                    creat_blt.Init(GameObject.Instantiate(pref));
                    creat_blt.SetConfig(cfg);
                    return creat_blt;
                }, get_blt =>
                {
                    get_blt.GetTr().SetParent(m_trActiveRoot, false);
                }, rels_blt =>
                {
                    rels_blt.ClearState();
                    rels_blt.GetTr().SetParent(m_trHidddenRoot, false);
                }, cls_blt => Destroy(cls_blt.GetTr().gameObject), true, 1);
                m_BulletPool.Add(hashCode, pool);
            }

            var bullet = pool.Get();
            bullet.Active(newPos);
            m_ActiveBullets.Add(bullet);
            return bullet as T;
        }

        public Transform ShowEffect(string effectPath, Vector3 pos, float duration = 1f) => ShowEffect(effectPath, pos, Quaternion.identity, duration);
        public Transform ShowEffect(string effectPath, Vector3 pos, Quaternion rotation, float duration = 1f) => GameEffectManager.Instance.ShowEffect(effectPath, pos, rotation, duration);

        // private IEnumerator DelayRecycleGameObject(GameObject obj, float duration, List<GameObject> pool)
        // {
        //     yield return new WaitForSeconds(duration);
        //     obj.transform.SetParent(m_trHidddenRoot, false);
        //     pool.Add(obj);
        // }

        // public void OnLoadAssetSuccess(Object obj, uint nResKey, object ud = null)
        // {
        //     m_BulletPrefab = (GameObject)obj;
        // }

        // public void OnLoadAssetFail(uint nResKey, object userData = null)
        // {
        //     Debug.Log($"加载资源失败: {nResKey}");
        // }


        public void Release()
        {
            foreach (var bullet in m_ActiveBullets)
            {
                bullet.ClearState();
                var tr = bullet.GetTr();
                if (tr != null)
                    GameObject.Destroy(tr.gameObject);
            }
            m_ActiveBullets.Clear();

            foreach (var pool in m_BulletPool.Values)
            {
                pool.Clear();
            }
            m_BulletPool.Clear();

            foreach (var prefab in m_PrefabDict.Values)
            {
                if (prefab != null)
                    GameObject.Destroy(prefab);
            }
            m_PrefabDict.Clear();

            // foreach (var pool in m_PrefabPool.Values)
            // {
            //     foreach (var obj in pool)
            //     {
            //         if (obj != null)
            //             GameObject.Destroy(obj);
            //     }
            //     pool.Clear();
            // }
            // m_PrefabPool.Clear();

            m_trActiveRoot = null;
            m_trHidddenRoot = null;
        }

    }
}