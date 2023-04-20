using msc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using GameEntry = StarForce.GameEntry;

public class AIPlayerHelper
{

    public static float[] samples;
    private static byte[] bytes;
    static int err_code;
    static IntPtr session_id;

    static RecogStatus rec_stat;
    static SynthStatus synth_status;
    /// <summary>
    /// 唤醒功能
    /// </summary>
    public static void InitAwaken()
    {
        return;
        //初始化语音唤醒            
        MSCDLL.ivw_ntf_handler IVW_callback = new MSCDLL.ivw_ntf_handler(cb_ivw_msg_proc);
        session_id = MSCDLL.QIVWSessionBegin(null, MSCConfig.qivw_session_begin_params, ref err_code);
        if ((int)Errors.MSP_SUCCESS != err_code)
        {
            Debug.LogError("QISRSessionBegin failed! error code:" + err_code);
            return;
        }
        IntPtr userDATA = IntPtr.Zero;
        err_code = MSCDLL.QIVWRegisterNotify(PtrToStr(session_id), cb_ivw_msg_proc, userDATA);
    }

    /// <summary>
    /// 回调函数
    /// </summary>
    /// <param name="sessionID"></param>
    /// <param name="msg"></param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="info"></param>
    /// <param name="userData"></param>
    /// <returns></returns>
    private static  int cb_ivw_msg_proc(string sessionID, int msg, int param1, int param2, IntPtr info, IntPtr userData)
    {
        if (msg == 2) //唤醒出错消息
        {
            Debug.LogError("MSP_IVW_MSG_ERROR error code:");
        }
        else if (msg == 1)//唤醒成功消息
        {
            Debug.Log("唤醒成功");
        }
        return 0;
    }

    /// <summary>
    /// 检测录音音量大小 需要优化
    /// </summary>
    /// <param name="audio"></param>
    /// <returns></returns>
    public static float Volume(AudioSource audio)
    {
        if (Microphone.IsRecording(Microphone.devices[0]))
        {
            // 采样数
            int sampleSize = 128;
            samples = new float[sampleSize];
            int startPosition = Microphone.GetPosition(Microphone.devices[0]) - (sampleSize + 1);
            if (startPosition > 0)
            {
                // 得到数据
                audio.clip.GetData(samples, startPosition);

                float levelMax = 0;
                for (int i = 0; i < sampleSize; ++i)
                {
                    float wavePeak = samples[i];
                    if (levelMax < wavePeak)
                        levelMax = wavePeak;
                }
                return levelMax * 99;
            }

        }
        return 0;
    }

    /// <summary>
    /// 音频识别功能
    /// </summary>
    /// <param name="audio_content">byte[]</param>
    /// <param name="session_begin_params">设置识别的参数：语言、领域、语言区域。。。。</param>
    public static void AudioDiscern(byte[] audio_content, string session_begin_params, Action<string> Over = null)
    {
        StringBuilder result = new StringBuilder();//存储最终识别的结果
        var aud_stat = AudioStatus.MSP_AUDIO_SAMPLE_CONTINUE;//音频状态
        var ep_stat = EpStatus.MSP_EP_LOOKING_FOR_SPEECH;//端点状态
        rec_stat = RecogStatus.MSP_REC_STATUS_SUCCESS;//识别状态
        err_code = (int)Errors.MSP_SUCCESS;
        int totalLength = 0;//用来记录总的识别后的结果的长度，判断是否超过缓存最大值    

        MSCHelper.QISRSessionBegin(null, session_begin_params, ref err_code, ref session_id);
        MSCHelper.QISRAudioWrite(session_id, audio_content, (uint)audio_content.Length, aud_stat, ref ep_stat, ref rec_stat);
        MSCHelper.QISRAudioWrite(session_id, null, 0, AudioStatus.MSP_AUDIO_SAMPLE_LAST, ref ep_stat, ref rec_stat);

        GameEntry.UI.StartCoroutine(StartRecogStatusWaitUntil(() => {
            MSCHelper.QISRGetResult(totalLength, result, session_id, ref rec_stat, 0, ref err_code);
            Debug.Log(" Debug.Log(\"正在进行语音识别...\"); errcode " + err_code);

        }, () => {

            Debug.Log("语音听写结束！\n结果：" + result.ToString());
            MSCHelper.QISRSessionEnd(session_id, "");
            Over?.Invoke(result.ToString());
        }));


    }

    static IEnumerator StartRecogStatusWaitUntil(Action updateAction, Action overAction)
    {
        while (rec_stat != RecogStatus.MSP_REC_STATUS_COMPLETE && err_code == (int)Errors.MSP_SUCCESS)
        {
            updateAction?.Invoke();
            yield return null;
        }
        overAction?.Invoke();
    }

    static IEnumerator StartSynthStatusWaitUntil(Action updateAction, Action overAction)
    {
        while (synth_status != SynthStatus.MSP_TTS_FLAG_DATA_END && err_code == (int)Errors.MSP_SUCCESS)
        {
            updateAction?.Invoke();
            yield return null;
        }
        overAction?.Invoke();
    }

