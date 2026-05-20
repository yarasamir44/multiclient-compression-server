using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CompressionServer
{
    internal class Program
    {
        static Socket serverSocket;

        static async Task Main(string[] args)
        {
            serverSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            IPEndPoint address =new IPEndPoint(IPAddress.Any, 9050);

            serverSocket.Bind(address);

            serverSocket.Listen(15);

            Console.WriteLine("Compression Server Running...");

            while (true)
            {
                Socket currentClient =
                    await serverSocket.AcceptAsync();

                Console.WriteLine("Client Connected");

                _ = Task.Run(() =>
                    ProcessClient(currentClient));
            }
        }

        static async Task ProcessClient(Socket currentClient)
        {
            try
            {
                NetworkStream stream =new NetworkStream(currentClient);

                

                byte[] lengthBytes =new byte[8];

                await stream.ReadAsync(
                    lengthBytes,
                    0,
                    lengthBytes.Length);

                long fileLength =
                    BitConverter.ToInt64(
                        lengthBytes,
                        0);

                Console.WriteLine($"Receiving {fileLength} bytes");

               

                byte[] originalFile =
                    new byte[fileLength];

                int received = 0;

                while (received < fileLength)
                {
                    int chunk =
                        await stream.ReadAsync(
                            originalFile,
                            received,
                            (int)(fileLength - received));

                    received += chunk;
                }

                Console.WriteLine("File Received");

                

                byte[] compressedFile;

                using (MemoryStream memory =
                    new MemoryStream())
                {
                    using (GZipStream compressor =
                        new GZipStream(
                            memory,
                            CompressionMode.Compress))
                    {
                        await compressor.WriteAsync(
                            originalFile,
                            0,
                            originalFile.Length);
                    }

                    compressedFile =memory.ToArray();
                }

                Console.WriteLine("Compression Finished");

                

                byte[] compressedLength =BitConverter.GetBytes((long)compressedFile.Length);

                await stream.WriteAsync(compressedLength,0,compressedLength.Length);

                
                await stream.WriteAsync(compressedFile,0,compressedFile.Length);

                Console.WriteLine("Compressed File Sent");

                stream.Close();
                currentClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}