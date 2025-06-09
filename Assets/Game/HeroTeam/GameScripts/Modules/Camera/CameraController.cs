using DG.Tweening;
using UnityEngine;
using XClient.Common;
using XGame.EventEngine;

namespace GameScripts.HeroTeam
{
    


    public class CameraController : MonoSingleton<CameraController>, IEventExecuteSink
    {
        private Transform m_trCamera = null;
        private Vector3 m_vec3Original;
        private Tween m_tweenShake;
        private Camera m_MainCamrea;

        public void OnExecute(ushort wEventID, byte bSrcType, uint dwSrcID, object pContext)
        {

            if (wEventID == DHeroTeamEvent.EVENT_CAMERA_SHAKE)
            {
                // ��ֹ����𶯵���
                if (m_tweenShake != null && m_tweenShake.IsActive())
                    m_tweenShake.Kill();

                transform.localPosition = m_vec3Original;
                if (pContext is CameraShakeEventContext ctx)
                {
                    m_tweenShake = transform.DOShakePosition(
                        duration: ctx.duration,
                        strength: ctx.intensity,
                        vibrato: ctx.vibrato,
                        randomness: ctx.randomness,
                        snapping: false,
                        fadeOut: ctx.fadeOut
                    ).OnComplete(() =>
                    {
                        transform.localPosition = m_vec3Original;
                    });
                }
            }
            else if (wEventID == DHeroTeamEvent.EVENT_START_GAME)
            {
                m_trCamera.DOMoveY(15f, 2f).OnComplete(() =>
                {
                    m_vec3Original = transform.localPosition;
                });
            }
            else if (wEventID == DHeroTeamEvent.EVENT_WIN)
            {
                var cam = m_trCamera.GetComponent<Camera>();
                cam.DOOrthoSize(21f, 1.0f);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            m_trCamera = transform;
            m_vec3Original = transform.localPosition;

            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_CAMERA_SHAKE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "CameraController:Start");
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_START_GAME, DEventSourceType.SOURCE_TYPE_UI, 0, "CameraController:Start");
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_WIN, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "CameraController:Start");

            m_MainCamrea = Camera.main;
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_CAMERA_SHAKE, DEventSourceType.SOURCE_TYPE_ENTITY, 0);
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_START_GAME, DEventSourceType.SOURCE_TYPE_UI, 0);
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_WIN, DEventSourceType.SOURCE_TYPE_ENTITY, 0);
        }


        public Rect GetCamViewBounds()
        {
            var cam = m_MainCamrea;
            float vert_half = cam.orthographicSize;
            float horz_half = vert_half * cam.aspect;

            Vector3 cam_pos = cam.transform.position;

            float left = cam_pos.x - horz_half;
            float right = cam_pos.x + horz_half;
            float bottom = cam_pos.y - vert_half;
            float top = cam_pos.y + vert_half;

            return new Rect(left, bottom, right - left, top - bottom);
        }

    }
}