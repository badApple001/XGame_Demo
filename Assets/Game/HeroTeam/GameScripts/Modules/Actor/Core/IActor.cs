using Spine.Unity;
using XClient.Entity;
using System.Collections.Generic;
using UnityEngine;
using XGame.Entity;

namespace GameScripts.HeroTeam
{
    public interface IActor : IVisibleEntity
    {
        Transform GetTr();

        Transform GetVisual();

        SkeletonAnimation GetSkeleton();

        cfg_ActorAnimConfig GetAnimConfig();

        cfg_Actor GetConfig();

        List<cfg_HeroTeamSkills> GetSkills();

        /// <summary>
        /// 仇恨值
        /// </summary>
        int GetHatred();

        /// <summary>
        /// 仇恨值
        /// </summary>
        /// <param name="value"></param>
        void SetHatred(int value);

        Transform GetLockTr();

        Transform GetFaceTr();

        /// <summary>
        /// 躲避Boss技能
        /// </summary>
        /// <param name="bossPos"></param>
        /// <param name="bossDir"></param>
        /// <param name="radius"></param>
        /// <param name="angleDeg"></param>
        void EludeBossSkill(Vector3 bossPos, Vector3 bossDir, float radius, float angleDeg);

        /// <summary>
        /// Boss点名
        /// </summary>
        /// <param name="bossPos"></param>
        void ReceiveBossSelect(Vector3 bossPos);

        /// <summary>
        /// 角色的职业
        /// </summary>
        /// <returns></returns>
        int GetHeroCls();

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

        //降低耦合，buff 可以给任何角色用， 而不是仅服务于Actor
        // List<GameScripts.HeroTeam.IBuff> GetBuffs();
        // void AddBuff(int buffId);
        // void RemoveBuff(GameScripts.HeroTeam.IBuff buff);

        long GetHP();

        long GetMaxHP();

        Vector3 GetPos();

        void SetPos(Vector3 pos);

        void SetPos(float[] float3Pos);

        float GetSpeed();

        void SetSpeed(float speed);


        int GetPower();

        void SetPower(int v);

        bool IsDie();

        void GoDead();

        void SetHPDelta(int hp);

        void SetMaxHP(long maxHp);

        void SetRotation(Quaternion rotate);

        Quaternion GetRotation();


        void SetIntAttr(int key, int value);

        int GetIntAttr(int key);

        void SetBoos();

        bool IsBoos();

        int GetCamp();

        //临时
        void SetRoad(List<Vector3> road);

        List<Vector3> GetRoad();

    }
}
