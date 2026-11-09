// ============================================================
// 文件：ImageSaveQueue.cs
// 模块：SmartVisionSystem.VisionCore（异步存图队列）
// 说明：图像异步落盘单例队列：生产者把待存图像（含路径/零件ID）入队后立即返回，
//       后台线程按扩展名分派 VisionPro/系统图像组件写盘；队列满时直接丢帧返回 false。
// ============================================================
using Cognex.VisionPro;
using Cognex.VisionPro.ImageFile;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

namespace SmartVisionSystem.VisionCore
{
    /// <summary>
    /// 异步存图队列（单例：gInit 创建 / gOnly 访问 / gShutDown 关闭）：
    /// 相机检测线程调用 AddToQueue 提交图像后立即返回，后台 workerThread 消费并写盘，
    /// 避免大文件写盘阻塞视觉检测主流程；队列满时丢弃新图返回 false。
    /// </summary>
    public class ImageSaveQueue
    {
        /// <summary>
        /// 队列元素：包装一张待存图像（VisionPro 图像 / 系统图像 / 记录对象三选一）、
        /// 目标保存路径与零件ID，并提供按零件名重命名和写盘能力。
        /// </summary>
        class ImageAndPath
        {
            // VisionPro 图像（ICogImage，按扩展名用 CogImageFile 系列保存）
            public ICogImage CogImage;
            // System.Drawing 图像（统一按 JPEG 保存）
            public Image SystemImage;
            // VisionPro 记录对象（ICogRecord，序列化保存）
            public ICogRecord RecordImage;
            // 是否已写盘（重命名时据此决定是否需要移动物理文件）
            public bool SavedToDisk;
            // 实际写盘的完整路径
            private string m_savedToDiskPath;
            // 所属零件ID（0 表示不关联零件）
            public UInt64 PartID;
            // 单元素锁，保证重命名/写盘的线程安全
            object m_sync;

            // 目标保存路径（可含 {0} 占位符，落盘时以零件名填充）
            public String SavePath;
            /// <summary>
            /// 以 VisionPro 图像构造队列元素。
            /// </summary>
            /// <param name="savePath">保存路径</param>
            /// <param name="cogImage">VisionPro 图像</param>
            /// <param name="partID">零件ID</param>
            public ImageAndPath(String savePath, ICogImage cogImage, UInt64 partID)
            {
                SavePath = savePath;
                CogImage = cogImage;
                SystemImage = null;
                RecordImage = null;
                SavedToDisk = false;
                PartID = partID;
                m_sync = new object();
            }

            /// <summary>
            /// 以 System.Drawing 图像构造队列元素。
            /// </summary>
            /// <param name="savePath">保存路径</param>
            /// <param name="sysImage">系统图像</param>
            /// <param name="partID">零件ID</param>
            public ImageAndPath(String savePath, Image sysImage, UInt64 partID)
            {
                SavePath = savePath;
                SystemImage = sysImage;
                CogImage = null;
                RecordImage = null;
                SavedToDisk = false;
                PartID = partID;
                m_sync = new object();
            }

            /// <summary>
            /// 以 VisionPro 记录对象构造队列元素。
            /// </summary>
            /// <param name="savePath">保存路径</param>
            /// <param name="sysImage">记录对象</param>
            /// <param name="partID">零件ID</param>
            public ImageAndPath(String savePath, ICogRecord sysImage, UInt64 partID)
            {
                SavePath = savePath;
                RecordImage = sysImage;
                CogImage = null;
                SystemImage = null;
                SavedToDisk = false;
                PartID = partID;
                m_sync = new object();
            }

            /// <summary>
            /// 用自身零件ID重算路径并重命名（转调重载方法）。
            /// </summary>
            public String Rename()
            {
                return Rename(PartID);
            }

