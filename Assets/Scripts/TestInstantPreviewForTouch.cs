using Eq.Unity;
using UnityEngine;
#if UNITY_EDITOR
using EditorInput = UnityEngine.Input;
#endif

public class TestInstantPreviewForTouch : BaseAndroidMainController {
    private static readonly string TouchPositionFmt = "last touched position(XY)=({0}, {1})";
    private UnityEngine.UI.Text mLastTouchedPositionText;
    private UnityEngine.UI.Text mLastButtonClickedText;

    // Use this for initialization
    internal override void Start()
    {
        base.Start();
        mLastTouchedPositionText = GameObject.Find("TouchPosition").GetComponent<UnityEngine.UI.Text>();
        mLastButtonClickedText = GameObject.Find("ButtonClicked").GetComponent<UnityEngine.UI.Text>();
    }

    // Update is called once per frame
    internal override void Update()
    {
        base.Update();

        Touch lastTouch;
        if(LastTouch(out lastTouch))
        {
            UpdateTouchPosition(lastTouch);
        }
    }

    public void BackClicked()
    {
        PopCurrentScene();
    }

    public void ButtonLeftTopClicked()
    {
#if UNITY_EDITOR
        if (EditorInput.touchCount > 0)
        {
            UpdateTouchPosition(EditorInput.GetTouch(0));
        }
#endif
        mLastButtonClickedText.text = "ButtonLeftTop is clicked";
    }

    public void ButtonLeftBottomClicked()
    {
#if UNITY_EDITOR
        if (EditorInput.touchCount > 0)
        {
            UpdateTouchPosition(EditorInput.GetTouch(0));
        }
#endif
        mLastButtonClickedText.text = "ButtonLeftBottom is clicked";
    }

    public void ButtonRightTopClicked()
    {
#if UNITY_EDITOR
        if (EditorInput.touchCount > 0)
        {
            UpdateTouchPosition(EditorInput.GetTouch(0));
        }
#endif
        mLastButtonClickedText.text = "ButtonRightTop is clicked";
    }

    public void ButtonRightBottomClicked()
    {
#if UNITY_EDITOR
        if (EditorInput.touchCount > 0)
        {
            UpdateTouchPosition(EditorInput.GetTouch(0));
        }
#endif
        mLastButtonClickedText.text = "ButtonRightBottom is clicked";
    }

    public void ButtonCenterClicked()
    {
#if UNITY_EDITOR
        if(EditorInput.touchCount > 0)
        {
            UpdateTouchPosition(EditorInput.GetTouch(0));
        }
#endif
        mLastButtonClickedText.text = "ButtonCenter is clicked";
    }

    private void UpdateTouchPosition(Touch lastTouch)
    {
        mLastTouchedPositionText.text = string.Format(TouchPositionFmt, (int)lastTouch.position.x, (int)lastTouch.position.y);
    }
}
