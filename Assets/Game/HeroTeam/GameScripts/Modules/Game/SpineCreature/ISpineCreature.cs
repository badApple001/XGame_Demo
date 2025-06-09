using System.Collections.Generic;
using Spine.Unity;
using UniFramework.Machine;
using UnityEngine;

namespace GameScripts.HeroTeam
{
    public interface ISpineCreature : IActor
    {

        /// <summary>
        /// 角色的职业
        /// </summary>
        /// <returns></returns>
        int GetHeroCls();


        /// <summary>
        /// 获取脊骨动画组件
        /// </summary>
        /// <returns></returns>
        SkeletonAnimation GetSkeleton();

        StateMachine GetStateMachine();


        /// <summary>
        /// 仇恨值
        /// </summary>
        int GetHatred();

        /// <summary>
        /// 仇恨值
        /// </summary>
        /// <param name="value"></param>
        void SetHatred(int value);


        /// <summary>
        /// 获取技能列表
        /// </summary>
        /// <returns></returns>
        List<cfg_HeroTeamSkills> GetSkills();


        cfg_HeroTeamCreature GetCreatureCig();


        cfg_ActorAnimConfig GetAnimConfig();


        /// <summary>
        /// 记录伤害/治疗
        /// </summary>
        /// <param name="addHarm"></param>
        void RecordHarm(int addHarm);

        /// <summary>
        /// 获取累加的伤害/治疗
        /// </summary>
        /// <returns></returns>
        int GetTotalHarm();


        long GetHP();


        long GetMaxHP();


        int GetPower();

        void SetPower(int v);

        void SetHPDelta(int hp);

        void SetMaxHP(long maxHp);

        int GetCamp();

        Transform GetFaceTr();

        Transform GetVisual();

        float GetSpeed();

        void SetSpeed(float speed);

        bool IsDie();

        Vector3 GetForward();
        
        void SetForward(Vector3 forward);
    }
}