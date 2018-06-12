using Eq.Unity;

public class TopMenuController : BaseAndroidMainController {

    // Use this for initialization
    internal override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    internal override void Update()
    {
        base.Update();
    }

    public void OnTestInstantPreviewForTouchClicked()
    {
        PushNextScene("TestInstantPreviewForTouch");
    }

    public void OnTestBaseEnvironmentClicked()
    {
        PushNextScene("TestBaseEnvironment");
    }

    public void OnTestEnvironmentalLightClicked()
    {
        PushNextScene("TestEnvironmentalLight");
    }

    public void OnTestPointCloudClicked()
    {
        PushNextScene("TestPointCloud");
    }

    public void OnTestHelloARClicked()
    {
        PushNextScene("HelloAR");
    }

    public void OnTestComputerVisionClicked()
    {
        PushNextScene("ComputerVision");
    }

    public void OnTestCloudAnchorClicked()
    {
        PushNextScene("CloudAnchor");
    }

    public void OnTestAugmentedImageClicked()
    {
        PushNextScene("AugmentedImage");
    }

    public void OnTestMeasureClicked()
    {
        PushNextScene("TestMeasure");
    }
}