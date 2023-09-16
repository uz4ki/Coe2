using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TinyGiantStudio.Text
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Tiny Giant Studio/Modular 3D Text/Input field", order: 20003)]
    [HelpURL("https://ferdowsur.gitbook.io/modular-3d-text/ui/input-field")]
    public class InputField : MonoBehaviour
    {
        [Tooltip("If set to true, the inputfield is focused on awake.\r\nIf it is in a list, the list controls who to focus on.")]
        public bool autoFocusOnGameStart = true;
        public bool interactable = true;

        [SerializeField]
        private string _text = string.Empty;
        /// <summary>
        /// Directly assigning the Text property makes a sound If it has both Audioclip and Audio Source.
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { _text = value; UpdateText(true); }
        }
        public string placeHolderText = "Enter Text...";

        public int maxCharacter = 20;
        public string caret = "|";
        public bool hideCaretIfMaxCharacter = true;



        public ContentType contentType = ContentType.Anything;

        public Modular3DText textComponent = null;
        public Renderer background = null;

        public bool enterKeyEndsInput = true;

        public Material placeHolderTextMat = null;

        public Material inFocusTextMat = null;
        public Material inFocusBackgroundMat = null;

        public Material outOfFocusTextMat = null;
        public Material outOfFocusBackgroundMat = null;

        public Material disabledTextMat = null;
        public Material disabledBackgroundMat = null;

        Material currentTextMaterial = null;

        [SerializeField]
        AudioClip typeSound = null;
        [SerializeField]
        AudioSource audioSource = null;

        public UnityEvent onInput = null;
        public UnityEvent onBackspace = null;
        public UnityEvent onInputEnd = null;

        TouchScreenKeyboard screenKeyboard;

        void Awake()
        {
            if (!StaticMethods.GetParentList(transform))
                Focus(autoFocusOnGameStart);
        }

#if ENABLE_INPUT_SYSTEM
        void OnEnable()
        {
            Keyboard.current.onTextInput += OnTextInput;
        }

        void OnDisable()
        {
            Keyboard.current.onTextInput -= OnTextInput;

            if (screenKeyboard != null)
                screenKeyboard.active = false;
        }

        void OnTextInput(char ch)
        {
            if (!interactable)
                return;
            ProcessNewChar(ch);
        }
#else
        private void Update()
        {
            if (!interactable)
                return;

            foreach (char c in Input.inputString)
            {
                ProcessNewChar(c);
            }
        }
