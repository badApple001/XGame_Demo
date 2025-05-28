using Spine.Unity;
using XClient.Entity;
using System.Collections.Generic;
using UnityEngine;

namespace GameScripts.HeroTeam
{
    public interface IActor
    {
        Transform GetTr();

        SkeletonAnimation GetSkeleton();

        cfg_ActorAnimConfig GetAnimConfig();

        void SetMonsterCfg(cfg_Monster cfg);

        cfg_Monster GetMonsterCfg();

        List<cfg_HeroTeamSkills> GetSkills();

        void SetCreatureEntity(ICreatureEntity entity);

        ICreatureEntity GetCreatureEntity();
    }
}