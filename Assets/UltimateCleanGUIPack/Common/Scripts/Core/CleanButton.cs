// Copyright (C) 2015-2020 gamevanilla - All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement.
// A Copy of the Asset Store EULA is available at http://unity3d.com/company/legal/as_terms.

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UltimateClean
{
    /// <summary>
    /// The main button component used throughout the kit, which fades in and out
    /// as the player rolls over/presses it.
    /// </summary>
    public class CleanButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public float fadeTime = 0.2f;
        public float onHoverAlpha;
        public float onClickAlpha;

        [Serializable]
        public class ButtonClickedEvent : UnityEvent { }

        [SerializeField]
        private ButtonClickedEvent onClicked = new ButtonClickedEvent();

        private CanvasGroup canvasGroup;
        private AudioSource audioSource;
        private ButtonSounds buttonSounds;

        private void Awake()
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            audioSource = GetComponent<AudioSource>();
            buttonSounds = GetComponent<ButtonSounds>();
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (audioSource != null)
            {
                audioSource.clip = buttonSounds.rolloverSound;
                audioSource.Play();
            }

            StopAllCoroutines();
            StartCoroutine(Utils.FadeOut(canvasGroup, onHoverAlpha, fadeTime));
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            StopAllCoroutines();
            StartCoroutine(Utils.FadeIn(canvasGroup, 1.0f, fadeTime));
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (audioSource != null)
            {
                audioSource.clip = buttonSounds.pressedSound;
                audioSource.Play();
            }

            canvasGroup.alpha = onClickAlpha;

            onClicked.Invoke();
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            canvasGroup.alpha = 1.0f;
        }
    }
}
