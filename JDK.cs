using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace saehyeon_mc_env
{
    internal class JDK
    {
        private static string JdkPath = Path.Combine(Constants.GetAppPath(), "bin", "jdk", "bin", "java.exe");

        public static async Task InstallJdk()
        {
            var zipPath = Path.Combine(Constants.GetTmpDir(), "jdk.zip");
            var unzipPath = Path.Combine(Constants.GetBinDir(), "jdk");
            await Downloader.DownloadFile(Config.JdkUrl, zipPath);

            // bin 폴더 보장
            await Fs.EnsureDir(Path.Combine(Constants.GetBinDir()));

            // 압축해제
            Logger.Info($"{Constants.Messages.UNZIP} \"{zipPath}\" -> \"{unzipPath}\"");
            await Zipper.Unzip(zipPath, unzipPath, strip: 1);

            // Jdk가 제대로 설치 됐는지 확인
            string jdkPath = Path.Combine(Constants.GetBinDir(), "jdk", "bin", "java.exe");

            if(!await Fs.PathExists(jdkPath))
            {
                // 제대로 설치 안됨
                Logger.Error(Constants.Messages.ERR_JDK_INSTALL_FAILED);
                Program.Close();
            }

            // 경로 설정
            JDK.SetJdkPath(jdkPath);
        }

        public static async Task InitJdkPath()
        {
            // 프로그램의 bin 폴더 안에 jdk 있으면 해당 jdk를 사용
            string jdkPath = Path.Combine(Constants.GetBinDir(), "jdk", "bin", "java.exe");

            if(await Fs.PathExists(jdkPath))
            {
                JDK.SetJdkPath(jdkPath);
            }
            else
            {
                // 없으면 컴퓨터에서 찾기
                jdkPath = Path.Combine("C:\\", "Program Files", "Java");
                bool hasJdk = false;

                foreach(var dir in Directory.EnumerateDirectories(jdkPath))
                {
                    string dirName = Path.GetFileName(dir);
                    jdkPath = Path.Combine(jdkPath, dir, "bin", "java.exe");

                    Logger.Log($"JDK 확인 중: {dirName} ({jdkPath})", output: false);

                    if (dirName.StartsWith("j"))
                    {

                        if(await Fs.PathExists(jdkPath))
                        {
                            hasJdk = true;
                            break;
                        }
                    }
                }

                // 컴퓨터에서도 JDK를 찾지 못함 -> 다운로드
                if(!hasJdk)
                {
                    Logger.Info(Constants.Messages.DOWNLOAD_JDK_CAUSED_NOT_FOUND);
                    await JDK.InstallJdk();
                }
                else
                {
                    // 컴퓨터에서 JDK를 찾음
                    Logger.Info($"{Constants.Messages.LOCAL_JDK_FOUND}");
                    SetJdkPath(jdkPath);
                }
            }
        }

        public static void SetJdkPath(string path)
        {
            // jdk 경로가 달라졌으면 설정 파일 새로 작성
            bool configSave = !JDK.JdkPath.Equals(path);

            Logger.Info($"JDK: \"{path}\"");
            JDK.JdkPath = path;

            if(configSave)
            {
                Config.Save();
            }
        }

        public static string GetJdkPath()
        {
            return JDK.JdkPath;
        }
    }
}
