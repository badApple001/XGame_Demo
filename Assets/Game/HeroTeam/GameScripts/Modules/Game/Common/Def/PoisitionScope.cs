using System;
using UnityEngine;

namespace GameScripts.HeroTeam
{

    [Serializable]
    public struct PoisitionScope
    {

        [SerializeField,InspectorName("开始角度")]
        public float forwardAngle;
        [SerializeField,InspectorName("范围")]
        public float rangeAngle;
        [SerializeField,InspectorName("半径")]
        public float radius;

        public PoisitionScope(float forwardAngle, float rangeAngle, float radius)
        {
            this.forwardAngle = forwardAngle;
            this.rangeAngle = rangeAngle;
            this.radius = radius;
        }


        public float this[int index]
        {
            get
            {
                return index switch
                {
                    0 => forwardAngle,
                    1 => rangeAngle,
                    2 => radius,
                    _ => throw new IndexOutOfRangeException("Invalid PoisitionScope index!"),
                };
            }
            set
            {
                switch (index)
                {
                    case 0:
                        forwardAngle = value;
                        break;
                    case 1:
                        rangeAngle = value;
                        break;
                    case 2:
                        radius = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid PoisitionScope index!");
                }
            }
        }

    }
}