//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using UnityEngine;
using UnityGameFramework.Runtime;
using StarForce;
using UnityEngine.UI;
using GameEntry = StarForce.GameEntry;
using GameFramework.Event;
using GameFramework;
using System.Text;

public class MenuForm : UGuiForm
{
    const int QuestionTipsCount = 3;
    float currentTime, RequestTime= 6f;
    Text[] questionTipsText = new Text[4];

    GameObject[] questionContain = new GameObject[2];

    GameObject questionItem;
    PageViewComponent pageViewCom;
    Text QuestionText;

    RenderTexture renderTexture;
    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        questionContain[0] = this.transform.Find("Question_left").gameObject;
        questionContain[1] = this.transform.Find("Question_right").gameObject;
        questionItem = this.transform.Find("Question_Item").gameObject;

        SetQuestionTipActive(true);
        pageViewCom = this.transform.Find("PageViewComponent").gameObject.AddComponent<PageViewComponent>();
        QuestionText = this.transform.Find("PageViewComponent/Question/Text").GetComponent<Text>();
        SetQuestionAnswerActive(false);
        bool isOk = MSCHelper.MSPLogin(MSCConfig.user, MSCConfig.pwd, MSCConfig.app_id);
        if (!isOk)
        {
            return;
        }

        renderTexture = new RenderTexture(1920,1080,101);
        GameObject.Find("Camera").GetComponent<Camera>().targetTexture = renderTexture;

        this.transform.Find("RawImage").GetComponent<RawImage>().texture =renderTexture;
    }
    private void OnDestroy()
    {
        MSCHelper.MSPLoginOut();
    }

    public void SetQuestionTipActive(bool isActive)
    {
        for (int i = 0; i < questionContain.Length; i++)
        {
            questionContain[i].SetActive(isActive);
        }
    }

    public void SetQuestionAnswerActive(bool isActive)
    {
        pageViewCom.gameObject.SetActive(isActive);
    }

    public void CreateQuestionTips(QueryAnswer queryAnswer)
    {
        for (int i = 0; i < questionContain.Length; i++)
        {
            if (questionContain[i].transform.childCount>0)
            {
                for (int j = questionContain[i].transform.childCount - 1; j >= 0; j--)
                {
                    GameObject.Destroy(questionContain[i].transform.GetChild(j).gameObject);

                }
            }
            
        }

        GameObject obj = Instantiate(questionItem);
        obj.transform.SetParent(questionContain[0].transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.Find("Bg1/Text").GetComponent<Text>().text = queryAnswer.data[0].question;
        obj.transform.Find("Bg2/Text").GetComponent<Text>().text = queryAnswer.data[1].question;
        obj.SetActive(true);
        obj = Instantiate(questionItem);
        obj.transform.SetParent(questionContain[1].transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.Find("Bg1/Text").GetComponent<Text>().text = queryAnswer.data[2].question;

        //obj.transform.Find("Bg2/Text").GetComponent<Text>().text = queryAnswer.data[3].question;
        obj.SetActive(true);
    }


    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
    }


    protected override void OnClose(bool isShutdown, object userData)
    {

        base.OnClose(isShutdown, userData);
    }
    protected override void OnResume()
    {
        base.OnResume();

        GameEntry.Event.Subscribe(WebRequestSuccessEventArgs.EventId, OnWebRequestSuccess);
        GameEntry.Event.Subscribe(WebRequestFailureEventArgs.EventId, OnWebRequestFailure);
        GameEntry.Event.Subscribe(GetAnswerEventArgs.EventId, OnGetAnswerEventArgs);
        GameEntry.Event.Subscribe(GetQuestionEventArgs.EventId, OnGetQuestionEventArgs);

    }

    protected override void OnPause()
    {
        GameEntry.Event.Unsubscribe(WebRequestSuccessEventArgs.EventId, OnWebRequestSuccess);
        GameEntry.Event.Unsubscribe(WebRequestFailureEventArgs.EventId, OnWebRequestFailure);
        GameEntry.Event.Unsubscribe(GetAnswerEventArgs.EventId, OnGetAnswerEventArgs);
        GameEntry.Event.Unsubscribe(GetQuestionEventArgs.EventId, OnGetQuestionEventArgs);


        base.OnPause();
    }

    private void OnGetQuestionEventArgs(object sender, GameEventArgs e)
    {
        GetQuestionEventArgs ne = (GetQuestionEventArgs)e;

        Debug.Log("OnGetQuestionEventArgs");
        QuestionText.text = ne.question;
    }

    private void OnGetAnswerEventArgs(object sender, GameEventArgs e)
    {
        GetAnswerEventArgs ne = (GetAnswerEventArgs)e;

        SetQuestionAnswerActive(true);
        SetQuestionTipActive(false);
        pageViewCom.RefreshUIByData(ne.MyAnswer);

    }

    private void OnWebRequestSuccess(object sender, GameEventArgs e)
    {
        WebRequestSuccessEventArgs ne = (WebRequestSuccessEventArgs)e;

        if (ne.WebRequestUri.Equals(MSCConfig.url_queryQuestions))
        {
            //获取三条随机数据
            // 解析版本信息
            byte[] versionInfoBytes = ne.GetWebResponseBytes();
            string message = Utility.Converter.GetString(versionInfoBytes);
            //QueryAnswer queryAnswer = JsonUtility.FromJson<QueryAnswer>(message);
            //for (int i = 0; i < queryAnswer.data.Count; i++)
            //{
            //    if (i < questionTipsText.Length)
            //    {
            //        questionTipsText[i].text = queryAnswer.data[i].question;
            //        Debug.Log("queryAnswer.data[i].question "+ queryAnswer.data[i].question);
            //    }
            //}
            Debug.Log(message);
            QueryAnswer root = LitJson.JsonMapper.ToObject<QueryAnswer>(message);
            CreateQuestionTips(root);
        }



        // 解析版本信息
        //byte[] versionInfoBytes = ne.GetWebResponseBytes();
        //string versionInfoString = Utility.Converter.GetString(versionInfoBytes);
        //m_VersionInfo = Utility.Json.ToObject<VersionInfo>(versionInfoString);
        //if (m_VersionInfo == null)
        //{
        //    Log.Error("Parse VersionInfo failure.");
        //    return;
        //}
    }

    private void OnWebRequestFailure(object sender, GameEventArgs e)
    {
        WebRequestFailureEventArgs ne = (WebRequestFailureEventArgs)e;
        if (!ne.WebRequestUri.Equals(MSCConfig.url_queryQuestions))
        {
            return;
        }

        Log.Warning("Check version failure, error message is '{0}'.", ne.ErrorMessage);
    }


    protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);

        if (questionContain[0]==null||!questionContain[0].activeSelf)
        {
            return;
        }
        currentTime += Time.deltaTime;
        if (currentTime >= RequestTime)
        {
            currentTime = 0f;
            InitShowTipsData();
        }
    }

    public void InitShowTipsData()
    {
        // 向服务器请求版本信息
        byte[] dataByte = Encoding.UTF8.GetBytes("{}");
        GameEntry.WebRequest.AddWebRequest(MSCConfig.url_queryQuestions, dataByte);
    }
}