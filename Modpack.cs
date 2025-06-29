using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace saehyeon_mc_env
{
    internal class Modpack
    {
        public static async Task<bool> VerifyInstall(string name)
        {
            string src = Path.Combine(Constants.GetTmpDir(), "modpack", name);
            string dest = Path.Combine(Constants.GetMinecraftDir(), name);

            return await Fs.Verify(src,dest);
        }

        public static async Task ApplyToMinecraft(string name)
        {
            string src = Path.Combine(Constants.GetTmpDir(), "modpack", name);
            string dest = Path.Combine(Constants.GetMinecraftDir(), name);

            try
            {
                if (await Fs.IsDirectory(src))
                {
                    await Fs.EnsureDir(dest);
                    await Fs.EmptyDir(dest);
                }
                else
                {
                    await Fs.Remove(dest);
                }

                Logger.Info($"{Constants.Messages.COPY}: \"{src}\" -> \"{dest}\"");
                await Fs.Copy(src, dest);
            }
            catch (Exception e)
            {
                Logger.Error($"{Constants.Messages.ERR_MODPACK_APPLY_FAILED} \"{name}\"");
                Logger.Error(e.Message);
                Program.Close();
            }
        }

        public static async Task CreateBackupModpack()
        {
            DateTime now = DateTime.Now;
            string y = now.Year.ToString().PadLeft(4, '0');
            string m = now.Month.ToString().PadLeft(2, '0');
            string d = now.Date.ToString().PadLeft(2, '0');

            string hh = now.Hour.ToString().PadLeft(2, '0');
            string mm = now.Minute.ToString().PadLeft(2, '0');
            string ss = now.Second.ToString().PadLeft(2, '0');

            string backupDir = Path.Combine(Constants.GetTmpDir(), y + m + d + " " + hh + mm + ss);

            // 백업 모드팩의 data.json 작성
            var dataJson = new
            {
                name = $"{y}-{m}-{d} {hh}:{mm}:{ss} 백업",
                type = "backup"
            };

            JsonUtil.WriteToFile(dataJson, Path.Combine(backupDir, "data.json"));

            // 압축
            await Fs.EnsureDir(Constants.GetBackupDir());

            string zipFile = Path.Combine(Constants.GetBackupDir(), backupDir+".zip");
            Logger.Info($"{Constants.Messages.CREATEING_BAK_MODPACK}(\"{zipFile}\")");
            try
            {
                await Zipper.ZipDir(backupDir, zipFile);
            }
            catch (Exception e)
            {
                Logger.Error(Constants.Messages.ERR_BAK_CREATE_FAILED);
                Logger.Error(e.Message);
                Program.Close();
            }
            Logger.Info($"{Constants.Messages.CREATE_BAK_MODPACK_COMPLETE} (위치: \"{zipFile}\")");
        }

        public static async Task Install(string path)
        {
            // 모드팩을 임시 폴더에 압축 해제
            string modpackDir = Path.Combine(Constants.GetTmpDir(), "modpack");
            Logger.Log($"{Constants.Messages.UNZIP} \"{path}\" -> \"{modpackDir}\"");

            try
            {
                await Zipper.Unzip(path, modpackDir);
            } 
            catch (Exception e)
            {
                Logger.Error(e.Message);
                Program.Close();
            }

            // 모드팩 데이터 가져오기
            Logger.Log(Constants.Messages.CHECKING_MODPACK_DATA);
            string dataJsonFile = Path.Combine(modpackDir, "data.json");

            if(!await Fs.PathExists(dataJsonFile))
            {
                Logger.Error(Constants.Messages.ERR_MODPACK_DATA_NOT_EXIST);
                Program.Close();
            }

            var data = JsonUtil.ReadFromFile<dynamic>(path);

            string name = data.name;
            string type = data.type;
            string version = data.version;
            string forgeName = data.forgeName;
            string mpRpDir = Path.Combine(modpackDir, "resourcepacks");
            string mpModsDir = Path.Combine(modpackDir, "mods");
            string mpConfigDir = Path.Combine(modpackDir, "config");
            string mpSpDir = Path.Combine(modpackDir, "shaderpacks");
            string mpOptionsFile = Path.Combine(modpackDir, "options.txt");
            string mpForgeFile = Path.Combine(modpackDir, "forge.jar");
            bool isBackup = type.Equals("backup");

            // 타입이 backup 이면 백업본을 복구하는 거임
            if (isBackup)
            {
                Logger.Info($"{name}{Constants.Messages.BAK_START}");

                // modpackDir에 있는 것들 바로 적용
                await Modpack.ApplyToMinecraft("mods");
                await Modpack.ApplyToMinecraft("resourcepacks");
                await Modpack.ApplyToMinecraft("config");
                await Modpack.ApplyToMinecraft("shaderpacks");
                await Modpack.ApplyToMinecraft("options.txt");

                Logger.Info($"{name}{Constants.Messages.BAK_COMPLETE}");
                Program.Close();
            }

            // 백업본으로 복구하는 경우가 아닌 경우
            if (string.IsNullOrWhiteSpace(name))
            {
                Logger.Warn(Constants.Messages.WARN_MODPACK_NAME_NOT_EXIST);
            }

            if (string.IsNullOrWhiteSpace(version))
            {
                Logger.Error(Constants.Messages.ERR_MODPACK_VERSION_NOT_EXIST);
                Program.Close();
            }

            // 바닐라 버전이 존재하는지 확인
            if (!await Minecraft.CheckVanilla(version))
            {
                try
                {
                    await Minecraft.InstallVanilla(version);
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                    Program.Close();
                }
            }
            else
            {
                Logger.Info(Constants.Messages.SKIP_VANILLA_INSTALL);
            }

            // 포지 확인
            string forgeJarFile = Path.Combine(modpackDir, "forge.jar");

            if (await Fs.PathExists(forgeJarFile))
            {
                // 포지 설치 파일은 있는데 data.json에 forgeName 값이 없음
                if (string.IsNullOrWhiteSpace(forgeName))
                {
                    Logger.Error(Constants.Messages.ERR_FORGE_NAME_NOT_EXIST);
                    Program.Close();
                }

                string forgeVersionFile = Path.Combine(Constants.GetMinecraftDir(), "versions", forgeName, forgeName + ".json");

                if(!await Fs.PathExists(forgeVersionFile))
                {
                    Logger.Info(Constants.Messages.INSTALLING_FORGE);

                    // 런처 프로필 없으면 더미 파일 생성
                    string launcherProfile = Path.Combine(Constants.GetMinecraftDir(), "launcher_profiles.json");

                    if (!await Fs.PathExists(launcherProfile))
                    {
                        Logger.Info(Constants.Messages.CREATE_LAUNCHER_PROFILE_DUMMY);
                        File.WriteAllText(launcherProfile, "{}");
                    }

                    // 포지 설치 시작
                    try
                    {
                        await ShellUtil.RunAsync(
                            $"\"{Constants.GetJdkPath()}\"",
                            $"-jar \"{forgeJarFile}\" --install-client"
                        );
                    }
                    catch (Exception e)
                    {
                        Logger.Error(Constants.Messages.ERR_FORGE_INSTALL_ERROR);
                        Logger.Error(e.Message);
                        Program.Close();
                    }

                    // 포지가 제대로 설치되었는지 확인
                    if (!await Fs.PathExists(forgeVersionFile))
                    {
                        Logger.Error(Constants.Messages.ERR_FORGE_INSTALL_FAILED);
                        Program.Close();
                    }
                }
                else
                {
                    Logger.Info(Constants.Messages.FORGE_EXIST);
                }
            }

            // mods, resourcepacks, shaderpacks, config, options.txt 교체
            await Modpack.ApplyToMinecraft(Constants.FileStrings.Mods);
            await Modpack.ApplyToMinecraft(Constants.FileStrings.Resourcepacks);
            await Modpack.ApplyToMinecraft(Constants.FileStrings.Shaderpacks);
            await Modpack.ApplyToMinecraft(Constants.FileStrings.Config);
            await Modpack.ApplyToMinecraft(Constants.FileStrings.OptionsTxt);

            Logger.Info(Constants.Messages.VERIFYING_INSTALLATION);

            // 모드팩이 제대로 적용되었는지 확인
            if(!await Modpack.VerifyInstall(Constants.FileStrings.Mods))
            {
                Logger.Error(Constants.Messages.ERR_MODS_APPLY_FAILED);
                Program.Close();
            }
            if (!await Modpack.VerifyInstall(Constants.FileStrings.Resourcepacks))
            {
                Logger.Error(Constants.Messages.ERR_RP_APPLY_FAILED);
                Program.Close();
            }
            if (!await Modpack.VerifyInstall(Constants.FileStrings.Shaderpacks))
            {
                Logger.Error(Constants.Messages.ERR_SP_APPLY_FAILED);
                Program.Close();
            }
            if (!await Modpack.VerifyInstall(Constants.FileStrings.Config))
            {
                Logger.Error(Constants.Messages.ERR_CONFIG_APPLY_FAILED);
                Program.Close();
            }
            if (!await Modpack.VerifyInstall(Constants.FileStrings.OptionsTxt))
            {
                Logger.Error(Constants.Messages.ERR_OPTIONS_APPLY_FAILED);
                Program.Close();
            }

            // 런처 프로필에 모드팩 프로필 추가
            Logger.Info(Constants.Messages.MODIFY_LAUNCHER_PROFILE);
            try
            {
                await Minecraft.AddLauncherProfile(name ?? Constants.Messages.MODPACK_NAME_EMPTY, version, autoSelect: true);
            }
            catch (Exception e)
            {
                Logger.Error(Constants.Messages.ERR_MODIFY_LAUNCHER_PROFILE_FAILED);
                Logger.Error(e.Message);
                Program.Close();
            }

            // 정리
            Logger.Info(Constants.Messages.CLEAN_UP);

            try
            {
                await Fs.Remove(Constants.GetTmpDir());
            }
            catch (Exception e)
            {
                Logger.Warn(Constants.Messages.ERR_CLEAN_UP_FAILED);
                Logger.Warn(e.Message);
            }

            Console.Clear();
            Logger.Info($"{Constants.Messages.MODPACK_INSTALL_COMPLETE} {name}");
            Program.Close();
        }
    }
}
