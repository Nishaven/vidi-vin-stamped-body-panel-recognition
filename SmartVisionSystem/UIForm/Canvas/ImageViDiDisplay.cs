// ============================================================
// 文件：ImageViDiDisplay.cs
// 模块：UIForm（界面层）
// 说明：单相机画面控件（UserControl）。通过 ElementHost 承载 WPF 的 ViDi SampleViewer
//       显示推理标注图，并按相机名初始化车型下拉框，支持手动触发拍照与本地图片回放测试。
// ============================================================
using Cognex.VisionPro;
using Sunny.UI;
using Sunny.UI.Win32;
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
using MvCamCtrl.NET;
using SmartVisionSystem.UIForm.Pages;
using ViDi2.UI;
using Cognex.VisionPro.ImageFile;

namespace SmartVisionSystem.UIForm.Canvas
{
    /// <summary>
    /// 单相机画面控件：以 ElementHost 嵌入 WPF 的 ViDi SampleViewer 显示该相机的推理标注图像，
    /// 按相机名初始化车型下拉框，并提供手动软触发拍照与本地图片离线回放入口。
    /// </summary>
    public partial class ImageViDiDisplay : UserControl
    {
        // 控制器 API 引用（本控件代码中未赋值使用）
        public CSTControllerAPI mController;
        // 该画面对应相机的视觉工具集（离线加载图片回放时按相机名赋值）
        public VisionTools mVisionTools;
        // WPF 的 ViDi 样本查看器控件，用于显示 ViDi 推理结果标注图
        public SampleViewer mSampleViewer;
        // 主窗体引用，用于取相机对象执行软触发等操作
        MainForm mMainForm;
        /// <summary>
        /// 构造函数：把传入的 ViDi SampleViewer 挂载到 ElementHost 上显示画面，
        /// 并按 SampleViewer 名称（相机名）初始化车型下拉框：车型识别相机隐藏车型选择，
        /// 各 VIN 码相机填充各自支持的车型并设默认选中项。
        /// </summary>
        /// <param name="sampleViewer">该相机对应的 ViDi 样本查看器（以 Name 区分相机）</param>
        /// <param name="main">主窗体引用</param>
        public ImageViDiDisplay(SampleViewer sampleViewer, MainForm main)
        {
            mMainForm = main;

            mSampleViewer = sampleViewer;
            InitializeComponent();
            // 把 WPF 的 SampleViewer 嵌入 WinForms 的 ElementHost，用于显示推理标注图
            ElementHostViDi.Child = mSampleViewer;
            // 先显示并清空车型下拉框，再按相机名决定是否显示及填充哪些车型
            ComboBoxCarModel.Visible = true;
            ComboBoxCarModel.Items.Clear();
            // 车型识别相机：不涉及车型选择，隐藏车型下拉框与提示标签
            if (mSampleViewer.Name == "CarIdentificationCameraSampleViewer")
            {
                ComboBoxCarModel.Visible = false;
                uiLabel4.Visible = false;
            }
            // 以下各 VIN 码相机分支：填充该画面支持的车型列表并设置默认选中项（用于选择对应车型配方）
            else if (mSampleViewer.Name == "VINCodeCamera1SampleViewer")
            {
                ComboBoxCarModel.Items.Add("A09");
                ComboBoxCarModel.Items.Add("T58");
                ComboBoxCarModel.Items.Add("A39");
                //ComboBoxCarModel.Items.Add("T68");
                ComboBoxCarModel.SelectedItem = "T58";
            }
            else if (mSampleViewer.Name == "VINCodeCamera2SampleViewer")
            {
                ComboBoxCarModel.Items.Add("T68");
                ComboBoxCarModel.Items.Add("A66");
                ComboBoxCarModel.Items.Add("T09-M");
                ComboBoxCarModel.SelectedItem = "T68";
            }
            else if (mSampleViewer.Name == "VINCodeCamera3SampleViewer")
            {
                ComboBoxCarModel.Items.Add("T68-G");
                ComboBoxCarModel.Items.Add("A66右舵");
                ComboBoxCarModel.Items.Add("CS145");
                ComboBoxCarModel.SelectedItem = "T68-G";
            }
            else if (mSampleViewer.Name == "VINCodeCamera4SampleViewer")
            {
                ComboBoxCarModel.Items.Add("T58-G");
                ComboBoxCarModel.SelectedItem = "T58-G";
            }
            else if (mSampleViewer.Name == "VINCodeCamera5SampleViewer")
            {
                ComboBoxCarModel.Items.Add("V22");
                ComboBoxCarModel.SelectedItem = "V22";
            }

            //2026.9 v22-G-s
            else if (mSampleViewer.Name == "VINCodeCamera6SampleViewer")
            {
                ComboBoxCarModel.Items.Add("V22-G");
                ComboBoxCarModel.SelectedItem = "V22-G";
            }
            //2026.9 v22-G-e

            //2026.9
            else if (mSampleViewer.Name == "VINCodeCamera7SampleViewer")
            {
                ComboBoxCarModel.Items.Add("NONE");
                ComboBoxCarModel.SelectedItem = "NONE";
            }
            else if (mSampleViewer.Name == "VINCodeCamera8SampleViewer")
            {
                ComboBoxCarModel.Items.Add("NONE");
                ComboBoxCarModel.SelectedItem = "NONE";
            }
            //2026.9
        }
        /// <summary>
        /// 画面内按钮的统一点击事件（UIHeaderButton）：BtnRun——手动软触发对应相机拍一张；
        /// BtnLoadImage——选择本地图片入队做离线推理测试；操作前校验用户权限，异常时弹窗提示。
        /// </summary>
        private void Btn_Click(object sender, EventArgs e)
        {
            //if (mMainForm.bAutoModel == true)
            //{
            //    UIMessageBox.ShowWarning("当前为自动运行状态，需要切换为手动状态才能点击！");
            //    return;
            //}

            // 权限校验：等级 ≥ 1（二级/管理员）才允许手动触发与离线回放
            if (mMainForm.mUser.Authority >= 1)
            {
                // 按被点击的按钮名分发处理
                switch (((UIHeaderButton)sender).Name)
                {
                    // 手动触发：对画面所属相机执行一次软触发拍照
                    case "BtnRun":
                        try
                        {
                            //mVisionTools.MyCameraBase.AutoStatus = false;
                            // 按 SampleViewer 名称找到对应相机执行软触发
                            if (mSampleViewer.Name == "CarIdentificationCameraSampleViewer")
                            {
                                mMainForm.mCarIdentificationCamera.SoftWareTrigger();
                            }
                            else if (mSampleViewer.Name == "VINCodeCamera1SampleViewer")
                            {
                                mMainForm.mVINCodeCamera1.SoftWareTrigger();
                            }
                            else if (mSampleViewer.Name == "VINCodeCamera2SampleViewer")
                            {
                                mMainForm.mVINCodeCamera2.SoftWareTrigger();
                            }
                            else if (mSampleViewer.Name == "VINCodeCamera3SampleViewer")
                            {
                                mMainForm.mVINCodeCamera3.SoftWareTrigger();
                            }
                            else if (mSampleViewer.Name == "VINCodeCamera4SampleViewer")
                            {
                                mMainForm.mVINCodeCamera4.SoftWareTrigger();
                            }
                            else if (mSampleViewer.Name == "VINCodeCamera5SampleViewer")
                            {
                                mMainForm.mVINCodeCamera5.SoftWareTrigger();
                            }
                            //2026.9 v22-G-s
                            else if (mSampleViewer.Name == "VINCodeCamera6SampleViewer")
                            {
                                mMainForm.mVINCodeCamera6.SoftWareTrigger();
                            }
                            else if (mSampleViewer.Name == "VINCodeCamera7SampleViewer")
                            {
                                mMainForm.mVINCodeCamera7.SoftWareTrigger();
                            }
                            else if (mSampleViewer.Name == "VINCodeCamera8SampleViewer")
                            {
                                mMainForm.mVINCodeCamera8.SoftWareTrigger();
                            }
                            //2026.9 v22-G-e
                        }
                        catch (Exception ex)
                        {
                            UIMessageBox.ShowError("拍照失败：" + ex.Message);
                        }
                        break;
                    // 离线回放：选择本地图片入队，模拟采图做推理测试
                    case "BtnLoadImage":
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
                            // 限制单次最多选择 10 张，防止图像队列堆积
                            if (openFileDialog.FileNames.Length > 10)
                            {
                                UIMessageBox.ShowError("图像数量过大，请控制在每次10张以内测试！", true);
                                return;
                            }
                            // 逐张加载所选图片（最多 10 张）
                            for (int i = 0; i < openFileDialog.FileNames.Length && i < 10; i++)
                            {
                                try
                                {
                                    // 按画面所属相机取对应的视觉工具集
                                    if (mSampleViewer.Name == "CarIdentificationCameraSampleViewer")
                                    {
                                        mVisionTools = mMainForm.mCarIdentificationCamera.MyVisionTools;
                                    }
                                    else if (mSampleViewer.Name == "VINCodeCamera1SampleViewer")
                                    {
                                        mVisionTools = mMainForm.mVINCodeCamera1.MyVisionTools;
                                    }
                                    else if (mSampleViewer.Name == "VINCodeCamera2SampleViewer")
                                    {
                                        mVisionTools = mMainForm.mVINCodeCamera2.MyVisionTools;
                                    }
                                    else if (mSampleViewer.Name == "VINCodeCamera3SampleViewer")
                                    {
                                        mVisionTools = mMainForm.mVINCodeCamera3.MyVisionTools;
                                    }
                                    else if (mSampleViewer.Name == "VINCodeCamera4SampleViewer")
                                    {
                                        mVisionTools = mMainForm.mVINCodeCamera4.MyVisionTools;
                                    }
                                    else if (mSampleViewer.Name == "VINCodeCamera5SampleViewer")
                                    {
                                        mVisionTools = mMainForm.mVINCodeCamera5.MyVisionTools;
                                    }
                                    //2026.9 v22-G-s
                                    else if (mSampleViewer.Name == "VINCodeCamera6SampleViewer")
                                    {
                                        mVisionTools = mMainForm.mVINCodeCamera6.MyVisionTools;
                                    }
                                    else if (mSampleViewer.Name == "VINCodeCamera7SampleViewer")
                                    {
                                        mVisionTools = mMainForm.mVINCodeCamera7.MyVisionTools;
                                    }
                                    else if (mSampleViewer.Name == "VINCodeCamera8SampleViewer")
                                    {
                                        mVisionTools = mMainForm.mVINCodeCamera8.MyVisionTools;
                                    }
                                    //2026.9 v22-G-e
                                    // 切出自动模式，停用相机自动采图
                                    mVisionTools.MyCameraBase.AutoStatus = false;
                                    // 打开图片文件并塞入相机图像队列，模拟一次采图输入
                                    ImageFile.Open(openFileDialog.FileNames[i], CogImageFileModeConstants.Read);
                                    mVisionTools.MyCameraBase.ImageQueue.Enqueue(ImageFile[0]);
                                    // VIN 码相机按界面所选车型设置车型号（后续按车型选择配方）
                                    if (mSampleViewer.Name != "CarIdentificationCameraSampleViewer") mVisionTools.MyCameraBase.CarModel = ComboBoxCarModel.SelectedItem.ToString();
                                    //mVisionTools.MyCameraBase.IsAutoStatus = false;
                                    // 唤醒采图处理线程，处理刚入队的图片
                                    mVisionTools.MyCameraBase.mAutoResetEvent.Set();
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("加载/运行图像失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            else UIMessageBox.ShowWarning("亲，你目前的权限不够哦，请使用技术员/管理员权限，谢谢！");
        }

        /// <summary>
        /// ElementHost 子控件变更事件：当前为空实现，未做处理。
        /// </summary>
        private void ElementHostViDi_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }
    }
}
