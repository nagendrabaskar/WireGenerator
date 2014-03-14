﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows;
using WireGenerator.Model;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Xml.Serialization;

namespace WireGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        #region variables, enums
        private static string xmlPath = "AppSchema.xml";
        private XElement appSchema;
        private AppModel appModel;
        private MenuSubItem menuSubItem;
        private StringBuilder content = new StringBuilder();
        private int counter;
        private bool lineItemModelSnippetAdded;
        private string destinationPath = "c:/WireFrames/";

        enum FormControlTypes : int
        {
            TextBox = 1,
            MultiLine = 2,
            DropDown = 3,
            CheckBox = 4,
            LineItems = 5,
            Content = 6
        }

        enum UserForms : int
        {
            AppMain = 1,
            AppNavigation = 2,
            AppEntities = 3
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            appSchema = XElement.Load(xmlPath);
            appModel = new AppModel();
            appModel.Navigation = new List<WireGenerator.Model.MenuItem>();

            if (!appSchema.IsEmpty)
            {
                this.App_Name.Text = appSchema.Element("Name").Value;
                this.App_UserName.Text = appSchema.Element("LoginUser").Value;

                //navigation
                var navigation = new XElement("Navigation");
                TreeViewItem node, subnode;
                WireGenerator.Model.MenuItem menuItem;
                IEnumerable<XElement> navMenuItems = from el in appSchema.Elements("Navigation").Elements("MenuItem") select el;
                foreach (XElement el in navMenuItems.ToList())
                {
                    menuItem = new WireGenerator.Model.MenuItem();
                    menuItem.Name = el.Attribute("name").Value;
                    menuItem.LinkToUrl = (el.Attribute("linkTo") != null) ? el.Attribute("linkTo").Value : string.Empty;

                    node = new TreeViewItem();
                    node.Header = el.Attribute("name").Value;
                    IEnumerable<XElement> menuSubItems = el.Descendants();
                    menuItem.MenuSubItems = new List<MenuSubItem>();
                    foreach (XElement subel in menuSubItems)
                    {
                        menuSubItem = new MenuSubItem();
                        menuSubItem.SubItem = subel.Value;
                        if (subel.HasAttributes)
                            menuSubItem.LinkToURL = subel.Attribute("linkTo").Value + "_list.html";

                        menuItem.MenuSubItems.Add(menuSubItem);

                        subnode = new TreeViewItem();
                        subnode.Header = subel.Value;
                        node.Items.Add(subnode);
                    }
                    NavigationTreeView.Items.Add(node);
                    appModel.Navigation.Add(menuItem);
                }
            }
        }

        #region GenerateOnClick(object sender, RoutedEventArgs e)
        private void GenerateOnClick(object sender, RoutedEventArgs e)
        {
            //load app schema file
            appSchema = XElement.Load(xmlPath);

            if (!appSchema.IsEmpty)
            {
                //first, clear the existing files in the wireframes destination folder
                WireGenerator.Utility.DeleteDestinationFolder(destinationPath);
                //then, copy the supporting js, css and image file assets to the wireframes destination folder
                WireGenerator.Utility.CopyAssets(@"assets", destinationPath + "assets/");

                //generate home page
                content = this.ConstructPageHeader();
                content = this.ConstructPageNavigation();
                content = content.Append("<div class=\"container-fluid fullheight\">");
                content = content.Append("<div class=\"row\">");
                content = this.ConstructPageFooter();
                this.CreateFile("index.html", content);

                //check for entity data available and generate the entity pages
                //load entities
                IEnumerable<XElement> entities = from el in appSchema.Elements("Entities").Elements("Entity") select el;
                if (entities.Count() > 0)
                {
                    foreach (var entity in entities)
                    {
                        content.Clear();
                        //generate entity list page
                        content = this.ConstructPageHeader();
                        content = this.ConstructPageNavigation();
                        content = content.Append("<div class=\"container-fluid fullheight\">");
                        content = content.Append("<div class=\"row\">");
                        content = this.ConstructListContent(entity.Attribute("name").Value);
                        content = this.ConstructPageFooter();
                        this.CreateFile(entity.Attribute("name").Value + "_list.html", content);

                        content.Clear();
                        //generate entity add page
                        lineItemModelSnippetAdded = false;
                        content = this.ConstructPageHeader();
                        content = this.ConstructPageNavigation();
                        content = content.Append("<div class=\"container-fluid fullheight\">");
                        content = content.Append("<div class=\"row\">");
                        content = this.ConstructAddContent(entity.Attribute("name").Value);
                        content = this.ConstructPageFooter();
                        this.CreateFile(entity.Attribute("name").Value + "_add.html", content);
                    }
                }
            }

            MessageBox.Show("done!");
        }
        #endregion

        #region private StringBuilder ConstructPageNavigation()
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private StringBuilder ConstructPageNavigation()
        {
            appModel = new AppModel();
            appModel.Name = appSchema.Element("Name").Value;
            appModel.LoginUser = appSchema.Element("LoginUser").Value;

            //load the navigation 
            IEnumerable<XElement> navMenuItems = from el in appSchema.Elements("Navigation").Elements("MenuItem") select el;
            WireGenerator.Model.MenuItem menuItem;
            appModel.Navigation = new List<WireGenerator.Model.MenuItem>();

            foreach (XElement el in navMenuItems.ToList())
            {
                menuItem = new WireGenerator.Model.MenuItem();
                menuItem.Name = el.Attribute("name").Value;
                menuItem.LinkToUrl = (el.Attribute("linkTo") != null) ? el.Attribute("linkTo").Value : string.Empty;
                menuItem.MenuSubItems = new List<MenuSubItem>();
                IEnumerable<XElement> menuSubItems = el.Descendants();
                foreach (XElement subel in menuSubItems)
                {
                    menuSubItem = new MenuSubItem();
                    menuSubItem.SubItem = subel.Value;
                    if (subel.HasAttributes)
                        menuSubItem.LinkToURL = subel.Attribute("linkTo").Value + "_list.html";
                    menuItem.MenuSubItems.Add(menuSubItem);
                }
                appModel.Navigation.Add(menuItem);
            }


            #region top nav menu
            content = content.Append("<nav id=\"NavBar\" class=\"navbar navbar-default navbar-fixed-top\" role=\"navigation\">");
            content = content.Append("<div class=\"container-fluid\">");
            content = content.Append("<div class=\"wmnavbar\">");
            content = content.Append("<div class=\"navbar-header\">");
            content = content.Append("<span type=\"button\" class=\"navbar-toggle\" data-toggle=\"collapse\" data-target=\"#bs-example-navbar-collapse-1\">");
            content = content.Append("<i class=\"fa fa-bars fa-lg\"></i>");
            content = content.Append("</span>");
            content = content.Append("<span id=\"appname\" class=\"navbar-brand brandfont\">" + appModel.Name + "</span>");
            content = content.Append("</div>");

            content = content.Append("<div class=\"collapse navbar-collapse\" id=\"bs-example-navbar-collapse-1\">");
            content = content.Append("<ul id=\"Ul2\" class=\"nav navbar-nav\">");
            counter = 1;
            foreach (var nav in appModel.Navigation)
            {
                if (!string.IsNullOrEmpty(nav.LinkToUrl))
                {
                    content = content.Append("<li id=\"home\"><a href=\"" + nav.LinkToUrl + "\">" + nav.Name + " </a>");
                }
                else
                {
                    content = content.Append("<li id=\"li" + counter + "\" class=\"dropdown\"><a class=\"dropdown-toggle\" data-hover=\"dropdown\" data-toggle=\"dropdown\" href=\"#\">" + nav.Name + " <b class=\"caret\"></b></a>");
                    content = content.Append("<ul class=\"dropdown-menu\">");
                    foreach (var submenu in nav.MenuSubItems)
                        content = content.Append("<li><a href=\"" + submenu.LinkToURL + "\"><i class=\"fa fa-list-alt fa-fw\"></i>&nbsp;" + submenu.SubItem + "</a></li>");
                    content = content.Append("</ul>");
                    content = content.Append("</li>");
                    counter++;
                }
            }
            content = content.Append("</ul>");
            #endregion

            #region user quick links
            content = content.Append("<ul class=\"nav navbar-nav navbar-right\">");
            content = content.Append("<li class=\"dropdown\">");
            content = content.Append("<a href=\"#\" class=\"dropdown-toggle\" data-hover=\"dropdown\" data-toggle=\"dropdown\">" + appModel.LoginUser + " <b class=\"caret\"></b></a>");
            content = content.Append("<ul class=\"dropdown-menu\">");
            content = content.Append("<li><a href=\"javascript:ShowBookmarks();\"><i class=\"fa fa-thumb-tack fa-fw\"></i>&nbsp;My Bookmarks</a></li>");
            content = content.Append("<li><a href=\"javascript:ShowChangeMyPassword();\"><i class=\"fa fa-key fa-fw\"></i>&nbsp;Change My Password</a></li>");
            content = content.Append("<li class=\"divider\"></li>");
            content = content.Append("<li><a href=\"javascript:LogOff();\"><i class=\"fa fa-sign-out fa-fw\"></i>&nbsp;Logout</a></li>");
            content = content.Append("</ul>");
            content = content.Append("</li>");
            content = content.Append("</ul>");
            content = content.Append("</div>");
            content = content.Append("</div>");
            content = content.Append("</div>");
            content = content.Append("</nav>");
            #endregion

            return content;
        }
        #endregion

        #region private StringBuilder ConstructPageHeader()
        private StringBuilder ConstructPageHeader()
        {
            content = content.Append("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\">");
            content = content.Append("<html class=\"no-js\" lang=\"en\">");
            content = content.Append("<head>");
            content = content.Append("<meta charset=\"utf-8\" />");
            content = content.Append("<title></title>");
            content = content.Append("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">");
            content = content.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");

            content = content.Append("<link href='" + destinationPath + "assets/css/fontawesome/css/font-awesome.min.css' rel='stylesheet' />");
            content = content.Append("<link href='" + destinationPath + "assets/css/jqueryui/jquery-ui-1.10.4.custom.min.css' rel='stylesheet' />");
            content = content.Append("<link href='" + destinationPath + "assets/Bootstrap/css/bootstrap.css' rel='stylesheet' />");
            content = content.Append("<link href='" + destinationPath + "assets/Bootstrap/Bootstrap-datepicker.css' rel='stylesheet' />");
            content = content.Append("<link href='" + destinationPath + "assets/Bootstrap/bootstrap.wiremakertheme.css' rel='stylesheet' />");
            content = content.Append("<link href='" + destinationPath + "assets/css/wiremaker-main.css' rel='stylesheet' />");

            content = content.Append("<script type='text/javascript' src='" + destinationPath + "assets/js/jquery/1.9.1/jquery.min.js'></script>");
            content = content.Append("<script type='text/javascript' src='" + destinationPath + "assets/js/jquery/ui/jquery-ui-1.10.4.custom.min.js'></script>");
            content = content.Append("<script type='text/javascript' src='" + destinationPath + "assets/Bootstrap/js/bootstrap.min.js'></script>");
            content = content.Append("<script type='text/javascript' src='" + destinationPath + "assets/Bootstrap/Bootstrap-datepicker.js'></script>");
            content = content.Append("<script type='text/javascript' src='" + destinationPath + "assets/Bootstrap/bootstrap-hover-dropdown.js'></script>");
            content = content.Append("<script type='text/javascript' src='" + destinationPath + "assets/js/common.js'></script>");
            content = content.Append("</head>");
            content = content.Append("<body class=\"wmbody\">");

            return content;
        }
        #endregion

        #region private StringBuilder ConstructListContent(string name)
        private StringBuilder ConstructListContent(string name)
        {
            XElement entity = ((IEnumerable<XElement>)from el in appSchema.Elements("Entities").Elements("Entity").Where(e => e.Attribute("name").Value == name) select el).First();
            IEnumerable<XElement> actions = from a in entity.Elements("ListActions").Descendants() select a;
            XElement listPropertiesElement = entity.Element("ListFields");
            IEnumerable<XElement> listFields = from a in entity.Elements("ListFields").Descendants() select a;
            IEnumerable<XElement> listData = from a in entity.Elements("ListData").Elements("Data") select a;

            content = content.Append("<div class=\"container-fluid fullheight\">");
            content = content.Append("<div class=\"row\">");
            content = content.Append("<div class=\"col-md-2 sidebar itemsidebar\" style=\"height: 679px;\">");

            //action items on the page
            if (actions.Count() > 0)
            {
                content = content.Append("<div class=\"panel panel-default\">");
                content = content.Append("<ul id=\"actionmenu\" class=\"nav nav-pills nav-stacked\">");
                foreach (XElement action in actions.ToList())
                {
                    if (action.HasAttributes)
                        content = content.Append("<li><a href=\"" + action.Attribute("linkTo").Value + "_add.html\"><i class=\"fa fa-arrow-right\"></i>&nbsp;" + action.Value + "</a></li>");
                    else
                        content = content.Append("<li><a href=\"\"><i class=\"fa fa-arrow-right\"></i>&nbsp;" + action.Value + "</a></li>");
                }
                content = content.Append("</ul>");
                content = content.Append("</div>");
            }

            //workflow
            if (entity.Elements("Workflow").Descendants().Count() > 0)
            {
                content = content.Append("<div class=\"\">");
                content = content.Append("<div id=\"pagemenu\" href=\"javascript:void(0);\" class=\"list-group\">");
                content = content.Append("<a class=\"list-group-item active\" href=\"javascript:ShowWorkFlowModel('" + entity.Attribute("title").Value + "','" + entity.Elements("Workflow").First().Attribute("name").Value + "')\" data-gridtemplate=\"\" data-pagemenuid=\"\">" + entity.Elements("Workflow").First().Attribute("name").Value + " <i class=\"fa fa-arrow-right\"></i></a>");
                content = content.Append("</div>");
                content = content.Append("</div>");

                content = content.Append("<div id=\"workflowchangemodal\" class=\"modal fade modal-xl\">");
                content = content.Append("<div class=\"modal-dialog\">");
                content = content.Append("<div class=\"modal-content\">");
                content = content.Append("<div class=\"modal-header\">");
                content = content.Append("<div id=\"workflowchangemodaltitle\" class=\"modaltitle\"></div>");
                content = content.Append("</div>");
                content = content.Append("<div class=\"modal-body\">");
                content = content.Append("<div class=\"row\">");
                content = content.Append("<div class=\"col-md-5\">");
                content = content.Append("<div class=\"row\">&nbsp;</div>");
                content = content.Append("<div class=\"row\">&nbsp;</div>");
                content = content.Append("<div class=\"row\">&nbsp;</div>");
                content = content.Append("<div class=\"row\">&nbsp;</div>");
                content = content.Append("<div class=\"row\">&nbsp;</div>");
                content = content.Append("<div class=\"row\">&nbsp;</div>");
                content = content.Append("<div class=\"row\">&nbsp;</div>");
                content = content.Append("<div class=\"row\">&nbsp;</div>");
                content = content.Append("</div>");
                content = content.Append("</div>");
                content = content.Append("</div>");
                content = content.Append("<div class=\"modal-footer\">");
                content = content.Append("<a data-dismiss=\"modal\" href=\"#\" class=\"btn btn-danger btn-sm pull-left\"><i class=\"fa fa-times\"></i>&nbsp;Cancel</a>");
                content = content.Append("<a class=\"btn btn-primary btn-sm pull-right\" href=\"javascript:SaveSequence();\"><i class=\"fa fa-save\"></i> Save</a>");
                content = content.Append("</div>");
                content = content.Append("</div>");
                content = content.Append("</div>");
                content = content.Append("</div>");
            }

            content = content.Append("</div>");
            content = content.Append("<div class=\"col-md-10 maincontent\">");
            content = content.Append("<div class=\"row\">");
            content = content.Append("<div class=\"page-header\">");
            content = content.Append("<div class=\"page-header-title\">");
            content = content.Append(entity.Attribute("title").Value);
            content = content.Append("<div class=\"input-group searchform\">");
            content = content.Append("<input id=\"txtSearch\" type=\"text\" class=\"form-control\" placeholder=\"Search\">");
            content = content.Append("<span class=\"input-group-addon\" id=\"txtSearchButton\"><span class=\"glyphicon glyphicon-search\"></span></span>");
            content = content.Append("</div>");
            content = content.Append("</div>");

            content = content.Append("<div class=\"page-header-subtitle\"></div>");
            content = content.Append("</div>");
            content = content.Append("</div>");

            content = content.Append("<div class=\"row\">");
            content = content.Append("<div class=\"col-md-12\">");

            content = content.Append("</div>");
            content = content.Append("</div>");
            content = content.Append("<div class=\"col-md-12\">");


            if (listFields.Count() > 0)
            {
                content = content.Append("<div id=\"DataTable\" class=\"datatable\">");
                if (entity.Elements("Workflow").Descendants().Count() > 0)
                {
                    content = content.Append("<div class=\"panel panel-default\">");
                    content = content.Append("<div class=\"panel-heading tabletoolbar\">");
                    content = content.Append("<div class=\"pull-left\">");

                    content = content.Append("<select class=\"form-control\" id=\"workflow_select\" onchange=\"WorkFlowChange(this)\">");
                    counter = 0;
                    foreach (XElement option in entity.Elements("Workflow").Descendants().ToList())
                        content = content.Append("<option value=\"workflow_" + counter + "\">" + option.Value + "</option>");
                    content = content.Append("</select>");
                    content = content.Append("</div>");
                    content = content.Append("</div>");

                }
                content = content.Append("<div class=\"row\">&nbsp;</div>");
                content = content.Append("<table class=\"table table-responsive table-condensed table-striped table-smallfont\">");
                content = content.Append("<thead>");
                content = content.Append("<tr>");
                if (listPropertiesElement.Attribute("WithSelector") != null)
                    if (listPropertiesElement.Attribute("WithSelector").Value == "true")
                        content = content.Append("<th class=\"text-center\"><a href=\"javascript:void(0);\"><i id=\"CheckAll\" class=\"phaseselectall fa fa-square-o fa-lg\"></i></a></th>");

                counter = 0;
                foreach (XElement property in listFields.ToList())
                {
                    if (counter == 0)
                    {
                        content = content.Append("<th id=\"\">" + property.Value + "<span>&nbsp;</span><i class=\"iconsortdir fa fa-sort-up\"></i></th>");
                        counter++;
                    }
                    else
                        content = content.Append("<th id=\"\">" + property.Value + "<span>&nbsp;</span><i class=\"iconsortdir fa fa-sort-down\"></i></i></th>");
                }
                content = content.Append("<th id=\"\">Edit</th>");
                content = content.Append("<th id=\"\">Delete</th>");
                content = content.Append("</tr>");
                content = content.Append("</thead>");
                content = content.Append("<tbody>");

                if (listData.Count() > 0)
                {
                    foreach (XElement data in listData.ToList())
                    {
                        IEnumerable<XElement> columns = from c in data.Descendants() select c;
                        content = content.Append("<tr>");
                        if (listPropertiesElement.Attribute("WithSelector") != null)
                            if (listPropertiesElement.Attribute("WithSelector").Value == "true")
                                content = content.Append("<td class=\"text-center\"><a href=\"javascript:void(0);\"><i id=item\"" + counter + "\" class=\"phaseselect fa fa-square-o fa-lg\"></i></a></td>");
                        foreach (XElement column in columns.ToList())
                            content = content.Append("<td>" + column.Value + "</td>");
                        content = content.Append("<td><i class=\"fa fa-edit fa-fw\"></i></td>");
                        content = content.Append("<td><i class=\"fa fa-trash-o fa-lg\"></i></td>");
                        content = content.Append("</tr>");
                    }
                }
                content = content.Append("</tbody>");
                content = content.Append("</table>");
                content = content.Append("</div>");

                if (entity.Elements("Workflow").Descendants().Count() > 0)
                {
                    content = content.Append("</div>");
                }

                content = content.Append("<div id=\"Pager\">");
                content = content.Append("<ul class=\"pagination\">");
                content = content.Append("<li class=\"arrow unavailable\"><a id=\"larrow\" href=\"\">«</a></li>");
                content = content.Append("<li class=\"active\"><a href=\"\">1</a></li>");
                content = content.Append("<li><a href=\"\">2</a></li>");
                content = content.Append("<li><a href=\"\">3</a></li>");
                content = content.Append("<li><a href=\"\">4</a></li>");
                content = content.Append("<li><a href=\"\">5</a></li>");
                content = content.Append("<li><a href=\"\">6</a></li>");
                content = content.Append("<li class=\"arrow unavailable\"><a id=\"rarrow\" href=\"\">»</a></li>");
                content = content.Append("</ul>");
                content = content.Append("</div>");
            }


            content = content.Append("</div>");
            content = content.Append("</div>");
            content = content.Append("</div>");
            content = content.Append("</div>");
            return content;
        }
        #endregion

        #region private StringBuilder ContructPageFooter()
        private StringBuilder ConstructPageFooter()
        {
            content = content.Append("</div>");
            content = content.Append("</div>");
            content = content.Append("<div class=\"container-fluid\">");
            content = content.Append("<div class=\"row\">");
            content = content.Append("<div class=\"col-md-12 footer\">");
            content = content.Append("<span class=\"pull-left\">Copyright <span class=\"sc-copy\">&copy;</span>2014.</span>");
            content = content.Append("<span class=\"pull-right\" data-toggle=\"tooltip\" data-placement=\"top\" title=\"Build\" id=\"appversion\"></span>");
            content = content.Append("</div>");
            content = content.Append("</div>");
            content = content.Append("</div>");
            content = content.Append("</body>");
            content = content.Append("</html>");
            return content;
        }
        #endregion

        #region private CreateFile(string filename, StringBuilder content)
        private void CreateFile(string fileName, StringBuilder content)
        {
            //create html file with content to the destination path
            string filePath = System.IO.Path.Combine(destinationPath, fileName);
            StreamWriter sw = new StreamWriter(filePath, true);
            sw.Write(content.ToString());
            sw.Close();
        }
        #endregion

        #region StringBuilder ConstructAddContent(string name)
        private StringBuilder ConstructAddContent(string name)
        {
            XElement entity = ((IEnumerable<XElement>)from el in appSchema.Elements("Entities").Elements("Entity").Where(e => e.Attribute("name").Value == name) select el).First();
            IEnumerable<XElement> zone1Elements = from a in entity.Elements("AddFields").Elements("Section").Where(a => a.Attribute("zone").Value == "1") select a;
            IEnumerable<XElement> zone2Elements = from a in entity.Elements("AddFields").Elements("Section").Where(a => a.Attribute("zone").Value == "2") select a;

            content = content.Append("<div class=\"container-fluid fullheight\">");
            content = content.Append("<div class=\"row\">");
            content = content.Append("<div class=\"col-md-2 sidebar itemsidebar\" style=\"height: 679px;\">");
            content = content.Append("<div id=\"pagemenudiv\" style=\"top: 5px;\">");
            content = content.Append("<ul id=\"SideBarMenu\" class=\"nav nav-pills nav-stacked sidebarmenu\">");
            content = content.Append("<li><a class=\"sidebarmenuitem\" href=\"javascript:history.back();\"><i class=\"fa fa-arrow-left fa-fw\"></i>Cancel</a></li>");
            content = content.Append("<li><a class=\"sidebarmenuitem\" href=\"\"><i class=\"fa fa-save fa-fw\"></i>Save</a></li>");
            content = content.Append("</ul>");
            content = content.Append("</div>");
            content = content.Append("</div>");
            content = content.Append("<div class=\"col-md-10 maincontent\">");
            content = content.Append("<div class=\"row\">");
            content = content.Append("<div class=\"page-header\">");
            content = content.Append("<div class=\"page-header-title\">");
            content = content.Append("Add " + entity.Attribute("title").Value);
            content = content.Append("</div>");
            content = content.Append("<div class=\"page-header-subtitle\"></div>");
            content = content.Append("</div>");
            content = content.Append("</div>");

            content = content.Append("<div class=\"row\">");
            content = content.Append("<div class=\"col-md-7\">");
            foreach (XElement section in zone1Elements)
            {
                content = content.Append("<div class=\"panel panel-default\">");
                content = content.Append("<div class=\"panel-heading juneoedit\">" + section.Attribute("name").Value + "</div>");
                content = content.Append("<div class=\"panel-body juneoedit\">");
                foreach (XElement field in section.Descendants())
                    content = content.Append(this.FormControlSnippet(Convert.ToInt32(field.Attribute("type").Value), field.Attribute("label").Value));
                content = content.Append("</div>");
                content = content.Append("</div>");

            }
            content = content.Append("</div>");
            content = content.Append("<div class=\"col-md-5\">");
            foreach (XElement section in zone2Elements)
            {
                content = content.Append("<div class=\"panel panel-default\">");
                content = content.Append("<div class=\"panel-heading juneoedit\">Label</div>");
                content = content.Append("<div class=\"panel-body juneoedit\">");

                foreach (XElement field in section.Descendants("Field"))
                {
                    if (field.Attribute("type").Value == "5")
                    {
                        //for checkbox
                        IEnumerable<string> options = from a in field.Elements("Options").Descendants() select a.Attribute("label").Value;
                        content = content.Append(this.FormControlSnippet(Convert.ToInt32(field.Attribute("type").Value), field.Attribute("label").Value, options.ToList()));
                    }
                    else
                    {
                        content = content.Append(this.FormControlSnippet(Convert.ToInt32(field.Attribute("type").Value), field.Attribute("label").Value));
                    }
                }
                content = content.Append("</div>");
                content = content.Append("</div>");

            }
            content = content.Append("</div>");
            content = content.Append("</div>");
            content = content.Append("</div>");
            content = content.Append("</div>");

            return content;
        }
        #endregion

        #region private StringBuilder FormControlSnippet(int controlType, string label)
        private StringBuilder FormControlSnippet(int controlType, string label = "", List<string> options = null)
        {
            StringBuilder controlSnippet = new StringBuilder();
            string id;

            switch (controlType)
            {
                case (int)FormControlTypes.TextBox:
                    {
                        controlSnippet = controlSnippet.Append("<div class=\"row\">");
                        controlSnippet = controlSnippet.Append("<div class=\"col-md-8 form-group\">");
                        controlSnippet = controlSnippet.Append("<label for=\"\">" + label + "</label>");
                        controlSnippet = controlSnippet.Append("<input class=\"form-control\" type=\"text\" id=\"\">");
                        controlSnippet = controlSnippet.Append("</div>");
                        controlSnippet = controlSnippet.Append("</div>");
                        break;
                    }
                case (int)FormControlTypes.MultiLine:
                    {
                        controlSnippet = controlSnippet.Append("<div class=\"row\">");
                        controlSnippet = controlSnippet.Append("<div class=\"col-md-6 form-group\">");
                        controlSnippet = controlSnippet.Append("<label for=\"\">" + label + "</label>");
                        controlSnippet = controlSnippet.Append("<textarea class=\"form-control\" id=\"\" rows=\"2\"></textarea>");
                        controlSnippet = controlSnippet.Append("</div>");
                        controlSnippet = controlSnippet.Append("</div>");
                        break;
                    }
                case (int)FormControlTypes.Content:
                    {
                        controlSnippet = controlSnippet.Append("<div class=\"row\">");
                        controlSnippet = controlSnippet.Append("<div class=\"col-md-12 form-group\">");
                        controlSnippet = controlSnippet.Append("<label for=\"\">" + label + "</label>");
                        controlSnippet = controlSnippet.Append("<textarea class=\"form-control\" id=\"\" rows=\"10\"></textarea>");
                        controlSnippet = controlSnippet.Append("</div>");
                        controlSnippet = controlSnippet.Append("</div>");
                        break;
                    }
                case (int)FormControlTypes.DropDown:
                    {
                        controlSnippet = controlSnippet.Append("<div class=\"row\">");
                        controlSnippet = controlSnippet.Append("<div class=\"col-md-8 form-group\">");
                        controlSnippet = controlSnippet.Append("<label for=\"\">" + label + "</label>");
                        controlSnippet = controlSnippet.Append("<select class=\"form-control\" id=\"\">");
                        controlSnippet = controlSnippet.Append("<option value=\"0\" selected=\"\">Select " + label + "</option>");
                        controlSnippet = controlSnippet.Append("</select>");
                        controlSnippet = controlSnippet.Append("</div>");
                        controlSnippet = controlSnippet.Append("</div>");
                        break;
                    }
                case (int)FormControlTypes.CheckBox:
                    {
                        controlSnippet = controlSnippet.Append("<div class=\"row\">");
                        controlSnippet = controlSnippet.Append("<div class=\"col-md-12\">");
                        controlSnippet = controlSnippet.Append("<div class=\"btn-group\">");
                        foreach (var option in options)
                        {
                            id = option.Replace(" ", string.Empty);
                            controlSnippet = controlSnippet.Append("<a class=\"btn btn-sm btn-default\" id=\"" + id + "Btn\" href=\"javascript:SwitchCheckBoxButton('" + id + "');\"><i id=\"" + id + "Text\" class=\"fa fa-lg valign fa-check-square-o\"></i>&nbsp;" + option + "</a>");
                        }
                        controlSnippet = controlSnippet.Append("</div>");
                        controlSnippet = controlSnippet.Append("</div>");
                        controlSnippet = controlSnippet.Append("</div>");
                        break;
                    }
                case (int)FormControlTypes.LineItems:
                    {
                        controlSnippet = controlSnippet.Append("<div class=\"panel panel-default\">");
                        controlSnippet = controlSnippet.Append("<div class=\"panel-heading\">");
                        controlSnippet = controlSnippet.Append(label + "s");
                        controlSnippet = controlSnippet.Append("<a id=\"btnShowLineItems\" class=\"pull-right btn btn-primary btn-xs editrights\" href=\"javascript:ShowLineItemModel('" + label + "');\"><i class=\"fa fa-plus\"></i>&nbsp;Add " + label + "</a>");
                        controlSnippet = controlSnippet.Append("</div>");
                        controlSnippet = controlSnippet.Append("<div class=\"panel-body\">");
                        controlSnippet = controlSnippet.Append("<div class=\"row\">");
                        controlSnippet = controlSnippet.Append("<div class=\"col-md-12\">");
                        controlSnippet = controlSnippet.Append("<ul id=\"\" class=\"itemlistul\">");
                        controlSnippet = controlSnippet.Append("<li id=\"\"><span class=\"smallgraytext\">- empty -</span></li></ul>");
                        controlSnippet = controlSnippet.Append("</div>");
                        controlSnippet = controlSnippet.Append("</div>");
                        controlSnippet = controlSnippet.Append("</div>");
                        controlSnippet = controlSnippet.Append("</div>");
                        if (!lineItemModelSnippetAdded)
                        {
                            controlSnippet = controlSnippet.Append("<div id=\"lineitemmodal\" class=\"modal fade modal-50\" aria-hidden=\"true\" style=\"display: none;\">");
                            controlSnippet = controlSnippet.Append("<div class=\"modal-dialog\">");
                            controlSnippet = controlSnippet.Append("<div class=\"modal-content\">");
                            controlSnippet = controlSnippet.Append("<div class=\"modal-header\">");
                            controlSnippet = controlSnippet.Append("<div id=\"lineitemmodaltitle\" class=\"modaltitle\"></div>");
                            controlSnippet = controlSnippet.Append("</div>");
                            controlSnippet = controlSnippet.Append("<div class=\"modal-body\">");
                            controlSnippet = controlSnippet.Append("<div class=\"row\">");
                            controlSnippet = controlSnippet.Append("<div class=\"col-md-3\">");
                            controlSnippet = controlSnippet.Append("<div class=\"row\">&nbsp;</div>");
                            controlSnippet = controlSnippet.Append("<div class=\"row\">&nbsp;</div>");
                            controlSnippet = controlSnippet.Append("<div class=\"row\">&nbsp;</div>");
                            controlSnippet = controlSnippet.Append("<div class=\"row\">&nbsp;</div>");
                            controlSnippet = controlSnippet.Append("<div class=\"row\">&nbsp;</div>");
                            controlSnippet = controlSnippet.Append("<div class=\"row\">&nbsp;</div>");
                            controlSnippet = controlSnippet.Append("<div class=\"row\">&nbsp;</div>");
                            controlSnippet = controlSnippet.Append("<div class=\"row\">&nbsp;</div>");
                            controlSnippet = controlSnippet.Append("</div>");
                            controlSnippet = controlSnippet.Append("</div>");
                            controlSnippet = controlSnippet.Append("</div>");
                            controlSnippet = controlSnippet.Append("<div class=\"modal-footer\">");
                            controlSnippet = controlSnippet.Append("<a data-dismiss=\"modal\" href=\"#\" class=\"btn btn-danger btn-sm pull-left\"><i class=\"fa fa-times\"></i>&nbsp;Cancel</a>");
                            controlSnippet = controlSnippet.Append("<a class=\"btn btn-primary btn-sm pull-right\" href=\"javascript:SaveSequence();\"><i class=\"fa fa-save\"></i> Add</a>");
                            controlSnippet = controlSnippet.Append("</div>");
                            controlSnippet = controlSnippet.Append("</div>");
                            controlSnippet = controlSnippet.Append("</div>");
                            controlSnippet = controlSnippet.Append("</div>");
                            lineItemModelSnippetAdded = true;
                        }
                        break;
                    }
            }
            return controlSnippet;
        }
        #endregion

        #region void SaveConfiguration_Click
        private void SaveConfiguration_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //root and application properties
            var root = new XElement("App");
            root.Add(new XElement("Name", this.App_Name.Text));
            root.Add(new XElement("LoginUser", this.App_UserName.Text));


            if (appModel != null)
            {
                //navigation
                if (appModel.Navigation != null)
                {
                    var navigation = new XElement("Navigation");
                    XElement menu, submenu;
                    foreach (var menuItem in appModel.Navigation)
                    {
                        menu = new XElement("MenuItem");
                        menu.SetAttributeValue("name", menuItem.Name);
                        menu.SetAttributeValue("linkTo", menuItem.LinkToUrl);

                        if (menuItem.MenuSubItems != null)
                        {
                            foreach (var submenuItem in menuItem.MenuSubItems)
                            {
                                submenu = new XElement("SubMenuItem");
                                submenu.Value = submenuItem.SubItem;
                                submenu.SetAttributeValue("linkTo", submenuItem.LinkToURL);
                                menu.Add(submenu);
                            }
                        }
                        navigation.Add(menu);
                    }
                    root.Add(navigation);
                }

                //entities
                if (appModel.Entities.Count > 0)
                {
                    var xEntities = new XElement("Entities");
                    XElement xEntity, xListFields, xListActions, xAddFields, xSection, xWorkflow, xElement;
                    foreach (var entity in appModel.Entities)
                    {
                        xEntity = new XElement("Entity");
                        xEntity.SetAttributeValue("name", entity.Name);
                        xEntity.SetAttributeValue("title", entity.Title);
                        xEntity.SetAttributeValue("hasSearch", entity.HasSearch);

                        //ListFields
                        if (entity.ListScreenFields != null)
                        {
                            if (entity.ListScreenFields.Count > 0)
                            {
                                xListFields = new XElement("ListFields");
                                xListFields.SetAttributeValue("WithSelector", entity.IsListScreenWithSelector);
                                foreach (var listField in entity.ListScreenFields)
                                {
                                    xElement = new XElement("Field");
                                    xElement.Value = listField.FieldName;
                                    xListFields.Add(xElement);
                                }
                                xEntity.Add(xListFields);
                            }
                        }
                        //ListActions
                        if (entity.ListScreenActions != null)
                        {
                            if (entity.ListScreenActions.Count > 0)
                            {
                                xListActions = new XElement("ListActions");
                                foreach (var listAction in entity.ListScreenActions)
                                {
                                    xElement = new XElement("Action");
                                    xElement.Value = listAction.ActionName;
                                    xListActions.Add(xElement);
                                }
                                xEntity.Add(xListActions);
                            }
                        }
                        //AddActions
                        if (entity.AddScreenSections != null)
                        {
                            if (entity.AddScreenSections.Count > 0)
                            {
                                xAddFields = new XElement("AddFields");
                                foreach (var section in entity.AddScreenSections)
                                {
                                    xSection = new XElement("Section");
                                    xSection.SetAttributeValue("name", section.SectionName);
                                    xSection.SetAttributeValue("zone", section.Zone);

                                    foreach (var field in section.Fields)
                                    {
                                        xElement = new XElement("Field");
                                        xElement.SetAttributeValue("type", field.Type + 1);
                                        xElement.SetAttributeValue("label", field.FieldName);
                                        xSection.Add(xElement);
                                    }
                                    xAddFields.Add(xSection);
                                }
                                xEntity.Add(xAddFields);
                            }
                        }

                        //workflow
                        if (entity.Workflow != null)
                        {
                            if (entity.Workflow.Count() > 0)
                            {
                                xWorkflow = new XElement("Workflow");
                                xWorkflow.SetAttributeValue("name", entity.WorkflowAliasName);
                                foreach (var phase in entity.Workflow)
                                {
                                    xElement = new XElement("Phase");
                                    xElement.Value = phase.PhaseName;
                                    xWorkflow.Add(xElement);
                                }
                                xEntity.Add(xWorkflow);
                            }
                        }
                        xEntities.Add(xEntity);
                    }
                    root.Add(xEntities);
                }

            }
            //save xml
            root.Save("AppSchema_Generated.xml");
        }
        #endregion

        #region AddNav_Click
        private void AddNav_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var treeNode = new TreeViewItem();
            WireGenerator.Model.MenuItem menuItem;
            bool isValid = true;
            if (!chkIsSubMenu.IsChecked == true)
            {
                //for Menu items
                treeNode.Header = txtMenuName.Text.Trim();
                NavigationTreeView.Items.Add(treeNode);

                menuItem = new WireGenerator.Model.MenuItem();
                menuItem.Name = txtMenuName.Text.Trim();
                menuItem.LinkToUrl = (!string.IsNullOrEmpty(txtMenuLinkToEntity.Text)) ? txtMenuLinkToEntity.Text : string.Empty;
                appModel.Navigation.Add(menuItem);
            }
            else
            {
                //for Sub Menu items
                var selectedNode = NavigationTreeView.SelectedItem as TreeViewItem;
                if (selectedNode == null)
                {
                    MessageBox.Show("Select Menu Item");
                    isValid = false;
                }

                if (isValid)
                {
                    treeNode.Header = txtMenuName.Text;
                    selectedNode.Items.Add(treeNode);

                    menuItem = appModel.Navigation.Where(a => a.Name.Equals(selectedNode.Header.ToString())).First();

                    if (menuItem.MenuSubItems == null)
                    {
                        menuItem.MenuSubItems = new List<MenuSubItem>();
                    }
                    MenuSubItem menuSubItem = new MenuSubItem();
                    menuSubItem.SubItem = txtMenuName.Text;
                    if (!string.IsNullOrEmpty(txtMenuLinkToEntity.Text))
                        menuSubItem.LinkToURL = txtMenuLinkToEntity.Text;
                    menuItem.MenuSubItems.Add(menuSubItem);
                }
            }

        }
        #endregion

        #region RemoveMenuItem
        private void RemoveMenuItem(object sender, System.Windows.RoutedEventArgs e)
        {
            if (NavigationTreeView.SelectedItem != null)
            {
                var selectedNode = NavigationTreeView.SelectedItem as TreeViewItem;
                var parentNode = selectedNode.Parent as TreeViewItem;
                
                if (parentNode == null)
                {
                    var menu = appModel.Navigation.Where(a => a.Name == ((TreeViewItem)NavigationTreeView.SelectedItem).Header.ToString()).First();
                    appModel.Navigation.Remove(menu);
                    NavigationTreeView.Items.RemoveAt(NavigationTreeView.Items.IndexOf(NavigationTreeView.SelectedItem));
                }
                else
                {
                    var menu = appModel.Navigation.Where(a => a.Name == parentNode.Header.ToString()).First();
                    var subMenu = menu.MenuSubItems.Where(a => a.SubItem == ((TreeViewItem)NavigationTreeView.SelectedItem).Header.ToString()).First();
                    menu.MenuSubItems.Remove(subMenu);
                    parentNode.Items.RemoveAt(parentNode.Items.IndexOf(NavigationTreeView.SelectedItem));
                }
            }
        }
        #endregion

        #region AddEntity_Click
        private void AddEntity_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            bool isValid = true;

            if (lstEntities.Items.Contains(txtEntityName.Text.Trim()))
                isValid = false;

            if (isValid)
            {
                this.lstEntities.Items.Add(txtEntityName.Text);

                Entity entity = new Entity();
                entity.Name = txtEntityName.Text;
                entity.Title = txtEntityTitle.Text;
                appModel.Entities = new List<Entity>();
                appModel.Entities.Add(entity);
            }
        }
        #endregion

        #region RemoveEntity_Click
        private void RemoveEntity_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (lstEntities.SelectedItem != null)
            {
                var entity = appModel.Entities.Where(a => a.Name.Equals(lstEntities.SelectedItem.ToString())).First();
                appModel.Entities.Remove(entity);
                lstEntities.Items.RemoveAt(lstEntities.Items.IndexOf(lstEntities.SelectedItem));
            }
        }
        #endregion

        #region AddListField_Click
        private void AddListField_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Field field = new Field();
            if (!string.IsNullOrEmpty(txtListFieldName.Text.Trim()))
            {
                field.FieldName = txtListFieldName.Text.Trim();

                bool isValid = true;
                if (lstEntities.SelectedItem == null)
                {
                    MessageBox.Show("Select Entity to add field");
                    isValid = false;
                }

                if (lstListFields.Items.Contains(field.FieldName.Trim()))
                    isValid = false;

                if (isValid)
                {
                    var selectedEntityName = lstEntities.SelectedItem;
                    Entity entity = appModel.Entities.Where(a => a.Name.Equals(selectedEntityName)).First();
                    if (entity.ListScreenFields == null)
                        entity.ListScreenFields = new List<Field>();

                    entity.ListScreenFields.Add(field);
                    this.lstListFields.Items.Add(field.FieldName);
                }
            }
        }
        #endregion

        #region RemoveListField_Click
        private void RemoveListField_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (lstListFields.SelectedItem != null)
            {
                var entity = appModel.Entities.Where(a => a.Name.Equals(lstEntities.SelectedItem.ToString())).First();
                var field = entity.ListScreenFields.Where(f => f.FieldName == lstListFields.SelectedItem.ToString()).First();
                entity.ListScreenFields.Remove(field);
                lstListFields.Items.RemoveAt(lstListFields.Items.IndexOf(lstListFields.SelectedItem));
            }
        }
        #endregion

        #region AddListAction_Click
        private void AddListAction_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            WireGenerator.Model.Action action = new WireGenerator.Model.Action();

            if (!string.IsNullOrEmpty(txtListActionName.Text.Trim()))
            {
                action.ActionName = txtListActionName.Text.Trim();

                bool isValid = true;
                if (lstEntities.SelectedItem == null)
                {
                    MessageBox.Show("Select Entity to add action");
                    isValid = false;
                }

                if (lstListActions.Items.Contains(action.ActionName.Trim()))
                    isValid = false;

                if (isValid)
                {
                    var selectedEntityName = lstEntities.SelectedItem;
                    Entity entity = appModel.Entities.Where(a => a.Name.Equals(selectedEntityName)).First();
                    if (entity.ListScreenActions == null)
                        entity.ListScreenActions = new List<WireGenerator.Model.Action>();

                    entity.ListScreenActions.Add(action);
                    this.lstListActions.Items.Add(action.ActionName);
                }
            }
        }
        #endregion

        #region RemoveListAction_Click
        private void RemoveListAction_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (lstListActions.SelectedItem != null)
            {
                var entity = appModel.Entities.Where(a => a.Name.Equals(lstEntities.SelectedItem.ToString())).First();
                var action = entity.ListScreenActions.Where(f => f.ActionName == lstListActions.SelectedItem.ToString()).First();
                entity.ListScreenActions.Remove(action);
                lstListActions.Items.RemoveAt(lstListActions.Items.IndexOf(lstListActions.SelectedItem));
            }
        }
        #endregion

        #region AddSection_Click
        private void AddSection_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            bool isValid = true;
            if (lstEntities.SelectedItem == null)
            {
                MessageBox.Show("Select Entity to add field");
                isValid = false;
            }

            if (string.IsNullOrEmpty(txtSectionName.Text.Trim()))
                isValid = false;

            if (isValid)
            {
                var entity = appModel.Entities.Where(a => a.Name.Equals(lstEntities.SelectedItem.ToString())).First();
                if (entity.AddScreenSections != null)
                {
                    if (entity.AddScreenSections.Any(s => s.SectionName.Equals(txtSectionName.Text.Trim())))
                        isValid = false;
                }

                if (isValid)
                {
                    TreeViewItem treeItem = new TreeViewItem();
                    treeItem.Header = this.txtSectionName.Text.Trim();

                    WireGenerator.Model.Section section = new WireGenerator.Model.Section();
                    section.SectionName = txtSectionName.Text.Trim();
                    section.Zone = (cmbZone.SelectedIndex + 1).ToString();

                    if (entity.AddScreenSections == null)
                    {
                        entity.AddScreenSections = new List<WireGenerator.Model.Section>();
                        entity.AddScreenSections.Add(section);
                    }
                    else
                    {
                        var sectionExists = entity.AddScreenSections.Any(s => s.SectionName.Equals(txtSectionName.Text.Trim()));
                        if (!sectionExists)
                        {
                            entity.AddScreenSections.Add(section);
                        }
                    }
                    var treeNode = new TreeViewItem();
                    treeNode.Header = txtSectionName.Text.Trim();
                    AddScreenSectionFieldsTreeView.Items.Add(treeNode);
                }
            }

        }
        #endregion

        #region RemoveSection_Click
        private void RemoveSection_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (AddScreenSectionFieldsTreeView.SelectedItem != null)
            {
                var selectedNode = AddScreenSectionFieldsTreeView.SelectedItem as TreeViewItem;
                var parentNode = selectedNode.Parent as TreeViewItem;
                if (parentNode == null)
                {
                    var entity = appModel.Entities.Where(a => a.Name.Equals(lstEntities.SelectedItem.ToString())).First();
                    var section = entity.AddScreenSections.Where(a => a.SectionName == ((TreeViewItem)AddScreenSectionFieldsTreeView.SelectedItem).Header.ToString()).First();
                    entity.AddScreenSections.Remove(section);
                    AddScreenSectionFieldsTreeView.Items.RemoveAt(AddScreenSectionFieldsTreeView.Items.IndexOf(AddScreenSectionFieldsTreeView.SelectedItem));
                }
            }
        }
        #endregion

        #region Zone_Loaded
        private void Zone_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            List<string> zoneTypes = new List<string>();
            zoneTypes.Add("Left");
            zoneTypes.Add("Right");
            this.cmbZone.ItemsSource = zoneTypes;

            this.cmbZone.SelectedIndex = 0;
        }
        #endregion

        #region FieldTypes_Loaded
        private void FieldTypes_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            List<string> fieldTypes = new List<string>();
            fieldTypes.Add("Text Box");
            fieldTypes.Add("Multi Line Text Box");
            fieldTypes.Add("Drop Down Selection");
            fieldTypes.Add("Check Box");
            fieldTypes.Add("Line Items");
            fieldTypes.Add("Content");
            this.cmbAddScreenFieldType.ItemsSource = fieldTypes;

            this.cmbAddScreenFieldType.SelectedIndex = 0;
        }
        #endregion

        #region AddScreenAddField_Click
        private void AddScreenAddField_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            bool isValid = true;

            var selectedNode = AddScreenSectionFieldsTreeView.SelectedItem as TreeViewItem;
            if (selectedNode == null)
            {
                MessageBox.Show("Select Section Item");
                isValid = false;
            }

            if (isValid)
            {
                WireGenerator.Model.Section section;
                var entity = appModel.Entities.Where(a => a.Name.Equals(lstEntities.SelectedItem.ToString())).First();
                var entitySection = entity.AddScreenSections.Where(s => s.SectionName == selectedNode.Header.ToString()).First();

                if (entitySection.Fields != null)
                {
                    if (entitySection.Fields.Any(a => a.FieldName.Equals(txtAddScreenFieldName.Text)))
                        isValid = false;
                }

                if (isValid)
                {

                    if (entitySection.Fields == null)
                        entitySection.Fields = new List<Field>();

                    Field field = new Field();
                    field.FieldName = txtAddScreenFieldName.Text.Trim();
                    field.Type = cmbAddScreenFieldType.SelectedIndex;
                    section = new Model.Section();
                    entitySection.Fields.Add(field);

                    var treeNode = new TreeViewItem();
                    treeNode.Header = txtAddScreenFieldName.Text;
                    selectedNode.Items.Add(treeNode);
                }
            }
        }
        #endregion

        #region AddScreenRemove_Click
        private void AddScreenRemove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (AddScreenSectionFieldsTreeView.SelectedItem != null)
            {
                var selectedNode = AddScreenSectionFieldsTreeView.SelectedItem as TreeViewItem;
                var parentNode = selectedNode.Parent as TreeViewItem;
                if (parentNode != null)
                {
                    var entity = appModel.Entities.Where(a => a.Name.Equals(lstEntities.SelectedItem.ToString())).First();
                    var section = entity.AddScreenSections.Where(a => a.SectionName == parentNode.Header.ToString()).First();
                    var field = section.Fields.Where(f => f.FieldName == ((TreeViewItem)AddScreenSectionFieldsTreeView.SelectedItem).Header.ToString()).First();
                    section.Fields.Remove(field);
                    parentNode.Items.RemoveAt(parentNode.Items.IndexOf(AddScreenSectionFieldsTreeView.SelectedItem));
                }
            }
        }
        #endregion

        #region AddPhase_Click
        private void AddPhase_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            WorkflowPhase phase = new WorkflowPhase();

            if (!string.IsNullOrEmpty(txtPhaseName.Text.Trim()))
            {
                phase.PhaseName = txtPhaseName.Text.Trim();

                bool isValid = true;
                if (lstEntities.SelectedItem == null)
                {
                    MessageBox.Show("Select Entity to add Phase");
                    isValid = false;
                }

                if (lstPhases.Items.Contains(phase.PhaseName.Trim()))
                    isValid = false;

                if (isValid)
                {
                    var selectedEntityName = lstEntities.SelectedItem;
                    Entity entity = appModel.Entities.Where(a => a.Name.Equals(selectedEntityName)).First();
                    if (entity.Workflow == null)
                        entity.Workflow = new List<WorkflowPhase>();

                    entity.Workflow.Add(phase);
                    this.lstPhases.Items.Add(phase.PhaseName);
                }
            }
        }
        #endregion

        #region RemovePhase_Click
        private void RemovePhase_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (lstPhases.SelectedItem != null)
                lstPhases.Items.RemoveAt(lstPhases.Items.IndexOf(lstPhases.SelectedItem));
        }
        #endregion

    }
}