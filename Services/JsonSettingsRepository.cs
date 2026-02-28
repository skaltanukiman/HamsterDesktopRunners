using System;
using System.IO;
using System.Text.Json;
using HamsterDesktopRunners.Models;

namespace HamsterDesktopRunners.Services
{
    /// <summary>
    /// %AppData%/HamsterDesktopRunners/settings.json にJSONで設定を保存する実装。
    /// </summary>
    public class JsonSettingsRepository : ISettingsRepository
    {
        private static readonly string SettingsDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HamsterDesktopRunners");

        private static readonly string SettingsPath =
            Path.Combine(SettingsDir, "settings.json");

        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        public AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json, Options);
                    if (settings != null)
                    {
                        // 新しいHamsterTypeが追加された場合にデフォルト行を補完する
                        var def = AppSettings.CreateDefault();
                        foreach (var defTypeSetting in def.HamsterSettings)
                        {
                            if (!settings.HamsterSettings.Exists(s => s.Type == defTypeSetting.Type))
                                settings.HamsterSettings.Add(defTypeSetting);
                        }
                        return settings;
                    }
                }
            }
            catch { /* ファイル破損等は無視してデフォルトにフォールバック */ }

            return AppSettings.CreateDefault();
        }

        public void Save(AppSettings settings)
        {
            try
            {
                Directory.CreateDirectory(SettingsDir);
                var json = JsonSerializer.Serialize(settings, Options);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"設定の保存に失敗しました:\n{ex.Message}", "エラー");
            }
        }
    }
}
