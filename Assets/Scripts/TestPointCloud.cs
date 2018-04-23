using Eq.Unity;
using GoogleARCore;
using System.Collections.Generic;
using UnityEngine;

public class TestPointCloud : BaseAndroidMainController
{
    private const int MaxDrawPointCount = 10000;
    private Dictionary<Vector3, GameObject> mManagedGameObjectDic = new Dictionary<Vector3, GameObject>();
    public GameObject mPointGO;

    internal override void Update()
    {
        base.Update();

        int pointCount = Frame.PointCloud.PointCount;
        if (pointCount > 0)
        {
            Dictionary<Vector3, GameObject> tempManagedGameObjectDic = new Dictionary<Vector3, GameObject>();
            int currentGOCount = mManagedGameObjectDic.Count;

            for (int i = 0; i < pointCount; i++)
            {
                Vector3 pointVector = Frame.PointCloud.GetPoint(i);
                GameObject recycleGameObject = null;

                if (mManagedGameObjectDic.TryGetValue(pointVector, out recycleGameObject))
                {
                    mManagedGameObjectDic.Remove(pointVector);
                    tempManagedGameObjectDic[pointVector] = recycleGameObject;
                }
                else
                {
                    if (currentGOCount < MaxDrawPointCount)
                    {
                        GameObject pointGO = Instantiate(mPointGO, pointVector, Quaternion.identity);
                        tempManagedGameObjectDic[pointVector] = pointGO;
                        pointGO.SetActive(true);
                    }
                }
            }

            mManagedGameObjectDic = tempManagedGameObjectDic;
        }
    }
}
