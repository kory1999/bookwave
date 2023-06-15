using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BeWild.Framework.Runtime.Utils.UI;
using BW.Framework.Utils;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;


namespace BeWild.AIBook.Runtime.Scene.Pages.BookContent.Content
{
    public class HighlightText : TextMeshProUGUI, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler,
        IPointerUpHandler
    {
        public class Caret
        {
            public CaretInfo Info;
            public Image Mask;
        }

        public struct CaretInfo
        {
            public int StartIndex;
            public int EndIndex;
            public int LineIndex;
        }

        public event Action OnTriggerSelectModeFail;
        public event Action OnTriggerSelectModeSuccess;
        public event Action<PointerEventData> OnBeginDragEvent;
        public event Action<PointerEventData> OnEndDragEvent;
        public event Action<PointerEventData> OnDragEvent;
        public event Action OnSelectModeFinishEvent;
        public event Action OnSelectModeCloseEvent;

        public Sprite StartMarkSprite
        {
            get
            {
                return _startMarkSprite;
            }
            set
            {
                if (_startMarkSprite is { } && _startMarkSprite != value)
                {
                    _startMarkSprite = value;
                    if (_leftDragMark != null)
                    {
                        _leftDragMark.sprite = _startMarkSprite;
                    }
                }
            }
        }

        public Sprite EndMarkSprite
        {
            get
            {
                return _endMarkSprite;
            }
            set
            {
                if (_endMarkSprite is { } && _endMarkSprite != value)
                {
                    _endMarkSprite = value;
                    if (_rightDragMark != null)
                    {
                        _rightDragMark.sprite = _endMarkSprite;
                    }
                }
            }
        }

        public Color SelectColor
        {
            get
            {
                return _selectColor;
            }
            set
            {
                _selectColor = value;
                if (_allHighlightCarets != null && _allHighlightCarets.Count > 0)
                {
                    _allHighlightCarets.ForEach(caret =>
                    {
                        caret.Mask.color = _selectColor;
                    });
                }
            }
        }

        [SerializeField] private Color _selectColor;
        /// <summary>
        /// 要触发选择逻辑，手指需要持续按下的时间
        /// </summary>
        [SerializeField] private float _triggerHighlightModeDuration;
        [SerializeField] private Sprite _startMarkSprite;
        [SerializeField] private Sprite _endMarkSprite;
        [SerializeField] private float _markWidth =100f;
        [SerializeField] private float _markHeight = 100f;

        [NonSerialized] public int? HighlightStartIndex;
        private int? _highlightCurrentIndex;
        [NonSerialized] public int? HighlightEndIndex;
        private bool _dragging = false;
        private bool _pointerPress = false;
        private bool _inSelectMode = false;
        private bool _checkCurrentPointer = false;
        private float _holdTimer = 0;
        private Camera _camera;
        private Vector2? _pointerPosition;
        private List<Caret> _allHighlightCarets;
        private bool _allowUpdateRegion = false;
        private DraggableImage _leftDragMark;
        private DraggableImage _rightDragMark;
        private DraggableImage _currentDragMark;
        private int? _onBeginDragStartCharacterIndex;
        private int? _onBeginDragStartWordIndex;
        private Tweener _tweener;

        public Vector3 GetCharWorldPosition(int index, bool ifWhiteSpaceFindNext, int vextexIndex)
        {
            int realIndex;
            if (ifWhiteSpaceFindNext)
            {
                realIndex = FindNextCharIndex(index, text.Length - 1);
            }
            else
            {
                realIndex = FindPreviousCharIndex(index, 0);
            }

            TMP_CharacterInfo charInfo = textInfo.characterInfo[realIndex];
            TMP_MeshInfo charMeshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];
            return transform.TransformPoint(charMeshInfo.vertices[charInfo.vertexIndex+vextexIndex]);
        }

