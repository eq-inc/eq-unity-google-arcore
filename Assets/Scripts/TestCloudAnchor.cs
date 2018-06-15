using Eq.Unity;
using GoogleARCore;
using GoogleARCore.CrossPlatform;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestCloudAnchor : BaseAndroidMainController
{
    public GameObject mButtonGroupCanvasGO;
    public GameObject mListCloudAnchorPanelGO;
    public GameObject mAddCloudAnchorPanelGO;
    public GameObject mAnchorPrefab = null;
    public GameObject mListItemPrefab = null;

    private Dictionary<string, XPAnchor> mCloudAnchorDic = new Dictionary<string, XPAnchor>();
    private Dictionary<string, GameObject> mCloudAnchorGODic = new Dictionary<string, GameObject>();
    private UnityEngine.UI.InputField mIfCloudAnchorId;

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

        if((mListCloudAnchorPanelGO.activeInHierarchy == false) && (mAddCloudAnchorPanelGO.activeInHierarchy == false))
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

                        mLogger.CategoryLog(LogCategoryMethodTrace, "create local anchor");

                        // CloudAnchorを生成
                        XPSession.CreateCloudAnchor(anchor).ThenAction(delegate (CloudAnchorResult result)
                        {
                            XPAnchor xpAnchor = result.Anchor;

                            if (xpAnchor != null)
                            {
                                mLogger.CategoryLog(LogCategoryMethodTrace, "create cloud anchor: id = " + xpAnchor.CloudId);
                                AddAnchorGO(xpAnchor, anchorGO);
                            }
                        });
                    }
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
        for (int i=0; i<anchorCount; i++)
        {
            if(CloudAnchorManager.GetCloudAnchorId(i, out anchorId))
            {
                XPSession.ResolveCloudAnchor(anchorId).ThenAction(
                    delegate (CloudAnchorResult result)
                    {
                        XPAnchor xpAnchor = result.Anchor;
                        if(xpAnchor != null)
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
        for(int i=0, size= cloudAnchorListContentGO.transform.childCount; i<size; i++)
        {
            GameObject childGO = cloudAnchorListContentGO.transform.GetChild(i).gameObject;
            Destroy(childGO);
        }
        cloudAnchorListContentGO.transform.DetachChildren();

        int cloudAnchorCount = CloudAnchorManager.Count;
        mLogger.CategoryLog(LogCategoryMethodTrace, "CloudAnchorManager.Count = " + cloudAnchorCount);

        // Scroll Viewの設定
        float itemHeight = mListItemPrefab.GetComponent<LayoutElement>().preferredHeight;
        float itemVerticalSpace = cloudAnchorListContentGO.GetComponent<VerticalLayoutGroup>().spacing;
        cloudAnchorListContentGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, (itemHeight + itemVerticalSpace) * cloudAnchorCount);

        for (int i=0, size=CloudAnchorManager.Count; i<size; i++)
        {
            if(CloudAnchorManager.GetCloudAnchorId(i, out cloudAnchorId))
            {
                XPSession.ResolveCloudAnchor(cloudAnchorId).ThenAction((CloudAnchorResult result) =>
                {
                    XPAnchor xpAnchor = result.Anchor;

                    if(xpAnchor != null)
                    {
                        // リストに追加
                        GameObject listItemGO = Instantiate(mListItemPrefab);
                        //listItemGO.transform.Find("CloudAnchorId").gameObject.GetComponent<UnityEngine.UI.Text>().text = xpAnchor.CloudId;
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
                if(xpAnchor != null)
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

    private void AddAnchorGO(XPAnchor xpAnchor, GameObject anchorGO = null)
    {
        mLogger.CategoryLog(LogCategoryMethodIn);

        if(!mCloudAnchorGODic.TryGetValue(xpAnchor.CloudId, out anchorGO))
        {
            if(anchorGO == null)
            {
                anchorGO = Instantiate(mAnchorPrefab, xpAnchor.transform.position, xpAnchor.transform.rotation);
            }
            anchorGO.SetActive(true);

            mCloudAnchorDic[xpAnchor.CloudId] = xpAnchor;
            mCloudAnchorGODic[xpAnchor.CloudId] = anchorGO;
            CloudAnchorManager.Append(xpAnchor.CloudId);
        }
        mLogger.CategoryLog(LogCategoryMethodOut);
    }
}
