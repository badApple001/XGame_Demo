using System;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XGame;
using XGame.FrameUpdate;
using XGame.Utils;

namespace GameScripts.HeroTeam
{

    public class BuffManager : MonoSingleton<BuffManager>, IFrameUpdateSink
    {
        private Dictionary<Type, Stack<IBuff>> m_BuffPool = new Dictionary<Type, Stack<IBuff>>();

        private List<IBuff> m_ActiveBuffs = new List<IBuff>();

        public BuffManager() => Setup();


        private void Setup()
        {
            var frameUpdMgr = XGameComs.Get<IFrameUpdateManager>();
            frameUpdMgr.RegUpdate(this, EnUpdateType.Update, "BuffManager.Update");
        }

        public IBuff CreateBuff(ISpineCreature owner, int buffId)
        {
            var cfg = GameGlobal.GameScheme.HeroTeamBuff_0(buffId);
            if (cfg == null)
            {
                Debug.LogError($"BuffId 不存在: {buffId}");
                return null;
            }
            else
            {
                return CreateBuff(owner, cfg);
            }
        }

        public IBuff CreateBuff(ISpineCreature owner, cfg_HeroTeamBuff cfg)
        {
            string buffCls = cfg.szBuffCls;
            Type type = Type.GetType(buffCls);
            if (type == null)
            {
                Debug.LogError($"没有找到Buff的实现: {buffCls}");
                return null;
            }

            if (!m_BuffPool.TryGetValue(type, out var pool))
            {
                pool = new Stack<IBuff>();
                m_BuffPool.Add(type, pool);
            }

            if (!IsCreationValid(owner, cfg))
            {
                return null;
            }

            IBuff buff = null;
            if (pool.Count > 0)
            {
                buff = pool.Pop();
            }
            else
            {
                object instance = Activator.CreateInstance(type);
                if (typeof(IBuff).IsAssignableFrom(type))
                {
                    buff = (IBuff)instance;
                }
                else
                {
                    Debug.LogError($"{buffCls} 没有扩展IBuff接口");
                }
            }

            if (buff != null)
            {
                buff.OnInit(owner, cfg);
                m_ActiveBuffs.Add(buff);
            }
            return buff;
        }

        private bool IsCreationValid(ISpineCreature owner, cfg_HeroTeamBuff cfg)
        {
            if (cfg.isStackable == 0)
            {
                var buffs = m_ActiveBuffs.FindAll(buff => buff.GetOwner() == owner);
                var buff = buffs.Find(buf => buf.GetCfg() == cfg);
                if (cfg.allowDurationReset == 1)
                {
                    buff?.RePlay();
                }
                return buff == null;
            }
            else
            {
                return true;
            }
        }

        public void ReleaseBuff(IBuff buff)
        {
            int index = m_ActiveBuffs.IndexOf(buff);
            if (index != -1)
            {
                int lastIndex = m_ActiveBuffs.Count - 1;
                m_ActiveBuffs[index] = m_ActiveBuffs[lastIndex];
                m_ActiveBuffs.RemoveAt(lastIndex);
                ReleaseBuffInternal(buff);
            }
        }

        private void ReleaseBuffInternal(IBuff buff)
        {
            var type = buff.GetType();
            if (!m_BuffPool.TryGetValue(type, out var pool))
            {
                pool = new Stack<IBuff>();
                m_BuffPool.Add(type, pool);
                Debug.LogWarning("推荐使用BuffManager.AddBuff来创建Buff, 不要再外部临时生成的buff");
            }

            if (!pool.Contains(buff))
            {
                buff.OnClear();
                pool.Push(buff);
            }
            else
            {
                Debug.Log($"跳过重复回收：{type}");
            }
        }

        public void OnFrameUpdate()
        {
            float now = Time.time;
            IBuff buff;
            for (int i = 0; i < m_ActiveBuffs.Count; i++)
            {
                buff = m_ActiveBuffs[i];
                if (buff.NextTime() <= now)
                {
                    buff.OnStep(now);
                    if (buff.IsDone())
                    {
                        int lastIndex = m_ActiveBuffs.Count - 1;
                        m_ActiveBuffs[i--] = m_ActiveBuffs[lastIndex];
                        m_ActiveBuffs.RemoveAt(lastIndex);
                        ReleaseBuffInternal(buff);
                    }
                }

            }
        }
    }

}