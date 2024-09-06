using Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Square : MonoBehaviour
{
    public enum TargetingScheme
	{
        Self,
        Left,
        Right,
        SelfLeft,
        SelfRight,
        LeftRight,
        SelfLeftRight
	}

    [SerializeField] private GameObject _outline = null;
    [SerializeField] private Color _clickedOutlineColor = Color.black;
    [SerializeField] private Sprite[] _targetSchemeSprites = null;
    [SerializeField] private Color _toggledColor = Color.black;
    [SerializeField] private float _shakeTime = 0f;
    [SerializeField] private float _shakeStrength = 0f;
    [SerializeField] private int _shakeVibrato = 0;
    [SerializeField] private float _shakeRandomness = 0f;
    [SerializeField] private bool _shakeSnapping = false;
    [SerializeField] private bool _shakeFadeOut = false;

    public bool Interactable { get; set; }
    public bool Highlighted { get; set; }
    public bool Toggled { get; set; }
    public bool SolutionSquare { get; set; }
    public TargetingScheme TargetScheme { get; set; }

    private int _id;
    //private Level _level;
    private Color _normalColor;
    private Color _normalOutlineColor;
    private Rectangle _rectangle;
    private Rectangle _outlineRectangle;
    private SpriteRenderer _targetIndicator;

	public void Initialize(int id/*, Level level*/, Square referenceSquare = null)
	{
        SolutionSquare = referenceSquare != null;

        _rectangle = GetComponent<Rectangle>();
        _normalColor = _rectangle.Color;

        _id = id;
        //_level = level;

        gameObject.name = $"{(SolutionSquare ? "Solution" : "")}Square({_id})";

        if (!SolutionSquare)
		{
            _outlineRectangle = _outline.GetComponent<Rectangle>();
            _normalOutlineColor = _outlineRectangle.Color;

            _outline.SetActive(false);

            //TargetScheme = (TargetingScheme)Random.Range(0, System.Enum.GetValues(typeof(TargetingScheme)).Length);
            TargetScheme = TargetingScheme.Self;

            _targetIndicator = GetComponentInChildren<SpriteRenderer>(true);
            _targetIndicator.sprite = _targetSchemeSprites[(int)TargetScheme];

            if (Random.Range(0f, 1f) > 0.5f)
            {
                Toggle();
            }
        }
		else
		{
            TargetScheme = referenceSquare.TargetScheme;

            Toggle(referenceSquare.Toggled);
        }

        Interactable = true;
    }

    public void OnMouseOverEnter(bool showOutline)
    {
        if(showOutline && !_outline.activeSelf)
		{
            _outline.SetActive(true);
        }
        
        Highlighted = true;
    }

    public void OnMouseOverExit()
    {
        if(_outline.activeSelf)
		{
            _outline.SetActive(false);
        }
        
        Highlighted = false;
    }

    public void OnMouseClickDown()
	{
        _outlineRectangle.Color = _clickedOutlineColor;
    }

    public void OnMouseClickUp()
    {
        _outlineRectangle.Color = _normalOutlineColor;
    }

    public void ToggleTargets(Square[] targetArray)
	{
        var targets = new List<Square>();

		switch (TargetScheme)
		{
			case TargetingScheme.Self:
                targets.Add(this);
                break;
			case TargetingScheme.Left:
                if(_id > 0) targets.Add(targetArray[_id - 1]);
                break;
			case TargetingScheme.Right:
                if (_id < targetArray.Length - 1) targets.Add(targetArray[_id + 1]);
                break;
			case TargetingScheme.SelfLeft:
                targets.Add(this);
                if (_id > 0) targets.Add(targetArray[_id - 1]);
                break;
			case TargetingScheme.SelfRight:
                targets.Add(this);
                if (_id < targetArray.Length - 1) targets.Add(targetArray[_id + 1]);
                break;
			case TargetingScheme.LeftRight:
                if (_id > 0) targets.Add(targetArray[_id - 1]);
                if (_id < targetArray.Length - 1) targets.Add(targetArray[_id + 1]);
                break;
			case TargetingScheme.SelfLeftRight:
                targets.Add(this);
                if (_id > 0) targets.Add(targetArray[_id - 1]);
                if (_id < targetArray.Length - 1) targets.Add(targetArray[_id + 1]);
                break;
		}

        foreach(var target in targets)
		{
            target.Toggle();
        }
	}

	public void Toggle()
	{
        Toggled = !Toggled;

        _rectangle.Color = Toggled 
            ? _toggledColor
            : _normalColor;
    }

    public void Toggle(bool toggle)
    {
        Toggled = toggle;

        _rectangle.Color = Toggled
            ? _toggledColor
            : _normalColor;
    }

    public void Shake()
    {
        ChangeSortingOrderOfComponents(10);

        transform.DOShakePosition(
            _shakeTime,
            _shakeStrength,
            _shakeVibrato,
            _shakeRandomness,
            _shakeSnapping,
            _shakeFadeOut
        ).OnComplete(() =>
		{
            ChangeSortingOrderOfComponents(-10);
        });

        var originalColor = _rectangle.Color;
        var rectangleColor = _rectangle.Color;

        DOTween.To(() => rectangleColor, x =>
        {
            rectangleColor = x;

            _rectangle.Color = rectangleColor;
        },
        Color.red,
        _shakeTime / 2f).OnComplete(() =>
		{
            var rectangleColorDown = _rectangle.Color;

            DOTween.To(() => rectangleColor, x =>
            {
                rectangleColor = x;

                _rectangle.Color = rectangleColor;
            },
            originalColor,
            _shakeTime / 2f);
        });
    }

    private void ChangeSortingOrderOfComponents(int factor)
	{
        var components = GetComponentsInChildren<Rectangle>(true);

        foreach(var component in components)
		{
            component.SortingOrder += factor;
        }

        _targetIndicator.sortingOrder += factor;
    }
}