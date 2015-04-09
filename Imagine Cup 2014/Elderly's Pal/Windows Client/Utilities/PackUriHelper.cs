﻿using System;
using System.Globalization;
using System.Reflection;

namespace NiceDreamers.Windows.Utilities
{
    /// <summary>
    ///     Helper class that generates a pack Uri, given a resource path
    /// </summary>
    public static class PackUriHelper
    {
        /// <summary>
        ///     Generates a pack Uri for the given resource path. Assumes the resource is located in the
        ///     same assembly as this type (PackUriHelper).
        /// </summary>
        /// <param name="resourcePath">String representing the relative path to the resource</param>
        /// <returns>Pack Uri pointing to the resource</returns>
        public static Uri CreatePackUri(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                throw new ArgumentNullException("resourcePath");
            }

            return CreatePackUri(Assembly.GetAssembly(typeof (PackUriHelper)), resourcePath);
        }

        /// <summary>
        ///     Generates a pack Uri for the given resource path within the given assembly
        /// </summary>
        /// <param name="assembly">Assembly within which the resource is located</param>
        /// <param name="resourcePath">String representing the relative path to the resource</param>
        /// <returns>Pack Uri pointing to the resource</returns>
        public static Uri CreatePackUri(Assembly assembly, string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                throw new ArgumentNullException("resourcePath");
            }

            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            return
                new Uri(string.Format(CultureInfo.InvariantCulture, "pack://application:,,,/{0};component/{1}",
                    assembly.GetName().Name, resourcePath));
        }
    }
}