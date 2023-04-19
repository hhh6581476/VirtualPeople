using SuperScrollView;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DotElem
{
    public GameObject mDotElemRoot;
    public GameObject mDotSmall;
    public GameObject mDotBig;
}
public class PageViewComponent : MonoBehaviour
{
    LoopListView2 mLoopListView;
    int mPageCount = 5;
    List<DotElem> mDotElemList = new List<DotElem>();
    Transform mDotsRootObj;
    MyAnswer myAnswer;

    bool isInit = false;
    // Start is called before the first frame update
    void Start()
    {
        if (isInit)
        {
            return;
        }

        OnInit();
    }


    private void OnInit()
    {
        mLoopListView = transform.Find("ScrollView_LeftToRight").GetComponent<LoopListView2>();
        mDotsRootObj = transform.Find("ScrollView_LeftToRight/DotsRoot");
        InitDots();
        LoopListViewInitParam initParam = LoopListViewInitParam.CopyDefaultInitParam();
        initParam.mSnapVecThreshold = 99999;
        mLoopListView.mOnBeginDragAction = OnBeginDrag;
        mLoopListView.mOnDragingAction = OnDraging;
        mLoopListView.mOnEndDragAction = OnEndDrag;
        mLoopListView.mOnSnapNearestChanged = OnSnapNearestChanged;
        mLoopListView.InitListView(mPageCount, OnGetItemByIndex, initParam);

        isInit = true;
    }

    void InitDots()
    {
        int childCount = mDotsRootObj.childCount;
        for (int i = 0; i < childCount; ++i)
        {
            Transform tf = mDotsRootObj.GetChild(i);
            DotElem elem = new DotElem();
            elem.mDotElemRoot = tf.gameObject;
            elem.mDotSmall = tf.Find("dotSmall").gameObject;
            elem.mDotBig = tf.Find("dotBig").gameObject;
            ClickEventListener listener = ClickEventListener.Get(elem.mDotElemRoot);
            int index = i;
            listener.SetClickEventHandler(delegate (GameObject obj) { OnDotClicked(index); });
            mDotElemList.Add(elem);
        }
    }

    public void RefreshUIByData(MyAnswer mmyAnswer)
    {
        myAnswer = mmyAnswer;

        if(!isInit)
        {
            OnInit();
        }
        mLoopListView.RefreshAllShownItem();
    }

    void OnDotClicked(int index)
    {
        int curNearestItemIndex = mLoopListView.CurSnapNearestItemIndex;
        if (curNearestItemIndex < 0 || curNearestItemIndex >= mPageCount)
        {
            return;
        }
        if (index == curNearestItemIndex)
        {
            return;
        }
        if (index > curNearestItemIndex)
        {
            mLoopListView.SetSnapTargetItemIndex(curNearestItemIndex + 1);
        }
        else
        {
            mLoopListView.SetSnapTargetItemIndex(curNearestItemIndex - 1);
        }
    }

    void UpdateAllDots()
    {
        int curNearestItemIndex = mLoopListView.CurSnapNearestItemIndex;
        if (curNearestItemIndex < 0 || curNearestItemIndex >= mPageCount)
        {
            return;
        }
        int count = mDotElemList.Count;
        if (curNearestItemIndex >= count)
        {
            return;
        }
        for (int i = 0; i < count; ++i)
        {
            DotElem elem = mDotElemList[i];
            if (i != curNearestItemIndex)
            {
                elem.mDotSmall.SetActive(true);
                elem.mDotBig.SetActive(false);
            }
            else
            {
                elem.mDotSmall.SetActive(false);
                elem.mDotBig.SetActive(true);
            }
        }
    }

    void OnSnapNearestChanged(LoopListView2 listView, LoopListViewItem2 item)
    {
        UpdateAllDots();
    }


    LoopListViewItem2 OnGetItemByIndex(LoopListView2 listView, int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= mPageCount)
        {
            return null;
        }

