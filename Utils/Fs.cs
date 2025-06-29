using System.IO;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

public static class Fs
{
    public static void EmptyDirSync(string path)
    {
        if (!Directory.Exists(path))
            return;

        var dirInfo = new DirectoryInfo(path);

        // 파일 삭제
        foreach (var file in dirInfo.GetFiles())
        {
            file.IsReadOnly = false;
            file.Delete();
        }

        // 폴더 삭제
        foreach (var subDir in dirInfo.GetDirectories())
        {
            subDir.Attributes &= ~FileAttributes.ReadOnly;
            EmptyDirSync(subDir.FullName);
            subDir.Delete(true);
        }
    }

    public static bool PathExistsSync(string path)
    {
        return File.Exists(path) || Directory.Exists(path);
    }

    public static bool IsDirectorySync(string path)
    {
        if (!File.Exists(path) && !Directory.Exists(path))
            throw new FileNotFoundException("해당 경로가 존재하지 않습니다.", path);

        FileAttributes attr = File.GetAttributes(path);
        return (attr & FileAttributes.Directory) == FileAttributes.Directory;
    }

    public static void EnsureDirSync(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("경로가 비어있습니다.", nameof(path));

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static void CopySync(string src, string dest, bool overwrite = true)
    {
        if (IsDirectorySync(src))
        {
            CopyDirSync(src, dest);
        }
        else
        {
            File.Copy(src, dest, overwrite);
        }
    }

    public static void CopyDirSync(string src, string dest)
    {
        if (!Directory.Exists(src))
            throw new DirectoryNotFoundException($"\"{src}\" 경로를 찾을 수 없습니다.");

        if (!Directory.Exists(dest))
            Directory.CreateDirectory(dest);

        foreach (string file in Directory.EnumerateFiles(src))
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(dest, fileName);
            File.Copy(file, destFile, overwrite: true);
        }

        foreach (string subDir in Directory.EnumerateDirectories(src))
        {
            string dirName = Path.GetFileName(subDir);
            string destSubDir = Path.Combine(dest, dirName);
            CopyDirSync(subDir, destSubDir);
        }
    }

    public static Task EmptyDir(string path)
    {
        return Task.Run(() => EmptyDirSync(path));
    }

    public static Task<bool> PathExists(string path)
    {
        return Task.FromResult(PathExistsSync(path));
    }

    public static Task<bool> IsDirectory(string path)
    {
        return Task.FromResult(IsDirectorySync(path));
    }

    public static Task EnsureDir(string path)
    {
        // 유효성 검사는 동기적으로 즉시 처리
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("경로가 비어있습니다.", nameof(path));

        return Task.Run(() => EnsureDirSync(path));
    }

    public async static Task Copy(string src, string dest)
    {
        if (await IsDirectory(src))
        {
            await CopyDir(src, dest);
            return;
        }
            

        await CopyFile(src, dest);
        return;
    }

    private static async Task CopyFile(string src, string dest)
    {
        // dest 디렉터리가 없으면 생성
        var destDir = Path.GetDirectoryName(dest);
        if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
            Directory.CreateDirectory(destDir);

        using (var sourceStream = new FileStream(
            src, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 81920, useAsync: true))
        using (var destStream = new FileStream(
            dest, FileMode.Create, FileAccess.Write, FileShare.None,
            bufferSize: 81920, useAsync: true))
        {
            await sourceStream.CopyToAsync(destStream);
        }
    }

    public static async Task CopyDir(string src, string dest)
    {
        if (!Directory.Exists(src))
            throw new DirectoryNotFoundException($"\"{src}\" 경로를 찾을 수 없습니다.");

        if (!Directory.Exists(dest))
            Directory.CreateDirectory(dest);

        // 파일 복사
        foreach (var file in Directory.GetFiles(src))
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(dest, fileName);
            await CopyFile(file, destFile);
        }

