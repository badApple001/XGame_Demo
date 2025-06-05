#if UNITY_EDITOR
namespace GameScripts.HeroTeam
{
    using UnityEngine;
    using XGame.Entity;

    [DisallowMultipleComponent]
    public class ActorPartInspector : MonoBehaviour
    {

        [HideInInspector]
        public BaseEntity Entity;

        public void BindEntity(BaseEntity entity) => Entity = entity;

    }

}
#endif