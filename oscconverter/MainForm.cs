using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OscConverter.Output;

namespace OscConverter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        List<Channel> channels = null;

        private void open_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                openFileName.Text = openFileDialog.FileName;

                Input.InputInterface inputInstance = Input.InputFactory.GetInstance(openFileDialog.FileName);

                channels = inputInstance.GetChannels();

                inputFileInfo.Text = string.Empty;

                for (int i = 0; i < channels.Count; i++)
                {
                    inputFileInfo.Text += "Channel " + (i + 1) + ":\r\n";
                    inputFileInfo.Text += channels[i].ToString();
                    inputFileInfo.Text += "\r\n";
                }
            }            
        }

        private void save_Click(object sender, EventArgs e)
        {
            if (channels == null)
            {
                MessageBox.Show("You must change input file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (saveFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            saveFileName.Text = saveFileDialog.FileName;

            outInterface outInstance = outFactory.GetInstance(saveFileDialog.FileName);
            outInstance.SetChannels(channels);

            MessageBox.Show("File converted: " + saveFileDialog.FileName, "Converting", MessageBoxButtons.OK, MessageBoxIcon.Information);                
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            outPutFormatList.SelectedIndex = 0;
        }

        private void outPutFormatList_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(outPutFormatList.SelectedIndex)
            {
                case 0: saveFileDialog.Filter = "Postolov|*.mwf"; break;
                case 1: saveFileDialog.Filter = "MTPro|*.mt"; break;
                default: throw new Exception("");
            }                        
        }
    }
}