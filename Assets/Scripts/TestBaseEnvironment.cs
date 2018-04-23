using Eq.Unity;
using GoogleARCore;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;
#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class TestBaseEnvironment : BaseAndroidMainController {

    // Use this for initialization
    internal override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    internal override void Update()
    {
        base.Update();

        mLogger.CategoryLog(LogCategoryMethodIn, "Session Status=" + Session.Status.ToString() + ", status is " + (Session.Status.IsValid() ? "valid" : "invalid"));

        switch (Session.Status)
        {
            case SessionStatus.None:
                break;
            case SessionStatus.Initializing:
                break;
            case SessionStatus.Tracking:
                break;
            case SessionStatus.LostTracking:
                break;
            case SessionStatus.NotTracking:
                break;
            case SessionStatus.FatalError:
                break;
            case SessionStatus.ErrorApkNotAvailable:
                break;
            case SessionStatus.ErrorPermissionNotGranted:
                break;
            case SessionStatus.ErrorSessionConfigurationNotSupported:
                break;
        }

        mLogger.CategoryLog(LogCategoryMethodTrace, "pose position=" + Frame.Pose.position + ", rotation=" + Frame.Pose.rotation + ", up=" + Frame.Pose.up + ", right=" + Frame.Pose.right);
        mLogger.CategoryLog(LogCategoryMethodOut);
    }
}
