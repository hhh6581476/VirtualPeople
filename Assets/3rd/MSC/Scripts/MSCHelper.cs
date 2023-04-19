using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using msc;
using System;
using System.Text;
using System.Runtime.InteropServices;

public class MSCHelper
{
    /// <summary>
    /// 登录
    /// </summary>
    public static bool MSPLogin(string usr, string pwd, string parameters)
    {
        Debug.Log("usr:"+ usr+ " pwd:"+ pwd+ " parameters:"+ parameters);

        int message = MSCDLL.MSPLogin(usr, pwd, parameters);
        if (message != (int)Errors.MSP_SUCCESS)
        {
            Debug.LogError("登录失败！错误信息：" + message);
            return false;
        }
        Debug.Log("登录成功");
        return true;
    }

    /// <summary>
    /// 注销
    /// </summary>
    /// <returns></returns>
    public static void MSPLoginOut()
    {
        int message = MSCDLL.MSPLogout();
        if (message != (int)Errors.MSP_SUCCESS)
        {
            Debug.LogError("注销失败！错误信息: " + message);
            return;
        }
        Debug.Log("注销成功");
    }

    /// <summary>
    /// 语音识别开始
    /// </summary>
    /// <param name="grammarList">定义关键词识别||语法识别||连续语音识别（null）</param>
    /// <param name="_params">设置识别的参数：语言、领域、语言区域。。。。</param>
    /// <param name="errorCode">错误代码</param>
    /// <param name="sessionID">会话ID</param>
    /// <returns></returns>
    public static void QISRSessionBegin(string grammarList, string _params, ref int errorCode, ref IntPtr sessionID)
    {
        /*
        * QISRSessionBegin（）；
        * 功能：开始一次语音识别
        * 参数1：定义关键词识别||语法识别||连续语音识别（null）
        * 参数2：设置识别的参数：语言、领域、语言区域。。。。
        * 参数3：带回语音识别的结果，成功||错误代码
        * 返回值intPtr类型,后面会用到这个返回值
        */
        sessionID = MSCDLL.QISRSessionBegin(grammarList, _params, ref errorCode);
        if (errorCode != (int)Errors.MSP_SUCCESS)
        {
            Debug.LogError("初始化语音识别失败！");
            return;
        }
    }

    /// <summary>
    /// 写入本次识别的音频
    /// </summary>
    /// <param name="sessionID">会话ID</param>
    /// <param name="waveData">音频数据缓冲区起始地址</param>
    /// <param name="waveLen">音频数据长度,单位字节。</param>
    /// <param name="audioStatus">音频状态</param>
    /// <param name="epStatus">端点检测（End-point detected）器所处的状态</param>
    /// <param name="recogStatus">识别器返回的状态</param>
    /// <returns></returns>
    public static void QISRAudioWrite(IntPtr sessionID, byte[] waveData, uint waveLen, AudioStatus audioStatus, ref EpStatus epStatus, ref RecogStatus recogStatus)
    {
        /*
         QISRAudioWrite（）；
         功能：写入本次识别的音频
         参数1：之前已经得到的sessionID
         参数2：音频数据缓冲区起始地址
         参数3：音频数据长度,单位字节。
          参数4：用来告知MSC音频发送是否完成     MSP_AUDIO_SAMPLE_FIRST = 1	第一块音频
                                                  MSP_AUDIO_SAMPLE_CONTINUE = 2	还有后继音频
                                                   MSP_AUDIO_SAMPLE_LAST = 4	最后一块音频
         参数5：端点检测（End-point detected）器所处的状态
                                                MSP_EP_LOOKING_FOR_SPEECH = 0	还没有检测到音频的前端点。
                                                 MSP_EP_IN_SPEECH = 1	已经检测到了音频前端点，正在进行正常的音频处理。
                                                 MSP_EP_AFTER_SPEECH = 3	检测到音频的后端点，后继的音频会被MSC忽略。
                                                  MSP_EP_TIMEOUT = 4	超时。
                                                 MSP_EP_ERROR = 5	出现错误。
                                                 MSP_EP_MAX_SPEECH = 6	音频过大。
         参数6：识别器返回的状态，提醒用户及时开始\停止获取识别结果
                                       MSP_REC_STATUS_SUCCESS = 0	识别成功，此时用户可以调用QISRGetResult来获取（部分）结果。
                                        MSP_REC_STATUS_NO_MATCH = 1	识别结束，没有识别结果。
                                      MSP_REC_STATUS_INCOMPLETE = 2	正在识别中。
                                      MSP_REC_STATUS_COMPLETE = 5	识别结束。
         返回值：函数调用成功则其值为MSP_SUCCESS，否则返回错误代码。
           本接口需不断调用，直到音频全部写入为止。上传音频时，需更新audioStatus的值。具体来说:
           当写入首块音频时,将audioStatus置为MSP_AUDIO_SAMPLE_FIRST
           当写入最后一块音频时,将audioStatus置为MSP_AUDIO_SAMPLE_LAST
           其余情况下,将audioStatus置为MSP_AUDIO_SAMPLE_CONTINUE
           同时，需定时检查两个变量：epStatus和rsltStatus。具体来说:
           当epStatus显示已检测到后端点时，MSC已不再接收音频，应及时停止音频写入
           当rsltStatus显示有识别结果返回时，即可从MSC缓存中获取结果*/
        int message = MSCDLL.QISRAudioWrite(sessionID, waveData, waveLen, audioStatus, ref epStatus, ref recogStatus);
        if (message != (int)Errors.MSP_SUCCESS)
        {
            Debug.LogError("写入识别的音频失败！错误信息: " + message);
            return;
        }
    }

