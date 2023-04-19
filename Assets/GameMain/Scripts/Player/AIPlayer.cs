using GameFramework;
using GameFramework.Event;
using msc;
using StarForce;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityGameFramework.Runtime;
using GameEntry = StarForce.GameEntry;

public class AIPlayer
{
    AudioSource audioSource;
    private bool isTranslate = false;
    private string Player_Audio_Value;
    private bool isRecord;//录音开关
    private int minVolume_Number;//记录的小音量数量

    private bool hasDevice;

    private string modelAssetName;
    public AIPlayer()
    {
        audioSource =new GameObject("Player").AddComponent<AudioSource>();
        GameEntry.Event.Subscribe(WebRequestSuccessEventArgs.EventId, OnWebRequestSuccess);

        GameEntry.Event.Subscribe(ResourceLoadAssetSuccessEventArgs.EventId, OnResourceLoadAssetSuccess);

        Debug.Log("AIPlayer1");
        //var hotfixListFile = UtilityBuiltin.ResPath.GetCombinePath("Assets", ConstBuiltin.Model_DIR, "MyRole.prefab");
        //Debug.Log("AIPlayer2");

        //if (GameEntry.Resource.HasAsset(hotfixListFile) == GameFramework.Resource.HasAssetResult.NotExist)
        //{
        //    Log.Fatal("模型文件不存在:{0}", hotfixListFile);
        //    return;
        //}
        //Debug.Log("AIPlayer3");

        //GameEntry.Resource.LoadAsset(hotfixListFile, new GameFramework.Resource.LoadAssetCallbacks((string assetName, object asset, float duration, object userData) =>
        //{
        //    Debug.Log("AIPlayer4");

        //    GameObject mode = GameObject.Instantiate((GameObject)asset);
        //    mode.transform.parent = GameObject.Find("Camera/Contain").transform;
        //    mode.transform.localPosition = Vector3.zero;
        //    mode.transform.localScale = Vector3.one;
        //    mode.transform.localEulerAngles = Vector3.zero;

        //}));

        //string AI_audio_url = Application.streamingAssetsPath + "/AI.wav";
        //GameEntry.UI.StartCoroutine(AIPlayerHelper.OnAudioLoadAndPaly(AI_audio_url, AudioType.WAV, GameObject.Find("AI").GetComponent<AudioSource>()));



        modelAssetName = UtilityBuiltin.ResPath.GetCombinePath("Assets", ConstBuiltin.Model_DIR, "MyRole.prefab");
        if (GameEntry.Resource.HasAsset(modelAssetName) == GameFramework.Resource.HasAssetResult.NotExist)
        {
            Log.Fatal("模型文件不存在:{0}", modelAssetName);
            return;
        }
        GameEntry.Resource.LoadAsset(modelAssetName);

        //GameEntry.Resource.LoadAsset(hotfixListFile2, new GameFramework.Resource.LoadAssetCallbacks((string assetName, object asset, float duration, object userData) =>
        //{
        //    GameObject mode = GameObject.Instantiate((GameObject)asset);
        //    mode.transform.parent = GameObject.Find("Camera/Contain").transform;
        //    mode.transform.localPosition = Vector3.zero;
        //    mode.transform.localScale = Vector3.one;
        //    mode.transform.localEulerAngles = Vector3.zero;

        //}));
    }

    private void OnResourceLoadAssetSuccess(object sender, GameEventArgs e)
    {
        ResourceLoadAssetSuccessEventArgs ne = (ResourceLoadAssetSuccessEventArgs)e;
        if (ne.AssetName.Equals(modelAssetName))
        {
            GameObject mode = GameObject.Instantiate((GameObject)ne.Asset);
            mode.transform.parent = GameObject.Find("Camera/Contain").transform;
            mode.transform.localPosition = Vector3.zero;
            mode.transform.localScale = Vector3.one;
            mode.transform.localEulerAngles = Vector3.zero;
        }
        
    }

