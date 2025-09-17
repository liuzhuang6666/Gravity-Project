/*----------------------------------------------------------------
// Copyright (C) 2020 中地数码科技有限公司
// 版权所有。
//
// 文件名：DocumentItemPropertiesManage
// 文件功能描述：
//
//
// 创建标识：hw 2020/7/3 14:26:07
//----------------------------------------------------------------*/

using MapGIS.Desktop.UI.Controls;
using MapGIS.GeoMap;
using MapGIS.UI.Controls;
using System;
using System.Windows.Forms;

namespace MapGISPlugin3
{
    public class DocumentItemPropertiesManage : IDisposable
    {
        private WholeProperty wp = null;

        public DocumentItemPropertiesManage()
        {
            MapGIS.Desktop.UI.Controls.DocumentItemProperties.RequestExtend += new EventHandler<PageExtendArgs>(DocumentItemProperties_RequestExtend);
        }

        public void Dispose()
        {
            MapGIS.Desktop.UI.Controls.DocumentItemProperties.RequestExtend -= new EventHandler<PageExtendArgs>(DocumentItemProperties_RequestExtend);
        }

        private void DocumentItemProperties_RequestExtend(object sender, PageExtendArgs e)
        {
            if (e.NodePage != null)
            {
                if (e.Items != null)
                {
                    DocumentItemProperty itemPro = MapGIS.Desktop.UI.Controls.DocumentItemProperties.DocItemProperty;
                    if (itemPro != null)
                    {
                        //GetWholeProperty(itemPro);
                    }
                }
                if (e.NodePage.Nodes != null)
                {
                    for (int i = 0; i < e.NodePage.Nodes.Length; i++)
                    {
                        System.Windows.Forms.TreeNode node = e.NodePage.Nodes[i];

                        UpdateTreeTag(node);
                    }
                }
                if (e.NodePage.Nodes != null && e.Item != null && e.Item is VectorLayer)
                {
                    //if (StaticFunctions.IsCartoLayer(e.Item as MapLayer) && (e.Item as MapLayer).GeometryType != GeoObjects.GeomType.Ann)
                    //{
                    //    if (e.NodePage.Nodes[2].Nodes.Count >= 2)
                    //    {
                    //        for (int i = e.NodePage.Nodes[2].Nodes.Count - 1; i >= 0; i--)
                    //        {
                    //            TreeNode node = e.NodePage.Nodes[2].Nodes[i];
                    //            if (node == null || node.Tag == null || !"显示".Equals(node.Text))
                    //            {
                    //                e.NodePage.Nodes[2].Nodes.RemoveAt(i);
                    //            }
                    //        }
                    //        if (e.NodePage.Nodes[2].Nodes != null && e.NodePage.Nodes[2].Nodes.Count > 0 && e.NodePage.Nodes[2].Nodes[0] != null)
                    //            e.NodePage.Nodes[2].Tag = e.NodePage.Nodes[2].Nodes[0].Tag;
                    //    }
                    //}
                }
            }
        }

