using System.IO;
using System;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

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
            bool autoSelect = true
        ) {
            string filePath = Path.Combine(Constants.GetMinecraftDir(), Constants.FileStrings.LauncherProfile);

            var jsonText = "{}";

            if (await Fs.PathExists(filePath))
            {
                jsonText = await Fs.ReadAllText(filePath);
            }

            var root = JObject.Parse(jsonText);

            // profiles 속성 없으면 새로 생성
            if (!(root["profiles"] is JObject profiles))
            {
                profiles = new JObject();
                root["profiles"] = profiles;
            }


            // 새 프로필
            string uuid = Guid.NewGuid().ToString().Replace("-","");

            // 같은 이름을 가진 프로필 삭제
            var toRemove = new List<string>();

            foreach (var p in profiles.Properties())
            {
                var obj = p.Value as JObject;
                if (obj != null && obj["name"]?.ToString() == profileName)
                {
                    toRemove.Add(p.Name);
                }
            }
            foreach (var key in toRemove)
            {
                profiles.Property(key).Remove();
            }

            var newProfile = new JObject
            {
                //["icon"] = "Dirt",
                ["lastUsed"] = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'"),
                ["lastVersionId"] = lastVersionId,
                ["name"] = profileName,
                ["type"] = "custom"
            };

            profiles[uuid] = newProfile;

            var updatedJson = JsonConvert.SerializeObject(root, Formatting.Indented);
            await Fs.WriteAllText(filePath, updatedJson);
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
