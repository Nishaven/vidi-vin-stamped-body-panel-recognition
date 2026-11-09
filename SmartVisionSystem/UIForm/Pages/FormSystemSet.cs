// ============================================================
// 文件：FormSystemSet.cs
// 模块：UIForm（界面层）
// 说明：系统设置页面（UIPage）。分页编辑 PLC 通讯地址、各相机曝光/增益、存图开关等
//       系统级配置，保存时先把旧 JSON 备份到“备份”目录，再序列化写回新配置。
// ============================================================
using Sunny.UI;
using System;
using System.IO;
using System.Text;
using SmartVisionSystem.Log4Net;
using SmartVisionSystem.VisionCore;


namespace SmartVisionSystem.UIForm.Pages
{
    /// <summary>
    /// 系统设置页面：集中显示并保存 PLC 通讯地址、相机曝光/增益、各相机 OK/NG 存图开关等系统级参数；
    /// 页面内勾选变化即时回写内存配置，保存按钮统一备份并持久化到 JSON 配置文件。
    /// </summary>
    public partial class FormSystemSet : UIPage
    {
        // 全局系统配置对象（相机参数列表、PLC 地址、存图开关等），由主窗体构造时传入
        SystemConfigurations mCameraParameters;
        // 主窗体引用，用于读取当前登录用户的权限等级
        private MainForm mainForm;
        // 相机配置文件的绝对路径：..\Configurations\CameraConfigurations.json
        private readonly string CameraParameterConfigurationPath = Path.GetFullPath("..") + "\\Configurations\\CameraConfigurations.json";
        /// <summary>
        /// 构造函数：接收全局配置与主窗体引用，初始化控件后把配置文件中的
        /// 存图开关、曝光/增益、PLC 地址等参数回显到页面各控件上。
        /// </summary>
        /// <param name="systemConfigurations">全局系统配置对象</param>
        /// <param name="main">主窗体引用</param>
        public FormSystemSet(SystemConfigurations systemConfigurations, MainForm main)
        {
            mCameraParameters = systemConfigurations;
            mainForm = main;
            InitializeComponent();
            // 回显全局开关（SPA、自动存图）到对应复选框
            CheckBoxISPAOn2.Checked = mCameraParameters.ISPAOn2;
             CheckBoxISAutoSaveImage.Checked= mCameraParameters.ISAutoSaveImage;
            // 按相机名从配置列表取出各相机的 OK/NG 图存档开关并回显（车型识别相机 + 各 VIN 码相机）
            CheckBoxCam1IsSaveOKImage.Checked = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "CarIdentificationCamera").IsSaveOKImage;
            CheckBoxCam1IsSaveNGImage.Checked = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "CarIdentificationCamera").IsSaveNGImage;
            CheckBoxCam2IsSaveOKImage.Checked = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera1").IsSaveOKImage;
            CheckBoxCam2IsSaveNGImage.Checked = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera1").IsSaveNGImage;
            CheckBoxCam3IsSaveOKImage.Checked = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera2").IsSaveOKImage;
            CheckBoxCam3IsSaveNGImage.Checked = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera2").IsSaveNGImage;
            CheckBoxCam4IsSaveOKImage.Checked = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera3").IsSaveOKImage;
            CheckBoxCam4IsSaveNGImage.Checked = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera3").IsSaveNGImage;
            CheckBoxCam5IsSaveOKImage.Checked = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera4").IsSaveOKImage;
            CheckBoxCam5IsSaveNGImage.Checked = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera4").IsSaveNGImage;
            CheckBoxCam6IsSaveOKImage.Checked = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera5").IsSaveOKImage;
            CheckBoxCam6IsSaveNGImage.Checked = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera5").IsSaveNGImage;

            //2026.9 V22-G-s
            CheckBoxCam7IsSaveOKImage.Checked = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera6").IsSaveOKImage;
            CheckBoxCam7IsSaveNGImage.Checked = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera6").IsSaveNGImage;
            CheckBoxCam8IsSaveOKImage.Checked = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera7").IsSaveOKImage;
            CheckBoxCam8IsSaveNGImage.Checked = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera7").IsSaveNGImage;
            CheckBoxCam9IsSaveOKImage.Checked = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera8").IsSaveOKImage;
            CheckBoxCam9IsSaveNGImage.Checked = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera8").IsSaveNGImage;
            //2026.9 V22-G-e

