using Spine.Unity;
using Spine;
using UnityEngine;
using System.Collections.Generic;
using XGame.Poolable;

namespace XGame.Entity.Part
{
    public class SpineMixSkinAnimation : ISpineSkin
    {
        // private Skin resultCombinedSkin = SpineObjectPoolFacade.Instance().Pop<Skin>(); //new Skin("character-combined");
        //private Skin characterSkin;
        private SkeletonAnimation skeletonAnimation;

        public void Init(object context = null)
        {
            skeletonAnimation = context as SkeletonAnimation;
            //characterSkin = new Skin("character-base");
            applied = true;
        }
        public void Equip(EquipAsset asset)
        {
            if (applied)
            {
                // resultCombinedSkin.Clear(false);
                applied = false;
            }
            var skeleton = skeletonAnimation.Skeleton;
            var skeletonData = skeleton.Data;
            Skin skin = skeletonData.FindSkin(asset.skin);
            if (skin != null)
            {
                // resultCombinedSkin.AddSkin(skin);
            }
            else
            {
                Debug.LogError($" ��λ{asset.partType} Ƥ��{asset.skin}�Ҳ���");
            }
        }
        private bool applied = false;
        public void Apply()
        {
            var skeleton = skeletonAnimation.Skeleton;
            //todo ����new
            //var resultCombinedSkin = new Skin("character-combined");
            //resultCombinedSkin.Clear(false);
            //resultCombinedSkin.AddSkin(characterSkin);
            //AddEquipmentSkinsTo(resultCombinedSkin);

            // skeleton.SetSkin(resultCombinedSkin);
            skeleton.SetSlotsToSetupPose();
            applied = true;
        }

        public void Reset()
        {
            // resultCombinedSkin.Clear(false);
            skeletonAnimation = null;
            applied = true;
        }

        public bool Create()
        {
            return true;
        }

        public void Release()
        {
            Reset();
            // SpineObjectPoolFacade.Instance().Push(resultCombinedSkin);
            // resultCombinedSkin = null;
        }
    }
}