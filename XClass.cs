using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace UnityEditor.XCodeEditor
{
    public partial class XClass : System.IDisposable
    {
        private string filePath;

        public XClass(string fPath)
        {
            filePath = fPath;
            if (!System.IO.File.Exists(filePath))
            {
                Debug.LogError(filePath + "路径下文件不存在");

                return;
            }
        }

        public void WriteBelow(string below, string text)
        {
            StreamReader streamReader = new StreamReader(filePath);
            string text_all = streamReader.ReadToEnd();
            streamReader.Close();

            int beginIndex = text_all.IndexOf(below);
            if (beginIndex == -1)
            {
                Debug.LogError(filePath + "中没有找到标志" + below);

                return;
            }

            int endIndex = text_all.LastIndexOf("\n", beginIndex + below.Length);

            text_all = text_all.Substring(0, endIndex) + "\n" + text + "\n" + text_all.Substring(endIndex);

            StreamWriter streamWriter = new StreamWriter(filePath);
            streamWriter.Write(text_all);
            streamWriter.Close();
        }

        public void Replace(string below, string newText)
        {
            StreamReader streamReader = new StreamReader(filePath);
            string text_all = streamReader.ReadToEnd();
            streamReader.Close();

            int beginIndex = text_all.IndexOf(below);

            if (beginIndex == -1)
            {
                Debug.LogError(filePath + "中没有找到标志" + below);

                return;
            }

            text_all = text_all.Replace(below, newText);
            StreamWriter streamWriter = new StreamWriter(filePath);
            streamWriter.Write(text_all);
            streamWriter.Close();
        }

        public void Dispose()
        {

        }
    }


    public partial class XCPlist : System.IDisposable
    {
        List<string> contents = new List<string>();
        partial void Init()
        {
            if (!System.IO.File.Exists(plistPath))
            {
                Debug.LogError(plistPath + "路径下文件不存在");
                return;
            }

            FileInfo projectFileInfo = new FileInfo(plistPath);
            StreamReader sr = projectFileInfo.OpenText();
            while (sr.Peek() >= 0)
            {
                contents.Add(sr.ReadLine());
            }
            sr.Close();

        }
        public void AddKey(string key)
        {
            if (contents.Count < 2)
                return;
            contents.Insert(contents.Count - 2, key);

        }

        public void ReplaceKey(string key, string replace)
        {
            for (int i = 0; i < contents.Count; i++)
            {
                if (contents[i].IndexOf(key) != -1)
                {
                    contents[i] = contents[i].Replace(key, replace);
                }
            }
        }

        public void Save()
        {
            StreamWriter saveFile = File.CreateText(plistPath);
            foreach (string line in contents)
                saveFile.WriteLine(line);
            saveFile.Close();
        }

        public void Dispose()
        {

        }
    }
}