using Eq.Unity;
using GoogleARCore;

public class TrackedImageController : BaseAndroidBehaviour
{
    public AugmentedImage mTargetImage;
    private Anchor mTargetImageAnchor;

    private void Update()
    {
        if (mTargetImage != null)
        {
            if (mTargetImageAnchor == null)
            {
                mTargetImageAnchor = mTargetImage.CreateAnchor(mTargetImage.CenterPose);
            }

            if (mTargetImage.TrackingState == TrackingState.Tracking)
            {
                if (mTargetImageAnchor != null)
                {
                    gameObject.transform.SetPositionAndRotation(mTargetImageAnchor.transform.position, mTargetImageAnchor.transform.rotation);
                    gameObject.transform.localScale = new UnityEngine.Vector3(mTargetImage.ExtentX, 0.001f, mTargetImage.ExtentZ);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (mTargetImageAnchor != null)
        {
            Destroy(mTargetImageAnchor);
        }
    }
}
