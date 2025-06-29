using System;
using System.IO;
using System.Reflection;

namespace saehyeon_mc_env
{
    internal class Constants
    {
        public class Messages
        {
            public static string ERR_MODS_APPLY_FAILED = "모드가 제대로 적용되지 않았습니다.";
            public static string ERR_RP_APPLY_FAILED = "리소스팩이 제대로 적용되지 않았습니다.";
            public static string ERR_SP_APPLY_FAILED = "쉐이더가 제대로 적용되지 않았습니다.";
            public static string ERR_CONFIG_APPLY_FAILED = "모드 설정이 제대로 적용되지 않았습니다.";
            public static string ERR_OPTIONS_APPLY_FAILED = "게임 설정이 제대로 적용되지 않았습니다.";
            public static string ERR_FORGE_INSTALL_FAILED = "포지가 제대로 설치되지 않았습니다.";
            public static string ERR_FORGE_INSTALL_ERROR = "포지 설치 중 오류가 발생했습니다.";
            public static string ERR_FORGE_NAME_NOT_EXIST = "모드팩 데이터에 forgeName 값이 없습니다. 모드팩 제작자가 data.json를 검토해야할 수 있습니다.";
            public static string FORGE_EXIST = "이미 해당 버전의 포지가 설치되어 있습니다. 설치를 스킵합니다.";
            public static string CREATE_LAUNCHER_PROFILE_DUMMY = "런처 프로필 더미 파일 생성 중";
            public static string SKIP_VANILLA_INSTALL = "이미 해당 버전의 바닐라 마인크래프트가 설치되어 있습니다. 설치를 스킵합니다.";
            public static string ERR_MODPACK_VERSION_NOT_EXIST = "모드팩의 마인크래프트 버전을 확인할 수 없습니다.";
            public static string ERR_MODPACK_DATA_NOT_EXIST = "모드팩 정보를 가져올 수 없습니다. 모드팩에 data.json이 없습니다.";
            public static string BAK_START = "으로 마인크래프트를 복구합니다.";
            public static string BAK_COMPLETE = "으로 마인크래프트를 복구했습니다.";
            public static string WARN_MODPACK_NAME_NOT_EXIST = "모드팩 이름을 확인할 수 없습니다.";
            public static string ERR_JDK_NOT_EXIST = "JDK가 없습니다.";
            public static string ERR_BAK_CREATE_FAILED = "백업 용 모드팩 생성에 실패했습니다.";
            public static string CREATEING_BAK_MODPACK = "백업본을 제작 중입니다.";
            public static string CREATE_BAK_MODPACK_COMPLETE = "백업 용 모드팩을 생성했습니다.";
            public static string ERR_MODPACK_APPLY_FAILED = "모드팩 구성요소 적용에 실패했습니다:";
            public static string ERR_WRONG_MODPACK = "손상되었거나 올바르지 않은 모드팩입니다.";
            public static string ERR_MODPACK_NOT_EXIST = "모드팩 경로를 찾을 수 없습니다.";
            public static string ERR_WRONG_MODPACK_PATH = "모드팩 경로가 올바르지 않습니다.";
            public static string ERR_MODPACK_NOT_INPUT = "모드팩이 프로그램 위에 끌어다 놓아져야 합니다.";
            public static string UNZIP = "압축해제:";
            public static string COPY = "복사 중:";
            public static string CHECKING_MODPACK_DATA = "모드팩 정보 확인 중";
            public static string INSTALLING_FORGE = "포지 설치 중";
            public static string VERIFYING_INSTALLATION = "무결성 검증 중";
            public static string MODIFY_LAUNCHER_PROFILE = "런처 프로필 작성 중";
            public static string CLEAN_UP = "정리 중";
            public static string ERR_CLEAN_UP_FAILED = "정리 중 오류가 발생했습니다. 정리를 스킵합니다.";
            public static string ERR_MODIFY_LAUNCHER_PROFILE_FAILED = "런처 프로필 작성 중 오류가 발생했습니다.";
            public static string MODPACK_INSTALL_COMPLETE = "\n\n\t\t설치를 완료했습니다:";
            public static string MODPACK_NAME_EMPTY = "이름없는 모드팩";
        }

        public class FileStrings
        {
            public static string Mods = "mods";
            public static string Resourcepacks = "resourcepacks";
            public static string Shaderpacks = "shaderpacks";
            public static string Config = "config";
            public static string OptionsTxt = "options.txt";
            public static string LauncherProfile = "launcher_profiles.json";
        }

        public static string GetJdkPath()
        {
            return Path.Combine(Constants.GetBinDir(), "jdk", "bin", "java.exe");
        }

        public static string GetTmpDir()
        {
            return Path.Combine(Constants.GetAppPath(), "tmp");
        }

        public static string GetBinDir()
        {
            return Path.Combine(Constants.GetAppPath(), "bin");
        }

        public static string GetBackupDir()
        {
            return Path.Combine(Constants.GetAppPath(), "backup");
        }

        public static string GetMinecraftDir()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, ".minecraft");
        }

        public static string GetAppPath()
        {
            string fullPath = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(fullPath);
        }
    }
}