    /// <summary>
    /// 获取识别结果
    /// </summary>
    /// <param name="totalLength"></param>
    /// <param name="result">session，之前已获得</param>
    /// <param name="sessionID">会话状态</param>
    /// <param name="rsltStatus">识别结果的状态</param>
    /// <param name="waitTime">waitTime[in]	此参数做保留用</param>
    /// <param name="errorCode">错误编码</param>
    public static void QISRGetResult(int totalLength, StringBuilder result, IntPtr sessionID, ref RecogStatus rsltStatus, int waitTime, ref int errorCode)
    {            /*
             QISRGetResult（）；
             功能：获取识别结果
             参数1：session，之前已获得
             参数2：识别结果的状态
             参数3：waitTime[in]	此参数做保留用
             参数4：错误编码||成功
             返回值：函数执行成功且有识别结果时，返回结果字符串指针；其他情况(失败或无结果)返回NULL。
             */
        IntPtr now_result = MSCDLL.QISRGetResult(sessionID, ref rsltStatus, 0, ref errorCode);
        if (errorCode != (int)Errors.MSP_SUCCESS)
        {
            Debug.LogError("获取结果失败！错误信息：" + errorCode);
            return;
        }
        else
        {
            if (now_result != null)
            {
                int length = now_result.ToString().Length;
                totalLength += length;
                if (totalLength > 4096)
                {
                    Debug.Log("缓存空间不够:" + totalLength);
                }
                result.Append(Marshal.PtrToStringAnsi(now_result));
            }
        }
    }

    /// <summary>
    /// 语音识别会话结束
    /// </summary>
    /// <param name="sessionID">会话ID</param>
    /// <param name="hints">提示</param>
    /// <returns></returns>
    public static void QISRSessionEnd(IntPtr sessionID, string hints)
    {
        int message = MSCDLL.QISRSessionEnd(sessionID, hints);
        if (message != (int)Errors.MSP_SUCCESS)
        {
            Debug.LogError("会话结束失败！错误信息: " + message);
            return;
        }
        Debug.Log("识别结束成功");
    }

    /// <summary>
    /// 语音合成开始
    /// </summary>
    /// <param name="sessionID">会话ID</param>
    /// <param name="_params">设置识别的参数：语言、领域、语言区域。。。。</param>
    /// <param name="errorCode">错误代码</param>
    public static void QTTSSessionBegin(ref IntPtr sessionID, string _params, ref int errorCode)
    {
        sessionID = MSCDLL.QTTSSessionBegin(_params, ref errorCode);
        if (errorCode != (int)Errors.MSP_SUCCESS)
        {
            Debug.LogError("初始化语音合成失败，错误信息：" + errorCode);
            return;
        }
    }

    /// <summary>
    /// 语音合成文本参数
    /// </summary>
    /// <param name="sessionID">会话ID</param>
    /// <param name="speekText">合成语音的文本</param>
    /// <param name="textLen">文本长度</param>
    /// <param name="_params">参数</param>
    public static void QTTSTextPut(IntPtr sessionID, string speekText, uint textLen, string _params)
    {
        int message = MSCDLL.QTTSTextPut(sessionID, speekText, textLen, _params);
        if (message != (int)Errors.MSP_SUCCESS)
        {
            Debug.LogError("向服务器发送数据失败，错误信息：" + message);
            return;
        }
    }

    /// <summary>
    /// 语音合成结束
    /// </summary>
    /// <param name="sessionID"></param>
    /// <param name="hints"></param>
    public static void QTTSSessionEnd(IntPtr sessionID, string hints)
    {
        int message = MSCDLL.QTTSSessionEnd(sessionID, hints);
        if (message != (int)Errors.MSP_SUCCESS)
        {
            Debug.LogError("会话结束失败！错误信息: " + message);
            return;
        }
        Debug.Log("合成结束成功");
    }
    public static float[] samples;
    /// <summary>
    /// 检测录音音量大小 需要优化
    /// </summary>
    /// <param name="audio"></param>
    /// <returns></returns>
    public static float Volume(AudioSource audio)
    {
        if (Microphone.IsRecording(null))
        {
            // 采样数
            int sampleSize = 128;
            samples = new float[sampleSize];
            int startPosition = Microphone.GetPosition(Microphone.devices[0]) - (sampleSize + 1);
            if (startPosition>0)
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
    /// UnityWebRequest 加载音频播放
    /// </summary>
    /// <param name="url">路径</param>
    /// <param name="type">音频格式</param>
    /// <param name="audio">音频</param>
    /// <returns></returns>
    public static IEnumerator OnAudioLoadAndPaly(string url,AudioType type,AudioSource audio)
    {
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, type);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError||www.result == UnityWebRequest.Result.DataProcessingError)
            //if (www.isHttpError || www.isNetworkError)
            Debug.LogError(www.error);
        else
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            audio.clip = clip;
            audio.Play();
        }
    }
}
