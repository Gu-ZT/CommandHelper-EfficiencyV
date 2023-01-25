﻿using cbhk_environment.CustomControls;
using cbhk_environment.CustomControls.ColorPickers;
using cbhk_environment.CustomControls.TimeLines;
using cbhk_environment.GeneralTools;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace cbhk_environment.Generators.ArmorStandGenerator
{
    public class armor_stand_datacontext: ObservableObject
    {
        #region 所有3D视图对象
        private WpfCube Head_cube = new WpfCube(new Point3D(0.0, 12.5, 0.0), 1.0, 3.0, 1.0);

        private WpfCube Lhand_cube = new WpfCube(new Point3D(3.0, 10.0, 0.0), 1.0, 5.0, 1.0);

        private WpfCube Rhand_cube = new WpfCube(new Point3D(-3.0, 10.0, 0.0), 1.0, 5.0, 1.0);

        private WpfCube Lleg_cube = new WpfCube(new Point3D(1.0, 5.0, 0.0), 1.0, 5.0, 1.0);

        private WpfCube Rleg_cube = new WpfCube(new Point3D(-1.0, 5.0, 0.0), 1.0, 5.0, 1.0);

        private WpfCube Top_cube = new WpfCube(new Point3D(-3.0, 10.0, 0.0), 7.0, 1.0, 1.0);

        private WpfCube Bottom_cube = new WpfCube(new Point3D(-1.5, 5.0, 0.0), 4.0, 1.0, 1.0);

        private WpfCube Left_cube = new WpfCube(new Point3D(1.0, 5.0, 0.0), 1.0, -5.0, 1.0);

        private WpfCube Right_cube = new WpfCube(new Point3D(-1.0, 5.0, 0.0), 1.0, -5.0, 1.0);

        private WpfCube BasePlate_cube = new WpfCube(new Point3D(-4.5, 0.0, -4.5), 10.0, 1.0, 10.0);

        private WpfCube X_axis_cube = new WpfCube(new Point3D(0.0, 0.0, 0.0), 100.0, 0.1, 0.1);

        private WpfCube Y_axis_cube = new WpfCube(new Point3D(0.0, 0.0, 0.0), 0.1, -100.0, 0.1);

        private WpfCube Z_axis_cube = new WpfCube(new Point3D(0.0, 0.0, 0.0), 0.1, 0.1, 100.0);

        private GeometryModel3D Head_cubeModel;

        private GeometryModel3D Lhand_cubeModel;

        private GeometryModel3D Rhand_cubeModel;

        private GeometryModel3D Lleg_cubeModel;

        private GeometryModel3D Rleg_cubeModel;

        private GeometryModel3D Top_cubeModel;

        private GeometryModel3D Bottom_cubeModel;

        private GeometryModel3D Left_cubeModel;

        private GeometryModel3D Right_cubeModel;

        private GeometryModel3D BasePlate_cubeModel;

        private GeometryModel3D X_axis_cubeModel;

        private GeometryModel3D Y_axis_cubeModel;

        private GeometryModel3D Z_axis_cubeModel;

        private Model3DGroup groupScene = new Model3DGroup();

        private double pos_x = -15.0;

        private double pos_y = -10.0;

        private double pos_z = -15.0;

        private Vector3D zero_pos = new Vector3D(0.0, 0.0, 0.0);

        private double motion_x;

        private double motion_y;

        private double motion_z;

        private double Camera_pos_x = 15.0;

        private bool Mousedown = false;

        private double mouse_pos_x = 0.0;

        private double mouse_pos_y = 0.0;

        private double move_x;

        private double move_y;

        private double mouse_move_x;

        private double mouse_move_y;

        private Viewport3D AS_model;
        #endregion

        /// <summary>
        /// 检查是否加载完毕
        /// </summary>
        private bool WindowLoading = true;

        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowLoading = false;
        }

        #region 生成器版本
        //版本切换锁,防止属性之间无休止更新
        private bool version_switch_lock = false;
        private bool version1_8;
        public bool Version1_8
        {
            get { return version1_8; }
            set
            {
                version1_8 = value;
                if (!version_switch_lock)
                {
                    version_switch_lock = !version_switch_lock;
                    UseMainHandPermission = Version1_8 ? Visibility.Visible : Visibility.Collapsed;
                    Version1_9 = !Version1_8;
                    version_switch_lock = false;
                }
                OnPropertyChanged();
            }
        }
        private bool version1_9 = true;
        public bool Version1_9
        {
            get { return version1_9; }
            set
            {
                version1_9 = value;
                if(!version_switch_lock)
                {
                    version_switch_lock = !version_switch_lock;
                    HaveOffHandPermission = Version1_9 ? Visibility.Visible : Visibility.Collapsed;
                    Version1_8 = !Version1_9;
                    version_switch_lock = false;
                }
                OnPropertyChanged();
            }
        }
        #endregion

        #region 是否拥有副手权限
        //版本切换锁,防止属性之间无休止更新
        private bool permission_switch_lock = false;
        private Visibility haveOffHandPermission = Visibility.Visible;
        public Visibility HaveOffHandPermission
        {
            get { return haveOffHandPermission; }
            set
            {
                haveOffHandPermission = value;
                if(!permission_switch_lock)
                {
                    permission_switch_lock = !permission_switch_lock;
                    UseMainHandPermission = Version1_9 ? Visibility.Collapsed : Visibility.Visible;
                    permission_switch_lock = false;
                }
                OnPropertyChanged();
            }
        }
        #endregion

        #region 是否与主手共用权限
        private Visibility useMainHandPermission = Visibility.Collapsed;
        public Visibility UseMainHandPermission
        {
            get { return useMainHandPermission; }
            set
            {
                useMainHandPermission = value;
                if(!permission_switch_lock)
                {
                    permission_switch_lock = !permission_switch_lock;
                    HaveOffHandPermission = Version1_8 ? Visibility.Collapsed : Visibility.Visible;
                    permission_switch_lock = false;
                }
                OnPropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// 颜色应用载体
        /// </summary>
        Control ColorSourceControl = new Control();

        /// <summary>
        /// 在mc中已存在的颜色结构
        /// </summary>
        Dictionary<string,string> KnownColorsInMc = new Dictionary<string, string>();

        List<int> KnownColorDifferenceSet = new List<int>();
        /// <summary>
        /// 盔甲架全身材质
        /// </summary>
        private BitmapImage armor_stand_material = null;

        /// <summary>
        /// 底座材质
        /// </summary>
        private BitmapImage base_plate_material = null;

        /// <summary>
        /// 盔甲架全身材质文件路径
        /// </summary>
        private string armor_stand_material_path = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\ArmorStand\\images\\oak_planks.png";

        /// <summary>
        /// 底座材质文件路径
        /// </summary>
        private string base_plate_material_path = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\ArmorStand\\images\\smooth_stone.png";

        /// <summary>
        /// 当前视图模型
        /// </summary>
        private Viewport3D CurrentViewModel = null;

        //as的所有NBT项
        List<string> as_nbts = new List<string> { };

        /// <summary>
        /// 生成盔甲架数据
        /// </summary>
        public RelayCommand RunCommand { get; set; }

        /// <summary>
        /// 返回主页
        /// </summary>
        public RelayCommand<Window> ReturnCommand { get; set; }

        /// <summary>
        /// 重置所有动作
        /// </summary>
        public RelayCommand ResetAllPoseCommand { get; set; }

        /// <summary>
        /// 重置头部动作
        /// </summary>
        public RelayCommand ResetHeadPoseCommand { get; set; }

        /// <summary>
        /// 重置身体动作
        /// </summary>
        public RelayCommand ResetBodyPoseCommand { get; set; }

        /// <summary>
        /// 重置左臂动作
        /// </summary>
        public RelayCommand ResetLArmPoseCommand { get; set; }

        /// <summary>
        /// 重置右臂动作
        /// </summary>
        public RelayCommand ResetRArmPoseCommand { get; set; }

        /// <summary>
        /// 重置左腿动作
        /// </summary>
        public RelayCommand ResetLLegPoseCommand { get; set; }

        /// <summary>
        /// 重置右腿动作
        /// </summary>
        public RelayCommand ResetRLegPoseCommand { get; set; }

        /// <summary>
        /// 播放动画
        /// </summary>
        public RelayCommand<IconTextButtons> PlayAnimation { get; set; }

        /// <summary>
        /// 正在播放
        /// </summary>
        private bool IsPlaying = false;

        /// <summary>
        /// 动画时间轴
        /// </summary>
        TimeLine tl = new TimeLine(380, 200)
        {
            Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0))
        };

        #region 是否叠加生成结果
        private bool over_lying;
        public bool OverLying
        {
            get { return over_lying; }
            set
            {
                over_lying = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region as名称
        private string custom_name;
        public string CustomName
        {
            get 
            {
                string result = "";
                result = custom_name != "" ? "CustomName:'{\"text\":\"" + custom_name + "\"" + (CustomNameColor != null ? ",\"color\":\"" + CustomNameColor.ToString().Remove(1, 2) + "\"" : "") + "}'," : "";
                return result;
            }
            set
            {
                custom_name = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region as名称可见性
        private bool custom_name_visible;
        public bool CustomNameVisible
        {
            get { return custom_name_visible; }
            set
            {
                custom_name_visible = value;
                OnPropertyChanged();
            }
        }
        private string CustomNameVisibleString
        {
            get { return CustomNameVisible ?"CustomNameVisible:true,":""; }
        }
        #endregion

        #region as的tag
        private string tag;
        public string Tag
        {
            get 
            {
                if (tag != null && tag.Length > 0)
                {
                    string[] tag_string = tag.Split(',');
                    string result = "Tags:[";
                    for (int i = 0; i < tag_string.Length; i++)
                    {
                        result += "\"" + tag_string[i] + "\",";
                    }
                    result = result.TrimEnd(',') + "],";
                    return result;
                }
                else
                    return "";
            }
            set
            {
                tag = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 名称颜色
        private SolidColorBrush custom_name_color;
        public SolidColorBrush CustomNameColor
        {
            get { return custom_name_color; }
            set
            {
                custom_name_color = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region BoolNBTs
        private string BoolNBTs
        {
            get
            {
                string result = "";
                foreach (string item in BoolTypeNBT)
                {
                    result += item+":true,";
                }
                return result;
            }
        }
        #endregion

        #region DisabledValue
        private string DisabledValue
        {
            get
            {
                string result = CannotPlaceSum + CannotTakeOrReplceSum + CannotPlaceOrReplaceSum + "";
                return int.Parse(result)>0 ? "DisabledSlots:"+result+"," : "" ;
            }
        }
        #endregion

        #region PoseString
        private string PoseString
        {
            get
            {
                string result = "";
                bool have_value = HeadXValue || HeadYValue || HeadZValue || BodyXValue || BodyYValue || BodyZValue || LArmXValue || LArmYValue || LArmZValue || RArmXValue || RArmYValue || RArmZValue || LLegXValue || LLegYValue || LLegZValue || RLegXValue || RLegYValue || RLegZValue;

                result = (have_value) ?(HeadXValue || HeadYValue || HeadZValue ? "Head:[" + HeadX + "f," + HeadY + "f," + HeadZ + "f]," : "") + (BodyXValue || BodyYValue || BodyZValue ? "Body:[" + BodyX + "f," + BodyY + "f," + BodyZ + "f]," : "")
                      + (LArmXValue || LArmYValue || LArmZValue ? "LeftArm:[" + LArmX + "f," + LArmY + "f," + LArmZ + "f]," : "")
                      + (RArmXValue || RArmYValue || RArmZValue ? "RightArm:[" + RArmX + "f," + RArmY + "f," + RArmZ + "f]," : "")
                      + (LLegXValue || LLegYValue || LLegZValue ? "LeftLeg:[" + LLegX + "f," + LLegY + "f," + LLegZ + "f]," : "")
                      + (RLegXValue || RLegYValue || RLegZValue ? "RightLeg:[" + RLegX + "f," + RLegY + "f," + RLegZ + "f]}," : "") : "";

                result = result.ToString() !="" ? "Pose:{" + result.TrimEnd(',') + "}" : "";
                return result.ToString();
            }
        }
        #endregion

        #region 重置动作的按钮前景颜色对象
        //灰色
        static SolidColorBrush gray_brush = new SolidColorBrush(Color.FromRgb(100, 100, 100));
        //白色
        static SolidColorBrush white_brush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        #endregion

        #region 是否可以重置所有动作
        private bool can_reset_all_pose;
        public bool CanResetAllPose
        {
            get { return can_reset_all_pose; }
            set
            {
                can_reset_all_pose = value;
                ResetAllPoseButtonForeground = CanResetAllPose ? white_brush:gray_brush;
                OnPropertyChanged();
            }
        }
        #endregion
        #region 重置所有动作的按钮前景
        private Brush reset_all_pose_button_foreground = gray_brush;
        public Brush ResetAllPoseButtonForeground
        {
            get { return reset_all_pose_button_foreground; }
            set
            {
                reset_all_pose_button_foreground = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 是否可以重置头部动作
        private bool can_reset_head_pose;
        public bool CanResetHeadPose
        {
            get { return can_reset_head_pose; }
            set
            {
                can_reset_head_pose = value;
                ResetHeadPoseButtonForeground = CanResetHeadPose ? white_brush : gray_brush;
                OnPropertyChanged();
            }
        }
        #endregion
        #region 重置头部动作的按钮前景
        private Brush reset_head_pose_button_foreground = gray_brush;
        public Brush ResetHeadPoseButtonForeground
        {
            get { return reset_head_pose_button_foreground; }
            set
            {
                reset_head_pose_button_foreground = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 是否可以重置身体动作
        private bool can_reset_body_pose;
        public bool CanResetBodyPose
        {
            get { return can_reset_body_pose; }
            set
            {
                can_reset_body_pose = value;
                ResetBodyPoseButtonForeground = CanResetBodyPose ? white_brush : gray_brush;
                OnPropertyChanged();
            }
        }
        #endregion
        #region 重置身体动作的按钮前景
        private Brush reset_body_pose_button_foreground = gray_brush;
        public Brush ResetBodyPoseButtonForeground
        {
            get { return reset_body_pose_button_foreground; }
            set
            {
                reset_body_pose_button_foreground = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 是否可以重置左臂动作
        private bool can_reset_larm_pose;
        public bool CanResetLArmPose
        {
            get { return can_reset_larm_pose; }
            set
            {
                can_reset_larm_pose = value;
                ResetLArmPoseButtonForeground = CanResetLArmPose ? white_brush : gray_brush;
                OnPropertyChanged();
            }
        }
        #endregion
        #region 重置左臂动作的按钮前景
        private Brush reset_larm_pose_button_foreground = gray_brush;
        public Brush ResetLArmPoseButtonForeground
        {
            get { return reset_larm_pose_button_foreground; }
            set
            {
                reset_larm_pose_button_foreground = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 是否可以重置右臂动作
        private bool can_reset_rarm_pose;
        public bool CanResetRArmPose
        {
            get { return can_reset_rarm_pose; }
            set
            {
                can_reset_rarm_pose = value;
                ResetRArmPoseButtonForeground = CanResetRArmPose ? white_brush : gray_brush;
                OnPropertyChanged();
            }
        }
        #endregion
        #region 重置右臂动作的按钮前景
        private Brush reset_rarm_pose_button_foreground = gray_brush;
        public Brush ResetRArmPoseButtonForeground
        {
            get { return reset_rarm_pose_button_foreground; }
            set
            {
                reset_rarm_pose_button_foreground = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 是否可以重置左腿动作
        private bool can_reset_lleg_pose;
        public bool CanResetLLegPose
        {
            get { return can_reset_lleg_pose; }
            set
            {
                can_reset_lleg_pose = value;
                ResetLLegPoseButtonForeground = CanResetLLegPose ? white_brush : gray_brush;
                OnPropertyChanged();
            }
        }
        #endregion
        #region 重置左臂动作的按钮前景
        private Brush reset_lleg_pose_button_foreground = gray_brush;
        public Brush ResetLLegPoseButtonForeground
        {
            get { return reset_lleg_pose_button_foreground; }
            set
            {
                reset_lleg_pose_button_foreground = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 是否可以重置右腿动作
        private bool can_reset_rleg_pose;
        public bool CanResetRLegPose
        {
            get { return can_reset_rleg_pose; }
            set
            {
                can_reset_rleg_pose = value;
                ResetRLegPoseButtonForeground = CanResetRLegPose ? white_brush : gray_brush;
                OnPropertyChanged();
            }
        }
        #endregion
        #region 重置右臂动作的按钮前景
        private Brush reset_rleg_pose_button_foreground = gray_brush;
        public Brush ResetRLegPoseButtonForeground
        {
            get { return reset_rleg_pose_button_foreground; }
            set
            {
                reset_rleg_pose_button_foreground = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 头部XYZ
        private bool HeadXValue;
        private float head_x = 0f;
        public float HeadX
        {
            get { return head_x; }
            set
            {
                head_x = value;
                HeadXValue = HeadX != 0f?true:false;
                TurnModel(new Point3D(0.5, 9.5, 0.5), Head_cubeModel, 0.02, true, HeadX, HeadY, HeadZ);
                OnPropertyChanged();
            }
        }

        private bool HeadYValue;
        private float head_y = 0f;
        public float HeadY
        {
            get { return head_y; }
            set
            {
                head_y = value;
                HeadYValue = HeadY != 0f ?true:false;
                TurnModel(new Point3D(0.5, 9.5, 0.5), Head_cubeModel, 0.02, true, HeadX, HeadY, HeadZ);
                OnPropertyChanged();
            }
        }

        private bool HeadZValue;
        private float head_z = 0f;
        public float HeadZ
        {
            get { return head_z; }
            set
            {
                head_z = value;
                HeadZValue = HeadZ != 0f ?true:false;
                TurnModel(new Point3D(0.5, 9.5, 0.5), Head_cubeModel, 0.02, true, HeadX, HeadY, HeadZ);
                OnPropertyChanged();
            }
        }
        #endregion
        #region 身体XYZ
        private bool BodyXValue;
        private float body_x = 0f;
        public float BodyX
        {
            get { return body_x; }
            set
            {
                body_x = value;
                BodyXValue = BodyX != 0f ?true:false;
                TurnModel(new Point3D(0.5, 9.5, 0.5), Top_cubeModel, 0.02, true, BodyX, BodyY, BodyZ);
                TurnModel(new Point3D(0.5, 9.5, 0.5), Bottom_cubeModel, 0.02, true, BodyX, BodyY, BodyZ);
                TurnModel(new Point3D(0.5, 9.5, 0.5), Left_cubeModel, 0.02, true, BodyX, BodyY, BodyZ);
                TurnModel(new Point3D(0.5, 9.5, 0.5), Right_cubeModel, 0.02, true, BodyX, BodyY, BodyZ);
                OnPropertyChanged();
            }
        }

        private bool BodyYValue;
        private float body_y = 0f;
        public float BodyY
        {
            get { return body_y; }
            set
            {
                body_y = value;
                BodyYValue = BodyY != 0f ?true:false;
                TurnModel(new Point3D(0.5, 9.5, 0.5), Top_cubeModel, 0.02, true, BodyX, BodyY, BodyZ);
                TurnModel(new Point3D(0.5, 9.5, 0.5), Bottom_cubeModel, 0.02, true, BodyX, BodyY, BodyZ);
                TurnModel(new Point3D(0.5, 9.5, 0.5), Left_cubeModel, 0.02, true, BodyX, BodyY, BodyZ);
                TurnModel(new Point3D(0.5, 9.5, 0.5), Right_cubeModel, 0.02, true, BodyX, BodyY, BodyZ);
                OnPropertyChanged();
            }
        }

        private bool BodyZValue;
        private float body_z = 0f;
        public float BodyZ
        {
            get { return body_z; }
            set
            {
                body_z = value;
                BodyZValue = BodyZ != 0f ?true:false;
                TurnModel(new Point3D(0.5, 9.5, 0.5), Top_cubeModel, 0.02, true, BodyX, BodyY, BodyZ);
                TurnModel(new Point3D(0.5, 9.5, 0.5), Bottom_cubeModel, 0.02, true, BodyX, BodyY, BodyZ);
                TurnModel(new Point3D(0.5, 9.5, 0.5), Left_cubeModel, 0.02, true, BodyX, BodyY, BodyZ);
                TurnModel(new Point3D(0.5, 9.5, 0.5), Right_cubeModel, 0.02, true, BodyX, BodyY, BodyZ);
                OnPropertyChanged();
            }
        }
        #endregion
        #region 左臂XYZ
        private bool LArmXValue;
        private float larm_x = 0f;
        public float LArmX
        {
            get { return larm_x; }
            set
            {
                larm_x = value;
                LArmXValue = LArmX != 0f ?true:false;
                TurnModel(new Point3D(3.5, 9.5, 0.5), Lhand_cubeModel, 0.02, true, LArmX, LArmY, LArmZ);
                OnPropertyChanged();
            }
        }

        private bool LArmYValue;
        private float larm_y = 0f;
        public float LArmY
        {
            get { return larm_y; }
            set
            {
                larm_y = value;
                LArmYValue = LArmY != 0f ?true:false;
                TurnModel(new Point3D(3.5, 9.5, 0.5), Lhand_cubeModel, 0.02, true, LArmX, LArmY, LArmZ);
                OnPropertyChanged();
            }
        }

        private bool LArmZValue;
        private float larm_z = 0f;
        public float LArmZ
        {
            get { return larm_z; }
            set
            {
                larm_z = value;
                LArmZValue = LArmZ != 0f ?true:false;
                TurnModel(new Point3D(3.5, 9.5, 0.5), Lhand_cubeModel, 0.02, true, LArmX, LArmY, LArmZ);
                OnPropertyChanged();
            }
        }
        #endregion
        #region 右臂XYZ
        private bool RArmXValue;
        private float rarm_x = 0f;
        public float RArmX
        {
            get { return rarm_x; }
            set
            {
                rarm_x = value;
                RArmXValue = RArmX != 0f ?true:false;
                TurnModel(new Point3D(-2.5, 9.5, 0.5), Rhand_cubeModel, 0.02, true, RArmX, RArmY, RArmZ);
                OnPropertyChanged();
            }
        }

        private bool RArmYValue;
        private float rarm_y = 0f;
        public float RArmY
        {
            get { return rarm_y; }
            set
            {
                rarm_y = value;
                RArmYValue = RArmY != 0f ?true:false;
                TurnModel(new Point3D(-2.5, 9.5, 0.5), Rhand_cubeModel, 0.02, true, RArmX, RArmY, RArmZ);
                OnPropertyChanged();
            }
        }

        private bool RArmZValue;
        private float rarm_z = 0f;
        public float RArmZ
        {
            get { return rarm_z; }
            set
            {
                rarm_z = value;
                RArmZValue = RArmZ != 0f ?true:false;
                TurnModel(new Point3D(-2.5, 9.5, 0.5), Rhand_cubeModel, 0.02, true, RArmX, RArmY, RArmZ);
                OnPropertyChanged();
            }
        }
        #endregion
        #region 左腿XYZ
        private bool LLegXValue;
        private float lleg_x = 0f;
        public float LLegX
        {
            get { return lleg_x; }
            set
            {
                lleg_x = value;
                LLegXValue = LLegX != 0f ?true:false;
                TurnModel(new Point3D(1.5, 4.5, 0.5), Lleg_cubeModel, 0.02, true, LLegX, LLegY, LLegZ);
                OnPropertyChanged();
            }
        }

        private bool LLegYValue;
        private float lleg_y = 0f;
        public float LLegY
        {
            get { return lleg_y; }
            set
            {
                lleg_y = value;
                LLegYValue = LLegY != 0f ?true:false;
                TurnModel(new Point3D(1.5, 4.5, 0.5), Lleg_cubeModel, 0.02, true, LLegX, LLegY, LLegZ);
                OnPropertyChanged();
            }
        }

        private bool LLegZValue;
        private float lleg_z = 0f;
        public float LLegZ
        {
            get { return lleg_z; }
            set
            {
                lleg_z = value;
                LLegZValue = LLegZ != 0f ?true:false;
                TurnModel(new Point3D(1.5, 4.5, 0.5), Lleg_cubeModel, 0.02, true, LLegX, LLegY, LLegZ);
                OnPropertyChanged();
            }
        }
        #endregion
        #region 右腿XYZ
        private bool RLegXValue;
        private float rleg_x = 0f;
        public float RLegX
        {
            get { return rleg_x; }
            set
            {
                rleg_x = value;
                RLegXValue = RLegX != 0f ?true:false;
                TurnModel(new Point3D(-0.5, 4.5, 0.5), Rleg_cubeModel, 0.02, true, RLegX, RLegY, RLegZ);
                OnPropertyChanged();
            }
        }

        private bool RLegYValue;
        private float rleg_y = 0f;
        public float RLegY
        {
            get { return rleg_y; }
            set
            {
                rleg_y = value;
                RLegYValue = RLegY != 0f ?true:false;
                TurnModel(new Point3D(-0.5, 4.5, 0.5), Rleg_cubeModel, 0.02, true, RLegX, RLegY, RLegZ);
                OnPropertyChanged();
            }
        }

        private bool RLegZValue;
        private float rleg_z = 0f;
        public float RLegZ
        {
            get { return rleg_z; }
            set
            {
                rleg_z = value;
                RLegZValue = RLegZ != 0f ?true:false;
                TurnModel(new Point3D(-0.5, 4.5, 0.5), Rleg_cubeModel, 0.02, true, RLegX, RLegY, RLegZ);
                OnPropertyChanged();
            }
        }
        #endregion

        #region 装备

        #region Head
        private string head_item;
        public string HeadItem
        {
            get { return head_item; }
            set
            {
                head_item = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Body
        private string body_item;
        public string BodyItem
        {
            get { return body_item; }
            set
            {
                body_item = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region LeftHand
        private string left_hand_item;
        public string LeftHandItem
        {
            get { return body_item; }
            set
            {
                left_hand_item = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region RightHand
        private string right_hand_item;
        public string RightHandItem
        {
            get { return body_item; }
            set
            {
                right_hand_item = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Legs
        private string leg_item;
        public string LegsItem
        {
            get { return body_item; }
            set
            {
                leg_item = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Boots
        private string boots_item;
        public string BootsItem
        {
            get { return body_item; }
            set
            {
                boots_item = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #endregion


        #region 记录上一次的鼠标位置
        private Point last_cursor_position;
        private Point LastCursorPosition
        {
            get { return last_cursor_position; }
            set
            {
                last_cursor_position = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 三轴合一
        public RelayCommand<Button> HeadThreeAxisCommand { get; set; }
        public RelayCommand<Button> BodyThreeAxisCommand { get; set; }
        public RelayCommand<Button> LArmThreeAxisCommand { get; set; }
        public RelayCommand<Button> RArmThreeAxisCommand { get; set; }
        public RelayCommand<Button> LLegThreeAxisCommand { get; set; }
        public RelayCommand<Button> RLegThreeAxisCommand { get; set; }
        #endregion

        /// <summary>
        /// 开始三轴合一
        /// </summary>
        private bool UsingThreeAxis = false;

        #region 三轴合一数据更新载体
        TextBlock XAxis = new TextBlock();
        TextBlock YAxis = new TextBlock();
        TextBlock ZAxis = new TextBlock();
        
        //用于自增和自减
        float XAxisValue = 0f;
        float YAxisValue = 0f;
        float ZAxisValue = 0f;
        #endregion

        /// <summary>
        /// 超出三轴合一按钮范围
        /// </summary>
        private bool OutOfThreeAxis = false;

        /// <summary>
        /// 当前三轴合一按钮位置
        /// </summary>
        private Point CurrentButtonCenter;

        // 布尔型NBT链表
        List<string> BoolTypeNBT = new List<string> { };

        //禁止移除或改变总值
        private int CannotTakeOrReplceSum;
        //禁止添加或改变总值
        private int CannotPlaceOrReplaceSum;
        //禁止添加总值
        private int CannotPlaceSum;

        #region 禁止移除或改变头部、身体、手部、腿部、脚部装备
        private bool cannotTakeOrReplaceHead;
        public bool CannotTakeOrReplaceHead
        {
            get => cannotTakeOrReplaceHead;
            set
            {
                cannotTakeOrReplaceHead = value;
                if (!WindowLoading)
                    CannotTakeOrReplceSum += CannotTakeOrReplaceHead ? 4096 : -4096;
                OnPropertyChanged();
            }
        }

        private bool cannotTakeOrReplaceBody;
        public bool CannotTakeOrReplaceBody
        {
            get { return cannotTakeOrReplaceBody; }
            set
            {
                cannotTakeOrReplaceBody = value;
                if (!WindowLoading)
                    CannotTakeOrReplceSum += CannotTakeOrReplaceBody ? 2048 : -2048;
                OnPropertyChanged();
            }
        }

        private bool cannotTakeOrReplaceMainhand;
        public bool CannotTakeOrReplaceMainHand
        {
            get { return cannotTakeOrReplaceMainhand; }
            set
            {
                cannotTakeOrReplaceMainhand = value;
                if (!WindowLoading)
                    CannotTakeOrReplceSum += CannotTakeOrReplaceMainHand ? 256 : -256;
                OnPropertyChanged();
            }
        }

        private bool cannotTakeOrReplaceOffHand;
        public bool CannotTakeOrReplaceOffHand
        {
            get { return cannotTakeOrReplaceOffHand; }
            set
            {
                cannotTakeOrReplaceOffHand = value;
                if (!WindowLoading)
                    CannotTakeOrReplceSum += HaveOffHandPermission == Visibility.Visible && CannotTakeOrReplaceOffHand ? 8192 : -8192;
                OnPropertyChanged();
            }
        }

        private bool cannotTakeOrReplaceLegs;
        public bool CannotTakeOrReplaceLegs
        {
            get { return cannotTakeOrReplaceLegs; }
            set
            {
                cannotTakeOrReplaceLegs = value;
                if (!WindowLoading)
                    CannotTakeOrReplceSum += CannotTakeOrReplaceLegs ? 1024 : -1024;
                OnPropertyChanged();
            }
        }

        private bool cannotTakeOrReplaceBoots;
        public bool CannotTakeOrReplaceBoots
        {
            get { return cannotTakeOrReplaceBoots; }
            set
            {
                cannotTakeOrReplaceBoots = value;
                if (!WindowLoading)
                    CannotTakeOrReplceSum += CannotTakeOrReplaceBoots ? 512 : -512;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 禁止添加或改变头部、身体、手部、腿部、脚部装备
        private bool cannotPlaceOrReplacehead;
        public bool CannotPlaceOrReplaceHead
        {
            get { return cannotPlaceOrReplacehead; }
            set
            {
                cannotPlaceOrReplacehead = value;
                if (!WindowLoading)
                    CannotPlaceOrReplaceSum += CannotPlaceOrReplaceHead ? 16 : -16;
                OnPropertyChanged();
            }
        }

        private bool cannotPlaceOrReplacebody;
        public bool CannotPlaceOrReplaceBody
        {
            get { return cannotPlaceOrReplacebody; }
            set
            {
                cannotPlaceOrReplacebody = value;
                if (!WindowLoading)
                    CannotPlaceOrReplaceSum += CannotPlaceOrReplaceBody ? 8 : -8;
                OnPropertyChanged();
            }
        }

        private bool cannotPlaceOrReplaceMainHand;
        public bool CannotPlaceOrReplaceMainHand
        {
            get { return cannotPlaceOrReplaceMainHand; }
            set
            {
                cannotPlaceOrReplaceMainHand = value;
                if (!WindowLoading)
                    CannotPlaceOrReplaceSum += CannotPlaceOrReplaceMainHand ? 1 : -1;
                OnPropertyChanged();
            }
        }

        private bool cannotPlaceOrReplaceOffHand;
        public bool CannotPlaceOrReplaceOffHand
        {
            get { return cannotPlaceOrReplaceOffHand; }
            set
            {
                cannotPlaceOrReplaceOffHand = value;
                if (!WindowLoading)
                    CannotPlaceOrReplaceSum += HaveOffHandPermission == Visibility.Visible && CannotPlaceOrReplaceOffHand ? 32 : -32;
                OnPropertyChanged();
            }
        }

        private bool cannotPlaceOrReplaceLegs;
        public bool CannotPlaceOrReplaceLegs
        {
            get { return cannotPlaceOrReplaceLegs; }
            set
            {
                cannotPlaceOrReplaceLegs = value;
                if (!WindowLoading)
                    CannotPlaceOrReplaceSum += CannotPlaceOrReplaceLegs ? 4 : -4;
                OnPropertyChanged();
            }
        }

        private bool cannotPlaceOrReplaceBoots;
        public bool CannotPlaceOrReplaceBoots
        {
            get { return cannotPlaceOrReplaceBoots; }
            set
            {
                cannotPlaceOrReplaceBoots = value;
                if (!WindowLoading)
                    CannotPlaceOrReplaceSum += CannotPlaceOrReplaceBoots ? 2 : -2;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 禁止添加头部、身体、手部、腿部、脚部装备
        private bool cannotPlaceHead;
        public bool CannotPlaceHead
        {
            get => cannotPlaceHead;
            set
            {
                cannotPlaceHead = value;
                if (!WindowLoading)
                    CannotPlaceSum += CannotPlaceHead ? 1048576 : -1048576;
                OnPropertyChanged();
            }
        }

        private bool cannotPlaceBody;
        public bool CannotPlaceBody
        {
            get { return cannotPlaceBody; }
            set
            {
                cannotPlaceBody = value;
                if (!WindowLoading)
                    CannotPlaceSum += CannotPlaceBody ? 524288 : -524288;
                OnPropertyChanged();
            }
        }

        private bool cannotPlaceMainHand;
        public bool CannotPlaceMainHand
        {
            get { return cannotPlaceMainHand; }
            set
            {
                cannotPlaceMainHand = value;
                if (!WindowLoading)
                    CannotPlaceSum += CannotPlaceMainHand ? 65536 : -65536;
                OnPropertyChanged();
            }
        }

        private bool cannotPlaceOffHand;
        public bool CannotPlaceOffHand
        {
            get { return cannotPlaceOffHand; }
            set
            {
                cannotPlaceOffHand = value;
                if (!WindowLoading)
                    CannotPlaceSum += HaveOffHandPermission == Visibility.Visible && CannotPlaceOffHand ? 2097152 : -2097152;
                OnPropertyChanged();
            }
        }

        private bool cannotPlaceLegs;
        public bool CannotPlaceLegs
        {
            get { return cannotPlaceLegs; }
            set
            {
                cannotPlaceLegs = value;
                if (!WindowLoading)
                    CannotPlaceSum += cannotPlaceLegs ? 262144 : -262144;
                OnPropertyChanged();
            }
        }

        private bool cannotPlaceBoots;
        public bool CannotPlaceBoots
        {
            get { return cannotPlaceBoots; }
            set
            {
                cannotPlaceBoots = value;
                if (!WindowLoading)
                    CannotPlaceSum += CannotPlaceBoots ? 131072 : -131072;
                OnPropertyChanged();
            }
        }
        #endregion

        //布尔NBT集合
        StackPanel NBTList = null;

        public armor_stand_datacontext()
        {

            #region 绑定指令
            RunCommand = new RelayCommand(run_command);
            ReturnCommand = new RelayCommand<Window>(return_command);
            ResetAllPoseCommand = new RelayCommand(reset_all_pose_command);
            ResetHeadPoseCommand = new RelayCommand(reset_head_pose_command);
            ResetBodyPoseCommand = new RelayCommand(reset_body_pose_command);
            ResetLArmPoseCommand = new RelayCommand(reset_larm_pose_command);
            ResetRArmPoseCommand = new RelayCommand(reset_rarm_pose_command);
            ResetLLegPoseCommand = new RelayCommand(reset_lleg_pose_command);
            ResetRLegPoseCommand = new RelayCommand(reset_rleg_pose_command);
            PlayAnimation = new RelayCommand<IconTextButtons>(play_animation);
            HeadThreeAxisCommand = new RelayCommand<Button>(head_three_axis_command);
            BodyThreeAxisCommand = new RelayCommand<Button>(body_three_axis_command);
            LArmThreeAxisCommand = new RelayCommand<Button>(larm_three_axis_command);
            RArmThreeAxisCommand = new RelayCommand<Button>(rarm_three_axis_command);
            LLegThreeAxisCommand = new RelayCommand<Button>(lleg_three_axis_command);
            RLegThreeAxisCommand = new RelayCommand<Button>(rleg_three_axis_command);
            #endregion

            #region 连接三个轴的上下文
            XAxis.DataContext = this;
            YAxis.DataContext = this;
            ZAxis.DataContext = this;
            #endregion

            #region 加载mc中已存在的颜色结构列表
            if(File.Exists(AppDomain.CurrentDomain.BaseDirectory+ "resources\\configs\\struct_colors.ini"))
            {
                string[] ColorMap = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\struct_colors.ini");
                for (int i = 0; i < ColorMap.Length; i++)
                {
                    string[] group = ColorMap[i].Split('=');
                    KnownColorsInMc.Add(group[0], group[1]);
                }
            }
            #endregion
        }

        /// <summary>
        /// 生成as
        /// </summary>
        private void run_command()
        {
            string[] result = new string[1] { "" };

            #region 拼接指令数据

            #region HaveAniamtion
            #endregion

            #region Equipments
            #endregion

            #region result
            result[0] = CustomName + BoolNBTs + DisabledValue + CustomNameVisibleString + Tag + PoseString;
            result[0] = result[0].TrimEnd(',');
            result[0] = "/summon armor_stand ~ ~ ~" + (result[0] != "" ? " {" + result[0] +"}":"");
            #endregion

            #endregion

            #region 唤出生成结果窗体
            GenerateResultDisplayer.Displayer displayer = GenerateResultDisplayer.Displayer.GetContentDisplayer();
            displayer.GeneratorResult(OverLying,result, new string[] { custom_name },
                                    new string[] { AppDomain.CurrentDomain.BaseDirectory+ "\\resources\\configs\\ArmorStand\\images\\icon.png" },
                                    new Vector3D() { X = 25, Y = 45});
            displayer.Show();
            #endregion
        }

        /// <summary>
        /// 返回主页
        /// </summary>
        private void return_command(Window win)
        {
            ArmorStand.cbhk.Topmost = true;
            ArmorStand.cbhk.WindowState = WindowState.Normal;
            ArmorStand.cbhk.Show();
            ArmorStand.cbhk.Topmost = false;
            ArmorStand.cbhk.ShowInTaskbar = true;
            win.Close();
        }

        public void ColorPickersPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ColorPickers cp = sender as ColorPickers;
            CustomNameColor = cp.SelectColor;
        }

        /// <summary>
        /// 重置所有动作
        /// </summary>
        private void reset_all_pose_command()
        {
            if(CanResetAllPose)
            {
                HeadX = HeadY = HeadZ = BodyX = BodyY = BodyZ = LArmX = LArmY = LArmZ = RArmX = RArmY = RArmZ = LLegX = LLegY = LLegZ = RLegX = RLegY = RLegZ = 0f;
                CanResetAllPose = false;
            }
        }

        /// <summary>
        /// 重置头部动作
        /// </summary>
        private void reset_head_pose_command()
        {
            if(CanResetHeadPose)
            {
                HeadX = HeadY = HeadZ = 0f;
                CanResetHeadPose = false;
            }
        }

        /// <summary>
        /// 重置身体动作
        /// </summary>
        private void reset_body_pose_command()
        {
            if(CanResetBodyPose)
            {
                BodyX = BodyY = BodyZ = 0f;
                CanResetBodyPose = false;
            }
        }

        /// <summary>
        /// 重置左臂动作
        /// </summary>
        private void reset_larm_pose_command()
        {
            if(CanResetLArmPose)
            {
                LArmX = LArmY = LArmZ = 0f;
                CanResetLArmPose = false;
            }
        }

        /// <summary>
        /// 重置右臂动作
        /// </summary>
        private void reset_rarm_pose_command()
        {
            if(CanResetRArmPose)
            {
                RArmX = RArmY = RArmZ = 0f;
                CanResetRArmPose = false;
            }
        }

        /// <summary>
        /// 重置左腿动作
        /// </summary>
        private void reset_lleg_pose_command()
        {
            if(CanResetLLegPose)
            {
                LLegX = LLegY = LLegZ = 0f;
                CanResetLLegPose = false;
            }
        }

        /// <summary>
        /// 重置右腿动作
        /// </summary>
        private void reset_rleg_pose_command()
        {
            if(CanResetRLegPose)
            {
                RLegX = RLegY = RLegZ = 0f;
                CanResetRLegPose = false;
            }
        }

        /// <summary>
        /// 头部三轴合一
        /// </summary>
        private void head_three_axis_command(Button btn)
        {
            UsingThreeAxis = !UsingThreeAxis;

            if(UsingThreeAxis)
            {
                btn.Cursor = UsingThreeAxis ? Cursors.None : Cursors.Arrow;

                Binding x_binder = new Binding()
                {
                    Path = new PropertyPath("HeadX"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                Binding y_binder = new Binding()
                {
                    Path = new PropertyPath("HeadY"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                Binding z_binder = new Binding()
                {
                    Path = new PropertyPath("HeadZ"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                XAxis.SetBinding(FrameworkElement.TagProperty, x_binder);
                YAxis.SetBinding(FrameworkElement.TagProperty, y_binder);
                ZAxis.SetBinding(FrameworkElement.TagProperty, z_binder);
            }
        }

        /// <summary>
        /// 身体三轴合一
        /// </summary>
        private void body_three_axis_command(Button btn)
        {
            UsingThreeAxis = !UsingThreeAxis;

            if (UsingThreeAxis)
            {
                btn.Cursor = UsingThreeAxis ? Cursors.None : Cursors.Arrow;

                Binding x_binder = new Binding()
                {
                    Path = new PropertyPath("BodyX"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                Binding y_binder = new Binding()
                {
                    Path = new PropertyPath("BodyY"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                Binding z_binder = new Binding()
                {
                    Path = new PropertyPath("BodyZ"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                XAxis.SetBinding(FrameworkElement.TagProperty, x_binder);
                YAxis.SetBinding(FrameworkElement.TagProperty, y_binder);
                ZAxis.SetBinding(FrameworkElement.TagProperty, z_binder);
            }
        }

        /// <summary>
        /// 左臂三轴合一
        /// </summary>
        private void larm_three_axis_command(Button btn)
        {
            UsingThreeAxis = !UsingThreeAxis;

            if (UsingThreeAxis)
            {
                btn.Cursor = UsingThreeAxis ? Cursors.None : Cursors.Arrow;

                Binding x_binder = new Binding()
                {
                    Path = new PropertyPath("LArmX"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                Binding y_binder = new Binding()
                {
                    Path = new PropertyPath("LArmY"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                Binding z_binder = new Binding()
                {
                    Path = new PropertyPath("LArmZ"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                XAxis.SetBinding(FrameworkElement.TagProperty, x_binder);
                YAxis.SetBinding(FrameworkElement.TagProperty, y_binder);
                ZAxis.SetBinding(FrameworkElement.TagProperty, z_binder);
            }
        }

        /// <summary>
        /// 右臂三轴合一
        /// </summary>
        private void rarm_three_axis_command(Button btn)
        {
            UsingThreeAxis = !UsingThreeAxis;
            
            if (UsingThreeAxis)
            {
                btn.Cursor = UsingThreeAxis ? Cursors.None : Cursors.Arrow;

                Binding x_binder = new Binding()
                {
                    Path = new PropertyPath("RArmX"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                Binding y_binder = new Binding()
                {
                    Path = new PropertyPath("RArmY"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                Binding z_binder = new Binding()
                {
                    Path = new PropertyPath("RArmZ"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                XAxis.SetBinding(FrameworkElement.TagProperty, x_binder);
                YAxis.SetBinding(FrameworkElement.TagProperty, y_binder);
                ZAxis.SetBinding(FrameworkElement.TagProperty, z_binder);
            }
        }

        /// <summary>
        /// 左腿三轴合一
        /// </summary>
        private void lleg_three_axis_command(Button btn)
        {
            UsingThreeAxis = !UsingThreeAxis;
            
            if (UsingThreeAxis)
            {
                btn.Cursor = UsingThreeAxis ? Cursors.None : Cursors.Arrow;

                Binding x_binder = new Binding()
                {
                    Path = new PropertyPath("LLegX"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                Binding y_binder = new Binding()
                {
                    Path = new PropertyPath("LLegY"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                Binding z_binder = new Binding()
                {
                    Path = new PropertyPath("LLegZ"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                XAxis.SetBinding(FrameworkElement.TagProperty, x_binder);
                YAxis.SetBinding(FrameworkElement.TagProperty, y_binder);
                ZAxis.SetBinding(FrameworkElement.TagProperty, z_binder);
            }
        }

        /// <summary>
        /// 右腿三轴合一
        /// </summary>
        private void rleg_three_axis_command(Button btn)
        {
            UsingThreeAxis = !UsingThreeAxis;

            if (UsingThreeAxis)
            {
                btn.Cursor = UsingThreeAxis ? Cursors.None : Cursors.Arrow;
                
                Binding x_binder = new Binding()
                {
                    Path = new PropertyPath("RLegX"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                Binding y_binder = new Binding()
                {
                    Path = new PropertyPath("RLegY"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                Binding z_binder = new Binding()
                {
                    Path = new PropertyPath("RLegZ"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                XAxis.SetBinding(FrameworkElement.TagProperty, x_binder);
                YAxis.SetBinding(FrameworkElement.TagProperty, y_binder);
                ZAxis.SetBinding(FrameworkElement.TagProperty, z_binder);
            }
        }

        /// <summary>   
        /// 设置鼠标的坐标   
        /// </summary>   
        /// <param name="x">横坐标</param>   
        /// <param name="y">纵坐标</param>   
        [DllImport("User32")]
        public extern static void SetCursorPos(int x, int y);


        public void ThreeAxisMouseMove(object sender, MouseEventArgs e)
        {
            if (UsingThreeAxis)
            {
                Button btn = sender as Button;
                CurrentButtonCenter = Mouse.GetPosition(btn);
                LastCursorPosition = new Point(LastCursorPosition.X - CurrentButtonCenter.X, LastCursorPosition.Y - CurrentButtonCenter.Y);

                if(LastCursorPosition.X > 0)
                XAxisValue += -1;
                else
                    if(LastCursorPosition.X < 0)
                    XAxisValue += 1;
                else
                    if(LastCursorPosition.Y > 0)
                    ZAxisValue += -1;
                else
                if (LastCursorPosition.Y < 0)
                    ZAxisValue += 1;

                XAxisValue = XAxisValue > 180 ? 180 : (XAxisValue < -180)? -180 : XAxisValue;
                ZAxisValue = ZAxisValue > 180 ? 180 : (ZAxisValue < -180) ? -180 : ZAxisValue;

                XAxis.Tag = XAxisValue;
                ZAxis.Tag = ZAxisValue;

                //值复位
                if (!OutOfThreeAxis)
                LastCursorPosition = CurrentButtonCenter;
            }
            OutOfThreeAxis = false;
        }

        public void ThreeAxisMouseLeave(object sender, MouseEventArgs e)
        {
            if (UsingThreeAxis)
            {
                OutOfThreeAxis = true;
                Button btn = sender as Button;
                CurrentButtonCenter = btn.PointToScreen(new Point(0, 0));
                CurrentButtonCenter.X += btn.Width / 2;
                CurrentButtonCenter.Y += btn.Height / 2;
                int point_x = (int)CurrentButtonCenter.X;
                int point_y = (int)CurrentButtonCenter.Y;
                SetCursorPos(point_x, point_y);
            }
        }

        public void ThreeAxisMouseWheel(object sender, MouseWheelEventArgs e)
        {
            YAxisValue += e.Delta > 0 ? 1f : (-1f);
            YAxisValue = YAxisValue > 180 ?180:(YAxisValue<-180)?-180:YAxisValue;
            YAxis.Tag = YAxisValue;
        }

        private void play_animation(IconTextButtons btn)
        {
            IsPlaying = !IsPlaying;
            string pause_data = "F1 M191.397656 128.194684l191.080943 0 0 768.472256-191.080943 0 0-768.472256Z M575.874261 128.194684l192.901405 0 0 768.472256-192.901405 0 0-768.472256Z";
            string playing_data = "M870.2 466.333333l-618.666667-373.28a53.333333 53.333333 0 0 0-80.866666 45.666667v746.56a53.206667 53.206667 0 0 0 80.886666 45.666667l618.666667-373.28a53.333333 53.333333 0 0 0 0-91.333334z";
            var converter = TypeDescriptor.GetConverter(typeof(Geometry));
            btn.IconData = IsPlaying? converter.ConvertFrom(pause_data) as Geometry: converter.ConvertFrom(playing_data) as Geometry;
            btn.ContentData = IsPlaying?"暂停":"播放";
        }

        /// <summary>
        /// 载入基础NBT列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void NBTCheckboxListLoaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\ArmorStand\\data\\base_nbt.ini"))
                as_nbts = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\ArmorStand\\data\\base_nbt.ini", Encoding.UTF8).ToList();
            NBTList = sender as StackPanel;

            if (as_nbts.Count > 0)
            {
                foreach (string item in as_nbts)
                {
                    TextCheckBoxs textCheckBox = new TextCheckBoxs
                    {
                        Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                        Margin = new Thickness(10,0,0,0),
                        HeaderText = item,
                        HeaderHeight = 20,
                        HeaderWidth = 20,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Style = (NBTList.Children[0] as TextCheckBoxs).Style
                    };
                    NBTList.Children.Add(textCheckBox);
                    textCheckBox.Click += BoolNBTBoxClick;
                    switch (item)
                    {
                        case "ShowArms":
                            {
                                textCheckBox.Click += ShowArmsInModel;
                                break;
                            }
                        case "NoBasePlate":
                            {
                                textCheckBox.Click += NoBasePlateInModel;
                                break;
                            }
                    }
                }
                NBTList.Children.RemoveAt(0);
            }
        }

        /// <summary>
        /// 载入动画时间轴
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TimeLineBoxLoaded(object sender, RoutedEventArgs e)
        {
            Viewbox viewbox = sender as Viewbox;
            tl.Setup(0, 50, 10, 150);
            viewbox.Child = tl;
            tl.AddElement(5);
        }

        /// <summary>
        /// 添加以勾选的布尔型NBT
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoolNBTBoxClick(object sender, RoutedEventArgs e)
        {
            TextCheckBoxs current = sender as TextCheckBoxs;
            if (current.IsChecked.Value)
                BoolTypeNBT.Add(current.HeaderText);
            else
                BoolTypeNBT.Remove(current.HeaderText);
        }

        #region 处理3D模型

        /// <summary>
        /// 载入盔甲架模型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ModelViewLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is Viewport3D MainWindow)
            {
                if (File.Exists(armor_stand_material_path))
                    armor_stand_material = BitmapImageConverter.ToBitmapImage(System.Drawing.Image.FromFile(armor_stand_material_path) as Bitmap);
                if (File.Exists(armor_stand_material_path))
                    base_plate_material = BitmapImageConverter.ToBitmapImage(System.Drawing.Image.FromFile(base_plate_material_path) as Bitmap);

                Head_cubeModel = Head_cube.CreateModel(Color.FromRgb(176, 144, 86));
                Lhand_cubeModel = Lhand_cube.CreateModel(Color.FromRgb(176, 144, 86));
                Rhand_cubeModel = Rhand_cube.CreateModel(Color.FromRgb(176, 144, 86));
                Lleg_cubeModel = Lleg_cube.CreateModel(Color.FromRgb(176, 144, 86));
                Rleg_cubeModel = Rleg_cube.CreateModel(Color.FromRgb(176, 144, 86));
                Top_cubeModel = Top_cube.CreateModel(Color.FromRgb(176, 144, 86));
                Bottom_cubeModel = Bottom_cube.CreateModel(Color.FromRgb(176, 144, 86));
                Left_cubeModel = Left_cube.CreateModel(Color.FromRgb(176, 144, 86));
                Right_cubeModel = Right_cube.CreateModel(Color.FromRgb(176, 144, 86));
                BasePlate_cubeModel = BasePlate_cube.CreateModel(Color.FromRgb(176, 176, 176));
                X_axis_cubeModel = X_axis_cube.CreateModel(Color.FromRgb(255, 0, 0));
                Y_axis_cubeModel = Y_axis_cube.CreateModel(Color.FromRgb(0, 255, 0));
                Z_axis_cubeModel = Z_axis_cube.CreateModel(Color.FromRgb(0, 0, 255));
                groupScene.Children.Add(Head_cubeModel);
                groupScene.Children.Add(Lleg_cubeModel);
                groupScene.Children.Add(Rleg_cubeModel);
                groupScene.Children.Add(Top_cubeModel);
                groupScene.Children.Add(Bottom_cubeModel);
                groupScene.Children.Add(Left_cubeModel);
                groupScene.Children.Add(Right_cubeModel);
                groupScene.Children.Add(BasePlate_cubeModel);
                groupScene.Children.Add(X_axis_cubeModel);
                groupScene.Children.Add(Y_axis_cubeModel);
                groupScene.Children.Add(Z_axis_cubeModel);
                groupScene.Children.Add(PositionLight(new Point3D(-5.0, 10.0, 0.0)));
                groupScene.Children.Add(new AmbientLight(Colors.LightGray));
                ModelVisual3D visual = new ModelVisual3D
                {
                    Content = groupScene
                };
                MainWindow.Children.Add(visual);
                MainWindow.Camera = Camera(15.0, 10.0, 15.0, 90, new Vector3D(-15.0, -10.0, -15.0));
                AS_model = MainWindow;

                CurrentViewModel = sender as Viewport3D;
            }
        }

        /// <summary>
        /// 处理模型视图中鼠标按下
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ModelViewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (CurrentViewModel == null)
                CurrentViewModel = sender as Viewport3D;
            Mousedown = true;
            mouse_pos_x = e.GetPosition(CurrentViewModel).X;
            mouse_pos_y = e.GetPosition(CurrentViewModel).Y;
        }

        /// <summary>
        /// 处理模型视图中鼠标抬起
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ModelViewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (CurrentViewModel == null)
                CurrentViewModel = sender as Viewport3D;
            Mousedown = false;
            move_x = mouse_move_x;
            move_y = mouse_move_y;
        }

        /// <summary>
        /// 处理模型视图中鼠标移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ModelViewMouseMove(object sender, MouseEventArgs e)
        {
            if (CurrentViewModel == null)
                CurrentViewModel = sender as Viewport3D;
            if (Mousedown)
            {
                mouse_move_x = 0.01 * (e.GetPosition(CurrentViewModel).X - mouse_pos_x) + move_x;
                mouse_move_y = 0.01 * (e.GetPosition(CurrentViewModel).Y - mouse_pos_y) + move_y;
                pos_x = Camera_pos_x * Math.Cos(mouse_move_x) * Math.Cos(mouse_move_y);
                pos_y = Camera_pos_x * Math.Sin(mouse_move_y) * -1.0;
                pos_z = Camera_pos_x * Math.Sin(mouse_move_x) * Math.Cos(mouse_move_y);
                motion_x = zero_pos.X - pos_x;
                motion_y = zero_pos.Y - pos_y;
                motion_z = zero_pos.Z - pos_z;
                CurrentViewModel.Camera = Camera(pos_x, pos_y, pos_z, 90, new Vector3D(motion_x, motion_y, motion_z));
            }
        }

        /// <summary>
        /// 处理模型视图中鼠标滚轮的滚动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ModelViewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double x_move = zero_pos.X - pos_x;
            double y_move = zero_pos.Y - pos_y;
            double z_move = zero_pos.Z - pos_z;
            if (e.Delta > 0)
            {
                pos_x += x_move * 0.1;
                pos_y += y_move * 0.1;
                pos_z += z_move * 0.1;
                CurrentViewModel.Camera = Camera(pos_x, pos_y, pos_z, 60, new Vector3D(zero_pos.X - pos_x, zero_pos.Y - pos_y, zero_pos.Z - pos_z));
                Camera_pos_x = Math.Sqrt(Math.Pow(pos_x, 2.0) + Math.Pow(pos_y, 2.0) + Math.Pow(pos_z, 2.0));
            }
            else if (e.Delta < 0)
            {
                pos_x -= x_move * 0.1;
                pos_y -= y_move * 0.1;
                pos_z -= z_move * 0.1;
                CurrentViewModel.Camera = Camera(pos_x, pos_y, pos_z, 60, new Vector3D(zero_pos.X - pos_x, zero_pos.Y - pos_y, zero_pos.Z - pos_z));
                Camera_pos_x = Math.Sqrt(Math.Pow(pos_x, 2.0) + Math.Pow(pos_y, 2.0) + Math.Pow(pos_z, 2.0));
            }
        }

        /// <summary>
        /// 显示盔甲架的手臂
        /// </summary>
        public void ShowArmsInModel(object sender, RoutedEventArgs e)
        {
            if ((sender as TextCheckBoxs).IsChecked.Value)
            {
                groupScene.Children.Add(Lhand_cubeModel);
                groupScene.Children.Add(Rhand_cubeModel);
            }
            else
            {
                groupScene.Children.Remove(Lhand_cubeModel);
                groupScene.Children.Remove(Rhand_cubeModel);
            }
        }

        /// <summary>
        /// 不显示盔甲架的底座
        /// </summary>
        public void NoBasePlateInModel(object sender, RoutedEventArgs e)
        {
            if ((sender as TextCheckBoxs).IsChecked.Value)
                groupScene.Children.Remove(BasePlate_cubeModel);
            else
                groupScene.Children.Add(BasePlate_cubeModel);
        }

        public void TurnModel(Point3D center, GeometryModel3D model, double seconds, bool axis, float X, float Y, float Z)
        {
            Vector3D vector5 = new Vector3D(0.0, 1.0, 0.0);
            Vector3D vector4 = new Vector3D(1.0, 0.0, 0.0);
            Vector3D vector3 = new Vector3D(0.0, 0.0, 1.0);
            AxisAngleRotation3D rotation5 = new AxisAngleRotation3D(vector5, 0.0);
            AxisAngleRotation3D rotation4 = new AxisAngleRotation3D(vector4, 0.0);
            AxisAngleRotation3D rotation3 = new AxisAngleRotation3D(vector3, 0.0);
            RotateTransform3D rotateTransform5 = new RotateTransform3D(rotation5, center);
            RotateTransform3D rotateTransform4 = new RotateTransform3D(rotation4, center);
            RotateTransform3D rotateTransform3 = new RotateTransform3D(rotation3, center);
            Transform3DGroup transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(rotateTransform5);
            transformGroup.Children.Add(rotateTransform4);
            transformGroup.Children.Add(rotateTransform3);
            model.Transform = transformGroup;
            if (axis)
            {
                DoubleAnimation doubleAnimation5 = new DoubleAnimation(double.Parse(Y.ToString()), double.Parse(Y.ToString()), DurationTS(seconds))
                {
                    BeginTime = DurationTS(0.0)
                };
                rotation5.BeginAnimation(AxisAngleRotation3D.AngleProperty, doubleAnimation5);
                DoubleAnimation doubleAnimation4 = new DoubleAnimation(double.Parse(X.ToString()), double.Parse(X.ToString()), DurationTS(seconds))
                {
                    BeginTime = DurationTS(0.0)
                };
                rotation4.BeginAnimation(AxisAngleRotation3D.AngleProperty, doubleAnimation4);
                DoubleAnimation doubleAnimation3 = new DoubleAnimation(double.Parse(Z.ToString()), double.Parse(Z.ToString()), DurationTS(seconds))
                {
                    BeginTime = DurationTS(0.0)
                };
                rotation3.BeginAnimation(AxisAngleRotation3D.AngleProperty, doubleAnimation3);
            }
        }

        private int DurationM(double seconds)
        {
            return (int)(seconds * 1000.0);
        }

        public TimeSpan DurationTS(double seconds)
        {
            return new TimeSpan(0, 0, 0, 0, DurationM(seconds));
        }

        public PerspectiveCamera Camera(double x, double y, double z, int fieldofView, Vector3D xyz_rotation)
        {
            return new PerspectiveCamera
            {
                Position = new Point3D(x, y, z),
                FieldOfView = fieldofView,
                LookDirection = xyz_rotation
            };
        }

        public DirectionalLight PositionLight(Point3D position)
        {
            return new DirectionalLight
            {
                Color = Colors.Gray,
                Direction = new Point3D(0.0, 0.0, 0.0) - position
            };
        }
        #endregion
    }

    public static class Rotate3DModel
    {
        public static Vector3D Rotate(this Vector3D vector3D, double x, double y, double z)
        {
            Matrix3D rotateX = new Matrix3D(1.0, 0.0, 0.0, 0.0, 0.0, Math.Cos(x), Math.Sin(x), 0.0, 0.0, 0.0 - Math.Sin(x), Math.Cos(x), 0.0, 0.0, 0.0, 0.0, 1.0);
            Matrix3D rotateY = new Matrix3D(Math.Cos(y), 0.0, 0.0 - Math.Sin(y), 0.0, 0.0, 1.0, 0.0, 0.0, Math.Sin(y), 0.0, Math.Cos(y), 0.0, 0.0, 0.0, 0.0, 1.0);
            Matrix3D rotateZ = new Matrix3D(Math.Cos(z), Math.Sin(z), 0.0, 0.0, 0.0 - Math.Sin(z), Math.Cos(z), 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0);
            return vector3D * rotateX * rotateY * rotateZ;
        }
    }

    internal class WpfTriangle
    {
        private Point3D p1;

        private Point3D p2;

        private Point3D p3;

        public WpfTriangle(Point3D P1, Point3D P2, Point3D P3)
        {
            p1 = P1;
            p2 = P2;
            p3 = P3;
        }

        public static void AddTriangleToMesh(Point3D p0, Point3D p1, Point3D p2, MeshGeometry3D mesh)
        {
            AddTriangleToMesh(p0, p1, p2, mesh, combine_vertices: false);
        }

        public static void AddPointCombined(Point3D point, MeshGeometry3D mesh, Vector3D normal)
        {
            bool found = false;
            int i = 0;
            foreach (Point3D position in mesh.Positions)
            {
                if (position.Equals(point))
                {
                    found = true;
                    mesh.TriangleIndices.Add(i);
                    mesh.Positions.Add(point);
                    mesh.Normals.Add(normal);
                    break;
                }
                i++;
            }
            if (!found)
            {
                mesh.Positions.Add(point);
                mesh.TriangleIndices.Add(mesh.TriangleIndices.Count);
                mesh.Normals.Add(normal);
            }
        }

        public static void AddTriangleToMesh(Point3D p0, Point3D p1, Point3D p2, MeshGeometry3D mesh, bool combine_vertices)
        {
            Vector3D normal = CalculateNormal(p0, p1, p2);
            if (combine_vertices)
            {
                AddPointCombined(p0, mesh, normal);
                AddPointCombined(p1, mesh, normal);
                AddPointCombined(p2, mesh, normal);
                return;
            }
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.TriangleIndices.Add(mesh.TriangleIndices.Count);
            mesh.TriangleIndices.Add(mesh.TriangleIndices.Count);
            mesh.TriangleIndices.Add(mesh.TriangleIndices.Count);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
        }

        public GeometryModel3D CreateTriangleModel(Color color)
        {
            return CreateTriangleModel(p1, p2, p3, color);
        }

        public static GeometryModel3D CreateTriangleModel(Point3D P0, Point3D P1, Point3D P2, Color color)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            AddTriangleToMesh(P0, P1, P2, mesh);
            Material material = new DiffuseMaterial(new SolidColorBrush(color));
            return new GeometryModel3D(mesh, material);
        }

        public static Vector3D CalculateNormal(Point3D P0, Point3D P1, Point3D P2)
        {
            Vector3D v2 = new Vector3D(P1.X - P0.X, P1.Y - P0.Y, P1.Z - P0.Z);
            Vector3D v = new Vector3D(P2.X - P1.X, P2.Y - P1.Y, P2.Z - P1.Z);
            return Vector3D.CrossProduct(v2, v);
        }
    }

    public class WpfRectangle
    {
        private Point3D p0;

        private Point3D p1;

        private Point3D p2;

        private Point3D p3;

        public WpfRectangle(Point3D P0, Point3D P1, Point3D P2, Point3D P3)
        {
            p0 = P0;
            p1 = P1;
            p2 = P2;
            p3 = P3;
        }

        public WpfRectangle(Point3D P0, double w, double h, double d)
        {
            p0 = P0;
            if (w != 0.0 && h != 0.0)
            {
                p1 = new Point3D(p0.X + w, p0.Y, p0.Z);
                p2 = new Point3D(p0.X + w, p0.Y - h, p0.Z);
                p3 = new Point3D(p0.X, p0.Y - h, p0.Z);
            }
            else if (w != 0.0 && d != 0.0)
            {
                p1 = new Point3D(p0.X, p0.Y, p0.Z + d);
                p2 = new Point3D(p0.X + w, p0.Y, p0.Z + d);
                p3 = new Point3D(p0.X + w, p0.Y, p0.Z);
            }
            else if (h != 0.0 && d != 0.0)
            {
                p1 = new Point3D(p0.X, p0.Y, p0.Z + d);
                p2 = new Point3D(p0.X, p0.Y - h, p0.Z + d);
                p3 = new Point3D(p0.X, p0.Y - h, p0.Z);
            }
        }

        public void AddToMesh(MeshGeometry3D mesh)
        {
            WpfTriangle.AddTriangleToMesh(p0, p1, p2, mesh);
            WpfTriangle.AddTriangleToMesh(p2, p3, p0, mesh);
        }

        public static void AddRectangleToMesh(Point3D p0, Point3D p1, Point3D p2, Point3D p3, MeshGeometry3D mesh)
        {
            WpfTriangle.AddTriangleToMesh(p0, p1, p2, mesh);
            WpfTriangle.AddTriangleToMesh(p2, p3, p0, mesh);
        }

        public static GeometryModel3D CreateRectangleModel(Point3D p0, Point3D p1, Point3D p2, Point3D p3)
        {
            return CreateRectangleModel(p0, p1, p2, p3, texture: false);
        }

        public static GeometryModel3D CreateRectangleModel(Point3D p0, Point3D p1, Point3D p2, Point3D p3, bool texture)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            AddRectangleToMesh(p0, p1, p2, p3, mesh);
            Material material = new DiffuseMaterial(new SolidColorBrush(Colors.White));
            return new GeometryModel3D(mesh, material);
        }
    }

    public class WpfCube
    {
        private Point3D origin;

        private double width;

        private double height;

        private double depth;

        public Point3D CenterBottom()
        {
            return new Point3D(origin.X + width / 2.0, origin.Y + height, origin.Z + depth / 2.0);
        }

        public Point3D Center()
        {
            return new Point3D(origin.X + width / 2.0, origin.Y - height / 2.0, origin.Z + depth / 2.0);
        }

        public Point3D CenterTop()
        {
            return new Point3D(origin.X + width / 2.0, origin.Y, origin.Z + depth / 2.0);
        }

        public WpfCube(Point3D P0, double w, double h, double d)
        {
            width = w;
            height = h;
            depth = d;
            origin = P0;
        }

        public WpfCube(WpfCube cube)
        {
            width = cube.width;
            height = cube.height;
            depth = cube.depth;
            origin = new Point3D(cube.origin.X, cube.origin.Y, cube.origin.Z);
        }

        public WpfRectangle Front()
        {
            return new WpfRectangle(origin, width, height, 0.0);
        }

        public WpfRectangle Back()
        {
            return new WpfRectangle(new Point3D(origin.X + width, origin.Y, origin.Z + depth), 0.0 - width, height, 0.0);
        }

        public WpfRectangle Left()
        {
            return new WpfRectangle(new Point3D(origin.X, origin.Y, origin.Z + depth), 0.0, height, 0.0 - depth);
        }

        public WpfRectangle Right()
        {
            return new WpfRectangle(new Point3D(origin.X + width, origin.Y, origin.Z), 0.0, height, depth);
        }

        public WpfRectangle Top()
        {
            return new WpfRectangle(origin, width, 0.0, depth);
        }

        public WpfRectangle Bottom()
        {
            return new WpfRectangle(new Point3D(origin.X + width, origin.Y - height, origin.Z), 0.0 - width, 0.0, depth);
        }

        public static void AddCubeToMesh(Point3D p0, double w, double h, double d, MeshGeometry3D mesh)
        {
            WpfCube cube = new WpfCube(p0, w, h, d);
            WpfRectangle front = cube.Front();
            WpfRectangle back = cube.Back();
            WpfRectangle right = cube.Right();
            WpfRectangle left = cube.Left();
            WpfRectangle top = cube.Top();
            WpfRectangle bottom = cube.Bottom();
            front.AddToMesh(mesh);
            back.AddToMesh(mesh);
            right.AddToMesh(mesh);
            left.AddToMesh(mesh);
            top.AddToMesh(mesh);
            bottom.AddToMesh(mesh);
        }

        public GeometryModel3D CreateModel(BitmapImage image_material)
        {
            return CreateCubeModel(origin, width, height, depth, image_material);
        }

        public static GeometryModel3D CreateCubeModel(Point3D p0, double w, double h, double d, BitmapImage image_material)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();

            #region 3DMaterialInformation
            //mesh.Positions.Add(new Point3D(20, 0, 0));
            //mesh.Positions.Add(new Point3D(20, 0, 3)); 
            //mesh.Positions.Add(new Point3D(20, 20, 3)); 
            //mesh.Positions.Add(new Point3D(20, 20, 0)); 
            //mesh.Positions.Add(new Point3D(0, 20, 3)); 
            //mesh.Positions.Add(new Point3D(0, 0, 0)); 
            //mesh.Positions.Add(new Point3D(0, 20, 0)); 
            //mesh.Positions.Add(new Point3D(0, 0, 3));

            //mesh.TriangleIndices.Add(0);
            //mesh.TriangleIndices.Add(1);
            //mesh.TriangleIndices.Add(2);
            //mesh.TriangleIndices.Add(3);
            //mesh.TriangleIndices.Add(0);
            //mesh.TriangleIndices.Add(2);
            //mesh.TriangleIndices.Add(4);
            //mesh.TriangleIndices.Add(5);
            //mesh.TriangleIndices.Add(6);
            //mesh.TriangleIndices.Add(4);
            //mesh.TriangleIndices.Add(7);
            //mesh.TriangleIndices.Add(5);
            //mesh.TriangleIndices.Add(0);
            //mesh.TriangleIndices.Add(5);
            //mesh.TriangleIndices.Add(1);
            //mesh.TriangleIndices.Add(1);
            //mesh.TriangleIndices.Add(5);
            //mesh.TriangleIndices.Add(7);
            //mesh.TriangleIndices.Add(6);
            //mesh.TriangleIndices.Add(3);
            //mesh.TriangleIndices.Add(4);
            //mesh.TriangleIndices.Add(4);
            //mesh.TriangleIndices.Add(3);
            //mesh.TriangleIndices.Add(2);

            //mesh.TextureCoordinates.Add(new Point(0, 0));
            //mesh.TextureCoordinates.Add(new Point(0, 1));
            //mesh.TextureCoordinates.Add(new Point(1, 0));
            //mesh.TextureCoordinates.Add(new Point(1, 1));
            //mesh.TextureCoordinates.Add(new Point(0, 0));
            //mesh.TextureCoordinates.Add(new Point(0, 1));
            //mesh.TextureCoordinates.Add(new Point(1, 0));
            //mesh.TextureCoordinates.Add(new Point(1, 1));
            //mesh.TextureCoordinates.Add(new Point(0, 1));
            //mesh.TextureCoordinates.Add(new Point(0, 1));
            //mesh.TextureCoordinates.Add(new Point(1, 0));
            //mesh.TextureCoordinates.Add(new Point(1, 1));

            //mesh.TextureCoordinates.Add(new Point(0, 0));
            //mesh.TextureCoordinates.Add(new Point(0, 1));
            //mesh.TextureCoordinates.Add(new Point(1, 0));
            //mesh.TextureCoordinates.Add(new Point(1, 1));
            #endregion

            AddCubeToMesh(p0, w, h, d, mesh);
            Material material = new DiffuseMaterial(new ImageBrush(image_material));
            return new GeometryModel3D(mesh, material);
        }

        public GeometryModel3D CreateModel(Color image_material)
        {
            return CreateCubeModel(origin, width, height, depth, image_material);
        }

        public static GeometryModel3D CreateCubeModel(Point3D p0, double w, double h, double d, Color image_material)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            AddCubeToMesh(p0, w, h, d, mesh);
            Material material = new DiffuseMaterial(new SolidColorBrush(image_material));
            return new GeometryModel3D(mesh, material);
        }
    }
}