            /// <summary>
            /// 更新零件ID并按新零件名重算保存路径；若文件已落盘且路径变化，则把物理文件移动到新路径，
            /// 并返回腾空的旧目录（供上层清理）；移动失败静默忽略。
            /// </summary>
            /// <param name="partID">新的零件ID</param>
            /// <returns>移动后腾空的旧目录；无需移动时返回 null</returns>
            public String Rename(UInt64 partID)
            {
                lock (m_sync)
                {
                    PartID = partID;
                    // 用新零件名填充路径占位符得到新路径
                    String partName = ImageSaveQueue.gOnly.GetPartName(PartID);
                    String newSavePath = String.Format(SavePath, partName);
                    if (PartID > 0 && partName != null)
                    {
                        // 已落盘且目标路径变化：物理移动文件到新目录
                        if (SavedToDisk && m_savedToDiskPath != newSavePath)
                        {
                            // Physically move the file on disk
                            try
                            {
                                String emptyDirectory = Path.GetDirectoryName(m_savedToDiskPath);
                                String parentDirectory = Path.GetDirectoryName(newSavePath);
                                Directory.CreateDirectory(parentDirectory);
                                File.Move(m_savedToDiskPath, newSavePath);
                                m_savedToDiskPath = newSavePath;
                                return emptyDirectory;
                            }
                            catch (Exception ex)
                            {
                                // MessageManager.gOnly.Alarm("Unable to move saved image!", AlarmType.Unknown, ex);
                            }
                        }
                    }
                }

                return null;

            }

            /// <summary>
            /// 把图像写入磁盘：先按零件名确定最终路径并创建目录，
            /// 再按图像类型与扩展名分派保存（.png/.bmp/.tif/.vri/.cdb 用 VisionPro 组件，
            /// System.Drawing 图像统一转 JPEG），写盘成功后释放图像引用；异常静默忽略。
            /// </summary>
            public void SaveToDisk()
            {
                lock (m_sync)
                {


                    // 有关联零件时用零件名填充路径占位符，否则按原始路径保存
                    String partName = ImageSaveQueue.gOnly.GetPartName(PartID);
                    if (partName != null && PartID > 0)
                    {
                        m_savedToDiskPath = String.Format(SavePath, partName);
                    }
                    else
                    {
                        m_savedToDiskPath = SavePath;
                    }

                    // 确保目标目录存在（按日期/相机/结果分层）
                    String parentDirectory = Path.GetDirectoryName(m_savedToDiskPath);
                    Directory.CreateDirectory(parentDirectory);

                    // VisionPro 图像：按扩展名选择对应的 CogImageFile 写入器
                    if (CogImage != null)
                    {
                        try
                        {
                            if (SavePath.EndsWith(".png"))
                            {
                                CogImageFilePNG file = new CogImageFilePNG();
                                file.Open(m_savedToDiskPath, CogImageFileModeConstants.Write);
                                file.Append(CogImage);
                                file.Close();
                            }
                            else if (SavePath.EndsWith(".bmp"))
                            {
                                CogImageFileBMP file = new CogImageFileBMP();
                                file.Open(m_savedToDiskPath, CogImageFileModeConstants.Write);
                                file.Append(CogImage);
                                file.Close();
                            }
                            else if (SavePath.EndsWith(".tif") || SavePath.EndsWith(".tiff"))
                            {
                                CogImageFileTIFF file = new CogImageFileTIFF();
                                file.Open(m_savedToDiskPath, CogImageFileModeConstants.Write);
                                file.Append(CogImage);
                                file.Close();
                            }
                            else if (SavePath.EndsWith(".vri"))
                            {
                                CogSerializer.SaveObjectToFile(CogImage, m_savedToDiskPath);
                            }
                            else if (SavePath.EndsWith(".cdb"))
                            {
                                CogImageFileCDB file = new CogImageFileCDB();
                                file.Open(m_savedToDiskPath, CogImageFileModeConstants.Write);
                                file.Append(CogImage);
                                file.Close();
                            }
                            else
                            {
                                CogImageFileBMP file = new CogImageFileBMP();
                                file.Open(m_savedToDiskPath, CogImageFileModeConstants.Write);
                                file.Append(CogImage);
                                file.Close();
                            }
                            // 写盘成功后置空图像引用，防止重复保存并释放内存
                            CogImage = null;
                            SavedToDisk = true;
                        }
                        catch (Exception ex)
                        {
                            //MessageManager.gOnly.Alarm(String.Format("Failed to save system image to {0}", m_savedToDiskPath), AlarmType.Unknown, ex);
                        }
                    }
                    // 系统图像：统一按 JPEG 格式落盘
                    else if (SystemImage != null)
                    {
                        try
                        {
                            SystemImage.Save(m_savedToDiskPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                            SystemImage = null;
                            SavedToDisk = true;
                        }
                        catch (Exception ex)
                        {
                            //MessageManager.gOnly.Alarm(String.Format("Failed to save system image to {0}", m_savedToDiskPath), AlarmType.Unknown, ex);
                        }
                    }
                    // 记录对象：用 CogSerializer 序列化保存
                    else if (RecordImage != null)
                    {
                        try
                        {
                            CogSerializer.SaveObjectToFile(RecordImage, m_savedToDiskPath);
                            //SystemImage.Save(m_savedToDiskPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                            RecordImage = null;
                            SavedToDisk = true;
                        }
                        catch (Exception ex)
                        {
                            //MessageManager.gOnly.Alarm(String.Format("Failed to save system image to {0}", m_savedToDiskPath), AlarmType.Unknown, ex);
                        }
                    }
                }
            }
        }

