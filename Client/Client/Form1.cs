using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Client
{
    public partial class Form1 : Form
    {
        Socket clientSocket;
        NetworkStream stream;

        public Form1()
        {
            InitializeComponent();
        }

        
        private void btnBrowse_Click(
            object sender,
            EventArgs e)
        {
            OpenFileDialog dialog =
                new OpenFileDialog();

            if (dialog.ShowDialog() ==
                DialogResult.OK)
            {
                txtPath.Text =
                    dialog.FileName;
            }
        }


        private async void btnConnect_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                clientSocket =
                    new Socket(
                        AddressFamily.InterNetwork,
                        SocketType.Stream,
                        ProtocolType.Tcp);

                await clientSocket.ConnectAsync(
                    new IPEndPoint(
                        IPAddress.Parse("127.0.0.1"),
                        9050));

                stream =
                    new NetworkStream(clientSocket);

                AddMessage(
                    "Connected To Server");
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
            }
        }

        
        private async void btnSend_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                string selectedPath =
                    txtPath.Text;

                byte[] fileBytes =
                    File.ReadAllBytes(selectedPath);

               
                byte[] sizeInfo =
                    BitConverter.GetBytes(
                        (long)fileBytes.Length);

                await stream.WriteAsync(
                    sizeInfo,
                    0,
                    sizeInfo.Length);


                await stream.WriteAsync(
                    fileBytes,
                    0,
                    fileBytes.Length);

                AddMessage("File Uploaded");

                

                byte[] compressedInfo =
                    new byte[8];

                await stream.ReadAsync(
                    compressedInfo,
                    0,
                    compressedInfo.Length);

                long compressedLength =
                    BitConverter.ToInt64(
                        compressedInfo,
                        0);


                byte[] compressedData =
                    new byte[compressedLength];

                int downloaded = 0;

                while (downloaded < compressedLength)
                {
                    int currentPart =
                        await stream.ReadAsync(
                            compressedData,
                            downloaded,
                            (int)(compressedLength - downloaded));

                    downloaded += currentPart;

                    progressBar1.Value =
                        (int)((downloaded * 100)
                        / compressedLength);
                }

               

                string compressedPath =
                    selectedPath + ".gz";

                File.WriteAllBytes(
                    compressedPath,
                    compressedData);

                AddMessage(
                    "Compressed File Saved");

                AddMessage(
                    compressedPath);

                MessageBox.Show(
                    "Compression Completed");
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
            }
        }


        void AddMessage(string text)
        {
            txtLogs.AppendText(
                text + Environment.NewLine);
        }
    }
}