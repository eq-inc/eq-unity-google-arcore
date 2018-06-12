using Eq.Unity;
using GoogleARCore;
using UnityEngine;
#if UNITY_EDITOR
using EditorInput = UnityEngine.Input;
#endif

public class TestMeasure : BaseAndroidMainController
{
    private static readonly TrackableHit EmptyTrackableHit;

    private TrackableHit[] mTrackableHitArray = new TrackableHit[] {
        EmptyTrackableHit,
        EmptyTrackableHit
    };
    private GameObject[] mHitPointGO = null;
    private GameObject mBindingTouchLine;
    private UnityEngine.UI.Text mDistanceText;
    private UnityEngine.UI.Text mPointCountText;
    public GameObject mTrackableHitPrefab;

    internal override void Start()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        base.Start();
        mHitPointGO = new GameObject[mTrackableHitArray.Length];
        mBindingTouchLine = GameObject.Find("BindingTouchLine");
        mBindingTouchLine.SetActive(false);

        mDistanceText = GameObject.Find("DistanceText").GetComponent<UnityEngine.UI.Text>();
        mPointCountText = GameObject.Find("PointCountText").GetComponent<UnityEngine.UI.Text>();
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    internal override void Update()
    {
        base.Update();

        mPointCountText.text = "Point: " + Frame.PointCloud.PointCount;

        Touch lastTouch;
        if (LastTouch(out lastTouch))
        {
            if (lastTouch.phase == TouchPhase.Ended)
            {
                TrackableHit hit;
                if (Frame.Raycast(lastTouch.position.x, lastTouch.position.y, TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.FeaturePointWithSurfaceNormal, out hit))
                {
                    if (IsFullOfTrackableHit())
                    {
                        InitializeTrackableHit();
                    }

                    if (AppendTrackableHit(hit))
                    {
                        // 追加出来たので、2点揃っているか確認
                        if (IsFullOfTrackableHit())
                        {
                            // 2点間の線を描画するのと、距離を算出
                            mBindingTouchLine.SetActive(true);
                            LineRenderer lineRenderer = mBindingTouchLine.GetComponent<LineRenderer>();
                            if(lineRenderer != null)
                            {
                                lineRenderer.SetPosition(0, mHitPointGO[0].transform.position);
                                lineRenderer.SetPosition(1, mHitPointGO[1].transform.position);
                            }

                            float distance = Vector3.Distance(mHitPointGO[0].transform.position, mHitPointGO[1].transform.position);
                            mDistanceText.text = distance.ToString() + " m";
                        }
                    }
                }
            }
        }
    }

    private void InitializeTrackableHit()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        for (int i = 0, size = mTrackableHitArray.Length; i < size; i++)
        {
            mTrackableHitArray[i] = EmptyTrackableHit;

            if(mHitPointGO[i] != null)
            {
                Destroy(mHitPointGO[i]);
                mHitPointGO[i] = null;
            }
        }
        mDistanceText.text = "";
        mBindingTouchLine.SetActive(false);
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    private bool IsFullOfTrackableHit()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        bool isFull = true;

        foreach(TrackableHit trackableHit in mTrackableHitArray)
        {
            if (trackableHit.Equals(EmptyTrackableHit))
            {
                isFull = false;
                break;
            }
        }

        mLogger.CategoryLog(LogCategoryMethodOut, "ret = " + isFull);
        return isFull;
    }

    private bool AppendTrackableHit(TrackableHit trackableHit)
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        bool ret = false;

        for (int i = 0, size = mTrackableHitArray.Length; i < size; i++)
        {
            if (mTrackableHitArray[i].Equals(EmptyTrackableHit))
            {
                mTrackableHitArray[i] = trackableHit;
                mHitPointGO[i] = Instantiate(mTrackableHitPrefab, trackableHit.Pose.position, trackableHit.Pose.rotation);
                mHitPointGO[i].SetActive(true);
                ret = true;
                break;
            }
        }

        mLogger.CategoryLog(LogCategoryMethodOut, "ret = " + ret);
        return ret;
    }
}
