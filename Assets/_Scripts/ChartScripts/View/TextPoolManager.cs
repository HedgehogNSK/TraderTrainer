using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
namespace Chart
{
    public class TextPoolManager : MonoBehaviour
    {
        public enum ShiftBy
        {
            Horizontal,
            Vertical
        }
        [SerializeField] Text[] textPool;
        public int FieldsAmount { get { return textPool.Length; } }
        public int CurrentField { get { return current_id; } }
        int current_id;

        bool IsCurrentExists{
            get
            {
                if (current_id < textPool.Length)
                {
                    return true;
                }

                Debug.LogError("Всего текстовых полей: "+textPool.Length+ "Текущее поле"+current_id+ "\n Очисти пул, либо добавь ещё объектов");
                return false;

            }
        }

        private void Awake()
        {
            textPool = GetComponentsInChildren<Text>();
            CleanPool();
        }

        private float ClampPosition(float pos, ShiftBy shiftBy)
        {
            if (pos < 0) pos = 0;
            else
            {
                switch(shiftBy)
                {
                    case ShiftBy.Horizontal: { if (pos > Screen.width) pos = Screen.width; } break;
                    case ShiftBy.Vertical: { if (pos > Screen.height) pos = Screen.height; } break;
                    default: { throw new System.ArgumentOutOfRangeException("Действие для ShiftBy=" + shiftBy + " не описано"); }
                }
                
            }
            return pos;
        }
        public void SetText(string txt, Vector2 position)
        {
            if (IsCurrentExists)
            {
                textPool[current_id].text = txt;    
                textPool[current_id].rectTransform.position = position;
                current_id++;
            }
        }
        public void SetText(string txt, float position, ShiftBy shiftBy)
        {

            if (IsCurrentExists)
            {
                textPool[current_id].text = txt;
                switch (shiftBy)
                {
                    case ShiftBy.Horizontal: { textPool[current_id].rectTransform.position = new Vector2(ClampPosition(position,shiftBy), textPool[current_id].rectTransform.position.y); } break;
                    case ShiftBy.Vertical: { textPool[current_id].rectTransform.position = new Vector2(textPool[current_id].rectTransform.position.x, ClampPosition(position, shiftBy)); } break;
                    default: { throw new System.ArgumentOutOfRangeException("Действие для ShiftBy=" + shiftBy + " не описано"); }
                }
                current_id++;
            }
        }
        



        public void CleanPool()
        {
            for (int id = 0; id < textPool.Length; id++)
            {
                textPool[id].text = null;
            }
            current_id = 0;
        }
    }
}