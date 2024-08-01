using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FTP_Program.Services
{
    public class FtpClient
    {
        private string host;
        private string user;
        private string pass;

        // 생성자: 호스트, 사용자 이름, 비밀번호를 초기화합니다.
        public FtpClient(string host, string user, string pass)
        {
            this.host = host;
            this.user = user;
            this.pass = pass;
        }

        // FTP 서버에 연결하는 메서드
        public bool Connect()
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(host);
                request.Credentials = new NetworkCredential(user, pass);
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == FtpStatusCode.OpeningData;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"연결 실패: {ex.Message}");
                return false;
            }
        }

        // 파일을 업로드하는 메서드
        public void UploadFile(string filePath)
        {
            try
            {
                string uri = $"{host}/{Path.GetFileName(filePath)}";
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
                request.Credentials = new NetworkCredential(user, pass);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                byte[] fileContents = File.ReadAllBytes(filePath);
                request.ContentLength = fileContents.Length;

                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(fileContents, 0, fileContents.Length);
                }

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Console.WriteLine($"업로드 완료: {response.StatusDescription}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"업로드 실패: {ex.Message}");
            }
        }

        // 파일을 다운로드하는 메서드
        public void DownloadFile(string remoteFileName, string localPath)
        {
            try
            {
                string uri = $"{host}/{remoteFileName}";
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
                request.Credentials = new NetworkCredential(user, pass);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (Stream responseStream = response.GetResponseStream())
                using (FileStream fs = new FileStream(localPath, FileMode.Create))
                {
                    responseStream.CopyTo(fs);
                }

                Console.WriteLine("다운로드 완료");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"다운로드 실패: {ex.Message}");
            }
        }
    }
}
