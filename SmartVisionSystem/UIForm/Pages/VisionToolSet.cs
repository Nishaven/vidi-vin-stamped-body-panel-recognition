// ============================================================
// 文件：VisionToolSet.cs
// 模块：UIForm/Pages（视觉工具设置页 —— 遗留文件）
// 说明：VisionPro 时代的视觉工具（ToolBlock）编辑页面，提供工具算法保存、离线图片
//       回放测试、手动拍照、曝光/增益设置等功能。【遗留文件，当前未被编译引用——
//       不在 SmartVisionSystem.csproj 编译清单内，现行系统已改用 ViDi 深度学习方案】
// ============================================================
using Cognex.VisionPro;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SmartVisionSystem.Log4Net;
using SmartVisionSystem.VisionCore;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ToolBlock;

namespace SmartVisionSystem.UIForm.Pages
{
    /// <summary>
    /// 视觉工具设置窗口（VisionPro 时代遗留）：内嵌 CogToolBlockEditV2 编辑器调阅相机的
    /// ToolBlock 算法，并支持保存算法文件、离线图片回放、手动拍照与曝光/增益调整。
    /// 遗留文件，当前未被编译引用。
    /// </summary>
    public partial class VisionToolSet : Sunny.UI.UIForm
    {
        // VisionPro ToolBlock 可视化编辑控件（算法流程编辑界面）
        CogToolBlockEditV2 ToolBlockEditV21 = new CogToolBlockEditV2();
        // 相机参数配置文件路径（CameraConfigurations.json，遗留用法）
        private readonly string CameraParameterConfigurationPath = Path.GetFullPath("..") + "\\Configurations\\CameraConfigurations.json";
        // 当前页面绑定的视觉工具对象（相机 + ToolBlock 算法）
        VisionTools MyVisionTools;
        // 主窗体引用，用于取相机参数列表等
        MainForm mMainForm;
        public VisionToolSet()
        {
        }

        /// <summary>
        /// 主构造函数：按相机名初始化窗口标题与车型下拉框，并挂载 ToolBlock 编辑器、
        /// 回显当前相机的增益/曝光参数。
        /// </summary>
        /// <param name="mainForm">主窗体</param>
        /// <param name="mVisionTools">该相机对应的视觉工具对象</param>
        public VisionToolSet(MainForm mainForm, VisionTools mVisionTools)
        {
            InitializeComponent();
            MyVisionTools = mVisionTools;
            mMainForm = mainForm;
            this.Text = $"工具设置--{MyVisionTools.MyCameraBase.MyCameraParameters.CameraName}";
            // 车型识别相机无车型选择；读码相机按相机名绑定可测试的车型（离线回放时用）
            if (MyVisionTools.MyCameraBase.MyCameraParameters.CameraName == "CarIdentificationCamera")
            {
                uiLabel4.Visible = false;
                ComboBoxCarModel.Visible = false;
            }
            else
            {
                uiLabel4.Visible = true;
                ComboBoxCarModel.Visible = true;
                ComboBoxCarModel.Items.Clear();
                if (MyVisionTools.MyCameraBase.MyCameraParameters.CameraName == "VINCodeCamera1")
                {
                    ComboBoxCarModel.Items.Add("GS8");
                    ComboBoxCarModel.SelectedItem = "GS8";
                }
                else if (MyVisionTools.MyCameraBase.MyCameraParameters.CameraName == "VINCodeCamera2")
                {
                    ComboBoxCarModel.Items.Add("M8");
                    ComboBoxCarModel.SelectedItem = "M8";
                }
                else
                {
                    ComboBoxCarModel.Items.Add("M6");
                    ComboBoxCarModel.Items.Add("AM8");
                    ComboBoxCarModel.Items.Add("A09");
                    ComboBoxCarModel.SelectedItem = "M6";
                }
            }
            ToolBlockEditV21.Dock = DockStyle.Fill;
            ToolBlockEditV21.Margin = new Padding(0);
            uiTableLayoutPanel1.Controls.Add(ToolBlockEditV21, 0, 0);
            ToolBlockEditV21.Subject = MyVisionTools.CameraToolBlock.ToolBlock;
            TextBoxGain.Text = MyVisionTools.MyCameraBase.MyCameraParameters.CameraGain.ToString();
            TextBoxExposures.Text = MyVisionTools.MyCameraBase.MyCameraParameters.CameraExposure.ToString();
        }

        /// <summary>
        /// 保存视觉工具算法：把当前 ToolBlock 序列化保存到 .vpp 算法文件，并弹窗提示 + 记录日志
        /// </summary>
        private void BtnSaveTools_Click(object sender, EventArgs e)
        {
            CogSerializer.SaveObjectToFile(MyVisionTools.CameraToolBlock.ToolBlock, MyVisionTools.CameraToolBlock.Path, typeof(System.Runtime.Serialization.Formatters.Binary.BinaryFormatter), CogSerializationOptionsConstants.Minimum);
            ShowSuccessNotifier($"{Directory.GetParent(Path.GetDirectoryName(MyVisionTools.CameraToolBlock.Path)).Name}视觉工具算法配置文件{MyVisionTools.CameraToolBlock.Name}保存完成");
            Log.Info($"{Directory.GetParent(Path.GetDirectoryName(MyVisionTools.CameraToolBlock.Path)).Name}视觉工具算法配置文件{MyVisionTools.CameraToolBlock.Name}保存完成");
        }

