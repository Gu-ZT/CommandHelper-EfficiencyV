using cbhk_environment.WindowDictionaries;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Windows;
using cbhk_environment.CustomControls;
using System.Windows.Controls;
using cbhk_environment.Generators.ItemGenerator.Components;
using System;
using cbhk_environment.ControlsDataContexts;
using System.Linq;
using cbhk_environment.GeneralTools;
using System.Text.RegularExpressions;
using cbhk_environment.GenerateResultDisplayer;

namespace cbhk_environment.Generators.ItemGenerator
{
    public class item_datacontext:ObservableObject
    {
        #region 返回和运行指令
        public RelayCommand<CommonWindow> ReturnCommand { get; set; }
        public RelayCommand RunCommand { get; set; }
        #endregion

        public item_datacontext()
        {
            #region 连接指令
            ReturnCommand = new RelayCommand<CommonWindow>(return_command);
            RunCommand = new RelayCommand(run_command);
            #endregion

            #region 清除所有数据
            CanDestroyBlocks.Clear();
            CanPlaceOnBlocks.Clear();
            EnchantmentIDs.Clear();
            EnchantmentLevels.Clear();
            MobEffectIDs.Clear();
            MobEffectDurations.Clear();
            MobEffectLevels.Clear();
            AttributeIDs.Clear();
            AttributeNames.Clear();
            AttributeValues.Clear();
            AttributeValueTypes.Clear();
            AttributeSlots.Clear();
            #endregion
        }

