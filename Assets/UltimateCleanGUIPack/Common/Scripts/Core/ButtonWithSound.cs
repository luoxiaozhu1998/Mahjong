// Copyright (C) 2015-2020 gamevanilla - All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement.
// A Copy of the Asset Store EULA is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UltimateClean
{
    /// <summary>
    /// This component goes together with a ButtonWithSound object and contains
    /// the audio clips to play when the player rolls over and presses it.
    /// </summary>
    public class ButtonWithSound : Button
    {
        private bool pointerWasUp;

        private ButtonSounds buttonSounds;

        protected override void Awake()
        {
            base.Awake();
            buttonSounds = GetComponent<ButtonSounds>();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (buttonSounds != null)
            {
                buttonSounds.PlayPressedSound();
            }

            base.OnPointerClick(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            pointerWasUp = true;
            base.OnPointerUp(eventData);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (pointerWasUp)
            {
                pointerWasUp = false;
                base.OnPointerEnter(eventData);
            }
            else
            {
                if (buttonSounds != null)
                {
                    buttonSounds.PlayRolloverSound();
                }

                base.OnPointerEnter(eventData);
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            pointerWasUp = false;
            base.OnPointerExit(eventData);
        }
    }
}