//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using StarForce;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;
using GameEntry = StarForce.GameEntry;
using UnityGameFramework.Runtime;
using GameFramework.Event;
using GameFramework;
using UnityEngine;
using System.Text;

public class ProcedureMain : MonoBehaviour
{
    private AIPlayer player;



    void Awake()
    {
        player = new AIPlayer();
    }

    private void Start()
    {
        bool isOk = MSCHelper.MSPLogin(MSCConfig.user, MSCConfig.pwd, MSCConfig.app_id);
        if (!isOk)
        {
            return;
        }

        player.Init();

        GameEntry.UI.OpenUIForm(UIFormId.MenuForm, this);
        player.StartRecord();


        //MyQuestion myQuestion = new MyQuestion();
        //myQuestion.question = "你好啊";
        //string postData = JsonUtility.ToJson(myQuestion);
        //Debug.LogError("postData "+ postData);
        //byte[] dataByte = Encoding.UTF8.GetBytes(postData);

        //GameEntry.WebRequest.AddWebRequest(MSCConfig.url_intelligentQA, dataByte);
    }


    private void Update()
    {
        if (player != null)
        {
            player.Update();
        }
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.Release();
            player = null;
        }
    }

}
