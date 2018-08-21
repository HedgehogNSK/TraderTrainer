using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Globalization;

namespace ChartGame
{
    public class GamePreferences : MonoBehaviour
    {
        #region INSTANCE
        static GamePreferences _instance;
        public static GamePreferences Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindObjectOfType<GamePreferences>();
                    if (!_instance)
                    {
                        Debug.LogWarning("Объект GamePreferences отсутствует на сцене");
                    }
                    return _instance;
                }
                return _instance;
            }

        }
        #endregion

        [SerializeField] int fast_ma_length = 13;
        [SerializeField] Color fast_ma_Color;
        [SerializeField] int slow_ma_length = 32;
        [SerializeField] Color slow_ma_Color;

        public event System.Action<int> FastMALengthChanged, SlowMALengthChanged;

        public int Fast_ma_length
        {
            get
            {
                return PlayerPrefs.GetInt("fast_ma_length", fast_ma_length);
            }

            set
            {
                PlayerPrefs.SetInt("fast_ma_length", value);
                if(FastMALengthChanged!=null) FastMALengthChanged(value);
            }
        }
        public int Slow_ma_length
        {
            get
            {
                return PlayerPrefs.GetInt("slow_ma_length", slow_ma_length);
            }

            set
            {
                PlayerPrefs.SetInt("slow_ma_length", value);
                if (SlowMALengthChanged != null) SlowMALengthChanged(value);
            }
        }
        public Color32 Fast_ma_color
        {
            get
            {
                Color32 color;
                if (HexColorField.HexToColor(PlayerPrefs.GetString("fast_ma_color"), out color))
                {
                    return color;
                }
                else
                {
                    return fast_ma_Color;
                }

            }

            set
            {
                string color2string;
                color2string = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", value.r, value.g, value.b, value.a);

                PlayerPrefs.SetString("fast_ma_color", color2string);
            }
        }
        public Color32 Slow_ma_color
        {
            get
            {
                Color32 color;
                if (HexColorField.HexToColor(PlayerPrefs.GetString("slow_ma_color"), out color))
                {
                    return color;
                }
                else
                {
                    return slow_ma_Color;
                }

            }

            set
            {
                string color2string = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", value.r, value.g, value.b, value.a);
                PlayerPrefs.SetString("slow_ma_color", color2string);
            }
        }
        private void OnApplicationPause(bool pause)
        {
            PlayerPrefs.Save();
        }
    }
}