        /// <summary>
        /// OnDrag只记录点击数据，真正的drag逻辑在update
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrag(PointerEventData eventData)
        {
            if (_dragging && HighlightStartIndex != null)
            {
                _pointerPosition = eventData.position;
            }

            OnDragEvent?.Invoke(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _dragging = true;
            HideSelectMark();
            OnBeginDragEvent?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnEndDragEvent?.Invoke(eventData);
            _dragging = false;
            ShowSelectMark();
        }

        public bool IsAnyContentBeSelected()
        {
            return HighlightStartIndex != null && HighlightEndIndex != null;
        }

        protected override void Awake()
        {
            base.Awake();
            _camera = Camera.main;
            _allHighlightCarets = new List<Caret>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pointerPosition = eventData.position;
            _onBeginDragStartCharacterIndex =
                TMP_TextUtilities.GetCursorIndexFromPosition(this, _pointerPosition.Value, _camera);
            _pointerPress = true;
            _checkCurrentPointer = true;
            CloseSelectMode(true);
        }

        public void CloseSelectMode(bool keepSelectedContent = false)
        {
            if (!keepSelectedContent)
            {
                ClearAllCarets();
            }

            OnSelectModeCloseEvent?.Invoke();
            _inSelectMode = false;
            BaseLogger.Log(nameof(HighlightText), $"close select mode");
            _holdTimer = 0;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _tweener?.Kill();
        }

        private float GetLineHeight()
        {
            if (textInfo.lineCount > 0)
            {
                return rectTransform.rect.height / textInfo.lineCount;
            }
            else
            {
                return 0;
            }
        }

        private float GetAnchoredPositionY(int lineIndex)
        {
            if (textInfo.lineCount > 0)
            {
                return (rectTransform.rect.y + rectTransform.rect.height) - GetLineHeight() * lineIndex -
                       (GetLineHeight() / 2f);
            }
            else
            {
                return 0;
            }
        }

        private void Update()
        {
            if (_pointerPress && _checkCurrentPointer && _onBeginDragStartCharacterIndex != null)
            {
                _highlightCurrentIndex =
                    TMP_TextUtilities.GetCursorIndexFromPosition(this, _pointerPosition.Value, _camera);
                if (_inSelectMode)
                {
                    if (!IsCurrentCursorInCaret() || _allowUpdateRegion)
                    {
                        _allowUpdateRegion = true;
                        UpdateDragRegion(_highlightCurrentIndex);
                    }
                }
                else
                {
                    if (_onBeginDragStartCharacterIndex != _highlightCurrentIndex)
                    {
                        _checkCurrentPointer = false;
                        _onBeginDragStartCharacterIndex = null;
                        OnTriggerSelectModeFail?.Invoke();
                    }
                    else if (_holdTimer < _triggerHighlightModeDuration)
                    {
                        _holdTimer += Time.deltaTime;
                    }
                    else if (!_inSelectMode)
                    {
                        _inSelectMode = true;
                        BaseLogger.Log(nameof(HighlightText), $"start select mode");
                        SelectWord(_onBeginDragStartCharacterIndex);
                        OnTriggerSelectModeSuccess?.Invoke();
                    }
                }
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pointerPress = false;
            _onBeginDragStartCharacterIndex = null;
            _onBeginDragStartWordIndex = null;
            _holdTimer = 0;
            _highlightCurrentIndex = null;
            if (_inSelectMode)
            {
                _allowUpdateRegion = false;
                OnSelectModeFinishEvent?.Invoke();
                _inSelectMode = false;
                ShowSelectMark();
            }
        }
        
        public void SetPartialTextColor(Color color,int startIndex,int endIndex)
        {
            startIndex = Mathf.Clamp(startIndex, 0, text.Length - 1);
            endIndex = Mathf.Clamp(endIndex, 0, text.Length - 1);

            for (int i = startIndex; i <= endIndex; i++)
            {
                if (text[i] != ' ' && text[i] != '\n')
                {
                    this.ChangePartialCharColor(i,color);
                }
            }

            UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        public void RefreshSelectArea()
        {
            if (HighlightStartIndex != null && HighlightEndIndex != null)
            {
                GenerateCaret(HighlightStartIndex.Value,HighlightEndIndex.Value);
                ShowSelectMark();
            }
        }

        private void HandleOnMarkBeginDrag(PointerEventData pointerEventData, DraggableImage draggableImage)
        {
            HideSelectMark(false);
        }

        private void HandleOnMarkDragging(PointerEventData pointerEventData, DraggableImage draggableImage)
        {
            _highlightCurrentIndex = TMP_TextUtilities.FindNearestCharacter(this,
                _camera.WorldToScreenPoint(draggableImage.transform.position),
                _camera, true);
            
            UpdateDragRegion(_highlightCurrentIndex);
        }

        private void HandleOnMarkEndDrag(PointerEventData pointerEventData, DraggableImage draggableImage)
        {
            ShowSelectMark();
        }
        
        private void HideSelectMark(bool blockRaycast = true)
        {
            if (_leftDragMark != null)
            {
                _leftDragMark.color = new Color(_leftDragMark.color.r, _leftDragMark.color.g, _leftDragMark.color.b, 0);
                _leftDragMark.raycastTarget = !blockRaycast;
            }

            if (_rightDragMark != null)
            {
                _rightDragMark.color =
                    new Color(_rightDragMark.color.r, _rightDragMark.color.g, _rightDragMark.color.b, 0);
                _rightDragMark.raycastTarget = !blockRaycast;
            }
        }

        private void ShowSelectMark()
        {
            if (HighlightStartIndex != null && HighlightEndIndex != null)
            {
                if (_leftDragMark == null)
                {
                    GameObject leftDragMarkObj = new GameObject("LeftDragMark");
                    RectTransform leftDragMarkRectTransform = leftDragMarkObj.AddComponent<RectTransform>();
                    leftDragMarkObj.transform.SetParent(transform);
                    leftDragMarkObj.transform.localScale = Vector3.one;
                    leftDragMarkObj.transform.localPosition = Vector3.zero;
                    leftDragMarkRectTransform.pivot = new Vector2(1f, 0);
                    leftDragMarkRectTransform.sizeDelta = new Vector2(_markWidth, _markHeight);
                    _leftDragMark = leftDragMarkObj.AddComponent<DraggableImage>();
                    _leftDragMark.preserveAspect = true;
                    if (_startMarkSprite != null)
                    {
                        _leftDragMark.sprite = _startMarkSprite;
                    }
                    _leftDragMark.color = new Color(_leftDragMark.color.r, _leftDragMark.color.g, _leftDragMark.color.b,
                        0);
                    _leftDragMark.OnBeginDragEvent += HandleOnMarkBeginDrag;
                    _leftDragMark.OnDragEvent += HandleOnMarkDragging;
                    _leftDragMark.OnEndDragEvent += HandleOnMarkEndDrag;
                }

                if (_rightDragMark == null)
                {
                    GameObject rightDragMarkObj = new GameObject("RightDragMark");
                    RectTransform rightDragMarkRectTransform = rightDragMarkObj.AddComponent<RectTransform>();
                    rightDragMarkObj.transform.SetParent(transform);
                    rightDragMarkObj.transform.localScale = Vector3.one;
                    rightDragMarkObj.transform.localPosition = Vector3.zero;
                    rightDragMarkRectTransform.pivot = new Vector2(0, 1f);
                    rightDragMarkRectTransform.sizeDelta = new Vector2(_markWidth, _markHeight);
                    _rightDragMark = rightDragMarkObj.AddComponent<DraggableImage>();
                    _rightDragMark.preserveAspect = true;
                    if (_endMarkSprite != null)
                    {
                        _rightDragMark.sprite = _endMarkSprite;
                    }
                    _rightDragMark.color = new Color(_rightDragMark.color.r, _rightDragMark.color.g,
                        _rightDragMark.color.b, 0);
                    _rightDragMark.OnBeginDragEvent += HandleOnMarkBeginDrag;
                    _rightDragMark.OnDragEvent += HandleOnMarkDragging;
                    _rightDragMark.OnEndDragEvent += HandleOnMarkEndDrag;
                }

                Vector3 leftMarkWorldPosition = GetCharWorldPosition(HighlightStartIndex.Value, true,1);
                Vector3 rightMarkWorldPosition = GetCharWorldPosition(HighlightEndIndex.Value, false, 3);

                _leftDragMark.transform.position = leftMarkWorldPosition;
                _rightDragMark.transform.position = rightMarkWorldPosition;

                _tweener?.Kill();
                _tweener = DOTween.To(() => { return _leftDragMark.color.a; }, (newValue) =>
                {
                    _leftDragMark.color = new Color(_leftDragMark.color.r, _leftDragMark.color.g, _leftDragMark.color.b,
                        newValue);

                    _rightDragMark.color = new Color(_rightDragMark.color.r, _rightDragMark.color.g,
                        _rightDragMark.color.b, newValue);
                }, 1f, 0.2f).OnComplete(() =>
                {
                    _leftDragMark.raycastTarget = true;
                    _rightDragMark.raycastTarget = true;
                });
            }
        }

        private void UpdateDragRegion(int? highlightCurrentIndex)
        {
            if (highlightCurrentIndex != null && HighlightStartIndex != null && HighlightEndIndex != null)
            {
                if (_onBeginDragStartWordIndex == null)
                {
                    if (HighlightStartIndex >= highlightCurrentIndex)
                    {
                        _onBeginDragStartWordIndex = HighlightEndIndex;
                    }
                    else if (HighlightEndIndex <= highlightCurrentIndex)
                    {
                        _onBeginDragStartWordIndex = HighlightStartIndex;
                    }
                    else
                    {
                        int leftOffset = highlightCurrentIndex.Value - HighlightStartIndex.Value;
                        int rightOffset = HighlightEndIndex.Value - highlightCurrentIndex.Value;

                        if (leftOffset > rightOffset)
                        {
                            _onBeginDragStartWordIndex = HighlightStartIndex;
                        }
                        else
                        {
                            _onBeginDragStartWordIndex = HighlightEndIndex;
                        }
                    }
                }

                bool canUpdate = false;

                if (HighlightStartIndex > highlightCurrentIndex)
                {
                    HighlightStartIndex = highlightCurrentIndex;
                    HighlightEndIndex = _onBeginDragStartWordIndex;
                    canUpdate = true;
                }
                else if (HighlightEndIndex < highlightCurrentIndex)
                {
                    HighlightEndIndex = highlightCurrentIndex;
                    HighlightStartIndex = _onBeginDragStartWordIndex;
                    canUpdate = true;
                }
                else if (highlightCurrentIndex > HighlightStartIndex && highlightCurrentIndex < HighlightEndIndex)
                {
                    if (highlightCurrentIndex - HighlightStartIndex <= HighlightEndIndex - highlightCurrentIndex)
                    {
                        HighlightStartIndex = highlightCurrentIndex;
                        HighlightEndIndex = _onBeginDragStartWordIndex;
                        canUpdate = true;
                    }
                    else
                    {
                        HighlightEndIndex = highlightCurrentIndex;
                        HighlightStartIndex = _onBeginDragStartWordIndex;
                        canUpdate = true;
                    }
                }

                if (canUpdate)
                {
                    GenerateCaret(HighlightStartIndex.Value, HighlightEndIndex.Value);
                }
            }
        }

        private bool IsCurrentCursorInCaret()
        {
            if (_highlightCurrentIndex != null && HighlightStartIndex != null && HighlightEndIndex != null)
            {
                return _highlightCurrentIndex >= HighlightStartIndex && _highlightCurrentIndex <= HighlightEndIndex;
            }
            else
            {
                return false;
            }
        }

        private void GenerateCaret(int startIndex, int endIndex)
        {
            if (IsInCommonLine(startIndex, endIndex, out int lineIndex))
            {
                CaretInfo caretInfo = new CaretInfo();
                caretInfo.StartIndex = startIndex;
                caretInfo.EndIndex = endIndex;
                caretInfo.LineIndex = lineIndex;
                if (_allHighlightCarets.Count > 0)
                {
                    for (int i = _allHighlightCarets.Count - 1; i >= 0; i--)
                    {
                        int index = i;
                        if (caretInfo.LineIndex != _allHighlightCarets[i].Info.LineIndex)
                        {
                            Destroy(_allHighlightCarets[i].Mask.gameObject);
                            _allHighlightCarets.RemoveAt(i);
                        }
                    }
                }

                GenerateOneLineCaret(caretInfo);
            }
            else
            {
                List<CaretInfo> caretInfos = GetCaretInfo(startIndex, endIndex);

                if (_allHighlightCarets.Count > 0)
                {
                    for (int i = _allHighlightCarets.Count - 1; i >= 0; i--)
                    {
                        if (caretInfos.Count > 0 && !caretInfos.Any(care =>
                        {
                            return _allHighlightCarets[i].Info.LineIndex == care.LineIndex;
                        }))
                        {
                            Destroy(_allHighlightCarets[i].Mask.gameObject);
                            _allHighlightCarets.RemoveAt(i);
                        }
                    }
                }

                if (caretInfos.Count > 0)
                {
                    for (int i = 0; i < caretInfos.Count; i++)
                    {
                        GenerateOneLineCaret(caretInfos[i]);
                    }
                }
            }
            
            _leftDragMark?.transform.SetAsLastSibling();
            _rightDragMark?.transform.SetAsLastSibling();
        }

        private List<CaretInfo> GetCaretInfo(int startIndex, int endIndex)
        {
            List<CaretInfo> caretInfos = new List<CaretInfo>();

            int firstLineIndex = GetLineIndexByCharacterIndex(startIndex);
            int lastLineIndex = GetLineIndexByCharacterIndex(endIndex);

            CaretInfo firstCaretInfo = new CaretInfo();
            firstCaretInfo.StartIndex = startIndex;
            firstCaretInfo.EndIndex = textInfo.lineInfo[firstLineIndex].lastCharacterIndex;
            firstCaretInfo.LineIndex = firstLineIndex;
            caretInfos.Add(firstCaretInfo);

            for (int i = firstLineIndex + 1; i < lastLineIndex; i++)
            {
                CaretInfo caretInfo = new CaretInfo();
                caretInfo.StartIndex = textInfo.lineInfo[i].firstCharacterIndex;
                caretInfo.EndIndex = textInfo.lineInfo[i].lastCharacterIndex;
                caretInfo.LineIndex = i;
                caretInfos.Add(caretInfo);
            }

            CaretInfo lastCaretInfo = new CaretInfo();
            lastCaretInfo.StartIndex = textInfo.lineInfo[lastLineIndex].firstCharacterIndex;
            lastCaretInfo.EndIndex = endIndex;
            lastCaretInfo.LineIndex = lastLineIndex;

            caretInfos.Add(lastCaretInfo);

            return caretInfos;
        }

        private int GetLineIndexByCharacterIndex(int characterIndex)
        {
            for (int i = 0; i < textInfo.lineInfo.Length; i++)
            {
                if (textInfo.lineInfo[i].firstCharacterIndex <= characterIndex &&
                    textInfo.lineInfo[i].lastCharacterIndex >= characterIndex)
                {
                    return i;
                }
            }

            Debug.LogError($"can not find character index: {characterIndex} in any lines!");
            if (characterIndex > text.Length - 1)
            {
                return textInfo.lineInfo.Length - 1;
            }
            else
            {
                return 0;
            }
        }

        private void GenerateOneLineCaret(CaretInfo caretInfo)
        {
            Caret newCaret;
            Image caretImage;
            RectTransform caretRectTransform;
            if (!TryGetCaret(caretInfo.LineIndex, out newCaret))
            {
                newCaret = new Caret();
                GameObject caret = new GameObject("caret");
                caretRectTransform = caret.AddComponent<RectTransform>();
                caret.transform.SetParent(transform);
                caret.transform.localScale = Vector3.one;
                caretImage = caret.AddComponent<Image>();
                _allHighlightCarets.Add(newCaret);
            }
            else
            {
                caretImage = newCaret.Mask;
                caretRectTransform = caretImage.GetComponent<RectTransform>();
            }

            GetCaretCenterAndWidth(caretInfo.StartIndex, caretInfo.EndIndex, out float centerX, out float width);
            caretRectTransform.localPosition = new Vector3(centerX, GetAnchoredPositionY(caretInfo.LineIndex), 0);
            caretRectTransform.sizeDelta =
                new Vector2(width, GetLineHeight());

            caretImage.color = _selectColor;
            newCaret.Mask = caretImage;
            newCaret.Info = caretInfo;
        }

        public void ClearAllCarets()
        {
            for (int i = _allHighlightCarets.Count - 1; i >= 0; i--)
            {
                Destroy(_allHighlightCarets[i].Mask.gameObject);
            }

            _allHighlightCarets.Clear();

            HighlightStartIndex = null;
            HighlightEndIndex = null;
            HideSelectMark();
        }

        private bool TryGetCaret(int lineIndex, out Caret caret)
        {
            if (_allHighlightCarets.Count > 0)
            {
                caret = _allHighlightCarets.Find(care => { return care.Info.LineIndex == lineIndex; });

                if (caret != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                caret = null;
                return false;
            }
        }

        private void GetCaretCenterAndWidth(int startIndex, int endIndex, out float centerX, out float width)
        {
            startIndex = FindNextCharIndex(startIndex, endIndex);
            endIndex = FindPreviousCharIndex(endIndex, startIndex);
            if (startIndex == endIndex)
            {
                centerX = 0;
                width = 0;
            }
            else
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[startIndex];
                TMP_MeshInfo charMeshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];
                Vector3 worldPos = transform.TransformPoint(charMeshInfo.vertices[charInfo.vertexIndex]);
                Vector2 screenPosition = _camera.WorldToScreenPoint(worldPos);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPosition, _camera,
                    out Vector2 leftBottom);

                charInfo = textInfo.characterInfo[endIndex];
                charMeshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];
                worldPos = transform.TransformPoint(charMeshInfo.vertices[charInfo.vertexIndex + 3]);
                screenPosition = _camera.WorldToScreenPoint(worldPos);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPosition, _camera,
                    out Vector2 rightBottom);

                centerX = (leftBottom.x + rightBottom.x) / 2f;
                width = rightBottom.x - leftBottom.x;
            }
        }

        //计算出从开始字符到结束字符的四个顶点的anchored position，以及中心点位置
        private List<Vector2> GetVertices(int startIndex, int endIndex)
        {
            startIndex = FindNextCharIndex(startIndex, endIndex);
            endIndex = FindPreviousCharIndex(endIndex, startIndex);
            List<Vector2> vector2s = new List<Vector2>();
            Vector2 leftBottom;
            TMP_CharacterInfo charInfo = textInfo.characterInfo[startIndex];
            TMP_MeshInfo charMeshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];
            Vector3 worldPos = transform.TransformPoint(charMeshInfo.vertices[charInfo.vertexIndex]);
            Vector2 screenPosition = _camera.WorldToScreenPoint(worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPosition, _camera,
                out leftBottom);
            vector2s.Add(leftBottom);

            Vector2 leftTop;
            charInfo = textInfo.characterInfo[startIndex];
            charMeshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];
            worldPos = transform.TransformPoint(charMeshInfo.vertices[charInfo.vertexIndex + 1]);
            screenPosition = _camera.WorldToScreenPoint(worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPosition, _camera,
                out leftTop);
            vector2s.Add(leftTop);

            Vector2 rightTop;
            charInfo = textInfo.characterInfo[endIndex];
            charMeshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];
            worldPos = transform.TransformPoint(charMeshInfo.vertices[charInfo.vertexIndex + 2]);

            screenPosition = RectTransformUtility.WorldToScreenPoint(_camera, worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPosition, _camera,
                out rightTop);
            vector2s.Add(rightTop);

            Vector2 rightBottom;
            charInfo = textInfo.characterInfo[endIndex];
            charMeshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];
            worldPos = transform.TransformPoint(charMeshInfo.vertices[charInfo.vertexIndex + 3]);
            screenPosition = _camera.WorldToScreenPoint(worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPosition, _camera,
                out rightBottom);
            vector2s.Add(rightBottom);

            Vector2 center;
            center.x = (vector2s[0].x + vector2s[2].x) / 2f;
            float centerYLeft = (vector2s[1].y - vector2s[0].y);
            float centerYRight = (vector2s[2].y - vector2s[3].y);
            if (centerYLeft > centerYRight)
            {
                center.y = (vector2s[0].y + vector2s[1].y) / 2f;
            }
            else
            {
                center.y = (vector2s[2].y + vector2s[3].y) / 2f;
            }

            vector2s.Add(center);

            return vector2s;
        }

        private int FindNextCharIndex(int characterIndex, int limitIndex)
        {
            for (int i = characterIndex; i < limitIndex; i++)
            {
                if (!string.IsNullOrWhiteSpace(text[i].ToString()))
                {
                    return i;
                }
            }

            return characterIndex;
        }

        private int FindPreviousCharIndex(int characterIndex, int limitIndex)
        {
            for (int i = characterIndex; i > limitIndex; i--)
            {
                if (text.Length <= i)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(text[i].ToString()))
                {
                    return i;
                }
            }

            return characterIndex;
        }

        private void SelectWord(int? startIndex)
        {
            for (int i = 0; i < textInfo.wordInfo.Length; i++)
            {
                if (textInfo.wordInfo[i].firstCharacterIndex <= startIndex &&
                    textInfo.wordInfo[i].lastCharacterIndex >= startIndex)
                {
                    HighlightStartIndex = textInfo.wordInfo[i].firstCharacterIndex;
                    HighlightEndIndex = textInfo.wordInfo[i].lastCharacterIndex;
                    break;
                }
            }

            if (HighlightStartIndex != null && HighlightEndIndex != null)
            {
                GenerateCaret(HighlightStartIndex.Value, HighlightEndIndex.Value);
            }
        }

        private bool IsInCommonLine(int startIndex, int endIndex, out int lineIndex)
        {
            for (int i = 0; i < textInfo.lineCount; i++)
            {
                if (textInfo.lineInfo[i].firstCharacterIndex <= startIndex &&
                    textInfo.lineInfo[i].lastCharacterIndex >= endIndex)
                {
                    lineIndex = i;
                    return true;
                }
            }

            lineIndex = -1;
            return false;
        }
    }
}