#endif

        void ProcessNewChar(char c)
        {
            if (c == '\b') // has backspace/delete been pressed?
            {
                if (Text.Length != 0)
                {
                    Text = Text.Substring(0, Text.Length - 1);
                    UpdateText(true);
                    onBackspace.Invoke();
                }
            }
            else if (((c == '\n') || (c == '\r')) && enterKeyEndsInput) // enter/return
            {
                InputComplete();
            }
            else
            {
                if (_text.Length < maxCharacter)
                {
                    Text += c;
                    UpdateText(true);
                    onInput.Invoke();
                }
            }
        }

        public void InputComplete()
        {
            onInputEnd.Invoke();
            this.enabled = false;
        }

        public void UpdateText()
        {
            UpdateText(false);
        }
        public void UpdateText(string newText)
        {
            _text = newText;
            UpdateText(false);
        }
        public void UpdateText(int newTextInt)
        {
            _text = newTextInt.ToString();
            UpdateText(false);
        }
        public void UpdateText(float newTextFloat)
        {
            _text = newTextFloat.ToString();
            UpdateText(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sound">Sound on/off</param>
        public void UpdateText(bool sound)
        {
            if (!textComponent)
                return;



            if (!string.IsNullOrEmpty(Text))
            {
                textComponent.Material = currentTextMaterial;
                if (_text.Length >= maxCharacter && hideCaretIfMaxCharacter)
                    textComponent.Text = GetText();
                else
                    textComponent.Text = (string.Concat(GetText(), caret));
            }
            else
            {
                textComponent.Material = placeHolderTextMat;
                textComponent.Text = placeHolderText;
            }

            if (typeSound && sound && audioSource)
            {
                audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(typeSound);
            }

            OpenTouchScreenKeyboard();
        }



        string GetText()
        {
            if (contentType == ContentType.Anything)
                return Text;
            else if (contentType == ContentType.IntegarNumber)
            {
                _text = new String(_text.Where(Char.IsDigit).ToArray());
                return Text;
            }
            else if (contentType == ContentType.FloatNumber)
            {
                CultureInfo ci = CultureInfo.CurrentCulture;
                var decimalSeparator = ci.NumberFormat.NumberDecimalSeparator;
                string newText = "";
                bool alreadyGotSeparator = false;
                foreach (char c in _text)
                {
                    if (alreadyGotSeparator! && c == '.' || c == decimalSeparator[0])
                    {
                        newText += c;
                    }
                    else if ((c >= '0' && c <= '9'))
                    {
                        newText += c;
                    }
                }
                _text = newText;
                return Text;
            }
            else if (contentType == ContentType.Password)
            {
                string newText = "";
                foreach (char c in _text)
                    newText += c;

                return newText;
            }
            else if (contentType == ContentType.Pin)
            {
                _text = new String(_text.Where(Char.IsDigit).ToArray());
                string newText = "";
                foreach (char c in _text)
                    newText += c;
            }
            return Text;
        }

        /// <summary>
        /// Sets the text to empty
        /// </summary>
        public void EmptyText()
        {
            _text = string.Empty;
            UpdateText(false);
        }

        public void Select()
        {
            Focus(true);

            if (transform.parent)
            {
                if (transform.parent.GetComponent<List>())
                    transform.parent.GetComponent<List>().SelectItem(transform.GetSiblingIndex());
            }
        }

        /// <summary>
        /// There is a one-frame delay in this working.
        /// This is to avoid pressing the "enter" key in one list to apply to another UI just getting enabled
        /// </summary>
        /// <param name="enable"></param>
        public void Focus(bool enable)
        {
            StartCoroutine(FocusRoutine(enable));
        }

        /// <summary>
        /// There is a one-frame delay in this working.
        /// This is to avoid pressing the "enter" key in one list to apply to another UI just getting enabled
        /// </summary>
        /// <param name="enable"></param>
        IEnumerator FocusRoutine(bool enable)
        {
            yield return null;
            FocusFunction(enable);
        }

        public void Focus(bool enable, bool delay)
        {
            if (!delay)
                FocusFunction(enable);
            else
                StartCoroutine(FocusRoutine(enable));
        }

        void FocusFunction(bool enable)
        {
            if (interactable)
            {
                if (Application.isPlaying)
                    this.enabled = enable;

                if (enable)
                {
                    SelectedVisual();
                    OpenTouchScreenKeyboard();
                }
                else
                {
                    UnselectedVisual();
                    CloseTouchScreenKeyboard();
                }
            }
            else
            {
                DisableVisual();
            }

            UpdateText(false);
        }


        public void OpenTouchScreenKeyboard()
        {
            if (TouchScreenKeyboard.isSupported)
            {
                if (screenKeyboard == null)
                    screenKeyboard = TouchScreenKeyboard.Open(Text);
                else if (screenKeyboard.status != TouchScreenKeyboard.Status.Visible)
                {
                    screenKeyboard.text = Text;
                    screenKeyboard.active = true;
                }
            }
        }

        public void CloseTouchScreenKeyboard()
        {
            if (screenKeyboard != null)
                screenKeyboard.active = false;
        }


        public void Interactable()
        {
            Focus(false, false);
        }
        public void Uninteractable()
        {
            this.enabled = false;

            DisableVisual();
            UpdateText(false);
        }

#if UNITY_EDITOR
        public void InteractableUsedByEditorOnly()
        {
            FocusFunction(false);
        }

        public void UninteractableUsedByEditorOnly()
        {
            DisableVisual();
            UpdateText(false);
        }
#endif




        #region Visuals
        private void SelectedVisual()
        {
            //item1 = applyStyleFromParent
            var applySelectedItemMaterial = ApplySelectedStyleFromParent();

            //apply parent list mat
            if (applySelectedItemMaterial.Item1)
                UpdateMaterials(applySelectedItemMaterial.Item2.SelectedTextMaterial, applySelectedItemMaterial.Item2.SelectedBackgroundMaterial);
            //apply self mat
            else
                UpdateMaterials(inFocusTextMat, inFocusBackgroundMat);
        }

        private void UnselectedVisual()
        {
            var applyNormalStyle = ApplyNormalStyleFromParent();

            //apply parent list mat
            if (applyNormalStyle.Item1)
            {
                Material textMat = string.IsNullOrEmpty(_text) ? placeHolderTextMat : applyNormalStyle.Item2.NormalTextMaterial;
                UpdateMaterials(textMat, applyNormalStyle.Item2.NormalBackgroundMaterial);
            }
            //apply self mat
            else
            {
                Material textMat = string.IsNullOrEmpty(_text) ? placeHolderTextMat : outOfFocusTextMat;
                UpdateMaterials(textMat, outOfFocusBackgroundMat);
            }
        }

        public void DisableVisual()
        {
            var applySelectedItemMaterial = ApplyDisabledStyleFromParent();

            //apply parent list mat
            if (applySelectedItemMaterial.Item1)
            {
                Material textMat = string.IsNullOrEmpty(_text) ? placeHolderTextMat : applySelectedItemMaterial.Item2.DisabledTextMaterial;
                UpdateMaterials(textMat, applySelectedItemMaterial.Item2.DisabledBackgroundMaterial);
            }
            //apply self mat
            else
            {
                Material textMat = string.IsNullOrEmpty(_text) ? placeHolderTextMat : disabledTextMat;
                UpdateMaterials(textMat, disabledBackgroundMat);
            }
        }

        //here
        void UpdateMaterials(Material textMat, Material backgroundMat)
        {
            if (textComponent)
                textComponent.Material = textMat;
            if (background)
                background.material = backgroundMat;

            currentTextMaterial = textMat;
        }

        private List GetParentList()
        {
            return StaticMethods.GetParentList(transform);
        }

        public (bool, List) ApplyNormalStyleFromParent()
        {
            List list = GetParentList();
            if (list)
            {
                if (list.UseStyle && list.UseNormalItemVisual)
                {
                    return (true, list);
                }
            }
            //don't apply from list
            return (false, list);
        }
        public (bool, List) ApplySelectedStyleFromParent()
        {
            //get style from parent list
            List list = GetParentList();
            if (list)
            {
                if (list.UseStyle && list.UseSelectedItemVisual)
                {
                    return (true, list);
                }
            }
            //don't apply from list
            return (false, list);
        }
        public (bool, List) ApplyDisabledStyleFromParent()
        {
            List list = GetParentList();

            if (list)
            {
                if (list.UseStyle && list.UseDisabledItemVisual)
                    return (true, list);
            }
            return (false, list);
        }
        #endregion Visual


        public enum ContentType
        {
            Anything,
            IntegarNumber,
            FloatNumber,
            Password,
            Pin
            //Custom //need to think of what options should customs have
        }
    }
}