        private void UpdateTreeTag(TreeNode node)
        {
            if (node != null)
            {
                if (node.Tag != null)
                {
                    //if (node.Tag is MapGIS.UI.Controls.DocumentBriefProperty)
                    //{
                    //    if (node.Parent != null && node.Parent.Tag != null && node.Parent.Tag is CTPublicUI.DocumentBriefProperty)
                    //    {
                    //        node.Tag = node.Parent.Tag;
                    //    }
                    //    else
                    //    {
                    //        CTPublicUI.DocumentBriefProperty common = new CTPublicUI.DocumentBriefProperty();
                    //        common.SetItem((node.Tag as MapGIS.UI.Controls.DocumentBriefProperty).DocItem);
                    //        node.Tag = common;

                    //    }
                    //}
                    //else if (node.Tag is MapGIS.UI.Controls.MapCommonProperty)
                    //{
                    //    if (node.Parent != null && node.Parent.Tag != null && node.Parent.Tag is CTPublicUI.MapCommonProperty)
                    //    {
                    //        node.Tag = node.Parent.Tag;
                    //    }
                    //    else
                    //    {
                    //        CTPublicUI.MapCommonProperty common = new CTPublicUI.MapCommonProperty();
                    //        common.SetItem((node.Tag as MapGIS.UI.Controls.MapCommonProperty).DocItem);
                    //        node.Tag = common;
                    //    }
                    //}
                    //else if (node.Tag is MapGIS.UI.Controls.SfOrACls6xCommonProperty)
                    //{
                    //    if (node.Parent != null && node.Parent.Tag != null && node.Parent.Tag is CTPublicUI.SfOrACls6xCommonProperty)
                    //    {
                    //        node.Tag = node.Parent.Tag;
                    //    }
                    //    else
                    //    {
                    //        CTPublicUI.SfOrACls6xCommonProperty common = new CTPublicUI.SfOrACls6xCommonProperty();
                    //        common.SetItem((node.Tag as MapGIS.UI.Controls.SfOrACls6xCommonProperty).DocItem);
                    //        node.Tag = common;
                    //    }
                    //}
                    //else if (node.Tag is MapGIS.UI.Controls.SfOrAClsCommonProperty)
                    //{
                    //    if (node.Parent != null && node.Parent.Tag != null && node.Parent.Tag is CTPublicUI.SfOrAClsCommonProperty)
                    //    {
                    //        node.Tag = node.Parent.Tag;
                    //    }
                    //    else
                    //    {
                    //        CTPublicUI.SfOrAClsCommonProperty common = new CTPublicUI.SfOrAClsCommonProperty();
                    //        common.SetItem((node.Tag as MapGIS.UI.Controls.SfOrAClsCommonProperty).DocItem);
                    //        node.Tag = common;
                    //    }
                    //}
                    //else if (node.Tag is MapGIS.UI.Controls.VectorClsCommonProperty)
                    //{
                    //    if (node.Parent != null && node.Parent.Tag != null && node.Parent.Tag is CTPublicUI.VectorClsCommonProperty)
                    //    {
                    //        node.Tag = node.Parent.Tag;
                    //    }
                    //    else
                    //    {
                    //        CTPublicUI.VectorClsCommonProperty common = new CTPublicUI.VectorClsCommonProperty();
                    //        common.SetItem((node.Tag as MapGIS.UI.Controls.VectorClsCommonProperty).DocItem);
                    //        node.Tag = common;
                    //    }
                    //}
                    //else if (node.Tag is MapGIS.UI.Controls.MapsetCommonProperty)
                    //{
                    //    if (node.Parent != null && node.Parent.Tag != null && node.Parent.Tag is CTPublicUI.MapsetCommonProperty)
                    //    {
                    //        node.Tag = node.Parent.Tag;
                    //    }
                    //    else
                    //    {
                    //        CTPublicUI.MapsetCommonProperty common = new CTPublicUI.MapsetCommonProperty();
                    //        common.SetItem((node.Tag as MapGIS.UI.Controls.MapsetCommonProperty).DocItem);
                    //        node.Tag = common;
                    //    }
                    //}
                    //else if (node.Tag is MapGIS.UI.Controls.NetClsCommonProperty)
                    //{
                    //    if (node.Parent != null && node.Parent.Tag != null && node.Parent.Tag is CTPublicUI.NetClsCommonProperty)
                    //    {
                    //        node.Tag = node.Parent.Tag;
                    //    }
                    //    else
                    //    {
                    //        CTPublicUI.NetClsCommonProperty common = new CTPublicUI.NetClsCommonProperty();
                    //        common.SetItem((node.Tag as MapGIS.UI.Controls.NetClsCommonProperty).DocItem);
                    //        node.Tag = common;
                    //    }
                    //}
                    //else if (node.Tag is MapGIS.UI.Controls.OClsCommonProperty)
                    //{
                    //    if (node.Parent != null && node.Parent.Tag != null && node.Parent.Tag is CTPublicUI.OClsCommonProperty)
                    //    {
                    //        node.Tag = node.Parent.Tag;
                    //    }
                    //    else
                    //    {
                    //        CTPublicUI.OClsCommonProperty common = new CTPublicUI.OClsCommonProperty();
                    //        common.SetItem((node.Tag as MapGIS.UI.Controls.OClsCommonProperty).DocItem);
                    //        node.Tag = common;
                    //    }
                    //}
                    //else if (node.Tag is MapGIS.UI.Controls.RasterCommonProperty)
                    //{
                    //    if (node.Parent != null && node.Parent.Tag != null && node.Parent.Tag is CTPublicUI.RasterCommonProperty)
                    //    {
                    //        node.Tag = node.Parent.Tag;
                    //    }
                    //    else
                    //    {
                    //        CTPublicUI.RasterCommonProperty common = new CTPublicUI.RasterCommonProperty();
                    //        common.SetItem((node.Tag as MapGIS.UI.Controls.RasterCommonProperty).DocItem);
                    //        node.Tag = common;
                    //    }
                    //}
                    //else if (node.Tag is MapGIS.UI.Controls.RasCommonProperty)
                    //{
                    //    if (node.Parent != null && node.Parent.Tag != null && node.Parent.Tag is CTPublicUI.RasBaseProperty)
                    //    {
                    //        node.Tag = node.Parent.Tag;
                    //    }
                    //    else
                    //    {
                    //        CTPublicUI.RasCommonProperty common = new CTPublicUI.RasCommonProperty();
                    //        common.SetItem((node.Tag as MapGIS.UI.Controls.RasCommonProperty).DocItem);
                    //        node.Tag = common;
                    //    }
                    //}
                    //else if (node.Tag is MapGIS.UI.Controls.MosaicDataSetCommonProperty)
                    //{
                    //    if (node.Parent != null && node.Parent.Tag != null && node.Parent.Tag is CTPublicUI.MosaicDataSetCommonProperty)
                    //    {
                    //        node.Tag = node.Parent.Tag;
                    //    }
                    //    else
                    //    {
                    //        CTPublicUI.MosaicDataSetCommonProperty common = new CTPublicUI.MosaicDataSetCommonProperty();
                    //        common.SetItem((node.Tag as MapGIS.UI.Controls.MosaicDataSetCommonProperty).DocItem);
                    //        node.Tag = common;
                    //    }
                    //}
                    //else if (node.Tag is MapGIS.UI.Controls.RasBaseProperty)
                    //{
                    //    if (node.Parent != null && node.Parent.Tag != null && node.Parent.Tag is CTPublicUI.RasBaseProperty)
                    //    {
                    //        node.Tag = node.Parent.Tag;
                    //    }
                    //    else
                    //    {
                    //        CTPublicUI.RasBaseProperty common = new CTPublicUI.RasBaseProperty();
                    //        common.SetItem((node.Tag as MapGIS.UI.Controls.RasBaseProperty).DocItem);
                    //        node.Tag = common;
                    //    }
                    //}
                    //else if (node.Tag is MapGIS.UI.Controls.CommonProperty)
                    //{
                    //    if (node.Parent != null && node.Parent.Tag != null && node.Parent.Tag is CTPublicUI.CommonProperty)
                    //    {
                    //        node.Tag = node.Parent.Tag;
                    //    }
                    //    else
                    //    {
                    //        CTPublicUI.CommonProperty common = new CTPublicUI.CommonProperty();
                    //        common.SetItem((node.Tag as MapGIS.UI.Controls.CommonProperty).DocItem);
                    //        node.Tag = common;
                    //    }
                    //}
                    //else if (node.Tag is MapGIS.UI.Controls.LayersSfclsProperty)
                    //{
                    //    if (node.Parent != null && node.Parent.Tag != null && node.Parent.Tag is CTPublicUI.LayersSfclsProperty)
                    //    {
                    //        node.Tag = node.Parent.Tag;
                    //    }
                    //    else
                    //    {
                    //        CTPublicUI.LayersSfclsProperty common = new CTPublicUI.LayersSfclsProperty();
                    //        common.SetItem((node.Tag as MapGIS.UI.Controls.LayersSfclsProperty).DocItem);
                    //        node.Tag = common;
                    //    }
                    //}
                    //else if (node.Tag is MapGIS.UI.Controls.PntSFClsShowProperty)
                    //{
                    //    if (node.Parent != null && node.Parent.Tag != null && (node.Parent.Tag is PntSFClsCartoShowProperty || node.Parent.Tag is PntSFClsShowProperty))
                    //    {
                    //        node.Tag = node.Parent.Tag;
                    //    }
                    //    else
                    //    {
                    //        IProperty pntProperty = null;
                    //        DocumentItem item = (node.Tag as MapGIS.UI.Controls.PntSFClsShowProperty).DocItem;
                    //        if (item != null && item is VectorLayer)
                    //        {
                    //            if (StaticFunctions.IsCartoLayer(item as MapLayer))
                    //                pntProperty = new PntSFClsCartoShowProperty();
                    //            else
                    //                pntProperty = new PntSFClsShowProperty();
                    //        }
                    //        if (pntProperty != null)
                    //        {
                    //            pntProperty.SetItem(item);
                    //            node.Tag = pntProperty;
                    //        }
                    //    }
                    //}
                    //else if (node.Tag is MapGIS.UI.Controls.LinSFClsShowProperty)
                    //{
                    //    if (node.Parent != null && node.Parent.Tag != null && (node.Parent.Tag is LinSFClsCartoShowProperty || node.Parent.Tag is LinSFClsShowProperty))
                    //    {
                    //        node.Tag = node.Parent.Tag;
                    //    }
                    //    else
                    //    {
                    //        IProperty linProperty = null;
                    //        DocumentItem item = (node.Tag as MapGIS.UI.Controls.LinSFClsShowProperty).DocItem;
                    //        if (item != null && item is VectorLayer)
                    //        {
                    //            if (StaticFunctions.IsCartoLayer(item as MapLayer))
                    //                linProperty = new LinSFClsCartoShowProperty();
                    //            else
                    //                linProperty = new LinSFClsShowProperty();
                    //        }
                    //        if (linProperty != null)
                    //        {
                    //            linProperty.SetItem(item);
                    //            node.Tag = linProperty;
                    //        }
                    //    }
                    //}
                    //else if (node.Tag is MapGIS.UI.Controls.RegSFClsShowProperty)
                    //{
                    //    if (node.Parent != null && node.Parent.Tag != null && (node.Parent.Tag is RegSFClsCartoShowProperty || node.Parent.Tag is RegSFClsShowProperty))
                    //    {
                    //        node.Tag = node.Parent.Tag;
                    //    }
                    //    else
                    //    {
                    //        IProperty regProperty = null;
                    //        DocumentItem item = (node.Tag as MapGIS.UI.Controls.RegSFClsShowProperty).DocItem;
                    //        if (item != null && item is VectorLayer)
                    //        {
                    //            if (StaticFunctions.IsCartoLayer(item as MapLayer))
                    //                regProperty = new RegSFClsCartoShowProperty();
                    //            else
                    //                regProperty = new RegSFClsShowProperty();
                    //        }
                    //        if (regProperty != null)
                    //        {
                    //            regProperty.SetItem(item);
                    //            node.Tag = regProperty;
                    //        }
                    //    }
                    //}
                    //else if (node.Tag is MapGIS.UI.Controls.AnnShowProperty)
                    //{
                    //    if (node.Parent != null && node.Parent.Tag != null && node.Parent.Tag is CTPublicUI.AnnShowProperty)
                    //    {
                    //        node.Tag = node.Parent.Tag;
                    //    }
                    //    else
                    //    {
                    //        AnnShowProperty annProperty = new AnnShowProperty();
                    //        annProperty.SetItem((node.Tag as MapGIS.UI.Controls.AnnShowProperty).DocItem);
                    //        node.Tag = annProperty;
                    //    }
                    //}
                }
                if (node.Nodes != null && node.Nodes.Count > 0)
                {
                    //for (int i = node.Nodes.Count - 1; i >= 0; i--)
                    //{
                    //    System.Windows.Forms.TreeNode choildNode = node.Nodes[i];
                    //    if (choildNode != null)
                    //    {
                    //        if (choildNode.Tag is MapGIS.UI.Controls.GraphicsDataProperty)
                    //        { //去除地图上的 制图数据 选项，防止引起误会
                    //            node.Nodes.RemoveAt(i);
                    //        }
                    //        else if (choildNode.Tag is MapGIS.UI.Controls.SfclsDynamicAnnProperty)
                    //        {
                    //            MapGIS.UI.Controls.SfclsDynamicAnnProperty sfclsPro = choildNode.Tag as MapGIS.UI.Controls.SfclsDynamicAnnProperty;
                    //            if (sfclsPro != null && sfclsPro.DocItem is Map)
                    //            {
                    //                DocumentItem item = GetPrivateVectorLayer(sfclsPro);
                    //                if (item != null && item is VectorLayer)
                    //                {
                    //                    if (StaticFunctions.IsCartoLayer(item as VectorLayer))
                    //                        node.Nodes.RemoveAt(i);
                    //                    else
                    //                        UpdateTreeTag(choildNode);
                    //                }
                    //                else
                    //                    UpdateTreeTag(choildNode);
                    //            }
                    //            else
                    //                UpdateTreeTag(choildNode);
                    //        }
                    //        else
                    //        {
                    //            UpdateTreeTag(choildNode);
                    //        }
                    //    }
                    //}
                }
            }
        }

