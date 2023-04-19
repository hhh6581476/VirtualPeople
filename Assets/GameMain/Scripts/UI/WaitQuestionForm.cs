using GameFramework;
using GameFramework.Event;
using StarForce;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;
using GameEntry = StarForce.GameEntry;

public class WaitQuestionForm : UGuiForm
{
    GameObject[] questionContain = new GameObject[2];
    GameObject questionItem;
    float currentTime, RequestTime = 6f;
    // Start is called before the first frame update
    protected override void OnInit(object userData)
    {
        base.OnInit(userData);

        questionContain[0] = this.transform.Find("Question_left").gameObject;
        questionContain[1] = this.transform.Find("Question_right").gameObject;
        questionItem = this.transform.Find("Question_Item").gameObject;

    }


    protected override void OnResume()
    {
        base.OnResume();

        GameEntry.Event.Subscribe(WebRequestSuccessEventArgs.EventId, OnWebRequestSuccess);
        GameEntry.Event.Subscribe(WebRequestFailureEventArgs.EventId, OnWebRequestFailure);
    }

    protected override void OnPause()
    {
        GameEntry.Event.Unsubscribe(WebRequestSuccessEventArgs.EventId, OnWebRequestSuccess);
        GameEntry.Event.Unsubscribe(WebRequestFailureEventArgs.EventId, OnWebRequestFailure);

        base.OnPause();
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
            QueryAnswer queryAnswer = JsonUtility.FromJson<QueryAnswer>(message);
            CreateQuestionTips(queryAnswer);
        }
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


    public void CreateQuestionTips(QueryAnswer queryAnswer)
    {
        for (int i = 0; i < questionContain.Length; i++)
        {
            for (int j = questionContain[i].transform.childCount - 1; j >= 0; j--)
            {
                GameObject.Destroy(questionContain[i].transform.GetChild(j).gameObject);
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
    protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
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
