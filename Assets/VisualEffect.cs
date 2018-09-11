using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChartGame
{
    public class VisualEffect : MonoBehaviour
    {

        [SerializeField] GameObject effect;
        [SerializeField] Sprite correct;
        [SerializeField] Sprite wrong;

        [Range(0.1f,5)]
        public float lifeTime = 0.5f;
        [Range(0.01f, 1)]
        public float effectSize = 0.2f;
        public float xMaxSpeed = 2;
        public float yMaxSpeed = 3;
        public bool fade;

        float xMin = 0.2f;
        float yMin = 0.4f;
        public void ShowEffect(bool good)
        {
            Image img = Instantiate(effect, transform).GetComponent<Image>();

            img.sprite = good ? correct : wrong;
            RectTransform rt = img.rectTransform;

            xMin = UnityEngine.Random.Range(0.3f, 0.7f);

            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMin+ effectSize, yMin + effectSize);

            StartCoroutine(MoveImage(rt, good));
            
            if(fade)StartCoroutine(FadeImage(img));

        }

        private IEnumerator FadeImage(Image img)
        {
            while (img != null)
            {
                img.color = new Color(img.color.r, img.color.g, img.color.b, img.color.a * 0.95f);
                yield return new WaitForEndOfFrame();
            }

        }

        private IEnumerator MoveImage(RectTransform rt, bool up)
        {
            Destroy(rt.gameObject, lifeTime);
            float vx = UnityEngine.Random.Range(-xMaxSpeed, xMaxSpeed);
            float vy =  UnityEngine.Random.Range(up ? 1f:-yMaxSpeed,up? yMaxSpeed: -1f);

            while (rt != null)
            {
               
                Vector3 v = new Vector3(vx, vy, 0);
                rt.localPosition = rt.localPosition + v;
               
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
