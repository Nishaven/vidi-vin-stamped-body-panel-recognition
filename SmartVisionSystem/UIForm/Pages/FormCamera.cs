// ============================================================
// 文件：FormCamera.cs
// 模块：UIForm（界面层）
// 说明：相机监视主页面（UIPage）。为车型识别相机与各 VIN 码相机创建画面控件
//       （ImageViDiDisplay + ViDi SampleViewer），按 3 行 × 3 列表格布局装入并设置标题。
// ============================================================
using Sunny.UI;
using System.Windows.Forms;
using SmartVisionSystem.UIForm.Canvas;
using System.Drawing;
using ViDi2.UI;

namespace SmartVisionSystem.UIForm.Pages
{
    /// <summary>
    /// 相机监视页面：创建车型识别相机与 8 个 VIN 码相机的画面控件（ImageViDiDisplay），
    /// 装入 3×3 表格布局并设置各画面标题，供实时监视推理画面与手动触发拍照。
    /// </summary>
    public partial class FormCamera : UIPage
    {
        // 车型识别相机画面控件（显示车型识别相机的 ViDi 推理画面）
        public ImageViDiDisplay CarIdentificationCameraDisplay;
        // VIN 码相机1 画面控件（对应 A09/T58/A39 车型读码）
        public ImageViDiDisplay VINCodeCamera1CameraDisplay;
        // VIN 码相机2 画面控件（对应 T68/A66/T09-M 车型读码）
        public ImageViDiDisplay VINCodeCamera2CameraDisplay;
        // VIN 码相机3 画面控件（对应 T68-G/A66右舵/CS145 车型读码）
        public ImageViDiDisplay VINCodeCamera3CameraDisplay;
        // VIN 码相机4 画面控件（对应 T58-G 车型读码）
        public ImageViDiDisplay VINCodeCamera4CameraDisplay;
        // VIN 码相机5 画面控件（对应 V22 车型读码）
        public ImageViDiDisplay VINCodeCamera5CameraDisplay;



        //2026.9 V22-G-s
        public ImageViDiDisplay VINCodeCamera6CameraDisplay;
        public ImageViDiDisplay VINCodeCamera7CameraDisplay;
        public ImageViDiDisplay VINCodeCamera8CameraDisplay;
        //2026.9 V22-G-e

