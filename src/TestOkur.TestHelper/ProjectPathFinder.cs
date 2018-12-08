﻿namespace TestOkur.TestHelper
{
    using System;
    using System.IO;
    using System.Reflection;

    internal static class ProjectPathFinder
    {
        public static string GetPath(string projectRelativePath, Type startupType)
        {
            var assembly = GetAssembly(startupType);
            var projectName = assembly.GetName().Name;
            var applicationBasePath = GetAssemblyDirectory(assembly);
            var directoryInfo = new DirectoryInfo(applicationBasePath);
            string foundPath = null;

            do
            {
                directoryInfo = directoryInfo.Parent;
                foundPath = GetProjectFilePath(projectName, directoryInfo, projectRelativePath);
            }
            while (directoryInfo?.Parent != null && foundPath != null);

            return foundPath ??
                   throw new Exception($"Project root could not be located using the application root {applicationBasePath}.");
        }

        private static string GetProjectFilePath(string projectName, DirectoryInfo directoryInfo, string projectRelativePath)
        {
            if (directoryInfo == null)
            {
                return null;
            }

            var projectDirectoryInfo = new DirectoryInfo(Path.Combine(directoryInfo.FullName, projectRelativePath));
            if (projectDirectoryInfo.Exists)
            {
                var projectFileInfo = new FileInfo(Path.Combine(projectDirectoryInfo.FullName, projectName, $"{projectName}.csproj"));
                if (projectFileInfo.Exists)
                {
                    return Path.Combine(projectDirectoryInfo.FullName, projectName);
                }
            }

            return null;
        }

        private static string GetAssemblyDirectory(Assembly assembly)
        {
            var codeBase = assembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);

            return Path.GetDirectoryName(path);
        }

        private static Assembly GetAssembly(Type startupType)
        {
            while (startupType.GetTypeInfo().BaseType != typeof(object))
            {
                startupType = startupType.BaseType;
            }

            return startupType.GetTypeInfo().Assembly;
        }
    }
}
