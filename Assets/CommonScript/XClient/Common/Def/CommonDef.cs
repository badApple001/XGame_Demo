
namespace GameScripts
{
    /// <summary>
    /// 数值物品ID（虚拟物品）
    /// </summary>
    public class NumbericGoodsID
    {
        //金币
        public readonly static int Gold = 1;

        //绑定钻石
        public readonly static int Diamond = 2;
    }

    /// <summary>
    /// 卡牌相关设置
    /// </summary>
    public class CardSettings
    {
        /// <summary>
        /// 能拥有的最大卡牌数量
        /// </summary>
        public const int OwnUsableCardMaxNum = 8;

        /// <summary>
        /// 能保存的最大技能卡数量
        /// </summary>
        public const int OwnUsableSkillCardMaxNum = 4;

    }

    public static class BattleSettings
    {
        /// <summary>
        /// 战斗英雄最大数量
        /// </summary>
        public const int BattleHeroMaxNum = 7;

        /// <summary>
        /// 技能卡的最大数量
        /// </summary>
        public const int BattleSkillCardMaxNum = 4;
    }


    public class CreatureAttributeDef
    {
        //BASE
        public readonly static int MAX_ID = 0;

        //攻击力
        public readonly static int ATTACK = MAX_ID++;
        //攻击速度
        public readonly static int ATTACK_SPEED = MAX_ID++;
        //生命值
        public readonly static int HP = MAX_ID++;
        //物防
        public readonly static int PHY_DEFENSE = MAX_ID++;
        //法防
        public readonly static int MAGIC_DEFENSE = MAX_ID++;
        //移动速度
        public readonly static int MOVE_SPEDD = MAX_ID++;

        //暴击概率
        public readonly static int POWER_ATTACK_RATE = MAX_ID++;

        //暴击伤系数
        public readonly static int POWER_ATTACK_COFF = MAX_ID++;

        //属性最大值
        public readonly static int MAX_ATTR = MAX_ID++;

    }

    //怪物定义
    public class MonsterTypeDef
    {
        //棋子英雄
        public readonly static int HERO = 0;

        //普通怪
        public readonly static int NORMAL = 1;

        //精英怪物
        public readonly static int ELITE = 2;

        //boss
        public readonly static int BOSS = 3;

        //城墙
        public readonly static int Wall = 4;

    }

    /// <summary>
    /// 飞行光效ID
    /// </summary>
    public class FlyEffectID
    {
        /// <summary>
        /// 技能卡槽飞行光效ID
        /// </summary>
        public static readonly int BattleSkillCardSlot0 = 1000;
        public static readonly int BattleSkillCardSlot1 = 1001;
        public static readonly int BattleSkillCardSlot2 = 1002;
        public static readonly int BattleSkillCardSlot3 = 1003;

        /// <summary>
        /// 英雄卡槽飞行光效ID
        /// </summary>
        public static readonly int BattleHeroCardSlot0 = 1100;
        public static readonly int BattleHeroCardSlot1 = 1101;
        public static readonly int BattleHeroCardSlot2 = 1102;
        public static readonly int BattleHeroCardSlot3 = 1103;
        public static readonly int BattleHeroCardSlot4 = 1104;
        public static readonly int BattleHeroCardSlot5 = 1105;
        public static readonly int BattleHeroCardSlot6 = 1106;

        /// <summary>
        /// 临时卡牌区
        /// </summary>
        public static readonly int TempCardSlot0 = 1200;
    }

    public static class AudioID
    {
        public static readonly int BattleWin = 2001;
        public static readonly int BattleFail = 2002;
        public static readonly int SellCard = 2003;
        public static readonly int BossComming = 2004;
        public static readonly int UseSkillCard = 3004;
    }

}
