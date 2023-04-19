//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.Event;
using GameFramework.Resource;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 资源校验成功事件。
    /// </summary>
    public sealed class ResourceLoadAssetFailureEventArgs : GameEventArgs
    {
        /// <summary>
        /// 资源校验成功事件编号。
        /// </summary>
        public static readonly int EventId = typeof(ResourceLoadAssetFailureEventArgs).GetHashCode();

        /// <summary>
        /// 初始化资源校验成功事件的新实例。
        /// </summary>
        public ResourceLoadAssetFailureEventArgs()
        {
            
        }


        /// <summary>
        /// 获取资源名称。
        /// </summary>
        public string AssetName
        {
            get;
            set;
        }


        /// <summary>
        /// 加载错误信息
        /// </summary>
        public string ErrorMessage
        {
            get;
            set;
        }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData
        {
            get;
            set;
        }

        public override int Id
        {
            get { return EventId; }
        }

        /// <summary>
        /// 清理资源校验成功事件。
        /// </summary>
        public override void Clear()
        {
            AssetName = null;
            UserData = null;
            ErrorMessage =null;
        }
    }
}
