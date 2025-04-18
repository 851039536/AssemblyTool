using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MewTool.AssemblyVersionCategory
{
    /// <summary>
    /// 语义化版本工具类
    /// </summary>
    public class MSemanticVersionHelper : IComparable<MSemanticVersionHelper>, IEquatable<MSemanticVersionHelper>
    {
        private static readonly Regex VersionRegex = new Regex(
            @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)" +
            @"(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?" +
            @"(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$",
            RegexOptions.Compiled);

        /// <summary>
        /// 主版本号
        /// </summary>
        public int Major { get; }
        /// <summary>
        /// 次开发版本号
        /// </summary>
        public int Minor { get; }
        /// <summary>
        /// 补丁版本号
        /// </summary>
        public int Patch { get; }
        /// <summary>
        /// 预发布标识符
        /// </summary>
        public string PreRelease { get; }
        /// <summary>
        /// 构建元数据
        /// </summary>
        public string BuildMetadata { get; }

        /// <summary>
        /// 初始化语义化版本
        /// </summary>
        public MSemanticVersionHelper(int major = 1, int minor = 0, int patch = 0, string preRelease = null, string buildMetadata = null)
        {
            if (major < 0 || minor < 0 || patch < 0)
                throw new ArgumentException("版本号不能为负数");

            if (preRelease != null && !IsValidIdentifier(preRelease, true))
                throw new ArgumentException("无效的预发布标识符");

            if (buildMetadata != null && !IsValidIdentifier(buildMetadata, false))
                throw new ArgumentException("无效的构建元数据");

            Major = major;
            Minor = minor;
            Patch = patch;
            PreRelease = preRelease;
            BuildMetadata = buildMetadata;
        }

        /// <summary>
        /// 从字符串解析语义化版本
        /// </summary>
        public static MSemanticVersionHelper Parse(string versionString)
        {
            if (string.IsNullOrWhiteSpace(versionString))
                throw new ArgumentNullException(nameof(versionString));

            var match = VersionRegex.Match(versionString);
            if (!match.Success)
                throw new FormatException("无效的版本字符串格式");

            int major = int.Parse(match.Groups["major"].Value);
            int minor = int.Parse(match.Groups["minor"].Value);
            int patch = int.Parse(match.Groups["patch"].Value);
            string preRelease = match.Groups["prerelease"].Success ? match.Groups["prerelease"].Value : null;
            string buildMetadata = match.Groups["buildmetadata"].Success ? match.Groups["buildmetadata"].Value : null;

            return new MSemanticVersionHelper(major, minor, patch, preRelease, buildMetadata);
        }

        /// <summary>
        /// 尝试从字符串解析语义化版本
        /// </summary>
        public static bool TryParse(string versionString, out MSemanticVersionHelper versionHelper)
        {
            try
            {
                versionHelper = Parse(versionString);
                return true;
            }
            catch
            {
                versionHelper = null;
                return false;
            }
        }

        /// <summary>
        /// 生成版本字符串
        /// </summary>
        public override string ToString()
        {
            var builder = new StringBuilder($"{Major}.{Minor}.{Patch}");

            if (!string.IsNullOrEmpty(PreRelease))
                builder.Append($"-{PreRelease}");

            if (!string.IsNullOrEmpty(BuildMetadata))
                builder.Append($"+{BuildMetadata}");

            return builder.ToString();
        }

        /// <summary>
        /// 比较两个版本
        /// </summary>
        public int CompareTo(MSemanticVersionHelper other)
        {
            if (other is null) return 1;

            int majorCompare = Major.CompareTo(other.Major);
            if (majorCompare != 0) return majorCompare;

            int minorCompare = Minor.CompareTo(other.Minor);
            if (minorCompare != 0) return minorCompare;

            int patchCompare = Patch.CompareTo(other.Patch);
            if (patchCompare != 0) return patchCompare;

            // 预发布版本的优先级低于正式版本
            if (string.IsNullOrEmpty(PreRelease) && !string.IsNullOrEmpty(other.PreRelease))
                return 1;
            if (!string.IsNullOrEmpty(PreRelease) && string.IsNullOrEmpty(other.PreRelease))
                return -1;

            // 比较预发布标识符
            if (!string.IsNullOrEmpty(PreRelease) && !string.IsNullOrEmpty(other.PreRelease))
            {
                string[] thisIdentifiers = PreRelease.Split('.');
                string[] otherIdentifiers = other.PreRelease.Split('.');

                for (int i = 0; i < Math.Min(thisIdentifiers.Length, otherIdentifiers.Length); i++)
                {
                    bool thisIsNumeric = int.TryParse(thisIdentifiers[i], out int thisNum);
                    bool otherIsNumeric = int.TryParse(otherIdentifiers[i], out int otherNum);

                    if (thisIsNumeric && otherIsNumeric)
                    {
                        int numCompare = thisNum.CompareTo(otherNum);
                        if (numCompare != 0) return numCompare;
                    }
                    else if (thisIsNumeric)
                    {
                        return -1; // 数字标识符优先级低于非数字
                    }
                    else if (otherIsNumeric)
                    {
                        return 1; // 非数字标识符优先级高于数字
                    }
                    else
                    {
                        int strCompare = string.Compare(thisIdentifiers[i], otherIdentifiers[i], StringComparison.Ordinal);
                        if (strCompare != 0) return strCompare;
                    }
                }

                // 如果前面的标识符都相同，长度更长的优先级更高
                return thisIdentifiers.Length.CompareTo(otherIdentifiers.Length);
            }

            return 0;
        }

        /// <summary>
        /// 比较两个版本
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as MSemanticVersionHelper);
        }

        public bool Equals(MSemanticVersionHelper other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// 获取哈希码
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + Major.GetHashCode();
                hash = (hash * 23) + Minor.GetHashCode();
                hash = (hash * 23) + Patch.GetHashCode();
                hash = (hash * 23) + (PreRelease?.GetHashCode() ?? 0);
                return hash;
            }
        }

        /// <summary>
        /// 重载运算符
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool operator ==(MSemanticVersionHelper left, MSemanticVersionHelper right) => Equals(left, right);
        public static bool operator !=(MSemanticVersionHelper left, MSemanticVersionHelper right) => !Equals(left, right);
        public static bool operator <(MSemanticVersionHelper left, MSemanticVersionHelper right) => left.CompareTo(right) < 0;
        public static bool operator >(MSemanticVersionHelper left, MSemanticVersionHelper right) => left.CompareTo(right) > 0;
        public static bool operator <=(MSemanticVersionHelper left, MSemanticVersionHelper right) => left.CompareTo(right) <= 0;
        public static bool operator >=(MSemanticVersionHelper left, MSemanticVersionHelper right) => left.CompareTo(right) >= 0;

        /// <summary>
        /// 递增主版本号 (重置次版本号和修订号)
        /// </summary>
        public MSemanticVersionHelper IncrementMajor()
        {
            return new MSemanticVersionHelper(Major + 1,0,0);
        }

        /// <summary>
        /// 递增次版本号 (重置修订号)
        /// </summary>
        public MSemanticVersionHelper IncrementMinor()
        {
            return new MSemanticVersionHelper(Major,Minor + 1,0);
        }

        /// <summary>
        /// 递增修订号
        /// </summary>
        public MSemanticVersionHelper IncrementPatch()
        {
            return new MSemanticVersionHelper(Major,Minor,Patch + 1);
        }

        /// <summary>
        /// 设置预发布版本标识
        /// </summary>
        public MSemanticVersionHelper WithPreRelease(string preRelease) => 
            new MSemanticVersionHelper(Major, Minor, Patch, preRelease, BuildMetadata);

        /// <summary>
        /// 设置构建元数据
        /// </summary>
        public MSemanticVersionHelper WithBuildMetadata(string buildMetadata) => 
            new MSemanticVersionHelper(Major, Minor, Patch, PreRelease, buildMetadata);

        /// <summary>
        /// 验证标识符是否符合SemVer规范
        /// </summary>
        private static bool IsValidIdentifier(string identifier, bool isPreRelease)
        {
            if (string.IsNullOrEmpty(identifier))
                return false;

            string[] identifiers = identifier.Split('.');
            foreach (string id in identifiers)
            {
                if (string.IsNullOrEmpty(id))
                    return false;

                // 构建元数据允许数字开头
                if (!isPreRelease && char.IsDigit(id[0]))
                    continue;

                // 预发布标识符不能只包含数字且以0开头
                if (isPreRelease && id.All(char.IsDigit) && id.Length > 1 && id[0] == '0')
                    return false;

                foreach (char c in id)
                {
                    if (!char.IsLetterOrDigit(c) && c != '-')
                        return false;
                }
            }

            return true;
        }
    }
}
