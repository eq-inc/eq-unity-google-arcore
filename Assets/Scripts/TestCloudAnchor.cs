using Eq.Unity;
using GoogleARCore;
using GoogleARCore.CrossPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TestCloudAnchor : BaseAndroidMainController
{
    public GameObject mAnchorPrefab = null;
    private Dictionary<string, XPAnchor> mCloudAnchorDic = new Dictionary<string, XPAnchor>();

    internal override void Start()
    {
        base.Start();
    }

    internal override void Update()
    {
        base.Update();

        if(Session.Status != SessionStatus.Tracking)
        {
            return;
        }

        Touch lastTouched;
        if(LastTouch(out lastTouched))
        {
            if(lastTouched.phase == TouchPhase.Ended)
            {
                TrackableHit hit;
                if (Frame.Raycast(lastTouched.position.x, lastTouched.position.y, TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.FeaturePointWithSurfaceNormal, out hit))
                {
                    // Anchorを生成
                    Anchor anchor = Session.CreateAnchor(hit.Pose, hit.Trackable);

                    // CloudAnchorを生成
                    XPSession.CreateCloudAnchor(anchor).ThenAction(delegate(CloudAnchorResult result)
                    {
                        XPAnchor xpAnchor = result.Anchor;

                        if(xpAnchor != null)
                        {

                            mCloudAnchorDic[xpAnchor.CloudId] = xpAnchor;
                        }
                    });
                }
            }
        }
    }

    public void RestoreAnchorsButtonClicked()
    {
        int anchorCount = CloudAnchorManager.Count;
        string anchorId = null;

        mCloudAnchorDic.Clear();
        for (int i=0; i<anchorCount; i++)
        {
            if(CloudAnchorManager.GetCloudAnchorId(i, out anchorId))
            {
                XPSession.ResolveCloudAnchor(anchorId).ThenAction(
                    delegate (CloudAnchorResult result)
                    {
                        XPAnchor anchor = result.Anchor;
                        mCloudAnchorDic[anchor.CloudId] = anchor;
                    }
                );
            }
        }
    }
}


