using System;
using System.Collections.Generic;
using DG.Tweening;
using UniFramework.Machine;
using UnityEngine;

namespace GameScripts.HeroTeam
{
    public class WeightedAction
    {
        public Action task;
        public float BaseWeight;
        public float CurrentWeight;

        public WeightedAction(Action task, float baseWeight)
        {
            this.task = task;
            BaseWeight = baseWeight;
            CurrentWeight = baseWeight;
        }
    }


    public class ActionSelector
    {
        private List<WeightedAction> m_Actions = new List<WeightedAction>();
        private float m_WeightIncreaseStep = 1.0f;
        private System.Random m_Random = new System.Random();

        public void AddAction(Action task, float baseWeight)
        {
            m_Actions.Add(new WeightedAction(task, baseWeight));
        }

        public Action SelectAction()
        {
            float total = 0f;
            foreach (var action in m_Actions)
            {
                total += action.CurrentWeight;
            }

            float roll = (float)(m_Random.NextDouble() * total);
            float accum = 0f;

            foreach (var action in m_Actions)
            {
                accum += action.CurrentWeight;
                if (roll <= accum)
                {
                    // 命中该行为
                    ResetWeightsExcept(action);
                    return action.task;
                }
            }

            return null; // 理论不会到达这里
        }

        private void ResetWeightsExcept(WeightedAction selected)
        {
            foreach (var action in m_Actions)
            {
                if (action == selected)
                {
                    action.CurrentWeight = action.BaseWeight;
                }
                else
                {
                    action.CurrentWeight += m_WeightIncreaseStep;
                }
            }
        }
    }

    public class HeroRandomBehaviourState : SpineCreatureStateBase
    {

        private ActionSelector selector = new ActionSelector();

        private static readonly string[] emojis = new string[] {
            "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Joy/EmojiCool.prefab",
            "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Joy/EmojiCute.prefab",
            "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Joy/EmojiDerpGasp.prefab",
            "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Joy/EmojiDrool.prefab",
            "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Joy/EmojiLaughCry.prefab",
            "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Joy/EmojiStarstruck.prefab",
            "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Joy/EmojiXD.prefab",
            "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Disgust/EmojiSick.prefab",
            "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Disgust/EmojiSick.prefab",
            "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Anger/EmojiAngry.prefab",
        };

        private static readonly string[] chats = new string[] {
    "团长别划水了，我都看到你在原地站着！",
    "治疗你看我一眼好吗？我快没了！",
    "老板别拉两波怪啊，命没了！",
    "这波仇恨又是谁没控好？",
    "我怎么又是第一个倒的……",
    "MT别冲太快，我腿短追不上！",
    "躲技能啊喂！那圈是你的吗？",
    "说好5秒后开怪，你3秒就冲了！",
    "这伤害差点把我打回登录界面！",
    "D奶快醒醒，你在加NPC吗？",
    "团长你是不是又开错技能了？",
    "这波操作我给0分，怕你骄傲。",
    "不是我说，你那输出能吓退怪？",
    "又忘记吃药了吧老哥？",
    "治疗在发呆，我命在呼吸。",
    "这波拉得好，差点团灭。",
    "我以为你是战士，结果是演员。",
    "躲技能这种事，靠天赋不靠眼力吗？",
    "拉住仇恨就像拉住爱情，全靠运气。",
    "团长喊集合，结果集合在地板上。",
    "谁又踩炸了！站撸就站撸别炸队友！",
    "我感觉我是来陪练的，不是来打本的。",
    "说好不掉线的，谁又蒸发了？",
    "你这闪避是写代码的闪避吗？",
    "奶妈：对不起我没蓝了……（常驻弹窗）",
    "开怪没倒计时，是为了惊喜？",
    "我感觉我在看一场喜剧。",
    "要不咱别打了，直接重开吧？",
    "怪：你们别吵了，我自己走。",
    "好了好了，这波锅归我，满意了吗？"
};

        public override void OnCreate(StateMachine machine)
        {
            base.OnCreate(machine);

            selector.AddAction(Task_Jump, 100);
            selector.AddAction(Task_Emoji, 100);
            selector.AddAction(Task_ChatMessage, 100);
        }
        public override void OnEnter()
        {
            base.OnEnter();
            try
            {
                selector.SelectAction()?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Back2IdleState();
            }
        }
        private void Back2IdleState() => m_StateMachine.ChangeState<HeroIdleState>();
        private void Task_Jump()
        {
            m_Anim.state.SetAnimation(0, m_ActorAnimConfig.szJump1, false);
            m_Owner.GetTr().DOScale(1.2f, 0.2f).OnComplete(() => m_Owner.GetTr().DOScale(1f, 0.3f));
            m_Owner.GetVisual().DOLocalJump(m_Owner.GetVisual().localPosition, 7f, 1, 0.5f).OnComplete(() =>
            {
                Back2IdleState();
            }).SetAutoKill(true);
        }
        private void Task_Emoji()
        {
            AddTimer(1f, Back2IdleState);
            if (m_Owner is IHero hero) hero.ShowEmoji(emojis[UnityEngine.Random.Range(0, emojis.Length)]);
        }
        private void Task_ChatMessage()
        {
            AddTimer(1f, Back2IdleState);
            string chat = chats[UnityEngine.Random.Range(0, chats.Length)];
            BubbleMessageManager.Instance.Show(
                chat,
                m_Owner.GetVisual().position,
                Vector3.up * 1f);
        }
    }
}