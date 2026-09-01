using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Mane.Unity.Editor
{
    public static class Screenshot
    {
        [MenuItem("Edit/Take Game Screenshot _F10", false, 910)]
        private static void CaptureHotkey() => Capture();

        public static void Capture(string path = null)
        {
            string filePath = ResolvePath(path);
            ScreenCapture.CaptureScreenshot(filePath, 1);
            Debug.Log($"Screenshot captured: {filePath}");
        }

        private static string ResolvePath(string path)
        {
            string fileName = GetFileName();
            if (string.IsNullOrWhiteSpace(path))
                return CreateDestktopPath(fileName);

            try
            {
                string fullPath = Path.GetFullPath(path);
                if (Directory.Exists(fullPath))
                    return Path.Combine(fullPath, fileName);

                string directory = Path.GetDirectoryName(fullPath);
                if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                    return CreateDestktopPath(fileName);

                if (string.IsNullOrEmpty(Path.GetExtension(fullPath)))
                    return CreateDestktopPath(fileName);

                return fullPath;
            }
            catch (Exception)
            {
                return CreateDestktopPath(fileName);
            }
        }

        private static string GetFileName()
        {
            DateTime t = DateTime.Now;
            return $"Screenshot_{t.Year}_{t.Month:00}_{t.Day:00}_{t.Hour:00}_{t.Minute:00}_{t.Second:00}.png";
        }

        private static string CreateDestktopPath(string fileName) =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
    }
}