        /// <summary>
        /// 离线回放测试：选择本地 bmp/png 图片（单次最多 300 张），逐张压入相机图像队列
        /// 并按当前选中车型触发一次算法处理，模拟在线读码流程。
        /// </summary>
        private void BtnOfflineRunTools_Click(object sender, EventArgs e)
        {
            CogImageFile ImageFile = new CogImageFile();
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "选择数据源文件",
                Filter = "bmp文件|*.bmp|png文件|*.png",
                FileName = string.Empty,
                FilterIndex = 1,
                Multiselect = true,
                RestoreDirectory = true,
                DefaultExt = "bmp"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (openFileDialog.FileNames.Length > 300)
                {
                    ShowErrorDialog("警告", "图像数量过大，请控制在每次300张以内测试！", UIStyle.Red, true);
                    return;
                }
                // 逐张入队并唤醒处理线程：走与在线拍照相同的图像处理链路
                for (int i = 0; i < openFileDialog.FileNames.Length; i++)
                {
                    try
                    {
                        MyVisionTools.MyCameraBase.AutoStatus = false;
                        ImageFile.Open(openFileDialog.FileNames[i], CogImageFileModeConstants.Read);
                        MyVisionTools.MyCameraBase.ImageQueue.Enqueue(ImageFile[0]);
                        MyVisionTools.MyCameraBase.CarModel = ComboBoxCarModel.SelectedItem.ToString();
                        MyVisionTools.MyCameraBase.IsAutoStatus = false;
                        MyVisionTools.MyCameraBase.mAutoResetEvent.Set();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("加载/运行图像失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// 手动拍照：关闭自动状态后向相机发一次软触发，采集一帧图像
        /// </summary>
        private void BtnRunAcqImage_Click(object sender, EventArgs e)
        {
            try
            {
                MyVisionTools.MyCameraBase.AutoStatus = false;
                MyVisionTools.MyCameraBase.SoftWareTrigger();
            }
            catch (Exception ex)
            {
                UIMessageBox.ShowError("拍照失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 曝光设置：输入框内回车后更新参数并下发到相机（单位 ms，下发时换算为 μs）。
        /// 注意实际下发走的是车型识别相机的相机对象。
        /// </summary>
        private void TextBoxExposures_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                MyVisionTools.MyCameraBase.MyCameraParameters.CameraExposure = double.Parse(TextBoxExposures.Text);
                mMainForm.mCarIdentificationCamera.CurrentCamera.MV_CC_SetExposureTime_NET(((float)MyVisionTools.MyCameraBase.MyCameraParameters.CameraExposure) * 1000);
                //foreach (CameraParameters item in mMainForm.ListCameraParameters)
                //{
                //    if (item.CameraName == mMainForm.ListCameraParameters.First(p => p.CameraName == MyVisionTools.CameraName).CameraName)
                //    {
                //        item.CameraExposure = double.Parse(TextBoxExposures.Text);
                //        Json.SerializeToFile(mMainForm.ListCameraParameters, CameraParameterConfigurationPath, Encoding.UTF8);
                //        break;
                //    }
                //}
            }
        }

        /// <summary>
        /// 增益设置：输入框内回车后更新参数并下发到相机。
        /// 注意实际下发走的是车型识别相机的相机对象。
        /// </summary>
        private void TextBoxGain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                MyVisionTools.MyCameraBase.MyCameraParameters.CameraGain = double.Parse(TextBoxGain.Text);
                mMainForm.mCarIdentificationCamera.CurrentCamera.MV_CC_SetGain_NET((float)MyVisionTools.MyCameraBase.MyCameraParameters.CameraGain);

                //foreach (CameraParameters item in mMainForm.ListCameraParameters)
                //{
                //    if (item.CameraName == mMainForm.ListCameraParameters.First(p => p.CameraName == MyVisionTools.CameraName).CameraName)
                //    {
                //        item.CameraGain = double.Parse(TextBoxGain.Text);
                //        Json.SerializeToFile(mMainForm.ListCameraParameters, CameraParameterConfigurationPath, Encoding.UTF8);
                //        break;
                //    }
                //}
            }
        }

        /// <summary>
        /// 窗口关闭清理：解除编辑器与 ToolBlock 的绑定，并恢复相机自动处理状态
        /// </summary>
        private void VisionToolSet_FormClosed(object sender, FormClosedEventArgs e)
        {
            ToolBlockEditV21.Subject = null;
            MyVisionTools.MyCameraBase.IsAutoStatus = true;
        }
    }
}
