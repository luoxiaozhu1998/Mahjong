// Copyright (C) 2015-2020 gamevanilla - All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement.
// A Copy of the Asset Store EULA is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UltimateClean
{
    /// <summary>
    /// This component is used to provide idle slider animations in the demos.
    /// </summary>
    public class SliderAnimation : MonoBehaviour
    {
        public Image image;
        public TextMeshProUGUI text;

        public float duration = 1;

        private RectTransform rectTransform;
        private float maxWidth;

        private StringBuilder strBuilder = new StringBuilder(4);
        private int lastPercentage = -1;

        private void Awake()
        {
            rectTransform = image.GetComponent<RectTransform>();
            var size = rectTransform.sizeDelta;
            maxWidth = size.x;

            var newSize = size;
            newSize.x = 0;
            size = newSize;
            rectTransform.sizeDelta = size;

            if (duration > 0)
                StartCoroutine(Animate());
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        private IEnumerator Animate()
        {
            while (true)
            {
                var ratio = 0.0f;
                var multiplier = 1.0f / duration;
                while (rectTransform.sizeDelta.x < maxWidth)
                {
                    ratio += Time.deltaTime * multiplier;

                    var width = Mathf.Lerp(0, maxWidth, ratio);
                    var newSize = rectTransform.sizeDelta;
                    newSize.x = width;
                    rectTransform.sizeDelta = newSize;

                    var percentage = (int)(width / maxWidth * 100);
                    if (percentage != lastPercentage)
                    {
                        lastPercentage = percentage;
                        if (text != null)
                        {
                            strBuilder.Clear();
                            text.text = strBuilder.Append(lastPercentage).Append("%").ToString();
                        }
                    }

                    yield return null;
                }

                while (rectTransform.sizeDelta.x > 0)
                {
                    ratio -= Time.deltaTime * multiplier;

                    var width = Mathf.Lerp(0, maxWidth, ratio);
                    var newSize = rectTransform.sizeDelta;
                    newSize.x = width;
                    rectTransform.sizeDelta = newSize;

                    if (text != null)
                        text.text = $"{(int)(width / maxWidth * 100)}%";

                    yield return null;
                }
            }
        }
    }
}