using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MSCConfig
{
    public static string user = "";//;//�˺�
    public static string pwd = "";//;//����
    public static string app_id = "appid=3960c859";//appid��msc.dllҪ����
    public static string qisr_session_begin_params = "sub = iat, domain = iat, language = zh_cn, accent = mandarin, sample_rate = 16000, result_type = plain, result_encoding = utf-8";
    public static string qivw_session_begin_params = "ivw_threshold=0:1450,sst=wakeup,ivw_res_path =fo|res/ivw/wakeupresource.jet";
    public static string qtts_session_begin_params = "voice_name = xiaoyan, text_encoding = utf8, sample_rate = 16000, speed = 50, volume = 50, pitch = 50, rdn = 0";
    public static int lengthSec = 999;//ʱ��
    public static int frequency = 16000;//Ƶ��
    public static float minVolume = 0.1f;// 5;//��С����
    public static float maxVolume = 1;//20;//�������
    public static int minVolume_Sum = 60;//С�����ܺ�ֵ
    public static int minVolume_Number;//��¼��С��������

    /// <summary>
    /// QA�ʴ�ӿ�
    /// </summary>
    public static string url_intelligentQA = "http://121.36.13.42:8081/aq/intelligentQA";
    /// <summary>
    /// ��ʾ��������ӿ�
    /// </summary>
    public static string url_queryQuestions = "http://121.36.13.42:8081/aq/queryQuestions";
    /// <summary>
    /// У��license�ӿ�
    /// </summary>
    public static string url_accredit_Permission = "http://121.36.13.42:8081/accredit/checkPermission";
}
