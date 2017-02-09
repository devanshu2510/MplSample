using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using Google.Apis.Download;
using Google.Apis.Auth.OAuth2;
using System.IO;
using System.Threading;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace GappFilePerms
{
    public partial class Form1 : Form
    {
        string directoryId;
        Google.Apis.Drive.v3.Data.File newFile;
        DriveService service;
        MplGoogleDriveHelper mplG = new MplGoogleDriveHelper();
        string m_p_directory_name;
        string m_c_directory_name;
        string m_downloadPath;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Connect with Oauth2 Ask user for permission //
            String CLIENT_ID = "451300536148-bjdonne98m7vdgevshpj8luq185mp10h.apps.googleusercontent.com";
            String CLIENT_SECRET = "VmlazFPQERUMSpRu02vuyMNM";
            service = Authentication.AuthenticateOauth(CLIENT_ID, CLIENT_SECRET, Environment.UserName);
           // service = Authentication.AuthenticateOauth(CLIENT_ID, CLIENT_SECRET, "devanshu2510");
            
        }
        

        //uploade one file in to folder 
        private void button1_Click(object sender, EventArgs e)
        {
            mplG.uploadFileInFolder(service,m_c_directory_name);
            MessageBox.Show("Successfully Uploaded");
        }
        
        //Download All File from Folder 
        private void button3_Click(object sender, EventArgs e)
        {
            mplG.downloadList(service);
            MessageBox.Show(MplGoogleDriveHelper.m_downloadCounter+ " File Downloaded Successfully");
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
             mplG.createDirectory(service, m_c_directory_name, m_p_directory_name);
             MessageBox.Show("Directory Created");
        }
        
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
            m_p_directory_name = textBox1.Text;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            m_c_directory_name = textBox5.Text;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            m_c_directory_name = textBox4.Text;
        }
    }
}