        public DocumentItem GetPrivateVectorLayer(MapGIS.UI.Controls.SfclsDynamicAnnProperty sfclsProperty)
        {
            if (sfclsProperty != null)
            {
                try
                {
                    Type type = sfclsProperty.GetType();
                    object rtn = type.InvokeMember("m_VectorLayer", System.Reflection.BindingFlags.NonPublic |
                            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField, null, sfclsProperty, null);
                    if (rtn != null && rtn is VectorLayer)
                    {
                        return rtn as VectorLayer;
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return null;
        }

        //public DocumentItem GetWholeProperty(DocumentItemProperty itemPro)
        //{
        //    if (itemPro != null)
        //    {
        //        try
        //        {
        //            Type type = itemPro.GetType();
        //            FieldInfo fieldInfo = type.GetField("wholeProperty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField);
        //            object objControl = fieldInfo.GetValue(itemPro);
        //            if (objControl != null && objControl is WholeProperty && !(objControl is WholePropertyExpand))
        //            {
        //                wp = objControl as WholeProperty;
        //                if (fieldInfo != null && wp != null)
        //                {
        //                    WholePropertyExpand wholeExpand = new WholePropertyExpand();
        //                    wholeExpand.Items = wp.Items;
        //                    fieldInfo.SetValue(itemPro, wholeExpand, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, null);
        //                    SetNodePage(itemPro, wholeExpand);
        //                    ShowUserProperty(itemPro, wholeExpand);
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MFObjects.Common.showtime(ex.ToString());
        //        }
        //    }
        //    return null;
        //}

        //private void SetNodePage(DocumentItemProperty itemPro, WholePropertyExpand wholeExpand)
        //{
        //    if (itemPro != null)
        //    {
        //        try
        //        {
        //            Type type = itemPro.GetType();
        //            FieldInfo fieldInfo = type.GetField("nodePage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField);
        //            object objControl = fieldInfo.GetValue(itemPro);
        //            if (objControl != null && objControl is WholeProperty && !(objControl is WholePropertyExpand))
        //            {
        //                fieldInfo.SetValue(itemPro, wholeExpand, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, null);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //        }
        //    }
        //    return;
        //}

        //private void ShowUserProperty(DocumentItemProperty itemPro, Control userControl)
        //{
        //    if (itemPro != null)
        //    {
        //        try
        //        {
        //            Type type = itemPro.GetType();
        //            MethodInfo methodInfo = type.GetMethod("ShowUserProperty", System.Reflection.BindingFlags.NonPublic |
        //                    System.Reflection.BindingFlags.Instance);
        //            if (methodInfo != null)
        //            {
        //                methodInfo.Invoke(itemPro, new object[] { userControl });
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //        }
        //    }
        //}
    }
}