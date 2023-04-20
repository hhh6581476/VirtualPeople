using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MSCConfig
{
    public static string user = "";//;//账号
    public static string pwd = "";//;//密码
    public static string app_id = "appid=3960c859";//appid和msc.dll要配套
    public static string qisr_session_begin_params = "sub = iat, domain = iat, language = zh_cn, accent = mandarin, sample_rate = 16000, result_type = plain, result_encoding = utf-8";
    public static string qivw_session_begin_params = "ivw_threshold=0:1450,sst=wakeup,ivw_res_path =fo|res/ivw/wakeupresource.jet";
    public static string qtts_session_begin_params = "voice_name = xiaoyan, text_encoding = utf8, sample_rate = 16000, speed = 50, volume = 50, pitch = 50, rdn = 0";
    public static int lengthSec = 999;//时长
    public static int frequency = 16000;//频率
    public static float minVolume = 0.1f;// 5;//最小音量
    public static float maxVolume = 1;//20;//最大音量
    public static int minVolume_Sum = 60;//小音量总和值
    public static int minVolume_Number;//记录的小音量数量

    /// <summary>
    /// QA问答接口
    /// </summary>
    public static string url_intelligentQA = "http://121.36.13.42:8081/aq/intelligentQA";
    /// <summary>
    /// 提示可问问题接口
    /// </summary>
    public static string url_queryQuestions = "http://121.36.13.42:8081/aq/queryQuestions";
    /// <summary>
    /// 校验license接口
    /// </summary>
    public static string url_accredit_Permission = "http://121.36.13.42:8081/accredit/checkPermission";
}
