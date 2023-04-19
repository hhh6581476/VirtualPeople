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


    public static void CreateAudio(string speekText, string szParams)
    {
       
        MSCHelper.QTTSSessionBegin(ref session_id, szParams, ref err_code);
        MSCHelper.QTTSTextPut(session_id, speekText, (uint)Encoding.Default.GetByteCount(speekText), string.Empty);
        uint audio_len = 0;
        SynthStatus synth_status = SynthStatus.MSP_TTS_FLAG_STILL_HAVE_DATA;
        MemoryStream memoryStream = new MemoryStream();
        memoryStream.Write(new byte[44], 0, 44);
        while (true)
        {
            IntPtr source = MSCDLL.QTTSAudioGet(session_id, ref audio_len, ref synth_status, ref err_code);
            byte[] array = new byte[audio_len];
            if (audio_len > 0)
            {
                Marshal.Copy(source, array, 0, (int)audio_len);
            }
            memoryStream.Write(array, 0, array.Length);
            Thread.Sleep(150);
            if (synth_status == SynthStatus.MSP_TTS_FLAG_DATA_END || err_code != (int)Errors.MSP_SUCCESS)
                break;
        }
        MSCHelper.QTTSSessionEnd(session_id, "");
        WAVE_Header header = getWave_Header((int)memoryStream.Length - 44);//创建wav文件头
        byte[] headerByte = StructToBytes(header);//把文件头结构转化为字节数组
        memoryStream.Position = 0;//定位到文件头
        memoryStream.Write(headerByte, 0, headerByte.Length);//写入文件头
        bytes = memoryStream.ToArray();
        memoryStream.Close();

        string AI_audio_url = Application.streamingAssetsPath + "/AI.wav";
        //if (AI_audio_url != null)
        //{
            
        //}
        if (File.Exists(AI_audio_url))
        {
            File.Delete(AI_audio_url);
        }
        File.WriteAllBytes(AI_audio_url, bytes);
        GameEntry.UI.StartCoroutine(OnAudioLoadAndPaly(AI_audio_url, AudioType.WAV, GameObject.Find("AI").GetComponent<AudioSource>()));
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