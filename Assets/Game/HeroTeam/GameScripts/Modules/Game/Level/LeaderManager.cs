using System;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XGame;
using XGame.Timer;
using XGame.Utils;

namespace GameScripts.HeroTeam
{

    public class LeaderSkillProperty
    {
        public Action SkillFoo;
        public int MpCast;
        public int CD;
        public float NextTime;
        public int UseCount = 0;
        public bool enoughMp = false;
        public bool EnoughCondition = false;

        private Action OnActive, OnInactive;

        private ILeaderManager leaderManager;

        public LeaderSkillProperty(int mapCast, int cd, ILeaderManager leaderManager)
        {
            MpCast = mapCast;
            CD = cd;
            this.leaderManager = leaderManager;
        }

        public void RefreshNextTime() => NextTime = TimeUtils.CurrentTime + CD;

        public bool HasCD() => TimeUtils.CurrentTime >= NextTime;

        public void Invoke()
        {
            RefreshNextTime();
            UseCount++;
            SkillFoo?.Invoke();
            leaderManager.DeductMp(MpCast);
            leaderManager.Refresh();
        }

        public void Reset()
        {

            // RefreshNextTime();
            NextTime = TimeUtils.CurrentTime;
        }

        public void Active() => OnActive?.Invoke();
        public void Inactive() => OnInactive?.Invoke();

        public void BindAction(Action activeCallback, Action inactiveCallback)
        {
            OnActive = activeCallback;
            OnInactive = inactiveCallback;
        }
    }

    public enum LeaderSkillType
    {
        Fire,
        Dodge,
        Purify
    }

    public interface ILeaderManager
    {
        void DeductMp(int cost);
        void Refresh();

        IHero GetLeader();
    }

    public class LeaderManager : Singleton<LeaderManager>, ITimerHandler, ILeaderManager
    {

        public List<LeaderSkillProperty> m_Skills { private set; get; } = new List<LeaderSkillProperty>();

        private int m_nMp;
        private int m_nMaxMp;
        private int m_nMpRecover;
        private int m_nBossSkillLockLeaderRate;
        private Action<int, int> OnMpChanged;
        private IHero m_Leader;
        public void Setup()
        {
            var cfg = GameGlobal.GameScheme.HeroTeamLeaderConfig(0);
            m_Skills.Add(new LeaderSkillProperty(
                cfg.iMpCost_Fire,
                cfg.iSkillCD_Fire,
                this
            ));

            m_Skills.Add(new LeaderSkillProperty(
                cfg.iMpCost_Dodge,
                cfg.iSkillCD_Dodge,
                this
            ));

            m_Skills.Add(new LeaderSkillProperty(
                cfg.iMpCost_Purify,
                cfg.iSkillCD_Purify,
                this
            ));

            Reset();
        }


        public void Reset()
        {

            var cfg = GameGlobal.GameScheme.HeroTeamLeaderConfig(0);
            m_nMp = m_nMaxMp = cfg.iDefaultMp;
            m_nMpRecover = cfg.iMpRecover;
            m_nBossSkillLockLeaderRate = cfg.iBossSkillLockLeaderRate;
            m_Skills.ForEach(skill => skill.Reset());


            var timerManager = XGameComs.Get<ITimerManager>();
            timerManager.AddTimer(this, this.GetType().GetHashCode(), 1000, "LeaderManager:Timer");

            Refresh();
        }


        public bool LockLeader()
        {
            if (m_Leader == null || m_Leader.GetTr() == null || m_Leader.GetState() > ActorState.Normal)
            {
                return false;
            }
            return UnityEngine.Random.value < (m_nBossSkillLockLeaderRate / 100f);
        }


        public void OnTimer(TimerInfo ti)
        {
            m_nMp = Mathf.Min(m_nMaxMp, m_nMp + m_nMpRecover);
            Refresh();
        }

        public LeaderSkillProperty GetLeaderSkillProperty(LeaderSkillType type)
        {
            return m_Skills[(int)type];
        }

        public cfg_HeroTeamLeaderConfig GetLeaderConfig() => GameGlobal.GameScheme.HeroTeamLeaderConfig(0);

        public void BindMpChangedEvent(Action<int, int> mpChangedCallback)
        {
            OnMpChanged += mpChangedCallback;
        }

        public void Refresh()
        {
            foreach (var skill in m_Skills)
            {
                skill.enoughMp = skill.MpCast <= m_nMp;
                bool enoughCondition = skill.enoughMp && skill.HasCD();
                if (enoughCondition != skill.EnoughCondition)
                {
                    if (enoughCondition)
                    {
                        skill.Active();
                    }
                    else
                    {
                        skill.Inactive();
                    }
                }
            }

            OnMpChanged?.Invoke(m_nMp, m_nMaxMp);
        }

        public void Release()
        {
            var timerManager = XGameComs.Get<ITimerManager>();
            timerManager.RemoveTimer(this);
        }

        public void DeductMp(int cost)
        {
            m_nMp -= cost;
        }

        public IHero GetLeader() => m_Leader;

        public void SetLeaderInstance(IHero leader) => m_Leader = leader;
    }

}