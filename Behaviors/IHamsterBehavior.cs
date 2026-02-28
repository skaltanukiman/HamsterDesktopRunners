using System.Collections.Generic;
using System.Windows;
using HamsterDesktopRunners.Models;

namespace HamsterDesktopRunners.Behaviors
{
    /// <summary>
    /// ハムスターの行動モジュールの共通インターフェース。
    /// 各Behaviorは毎フレームUpdateが呼ばれ、Hamsterの状態を変化させる。
    /// </summary>
    public interface IHamsterBehavior
    {
        /// <summary>Behaviorを一意に識別するID（例: "move", "eat"）</summary>
        string Id { get; }

        /// <summary>UI表示用の名称</summary>
        string DisplayName { get; }

        /// <summary>有効/無効フラグ</summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// 毎フレーム呼ばれる更新処理。
        /// </summary>
        void Update(Hamster hamster, List<Rect> screens);
    }
}
