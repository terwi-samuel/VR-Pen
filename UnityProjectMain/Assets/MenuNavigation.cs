//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Valve.VR.InteractionSystem;

namespace Valve.VR.InteractionSystem.Sample
{
    public class MenuNavigation : MonoBehaviour
    {
        public SteamVR_Action_Boolean upAction;
        public SteamVR_Action_Boolean leftAction;
        public SteamVR_Action_Boolean rightAction;
        public SteamVR_Action_Boolean downAction;
        public SteamVR_Action_Boolean selectAction;

        public SteamVR_Action_Boolean[] actions = new SteamVR_Action_Boolean[5];

        public Hand hand;

        public string text = "";

        public Button texture1btn;
        public Button texture2btn;
        public Button texture3btn;

        public Button color1btn;
        public Button color2btn;
        public Button color3btn;

        public Button size1btn;
        public Button size2btn;
        public Button size3btn;

        private Button[,] buttons = new Button[3, 3];
        private int selectedRow = 0;
        private int selectedCol = 0;

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

        private void Start()
        {
            buttons[0, 0] = texture1btn;
            buttons[0, 1] = texture2btn;
            buttons[0, 2] = texture3btn;

            buttons[1, 0] = color1btn;
            buttons[1, 1] = color2btn;
            buttons[1, 2] = color3btn;

            buttons[2, 0] = size1btn;
            buttons[2, 1] = size2btn;
            buttons[2, 2] = size3btn;
        }

        private void Update()
        {
            foreach(Button btn in buttons)
            {
                btn.GetComponent<Image>().color = Color.white;
            }
            buttons[selectedRow, selectedCol].GetComponent<Image>().color = Color.red;
        }

        private void UpActionChange(SteamVR_Action_Boolean actionIn, SteamVR_Input_Sources inputSource, bool newValue)
        {
            if (newValue)
                if (selectedRow > 0)
                    selectedRow--;
        }

        private void LeftActionChange(SteamVR_Action_Boolean actionIn, SteamVR_Input_Sources inputSource, bool newValue)
        {
            if (newValue)
                if (selectedCol > 0)
                    selectedCol--;
        }

        private void RightActionChange(SteamVR_Action_Boolean actionIn, SteamVR_Input_Sources inputSource, bool newValue)
        {
            if (newValue)
                if (selectedCol < 2)
                    selectedCol++;
        }

        private void DownActionChange(SteamVR_Action_Boolean actionIn, SteamVR_Input_Sources inputSource, bool newValue)
        {
            if (newValue)
                if (selectedRow < 2)
                    selectedRow++;
        }

        private void SelectActionChange(SteamVR_Action_Boolean actionIn, SteamVR_Input_Sources inputSource, bool newValue)
        {
            if (newValue)
                buttons[selectedRow, selectedCol].onClick.Invoke();
        }
    }
}