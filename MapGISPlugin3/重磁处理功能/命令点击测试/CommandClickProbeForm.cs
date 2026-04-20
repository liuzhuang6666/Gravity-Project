using System.Drawing;
using System.Windows.Forms;
using MapGIS.PluginEngine;

namespace MapGISPlugin3
{
    public class CommandClickProbeForm : Form
    {
        private readonly IApplication _hook;

        public CommandClickProbeForm(IApplication hook)
        {
            _hook = hook;
            InitializeUi();
        }

        private void InitializeUi()
        {
            Text = "命令点击测试窗口";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(420, 180);

            Label infoLabel = new Label();
            infoLabel.AutoSize = false;
            infoLabel.Location = new Point(20, 20);
            infoLabel.Size = new Size(380, 90);
            infoLabel.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(134)));
            infoLabel.Text =
                "这个窗口说明按钮已经成功进入命令。\r\n\r\n" +
                "IApplication: " + (_hook == null ? "null" : "ok") + "\r\n" +
                "Document: " + ((_hook != null && _hook.Document != null) ? "ok" : "null");

            Button closeButton = new Button();
            closeButton.Text = "关闭";
            closeButton.Size = new Size(90, 32);
            closeButton.Location = new Point(310, 125);
            closeButton.Click += delegate { Close(); };

            Controls.Add(infoLabel);
            Controls.Add(closeButton);
        }
    }
}