        // 主窗体引用，画面控件通过它访问主窗体内的相机对象
        private MainForm mMain;
        /// <summary>设计器所需的无参构造函数（未创建画面控件）。</summary>
        public FormCamera() { }
        /// <summary>
        /// 构造函数：为车型识别相机和各 VIN 码相机分别创建画面控件，
        /// 每个控件绑定一个独立的 ViDi SampleViewer（以 Name 区分相机），随后装入表格布局并设置标题。
        /// </summary>
        /// <param name="main">主窗体引用</param>
        public FormCamera(MainForm main)
        {
            // 创建车型识别相机画面控件（Dock 填满单元格，Margin 控制画面间距）
            CarIdentificationCameraDisplay = new ImageViDiDisplay(new SampleViewer { Name = "CarIdentificationCameraSampleViewer", AllowDrop = false }, main)
            {
                Name = "CarIdentificationCameraDisplay",
                AllowDrop = false,
                Margin = new Padding(2, 2, 1, 1),
                Dock = DockStyle.Fill
            };
            // 依次创建各 VIN 码相机画面控件（各自绑定独立的 SampleViewer）
            VINCodeCamera1CameraDisplay = new ImageViDiDisplay(new SampleViewer {Name= "VINCodeCamera1SampleViewer", AllowDrop = false }, main)
            {
                Name = "VINCodeCamera1CameraDisplay",
                AllowDrop = false,
                Margin = new Padding(1, 2, 1, 1),
                Dock = DockStyle.Fill
            };
            VINCodeCamera2CameraDisplay = new ImageViDiDisplay(new SampleViewer { Name = "VINCodeCamera2SampleViewer", AllowDrop = false }, main)
            {
                Name = "VINCodeCamera2CameraDisplay",
                AllowDrop = false,
                Margin = new Padding(1, 2, 2, 1),
                Dock = DockStyle.Fill
            };
            VINCodeCamera3CameraDisplay = new ImageViDiDisplay(new SampleViewer { Name = "VINCodeCamera3SampleViewer", AllowDrop = false }, main)
            {
                Name = "VINCodeCamera3CameraDisplay",
                AllowDrop = false,
                Margin = new Padding(2, 1, 1, 2),
                Dock = DockStyle.Fill
            };
            VINCodeCamera4CameraDisplay = new ImageViDiDisplay(new SampleViewer { Name = "VINCodeCamera4SampleViewer", AllowDrop = false }, main)
            {
                Name = "VINCodeCamera4CameraDisplay",
                AllowDrop = false,
                Margin = new Padding(1, 1, 1, 2),
                Dock = DockStyle.Fill
            };
            VINCodeCamera5CameraDisplay = new ImageViDiDisplay(new SampleViewer { Name = "VINCodeCamera5SampleViewer", AllowDrop = false }, main)
            {
                Name = "VINCodeCamera5CameraDisplay",
                AllowDrop = false,
                Margin = new Padding(1, 1, 2, 2),
                Dock = DockStyle.Fill
            };

            //2026.9 V22-G-s
            VINCodeCamera6CameraDisplay = new ImageViDiDisplay(new SampleViewer { Name = "VINCodeCamera6SampleViewer", AllowDrop = false }, main)
            {
                Name = "VINCodeCamera6CameraDisplay",
                AllowDrop = false,
                Margin = new Padding(1, 1, 1, 1),
                Dock = DockStyle.Fill
            };
            VINCodeCamera7CameraDisplay = new ImageViDiDisplay(new SampleViewer { Name = "VINCodeCamera7SampleViewer", AllowDrop = false }, main)
            {
                Name = "VINCodeCamera7CameraDisplay",
                AllowDrop = false,
                Margin = new Padding(1, 1, 1, 1),
                Dock = DockStyle.Fill
            };
            VINCodeCamera8CameraDisplay = new ImageViDiDisplay(new SampleViewer { Name = "VINCodeCamera8SampleViewer", AllowDrop = false }, main)
            {
                Name = "VINCodeCamera8CameraDisplay",
                AllowDrop = false,
                Margin = new Padding(1, 1, 1, 1),
                Dock = DockStyle.Fill
            };
            //2026.9 V22-G-e

            mMain = main;
            InitializeComponent();
            // 车型识别相机在监视页不提供手动触发，隐藏其画面上的运行按钮
            CarIdentificationCameraDisplay.BtnRun.Visible = false;
            // 设置各画面顶部的相机名称标题（车型识别相机 + VIN 码相机1~5）
            CarIdentificationCameraDisplay.LabelCameraName.Text = "车型识别相机";
            VINCodeCamera1CameraDisplay.LabelCameraName.Text = "VIN码相机1-A09/T58/A39";
            VINCodeCamera2CameraDisplay.LabelCameraName.Text = "VIN码相机2-T68/A66/T09-M";
            VINCodeCamera3CameraDisplay.LabelCameraName.Text = "VIN码相机3-T68-G/A66右舵";
            VINCodeCamera4CameraDisplay.LabelCameraName.Text = "VIN码相机4-T58-G";
            VINCodeCamera5CameraDisplay.LabelCameraName.Text = "VIN码相机5-V22";
            // 把各画面控件按（列,行）加入 3×3 表格布局：第 0~2 行 × 第 0~2 列
            uiTableLayoutPanel1.Controls.Add(CarIdentificationCameraDisplay, 0, 0);
            uiTableLayoutPanel1.Controls.Add(VINCodeCamera1CameraDisplay, 1, 0);
            uiTableLayoutPanel1.Controls.Add(VINCodeCamera2CameraDisplay, 2, 0);
            uiTableLayoutPanel1.Controls.Add(VINCodeCamera3CameraDisplay, 0, 1);
            uiTableLayoutPanel1.Controls.Add(VINCodeCamera4CameraDisplay, 1, 1);
            uiTableLayoutPanel1.Controls.Add(VINCodeCamera5CameraDisplay, 2, 1);

            //2026.9 V22-G-s
            VINCodeCamera6CameraDisplay.LabelCameraName.Text = "VIN码相机6-V22-G";
            VINCodeCamera7CameraDisplay.LabelCameraName.Text = "备用";
            VINCodeCamera8CameraDisplay.LabelCameraName.Text = "备用";
            // V22-G 新增的三个画面装入第三行（相机6 为 V22-G 读码，相机7/8 为备用画面）
            uiTableLayoutPanel1.Controls.Add(VINCodeCamera6CameraDisplay, 0, 2);
            uiTableLayoutPanel1.Controls.Add(VINCodeCamera7CameraDisplay, 1, 2);
            uiTableLayoutPanel1.Controls.Add(VINCodeCamera8CameraDisplay, 2, 2);
            //2026.9 V22-G-e
        }

        /// <summary>
        /// 表格布局重绘事件：空实现，无自定义绘制。
        /// </summary>
        private void uiTableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
