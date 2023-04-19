using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{ 

}


public class HomeData
{

}

/// <summary>
/// ”Ô“Ù“Ù∆µÕ∑
/// </summary>
public struct WAVE_Header
{
    public int RIFF_ID;
    public int File_Size;
    public int RIFF_Type;
    public int FMT_ID;
    public int FMT_Size;
    public short FMT_Tag;
    public ushort FMT_Channel;
    public int FMT_SamplesPerSec;
    public int AvgBytesPerSec;
    public ushort BlockAlign;
    public ushort BitsPerSample;
    public int DATA_ID;
    public int DATA_Size;
}

[Serializable]
public class AccreditPermissionResponse
{
    public string code;
    public string msg;
    public bool data;
}

[Serializable]
public class AccreditPermission
{
    public string mac_addr;
    public string pass_key;
}

[Serializable]
public class MyQuestion
{
    public string question;
}

[Serializable]
public class MyData
{
    public string answer;
    public int question_type;
    public string question;

}

[Serializable]
public class MyAnswer
{
    public int code;
    public MyData data;
    public string msg;
}

[Serializable]
public class QueryAnswer
{
    public int code;
    public List<MyData> data;
    public string msg;
}
