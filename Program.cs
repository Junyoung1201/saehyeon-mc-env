using System;
using System.IO;
using System.Threading.Tasks;

namespace saehyeon_mc_env
{
    internal class Program
    {
        public static void Close()
        {
            Logger.Close();

            // tmp 폴더 삭제
            Fs.RemoveSync(Constants.GetTmpDir());
            Console.ReadKey();
            Environment.Exit(0);
        }

        public static void Error(Exception e)
        {
            Logger.Error(e.Message + "\n" + e.StackTrace);
            Program.Close();
        }

        async static Task Main(string[] args)
        {
            Logger.StartWriteFile("saehyeon-mc-env.log");
            Logger.SetPrefixOutput(false);
            Logger.SetWritePrefix(true);

            Logger.Log("행복한 다람쥐가 되고 싶다.", output: false);

            if (args.Length == 0)
            {
                Logger.Error(Constants.Messages.ERR_MODPACK_NOT_INPUT);
                Close();
            }

            string modpackPath = "";
            
            // 모드팩 파일을 절대경로로 받기
            foreach (var path in args)
            {
                modpackPath = Path.GetFullPath(path);
            }

            Logger.Log($"modpackPath = \"{modpackPath}\"", output: false);

            // 모드팩 파일 검사
            await Modpack.Verify(modpackPath);

            // 설정 파일 보장
            Config.Ensure();

            // 설정 파일 불러오기
            Config.Load();

            // jdk 보장
            await JDK.InitJdkPath();

            // bin 폴더 보장
            await Fs.EnsureDir(Constants.GetBinDir());

            // 모드팩 설치 시작
            try
            {
                await Modpack.Install(modpackPath);
            }
            catch (Exception e)
            {
                Logger.Error(Constants.Messages.ERR_MODPACK_EXCPETION);
                Error(e);
            }
        }
    }
}
