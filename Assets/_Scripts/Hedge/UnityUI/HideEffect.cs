using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedge.UnityUI
{
    public class HideEffect : MonoBehaviour
    {

        [Range(0.1f, 5)]
        public float life_time = 0.5f;
        [Space]
        public bool move = true;

        [Range(0.01f, 1)]
        public float relativeSize = 0.2f;
        public Vector2 xSpeedRange = new Vector2(-2, 2);
        public Vector2 ySpeedRange = new Vector2(1, 3);

        float xMin = 0.2f;
        float yMin = 0.4f;

        [Space]
        protected System.Action PlayEffect;
        RectTransform rt;

        Vector3 v;

        protected virtual void Awake()
        {
            rt = GetComponent<RectTransform>();

            xMin = UnityEngine.Random.Range(0.3f, 0.7f);

            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMin + relativeSize, yMin + relativeSize);

            float vx = UnityEngine.Random.Range(xSpeedRange.x, xSpeedRange.y);
            float vy = UnityEngine.Random.Range(ySpeedRange.x, ySpeedRange.y);

            v = new Vector3(vx, vy, 0);

            if (move) PlayEffect += MoveImage;
        }
        // Use this for initialization
        protected virtual void Start()
        {
            Destroy(gameObject, life_time);
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            if (PlayEffect != null) PlayEffect();
        }


        protected virtual void MoveImage()
        {
            rt.localPosition = rt.localPosition + v;
        }

    }
}

