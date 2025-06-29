using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

public static class Zipper
{
    public static void ZipDirSync(string src, string dest)
    {
        if (!Directory.Exists(src))
            throw new DirectoryNotFoundException($"압축 대상 폴더를 찾을 수 없습니다: {src}");

        // 만약 dest 파일이 이미 존재하면 덮어쓰기
        if (File.Exists(dest))
            File.Delete(dest);

        ZipFile.CreateFromDirectory(src, dest, CompressionLevel.Optimal, includeBaseDirectory: false);
    }

    public static void UnzipSync(string src, string dest)
    {
        if (!File.Exists(src))
            throw new FileNotFoundException($"ZIP 파일을 찾을 수 없습니다: {src}");

        // 대상 폴더가 없으면 생성
        if (!Directory.Exists(dest))
            Directory.CreateDirectory(dest);

        ZipFile.ExtractToDirectory(src, dest);
    }

    public static bool VerifyZipSync(string path)
    {
        if (!File.Exists(path))
            return false;

        try
        {
            // 열어서 항목을 하나라도 읽어보면 유효성 검증
            using (var archive = ZipFile.OpenRead(path))
            {
                foreach (var entry in archive.Entries)
                {
                    // 아무 처리를 하지 않고 열기 성공 여부만 체크
                    break;
                }
            }
            return true;
        }
        catch (InvalidDataException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static Task ZipDir(string src, string dest)
    {
        return Task.Run(() => ZipDirSync(src, dest));
    }

    public static Task Unzip(string src, string dest)
    {
        return Task.Run(() => UnzipSync(src, dest));
    }

    public static Task<bool> VerifyZip(string path)
    {
        return Task.Run(() => VerifyZipSync(path));
    }
}
