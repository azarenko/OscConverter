using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OscConverter.Output;
using System.IO;

namespace OscConverter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            saveFileDialog.Filter = "Postolov|*.mwf";
        }

        List<Channel> channels = null;

        private void open_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                openFileName.Text = openFileDialog.FileName;

                Input.InputInterface inputInstance = null;

                try
                {
                    inputInstance = Input.InputFactory.GetInstance(openFileDialog.FileName);
                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                channels = inputInstance.GetChannels();

                inputFileInfo.Text = string.Empty;

                for (int i = 0; i < channels.Count; i++)
                {
                    inputFileInfo.Text += "Channel " + (i + 1) + ":\r\n";
                    inputFileInfo.Text += channels[i].ToString();
                    inputFileInfo.Text += "\r\n";
                }

                saveFileName.Text = Path.GetDirectoryName(openFileName.Text) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(openFileName.Text) + GetOutputFileExtension();
            }            
        }

        private void save_Click(object sender, EventArgs e)
        {
            if (channels == null)
            {
                MessageBox.Show("You must change input file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            saveFileDialog.FileName = saveFileName.Text;

            if (saveFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            saveFileName.Text = saveFileDialog.FileName;             
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            outPutFormatList.SelectedIndex = 0;
        }

        private void outPutFormatList_SelectedIndexChanged(object sender, EventArgs e)
        {
            saveFileDialog.Filter = GetOutputFileFilter();
        }

        private string GetOutputFileFilter()
        {
            switch (outPutFormatList.SelectedIndex)
            {
                case 0: return "Postolov|*.mwf";
                case 1: return "MTPro|*.mt";
                default: throw new Exception("Unsupported output file type");
            }
        }

        private string GetOutputFileExtension()
        {
            switch (outPutFormatList.SelectedIndex)
            {
                case 0: return ".mwf";
                case 1: return "*.mt";
                default: throw new Exception("Unsupported output file type");
            }
        }

        private void convert_Click(object sender, EventArgs e)
        {
            outInterface outInstance = outFactory.GetInstance(saveFileName.Text);
            outInstance.SetChannels(channels);

            MessageBox.Show("File converted: " + saveFileName.Text, "Converting", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}