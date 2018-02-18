using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace StretchyParts
{
    public class StretchyPart : PartModule , IPartMassModifier, IPartCostModifier
    {

        //You MUST MUST MUST fill this out for non 1m tanks
        [KSPField(isPersistant = true)]
        public float diameter = 1.0f;

        //The maximum length, in meters, of the tank
        [KSPField(isPersistant = true)]
        public float maxLength = 16.0f;

        //The minimum length, in meters, of the tank
        [KSPField(isPersistant = true)]
        public float minLength = 0.25f;

        //The GUI tank length interface
        [KSPField(isPersistant = true, guiName = "Length", guiFormat = "F3", guiUnits = "m"),
         UI_FloatEdit(scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 20.0f, incrementLarge = 1.0f, incrementSmall = 0.25f, incrementSlide = 0.001f, sigFigs = 2, unit = "m")]
        public float tankLength = 1.0f;

        //The default tank length
        [KSPField(isPersistant = true)]
        private float setScale = 1.0f;

        //For special parts, you should not need to touch this
        [KSPField(isPersistant = true)]
        public float capOffset = 0.5f;

        //Where should the top node be in relation to the caps
        [KSPField(isPersistant = true)]
        public float nodeOffsetTop = 0.0f;

        //Where should the top node be in relation to the caps
        [KSPField(isPersistant = true)]
        public float nodeOffsetBottom = 0.0f;

        //How much does a 1m barrel section weigh?
        [KSPField(isPersistant = true)]
        public float massDelta = 1.0f;

        //How much does a 1m barrel section cost?
        [KSPField(isPersistant = true)]
        public float costDelta = 500.0f;

        //How much does the top cap weigh?
        [KSPField(isPersistant = true)]
        public float massTopCap = 1.0f;

        //How much does the bottom cap weigh?
        [KSPField(isPersistant = true)]
        public float massBottomCap = 1.0f;

        //What if the nodes are screwy?
        [KSPField(isPersistant = true)]
        public float nodeMultFactor = 1.0f;

        //Correct for Unity texture scale?
        [KSPField(isPersistant = true)]
        public float textureScaleMultiplier = 1.0f;

        //Should we tile the texture?
        [KSPField(isPersistant = true)]
        public bool tileTexture = true;

        private bool justSetup = false;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            UI_FloatEdit edit = (UI_FloatEdit)Fields["tankLength"].uiControlEditor;
            if (edit != null)
            {
                edit.minValue = minLength;
                edit.maxValue = maxLength;

                Debug.Log("Trying to set value limits");
            } else
            {
                Debug.Log("There's a problem with the config loader doing UI_FloatEdit");
            }

            scaleTank(tankLength);
        }

        protected virtual void Setup(bool isInitial)
        {
            if (part.partInfo == null)
            {
                return;
            }

            if (isInitial)
            {
                //Debug.Log("FASA Setup: length: " + setScale);
                scaleTank(setScale);
                tankLength = setScale;
            }
            else
            {
                scaleTank(tankLength);
                setScale = tankLength;
            }
            justSetup = isInitial;

        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
        }

        public bool isCurrent()
        {
            if (tankLength == setScale)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual void scaleTank(float Scale)
        {
            //Debug.Log("Scale is: " + Scale + " and maxLength is: " + maxLength);

            if (Scale > maxLength)
            {
                Scale = maxLength * 1.0f;
                tankLength = maxLength * 1.0f;

                
            }

            if(Scale < minLength)
            {
                Scale = minLength;
                tankLength = minLength;
            }

            Transform body = part.FindModelTransform("MiddlePart");
            if (body != null)
            {
                if (tileTexture)
                {
                    Renderer[] renderers = body.GetComponentsInChildren<Renderer>();
                    foreach (Renderer r in renderers)
                    {

                        r.material.mainTextureScale = new Vector2(1.0f, (Scale * textureScaleMultiplier));
                        r.material.SetTextureScale("_BumpMap", new Vector2(1.0f, (Scale * textureScaleMultiplier)));

                    }
                }

                body.localScale = new Vector3(diameter, Scale, diameter);

            }
            else Debug.Log("No tank body found!");

            Transform upCap = part.FindModelTransform("TopPart");
            if (upCap != null)
            {
                float delta = (Scale / 2) - capOffset;
                upCap.localPosition = new Vector3(0f, delta, 0f);
            }
            else Debug.Log("No top cap found!");

            Transform dnCap = part.FindModelTransform("BottomPart");
            if (dnCap != null)
            {
                float delta = -(Scale / 2) + capOffset;
                dnCap.localPosition = new Vector3(0f, delta, 0f);
            }
            else Debug.Log("No bottom cap found!");

            //Debug.Log("Setting length to: " + Scale);

            AttachNode topNode = part.FindAttachNode("top");
            NodeUtil.updateAttachNodePosition(part, topNode, new Vector3(0f, (nodeOffsetTop + (Scale / 2f) * nodeMultFactor), 0f), topNode.orientation, true);
            //Debug.Log("Top node result: " + ((nodeOffsetBottom + (Scale / 2f))));

            AttachNode bottomNode = part.FindAttachNode("bottom");
            NodeUtil.updateAttachNodePosition(part, bottomNode, new Vector3(0f, -(nodeOffsetBottom + (Scale / 2f) * nodeMultFactor), 0f), bottomNode.orientation, true);
           //Debug.Log("Bottom node result: " + (-(nodeOffsetBottom + (Scale / 2f))));
        }

        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
        {
            //Debug.Log("Cost modifier..." + (tankLength * costDelta));
            return tankLength * costDelta;
        }

        public ModifierChangeWhen GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }

        public float GetModuleMass(float defaultMass, ModifierStagingSituation sit)
        {
            return (massTopCap + massBottomCap) + tankLength * massDelta;
        }

        public ModifierChangeWhen GetModuleMassChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }

        public void Update()
        {
            //Debug.Log("tanklength: " + tankLength + " | previousScale: " + previousScale);

            if (!isCurrent())
            {
                Setup(false);
            }

            else if (justSetup)
            {
                Setup(false);
            }

        }

    }
}