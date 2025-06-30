using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace saehyeon_mc_env
{
    public class Config
    {
        public static string AppName = "saehyeon-mc-env";
        public static string AppVersion = "1.0.2";
        public static string JdkUrl = "https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.7%2B6/OpenJDK21U-jdk_x64_windows_hotspot_21.0.7_6.zip";
        public static string RepoUrl = "https://https://github.com/Junyoung1201/saehyeon-mc-env";
        public static bool BackupBeforeInstall = true;

        public static string GetConfigFile()
        {
            return Path.Combine(Constants.GetAppPath(), "config.json");
        }

        public static void Save()
        {
            Logger.Info(Constants.Messages.SAVE_CONFIG);

            try
            {
                JsonUtils.WriteFileSync(Config.GetConfigFile(), new
                {
                    appName = AppName,
                    appVersion = AppVersion,
                    jdkUrl = JdkUrl,
                    repo = RepoUrl,
                    jdkPath = JDK.GetJdkPath(),
                    backupBeforeInstall = BackupBeforeInstall
                });
            }
            catch (Exception e)
            {
                Logger.Error(Constants.Messages.ERR_SAVE_CONFIG_FAILED);
                Program.Error(e);
            }
        }

        public static void Ensure()
        {
            if (!Fs.PathExistsSync(GetConfigFile()))
            {
                Config.Save();
            }
        }

        public static void Load()
        {
            string configFile = GetConfigFile();

            try
            {
                Logger.Log(Constants.Messages.CONFIG_LOAD);
                var configJson = JsonUtils.ReadFileSync(configFile);

                Config.AppName = (string)(configJson["appName"] ?? AppName);
                Config.AppVersion = (string)(configJson["appVersion"] ?? AppVersion);
                Config.JdkUrl = (string)(configJson["jdkUrl"] ?? JdkUrl);
                Config.RepoUrl = (string)(configJson["repo"] ?? RepoUrl);
                Config.BackupBeforeInstall = (bool)(configJson["backupBeforeInstall"] ?? BackupBeforeInstall);
                // jdk path
                JDK.SetJdkPath((string)configJson["jdkPath"]);

            }
            catch (Exception e)
            {
                Logger.Error(Constants.Messages.ERR_CONFIG_LOAD_FAILED);
                Program.Error(e);
            }
        }
    }
}
