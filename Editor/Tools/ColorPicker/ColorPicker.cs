using System.Globalization;
using Mane.DotNet;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mane.Unity.Editor
{
    public class ColorPicker : EditorWindow
    {
        [SerializeField] private VisualTreeAsset xml;
        [SerializeField] private Color _color = Color.white;

        private ColorField _colorField;
        private FloatField _rFloat;
        private FloatField _gFloat;
        private FloatField _bFloat;
        private FloatField _aFloat;
        private IntegerField _rInt;
        private IntegerField _gInt;
        private IntegerField _bInt;
        private IntegerField _aInt;
        private TextField _hexField;
        private TextField _hueFloat;
        private TextField _saturationFloat;
        private TextField _lightnessFloat;
        private TextField _hueInt;
        private TextField _saturationInt;
        private TextField _lightnessInt;
        private TextField _lumaField;
        private TextField _codeField;
        private bool _syncing;

        [MenuItem("Window/Mane/Color Picker %&C", false, 2000)]
        public static void ShowWindow()
        {
            ColorPicker window = GetWindow<ColorPicker>();
            window.titleContent = new GUIContent("Color Picker");
            window.minSize = new Vector2(280f, 180f);
            window.maxSize = new Vector2(280f, 372f);
        }

        private void CreateGUI()
        {
            if (xml == null)
            {
                Debug.LogError("ColorPicker UXML is not assigned.");
                return;
            }
            xml.CloneTree(rootVisualElement);
            rootVisualElement.style.overflow = Overflow.Hidden;
            _colorField = rootVisualElement.Q<ColorField>("colorField");
            _rFloat = rootVisualElement.Q<FloatField>("rFloat");
            _gFloat = rootVisualElement.Q<FloatField>("gFloat");
            _bFloat = rootVisualElement.Q<FloatField>("bFloat");
            _aFloat = rootVisualElement.Q<FloatField>("aFloat");
            _rInt = rootVisualElement.Q<IntegerField>("rInt");
            _gInt = rootVisualElement.Q<IntegerField>("gInt");
            _bInt = rootVisualElement.Q<IntegerField>("bInt");
            _aInt = rootVisualElement.Q<IntegerField>("aInt");
            _hexField = rootVisualElement.Q<TextField>("hexField");
            _hueFloat = rootVisualElement.Q<TextField>("hueFloat");
            _saturationFloat = rootVisualElement.Q<TextField>("saturationFloat");
            _lightnessFloat = rootVisualElement.Q<TextField>("lightnessFloat");
            _hueInt = rootVisualElement.Q<TextField>("hueInt");
            _saturationInt = rootVisualElement.Q<TextField>("saturationInt");
            _lightnessInt = rootVisualElement.Q<TextField>("lightnessInt");
            _lumaField = rootVisualElement.Q<TextField>("lumaField");
            _codeField = rootVisualElement.Q<TextField>("codeField");

            if (_colorField == null || _hexField == null || _codeField == null ||
                _rFloat == null || _gFloat == null || _bFloat == null || _aFloat == null ||
                _rInt == null || _gInt == null || _bInt == null || _aInt == null ||
                _hueFloat == null || _saturationFloat == null || _lightnessFloat == null ||
                _hueInt == null || _saturationInt == null || _lightnessInt == null ||
                _lumaField == null)
            {
                Debug.LogError("ColorPicker UXML is missing expected elements.");
                return;
            }

            _colorField.showAlpha = true;
            _colorField.showEyeDropper = true;

            SetupCopyField(_hueFloat, _saturationFloat, _lightnessFloat,
                _hueInt, _saturationInt, _lightnessInt, _lumaField, _codeField);

            _colorField.RegisterValueChangedCallback(evt => ApplyColor(evt.newValue));
            BindChannel(_rFloat, _rInt, (ref Color color, float value) => color.r = value);
            BindChannel(_gFloat, _gInt, (ref Color color, float value) => color.g = value);
            BindChannel(_bFloat, _bInt, (ref Color color, float value) => color.b = value);
            BindChannel(_aFloat, _aInt, (ref Color color, float value) => color.a = value);
            _hexField.RegisterValueChangedCallback(evt => ApplyHex(evt.newValue));

            RefreshFields();
            rootVisualElement.schedule.Execute(TryPasteClipboard).Every(200);
        }

        private void BindChannel(FloatField floatField, IntegerField intField, SetChannel setChannel)
        {
            floatField.RegisterValueChangedCallback(evt =>
            {
                if (_syncing)
                    return;

                Color color = _color;
                setChannel(ref color, evt.newValue.Clamp01());
                ApplyColor(color);
            });

            intField.RegisterValueChangedCallback(evt =>
            {
                if (_syncing)
                    return;

                Color color = _color;
                setChannel(ref color, evt.newValue.Clamp(0, 255) / 255f);
                ApplyColor(color);
            });
        }

        private void ApplyColor(Color color, bool clearClipboard = false)
        {
            if (_syncing)
                return;

            _color = color;
            if (clearClipboard)
                EditorGUIUtility.systemCopyBuffer = string.Empty;

            RefreshFields();
        }

        private void ApplyHex(string hex)
        {
            if (_syncing)
                return;

            if (TryParseHex(hex, out Color color))
            {
                _color = color;
                RefreshFields();
                return;
            }

            _syncing = true;
            _hexField.SetValueWithoutNotify(_color.ToHex());
            _syncing = false;
        }

        private void TryPasteClipboard()
        {
            if (_syncing)
                return;

            string buffer = EditorGUIUtility.systemCopyBuffer;
            if (string.IsNullOrWhiteSpace(buffer) || !TryParseHex(buffer, out Color color))
                return;

            if (_color == color)
                return;

            ApplyColor(color, true);
        }

        private void RefreshFields()
        {
            _syncing = true;

            _colorField.SetValueWithoutNotify(_color);
            _rFloat.SetValueWithoutNotify(_color.r);
            _gFloat.SetValueWithoutNotify(_color.g);
            _bFloat.SetValueWithoutNotify(_color.b);
            _aFloat.SetValueWithoutNotify(_color.a);
            _rInt.SetValueWithoutNotify(Mathf.RoundToInt(_color.r * 255f));
            _gInt.SetValueWithoutNotify(Mathf.RoundToInt(_color.g * 255f));
            _bInt.SetValueWithoutNotify(Mathf.RoundToInt(_color.b * 255f));
            _aInt.SetValueWithoutNotify(Mathf.RoundToInt(_color.a * 255f));
            _hexField.SetValueWithoutNotify(_color.ToHex());

            float hue = _color.GetHSL_Hue();
            float saturation = _color.GetHSL_Saturation();
            float lightness = _color.GetHSL_Lightness();
            float luma = _color.GetLuma();

            _hueFloat.SetValueWithoutNotify(FormatFloat(hue));
            _saturationFloat.SetValueWithoutNotify(FormatFloat(saturation));
            _lightnessFloat.SetValueWithoutNotify(FormatFloat(lightness));
            _hueInt.SetValueWithoutNotify(Mathf.RoundToInt(hue * 360f).ToString());
            _saturationInt.SetValueWithoutNotify(Mathf.RoundToInt(saturation * 100f).ToString());
            _lightnessInt.SetValueWithoutNotify(Mathf.RoundToInt(lightness * 100f).ToString());
            _lumaField.SetValueWithoutNotify(FormatFloat(luma));
            _codeField.SetValueWithoutNotify(
                $"new Color({_color.r:n3}f, {_color.g:n3}f, {_color.b:n3}f, {_color.a:n3}f)");

            _syncing = false;
        }

        private static void SetupCopyField(params TextField[] fields)
        {
            foreach (var field in fields)
            {
                field.isReadOnly = true;
                var field1 = field;
                field.RegisterCallback<FocusInEvent>(_ => field1.SelectAll());
            }
        }

        private static string FormatFloat(float value) =>
            value.ToString(CultureInfo.InvariantCulture);

        private static bool TryParseHex(string value, out Color color)
        {
            if (ColorUtility.TryParseHtmlString(value, out color))
                return true;

            return value.Length is 6 or 8 && ColorUtility.TryParseHtmlString("#" + value, out color);
        }

        private delegate void SetChannel(ref Color color, float value);
    }
}
