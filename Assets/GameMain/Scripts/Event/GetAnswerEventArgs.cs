using GameFramework.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetAnswerEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(GetAnswerEventArgs).GetHashCode();
    public override int Id => EventId;
    public MyAnswer MyAnswer { get; set; }
    public object UserData { get; set; }
    public override void Clear()
    {
        this.MyAnswer = null;
    }
}
