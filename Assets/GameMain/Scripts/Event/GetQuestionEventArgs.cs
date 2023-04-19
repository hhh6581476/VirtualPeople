using GameFramework.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetQuestionEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(GetQuestionEventArgs).GetHashCode();
    public override int Id => EventId;
    public string  question { get; set; }
    public object UserData { get; set; }
    public override void Clear()
    {
        this.question = null;
    }
}