        #region 覆盖生成
        private bool overLying;
        public bool OverLying
        {
            get { return overLying; }
            set
            {
                overLying = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 版本
        private bool behavior_lock;
        private bool version1_12 = false;
        public bool Version1_12
        {
            get { return version1_12; }
            set
            {
                version1_12 = value;
                if (behavior_lock)
                {
                    behavior_lock = false;
                    Version1_13 = !Version1_12;
                    behavior_lock = true;
                }
                OnPropertyChanged();
            }
        }
        private bool version1_13 = true;
        public bool Version1_13
        {
            get { return version1_13; }
            set
            {
                version1_13 = value;
                if (behavior_lock)
                {
                    behavior_lock = false;
                    Version1_12 = !Version1_13;
                    behavior_lock = true;
                }
                OnPropertyChanged();
            }
        }
        #endregion

        //本生成器的图标路径
        string icon_path = AppDomain.CurrentDomain.BaseDirectory+ "resources\\configs\\Item\\images\\icon.png";

        private void return_command(CommonWindow win)
        {
            Item.cbhk.Topmost = true;
            Item.cbhk.WindowState = WindowState.Normal;
            Item.cbhk.Show();
            Item.cbhk.Topmost = false;
            Item.cbhk.ShowInTaskbar = true;
            win.Close();
        }
        private void run_command()
        {
            string result = "";
            result += UnbreakableString+ ItemDamageString + ItemDisplay + ItemHideFlags + CustomTag + CanDestroyBlockList + CanPlaceOnBlockList + EnchantmentString + AttributeModifiers + MobEffect;
            result = result.Trim() != "" ? "give @p minecraft:" + ItemId + "{" + result.TrimEnd(',') + "} " + ItemCount : (ItemId.Trim() != "" ? "give @p minecraft:" + ItemId + " "+ (ItemCount.Trim() == "1" || ItemCount.Trim() == "0" ? "" : ItemCount) : "");
            Displayer displayer = Displayer.GetContentDisplayer();
            displayer.GeneratorResult(OverLying, new string[] { result }, new string[] { DisplayName.Trim() != "" ? DisplayName : "" }, new string[] { icon_path }, new System.Windows.Media.Media3D.Vector3D() { X = 30, Y = 30 });
            displayer.Show();
        }

        #region 运行与使用
        //保存可放置方块列表
        public static List<string> CanPlaceOnBlocks = new List<string> { };
        //保存可破坏方块列表
        public static List<string> CanDestroyBlocks = new List<string> { };
        //保存附魔ID列表
        public static List<string> EnchantmentIDs = new List<string> { };
        //保存附魔等级列表
        public static List<string> EnchantmentLevels = new List<string> { };
        //保存药水效果ID列表
        public static List<string> MobEffectIDs = new List<string> { };
        //保存药水效果持续时间列表
        public static List<string> MobEffectDurations = new List<string> { };
        //保存药水效果等级列表
        public static List<string> MobEffectLevels = new List<string> { };
        //保存属性ID列表
        public static List<string> AttributeIDs = new List<string> { };
        //保存属性name列表
        public static List<string> AttributeNames = new List<string> { };
        //保存属性值列表
        public static List<string> AttributeValues = new List<string> { };
        //保存属性值类型列表
        public static List<string> AttributeValueTypes = new List<string> { };
        //保存属性生效槽位列表
        public static List<string> AttributeSlots = new List<string> { };

        #region 无法破坏
        private bool unbreakable;
        public bool Unbreakable
        {
            get { return unbreakable; }
            set
            {
                unbreakable = value;
                OnPropertyChanged();
            }
        }
        private string UnbreakableString
        {
            get
            {
                return Unbreakable?"Unbreakable:1b,":"";
            }
        }
        #endregion

        #region 保存名称与描述
        private string ItemDisplay
        {
            get
            {
                string DisplayNameString = DisplayName.Trim() != "" ? "Name:'{\"text\":\"" + DisplayName + "\"}'" : "";
                string ItemLoreString = ItemLore.Trim() != "" ? ",Lore:[\"[\\\"" + ItemLore + "\\\"]\"]" : "";
                string result = DisplayNameString != "" || ItemLoreString != ""?"display:{"+DisplayNameString+ItemLoreString+"},":"";
                return result;
            }
        }
        #endregion

        #region 保存物品附加值/损耗值
        public string ItemDamageString
        {
            get
            {
                string result = ItemDamage.Trim()!="" && ItemDamage.Trim() != "0" ?"Damage:"+ItemDamage+",":"";
                return result;
            }
        }
        #endregion

        #region 保存物品信息隐藏选项
        private TextSource hide_infomation_option;
        public TextSource HideInfomationOption
        {
            get { return hide_infomation_option; }
            set
            {
                hide_infomation_option = value;
                OnPropertyChanged();
            }
        }
        private string ItemHideFlags
        {
            get
            {
                string key = MainWindow.hide_infomation_database.Where(item => item.Value == HideInfomationOption.ItemText).First().Key;
                string result = key!="0"? "HideFlags:"+key+"b," : "";
                return result;
            }
        }
        #endregion

        #region 保存物品ID
        private ItemDataGroup select_item_id_source;
        public ItemDataGroup SelectItemIdSource
        {
            get { return select_item_id_source; }
            set
            {
                select_item_id_source = value;
                //OnPropertyChanged();
            }
        }
        private string ItemId
        {
            get
            {
                string key = MainWindow.item_database.Where(item => Regex.Match(item.Key, @"[\u4e00-\u9fa5]+").ToString() == SelectItemIdSource.ItemText).First().Key;
                string result = key != "" ? Regex.Match(key,@"[a-zA-Z_]+").ToString() : "";
                return result;
            }
        }
        #endregion

        #region 保存物品数量
        private string item_count = "";
        public string ItemCount
        {
            get { return item_count; }
            set
            {
                item_count = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 保存物品自定义标签
        private string custom_tag = "";

        public string CustomTag
        {
            get 
            {
                string result = custom_tag.Trim() != "" ? custom_tag + "," : "";
                return result;
            }
            set
            {
                custom_tag = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 保存可破坏方块列表
        private string CanDestroyBlockList
        {
            get
            {
                string result = "CanDestroy:[";
                foreach (string item in CanDestroyBlocks)
                {
                    string key = MainWindow.item_database.Where(obj=>Regex.Match(obj.Key, "[\u4e00-\u9fa5]+").ToString() == item).First().Key;
                    key = Regex.Match(key,"[a-zA-Z_]+").ToString();
                    result += "\""+ key + "\",";
                }
                result = result.Trim() != "CanDestroy:[" ? result + "]," : "";
                return result;
            }
        }
        #endregion

        #region 保存可放置方块列表
        private string CanPlaceOnBlockList
        {
            get
            {
                string result = "CanPlaceOn:[";
                foreach (string item in CanPlaceOnBlocks)
                {
                    string key = MainWindow.item_database.Where(obj => Regex.Match(obj.Key, "[\u4e00-\u9fa5]+").ToString() == item).First().Key;
                    key = Regex.Match(key, "[a-zA-Z_]+").ToString();
                    result += "\"" + key + "\",";
                }
                result = result.Trim()!= "CanPlaceOn:[" ? result+ "],":"";
                return result;
            }
        }
        #endregion

        #region 保存附魔信息列表
        private string EnchantmentString
        {
            get
            {
                string result = "Enchantments:[";
                for (int i = 0; i < EnchantmentIDs.Count; i++)
                {
                    string enchantment_level = "lvl:";
                    if ((EnchantmentLevels.Count - 1) >= i)
                        enchantment_level += EnchantmentLevels[i]+"s";
                    else
                        enchantment_level += "1s";
                    result += "{id:\"minecraft:" + EnchantmentIDs[i] +"\","+enchantment_level+"},";
                }
                result = result.Trim()!= "Enchantments:["?result.Trim(',')+"],":"";
                return result;
            }
        }
        #endregion

        #region 保存属性列表
        private string AttributeModifiers
        {
            get
            {
                string result = "AttributeModifiers:[";
                for (int i = 0; i < AttributeIDs.Count; i++)
                {
                    string uid = Uid_spawner.NewUuidString();
                    string first_uid = Regex.Match(uid, "(\\d){4}").ToString();
                    uid = Uid_spawner.NewUuidString();
                    string second_uid = Regex.Match(uid, "(\\d){4}").ToString();
                    uid = Uid_spawner.NewUuidString();
                    string third_uid = Regex.Match(uid, "(\\d){4}").ToString();
                    uid = Uid_spawner.NewUuidString();
                    string fourth_uid = Regex.Match(uid, "(\\d){4}").ToString();
                    string UUID = "[I;"+first_uid+","+second_uid+","+third_uid+","+fourth_uid+"],";
                    string attribute_name = "AttributeName:\"" + AttributeIDs[i] +"\",";
                    string name = "Name:\"null\"";
                    if((AttributeNames.Count - 1) >= i)
                    name = "Name:\"" + AttributeNames[i] +"\",";
                    string amount = "Amount:0d,";
                    if ((AttributeValues.Count - 1) >= i)
                        amount = "Amount:"+ AttributeValues[i] +"d,";
                    string operation = "Operation:0,";
                    if ((AttributeValueTypes.Count - 1) >= i)
                        operation = "Operation:" + AttributeValueTypes[i]+",";
                    string slot = "";
                    if ((AttributeSlots.Count - 1) >= i)
                        if (AttributeSlots[i]!="all")
                        slot = ",Slot:" + AttributeSlots[i];
                    result += "{"+ attribute_name+amount+operation+name+ UUID+slot +"},";
                }
                result = result.Trim() != "AttributeModifiers:[" ? result + "]," : "";
                return result;
            }
        }
        #endregion

        #region 保存药水效果列表
        private string MobEffect
        {
            get
            {
                string result = "CustomPotionEffects:[";
                for (int i = 0; i < MobEffectIDs.Count; i++)
                {
                    string id = "Id:" + MobEffectIDs[i] +"b,";
                    string duration = "Duration:0";
                    if ((MobEffectDurations.Count - 1) >= i)
                        duration = "Duration:" + MobEffectDurations[i]+",";
                    string amplifier = "Amplifier:0b";
                    if ((MobEffectLevels.Count - 1) >= i)
                        amplifier = "Amplifier:" + MobEffectLevels[i]+",";
                    result += "{"+id+duration+amplifier+ "ShowParticles:0b},";
                }
                result = result != "CustomPotionEffects:[" ? result.TrimEnd(',') + "]," :"";
                return result;
            }
        }
        #endregion

        #endregion

        #region 加载与显示

        #region 保存物品附加值/损耗值
        private string item_damage = "";
        public string ItemDamage
        {
            get { return item_damage; }
            set
            {
                item_damage = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 保存物品名称
        private string display_name = "";
        public string DisplayName
        {
            get { return display_name; }
            set
            {
                display_name = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 保存物品描述
        private string item_lore = "";
        public string ItemLore
        {
            get { return item_lore; }
            set
            {
                item_lore = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 保存滚动视图样式
        public static Style scrollbarStyle;
        #endregion

        #endregion

        /// <summary>
        /// 载入物品id列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ItemIdsLoaded(object sender, RoutedEventArgs e)
        {
            IconComboBoxs iconComboBoxs = sender as IconComboBoxs;
            iconComboBoxs.ItemsSource = MainWindow.ItemIdSource.ItemDataSource;

            iconComboBoxs.SelectedIndex = 0;
            iconComboBoxs.ApplyTemplate();
            TextBox box = iconComboBoxs.Template.FindName("EditableTextBox", iconComboBoxs) as TextBox;
            ItemDataGroup first = iconComboBoxs.Items[0] as ItemDataGroup;
            box.Text = first.ItemText;
            Image image = iconComboBoxs.Template.FindName("PART_DisplayIcon", iconComboBoxs) as Image;
            image.Source = first.ItemImage;
            SelectItemIdSource = first;
        }

        /// <summary>
        /// 载入物品隐藏信息选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HideFlagsLoaded(object sender, RoutedEventArgs e)
        {
            TextComboBoxs iconComboBoxs = sender as TextComboBoxs;

            iconComboBoxs.ItemsSource = MainWindow.hide_flags_source.ItemDataSource;
            HideInfomationOption = iconComboBoxs.Items[0] as TextSource;
            TextBox textbox = iconComboBoxs.Template.FindName("EditableTextBox", iconComboBoxs) as TextBox;
            textbox.Text = HideInfomationOption.ItemText;
        }

        /// <summary>
        /// 滚动视图载入事件,获取样式引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ScrollViewerLoaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer viewer = sender as ScrollViewer;
            scrollbarStyle = viewer.Style;
        }

        /// <summary>
        /// 增加可破坏方块列表成员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddCanDestoryBlockClick(object sender, RoutedEventArgs e)
        {
            IconTextButtons iconTextButtons = sender as IconTextButtons;
            Grid parent = iconTextButtons.Parent as Grid;
            CollapsableGrids collapsableGrids = parent.Children[0] as CollapsableGrids;
            CanDestroyItems canDestroyItems = new CanDestroyItems();
            StackPanel stackPanel = collapsableGrids.Content as StackPanel;
            stackPanel.Children.Add(canDestroyItems);
        }

        /// <summary>
        /// 增加可放置方块列表成员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddCanPlaceOnBlockClick(object sender, RoutedEventArgs e)
        {
            IconTextButtons iconTextButtons = sender as IconTextButtons;
            Grid parent = iconTextButtons.Parent as Grid;
            CollapsableGrids collapsableGrids = parent.Children[0] as CollapsableGrids;
            CanPlaceOnItems canPlaceOnItems = new CanPlaceOnItems();
            StackPanel stackPanel = collapsableGrids.Content as StackPanel;
            stackPanel.Children.Add(canPlaceOnItems);
        }

        /// <summary>
        /// 增加附魔列表成员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddEnchantmentsClick(object sender, RoutedEventArgs e)
        {
            IconTextButtons iconTextButtons = sender as IconTextButtons;
            Grid parent = iconTextButtons.Parent as Grid;
            CollapsableGrids collapsableGrids = parent.Children[0] as CollapsableGrids;
            EnchantmentItems enchantmentItems = new EnchantmentItems();
            StackPanel stackPanel = collapsableGrids.Content as StackPanel;
            stackPanel.Children.Add(enchantmentItems);
        }

        /// <summary>
        /// 增加属性列表成员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddAttributesClick(object sender, RoutedEventArgs e)
        {
            IconTextButtons iconTextButtons = sender as IconTextButtons;
            AttributeItems attributeItems = new AttributeItems();
            Grid parent = iconTextButtons.Parent as Grid;
            CollapsableGrids collapsableGrids = parent.Children[0] as CollapsableGrids;
            StackPanel stackPanel = collapsableGrids.Content as StackPanel;
            stackPanel.Children.Add(attributeItems);
        }

        /// <summary>
        /// 增加特殊数据成员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddSpecialDataClick(object sender, RoutedEventArgs e)
        {
            IconTextButtons iconTextButtons = sender as IconTextButtons;
            PotionTypeItems potionTypeItems = new PotionTypeItems();
            Grid parent = iconTextButtons.Parent as Grid;
            CollapsableGrids collapsableGrids = parent.Children[0] as CollapsableGrids;
            StackPanel stackPanel = collapsableGrids.Content as StackPanel;
            stackPanel.Children.Add(potionTypeItems);
        }
    }
}
