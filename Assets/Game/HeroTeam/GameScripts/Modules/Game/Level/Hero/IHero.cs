
using System.Collections.Generic;
using UnityEngine;

namespace GameScripts.HeroTeam
{
    public interface IHero : ISpineCreature
    {
        /// <summary>
        /// 躲避Boss技能
        /// </summary>
        /// <param name="bossPos"></param>
        /// <param name="bossDir"></param>
        /// <param name="radius"></param>
        /// <param name="angleDeg"></param>
        bool EludeBossSkill(Vector3 bossPos, Vector3 bossDir, float radius, float angleDeg);

        void DodgeAndReturn();

        bool IsDodge();

        /// <summary>
        /// Boss点名
        /// </summary>
        /// <param name="bossPos"></param>
        void ReceiveBossSelect(Vector3 bossPos);

        void ShowEmoji(string emojiId, float showSeconds = 2);
    }
}