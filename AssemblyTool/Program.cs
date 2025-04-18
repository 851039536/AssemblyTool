using System.Reflection;
using MewTool.AssemblyVersionCategory;

// 获取当前程序集所在的目录
string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
Console.WriteLine("当前程序集所在的目录: " + currentDir);
// 获取上一级目录
string parentDir = Directory.GetParent(currentDir)?.FullName;
Console.WriteLine("上一级目录: " + parentDir);
string parentDir2 = Directory.GetParent(parentDir)?.FullName;
Console.WriteLine("上一级目录: " + parentDir2);
string parentDir3 = Directory.GetParent(parentDir2)?.FullName;
string assemblyInfoPath = Path.Combine(parentDir3, "Properties", "AssemblyInfo.cs");
Console.WriteLine("AssemblyInfo.cs路径: " + assemblyInfoPath); 
switch (args[0])
{
    case "setMajor": //发布正式版本
        MAsmbSeanVersionHelper.RemovePreRelease(assemblyInfoPath);
        break;
    case "setPre_alpha": // 设置预发布版本号 alpha 早期开发版本，功能不完整
        MAsmbSeanVersionHelper.SetPreRelease(assemblyInfoPath, "alpha");
        break;
    case "setPre_beta": //设置预发布版本号  beta - 测试版本，功能基本完整但可能有bug
        MAsmbSeanVersionHelper.SetPreRelease(assemblyInfoPath, "beta");
        break;
    case "setPre_rc": // (Release Candidate) - 发布候选版本，接近最终版
        MAsmbSeanVersionHelper.SetPreRelease(assemblyInfoPath, "rc");
        break;
    case "major+": // 递增主版本号
        MAsmbSeanVersionHelper.IncrementMajorVersion(assemblyInfoPath);
        break;
    case "minor+": // 递增次版本号
        var current1 = MAsmbSeanVersionHelper.GetCurrentVersion(assemblyInfoPath);
        MAsmbSeanVersionHelper.IncrementMinorVersion(assemblyInfoPath, current1.PreRelease);
        break;
    case "patch+": // 递增修订号
        var current2 = MAsmbSeanVersionHelper.GetCurrentVersion(assemblyInfoPath);
        // 递增时传递当前预发布标签
        MAsmbSeanVersionHelper.IncrementPatchVersion(assemblyInfoPath, current2.PreRelease);
        break;
}

// Thread.Sleep(3000);