        #region Singleton Stuff
        // 全局单例实例
        private static ImageSaveQueue m_this = null;
        /// <summary>
        /// 初始化全局单例：首次调用时按指定容量创建队列（并启动后台存盘线程）。
        /// </summary>
        /// <param name="queueSize">队列容量，默认32；队列满时新图直接丢弃</param>
        public static void gInit(int queueSize = 32)
        {
            if (m_this == null)
                m_this = new ImageSaveQueue(queueSize);
            if (m_this == null)
                throw new Exception("Can not create ImageSaveQueue object.");
            m_this.Init();
        }
        /// <summary>
        /// 关闭全局单例：通知后台线程退出并等待其完成剩余存盘任务。
        /// </summary>
        public static void gShutDown()
        {
            if (m_this != null)
                m_this.Shutdown();
        }
        /// <summary>
        /// 获取全局唯一实例（须先调用 gInit，否则为 null）。
        /// </summary>
        public static ImageSaveQueue gOnly
        {
            get { return m_this; }
        }

        /// <summary>
        /// 析构函数（当前无清理逻辑，保留占位）。
        /// </summary>
        ~ImageSaveQueue()
        {
        }
        #endregion

        // 队列最大容量，超出则丢弃新图
        int m_imageQueueSize;

        // 后台线程退出标志
        bool m_exit;
        // 后台存盘线程
        Thread m_thread;
        // 信号量：生产者入队后 Release 唤醒消费者
        SemaphoreSlim m_semaphore;
        // 全队列共享锁（入队/出队/零件表操作）
        Object m_lock;
        // 待存图像队列
        Queue<ImageAndPath> m_imageQueue;
        // 近期入队的图像记录（供零件改名时同步重命名文件）
        List<ImageAndPath> m_activeImageRecords;

        // 上次清理时间（当前未使用）
        DateTime m_lastCleanup;
        // 零件ID自增计数器
        UInt64 m_partID = 1;
        // 零件ID → 零件名映射
        Dictionary<UInt64, String> m_partNames;
        // 活跃零件ID队列（超过上限时淘汰最老零件名）
        Queue<UInt64> m_activePartIDs;

        /// <summary>
        /// 构造存图队列：初始化容量、同步对象与零件表，并立即启动后台存盘线程。
        /// </summary>
        /// <param name="queueSize">队列容量</param>
        public ImageSaveQueue(int queueSize = 32)
        {
            m_imageQueueSize = queueSize;
            m_exit = false;
            m_thread = new Thread(new ThreadStart(workerThread));
            m_semaphore = new SemaphoreSlim(0);
            m_lock = new Object();
            m_imageQueue = new Queue<ImageAndPath>();
            m_activeImageRecords = new List<ImageAndPath>();
            m_lastCleanup = DateTime.MinValue;
            m_partID = 1;
            m_partNames = new Dictionary<ulong, string>();
            m_activePartIDs = new Queue<ulong>();
            m_thread.Start();
        }

        /// <summary>
        /// 附加初始化（当前为空实现）。
        /// </summary>
        public void Init()
        {
        }

        /// <summary>
        /// 新建一个匿名零件：分配自增ID并按"时间_UnknownPart_ID"命名，
        /// 同时把活跃零件数量限制在 100 个以内（淘汰最老记录）。
        /// </summary>
        /// <returns>新零件ID</returns>
        public UInt64 NewPart()
        {
            lock (m_lock)
            {
                UInt64 newPartID = m_partID;
                m_partID++;

                if (!m_partNames.ContainsKey(newPartID))
                    m_partNames.Add(newPartID, String.Format("{0}_UnknownPart_{1}", DateTime.Now.ToString("HHmmssfff"), newPartID));

                m_activePartIDs.Enqueue(newPartID);

                // Go through the dictionary and remove old items
                while (m_activePartIDs.Count > 100)
                {
                    UInt64 expiredID = m_activePartIDs.Dequeue();
                    if (m_partNames.ContainsKey(expiredID))
                        m_partNames.Remove(expiredID);
                }

                return newPartID;
            }
        }

