using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace MewTool.AssemblyVersionCategory
{
    /// <summary>
    /// 程序集版本信息读取和修改工具类
    /// </summary>
    public static class MAssemblyVersionHelper
    {
       /// <summary>
        /// 获取当前执行程序集的版本信息（返回键值对集合）
        /// </summary>
        /// <returns>包含所有版本信息的键值对集合</returns>
        public static Dictionary<string, string> GetCurrentAssemblyVersionInfo()
        {
            return GetAssemblyVersionInfo(Assembly.GetExecutingAssembly());
        }
        /// <summary>
        /// 获取指定程序集的版本信息（返回键值对集合）
        /// </summary>
        /// <param name="assembly">要检查的程序集</param>
        /// <returns>包含所有版本信息的键值对集合</returns>
        public static Dictionary<string, string> GetAssemblyVersionInfo(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));
            var versionInfo = new Dictionary<string, string>();
            // 获取 AssemblyVersion
            var assemblyVersion = assembly.GetName().Version;
            versionInfo.Add("AssemblyVersion", assemblyVersion?.ToString() ?? "N/A");
            // 获取 AssemblyFileVersion
            var assemblyFileVersionAttribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            versionInfo.Add("FileVersion", assemblyFileVersionAttribute?.Version ?? "N/A");
            // 获取 AssemblyInformationalVersion
            var assemblyInformationalVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            versionInfo.Add("InformationalVersion", assemblyInformationalVersionAttribute?.InformationalVersion ?? "N/A");
            return versionInfo;
        }
        /// <summary>
        /// 获取当前执行程序集的版本信息（返回元组数组）
        /// </summary>
        /// <returns>包含所有版本信息的元组数组</returns>
        public static (string Name, string Value)[] GetCurrentAssemblyVersionInfoAsArray()
        {
            return GetAssemblyVersionInfoAsArray(Assembly.GetExecutingAssembly());
        }
        /// <summary>
        /// 获取指定程序集的版本信息（返回元组数组）
        /// </summary>
        /// <param name="assembly">要检查的程序集</param>
        /// <returns>包含所有版本信息的元组数组</returns>
        public static (string Name, string Value)[] GetAssemblyVersionInfoAsArray(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));
            var versionInfo = new List<(string, string)>();
            // 获取 AssemblyVersion
            var assemblyVersion = assembly.GetName().Version;
            versionInfo.Add(("AssemblyVersion", assemblyVersion?.ToString() ?? "N/A"));
            // 获取 AssemblyFileVersion
            var assemblyFileVersionAttribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            versionInfo.Add(("FileVersion", assemblyFileVersionAttribute?.Version ?? "N/A"));
            // 获取 AssemblyInformationalVersion
            var assemblyInformationalVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            versionInfo.Add(("InformationalVersion", assemblyInformationalVersionAttribute?.InformationalVersion ?? "N/A"));
            return versionInfo.ToArray();
        }

        /// <summary>
        /// 修改 AssemblyInfo.cs 文件中的版本信息
        /// </summary>
        /// <param name="assemblyInfoPath">AssemblyInfo.cs 文件路径</param>
        /// <param name="newVersion">新的版本信息</param>
        /// <returns>是否修改成功</returns>
        public static bool UpdateAssemblyInfoFile(string assemblyInfoPath, AssemblyVersionInfo newVersion)
        {
            if (string.IsNullOrWhiteSpace(assemblyInfoPath))
                throw new ArgumentException(@"AssemblyInfo.cs 文件路径不能为空", nameof(assemblyInfoPath));

            if (!File.Exists(assemblyInfoPath))
                throw new FileNotFoundException("找不到指定的 AssemblyInfo.cs 文件", assemblyInfoPath);

            if (newVersion == null)
                throw new ArgumentNullException(nameof(newVersion));

            try
            {
                string content = File.ReadAllText(assemblyInfoPath);
                bool changed = false;

                // 更新 AssemblyVersion
                if (newVersion.AssemblyVersion != null)
                {
                    content = UpdateAttribute(content, "AssemblyVersion", newVersion.AssemblyVersion.ToString(), ref changed);
                }

                // 更新 AssemblyFileVersion
                if (!string.IsNullOrWhiteSpace(newVersion.FileVersion))
                {
                    content = UpdateAttribute(content, "AssemblyFileVersion", newVersion.FileVersion, ref changed);
                }

                // 更新 AssemblyInformationalVersion
                if (!string.IsNullOrWhiteSpace(newVersion.InformationalVersion))
                {
                    content = UpdateAttribute(content, "AssemblyInformationalVersion", newVersion.InformationalVersion, ref changed);
                }

                if (changed)
                {
                    // 创建备份文件
                    string backupPath = assemblyInfoPath + ".bak";
                    File.Copy(assemblyInfoPath, backupPath, true);

                    // 写入新内容
                    File.WriteAllText(assemblyInfoPath, content, Encoding.UTF8);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"修改 AssemblyInfo.cs 失败: {ex.Message}");
                throw;
            }
        }

        private static string UpdateAttribute(string content, string attributeName, string newValue, ref bool changed)
        {
            string pattern = $@"\[assembly: {attributeName}\(""(.*?)""\)\]";
            var match = Regex.Match(content, pattern);

            if (match.Success)
            {
                string oldValue = match.Groups[1].Value;
                if (oldValue != newValue)
                {
                    changed = true;
                    return Regex.Replace(content, pattern, $@"[assembly: {attributeName}(""{newValue}"")]");
                }
            }

            return content;
        }
    }

    /// <summary>
    /// 程序集版本信息封装类
    /// </summary>
    public class AssemblyVersionInfo
    {
        /// <summary>
        /// AssemblyVersion 属性值
        /// </summary>
        public Version AssemblyVersion { get; set; }

        /// <summary>
        /// AssemblyFileVersion 属性值
        /// </summary>
        public string FileVersion { get; set; }

        /// <summary>
        /// AssemblyInformationalVersion 属性值
        /// </summary>
        public string InformationalVersion { get; set; }

        /// <summary>
        /// 格式化输出所有版本信息
        /// </summary>
        public override string ToString()
        {
            return $"AssemblyVersion: {AssemblyVersion}\n" +
                   $"FileVersion: {FileVersion}\n" +
                   $"InformationalVersion: {InformationalVersion}";
        }
    }
}
