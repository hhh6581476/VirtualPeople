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
using UnityEngine.Events;
using UnityGameFramework.Runtime;
using GameEntry = StarForce.GameEntry;

public class AIPlayer
{
    AudioSource audioSource;
    private bool isTranslate = false;
    private string Player_Audio_Value;
    private bool isRecording;//录音开关
    private int minVolume_Number;//记录的小音量数量

    private bool hasDevice;

    private string modelAssetName;
    private Animator animator;

    public AIPlayer()
    {
      
    }
    public void Init()
    {
        InitAIPlayer();
        AddListener();
        CreateModel();
    }
    /// <summary>
    /// 初始化AIplayer需要的对象
    /// </summary>
   void InitAIPlayer()
    {
        audioSource = new GameObject("Player").AddComponent<AudioSource>();

        AIPlayerHelper.InitAwaken();
    }

    /// <summary>
    /// 创建虚拟人角色
    /// </summary>
    void CreateModel()
    {
        modelAssetName = UtilityBuiltin.ResPath.GetCombinePath("Assets", ConstBuiltin.Model_DIR, "MyRole.prefab");
        if (GameEntry.Resource.HasAsset(modelAssetName) == GameFramework.Resource.HasAssetResult.NotExist)
        {
            Log.Fatal("模型文件不存在:{0}", modelAssetName);
            return;
        }
        GameEntry.Resource.LoadAsset(modelAssetName);
    }

    void PlayAni()
    {
        Debug.LogError("播放动作");
        int random = UnityEngine.Random.Range(1, 4);
        animator.SetTrigger("talk" + random);
    }

    /// <summary>
    /// 添加监听
    /// </summary>
    void AddListener()
    {
        GameEntry.Event.Subscribe(WebRequestSuccessEventArgs.EventId, OnWebRequestSuccess);

        GameEntry.Event.Subscribe(ResourceLoadAssetSuccessEventArgs.EventId, OnResourceLoadAssetSuccess);
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

            animator = mode.GetComponentInChildren<Animator>();
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
            AIPlayerHelper.CreateAudio(myAnswer.data.answer, MSCConfig.qtts_session_begin_params,()=> {
                PlayAni();
            });

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
        Microphone.End(Microphone.devices[0]);

        GameEntry.UI.StartCoroutine(WaitForNextFrame(()=> {
            audioSource.clip = Microphone.Start(Microphone.devices[0], true, MSCConfig.lengthSec, MSCConfig.frequency);

            hasDevice = true;
        }));

        return true;
    }

    IEnumerator WaitForNextFrame(UnityAction unityAction)
    {
        yield return new WaitForSeconds(0.1f);
        unityAction?.Invoke();
    }

    public void EndRecord()
    {
        OnInteractiveDesign();
    }

    /// <summary>
    /// 语音交互设计
    /// </summary>
    private void OnInteractiveDesign()
    {
        isTranslate = true;
        AIPlayerHelper.AudioDiscern(AIPlayerHelper.AudioClipToByte(audioSource.clip), MSCConfig.qisr_session_begin_params, (audioStr) => {
            isTranslate = false;
            Debug.LogError("结果是：" + audioStr);
            //showText.text = audioStr;
            if (string.IsNullOrEmpty(audioStr))
            {
                //问题显示
                StartRecord();
                return;
            }
            //问题显示
            MyQuestion myQuestion = new MyQuestion();
            myQuestion.question = audioStr;
            string postData = JsonUtility.ToJson(myQuestion);
            Debug.LogError("postData " + postData);
            byte[] dataByte = Encoding.UTF8.GetBytes(postData);

            GetQuestionEventArgs getQuestionEventArgs = ReferencePool.Acquire<GetQuestionEventArgs>();
            getQuestionEventArgs.question = audioStr;
            GameEntry.Event.Fire(this, getQuestionEventArgs);
            GameEntry.WebRequest.AddWebRequest(MSCConfig.url_intelligentQA, dataByte);
        });
    }

    public void Update()
    {
        if (!hasDevice)
        {
            return;
        }
        RecordOpenClose();
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
        Debug.LogWarning("currentVolume "+ currentVolume);
        //开
        if (!isRecording)
        {
            if (currentVolume >= MSCConfig.maxVolume)
            {
                minVolume_Number = 0;
                isRecording = true;
                Debug.LogError("开始录制");
            }
        }
        else
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
                isRecording = false;
                Debug.LogError("结束录制");

                EndRecord();
            }
        }
    }


}