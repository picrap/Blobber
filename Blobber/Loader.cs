﻿#region Blobber!
// Blobber - Merges or embed referenced assemblies
// https://github.com/picrap/Blobber
// MIT License - http://opensource.org/licenses/MIT
#endregion

namespace Blobber
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Reflection;

    public static class Loader
    {
        internal static void Initialize()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return GetEmbeddeddAssembly(assembly, args.Name) ?? GetMergedAssembly(assembly, args.Name);
        }

        /// <summary>
        /// Gets the embeddedd assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private static Assembly GetEmbeddeddAssembly(Assembly assembly, string name)
        {
            var resourceStream = assembly.GetManifestResourceStream(GetEmbeddedAssemblyResourceName(name));
            if (resourceStream == null)
                return null;

            using (var assemblyStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(resourceStream, CompressionMode.Decompress))
                    gzipStream.CopyTo(assemblyStream);
                return Assembly.Load(assemblyStream.ToArray());
            }
        }

        internal static string GetEmbeddedAssemblyResourceName(string name) => "blobber:embedded.gz:" + name;

        private static Assembly GetMergedAssembly(Assembly assembly, string name)
        {
            if (assembly.GetManifestResourceInfo(GetMergedAssemblyResourceName(name)) != null)
                return assembly;
            return null;
        }

        internal static string GetMergedAssemblyResourceName(string name) => "blobber:merged:" + name;
    }
}