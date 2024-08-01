using FTP_Program.Services;
using FTP_Program.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace FTP_Program.Views.Pages
{
    public partial class DashboardPage : INavigableView<DashboardViewModel>
    {
        private FtpServer ftpServer;

        public void MainWindow()
        {
            InitializeComponent();
        }

        // FTP 서버와 연결 버튼 클릭 이벤트
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string host = HostTextBox.Text;
            string user = IdTextBox.Text;
            string pass = PasswordTextBox.Text;

            FtpClient ftpClient = new FtpClient(host, user, pass);
            bool isConnected = ftpClient.Connect();

            if (isConnected)
            {
                MessageBox.Show("연결 성공", "FTP 연결", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("연결 실패", "FTP 연결", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // FTP 서버 시작 버튼 클릭 이벤트
        private void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            string ipAddress = "127.0.0.1"; // 로컬 IP 주소 또는 필요한 IP 주소를 입력
            int port = 21; // 사용할 포트 번호
            string rootDirectory = @"C:\FTPServerRoot"; // 서버의 루트 디렉터리 경로

            ftpServer = new FtpServer(ipAddress, port, rootDirectory);
            ftpServer.Start();

            MessageBox.Show("FTP 서버가 시작되었습니다.", "서버 상태", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // FTP 서버 중지 버튼 클릭 이벤트
        private void StopServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (ftpServer != null)
            {
                ftpServer.Stop();
                MessageBox.Show("FTP 서버가 중지되었습니다.", "서버 상태", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
