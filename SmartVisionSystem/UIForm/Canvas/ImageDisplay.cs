// ============================================================
// 文件：ImageDisplay.cs
// 模块：UIForm/Canvas（相机画面显示控件 —— 遗留文件）
// 说明：VisionPro 时代的相机画面/工具栏控件：按布局类型（定位/ViDi/VisionPro）组织
//       头部按钮，支持手动拍照、相机设置、实时预览与工具设置入口。【遗留文件，
//       当前未被编译引用——不在 SmartVisionSystem.csproj 编译清单内，现行画面
//       显示已由其他控件承担】
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

namespace SmartVisionSystem.UIForm.Canvas
{
    /// <summary>
    /// 画面布局类型：Location=定位/识别相机显示（带工具栏），ViDiDisplay=ViDi 运行结果显示
    /// （无工具栏），VisionproDisplay=VP 检测结果显示（带工具栏）
    /// </summary>
    public enum LayoutDisplay
    {
        [Description("定位相机显示")]
        Location,
        [Description("ViDi运行结果显示")]
        ViDiDisplay,
        [Description("VP检测结果显示")]
        VisionproDisplay
    }

    /// <summary>
    /// 相机画面显示控件（VisionPro 时代遗留）：头部工具栏提供手动拍照、相机设置、
    /// 实时预览、标定与视觉工具设置入口，中部承载相机图像画面。遗留文件，当前未被编译引用。
    /// </summary>
    public partial class ImageDisplay : UserControl
    {
        // VisionPro 图像采集 FIFO（VP 采集链路用，遗留）
        public ICogAcqFifo CurAcqFifo;
        // CST 光源控制器接口（预留封装，当前光源走串口协议）
        public CSTControllerAPI mController;
        // 该画面绑定的视觉工具对象（相机 + 算法）
        public VisionTools mVisionTools;
        // 主窗体引用
        MainForm mMain;
        // 相机参数配置文件路径（遗留用法）
        private readonly string CameraParameterConfigurationPath = Path.GetFullPath("..") + "\\Configurations\\CameraConfigurations.json";
        /// <summary>
        /// 构造函数：按布局类型装配头部工具栏按钮——Location/VisionproDisplay 显示
        /// 相机名、手动拍照、实时预览、工具设置等按钮；ViDiDisplay 仅显示结果、移除工具栏。
        /// </summary>
        /// <param name="layoutDisplay">画面布局类型</param>
        /// <param name="main">主窗体</param>
        public ImageDisplay(LayoutDisplay layoutDisplay, MainForm main)
        {
            InitializeComponent();
            mMain = main;
            HeadPanel.Controls.Clear();
            switch (layoutDisplay)
            {
                case LayoutDisplay.Location:
                    HeadPanel.Controls.Add(LabelCameraName, 0, 0);
                    HeadPanel.Controls.Add(Btn_Run, 1, 0);
                    //HeadPanel.Controls.Add(Btn_CamaraSetting, 3, 0);
                    HeadPanel.Controls.Add(Btn_LiveImage, 2, 0);
                    HeadPanel.Controls.Add(Btn_StopLive, 3, 0);
                    //HeadPanel.Controls.Add(Btn_Calibration, 5, 0);
                    HeadPanel.Controls.Add(Btn_ToolSetting, 4, 0);
                    //HeadPanel.Controls.Add(Btn_ProductChange, 7, 0);
                    break;
                case LayoutDisplay.ViDiDisplay:
                    Controls.Remove(HeadPanel);
                    break;
                case LayoutDisplay.VisionproDisplay:
                    HeadPanel.Controls.Add(LabelCameraName, 0, 0);
                    HeadPanel.Controls.Add(Btn_Run, 1, 0);
                    //HeadPanel.Controls.Add(Btn_CamaraSetting, 3, 0);
                    HeadPanel.Controls.Add(Btn_LiveImage, 2, 0);
                    HeadPanel.Controls.Add(Btn_StopLive, 3, 0);
                    //HeadPanel.Controls.Add(Btn_Calibration, 5, 0);
                    HeadPanel.Controls.Add(Btn_ToolSetting, 4, 0);
                    //HeadPanel.Controls.Add(Btn_ProductChange, 7, 0);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 头部工具栏按钮统一点击处理（需操作员及以上权限）：
        /// 手动拍照/相机设置/实时预览开关/标定（预留，已注释）/视觉工具设置。
        /// </summary>
        private void Btn_Click(object sender, EventArgs e)
        {
            // 权限校验：0=普通查看，>0=操作员可用，>2=管理员（工具设置/标定）
            if (mMain.mUser.Authority > 0)
            {
                switch (((UIHeaderButton)sender).Name)
                {
                    case "Btn_Run":
                        try
                        {
                            mVisionTools.MyCameraBase.AutoStatus = false;
                            mVisionTools.MyCameraBase.SoftWareTrigger();
                        }
                        catch (Exception ex)
                        {
                            UIMessageBox.ShowError("拍照失败：" + ex.Message);
                        }
                        break;
                    case "Btn_CamaraSetting":
                        CameraSet cameraSet = new CameraSet(mVisionTools, mMain);
                        cameraSet.ShowDialog();
                        break;
                    case "Btn_LiveImage":
                        // 实时预览：关闭触发模式改为连续采集，供调视野/对焦用
                        mVisionTools.MyCameraBase.StartLiveImage = true;
                        mVisionTools.MyCameraBase.CurrentCamera.MV_CC_SetEnumValue_NET("TriggerMode", 0);//关闭触发模式
                        if (MyCamera.MV_OK != mVisionTools.MyCameraBase.CurrentCamera.MV_CC_StartGrabbing_NET())// ch:开始采集 | en:Start Grabbing
                        {
                            Log.Warn($"开始实时采集图像，不使用实时采集图像务必点击停止实时按钮！");
                        }
                        break;
                    case "Btn_StopLive":
                        // 停止实时预览：停流后恢复触发模式并重启取流，回到 PLC 触发拍照状态
                        mVisionTools.MyCameraBase.CurrentCamera.MV_CC_StopGrabbing_NET();
                        mVisionTools.MyCameraBase.CurrentCamera.MV_CC_SetEnumValue_NET("TriggerMode", 1);//开启触发模式
                        mVisionTools.MyCameraBase.CurrentCamera.MV_CC_StartGrabbing_NET();
                        Thread.Sleep(50);
                        mVisionTools.MyCameraBase.StartLiveImage = false;
                        break;
                    case "Btn_Calibration":
                        if (mMain.mUser.Authority > 2)
                        {
                            //CamerasCalibration camerasCalibration = new CamerasCalibration(mVisionTools, mMain);
                            //camerasCalibration.ShowDialog();
                        }
                        else
                        {
                            UIMessageBox.ShowWarning("亲，你目前的权限不够哦，请使用管理员权限，谢谢！");
                        }
                        break;
                    case "Btn_ToolSetting":
                        // 视觉工具设置（管理员权限）：按画面名称把对应相机的视觉工具传入设置窗口
                        if (mMain.mUser.Authority > 2)
                        {
                            VisionToolSet visionToolSet;
                            if (this.Name == "CarIdentificationCameraDisplay")
                            {
                                visionToolSet = new VisionToolSet(mMain, mMain.mCarIdentificationCamera.MyVisionTools);
                            }
                            else if (this.Name == "VINCodeCamera1CameraDisplay")
                            {
                                visionToolSet = new VisionToolSet(mMain, mMain.mVINCodeCamera1.MyVisionTools);
                            }
                            else if (this.Name == "VINCodeCamera2CameraDisplay")
                            {
                                visionToolSet = new VisionToolSet(mMain, mMain.mVINCodeCamera2.MyVisionTools);
                            }
                            else if (this.Name == "VINCodeCamera3CameraDisplay")
                            {
                                visionToolSet = new VisionToolSet(mMain, mMain.mVINCodeCamera3.MyVisionTools);
                            }
                            else if (this.Name == "VINCodeCamera4CameraDisplay")
                            {
                                visionToolSet = new VisionToolSet(mMain, mMain.mVINCodeCamera4.MyVisionTools);
                            }
                            else if (this.Name == "VINCodeCamera5CameraDisplay")
                            {
                                visionToolSet = new VisionToolSet(mMain, mMain.mVINCodeCamera5.MyVisionTools);
                            }
                            // V22-G
                            else if (this.Name == "VINCodeCamera6CameraDisplay")
                            {
                                visionToolSet = new VisionToolSet(mMain, mMain.mVINCodeCamera6.MyVisionTools);
                            }
                            else if (this.Name == "VINCodeCamera7CameraDisplay")
                            {
                                visionToolSet = new VisionToolSet(mMain, mMain.mVINCodeCamera7.MyVisionTools);
                            }
                            else if (this.Name == "VINCodeCamera8CameraDisplay")
                            {
                                visionToolSet = new VisionToolSet(mMain, mMain.mVINCodeCamera8.MyVisionTools);
                            }
                            else
                            {
                                visionToolSet = new VisionToolSet(mMain, mMain.mCarIdentificationCamera.MyVisionTools);
                            }
                                visionToolSet.ShowDialog();
                            }
                            else
                            {
                                UIMessageBox.ShowWarning("当前的权限不足，请使用管理员权限，谢谢！");
                            }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                UIMessageBox.ShowWarning("当前的权限不足，请使用技术员权限，谢谢！");
            }

        }
    }
}
