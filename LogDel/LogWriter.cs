﻿using LogDel.Utilities.Extensions;
using System;
using System.IO;

namespace LogDel
{
    public static class LogWriter
    {
        public static string LogDirectory { get; set; } = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\.plexdl\logs";

        public static void LogDelWriter(string fileName, string[] headers, string[] logEntry)
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                    Directory.CreateDirectory(LogDirectory);

                var logdelLine = "";
                var fqFile = $"{LogDirectory}\\{fileName}";

                //log parsing will fail if the headers don't match the entry
                //it's the same as having 4 cells to a row; but only 3 columns...
                if (headers.Length != logEntry.Length) return;

                //remove forbidden characters from log entry like '!' and '#'
                logEntry = logEntry.CleanLogDel();

                foreach (var l in logEntry)
                    logdelLine += l + "!";

                //remove trailing '!'
                logdelLine = logdelLine.TrimEnd('!');

                //remove forbidden characters from log entry like '!' and '#'
                headers = headers.CleanLogDel();

                var headersString = "###";
                foreach (var h in headers)
                    headersString += h.CleanLogDel() + @"!";

                //remove trailing '!'
                headersString = headersString.TrimEnd('!');

                //does the log file already exist?
                if (File.Exists(fqFile))
                {
                    //it does, so we need to read the existing log entries into memory
                    var existing = File.ReadAllText(fqFile);

                    //check if the existing content is empty (maybe the file is blank?)
                    if (string.IsNullOrEmpty(existing))
                    {
                        //it is, so just rewrite the entire thing (including headers) without concatenating the original content (var existing)
                        var contentToWrite = headersString + "\n" + logdelLine;
                        File.WriteAllText(fqFile, contentToWrite);
                    }
                    else
                    {
                        //the file isn't empty, so we need to concatenate the original content with the new content, then rewrite
                        //the log file
                        var contentToWrite = existing + "\n" + logdelLine;
                        File.WriteAllText(fqFile, contentToWrite);
                    }
                }
                else
                {
                    //file doesn't exist, so create it and then write headers and the new entry
                    var contentToWrite = headersString + "\n" + logdelLine;
                    File.WriteAllText(fqFile, contentToWrite);
                }
            }
            catch (Exception)
            {
                //for debugging only!
                //MessageBox.Show(ex);
                //ignore the error
            }
        }
    }
}