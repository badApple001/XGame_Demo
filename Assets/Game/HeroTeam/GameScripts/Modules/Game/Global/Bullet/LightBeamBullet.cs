using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameScripts.HeroTeam
{

    /// <summary>
    /// 光束是持续性的伤害/或者治疗， 每秒处理一次
    /// </summary>
    public class LightBeamBullet : Bullet
    {
        private GameObject HitEffect;
        private ParticleSystem[] Effects;
        private ParticleSystem[] Hit;
        private LineRenderer Laser;
        private Vector4 Length = new Vector4(1, 1, 1, 1);
        private float HitOffset = 0.01f;
        private bool useLaserRotation = false;
        private float MainTextureLength = 0.5f;
        private float NoiseTextureLength = 0.5f;
        private float timePreSeconds = 0;
        protected Transform m_Shooter = null;

        public override void Init(GameObject objRef)
        {
            base.Init(objRef);

            HitEffect = transform.Find("Hit").gameObject;
            Laser = transform.GetComponent<LineRenderer>();
            Effects = transform.GetComponentsInChildren<ParticleSystem>();
            Hit = HitEffect.GetComponentsInChildren<ParticleSystem>();
        }


        public override void Active(Vector3 newPos)
        {
            base.Active(newPos);
            timePreSeconds = 1f;
            Laser.enabled = true;
            foreach (var AllPs in Effects)
            {
                if (AllPs.isPlaying) AllPs.Play();
            }
        }

        public virtual void SetShooter(Transform shooter)
        {
            m_Shooter = shooter;
        }

        protected override void OnExpired()
        {
            m_Shooter = null;

            if (Laser != null)
            {
                Laser.enabled = false;
            }

            if (Effects != null)
            {
                foreach (var AllPs in Effects)
                {
                    if (!AllPs.isPlaying) AllPs.Play();
                }
            }
        }

        public override void Fly()
        {
            if (targetEntity == null) return;

            maxLifeTime -= TimeUtils.DeltaTime;
            timePreSeconds += TimeUtils.DeltaTime;

            if (targetEntity.GetState() >= ActorState.Dying)
            {
                isExpired = true;
                return;
            }

            Laser.material.SetTextureScale("_MainTex", new Vector2(Length[0], Length[1]));
            Laser.material.SetTextureScale("_Noise", new Vector2(Length[2], Length[3]));
            if (Laser != null && target != null)
            {

                if (timePreSeconds >= 1f)
                {
                    timePreSeconds -= 1f;
                    targetEntity.SetHPDelta(-harm);
                }

                if (m_Shooter != null) transform.position = m_Shooter.position;

                var targetPos = target.position;
                var hitNormal = ( targetPos - transform.position ).normalized;
                Laser.SetPosition(0, transform.position);
                Laser.SetPosition(1, targetPos);

                HitEffect.transform.position = targetPos + hitNormal * HitOffset;
                if (useLaserRotation)
                    HitEffect.transform.rotation = transform.rotation;
                else
                    HitEffect.transform.LookAt(targetPos + hitNormal);


                Length[0] = MainTextureLength * (Vector3.Distance(transform.position, targetPos));
                Length[2] = NoiseTextureLength * (Vector3.Distance(transform.position, targetPos));
            }


            if (maxLifeTime <= 0f)
            {
                isExpired = true;
                OnExpired();
                return;
            }
        }

    }

}