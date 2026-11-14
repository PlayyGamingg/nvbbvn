using System.Diagnostics;
using System.Windows.Forms;

ApplicationConfiguration.Initialize();
Application.Run(new MainForm());

class MainForm : Form
{
    Label status = new() { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(8) };

    public MainForm()
    {
        Text = "Chiếu điện thoại lên PC (Miracast)";
        Width = 460; Height = 380;
        StartPosition = FormStartPosition.CenterScreen;
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(16) };
        panel.Controls.Add(Btn("1. Cài Wireless Display (chỉ lần đầu)", InstallFeature));
        panel.Controls.Add(Btn("2. Mở trình nhận Miracast", OpenReceiver));
        panel.Controls.Add(Btn("3. Cài đặt chiếu của Windows", () => Run("ms-settings:project")));
        panel.Controls.Add(Btn("4. Chiếu bằng scrcpy (cáp USB / Wi-Fi)", OpenScrcpy));
        panel.Controls.Add(Btn("Hướng dẫn trên điện thoại", Guide));
        Controls.Add(panel);
        Controls.Add(status);
        status.Text = "Sẵn sàng. Làm theo bước 1 → 2, rồi chọn Smart View / Cast trên điện thoại.";
    }

    Button Btn(string text, Action a)
    {
        var b = new Button { Text = text, Width = 400, Height = 44 };
        b.Click += (_, _) => { try { a(); } catch (Exception ex) { status.Text = "Lỗi: " + ex.Message; } };
        return b;
    }

    static void Run(string file, string args = "", bool admin = false, bool hidden = false) =>
        Process.Start(new ProcessStartInfo(file, args)
        {
            UseShellExecute = true,
            Verb = admin ? "runas" : "",
            WindowStyle = hidden ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
        });

    void InstallFeature()
    {
        Run("powershell.exe",
            "-NoProfile -Command \"Add-WindowsCapability -Online -Name App.WirelessDisplay.Connect~~~~0.0.1.0; pause\"",
            admin: true);
        status.Text = "Đang cài Wireless Display. Xong hãy khởi động lại máy nếu được yêu cầu.";
    }

    void OpenReceiver()
    {
        // Windows 10 / 11: ứng dụng Connect / Wireless Display
        try { Run("explorer.exe", "shell:AppsFolder\\Microsoft.PPIProjection_cw5n1h2txyewy!Microsoft.PPIProjection"); }
        catch { Run("ms-settings:project"); }
        status.Text = "Đã mở trình nhận. Trên điện thoại: Smart View / Cast → chọn tên PC này.";
    }

    void OpenScrcpy()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "scrcpy", "scrcpy.exe");
        if (!File.Exists(path))
        {
            status.Text = "Chưa có scrcpy. Tải tại github.com/Genymobile/scrcpy, giải nén vào thư mục 'scrcpy' cạnh file exe.";
            return;
        }
        Run(path);
        status.Text = "Đang chạy scrcpy (cần bật USB debugging trên điện thoại).";
    }

    void Guide() => MessageBox.Show(
        "1. PC và điện thoại cùng bật Wi-Fi.\n2. Mở trình nhận (bước 2).\n3. Điện thoại: Cài đặt → Smart View / Chiếu màn hình / Cast.\n4. Chọn tên máy tính, nhập PIN nếu có.\n\nNếu không thấy máy: card Wi-Fi cần hỗ trợ Wi-Fi Direct; hãy thử scrcpy.",
        "Hướng dẫn");
}
