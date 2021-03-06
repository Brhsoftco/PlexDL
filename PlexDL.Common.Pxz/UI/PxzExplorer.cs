﻿using PlexDL.Common.Pxz.Structures.File;
using PlexDL.WaitWindow;
using System;
using System.Data;
using System.IO;
using System.Windows.Forms;
using UIHelpers;

// ReSharper disable InvertIf

namespace PlexDL.Common.Pxz.UI
{
    public partial class PxzExplorer : Form
    {
        public PxzFile Pxz { get; set; }

        public PxzExplorer()
        {
            InitializeComponent();
        }

        public DataTable Records()
        {
            var table = new DataTable();
            table.Columns.Add(@"Record Name", typeof(string));
            table.Columns.Add(@"Stored Name", typeof(string));
            table.Columns.Add(@"Type", typeof(string));
            table.Columns.Add(@"Protected", typeof(string));
            table.Columns.Add(@"Sizing", typeof(string));
            table.Columns.Add(@"C. Ratio", typeof(string));
            table.Columns.Add(@"CS Valid", typeof(string));

            try
            {
                foreach (var r in Pxz.Records)
                {
                    var recordName = r.Header.Naming.RecordName;
                    var storedName = r.Header.Naming.StoredName;
                    var recordType = r.Header.Naming.DataType.ToString();
                    var recordProt = r.ProtectedRecord.ToString();
                    var recordSize = $"{Utilities.FormatBytes((long)r.Header.Size.RawSize)}/{Utilities.FormatBytes((long)r.Header.Size.DecSize)}";
                    var comprRatio = $"{r.Header.Size.Ratio}%";
                    var checkValid = r.ChecksumValid.ToString();

                    object[] row =
                    {
                        recordName,
                        storedName,
                        recordType,
                        recordProt,
                        recordSize,
                        comprRatio,
                        checkValid
                    };

                    table.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                UIMessages.Error(ex.ToString());
            }

            return table;
        }

        public DataTable Attributes()
        {
            var table = new DataTable();
            table.Columns.Add(@"Attribute", typeof(string));
            table.Columns.Add(@"Value", typeof(string));

            try
            {
                object[] authorName = { @"User", Pxz.FileIndex.Author.UserAccount };
                object[] authorDisplay = { @"Display Name", Pxz.FileIndex.Author.DisplayName };
                object[] authorMachine = { @"PC Name", Pxz.FileIndex.Author.MachineName };
                object[] formatVersion = { @"Version", Pxz.FileIndex.FormatVersion.ToString() };
                object[] buildState = { @"Release State", Pxz.FileIndex.BuildState.ToString() };
                object[] recordCount = { @"# Records", Pxz.FileIndex.RecordReference.Count.ToString() };

                table.Rows.Add(authorName);
                table.Rows.Add(authorDisplay);
                table.Rows.Add(authorMachine);
                table.Rows.Add(formatVersion);
                table.Rows.Add(buildState);
                table.Rows.Add(recordCount);
            }
            catch (Exception ex)
            {
                UIMessages.Error(ex.ToString());
            }

            return table;
        }

        private static void LoadPxz(object sender, WaitWindowEventArgs e)
        {
            if (e.Arguments.Count == 1)
                e.Result = LoadPxz((string)e.Arguments[0], false);
        }

        private static PxzFile LoadPxz(string fileName, bool waitWindow = true)
        {
            //multi-threaded handler
            if (waitWindow)
                return (PxzFile)WaitWindow.WaitWindow.Show(LoadPxz, @"Loading PXZ file", fileName);

            try
            {
                //ensure the file exists
                if (File.Exists(fileName))
                {
                    //attempt PXZ load
                    var pxz = new PxzFile();
                    pxz.Load(fileName);

                    //return the loaded PXZ file
                    return pxz;
                }
            }
            catch (Exception ex)
            {
                UIMessages.Error(ex.ToString(), @"Load PXZ File Error");
            }

            //default
            return null;
        }

        private void DoPxzLoad(string fileName)
        {
            try
            {
                //ensure the file exists
                if (File.Exists(fileName))
                {
                    //attempt PXZ load
                    var pxz = LoadPxz(fileName);

                    //set the global
                    Pxz = pxz;

                    //perform UI load
                    DoPxzLoad();
                }
                else
                    //alert the user
                    UIMessages.Error(@"Couldn't find the specified PXZ file", @"Validation Error");
            }
            catch (Exception ex)
            {
                UIMessages.Error(ex.ToString(), @"Load PXZ File Error");
            }
        }

        private void DoPxzLoad()
        {
            try
            {
                if (Pxz != null)
                {
                    dgvAttributes.DataSource = Attributes();
                    dgvRecords.DataSource = Records();
                }
            }
            catch (Exception ex)
            {
                UIMessages.Error(ex.ToString(), @"Load Error");
            }
        }

        private void PxzInformation_Load(object sender, EventArgs e)
            => DoPxzLoad();

        private void BtnOK_Click(object sender, EventArgs e)
            => Close();

        public static void ShowPxzExplorer(PxzFile file)
            => new PxzExplorer { Pxz = file }.ShowDialog();

        public static void ShowPxzExplorer()
            => new PxzExplorer().ShowDialog();

        private void ItmExtractRecord_Click(object sender, EventArgs e)
        {
            if (dgvRecords.SelectedRows.Count == 1)
            {
                var recordName = dgvRecords.SelectedRows[0].Cells[0].Value.ToString();
                var record = Pxz.LoadRecord(recordName);

                if (sfdExtract.ShowDialog() == DialogResult.OK)
                {
                    if (record.ExtractRecord(sfdExtract.FileName))
                        UIMessages.Info($"Successfully extracted '{recordName}'");
                    else
                        UIMessages.Error(@"Extraction failed; an unknown error occurred.");
                }
            }
        }

        private void CxtExtract_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //there needs to be a row selected to allow extraction
            if (dgvRecords.SelectedRows.Count == 0)
                e.Cancel = true;
        }

        private void ItmOpen_Click(object sender, EventArgs e)
        {
            if (ofdOpenPxzFile.ShowDialog() == DialogResult.OK)
                DoPxzLoad(ofdOpenPxzFile.FileName);
        }
    }
}