        /// <summary>
        /// 新建一个具名零件：分配自增ID并按"时间_零件名"命名，同样限制活跃零件数量。
        /// </summary>
        /// <param name="partName">零件名（如车型/VIN）</param>
        /// <returns>新零件ID</returns>
        public UInt64 NewPart(String partName)
        {
            lock (m_lock)
            {
                UInt64 newPartID = m_partID;
                m_partID++;

                if (!m_partNames.ContainsKey(newPartID))
                    m_partNames.Add(newPartID, String.Format("{0}_{1}", DateTime.Now.ToString("HHmmssfff"), partName));

                m_activePartIDs.Enqueue(newPartID);

                // Go through the dictionary and remove old items
                while (m_activePartIDs.Count > 100)
                {
                    UInt64 expiredID = m_activePartIDs.Dequeue();
                    if (m_partNames.ContainsKey(expiredID))
                        m_partNames.Remove(expiredID);
                }

                return newPartID;
            }
        }

        /// <summary>
        /// 把多个旧零件ID合并为一个并以新名称命名：首个有效ID改名保留，其余ID合并进来。
        /// </summary>
        /// <param name="newPartName">新零件名</param>
        /// <param name="oldPartIDs">待合并的旧零件ID列表</param>
        public void SetPartName(String newPartName, params UInt64[] oldPartIDs)
        {
            if (oldPartIDs == null || oldPartIDs.Length < 1)
                return;

            lock (m_lock)
            {

                UInt64 newPartID = 0;

                // 遍历旧零件ID：首个有效ID作为保留ID改名，其余合并到它
                foreach (UInt64 partID in oldPartIDs)
                {
                    if (partID == 0)
                        continue;

                    if (newPartID == 0)
                    {
                        newPartID = partID;
                        SetPartName(newPartName, newPartID, partID);
                    }
                    else
                    {
                        MergeParts(newPartID, partID);
                    }

                }
            }
        }

        /// <summary>
        /// 按旧零件名查找并合并改名：遍历零件表找到名字匹配的ID，首个改名、其余合并。
        /// </summary>
        /// <param name="newPartName">新零件名</param>
        /// <param name="oldPartNames">旧零件名列表（不含时间戳前缀）</param>
        public void SetPartName(String newPartName, params String[] oldPartNames)
        {
            if (oldPartNames == null || oldPartNames.Length < 1)
                return;

            lock (m_lock)
            {
                UInt64 newPartID = 0;

                List<UInt64> keys = new List<UInt64>(m_partNames.Keys);

                // 遍历零件表，按旧零件名（去掉时间戳前缀后）查找匹配项
                foreach (UInt64 partID in keys)
                {

                    // The first 10 characters of the part name is just the timestamp and underscore.
                    // Ignore those.

                    if (partID == 0 || m_partNames[partID] == null || m_partNames[partID].Length < 11)
                        continue;

                    String oldPartName = m_partNames[partID].Substring(10);
                    if (oldPartNames.Contains(oldPartName))
                    {
                        if (newPartID == 0)
                        {
                            newPartID = partID;
                            SetPartName(newPartName, newPartID, partID);
                        }
                        else
                        {
                            MergeParts(newPartID, partID);
                        }

                    }
                }


            }
        }

        /// <summary>
        /// 为单个零件ID设置新名称（不合并其他零件）。
        /// </summary>
        /// <param name="newPartName">新零件名</param>
        /// <param name="partID">零件ID</param>
        public void SetPartName(String newPartName, UInt64 partID)
        {
            SetPartName(newPartName, partID, partID);
        }

