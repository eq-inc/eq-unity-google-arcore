using Eq.Unity;
using GoogleARCore;
using GoogleARCore.CrossPlatform;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestCloudAnchor : BaseAndroidMainController
{
    private static readonly float AnimationInterval = 5f;
    private static readonly float MinimumAnimationInterval = 1f / 5;

    public GameObject mButtonGroupCanvasGO;
    public GameObject mListCloudAnchorPanelGO;
    public GameObject mAddCloudAnchorPanelGO;
    public GameObject mAnchorPrefab = null;
    public GameObject mListItemPrefab = null;

    private Dictionary<string, Anchor> mAnchorDic = new Dictionary<string, Anchor>();
    private Dictionary<string, XPAnchor> mCloudAnchorDic = new Dictionary<string, XPAnchor>();
    private Dictionary<string, GameObject> mCloudAnchorGODic = new Dictionary<string, GameObject>();
    private InputField mIfCloudAnchorId;
    private float mLeftAnimationInterval = 0f;
    private float mLeftOneAnimationInterval = 0f;
    private bool mUpdateAnchorsPosition = false;
    System.Random mRandom = new System.Random();

    internal override void Start()
    {
        base.Start();
    }

    internal override void Update()
    {
        base.Update();

        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }

        if ((mListCloudAnchorPanelGO.activeInHierarchy == false) && (mAddCloudAnchorPanelGO.activeInHierarchy == false))
        {
            Touch lastTouched;
            if (LastTouch(out lastTouched))
            {
                if (lastTouched.phase == TouchPhase.Ended)
                {
                    TrackableHit hit;
                    if (Frame.Raycast(lastTouched.position.x, lastTouched.position.y, TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.FeaturePointWithSurfaceNormal, out hit))
                    {
                        // Anchorを生成
                        Anchor anchor = Session.CreateAnchor(hit.Pose, hit.Trackable);

                        GameObject anchorGO = Instantiate(mAnchorPrefab, anchor.transform.position, anchor.transform.rotation);
                        anchorGO.SetActive(true);
                        anchorGO.transform.parent = anchor.transform;

                        // CloudAnchorを生成
                        XPSession.CreateCloudAnchor(anchor).ThenAction(delegate (CloudAnchorResult result)
                        {
                            XPAnchor xpAnchor = result.Anchor;

                            if (xpAnchor != null)
                            {
                                AddAnchorGO(xpAnchor, anchorGO, anchor);
                            }
                        });
                    }
                }
            }
        }

        // アニメーション
        bool enableAnimation = false;
        if (mLeftAnimationInterval > 0)
        {
            mLeftAnimationInterval -= Time.deltaTime;
            mLeftOneAnimationInterval -= Time.deltaTime;

            if(mLeftOneAnimationInterval <= 0)
            {
                enableAnimation = true;
                mLeftOneAnimationInterval = MinimumAnimationInterval;
            }
        }

        // 表示位置の更新
        if(mCloudAnchorDic.Count > 0)
        {
            foreach (string cloudAnchorId in mAnchorDic.Keys)
            {
                Anchor anchor = mAnchorDic[cloudAnchorId];
                GameObject cloudAnchorGO = null;

                if (enableAnimation)
                {
                    int direction = (mRandom.Next() * mRandom.Next()) % 3;
                    double distanceSource = mRandom.NextDouble() * mRandom.NextDouble();
                    float distance = ((float)(distanceSource - System.Math.Floor(distanceSource)) / 1);
                    if (direction == 0)
                    {
                        anchor.transform.position = new Vector3(anchor.transform.position.x + distance, anchor.transform.position.y, anchor.transform.position.z);
                    }
                    else if (direction == 1)
                    {
                        anchor.transform.position = new Vector3(anchor.transform.position.x, anchor.transform.position.y + distance, anchor.transform.position.z);
                    }
                    else
                    {
                        anchor.transform.position = new Vector3(anchor.transform.position.x, anchor.transform.position.y, anchor.transform.position.z + distance);
                    }
                }

                if ((enableAnimation || mUpdateAnchorsPosition) && mCloudAnchorGODic.TryGetValue(cloudAnchorId, out cloudAnchorGO))
                {
                    cloudAnchorGO.transform.SetPositionAndRotation(anchor.transform.position, anchor.transform.rotation);

                    mUpdateAnchorsPosition = false;
                }
            }
        }
    }

    public void RestoreAnchorsButtonClicked()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        int anchorCount = CloudAnchorManager.Count;
        string anchorId = null;

        mCloudAnchorDic.Clear();
        for (int i = 0; i < anchorCount; i++)
        {
            if (CloudAnchorManager.GetCloudAnchorId(i, out anchorId))
            {
                XPSession.ResolveCloudAnchor(anchorId).ThenAction(
                    delegate (CloudAnchorResult result)
                    {
                        XPAnchor xpAnchor = result.Anchor;
                        if (xpAnchor != null)
                        {
                            AddAnchorGO(xpAnchor);
                        }
                    }
                );
            }
        }
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void ListCloudAnchorButtonClicked()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        mListCloudAnchorPanelGO.SetActive(true);

        GameObject cloudAnchorListContentGO = mListCloudAnchorPanelGO.transform.Find("CloudAnchorList/Viewport/Content").gameObject;

        // 一旦子要素を全て解放
        string cloudAnchorId = null;
        for (int i = 0, size = cloudAnchorListContentGO.transform.childCount; i < size; i++)
        {
            GameObject childGO = cloudAnchorListContentGO.transform.GetChild(i).gameObject;
            Destroy(childGO);
        }
        cloudAnchorListContentGO.transform.DetachChildren();

        int cloudAnchorCount = CloudAnchorManager.Count;

        // Scroll Viewの設定
        float itemHeight = mListItemPrefab.GetComponent<LayoutElement>().preferredHeight;
        float itemVerticalSpace = cloudAnchorListContentGO.GetComponent<VerticalLayoutGroup>().spacing;
        cloudAnchorListContentGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, (itemHeight + itemVerticalSpace) * cloudAnchorCount);

        for (int i = 0, size = CloudAnchorManager.Count; i < size; i++)
        {
            if (CloudAnchorManager.GetCloudAnchorId(i, out cloudAnchorId))
            {
                XPSession.ResolveCloudAnchor(cloudAnchorId).ThenAction((CloudAnchorResult result) =>
                {
                    XPAnchor xpAnchor = result.Anchor;

                    if (xpAnchor != null)
                    {
                        // リストに追加
                        GameObject listItemGO = Instantiate(mListItemPrefab);
                        //listItemGO.transform.Find("CloudAnchorId").gameObject.GetComponent<UnityEngine.UI.Text>().text = xpAnchor.CloudId;
                        mLogger.CategoryLog(LogCategoryMethodTrace, "cloud anchor id = " + xpAnchor.CloudId);
                        listItemGO.GetComponent<UnityEngine.UI.Text>().text = xpAnchor.CloudId;
                        listItemGO.transform.SetParent(cloudAnchorListContentGO.transform);
                    }
                });
            }
        }
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void ClosePanelCloudAnchorListButtonClicked()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        mListCloudAnchorPanelGO.SetActive(false);
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    private void ReconstructAddCloudAnchorPanel()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        if (mAddCloudAnchorPanelGO.activeInHierarchy)
        {
            if (string.IsNullOrEmpty(mIfCloudAnchorId.text))
            {
                // 入力されていないので、追加ボタンは無効化
                mAddCloudAnchorPanelGO.transform.Find("AddCloudAnchorButton").gameObject.SetActive(false);
            }
            else
            {
                // 入力されているので、追加ボタンは有効化
                mAddCloudAnchorPanelGO.transform.Find("AddCloudAnchorButton").gameObject.SetActive(true);
            }
        }
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void AddCloudAnchorButtonClicked()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        mAddCloudAnchorPanelGO.SetActive(true);

        if (mIfCloudAnchorId == null)
        {
            mIfCloudAnchorId = mAddCloudAnchorPanelGO.transform.Find("CloudAnchorInputField").gameObject.GetComponent<UnityEngine.UI.InputField>();
        }

        mIfCloudAnchorId.text = "";
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void AddPanelCloudAnchorButtonClicked()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        mAddCloudAnchorPanelGO.SetActive(false);

        string inputAnchorId = mIfCloudAnchorId.text;

        if (!string.IsNullOrEmpty(inputAnchorId))
        {
            XPSession.ResolveCloudAnchor(inputAnchorId).ThenAction((CloudAnchorResult result) =>
            {
                XPAnchor xpAnchor = result.Anchor;
                if (xpAnchor != null)
                {
                    AddAnchorGO(xpAnchor);
                }
            });
        }
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void CancelPanelCloudAnchorButtonClicked()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        mAddCloudAnchorPanelGO.SetActive(false);
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void MoveAnchorsButtonClicked()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        mLeftAnimationInterval = AnimationInterval;
        mLeftOneAnimationInterval = 0;
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    public void UpdateAnchorsButtonClicked()
    {
        mLogger.CategoryLog(LogCategoryMethodIn);
        mUpdateAnchorsPosition = true;
        mLogger.CategoryLog(LogCategoryMethodOut);
    }

    private void AddAnchorGO(XPAnchor xpAnchor, GameObject anchorGO = null, Anchor anchor = null)
    {
        mLogger.CategoryLog(LogCategoryMethodIn);

        if (anchorGO != null || !mCloudAnchorGODic.TryGetValue(xpAnchor.CloudId, out anchorGO))
        {
            if (anchorGO == null)
            {
                anchorGO = Instantiate(mAnchorPrefab, xpAnchor.transform.position, xpAnchor.transform.rotation);
            }
            anchorGO.SetActive(true);

            mAnchorDic[xpAnchor.CloudId] = anchor;
            mCloudAnchorDic[xpAnchor.CloudId] = xpAnchor;
            mCloudAnchorGODic[xpAnchor.CloudId] = anchorGO;
            CloudAnchorManager.Append(xpAnchor.CloudId);
        }
        mLogger.CategoryLog(LogCategoryMethodOut);
    }
}
