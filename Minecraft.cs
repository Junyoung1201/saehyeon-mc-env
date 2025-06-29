using System.IO;
using System;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

namespace saehyeon_mc_env
{
    internal class Minecraft
    {
        public static async Task<bool> CheckVanilla(string version)
        {
            return await Fs.PathExists(Path.Combine(Constants.GetMinecraftDir(), "versions", version));
        }

        public static async Task AddLauncherProfile(
            string profileName,
            string lastVersionId,
            string gameDirectory = null,
            string javaArgs = null,
            bool autoSelect = true
        ) {
            string filePath = Path.Combine(Constants.GetMinecraftDir(), Constants.FileStrings.LauncherProfile);

            string json = await Fs.ReadAllText(filePath);
            var root = JObject.Parse(json);

            // profiles 객체 가져오고 없으면 생성
            if (!(root["profiles"] is JObject profiles))
            {
                profiles = new JObject();
                root["profiles"] = profiles;
            }

            // json에 새 프로필 추가
            string newProfileId = Guid.NewGuid().ToString();
            string defaultGameDir = gameDirectory
                ?? Path.Combine(Constants.GetMinecraftDir(), profileName);

            var newProfile = new JObject
            {
                ["id"] = newProfileId,
                ["name"] = profileName,
                ["lastVersionId"] = lastVersionId,
                ["gameDir"] = defaultGameDir,
                ["javaArgs"] = javaArgs ?? string.Empty,
            };

            profiles[newProfileId] = newProfile;

            // 프로필 자동 선택
            if(autoSelect)
            {
                root["selectedProfile"] = newProfileId;
            }

            // 파일 쓰기
            string output = root.ToString(Formatting.Indented);
            await Fs.WriteAllText(filePath, output);
        }

        public static async Task InstallVanilla(string version)
        {
            Logger.Info($"{version} 버전의 바닐라 마인크래프트 클라이언트 설치 중");
            // 버전 전체 목록 가져오기
            string manifestUrl = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
            string manifestJson;
            using (var wc = new WebClient())
            {
                manifestJson = await wc.DownloadStringTaskAsync(manifestUrl);
            }

            // 버전 정보 가져오기
            var manifest = JObject.Parse(manifestJson);
            var versionEntry = manifest["versions"]
                .FirstOrDefault(v => v["id"].Value<string>().Equals(version, StringComparison.OrdinalIgnoreCase));

            if (versionEntry == null)
            {
                Logger.Error($"\"{version}\"(이)라는 마인크래프트 버전을 찾을 수 없습니다.");
                Program.Close();
            }

            // 버전 url 가져오기
            string versionJsonUrl = versionEntry["url"].Value<string>();

            // 버전 json 다운로드
            string versionJson;
            using (var wc = new WebClient())
            {
                versionJson = await wc.DownloadStringTaskAsync(versionJsonUrl);
            }
            var versionData = JObject.Parse(versionJson);

            // 버전 폴더 생성
            string versionDir = Path.Combine(Constants.GetMinecraftDir(), "versions", version);
            await Fs.EnsureDir(versionDir);

            // 버전 json 저장
            string versionJsonPath = Path.Combine(versionDir, version + ".json");
            File.WriteAllText(versionJsonPath, versionData.ToString());

            // 클라이언트 jar 파일 다운로드
            string clientJarUrl = versionData["downloads"]["client"]["url"].Value<string>();
            string clientJarPath = Path.Combine(versionDir, version + ".jar");
            using (var wc = new WebClient())
            {
                await wc.DownloadFileTaskAsync(clientJarUrl, clientJarPath);
            }

            Logger.Info($"{version} 버전의 바닐라 마인크래프트 클라이언트를 설치했습니다.");
        }
    }
}