        /// <summary>
        /// 把旧零件ID的图像全部归并到新零件ID：逐条重命名活跃记录并删除腾空的目录，
        /// 最后从零件表中移除旧ID。
        /// </summary>
        /// <param name="newPartID">保留的新零件ID</param>
        /// <param name="oldPartID">被合并的旧零件ID</param>
        private void MergeParts(UInt64 newPartID, UInt64 oldPartID)
        {
            // Go through all the parts in the queue and leftovers using this ID and rename them
            List<String> emptyDirectories = new List<string>();
            foreach (ImageAndPath iap in m_activeImageRecords)
            {
                if (iap.PartID == oldPartID)
                {
                    String emptyDirectory = iap.Rename(newPartID);
                    emptyDirectories.Add(emptyDirectory);
                }
            }

            // Delete any directories now empty
            foreach (String directory in emptyDirectories)
            {
                if (directory != null && Directory.Exists(directory) && IsDirectoryEmpty(directory))
                {
                    Directory.Delete(directory, true);
                }
            }

            m_partNames.Remove(oldPartID);

            // Go through the dictionary and remove old items
            while (m_activePartIDs.Count > 1000)
            {
                UInt64 expiredID = m_activePartIDs.Dequeue();
                if (m_partNames.ContainsKey(expiredID))
                    m_partNames.Remove(expiredID);
            }
        }

        /// <summary>
        /// 重命名核心实现：更新零件表中旧ID的名称，遍历活跃图像记录把旧ID的文件改名/移动到新ID，
        /// 清理空目录并淘汰过期零件记录。
        /// </summary>
        /// <param name="newPartName">新零件名（自动加时间戳前缀）</param>
        /// <param name="newPartID">保留的新零件ID</param>
        /// <param name="oldPartID">待改名的旧零件ID</param>
        private void SetPartName(String newPartName, UInt64 newPartID, UInt64 oldPartID)
        {
            // 新名称带时间戳前缀，保证零件目录名唯一
            newPartName = String.Format("{0}_{1}", DateTime.Now.ToString("HHmmssfff"), newPartName);
            lock (m_lock)
            {
                // Set the name in the dictionary
                if (!m_partNames.ContainsKey(oldPartID))
                {
                    m_partNames.Add(oldPartID, newPartName);
                }
                else
                {
                    m_partNames[oldPartID] = newPartName;
                }

                // Go through all the parts in the queue and leftovers using this ID and rename them
                List<String> emptyDirectories = new List<string>();
                foreach (ImageAndPath iap in m_activeImageRecords)
                {
                    if (iap.PartID == oldPartID)
                    {
                        String emptyDirectory = iap.Rename(newPartID);
                        emptyDirectories.Add(emptyDirectory);
                    }
                }

                // Delete any directories now empty
                foreach (String directory in emptyDirectories)
                {
                    if (directory != null && Directory.Exists(directory) && IsDirectoryEmpty(directory))
                    {
                        Directory.Delete(directory, true);
                    }
                }

                // Go through the dictionary and remove old items
                while (m_activePartIDs.Count > 1000)
                {
                    UInt64 expiredID = m_activePartIDs.Dequeue();
                    if (m_partNames.ContainsKey(expiredID))
                        m_partNames.Remove(expiredID);
                }
            }
        }

        /// <summary>
        /// 查询零件ID对应的零件名。
        /// </summary>
        /// <param name="partID">零件ID</param>
        /// <returns>零件名；不存在时返回 null</returns>
        public String GetPartName(UInt64 partID)
        {
            lock (m_lock)
            {
                if (m_partNames.ContainsKey(partID))
                    return m_partNames[partID];
                else
                    return null;
            }
        }

