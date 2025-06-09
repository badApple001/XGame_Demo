using UnityEngine;

namespace GameScripts.HeroTeam
{

    /// <summary>
    /// 保持统一 Instance接口, 
    /// 框架内带的 一个Instance, 一个instance 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T m_Instance;
        private static readonly object m_Lock = new object();
        private static bool m_ApplicationIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (m_ApplicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton<{typeof(T)}>] Instance already destroyed on application quit.");
                    return null;
                }

                lock (m_Lock)
                {
                    if (m_Instance == null)
                    {
                        m_Instance = FindObjectOfType<T>();

                        if (m_Instance == null)
                        {
                            GameObject singletonObj = new GameObject(typeof(T).Name);
                            m_Instance = singletonObj.AddComponent<T>();
                            DontDestroyOnLoad(singletonObj);
                        }
                    }

                    return m_Instance;
                }
            }
        }

        protected virtual void OnDestroy()
        {
            if (m_Instance == this)
            {
                m_ApplicationIsQuitting = true;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            m_ApplicationIsQuitting = true;
        }
    }
}