        LoopListViewItem2 item = listView.NewListViewItem("ItemPrefab1");
        //ListItem14 itemScript = item.GetComponent<ListItem14>();
        //if (item.IsInitHandlerCalled == false)
        //{
        //    item.IsInitHandlerCalled = true;
        //    itemScript.Init();
        //}
        //List<ListItem14Elem> elemList = itemScript.mElemItemList;
        //int count = elemList.Count;
        //int picBeginIndex = pageIndex * count;
        //int i = 0;
        //for (; i < count; ++i)
        //{
        //    ItemData itemData = DataSourceMgr.Get.GetItemDataByIndex(picBeginIndex + i);
        //    if (itemData == null)
        //    {
        //        break;
        //    }
        //    ListItem14Elem elem = elemList[i];
        //    elem.mRootObj.SetActive(true);
        //    elem.mIcon.sprite = ResManager.Get.GetSpriteByName(itemData.mIcon);
        //    elem.mName.text = itemData.mName;
        //}
        //if (i < count)
        //{
        //    for (; i < count; ++i)
        //    {
        //        elemList[i].mRootObj.SetActive(false);
        //    }
        //}
        if (myAnswer != null)
        {
            item.transform.Find("Text").GetComponent<Text>().text = myAnswer.data.answer;
        }
        return item;
    }


    void OnBeginDrag()
    {

    }

    void OnDraging()
    {

    }
    void OnEndDrag()
    {
        float vec = mLoopListView.ScrollRect.velocity.y;
        int curNearestItemIndex = mLoopListView.CurSnapNearestItemIndex;
        LoopListViewItem2 item = mLoopListView.GetShownItemByItemIndex(curNearestItemIndex);
        if (item == null)
        {
            mLoopListView.ClearSnapData();
            return;
        }
        if (Mathf.Abs(vec) < 50f)
        {
            //Debug.LogError(", 111111curNearestItemIndex:" + curNearestItemIndex);
            mLoopListView.SetSnapTargetItemIndex(curNearestItemIndex);
            return;
        }
        Vector3 pos = mLoopListView.GetItemCornerPosInViewPort(item, ItemCornerEnum.LeftTop);
        if (pos.y < 0)
        {
            if (vec < 0)
            {
                //Debug.LogError(", 2222222222curNearestItemIndex:" + curNearestItemIndex);
                mLoopListView.SetSnapTargetItemIndex(curNearestItemIndex - 1);
            }
            else
            {
                //Debug.LogError(",3333333curNearestItemIndex:" + curNearestItemIndex);
                mLoopListView.SetSnapTargetItemIndex(curNearestItemIndex);
            }
        }
        else if (pos.y > 0)
        {
            if (vec < 0)
            {
                //Debug.LogError(",4444444curNearestItemIndex:" + curNearestItemIndex);
                mLoopListView.SetSnapTargetItemIndex(curNearestItemIndex);
            }
            else
            {
                //Debug.LogError(",666666curNearestItemIndex:" + curNearestItemIndex);
                mLoopListView.SetSnapTargetItemIndex(curNearestItemIndex + 1);
            }
        }
        else
        {
            if (vec > 0)
            {
                //Debug.LogError(",777777curNearestItemIndex:" + curNearestItemIndex);
                mLoopListView.SetSnapTargetItemIndex(curNearestItemIndex - 1);
            }
            else
            {
                //Debug.LogError(",888888curNearestItemIndex:" + curNearestItemIndex);
                mLoopListView.SetSnapTargetItemIndex(curNearestItemIndex + 1);
            }
        }
    }

    float currentTime, MaxTime = 6;

    int currentPage = 0;
    int MaxPage = 5;
    private void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime>= MaxTime)
        {
            currentTime = 0;
            currentPage++;
            if (currentPage>= MaxPage)
            {
                currentPage = 0;
                return;
            }
            Debug.Log("currentPage "+ currentPage);
            mLoopListView.SetSnapTargetItemIndex(currentPage);
        }
    }
}
