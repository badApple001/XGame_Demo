using System.Collections.Generic;
using UnityEngine;
using XGame.Utils;


namespace GameScripts.HeroTeam
{

    /// <summary>
    /// 刷新排行榜事件
    /// </summary>
    public class RefreshRankContext
    {
        private RefreshRankContext() { }
        public static RefreshRankContext Ins { private set; get; } = new RefreshRankContext();
        public List<IActor> arrHarmRank = new List<IActor>();
        public List<IActor> arrHateRank = new List<IActor>();
        public List<IActor> arrCuringRank = new List<IActor>();

    }

    /// <summary>
    /// 伤害排序，伤害降序
    /// </summary>
    partial class MonsterHarmComparer : IComparer<IActor>
    {
        public int Compare(IActor x, IActor y)
        {
            int xHarm = ((x.GetHeroCls() == HeroClassDef.SAGE) ? 0 : 1) * x.GetTotalHarm();
            int yHarm = ((y.GetHeroCls() == HeroClassDef.SAGE) ? 0 : 1) * y.GetTotalHarm();
            return yHarm.CompareTo(xHarm);
        }
    }

    /// <summary>
    /// 仇恨排序
    /// </summary>
    partial class MonsterHateComparer : IComparer<IActor>
    {
        public int Compare(IActor x, IActor y)
        {
            return y.GetHatred().CompareTo(x.GetHatred());
        }
    }

    /// <summary>
    /// 治疗排序
    /// </summary>
    partial class MonsterCuringComparer : IComparer<IActor>
    {
        public int Compare(IActor x, IActor y)
        {
            int xHarm = (x.GetHeroCls() == HeroClassDef.SAGE ? 1 : 0) * x.GetTotalHarm();
            int yHarm = (y.GetHeroCls() == HeroClassDef.SAGE ? 1 : 0) * y.GetTotalHarm();

            return yHarm.CompareTo(xHarm);
        }
    }



    public class RankPipe : Singleton<RankPipe>, IActorCampUpdateProcessPipe
    {
        // <summary>
        /// 记录角色伤害变化
        /// </summary>
        private Dictionary<ulong, int> m_dictActorRankInfos = new Dictionary<ulong, int>();

        
        public void Update(int camp, List<IActor> actors)
        {

            bool needRankSorted = false;
            bool canRankSorted = Time.frameCount % 120 == 0;


            // if (!m_dictHeroRankInfos.TryGetValue(actor.name, out var harm))
            // {
            //     m_dictHeroRankInfos.Add(actor.name, actor.GetTotalHarm());
            // }

            // if (harm != actor.GetTotalHarm())
            // {
            //     harm = actor.GetTotalHarm();
            //     needRankSorted = true;
            // }



            // if (needRankSorted && canRankSorted)
            // {
            //     m_arrMonsterCache.RemoveAll(t => t.IsBoos());

            //     var pContext = RefreshRankContext.Ins;
            //     var arrHarmRank = pContext.arrHarmRank;
            //     var arrCuringRank = pContext.arrCuringRank;
            //     var arrHateRank = pContext.arrHateRank;

            //     arrCuringRank.Clear();
            //     arrCuringRank.AddRange(m_arrMonsterCache);
            //     arrCuringRank.Sort(m_monsterCuringComparer);

            //     arrHarmRank.Clear();
            //     arrHarmRank.AddRange(m_arrMonsterCache);
            //     arrHarmRank.Sort(m_monsterHarmComparer);

            //     arrHateRank.Clear();
            //     arrHateRank.AddRange(m_arrMonsterCache);
            //     arrHateRank.Sort(m_monsterHateComparer);
            //     GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_REFRESH_RANKDATA, GameScripts.HeroTeam.DEventSourceType.SOURCE_TYPE_MONSTERSYSTEAM, 0, pContext);
            // }

        }


    }

}