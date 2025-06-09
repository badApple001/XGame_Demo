using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
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
        public List<ISpineCreature> arrHarmRank = new List<ISpineCreature>();
        public List<ISpineCreature> arrHateRank = new List<ISpineCreature>();
        public List<ISpineCreature> arrCuringRank = new List<ISpineCreature>();

    }

    /// <summary>
    /// 伤害排序，伤害降序
    /// </summary>
    partial class MonsterHarmComparer : IComparer<ISpineCreature>
    {
        public int Compare(ISpineCreature x, ISpineCreature y)
        {
            int xHarm = ((x.GetHeroCls() == HeroClassDef.SAGE) ? 0 : 1) * x.GetTotalHarm();
            int yHarm = ((y.GetHeroCls() == HeroClassDef.SAGE) ? 0 : 1) * y.GetTotalHarm();
            return yHarm.CompareTo(xHarm);
        }
    }

    /// <summary>
    /// 仇恨排序
    /// </summary>
    partial class MonsterHateComparer : IComparer<ISpineCreature>
    {
        public int Compare(ISpineCreature x, ISpineCreature y)
        {
            return y.GetHatred().CompareTo(x.GetHatred());
        }
    }

    /// <summary>
    /// 治疗排序
    /// </summary>
    partial class MonsterCuringComparer : IComparer<ISpineCreature>
    {
        public int Compare(ISpineCreature x, ISpineCreature y)
        {
            int xHarm = (x.GetHeroCls() == HeroClassDef.SAGE ? 1 : 0) * x.GetTotalHarm();
            int yHarm = (y.GetHeroCls() == HeroClassDef.SAGE ? 1 : 0) * y.GetTotalHarm();

            return yHarm.CompareTo(xHarm);
        }
    }



    public class RankPipe : Singleton<RankPipe>, ISpineCreatureCampUpdateProcessPipe
    {
        // <summary>
        /// 记录角色伤害变化
        /// </summary>
        private Dictionary<ulong, int> m_dictActorRankInfos = new Dictionary<ulong, int>();

        private MonsterHarmComparer m_monsterHarmComparer = new MonsterHarmComparer();
        private MonsterCuringComparer m_monsterCuringComparer = new MonsterCuringComparer();
        private MonsterHateComparer m_monsterHateComparer = new MonsterHateComparer();


        public void Update(int camp, List<ISpineCreature> actors)
        {

            bool needRankSorted = false;
            bool canRankSorted = Time.frameCount % 120 == 0;


            foreach (var actor in actors)
            {
                if (!m_dictActorRankInfos.TryGetValue(actor.id, out var harm))
                {
                    m_dictActorRankInfos.Add(actor.id, actor.GetTotalHarm());
                }

                if (harm != actor.GetTotalHarm())
                {
                    harm = actor.GetTotalHarm();
                    needRankSorted = true;
                }
            }

            if (needRankSorted && canRankSorted)
            {
                var pContext = RefreshRankContext.Ins;
                var arrHarmRank = pContext.arrHarmRank;
                var arrCuringRank = pContext.arrCuringRank;
                var arrHateRank = pContext.arrHateRank;

                arrCuringRank.Clear();
                arrCuringRank.AddRange(actors);
                arrCuringRank.Sort(m_monsterCuringComparer);

                arrHarmRank.Clear();
                arrHarmRank.AddRange(actors);
                arrHarmRank.Sort(m_monsterHarmComparer);

                arrHateRank.Clear();
                arrHateRank.AddRange(actors);
                arrHateRank.Sort(m_monsterHateComparer);
                GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_REFRESH_RANKDATA, GameScripts.HeroTeam.DEventSourceType.SOURCE_TYPE_MONSTERSYSTEAM, 0, pContext);
            }

        }


    }

}