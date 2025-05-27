/*******************************************************************
** �ļ���:	IMonster.cs
** ��  Ȩ:	(C) ��������
** ������:	���¼�
** ��  ��:	2024.6.25
** ��  ��:	1.0
** ��  ��:	
** Ӧ  ��:  ����ʵ��ӿ�

**************************** �޸ļ�¼ ******************************
** �޸���: 
** ��  ��: 
** ��  ��: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGame.Entity;

namespace XClient.Entity
{

    public enum EMonsterAni
    {
        Idle,
        BattleIdle,
        Move,
        Dead,
        Type2SkillAct,
        Attack,
        StartBattle,
        ExitBattle,
        Dodge,
    }

    public class MonsterAniHelper
    {
        public static string GetAniName( EMonsterAni aniType )
        {
            switch ( aniType )
            {
                case EMonsterAni.Idle:
                    return "idle";
                case EMonsterAni.BattleIdle:
                    return "idle_atk";
                case EMonsterAni.StartBattle:
                    return "begin";
                case EMonsterAni.Move:
                    return "move";
                case EMonsterAni.Dead:
                    return "die";
                case EMonsterAni.ExitBattle:
                    return "fall";
                case EMonsterAni.Attack:
                    return "atk";
                case EMonsterAni.Type2SkillAct:
                    return "shifa";
                case EMonsterAni.Dodge:
                    return "shanbi";
                default:
                    return "idle";
            }
        }
    }

    public interface IMonster : ICreatureEntity
    {
        void SetRoad(List<Vector3> road);
        List<Vector3> GetRoad();

        void SetBoos();
        bool IsBoos();

        int GetHatred();
        void SetHatred(int value);

        Transform GetTr();
    }

}