    private void OnWebRequestSuccess(object sender, GameEventArgs e)
    {
        WebRequestSuccessEventArgs ne = (WebRequestSuccessEventArgs)e;

        if (ne.WebRequestUri.Equals(MSCConfig.url_intelligentQA))
        {
            // 解析版本信息
            byte[] responseBytes = ne.GetWebResponseBytes();
            string responseString = Utility.Converter.GetString(responseBytes);
            Debug.LogError("responseString " + responseString);
            MyAnswer myAnswer = LitJson.JsonMapper.ToObject<MyAnswer>(responseString);
            if (myAnswer == null)
            {
                Log.Error("Parse VersionInfo failure.");
                return;
            }
            ////答案是：
            //MessageDataMgr.Get.AddChatData(false, myAnswer.data.answer);
            GetAnswerEventArgs getAnswerEventArgs =new GetAnswerEventArgs();
            getAnswerEventArgs.MyAnswer = myAnswer;
            GameEntry.Event.Fire(this, getAnswerEventArgs);
            AIPlayerHelper.CreateAudio(myAnswer.data.answer, MSCConfig.qtts_session_begin_params);
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

    public bool StartRecord()
    {
        if (Microphone.devices.Length<=0)
        {
            return false;
        }
        audioSource.clip = Microphone.Start(Microphone.devices[0], true, MSCConfig.lengthSec, MSCConfig.frequency);
        hasDevice = true;
        return true;
    }

    public void EndRecord()
    {

        OnInteractiveDesign();
        //Microphone.End(null);
    }

    /// <summary>
    /// 语音交互设计
    /// </summary>
    private void OnInteractiveDesign()
    {
        isTranslate = true;
        AudioDiscern(AIPlayerHelper.AudioClipToByte(audioSource.clip), MSCConfig.qisr_session_begin_params, () => {
            isTranslate = false;
            Debug.LogError("结果是：" + Player_Audio_Value);
            //showText.text = Player_Audio_Value;
            if (string.IsNullOrEmpty(Player_Audio_Value))
            {
                //问题显示
                //MessageDataMgr.Get.AddChatData(true, "未识别到问题，请重新提问");
                StartRecord();
                return;
            }
            //问题显示
            //MessageDataMgr.Get.AddChatData(true, Player_Audio_Value);

            //StartRecord();
            //return;

            MyQuestion myQuestion = new MyQuestion();
            myQuestion.question = Player_Audio_Value;
            string postData = JsonUtility.ToJson(myQuestion);
            Debug.LogError("postData " + postData);
            byte[] dataByte = Encoding.UTF8.GetBytes(postData);

            GetQuestionEventArgs getQuestionEventArgs = ReferencePool.Acquire<GetQuestionEventArgs>();
            getQuestionEventArgs.question = Player_Audio_Value;
            GameEntry.Event.Fire(this, getQuestionEventArgs);
            GameEntry.WebRequest.AddWebRequest(MSCConfig.url_intelligentQA, dataByte);

            //MyQuestion myQuestion = new MyQuestion();
            //myQuestion.question = Player_Audio_Value;
            //string postData = JsonUtility.ToJson(myQuestion);

            //GameEntry.UI.StartCoroutine(UnityWebRequestPost(MSCConfig.url_intelligentQA, postData, (isOk, message) =>
            //{
            //    if (isOk)
            //    {
            //        MyAnswer myAnswer = JsonUtility.FromJson<MyAnswer>(message);

            //        //答案是：
            //        MessageDataMgr.Get.AddChatData(false, myAnswer.data.answer);

            //        CreateAudio(myAnswer.data.answer, qtts_session_begin_params);
            //    }

            //}));
        });


        //CreateAudio(Player_Audio_Value, qtts_session_begin_params);
        //if (Player_Audio_Value == "小胡小胡。")
        //{
        //    //语音合成
        //    CreateAudio("在呢。", qtts_session_begin_params);
        //    Player_Audio_Value = "";
        //    isAwaken = true;
        //}
        //else if (isAwaken)
        //{
        //    CreateAudio(UNIT.UNIT_Utterance(Player_Audio_Value), qtts_session_begin_params);
        //}
    }

    public void Update()
    {
        if (!hasDevice)
        {
            return;
        }
        RecordOpenClose();
    }

    public void Init()
    {

    }
    public void Release()
    {

    }




    /// <summary>
    /// 录音自动开关
    /// </summary>
    private void RecordOpenClose()
    {
        if (isTranslate)
        {
            return;
        }
        float currentVolume = AIPlayerHelper.Volume(audioSource);
        //Debug.Log("currentVolume "+ currentVolume);
        //开
        if (currentVolume >= MSCConfig.maxVolume)
        {
            minVolume_Number = 0;
            isRecord = true;
        }
        //关
        if (isRecord)
        {
            if (currentVolume < MSCConfig.minVolume)
            {
                minVolume_Number++;
            }
            else
            {
                minVolume_Number = 0;
            }
            if (minVolume_Number > MSCConfig.minVolume_Sum)
            {
                minVolume_Number = 0;
                isRecord = false;
                EndRecord();
            }
        }
    }

    int errcode = (int)Errors.MSP_SUCCESS;
    RecogStatus rec_stat;
    private IntPtr session_id;

    /// <summary>
    /// 音频识别功能
    /// </summary>
    /// <param name="audio_content">byte[]</param>
    /// <param name="session_begin_params">设置识别的参数：语言、领域、语言区域。。。。</param>
    private void AudioDiscern(byte[] audio_content, string session_begin_params, Action Over = null)
    {
        StringBuilder result = new StringBuilder();//存储最终识别的结果
        var aud_stat = AudioStatus.MSP_AUDIO_SAMPLE_CONTINUE;//音频状态
        var ep_stat = EpStatus.MSP_EP_LOOKING_FOR_SPEECH;//端点状态
        rec_stat = RecogStatus.MSP_REC_STATUS_SUCCESS;//识别状态
        errcode = (int)Errors.MSP_SUCCESS;
        int totalLength = 0;//用来记录总的识别后的结果的长度，判断是否超过缓存最大值    

        MSCHelper.QISRSessionBegin(null, session_begin_params, ref errcode, ref session_id);
        MSCHelper.QISRAudioWrite(session_id, audio_content, (uint)audio_content.Length, aud_stat, ref ep_stat, ref rec_stat);
        MSCHelper.QISRAudioWrite(session_id, null, 0, AudioStatus.MSP_AUDIO_SAMPLE_LAST, ref ep_stat, ref rec_stat);

        GameEntry.UI.StartCoroutine(StartWaitUntil(() => {
            MSCHelper.QISRGetResult(totalLength, result, session_id, ref rec_stat, 0, ref errcode);
            Debug.Log(" Debug.Log(\"正在进行语音识别...\"); errcode " + errcode);

        }, () => {

            Debug.Log("语音听写结束！\n结果：" + result.ToString());
            Player_Audio_Value = result.ToString();
            MSCHelper.QISRSessionEnd(session_id, "");
            Over?.Invoke();
        }));


    }

    IEnumerator StartWaitUntil(Action updateAction, Action overAction)
    {
        while (rec_stat != RecogStatus.MSP_REC_STATUS_COMPLETE && errcode == (int)Errors.MSP_SUCCESS)
        {
            updateAction?.Invoke();
            yield return null;
        }
        overAction?.Invoke();
    }
}