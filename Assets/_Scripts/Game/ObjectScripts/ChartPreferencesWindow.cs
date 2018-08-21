using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ChartGame
{
    public class ChartPreferencesWindow : MonoBehaviour
    {
        GamePreferences gamePrefs;

        [SerializeField] InputField lengthField;
        [SerializeField] ColorPickerButton colorBtn;
        [SerializeField] InputField lengthField2;
        [SerializeField] ColorPickerButton colorBtn2;
        [SerializeField] Button acceptBtn;
        // Use this for initialization
        private void Awake()
        {
            acceptBtn.onClick.AddListener( AcceptAndClose);
            gameObject.SetActive(false);
        }
        void Start()
        {
            gamePrefs = GamePreferences.Instance;
            
            lengthField.text = gamePrefs.Fast_ma_length.ToString();
            lengthField2.text = gamePrefs.Slow_ma_length.ToString();
            colorBtn.image.color = gamePrefs.Fast_ma_color;
            colorBtn2.image.color = gamePrefs.Slow_ma_color;
        }

        public void AcceptAndClose()
        {
            //Передаём изменённые настройки GameManager'у
            gamePrefs.Fast_ma_length = int.Parse(lengthField.text);
            gamePrefs.Slow_ma_length = int.Parse(lengthField2.text);
            gamePrefs.Fast_ma_color = colorBtn.image.color;
            gamePrefs.Slow_ma_color = colorBtn2.image.color;

            gameObject.SetActive(false);
        }

    }
}