        /// <summary>
        /// 把 VisionPro 图像加入存盘队列；队列已满时直接丢弃并返回 false（不阻塞调用方）。
        /// </summary>
        /// <param name="savePath">保存路径（可含 {0} 占位符）</param>
        /// <param name="image">VisionPro 图像</param>
        /// <param name="partID">关联零件ID，0 表示不关联</param>
        /// <returns>入队成功返回 true；队列满被丢弃返回 false</returns>
        public bool AddToQueue(String savePath, ICogImage image, UInt64 partID = 0)
        {
            lock (m_lock)
            {
                // 队列未满才入队；满则丢帧返回 false，保证检测线程不被写盘拖慢
                if (m_imageQueue.Count < m_imageQueueSize)
                {
                    ImageAndPath newQueuedImage = new ImageAndPath(savePath, image, partID);
                    m_imageQueue.Enqueue(newQueuedImage);
                    // 关联零件的记录同时进活跃列表，便于后续按零件改名
                    if (partID > 0)
                        m_activeImageRecords.Add(newQueuedImage);
                    // 活跃记录最多保留 255 条
                    while (m_activeImageRecords.Count > 255)
                    {
                        m_activeImageRecords.RemoveAt(0);
                    }

                    // 释放信号量通知后台存盘线程有新任务
                    m_semaphore.Release();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 把 System.Drawing 图像加入存盘队列（统一按 JPEG 落盘）；队列满时丢弃返回 false。
        /// </summary>
        /// <param name="savePath">保存路径</param>
        /// <param name="image">系统图像</param>
        /// <param name="partID">关联零件ID，0 表示不关联</param>
        /// <returns>入队成功返回 true；队列满被丢弃返回 false</returns>
        public bool AddToQueue(String savePath, Image image, UInt64 partID = 0)
        {
            lock (m_lock)
            {
                if (m_imageQueue.Count < m_imageQueueSize)
                {
                    ImageAndPath newQueuedImage = new ImageAndPath(savePath, image, partID);
                    m_imageQueue.Enqueue(newQueuedImage);
                    if (partID > 0)
                        m_activeImageRecords.Add(newQueuedImage);
                    while (m_activeImageRecords.Count > 255)
                    {
                        m_activeImageRecords.RemoveAt(0);
                    }
                    m_semaphore.Release();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 把 VisionPro 记录对象加入存盘队列（按 CogSerializer 序列化落盘）；队列满时丢弃返回 false。
        /// </summary>
        /// <param name="savePath">保存路径</param>
        /// <param name="image">记录对象</param>
        /// <param name="partID">关联零件ID，0 表示不关联</param>
        /// <returns>入队成功返回 true；队列满被丢弃返回 false</returns>
        public bool AddToQueue(String savePath, ICogRecord image, UInt64 partID = 0)
        {
            lock (m_lock)
            {
                if (m_imageQueue.Count < m_imageQueueSize)
                {
                    ImageAndPath newQueuedImage = new ImageAndPath(savePath, image, partID);
                    m_imageQueue.Enqueue(newQueuedImage);
                    if (partID > 0)
                        m_activeImageRecords.Add(newQueuedImage);
                    while (m_activeImageRecords.Count > 255)
                    {
                        m_activeImageRecords.RemoveAt(0);
                    }
                    m_semaphore.Release();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 后台存盘线程主循环：限时等待信号量（200ms 超时用于响应退出标志），
        /// 出队一张图像执行写盘；单张写盘异常被吞掉，继续处理下一张。
        /// </summary>
        private void workerThread()
        {
            while (!m_exit)
            {
                // 限时等待：无任务时休眠，又能周期性醒来检查退出标志
                if (m_semaphore.Wait(200))
                {
                    ImageAndPath imageToSave = null;
                    lock (m_lock)
                    {
                        if (m_imageQueue.Count > 0)
                            imageToSave = m_imageQueue.Dequeue();
                    }

                    // 取到图像则执行写盘（异常吞掉，避免存盘线程退出）
                    if (imageToSave != null)
                    {
                        try
                        {
                            imageToSave.SaveToDisk();
                        }
                        catch (Exception ex)
                        {
                            //MessageManager.gOnly.Alarm(String.Format("Failed to save system image to {0}", imageToSave.SavePath), AlarmType.Unknown, ex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 判断目录是否为空（无任何文件与子目录）。
        /// </summary>
        /// <param name="path">目录路径</param>
        /// <returns>目录为空返回 true</returns>
        public bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        /// <summary>
        /// 递归删除指定目录下的所有空子目录（自底向上，删除失败静默忽略）。
        /// </summary>
        /// <param name="startLocation">起始目录</param>
        private void deleteEmptyFolders(string startLocation)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                deleteEmptyFolders(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    try
                    {
                        Directory.Delete(directory, false);
                    }
                    catch
                    {
                        //MessageManager.gOnly.Info(String.Format(LocalizationService.gOnly.GetLocalizedString("Could not delete folder {0} during image cleanup."), startLocation));
                    }
                }
            }
        }

        /// <summary>
        /// 停止后台存盘线程：置退出标志并 Join 等待线程收尾，保证已入队图像写完。
        /// </summary>
        public void Shutdown()
        {
            if (m_thread != null)
            {
                m_exit = true;
                m_thread.Join();
                m_thread = null;
            }
        }

    }
}
