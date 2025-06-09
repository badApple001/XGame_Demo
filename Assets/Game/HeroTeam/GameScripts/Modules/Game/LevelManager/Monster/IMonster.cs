using UnityEngine;

namespace GameScripts.HeroTeam
{
    public interface IMonster : ISpineCreature
    {

        void SetBoos();

        bool IsBoos();


        /// <summary>
        /// 释放技能的节点
        /// </summary>
        /// <returns></returns>
        Transform GetSkillRoot();


        SpriteRenderer GetSkillTipRenderer();

    }

}