using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGame.Utils;

public class BulletManager : Singleton<BulletManager>
{

    private List<IBullet> m_ActiveBullets = new List<IBullet>();
    private Stack<IBullet> m_FreeBulletPools = new Stack<IBullet>();
    private Transform m_trActiveRoot, m_trHidddenRoot;
    private GameObject m_BulletPrefab;
    public void Setup(Transform activeRoot, Transform hiddenRoot)
    {
        m_trActiveRoot = activeRoot;
        m_trHidddenRoot = hiddenRoot;
    }

    public void Update()
    {
        IBullet bullet = null;
        for (int i = m_ActiveBullets.Count; i >= 0; i--)
        {
            bullet = m_ActiveBullets[i];
            if (bullet.CheckCollision())
            {
                IBullet swapBullet = m_ActiveBullets[m_ActiveBullets.Count - 1];
                m_ActiveBullets[m_ActiveBullets.Count - 1] = m_ActiveBullets[i];
                m_ActiveBullets[i] = swapBullet;
                m_ActiveBullets.RemoveAt(m_ActiveBullets.Count - 1);


                continue;
            }
            bullet.Fly();
        }
    }

    public void Recycle(IBullet bullet)
    {
        bullet.ClearState();
        bullet.GetTr().SetParent(m_trHidddenRoot, false);
        m_FreeBulletPools.Push(bullet);
    }

    public IBullet Get()
    {
        IBullet bullet = null;
        if (m_FreeBulletPools.Count > 0)
        {
            bullet = m_FreeBulletPools.Pop();
        }
        else
        {
            //生成一个
            var inst = GameObject.Instantiate(m_BulletPrefab);

        }
        bullet.GetTr().SetParent(m_trActiveRoot, false);
        return bullet;
    }

}


public interface IBullet
{

    public bool CheckCollision();
    public void Fly();
    public void ClearState();
    public Transform GetTr();
}