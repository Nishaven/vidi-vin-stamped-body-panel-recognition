// ============================================================
// 文件：CameraSet.cs
// 模块：UIForm（界面层）
// 说明：相机参数设置窗体（UIForm，遗留页面）。显示并下发单台相机的曝光/增益，
//       回车确认后写回 CameraConfigurations.json；构造中 mCameraParameters 未赋值，属遗留问题。
// ============================================================
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SmartVisionSystem.Log4Net;
using SmartVisionSystem.VisionCore;

namespace SmartVisionSystem.UIForm.Pages
{
    /// <summary>
    /// 相机参数设置窗体（遗留页面）：对单台相机显示并修改曝光、增益，
    /// 回车确认即下发到相机并序列化写回 JSON 配置。
    /// </summary>
    public partial class CameraSet : Sunny.UI.UIForm
    {
        //public ICogAcqFifo CurAcqFifo;
        // 目标相机的视觉工具集，用于把曝光/增益下发到相机
        private VisionTools mvisionTools;
        // 主窗体引用（仅在注释掉的旧逻辑中用到）
        private MainForm mainForm;
        // 相机参数对象（注意：构造函数中未赋值，后续直接访问其成员存在空引用隐患）
        private CameraParameters mCameraParameters;
        // 相机配置文件绝对路径：..\Configurations\CameraConfigurations.json
        private readonly string CameraParameterConfigurationPath = Path.GetFullPath("..") + "\\Configurations\\CameraConfigurations.json";
        
        /// <summary>
        /// 构造函数：初始化控件并保存视觉工具与主窗体引用，随后把相机增益/曝光回显到文本框。
        /// 注意：mCameraParameters 的赋值语句已被注释（未赋值），此处访问其成员存在空引用隐患（遗留问题）。
        /// </summary>
        /// <param name="visionTools">目标相机的视觉工具集</param>
        /// <param name="main">主窗体引用</param>
        public CameraSet(VisionTools visionTools, MainForm main)
        {
            InitializeComponent();
            mainForm = main;
            mvisionTools = visionTools;
            //mCameraParameters = main.MyCameraParameters;
            // 回显当前增益/曝光（依赖的 mCameraParameters 未赋值，存在空引用隐患）
            TextBoxGain.Text = mCameraParameters.CameraGain.ToString();
            TextBoxExposures.Text = mCameraParameters.CameraExposure.ToString();
            
        }
        /// <summary>
        /// 上光源开关按钮点击事件：光源开关切换逻辑已整体注释，当前为空实现。
        /// </summary>
        private void BtnStartUpLiveImage_Click(object sender, EventArgs e)
        {
            //switch (BtnStartUpLiveImage.Text)
            //{
            //    case "打开上光源":
            //        mainForm.OperateLight(BtnStartUpLiveImage.Text);
            //        BtnStartUpLiveImage.Text = "关闭上光源";
            //        break;
            //    case "关闭上光源":
            //        mainForm.OperateLight(BtnStartUpLiveImage.Text);
            //        BtnStartUpLiveImage.Text = "打开上光源";
            //        break;
            //    default:
            //        break;
            //}
        }
        /// <summary>
        /// 保存工具参数按钮点击事件：把文本框中的曝光/增益写入参数对象；
        /// 后续的 JSON 序列化与备份逻辑均已注释，当前仅更新内存值。
        /// </summary>
        private void BtnSaveTools_Click(object sender, EventArgs e)
        {
            // 回写曝光/增益到参数对象（未持久化到文件）
            mCameraParameters.CameraExposure = double.Parse(TextBoxExposures.Text);
            mCameraParameters.CameraGain = double.Parse(TextBoxGain.Text);
            //Json.SerializeToFile(mvisionTools.mXCodeTrackParameter.CurrentTrackParameters, mvisionTools.mXCodeTrackParameter.Path, Encoding.UTF8);
            //Log.Info("系统文件保存完成");
            //UIMessageBox.ShowSuccess("系统文件保存完成");
            //#region 备份
            //string BackupPath = Path.GetFullPath("..") + $@"\Backups\Configurations\";
            //FileOperationClass fileOperationClass = new FileOperationClass();
            //fileOperationClass.CopyToFile(Path.GetFullPath("..") + $@"\Configurations\CameraConfigurations.json", BackupPath, true);
            //string TrackBackupPath = Path.GetFullPath("..") + $@"\Backups\CameraVisionTool\ClassifyCamera\{mCameraParameters.ProductName}";
            //fileOperationClass.CopyToFile(mvisionTools.mXCodeTrackParameter.Path, TrackBackupPath, true);
            //#endregion
        }
        /// <summary>
        /// 曝光文本框按键事件：按回车确认时把输入值下发到相机，
        /// 同步到参数对象并序列化写回 CameraConfigurations.json。
        /// </summary>
        private void TextBoxExposures_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // 下发曝光值到相机并同步到参数对象，随后持久化到 JSON
                mvisionTools.MyCameraBase.Exposure = double.Parse(TextBoxExposures.Text);
                mCameraParameters.CameraExposure = mvisionTools.MyCameraBase.Exposure;
                Json.SerializeToFile(mCameraParameters, CameraParameterConfigurationPath, Encoding.UTF8);
            }
        }
        /// <summary>
        /// 增益文本框按键事件：按回车确认时把输入值下发到相机，
        /// 同步到参数对象并序列化写回 CameraConfigurations.json。
        /// </summary>
        private void TextBoxGain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // 下发增益值到相机并同步到参数对象，随后持久化到 JSON
                mvisionTools.MyCameraBase.Gain = double.Parse(TextBoxGain.Text);
                mCameraParameters.CameraGain = mvisionTools.MyCameraBase.Gain;
                Json.SerializeToFile(mCameraParameters, CameraParameterConfigurationPath, Encoding.UTF8);
            }
        }
        /// <summary>
        /// 窗体关闭事件：弹窗询问是否保存参数，确认则把参数对象序列化写回 JSON，
        /// 记录日志并提示保存完成。
        /// </summary>
        private void CameraSet_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (UIMessageBox.ShowAsk("是否保存参数？") == true)
            {
                Json.SerializeToFile(mCameraParameters, CameraParameterConfigurationPath, Encoding.UTF8);
                Log.Info("系统文件保存完成");
                UIMessageBox.ShowSuccess("系统文件保存完成");
            }
        }
    }
}
