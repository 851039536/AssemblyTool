using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MewTool.AssemblyVersionCategory
{
    /// <summary>
    /// 程序集版本管理服务
    /// </summary>
    public static class MAsmbSeanVersionHelper
    {
        /// <summary>
        /// 从 AssemblyInfo.cs 文件中读取当前版本信息
        /// </summary>
        public static MSemanticVersionHelper GetCurrentVersion(string assemblyInfoPath)
        {
            if(!File.Exists(assemblyInfoPath))
                throw new FileNotFoundException("找不到 AssemblyInfo.cs 文件",assemblyInfoPath);

            string content = File.ReadAllText(assemblyInfoPath);
            return ParseVersionFromAssemblyInfo(content);
        }

        /// <summary>
        /// 从 AssemblyInfo.cs 文件中解析版本信息
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static MSemanticVersionHelper ParseVersionFromAssemblyInfo(string content)
        {
            // 优先使用 AssemblyInformationalVersion
            string versionString = ExtractAttributeValue(content,"AssemblyInformationalVersion");

            // 如果没有则使用 AssemblyFileVersion
            if(string.IsNullOrEmpty(versionString))
                versionString = ExtractAttributeValue(content,"AssemblyFileVersion");

            // 最后使用 AssemblyVersion
            if(string.IsNullOrEmpty(versionString))
                versionString = ExtractAttributeValue(content,"AssemblyVersion");

            if(string.IsNullOrEmpty(versionString))
                throw new InvalidOperationException("无法从 AssemblyInfo.cs 中解析版本信息");

            return MSemanticVersionHelper.Parse(versionString);
        }

        private static string ExtractAttributeValue(string content,string attributeName)
        {
            string pattern = $@"\[assembly: {attributeName}\(""(.*?)""\)\]";
            var match = Regex.Match(content,pattern);
            return match.Success ? match.Groups[1].Value : null;
        }

        /// <summary>
        /// 更新 AssemblyInfo.cs 文件中的版本信息
        /// </summary>
        public static bool UpdateVersion(
            string assemblyInfoPath,
            MSemanticVersionHelper newVersionHelper,
            bool updateAssemblyVersion = true,
            bool updateFileVersion = true,
            bool updateInformationalVersion = true)
        {
            var versionInfo = new AssemblyVersionInfo
            {
                AssemblyVersion =
                    updateAssemblyVersion ? new Version(newVersionHelper.Major,newVersionHelper.Minor,newVersionHelper.Patch,0) : null,
                FileVersion = updateFileVersion ? $"{newVersionHelper.Major}.{newVersionHelper.Minor}.{newVersionHelper.Patch}.0" : null,
                InformationalVersion = updateInformationalVersion ? newVersionHelper.ToString() : null
            };

            return MAssemblyVersionHelper.UpdateAssemblyInfoFile(assemblyInfoPath,versionInfo);
        }

        /// <summary>
        /// 递增主版本号并更新 AssemblyInfo.cs
        /// </summary>
        public static bool IncrementMajorVersion(string assemblyInfoPath,string preRelease = null)
        {
            var currentVersion = GetCurrentVersion(assemblyInfoPath);
            var newVersion = currentVersion.IncrementMajor();

            if(!string.IsNullOrEmpty(preRelease))
                newVersion = newVersion.WithPreRelease(preRelease);

            return UpdateVersion(assemblyInfoPath,newVersion);
        }

        /// <summary>
        /// 递增次版本号并更新 AssemblyInfo.cs
        /// </summary>
        public static bool IncrementMinorVersion(string assemblyInfoPath,string preRelease = null)
        {
            var currentVersion = GetCurrentVersion(assemblyInfoPath);
            var newVersion = currentVersion.IncrementMinor();

            if(!string.IsNullOrEmpty(preRelease))
                newVersion = newVersion.WithPreRelease(preRelease);

            return UpdateVersion(assemblyInfoPath,newVersion);
        }

        /// <summary>
        /// 递增修订号并更新 AssemblyInfo.cs
        /// </summary>
        public static bool IncrementPatchVersion(string assemblyInfoPath,string preRelease = null)
        {
            var currentVersion = GetCurrentVersion(assemblyInfoPath);
            var newVersion = currentVersion.IncrementPatch();

            if(!string.IsNullOrEmpty(preRelease))
                newVersion = newVersion.WithPreRelease(preRelease);

            return UpdateVersion(assemblyInfoPath,newVersion);
        }

        /// <summary>
        /// 设置预发布版本标识
        /// </summary>
        public static bool SetPreRelease(string assemblyInfoPath,string preRelease)
        {
            var currentVersion = GetCurrentVersion(assemblyInfoPath);
            var newVersion = currentVersion.WithPreRelease(preRelease);
            return UpdateVersion(assemblyInfoPath,newVersion);
        }

        /// <summary>
        /// 移除预发布版本标识
        /// </summary>
        public static bool RemovePreRelease(string assemblyInfoPath)
        {
            var currentVersion = GetCurrentVersion(assemblyInfoPath);
            var newVersion = new MSemanticVersionHelper(
                currentVersion.Major,
                currentVersion.Minor,
                currentVersion.Patch,
                null,
                currentVersion.BuildMetadata);

            return UpdateVersion(assemblyInfoPath,newVersion);
        }


        /// <summary>
        /// 获取assembly路径
        /// </summary>
        /// <returns></returns>
        public static string GetAssemblyPath()
        {
            // 获取当前程序集所在的目录
            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // 获取上一级目录
            string parentDir = Directory.GetParent(currentDir)?.FullName;
            string parentDir2 = Directory.GetParent(parentDir)?.FullName;
            string assemblyInfoPath = Path.Combine(parentDir2,"Properties","AssemblyInfo.cs");
            return assemblyInfoPath;
        }
    }
}