    public static void CreateAudio(string speekText, string szParams,Action overAction)
    {
        MSCHelper.QTTSSessionBegin(ref session_id, szParams, ref err_code);
        MSCHelper.QTTSTextPut(session_id, speekText, (uint)Encoding.Default.GetByteCount(speekText), string.Empty);
        uint audio_len = 0;
        synth_status = SynthStatus.MSP_TTS_FLAG_STILL_HAVE_DATA;
        MemoryStream memoryStream = new MemoryStream();
        memoryStream.Write(new byte[44], 0, 44);

        GameEntry.UI.StartCoroutine(StartSynthStatusWaitUntil(() => {
            IntPtr source = MSCDLL.QTTSAudioGet(session_id, ref audio_len, ref synth_status, ref err_code);
            Debug.Log(" Debug.Log(\"正在进行翻译识别...\"); session_id " + session_id + "errcode " + err_code + " synth_status " + synth_status + " audio_len " + audio_len);
            byte[] array = new byte[audio_len];
            if (audio_len > 0)
            {
                Marshal.Copy(source, array, 0, (int)audio_len);
            }
            memoryStream.Write(array, 0, array.Length);
            Thread.Sleep(150);
        }, () => {

            MSCHelper.QTTSSessionEnd(session_id, "");
            WAVE_Header header = getWave_Header((int)memoryStream.Length - 44);//创建wav文件头
            byte[] headerByte = StructToBytes(header);//把文件头结构转化为字节数组
            memoryStream.Position = 0;//定位到文件头
            memoryStream.Write(headerByte, 0, headerByte.Length);//写入文件头
            bytes = memoryStream.ToArray();
            memoryStream.Close();

            string AI_audio_url = Application.streamingAssetsPath + "/AI.wav";
            if (File.Exists(AI_audio_url))
            {
                File.Delete(AI_audio_url);
            }
            File.WriteAllBytes(AI_audio_url, bytes);
            GameEntry.UI.StartCoroutine(OnAudioLoadAndPaly(AI_audio_url, AudioType.WAV, GameObject.Find("AI").GetComponent<AudioSource>()));
            overAction?.Invoke();
        }));
    }

    /// <summary>
    /// clip转byte[]  需要优化
    /// </summary>
    /// <param name="clip"></param>
    /// <returns></returns> 
    public static byte[] AudioClipToByte(AudioClip clip)
    {
        float[] data = new float[clip.samples];
        clip.GetData(data, 0);
        int rescaleFactor = 32767; //to convert float to Int16
        byte[] outData = new byte[data.Length * 2];
        for (int i = 0; i < data.Length; i++)
        {
            short temshort = (short)(data[i] * rescaleFactor);
            byte[] temdata = BitConverter.GetBytes(temshort);
            outData[i * 2] = temdata[0];
            outData[i * 2 + 1] = temdata[1];
        }
        return outData;
    }

    /// <summary>
    /// 指针转字符串
    /// </summary>
    /// <param name="p">指向非托管代码字符串的指针</param>
    /// <returns>返回指针指向的字符串</returns>
    public static string PtrToStr(IntPtr p)
    {
        List<byte> lb = new List<byte>();
        try
        {
            while (Marshal.ReadByte(p) != 0)
            {
                lb.Add(Marshal.ReadByte(p));
                p = p + 1;
            }
        }
        catch (AccessViolationException ex)
        {
            Debug.LogError(String.Format(ex.Message));
        }
        return Encoding.UTF8.GetString(lb.ToArray());
    }

    /// <summary>
    /// UnityWebRequest 加载音频播放
    /// </summary>
    /// <param name="url">路径</param>
    /// <param name="type">音频格式</param>
    /// <param name="audio">音频</param>
    /// <returns></returns>
    public static IEnumerator OnAudioLoadAndPaly(string url, AudioType type, AudioSource audio)
    {
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, type);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.ProtocolError|| www.result == UnityWebRequest.Result.ConnectionError)
            Debug.LogError(www.error);
        else
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            audio.clip = clip;
            audio.Play();
        }
    }

    /// <summary>
    /// 结构体初始化赋值
    /// </summary>
    /// <param name="data_len"></param>
    /// <returns></returns>
    private static WAVE_Header getWave_Header(int data_len)
    {
        return new WAVE_Header
        {
            RIFF_ID = 1179011410,
            File_Size = data_len + 36,
            RIFF_Type = 1163280727,
            FMT_ID = 544501094,
            FMT_Size = 16,
            FMT_Tag = 1,
            FMT_Channel = 1,
            FMT_SamplesPerSec = 16000,
            AvgBytesPerSec = 32000,
            BlockAlign = 2,
            BitsPerSample = 16,
            DATA_ID = 1635017060,
            DATA_Size = data_len
        };
    }

    /// <summary>
    /// 结构体转字符串
    /// </summary>
    /// <param name="structure"></param>
    /// <returns></returns>
    private static byte[] StructToBytes(object structure)
    {
        int num = Marshal.SizeOf(structure);
        IntPtr intPtr = Marshal.AllocHGlobal(num);
        byte[] result;
        try
        {
            Marshal.StructureToPtr(structure, intPtr, false);
            byte[] array = new byte[num];
            Marshal.Copy(intPtr, array, 0, num);
            result = array;
        }
        finally
        {
            Marshal.FreeHGlobal(intPtr);
        }
        return result;
    }
}