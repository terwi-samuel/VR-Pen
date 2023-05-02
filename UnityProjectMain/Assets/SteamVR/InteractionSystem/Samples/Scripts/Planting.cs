//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace Valve.VR.InteractionSystem.Sample
{
    public class Planting : MonoBehaviour
    {
        public SteamVR_Action_Boolean upAction;
        public SteamVR_Action_Boolean leftAction;
        public SteamVR_Action_Boolean rightAction;
        public SteamVR_Action_Boolean downAction;
        public SteamVR_Action_Boolean selectAction;

        public SteamVR_Action_Boolean[] actions = new SteamVR_Action_Boolean[5];

        public Hand hand;

        public GameObject prefabToPlant;

        public string text = "";


        private void OnEnable()
        {
            if (hand == null)
                hand = this.GetComponent<Hand>();

            upAction.AddOnChangeListener(UpActionChange, hand.handType);
            rightAction.AddOnChangeListener(RightActionChange, hand.handType);
            leftAction.AddOnChangeListener(LeftActionChange, hand.handType);
            downAction.AddOnChangeListener(DownActionChange, hand.handType);
            selectAction.AddOnChangeListener(SelectActionChange, hand.handType);
        }

        private void OnDisable()
        {
            upAction.RemoveOnChangeListener(UpActionChange, hand.handType);
            rightAction.RemoveOnChangeListener(RightActionChange, hand.handType);
            leftAction.RemoveOnChangeListener(LeftActionChange, hand.handType);
            downAction.RemoveOnChangeListener(DownActionChange, hand.handType);
            selectAction.RemoveOnChangeListener(SelectActionChange, hand.handType);
        }

        private void UpActionChange(SteamVR_Action_Boolean actionIn, SteamVR_Input_Sources inputSource, bool newValue)
        {
            if (newValue)
                text = "Up is pressed";
            else
                text = "Up is not pressed";
        }

        private void LeftActionChange(SteamVR_Action_Boolean actionIn, SteamVR_Input_Sources inputSource, bool newValue)
        {
            if (newValue)
                text = "Left is pressed";
            else
                text = "Left is not pressed";
        }

        private void RightActionChange(SteamVR_Action_Boolean actionIn, SteamVR_Input_Sources inputSource, bool newValue)
        {
            if (newValue)
                text = "Right is pressed";
            else
                text = "Right is not pressed";
        }

        private void DownActionChange(SteamVR_Action_Boolean actionIn, SteamVR_Input_Sources inputSource, bool newValue)
        {
            if (newValue)
                text = "Down is pressed";
            else
                text = "Down is not pressed";
        }

        private void SelectActionChange(SteamVR_Action_Boolean actionIn, SteamVR_Input_Sources inputSource, bool newValue)
        {
            if (newValue)
                text = "Select is pressed";
            else
                text = "Select is not pressed";
        }
    }
}