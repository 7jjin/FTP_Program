using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FTP_Program.Services
{
    public class FtpServer
    {
        private TcpListener listener;
        private string rootDirectory;
        private bool isRunning;

        // 생성자: IP 주소, 포트, 루트 디렉토리를 초기화합니다.
        public FtpServer(string ipAddress, int port, string rootDirectory)
        {
            this.listener = new TcpListener(IPAddress.Parse(ipAddress), port);
            this.rootDirectory = rootDirectory;
        }

        // 서버 시작 메서드
        public void Start()
        {
            isRunning = true;
            listener.Start();
            Console.WriteLine("FTP 서버가 시작되었습니다.");

            while (isRunning)
            {
                TcpClient client = listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(HandleClient, client);
            }
        }

        // 서버 중지 메서드
        public void Stop()
        {
            isRunning = false;
            listener.Stop();
        }

        // 클라이언트 요청 처리 메서드
        private void HandleClient(object clientObject)
        {
            TcpClient client = (TcpClient)clientObject;
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream, Encoding.ASCII);
            StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

            writer.WriteLine("220 FTP 서버에 오신 것을 환영합니다.");

            bool authenticated = false;

            while (true)
            {
                string request = reader.ReadLine();
                if (request == null) break;

                string[] tokens = request.Split(' ');
                string command = tokens[0].ToUpper();
                string argument = tokens.Length > 1 ? tokens[1] : null;

                Console.WriteLine($"명령어 수신: {command} {argument}");

                if (!authenticated && command != "USER" && command != "PASS")
                {
                    writer.WriteLine("530 로그인 필요.");
                    continue;
                }

                switch (command)
                {
                    case "USER":
                        writer.WriteLine("331 사용자 이름 확인, 비밀번호 필요.");
                        break;

                    case "PASS":
                        authenticated = true;
                        writer.WriteLine("230 사용자 로그인 완료.");
                        break;

                    case "QUIT":
                        writer.WriteLine("221 안녕히 가세요.");
                        client.Close();
                        return;

                    case "PWD":
                        writer.WriteLine($"257 \"{rootDirectory}\"");
                        break;

                    case "LIST":
                        writer.WriteLine("150 파일 목록 전송 준비 완료.");
                        string[] files = Directory.GetFiles(rootDirectory);
                        foreach (string file in files)
                        {
                            FileInfo fileInfo = new FileInfo(file);
                            writer.WriteLine(fileInfo.Name);
                        }
                        writer.WriteLine("226 전송 완료.");
                        break;

                    case "RETR":
                        string filePath = Path.Combine(rootDirectory, argument);
                        if (File.Exists(filePath))
                        {
                            writer.WriteLine("150 파일 전송 시작.");
                            byte[] fileBytes = File.ReadAllBytes(filePath);
                            stream.Write(fileBytes, 0, fileBytes.Length);
                            writer.WriteLine("226 파일 전송 완료.");
                        }
                        else
                        {
                            writer.WriteLine("550 파일을 찾을 수 없습니다.");
                        }
                        break;

                    case "STOR":
                        string uploadPath = Path.Combine(rootDirectory, argument);
                        writer.WriteLine("150 파일 수신 준비 완료.");
                        byte[] buffer = new byte[1024];
                        int bytesRead;
                        using (FileStream fs = new FileStream(uploadPath, FileMode.Create, FileAccess.Write))
                        {
                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                fs.Write(buffer, 0, bytesRead);
                            }
                        }
                        writer.WriteLine("226 파일 수신 완료.");
                        break;

                    default:
                        writer.WriteLine("502 명령어를 지원하지 않습니다.");
                        break;
                }
            }

            client.Close();
        }
    }
}
