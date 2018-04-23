using Eq.Unity;
using GoogleARCore;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using UnityEditor;
#endif

public class TestEnvironmentalLight : BaseAndroidMainController {
    public GameObject mAndroidRobotPrefab;
    public GameObject mTrackedPlanePrefab;
    private UnityEngine.UI.Dropdown mTrackableHitFlagsFeatureDd;
    private UnityEngine.UI.Dropdown mTrackableHitFlagsPlaneWithDd;

    // Use this for initialization
    internal override void Start()
    {
        base.Start();

        mLogger.CategoryLog(LogCategoryMethodIn);
        GameObject trackableHitFlagsFeatureGO = GameObject.Find("TrackableHitFlagsFeature");
        GameObject trackableHitFlagsPlaneWithGO = GameObject.Find("TrackableHitFlagsPlaneWith");
        mTrackableHitFlagsFeatureDd = trackableHitFlagsFeatureGO.GetComponent<UnityEngine.UI.Dropdown>();
        mTrackableHitFlagsPlaneWithDd = trackableHitFlagsPlaneWithGO.GetComponent<UnityEngine.UI.Dropdown>();

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        GameObject arcoreDeviceGO = GameObject.Find("ARCore Device");
        ARCoreSessionConfig config = arcoreDeviceGO.GetComponent<ARCoreSession>().SessionConfig;
        config.EnablePlaneFinding = true;
        config.EnableLightEstimation = true;
        config.MatchCameraFramerate = true;
#if UNITY_EDITOR
        EditorUtility.SetDirty(config);
#endif

        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    // Update is called once per frame
    internal override void Update()
    {
        base.Update();

        if(Session.Status == SessionStatus.Tracking)
        {
            mTrackableHitFlagsFeatureDd.enabled = true;
            mTrackableHitFlagsPlaneWithDd.enabled = true;

            if (Screen.sleepTimeout != SleepTimeout.NeverSleep)
            {
                mLogger.CategoryLog(LogCategoryMethodTrace, "session timeout = " + Screen.sleepTimeout);
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            List<TrackedPlane> newTrackedPointList = new List<TrackedPlane>();
            Session.GetTrackables<TrackedPlane>(newTrackedPointList, TrackableQueryFilter.New);

            mLogger.CategoryLog(LogCategoryMethodTrace, newTrackedPointList.Count + " tracked point discovered");
            foreach (TrackedPlane newTrackedPoint in newTrackedPointList)
            {
                GameObject trackabledGO = Instantiate(mTrackedPlanePrefab, newTrackedPoint.CenterPose.position, newTrackedPoint.CenterPose.rotation);
                trackabledGO.SetActive(true);
            }

            List<TrackedPlane> allTrackedPointList = new List<TrackedPlane>();
            Session.GetTrackables<TrackedPlane>(newTrackedPointList, TrackableQueryFilter.All);
            mLogger.CategoryLog(LogCategoryMethodTrace, "all discovered point is " + allTrackedPointList.Count);

            Touch lastTouch;
            if (LastTouch(out lastTouch))
            {
                mLogger.CategoryLog(LogCategoryMethodTrace, "touched");
                TrackableHit hitTrackable;
                if (SearchHitTrackable(lastTouch.position.x, lastTouch.position.y, out hitTrackable))
                {
                    mLogger.CategoryLog(LogCategoryMethodTrace, "hit trackable");
                    GameObject androidRobotGO = Instantiate(mAndroidRobotPrefab, hitTrackable.Pose.position, hitTrackable.Pose.rotation);
                    androidRobotGO.SetActive(true);
                }
            }
            else
            {
                mLogger.CategoryLog(LogCategoryMethodTrace, "not touched");
            }
        }
        else
        {
            mLogger.CategoryLog(LogCategoryMethodTrace, "not tracking");
            mTrackableHitFlagsFeatureDd.enabled = false;
            mTrackableHitFlagsPlaneWithDd.enabled = false;
        }
    }

    private bool SearchHitTrackable(float worldPositionX, float worldPositionY, out TrackableHit hitTrackable)
    {
        bool ret = false;
        TrackableHitFlags flags = TrackableHitFlags.None;
        TrackableHitFlags featureFlag;
        TrackableHitFlags planeWithFlag;

        string currentFeatureCaptionText = mTrackableHitFlagsFeatureDd.captionText.text;
        FieldInfo featureFieldInfo = typeof(TrackableHitFlags).GetField(currentFeatureCaptionText, System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Static);
        if (featureFieldInfo != null)
        {
            featureFlag = (TrackableHitFlags)featureFieldInfo.GetValue(null);
            mLogger.CategoryLog(LogCategoryMethodTrace, "featureFlag=" + featureFlag.ToString());
        }
        else
        {
            featureFlag = TrackableHitFlags.None;
        }

        string currentPlaneWithCaptionText = mTrackableHitFlagsPlaneWithDd.captionText.text;
        FieldInfo planeWithFieldInfo = typeof(TrackableHitFlags).GetField(currentPlaneWithCaptionText, System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Static);
        if (planeWithFieldInfo != null)
        {
            planeWithFlag = (TrackableHitFlags)planeWithFieldInfo.GetValue(null);
            mLogger.CategoryLog(LogCategoryMethodTrace, "planeWithFlag=" + planeWithFlag.ToString());
        }
        else
        {
            planeWithFlag = TrackableHitFlags.None;
        }

        if(featureFlag == TrackableHitFlags.None && planeWithFlag == TrackableHitFlags.None)
        {
            flags = TrackableHitFlags.None;
        }
        else
        {
            if(featureFlag != TrackableHitFlags.None)
            {
                flags = featureFlag;
            }
            if(planeWithFlag != TrackableHitFlags.None)
            {
                if(flags == TrackableHitFlags.None)
                {
                    flags = planeWithFlag;
                }
                else
                {
                    flags |= planeWithFlag;
                }
            }
        }

        return Frame.Raycast(worldPositionX, worldPositionY, flags, out hitTrackable);
    }
}
