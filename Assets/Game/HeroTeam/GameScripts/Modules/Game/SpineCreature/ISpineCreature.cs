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
        /// 从CD了的技能中抽选一个
        /// </summary>
        /// <returns></returns>
        cfg_HeroTeamSkills RandomSelectSkill();

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

        /// <summary>
        /// 阵营
        /// </summary>
        /// <returns></returns>
        int GetCamp();


        /// <summary>
        /// 脸部表情节点
        /// </summary>
        /// <returns></returns>
        Transform GetFaceTr();

        /// <summary>
        /// 显示节点
        /// </summary>
        /// <returns></returns>
        Transform GetVisual();

        /// <summary>
        /// 锁定节点
        /// </summary>
        /// <returns></returns>
        Transform GetLockTr();

        /// <summary>
        /// 对话节点
        /// </summary>
        /// <returns></returns>
        Transform GetChatPoint();

        float GetSpeed();

        void SetSpeed(float speed);

        bool IsDie();

        Vector3 GetForward();

        void SetForward(Vector3 forward);

        float GetATKInterval();
    }
}