        // 서브디렉터리 재귀 복사
        foreach (var subDir in Directory.GetDirectories(src))
        {
            var dirName = Path.GetFileName(subDir);
            var destSubDir = Path.Combine(dest, dirName);
            await CopyDir(subDir, destSubDir);
        }
    }

    public static void RemoveSync(string path)
    {
        if (!PathExistsSync(path))
            return;

        if (IsDirectorySync(path))
        {
            var dirInfo = new DirectoryInfo(path);

            foreach (var file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
            {
                file.IsReadOnly = false;
            }
            dirInfo.Attributes &= ~FileAttributes.ReadOnly;

            Directory.Delete(path, recursive: true);
        }
        else
        {
            var fileInfo = new FileInfo(path);
            fileInfo.IsReadOnly = false;
            File.Delete(path);
        }
    }

    public static Task Remove(string path)
    {
        return Task.Run(() => RemoveSync(path));
    }

    public static bool VerifySync(string path1, string path2)
    {
        // 존재 여부 체크
        bool exists1 = PathExistsSync(path1);
        bool exists2 = PathExistsSync(path2);
        if (!exists1 || !exists2)
            return false;

        bool isDir1 = IsDirectorySync(path1);
        bool isDir2 = IsDirectorySync(path2);

        // 하나는 파일, 하나는 디렉터리면 false
        if (isDir1 != isDir2)
            return false;

        if (!isDir1) // 둘 다 파일
        {
            return CompareFiles(path1, path2);
        }
        else // 둘 다 디렉터리
        {
            // 디렉터리 내부 모든 파일 목록을 상대경로로 가져오기
            var files1 = GetAllFilesRelative(path1);
            var files2 = GetAllFilesRelative(path2);

            // 파일 목록이 다르면 false
            if (files1.Count != files2.Count || !new HashSet<string>(files1).SetEquals(files2))
                return false;

            // 각 파일을 하나씩 비교
            foreach (var relPath in files1)
            {
                string f1 = Path.Combine(path1, relPath);
                string f2 = Path.Combine(path2, relPath);
                if (!CompareFiles(f1, f2))
                    return false;
            }
            return true;
        }
    }

    public static async Task WriteAllText(string path, string content, Encoding encoding = null)
    {
        encoding = encoding ?? Encoding.UTF8;
        using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
        using (var writer = new StreamWriter(fs, encoding))
        {
            await writer.WriteAsync(content).ConfigureAwait(false);
        }
    }

    public static async Task<string> ReadAllText(string path, Encoding encoding = null)
    {
        encoding = encoding ?? Encoding.UTF8;
        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
        using (var reader = new StreamReader(fs, encoding))
        {
            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }
    }

    public static Task<bool> Verify(string path1, string path2)
    {
        return Task.FromResult(VerifySync(path1, path2));
    }

    private static bool CompareFiles(string file1, string file2)
    {
        var fi1 = new FileInfo(file1);
        var fi2 = new FileInfo(file2);

        if (fi1.Length != fi2.Length)
            return false;

        const int bufferSize = 81920;
        byte[] buf1 = new byte[bufferSize];
        byte[] buf2 = new byte[bufferSize];

        using (var fs1 = new FileStream(file1, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var fs2 = new FileStream(file2, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            int read1, read2;
            while ((read1 = fs1.Read(buf1, 0, bufferSize)) > 0)
            {
                read2 = fs2.Read(buf2, 0, bufferSize);
                if (read1 != read2)
                    return false;
                for (int i = 0; i < read1; i++)
                {
                    if (buf1[i] != buf2[i])
                        return false;
                }
            }
        }
        return true;
    }

    private static List<string> GetAllFilesRelative(string root)
    {
        var list = new List<string>();
        int rootLen = root.EndsWith(Path.DirectorySeparatorChar.ToString())
            ? root.Length
            : root.Length + 1;

        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            list.Add(file.Substring(rootLen));
        }

        return list;
    }
}