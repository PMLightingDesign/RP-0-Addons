using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;


namespace StretchyParts
{
    public class StretchyTank : StretchyPart
    {
        //This will be calulculated on the fly
        [KSPField(isPersistant = true)]
        public float totalVolume = 1.0f;

        //Leave at -1 for symetrical tanks, or specify
        [KSPField(isPersistant = true)]
        public float volumeBottomCap = -1.0f;

        //Leave at -1 for symetrical tanks, or specify
        [KSPField(isPersistant = true)]
        public float volumeTopCap = -1.0f;

        public override void scaleTank(float Scale)
        {
            base.scaleTank(Scale);

            float radius = diameter / 2.0f;
            float cylinderVolume = (float)Math.PI * (float)Math.Pow((double)radius, 2.0) * Scale;

            if (volumeTopCap == -1.0f && volumeBottomCap == -1.0f)
            {
                
                float capVolume = ((0.25f * diameter) * radius * radius * (float)Math.PI * (4 / 3));
                totalVolume = (capVolume + cylinderVolume);
            } else
            {
                totalVolume = (volumeTopCap + volumeBottomCap + cylinderVolume);
            }

            
            

            var data = new BaseEventDetails(BaseEventDetails.Sender.USER);
            data.Set<string>("volName", "Tankage");
            data.Set<double>("newTotalVolume", totalVolume);
            part.SendEvent("OnPartVolumeChanged", data, 0);
        }

        public void ChangeVolume(string volName, double newVolume)
        {
            var data = new BaseEventDetails(BaseEventDetails.Sender.USER);
            data.Set<string>("volName", "Tankage");
            data.Set<double>("newTotalVolume", newVolume);
            part.SendEvent("OnPartVolumeChanged", data, 0);
        }

    }
}

