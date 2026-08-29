using System.Globalization;
using Mane.DotNet;
using UnityEngine;
using UnityEditor;


namespace Mane.Unity.Editor
{
    public class ColorPicker : EditorWindow
    {
        [MenuItem("Mane/Color Picker %&C", false, 1000)]
        public static void ShowWindow() =>
            GetWindow(typeof(ColorPicker)).titleContent = new GUIContent("Color Picker");

        [SerializeField] private Color _color = Color.white;

        private string _hexText;
        private string _codeText;
        
        private string _hue;
        private int _hueInt;
        
        private string _saturation;
        private int _saturationInt;
        
        private string _lightness;
        private int _lightnessInt;

        private string _luma;
        private int _lumaInt;

        private static GUIStyle _labelStyle;
        
        private void OnEnable()
        {
            _hexText = _color.ToHex();
            UpdateReadOnlyFields();
        }

        private void OnGUI()
        {
            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleRight
                };
            }

            Color newColor = _color;
            
            // check clipboard
            string buffer = EditorGUIUtility.systemCopyBuffer;
            if (!string.IsNullOrWhiteSpace(buffer))
            {
                if (ColorUtility.TryParseHtmlString(buffer, out Color color))
                    newColor = color;
                else if (buffer.Length is 6 or 8 &&
                         ColorUtility.TryParseHtmlString("#" + buffer, out color))
                    newColor = color;
            }

            // clear buffer if color was pasted to allow future editing
            if (CheckColorChanged(newColor))
                EditorGUIUtility.systemCopyBuffer = string.Empty;
                
            // title
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Color");
            newColor = EditorGUILayout.ColorField(_color);
            EditorGUILayout.Space();

            // channel values
            newColor.r = DrawChannel(newColor.r, "R");
            newColor.g = DrawChannel(newColor.g, "G");
            newColor.b = DrawChannel(newColor.b, "B");
            newColor.a = DrawChannel(newColor.a, "A");
            
            CheckColorChanged(newColor);

            // color to hex
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("HEX", _labelStyle, GUILayout.Width(30f));
            string newHex = EditorGUILayout.TextField(_hexText);
            EditorGUILayout.EndHorizontal();
            
            CheckHexChanged(newHex);
            
            // color components
            EditorGUILayout.Space();
            DrawReadOnlyField("Hsl", _hue, _hueInt);
            DrawReadOnlyField("hSl", _saturation, _saturationInt);
            DrawReadOnlyField("hsL", _lightness, _lightnessInt);
            
            EditorGUILayout.Space();
            DrawReadOnlyField("Luma", _luma, _lumaInt);

            // c# script
            EditorGUILayout.Space();
            EditorGUILayout.TextField(_codeText);
        }

        private float DrawChannel(float value, string label)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, _labelStyle, GUILayout.Width(15f));
            value = EditorGUILayout.FloatField(value);
            value = EditorGUILayout.IntField((int)(255 * value)) / 255f;
            EditorGUILayout.EndHorizontal();

            value = value.Clamp01();

            return value;
        }
        
        private void DrawReadOnlyField(string label, string value, int intValue)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, _labelStyle, GUILayout.Width(65f));
            EditorGUILayout.TextField(value);
            EditorGUILayout.IntField(intValue);
            EditorGUILayout.EndHorizontal();
        }

        private bool CheckColorChanged(Color color)
        {
            if (_color == color)
                return false;

            _color = color;
            _hexText = _color.ToHex();
            
            UpdateReadOnlyFields();

            return true;
        }

        private bool CheckHexChanged(string hex)
        {
            if (_hexText == hex)
                return false;

            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                _color = color;
                _hexText = hex;
                
                UpdateReadOnlyFields();
            }
            else
                _hexText = _color.ToHex();

            return true;
        }

        private void UpdateReadOnlyFields()
        {
            _codeText = GetCodeText();
            
            float hue = _color.GetHSL_Hue();
            _hue = hue.ToString(CultureInfo.InvariantCulture);
            _hueInt = Mathf.RoundToInt(hue * 360f);
            
            float saturation = _color.GetHSL_Saturation();
            _saturation = saturation.ToString(CultureInfo.InvariantCulture);
            _saturationInt = Mathf.RoundToInt(saturation * 100f);
            
            float lightness = _color.GetHSL_Lightness();
            _lightness = lightness.ToString(CultureInfo.InvariantCulture);
            _lightnessInt = Mathf.RoundToInt(lightness * 100f);
            
            float luma = _color.GetLuma();
            _luma = luma.ToString(CultureInfo.InvariantCulture);
            _lumaInt = Mathf.RoundToInt(luma * 100f);
        }

        private string GetCodeText() => $"new Color({_color.r:n3}f, {_color.g:n3}f, {_color.b:n3}f, {_color.a:n3}f)";
    }
}