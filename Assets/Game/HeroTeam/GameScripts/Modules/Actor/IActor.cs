using Spine.Unity;
using XClient.Entity;
using System.Collections.Generic;

namespace GameScripts.HeroTeam
{
    interface IActor
    {
        SkeletonAnimation GetSkeleton();

        cfg_ActorAnimConfig GetAnimConfig();

        void SetMonsterCfg(cfg_Monster cfg);

        cfg_Monster GetMonsterCfg();

        List<cfg_HeroTeamSkills> GetSkills();

        void SetCreatureEntity(ICreatureEntity entity);

        ICreatureEntity GetCreatureEntity();
    }
}