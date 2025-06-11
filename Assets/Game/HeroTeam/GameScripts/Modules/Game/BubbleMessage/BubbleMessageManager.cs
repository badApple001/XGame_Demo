using UnityEngine;
using UnityEngine.UI;
using XGame;
using XGame.Asset;
using XGame.Utils;

namespace GameScripts.HeroTeam
{

    // public partial class EntityType
    // {
    //     public readonly static int BubbleMessage = EntityTypeBase++;
    // }

    public class BubbleMessageManager : Singleton<BubbleMessageManager>
    {
        private const string m_szPrefabPath = "Game/HeroTeam/GameResources/Prefabs/UI/BubbleMessage.prefab";
        private RectTransform m_rectRoot;
        private UnityEngine.Pool.ObjectPool<BubbleMessage> m_pool;
        public Camera uiCamera { private set; get; }


        public void Setup(RectTransform root)
        {
            m_rectRoot = root;

            // GameGlobal.EntityWorld.RegisterEntityType<BubbleMessage>(EntityType.BubbleMessage);
            // m_pool = new TransformPool()

            var rootCanvas = m_rectRoot.GetComponent<Canvas>();
            uiCamera = rootCanvas.worldCamera;

            var resLoader = XGameComs.Get<IGAssetLoader>();
            uint handle = 0;
            var pref = (GameObject)resLoader.LoadResSync<GameObject>(m_szPrefabPath, out handle);
            m_pool = new UnityEngine.Pool.ObjectPool<BubbleMessage>(() =>
            {
                var creat = new BubbleMessage();
                creat.Init(GameObject.Instantiate(pref));
                creat.transform.SetParent(m_rectRoot, false);
                creat.Reset();
                return creat;
            }, get_bubble => get_bubble.Reset(), release_bubble => release_bubble.Reset(), destory_bubble => GameObject.Destroy(destory_bubble.transform.gameObject), true, 1);
        }


        public void Show(string msg, Vector3 worldpos, Vector3 screenDelta)
        {
            var screenPos = Camera.main.WorldToScreenPoint(worldpos);
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_rectRoot, screenPos, uiCamera, out var wp))
            {
                var bubble = m_pool.Get();
                bubble.transform.position = wp + screenDelta;
                bubble.Show(msg);
                GameManager.Instance.AddTimer(2f, () => m_pool.Release(bubble));
            }
        }
    }


    public class BubbleMessage
    {
        public Transform transform { get; private set; }
        public CanvasGroup canvasGroup { get; private set; }
        public Animation animation { get; private set; }

        public Text content { get; private set; }

        public void Init(GameObject refObj)
        {
            transform = refObj.transform;
            animation = transform.GetComponent<Animation>();
            canvasGroup = transform.GetComponent<CanvasGroup>();
            content = transform.GetComponentInChildren<Text>();


            //直接禁用掉。
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public void Reset()
        {
            animation.Stop();
            canvasGroup.alpha = 0f;
        }

        public void Show(string msg)
        {
            animation["BubbleMessage_Show"].time = 0f; // 进度归零
            animation["BubbleMessage_Show"].enabled = true;
            animation.Play("BubbleMessage_Show");
            content.text = msg;
        }

    }

}