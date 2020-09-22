﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace PlexDL.Internal
{
    /// <summary>
    /// Handler for finding missing assemblies and reloading them on failure
    /// </summary>
    public static class AssemblyResolve
    {
        //THESE ARE THE FORMAL LOCATIONS OF PLEXDL FILES.
        //THEY ARE CREATED/REMOVED/UPDATED VIA POST-BUILD EVENTS.
        //DO NOT MODIFY UNLESS YOU ARE MODIFYING THE POST-BUILD EVENTS SCRIPT.
        public static string LibraryDirectory { get; } = @"lib";

        public static string DebugDirectory { get; } = @"pdb";
        public static string XmlDirectory { get; } = @"xml"; //NOTE: These are generated by the compiler; not by PlexDL.

        public static string PlexDlLocation { get; } = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty);

        /// <summary>
        /// Main locator and resolver
        /// <br></br>CREDIT: https://weblog.west-wind.com/posts/2016/dec/12/loading-net-assemblies-out-of-seperate-folders
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Assembly HandleResolve(object sender, ResolveEventArgs args)
        {
            // Ignore missing resources and autogenerated XmlSerializers binary
            if (args.Name.Contains(".resources") || args.Name.Contains(@".XmlSerializers"))
                return null;

            // check for assemblies already loaded
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
                return assembly;

            // We can't handle missing libraries if there's a missing PlexDL path

            if (!string.IsNullOrEmpty(PlexDlLocation))
            {
                // Try to load by filename - split out the filename of the full assembly name
                // and append the base path of the original assembly (ie. look in the same dir)
                var filename = args.Name.Split(',')[0] + ".dll".ToLower();

                // Did the user specify a custom search directory? We need to try and fetch it.
                var customDir = DifferentAssemblyPath();

                // Top-most directory to search for libraries
                var searchDir = string.IsNullOrEmpty(customDir) ? PlexDlLocation : customDir;

                try
                {
                    var toLoad = Directory.GetFiles(searchDir, filename, SearchOption.AllDirectories)
                        .Select(Assembly.LoadFrom).FirstOrDefault();
                    if (toLoad != null)
                        return toLoad;
                }
                catch (Exception)
                {
                    //nothing
                }
            }

            // The assembly should have been resolved above, if it wasn't then
            // error out and inform the user.
            MessageBox.Show($"Referencing error:\n\n'{args.Name}' could not be found; the application failed to start.", @"Critical Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            // Kill current PlexDL process
            Process.GetCurrentProcess().Kill();

            // Default
            return null;
        }

        /// <summary>
        /// If the user wants to locate assemblies elsewhere, the below routine is executed when the correct argument is passed.
        /// </summary>
        /// <returns></returns>
        public static string DifferentAssemblyPath()
        {
            try
            {
                const string checkFor = @"-libDir=";
                var sep = checkFor[checkFor.Length - 1];

                foreach (var s in Program.Args)
                {
                    if (!s.Contains(checkFor)) continue;

                    var split = s.Split(sep);
                    if (split.Length != 2) continue;

                    var path = split[1];
                    if (Directory.Exists(path))
                        return path;
                }

                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}