            // 回显各相机曝光/增益到对应文本框（车型识别相机 + VIN 码相机1~10）
            uiTextBox3.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "CarIdentificationCamera").CameraExposure.ToString();
            uiTextBox4.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "CarIdentificationCamera").CameraGain.ToString();
            uiTextBoxGS8.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera1").CameraExposure.ToString();
            uiTextBoxGS8_.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera1").CameraGain.ToString();
            uiTextBoxM8.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera2").CameraExposure.ToString();
            uiTextBoxM8_.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera2").CameraGain.ToString();
            uiTextBoxM6.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera3").CameraExposure.ToString();
            uiTextBoxM6_.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera3").CameraGain.ToString();
            uiTextBoxAM8.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera4").CameraExposure.ToString();
            uiTextBoxAM8_.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera4").CameraGain.ToString();
            uiTextBoxA09.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera5").CameraExposure.ToString();
            uiTextBoxA09_.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera5").CameraGain.ToString();
            uiTextBox9.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera6").CameraExposure.ToString();
            uiTextBox9_.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera6").CameraGain.ToString();
            uiTextBox8.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera99").CameraExposure.ToString();
            uiTextBox8_.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera99").CameraGain.ToString();
            uiTextBox6.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera8").CameraExposure.ToString();
            uiTextBox6_.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera8").CameraGain.ToString();
            uiTextBox7.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera9").CameraExposure.ToString();
            uiTextBox7_.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera9").CameraGain.ToString();
            uiTextBox5.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera10").CameraExposure.ToString();
            uiTextBox5_.Text = mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera10").CameraGain.ToString();

            //2026.9 V22-G-s
            //调用配置文件第11号曝光\增益
            V22_G_CameraExposure.Text= mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera7").CameraExposure.ToString();
            V22_G_CameraGain.Text= mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera7").CameraGain.ToString();
            //2026.9 V22-G-e

            // 回显 PLC 通讯 IP/端口及各数据交互点位地址（在线状态、车型识别、VIN 码读写、触发与完成信号等）
            TextBoxPLCIPAddress.Text = mCameraParameters.PLCCommunicationIP;
            TextBoxPLCPort.Text = string.Format("{0}", mCameraParameters.PLCCommunicationPort);
            TextBoxSystemOnlineAddress.Text = mCameraParameters.SystemOnlineAddress;
            TextBoxCarModelCameraFXAddress.Text = mCameraParameters.CarModelCameraFXAddress;
            TextBoxCarModelCameraOKAddress.Text = mCameraParameters.CarModelCameraOKAddress;
            TextBoxCarModelCameraNGAddress.Text = mCameraParameters.CarModelCameraNGAddress;
            TextBoxCarModelCameraCarModelAddress.Text = mCameraParameters.CarModelCameraCarModelAddress;
            TextBoxVINCodeCameraOKAddress.Text = mCameraParameters.VINCodeCameraOKAddress;
            TextBoxVINCodeCameraNGAddress.Text = mCameraParameters.VINCodeCameraNGAddress;
            TextBoxVINCodeCameraVINCodeAddress.Text = mCameraParameters.VINCodeCameraVINCodeAddress;
            TextBoxAutoModel.Text = mCameraParameters.AutoModelAddress;
            //TextBoxAutoModel2.Text = mCameraParameters.AutoModelAddress2;
            //uiTextBox2.Text = mCameraParameters.PLCStopAddress;
            TextBoxCameraTrigger.Text = mCameraParameters.CameraTriggerAddress;
            TextBoxJobRunDone.Text = mCameraParameters.JobRunDoneAddress;
            // 为所有存图开关复选框挂接统一的 CheckedChanged 处理器（勾选变化即时按权限回写配置）
            CheckBoxCam1IsSaveOKImage.CheckedChanged += CheckBox_CheckedChanged;
            CheckBoxCam1IsSaveNGImage.CheckedChanged += CheckBox_CheckedChanged;
            CheckBoxCam2IsSaveOKImage.CheckedChanged += CheckBox_CheckedChanged;
            CheckBoxCam2IsSaveNGImage.CheckedChanged += CheckBox_CheckedChanged;
            CheckBoxCam3IsSaveOKImage.CheckedChanged += CheckBox_CheckedChanged;
            CheckBoxCam3IsSaveNGImage.CheckedChanged += CheckBox_CheckedChanged;
            CheckBoxCam4IsSaveOKImage.CheckedChanged += CheckBox_CheckedChanged;
            CheckBoxCam4IsSaveNGImage.CheckedChanged += CheckBox_CheckedChanged;
            CheckBoxCam5IsSaveOKImage.CheckedChanged += CheckBox_CheckedChanged;
            CheckBoxCam5IsSaveNGImage.CheckedChanged += CheckBox_CheckedChanged;
            CheckBoxCam6IsSaveOKImage.CheckedChanged += CheckBox_CheckedChanged;
            CheckBoxCam6IsSaveNGImage.CheckedChanged += CheckBox_CheckedChanged;

            //2026.9 V22-G-s
            CheckBoxCam7IsSaveOKImage.CheckedChanged += CheckBox_CheckedChanged;
            CheckBoxCam7IsSaveNGImage.CheckedChanged += CheckBox_CheckedChanged;
            CheckBoxCam8IsSaveOKImage.CheckedChanged += CheckBox_CheckedChanged;
            CheckBoxCam8IsSaveNGImage.CheckedChanged += CheckBox_CheckedChanged;
            CheckBoxCam9IsSaveOKImage.CheckedChanged += CheckBox_CheckedChanged;
            CheckBoxCam9IsSaveNGImage.CheckedChanged += CheckBox_CheckedChanged;
            //2026.9 V22-G-e
        }

        /// <summary>
        /// 保存系统配置按钮点击事件：把控件上的全局开关、PLC 地址、存图开关、曝光/增益
        /// 全部写回内存配置对象，再把旧 JSON 移入备份目录、序列化新配置写回原路径，最后记录日志并弹窗提示。
        /// </summary>
        private void BtnSaveSystemConfigurations_Click(object sender, EventArgs e)
        {
            // 回写全局开关与 PLC 通讯地址、各交互点位地址
            mCameraParameters.ISPAOn2 = CheckBoxISPAOn2.Checked;
            mCameraParameters.ISAutoSaveImage=CheckBoxISAutoSaveImage.Checked;
            mCameraParameters.PLCCommunicationIP = TextBoxPLCIPAddress.Text;
            mCameraParameters.PLCCommunicationPort = int.Parse(TextBoxPLCPort.Text);
            mCameraParameters.SystemOnlineAddress = TextBoxSystemOnlineAddress.Text;
            mCameraParameters.SystemOnlineAddress = TextBoxSystemOnlineAddress.Text;
            mCameraParameters.CarModelCameraFXAddress = TextBoxCarModelCameraFXAddress.Text;
            mCameraParameters.CarModelCameraOKAddress = TextBoxCarModelCameraOKAddress.Text;
            mCameraParameters.CarModelCameraNGAddress = TextBoxCarModelCameraNGAddress.Text;
            mCameraParameters.CarModelCameraCarModelAddress = TextBoxCarModelCameraCarModelAddress.Text;
            mCameraParameters.VINCodeCameraOKAddress = TextBoxVINCodeCameraOKAddress.Text;
            mCameraParameters.VINCodeCameraNGAddress = TextBoxVINCodeCameraNGAddress.Text;
            mCameraParameters.VINCodeCameraVINCodeAddress = TextBoxVINCodeCameraVINCodeAddress.Text;
            mCameraParameters.AutoModelAddress = TextBoxAutoModel.Text;
            //mCameraParameters.AutoModelAddress2 = TextBoxAutoModel2.Text;
            //mCameraParameters.PLCStopAddress = uiTextBox2.Text;
            mCameraParameters.CameraTriggerAddress = TextBoxCameraTrigger.Text;
            mCameraParameters.JobRunDoneAddress = TextBoxJobRunDone.Text;
            // 回写各相机的 OK/NG 图存档开关（按相机名写入对应相机参数）
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "CarIdentificationCamera").IsSaveOKImage = CheckBoxCam1IsSaveOKImage.Checked;
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "CarIdentificationCamera").IsSaveNGImage = CheckBoxCam1IsSaveNGImage.Checked;
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera1").IsSaveOKImage = CheckBoxCam2IsSaveOKImage.Checked;
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera1").IsSaveNGImage = CheckBoxCam2IsSaveNGImage.Checked;
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera2").IsSaveOKImage = CheckBoxCam3IsSaveOKImage.Checked;
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera2").IsSaveNGImage = CheckBoxCam3IsSaveNGImage.Checked;
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera3").IsSaveOKImage = CheckBoxCam4IsSaveOKImage.Checked;
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera3").IsSaveNGImage = CheckBoxCam4IsSaveNGImage.Checked;
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera4").IsSaveOKImage = CheckBoxCam5IsSaveOKImage.Checked;
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera4").IsSaveNGImage = CheckBoxCam5IsSaveNGImage.Checked;
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera5").IsSaveOKImage = CheckBoxCam6IsSaveOKImage.Checked;
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera5").IsSaveNGImage = CheckBoxCam6IsSaveNGImage.Checked;

            //2026.9 V22-G-s
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera6").IsSaveOKImage = CheckBoxCam6IsSaveOKImage.Checked;
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera6").IsSaveNGImage = CheckBoxCam6IsSaveNGImage.Checked;
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera7").IsSaveOKImage = CheckBoxCam7IsSaveOKImage.Checked;
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera7").IsSaveNGImage = CheckBoxCam7IsSaveNGImage.Checked;
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera8").IsSaveOKImage = CheckBoxCam8IsSaveOKImage.Checked;
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera8").IsSaveNGImage = CheckBoxCam8IsSaveNGImage.Checked;
            //2026.9 V22-G-e

            // 回写各相机曝光/增益（文本框内容转 float 后按相机名写入对应相机参数）
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "CarIdentificationCamera").CameraExposure = uiTextBox3.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "CarIdentificationCamera").CameraGain = uiTextBox4.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera1").CameraExposure = uiTextBoxGS8.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera1").CameraGain = uiTextBoxGS8_.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera2").CameraExposure = uiTextBoxM8.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera2").CameraGain = uiTextBoxM8_.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera3").CameraExposure = uiTextBoxM6.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera3").CameraGain = uiTextBoxM6_.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera4").CameraExposure = uiTextBoxAM8.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera4").CameraGain = uiTextBoxAM8_.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera5").CameraExposure = uiTextBoxA09.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera5").CameraGain = uiTextBoxA09_.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera6").CameraExposure = uiTextBox9.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera6").CameraGain = uiTextBox9_.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera99").CameraExposure = uiTextBox8.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera99").CameraGain = uiTextBox8_.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera8").CameraExposure = uiTextBox6.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera8").CameraGain = uiTextBox6_.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera9").CameraExposure = uiTextBox7.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera9").CameraGain = uiTextBox7_.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera10").CameraExposure = uiTextBox5.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera10").CameraGain = uiTextBox5_.Text.ToFloat();

            //V22-G-s
            //反向写入配置文件11号位置
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera7").CameraExposure = V22_G_CameraExposure.Text.ToFloat();
            mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera7").CameraGain =V22_G_CameraGain.Text.ToFloat();
            //V22-G-e



            // 确保配置备份目录存在（..\Configurations\备份）
            if (!Directory.Exists(Path.GetFullPath("..") + "\\Configurations\\备份"))
            {
                Directory.CreateDirectory(Path.GetFullPath("..") + "\\Configurations\\备份");
            }

            // 先把当前配置 JSON 按时间戳移入备份目录，再把最新配置序列化写回原路径，防止误覆盖丢失
            File.Move(CameraParameterConfigurationPath, Path.GetFullPath("..") + "\\Configurations\\备份\\CameraConfigurations" + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss_ffff") + ".json");
            Json.SerializeToFile(mCameraParameters, CameraParameterConfigurationPath, Encoding.UTF8);
            // 记录日志并弹窗提示保存完成
            Log.Info("系统文件保存完成");
            UIMessageBox.ShowSuccess("系统文件保存完成");
        }
        /// <summary>
        /// 所有存图开关复选框共用的 CheckedChanged 事件：按控件名把勾选状态
        /// 即时写回对应相机的 OK/NG 存档配置；权限等级不大于 0（默认技术员）时拒绝修改并弹窗提示。
        /// </summary>
        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // 权限校验：仅二级（权限 2）及以上用户可修改存图配置
            if (mainForm.mUser.Authority > 0)
            {
                // 按触发事件的复选框名分发，把勾选状态写回对应相机的存图开关
                switch (((UICheckBox)sender).Name)
                {
                    case "CheckBoxCam1IsSaveOKImage":
                        mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "CarIdentificationCamera").IsSaveOKImage = CheckBoxCam1IsSaveOKImage.Checked;
                        break;
                    case "CheckBoxCam1IsSaveNGImage":
                        mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "CarIdentificationCamera").IsSaveNGImage = CheckBoxCam1IsSaveNGImage.Checked;
                        break;
                    case "CheckBoxCam2IsSaveOKImage":
                        mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera1").IsSaveOKImage = CheckBoxCam2IsSaveOKImage.Checked;
                        break;
                    case "CheckBoxCam2IsSaveNGImage":
                        mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera1").IsSaveNGImage = CheckBoxCam2IsSaveNGImage.Checked;
                        break;
                    case "CheckBoxCam3IsSaveOKImage":
                        mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera2").IsSaveOKImage = CheckBoxCam3IsSaveOKImage.Checked;
                        break;
                    case "CheckBoxCam3IsSaveNGImage":
                        mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera2").IsSaveNGImage = CheckBoxCam3IsSaveNGImage.Checked;
                        break;
                    case "CheckBoxCam4IsSaveOKImage":
                        mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera3").IsSaveOKImage = CheckBoxCam4IsSaveOKImage.Checked;
                        break;
                    case "CheckBoxCam4IsSaveNGImage":
                        mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera3").IsSaveNGImage = CheckBoxCam4IsSaveNGImage.Checked;
                        break;
                    case "CheckBoxCam5IsSaveOKImage":
                        mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera4").IsSaveOKImage = CheckBoxCam5IsSaveOKImage.Checked;
                        break;
                    case "CheckBoxCam5IsSaveNGImage":
                        mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera4").IsSaveNGImage = CheckBoxCam5IsSaveNGImage.Checked;
                        break;
                    case "CheckBoxCam6IsSaveOKImage":
                        mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera5").IsSaveOKImage = CheckBoxCam6IsSaveOKImage.Checked;
                        break;
                    case "CheckBoxCam6IsSaveNGImage":
                        mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera5").IsSaveNGImage = CheckBoxCam6IsSaveNGImage.Checked;
                        break;
                    //2026.9 V22-G-s
                    case "CheckBoxCam7IsSaveOKImage":
                        mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera6").IsSaveOKImage = CheckBoxCam6IsSaveOKImage.Checked;
                        break;
                    case "CheckBoxCam7IsSaveNGImage":
                        mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera6").IsSaveNGImage = CheckBoxCam6IsSaveNGImage.Checked;
                        break;
                    case "CheckBoxCam8IsSaveOKImage":
                        mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera7").IsSaveOKImage = CheckBoxCam6IsSaveOKImage.Checked;
                        break;
                    case "CheckBoxCam8IsSaveNGImage":
                        mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera7").IsSaveNGImage = CheckBoxCam6IsSaveNGImage.Checked;
                        break;
                    case "CheckBoxCam9IsSaveOKImage":
                        mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera8").IsSaveOKImage = CheckBoxCam6IsSaveOKImage.Checked;
                        break;
                    case "CheckBoxCam9IsSaveNGImage":
                        mCameraParameters.ListCameraParameters.Find(p => p.CameraName == "VINCodeCamera8").IsSaveNGImage = CheckBoxCam6IsSaveNGImage.Checked;
                        break;
                    //2026.9 V22-G-e

                    default:
                        break;
                }
            }
            else
            {
                UIMessageBox.ShowWarning("亲，你目前的权限不够哦，请使用技术员/管理员权限，谢谢！");
            }
        }
        /// <summary>
        /// 全选/取消按钮点击事件：一键勾选或取消所有相机的 OK/NG 存图开关
        /// （复选框状态变化会触发 CheckBox_CheckedChanged 自动回写配置）；权限不足时弹窗提示。
        /// </summary>
        private void Btn_Click(object sender, EventArgs e)
        {
            // 权限校验：仅二级（权限 2）及以上用户可操作
            if (mainForm.mUser.Authority > 0)
            {
                // 按被点击的按钮名分发处理（当前仅“全选”按钮）
                switch (((UIButton)sender).Name)
                {
                    case "Btn1全选":
                        // 按钮文本为“全选”时勾选全部存图开关，否则全部取消勾选
                        if (((UIButton)sender).Text == "全选")
                        {
                            CheckBoxCam1IsSaveOKImage.Checked = true;
                            CheckBoxCam1IsSaveNGImage.Checked = true;
                            CheckBoxCam2IsSaveOKImage.Checked = true;
                            CheckBoxCam2IsSaveNGImage.Checked = true;
                            CheckBoxCam3IsSaveOKImage.Checked = true;
                            CheckBoxCam3IsSaveNGImage.Checked = true;
                            CheckBoxCam4IsSaveOKImage.Checked = true;
                            CheckBoxCam4IsSaveNGImage.Checked = true;
                            CheckBoxCam5IsSaveOKImage.Checked = true;
                            CheckBoxCam5IsSaveNGImage.Checked = true;
                            CheckBoxCam6IsSaveOKImage.Checked = true;
                            CheckBoxCam6IsSaveNGImage.Checked = true;
                            //2026.9 V22-G-s
                            CheckBoxCam7IsSaveOKImage.Checked = true;
                            CheckBoxCam7IsSaveNGImage.Checked = true;
                            CheckBoxCam8IsSaveOKImage.Checked = true;
                            CheckBoxCam8IsSaveNGImage.Checked = true;
                            CheckBoxCam9IsSaveOKImage.Checked = true;
                            CheckBoxCam9IsSaveNGImage.Checked = true;
                            //2026.9 V22-G-e
                            ((UIButton)sender).Text = "取消";
                        }
                        else
                        {
                            CheckBoxCam1IsSaveOKImage.Checked = false;
                            CheckBoxCam1IsSaveNGImage.Checked = false;
                            CheckBoxCam2IsSaveOKImage.Checked = false;
                            CheckBoxCam2IsSaveNGImage.Checked = false;
                            CheckBoxCam3IsSaveOKImage.Checked = false;
                            CheckBoxCam3IsSaveNGImage.Checked = false;
                            CheckBoxCam4IsSaveOKImage.Checked = false;
                            CheckBoxCam4IsSaveNGImage.Checked = false;
                            CheckBoxCam5IsSaveOKImage.Checked = false;
                            CheckBoxCam5IsSaveNGImage.Checked = false;
                            CheckBoxCam6IsSaveOKImage.Checked = false;
                            CheckBoxCam6IsSaveNGImage.Checked = false;
                            //2026.9 V22-G-s
                            CheckBoxCam7IsSaveOKImage.Checked = false;
                            CheckBoxCam7IsSaveNGImage.Checked = false;
                            CheckBoxCam8IsSaveOKImage.Checked = false;
                            CheckBoxCam8IsSaveNGImage.Checked = false;
                            CheckBoxCam9IsSaveOKImage.Checked = false;
                            CheckBoxCam9IsSaveNGImage.Checked = false;
                            //2026.9 V22-G-e
                            ((UIButton)sender).Text = "全选";
                        }
                        break;
                }
            }
            else
            {
                UIMessageBox.ShowWarning("亲，你目前的权限不够哦，请使用技术员/管理员权限，谢谢！");
            }
        }

        /// <summary>
        /// 自动存图开关的 CheckedChanged 事件：当前为空实现。
        /// </summary>
        private void CheckBoxISAutoSaveImage_CheckedChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 复选框 uiCheckBox2 的 CheckedChanged 事件：当前为空实现。
        /// </summary>
        private void uiCheckBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 复选框 uiCheckBox2 的第二个 CheckedChanged 事件（设计器生成）：当前为空实现。
        /// </summary>
        private void uiCheckBox2_CheckedChanged_1(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// CheckBoxCam6IsSaveOKImage 的默认 CheckedChanged 事件（设计器生成）：
        /// 当前为空实现，存图开关的统一处理走 CheckBox_CheckedChanged。
        /// </summary>
        private void CheckBoxCam6IsSaveOKImage_CheckedChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 标签 uiLabel35 的点击事件：当前为空实现。
        /// </summary>
        private void uiLabel35_Click(object sender, EventArgs e)
        {

        }
    }
}
