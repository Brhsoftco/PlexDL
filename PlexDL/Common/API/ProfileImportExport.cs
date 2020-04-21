﻿using PlexDL.Common.Logging;
using PlexDL.Common.Structures.AppOptions;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace PlexDL.Common.API
{
    public static class ProfileImportExport
    {
        public static ApplicationOptions ProfileFromFile(string fileName, bool silent = false)
        {
            try
            {
                ApplicationOptions subReq = null;

                var serializer = new XmlSerializer(typeof(ApplicationOptions));

                var reader = new StreamReader(fileName);
                subReq = (ApplicationOptions)serializer.Deserialize(reader);
                reader.Close();

                return subReq;
            }
            catch (Exception ex)
            {
                LoggingHelpers.RecordException(ex.Message, "LoadProfileError");
                if (!silent)
                    MessageBox.Show(ex.ToString(), "Error in loading XML Profile", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                return null;
            }
        }

        public static void ProfileToFile(string fileName, ApplicationOptions options, bool silent = false)
        {
            try
            {
                var xsSubmit = new XmlSerializer(typeof(ApplicationOptions));
                using (var sww = new StringWriter())
                {
                    xsSubmit.Serialize(sww, options);

                    //delete the existing file if there is one; the user was asked if they wanted to replace it.
                    if (File.Exists(fileName))
                        File.Delete(fileName);

                    File.WriteAllText(fileName, sww.ToString());
                }
            }
            catch (Exception ex)
            {
                LoggingHelpers.RecordException(ex.Message, "@SaveProfileError");
                if (!silent)
                    MessageBox.Show(ex.ToString(), @"Error in saving XML Profile", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
            }
        }
    }
}