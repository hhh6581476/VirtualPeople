//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.Event;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 资源校验成功事件。
    /// </summary>
    public sealed class ResourceLoadAssetSuccessEventArgs : GameEventArgs
    {
        /// <summary>
        /// 资源校验成功事件编号。
        /// </summary>
        public static readonly int EventId = typeof(ResourceLoadAssetSuccessEventArgs).GetHashCode();

        /// <summary>
        /// 初始化资源校验成功事件的新实例。
        /// </summary>
        public ResourceLoadAssetSuccessEventArgs()
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
        ///加载成功的资源
        /// </summary>
        public object Asset
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
            Asset = null;
        }
    }
}
