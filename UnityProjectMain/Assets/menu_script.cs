using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace Valve.VR.InteractionSystem.Sample
{
    public class menu_script : MonoBehaviour//, ISelectHandler
    {
        // Start is called before the first frame update
        public string text = "";
        public Hand hand;
        public SteamVR_Action_Boolean upAction;
        public SteamVR_Action_Boolean leftAction;
        public SteamVR_Action_Boolean rightAction;
        public SteamVR_Action_Boolean downAction;
        public SteamVR_Action_Boolean selectAction;

        public SteamVR_Action_Boolean[] actions = new SteamVR_Action_Boolean[5];

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

        private void Start()
        {
            buttons[0, 0] = texture1btn;
            buttons[0, 1] = texture1btn;
            buttons[0, 2] = texture1btn;

            buttons[1, 0] = color1btn;
            buttons[1, 1] = color2btn;
            buttons[1, 2] = color3btn;

            buttons[2, 0] = size1btn;
            buttons[2, 1] = size2btn;
            buttons[2, 2] = size3btn;

            actions[0] = (upAction);
            actions[1] = (leftAction);
            actions[2] = (rightAction);
            actions[3] = (downAction);
            actions[4] = (selectAction);
            //buttons[0,0].Selected() = true;
        }

        private void onEnable()
        {
            if (hand == null)
                hand = this.GetComponent<Hand>();

            upAction.AddOnChangeListener(OnPlantActionChange, hand.handType);
        }

        private void OnDisable()
        {
            foreach (SteamVR_Action_Boolean action in actions)
            {
                action.RemoveOnChangeListener(OnPlantActionChange, hand.handType);
            }
        }

        private void OnPlantActionChange(SteamVR_Action_Boolean actionIn, SteamVR_Input_Sources inputSource, bool newValue)
        {
            if (newValue)
                text = "SOMETHING IS HAPPENING";
            else
                text = "SOMETHING IS NOT HAPPENING";
        }
    }
}