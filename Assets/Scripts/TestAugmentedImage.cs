using Eq.Unity;
using GoogleARCore;
using System.Collections.Generic;
using UnityEngine;

public class TestAugmentedImage : BaseAndroidMainController
{
    private List<AugmentedImage> mAugImageList = new List<AugmentedImage>();
    private Dictionary<int, GameObject> mAugImageDic = new Dictionary<int, GameObject>();
    private Dictionary<int, Anchor> mAnchorDic = new Dictionary<int, Anchor>();
    public GameObject mMarkerGO;

    internal override void Start()
    {
        base.Start();
    }

    internal override void Update()
    {
        base.Update();

        mLogger.CategoryLog(LogCategoryMethodTrace, "Session.Status = " + Session.Status);
        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }

        Session.GetTrackables<AugmentedImage>(mAugImageList);

        mLogger.CategoryLog(LogCategoryMethodTrace, "trackable count = " + mAugImageList.Count);
        foreach(AugmentedImage augImage in mAugImageList)
        {
            mLogger.CategoryLog(LogCategoryMethodTrace, "database index = " + augImage.DatabaseIndex + ", TrackingState = " + augImage.TrackingState);

            GameObject markerGO;
            if ((augImage.TrackingState == TrackingState.Tracking) && !mAugImageDic.TryGetValue(augImage.DatabaseIndex, out markerGO))
            {
                mLogger.CategoryLog(LogCategoryMethodTrace, "create game object: database index = " + augImage.DatabaseIndex);
                markerGO = Instantiate(mMarkerGO);
                markerGO.GetComponent<TrackedImageController>().mTargetImage = augImage;
                markerGO.SetActive(true);

                mAugImageDic[augImage.DatabaseIndex] = markerGO;
            }
            else if (augImage.TrackingState == TrackingState.Stopped && mAugImageDic.TryGetValue(augImage.DatabaseIndex, out markerGO))
            {
                mLogger.CategoryLog(LogCategoryMethodTrace, "remove game object: database index = " + augImage.DatabaseIndex);
                mAugImageDic.Remove(augImage.DatabaseIndex);
                Destroy(markerGO);
            }
        }
    }
}
