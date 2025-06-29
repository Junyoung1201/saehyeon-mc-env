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
            Console.ReadKey();
            Environment.Exit(0);
        }

        async static Task Main(string[] args)
        {
            Logger.StartWriteFile("installer.log");
            Logger.SetPrefixVisible(false);

            Logger.Log("행복한 다람쥐가 되고 싶다.");

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

            // 모드팩 파일 경로가 제대로 설정됐는지 확인
            if(string.IsNullOrWhiteSpace(modpackPath))
            {
                Logger.Error(Constants.Messages.ERR_WRONG_MODPACK_PATH);
                Close();
            }

            // 모드팩 파일 존재하는지 확인
            if (!await Fs.PathExists(modpackPath))
            {
                Logger.Error(Constants.Messages.ERR_MODPACK_NOT_EXIST);
                Close();
            }

            // 모드팩이 올바른 압축 파일인지 검증
            if(!await Zipper.VerifyZip(modpackPath))
            {
                Logger.Error(Constants.Messages.ERR_WRONG_MODPACK);
                Close();
            }

            // jdk 확인
            if(!await Fs.PathExists(Constants.GetJdkPath()))
            {
                Logger.Error(Constants.Messages.ERR_JDK_NOT_EXIST);
                Close();
            }

            // 임시 폴더 보장 및 초기화
            await Fs.EnsureDir(Constants.GetTmpDir());
            await Fs.EmptyDir(Constants.GetTmpDir());

            // 모드팩 설치 전 백업
            await Modpack.CreateBackupModpack();

            // 모드팩 설치 시작
            await Modpack.Install(modpackPath);
        }
    }
}
