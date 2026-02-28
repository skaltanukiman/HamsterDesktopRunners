using HamsterDesktopRunners.Models;

namespace HamsterDesktopRunners.Services
{
    /// <summary>
    /// 設定の永続化を抽象化するインターフェース。
    /// 将来DB化・クラウド保存等に差し替え可能。
    /// </summary>
    public interface ISettingsRepository
    {
        AppSettings Load();
        void Save(AppSettings settings);
    }
}
