using System;
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
        private string destinationPath = "c:/WireFrames/";

        enum FormControlTypes : int
        {
            TextBox = 1,
            MultiLine = 2,
            RichTextBox = 3,
            DropDown = 4,
            CheckBox = 5
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();
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
            XElement listPropertiesElement = entity.Element("ListProperties");
            IEnumerable<XElement> listProperties = from a in entity.Elements("ListProperties").Descendants() select a;
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
                        content = content.Append("<li><a href=\"" + action.Attribute("linkTo").Value + "_add.html\"><i class=\"fa fa-plus fa-fw\"></i>&nbsp;" + action.Value + "</a></li>");
                    else
                        content = content.Append("<li><a href=\"\"><i class=\"fa fa-plus fa-fw\"></i>&nbsp;" + action.Value + "</a></li>");
                }
                content = content.Append("</ul>");
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

            if (listProperties.Count() > 0)
            {
                content = content.Append("<div id=\"DataTable\" class=\"datatable\">");
                content = content.Append("<table class=\"table table-responsive table-condensed table-striped table-smallfont\">");
                content = content.Append("<thead>");
                content = content.Append("<tr>");
                if (listPropertiesElement.Attribute("WithSelector") != null)
                    if (listPropertiesElement.Attribute("WithSelector").Value == "true")
                        content = content.Append("<th class=\"text-center\"><a href=\"javascript:void(0);\"><i id=\"CheckAll\" class=\"phaseselectall fa fa-square-o fa-lg\"></i></a></th>");

                counter = 0;
                foreach (XElement property in listProperties.ToList())
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
                                content = content.Append("<td class=\"text-center\"><a href=\"javascript:void(0);\"><i id=\"\" class=\"phaseselect fa fa-square-o fa-lg\"></i></a></td>");
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
            IEnumerable<XElement> zone1Elements = from a in entity.Elements("AddProperties").Elements("Section").Where(a => a.Attribute("zone").Value == "1") select a;
            IEnumerable<XElement> zone2Elements = from a in entity.Elements("AddProperties").Elements("Section").Where(a => a.Attribute("zone").Value == "2") select a;

            content = content.Append("<div class=\"container-fluid fullheight\">");
            content = content.Append("<div class=\"row\">");
            content = content.Append("<div class=\"col-md-2 sidebar itemsidebar\" style=\"height: 679px;\">");
            content = content.Append("<div id=\"pagemenudiv\" style=\"top: 5px;\">");
            content = content.Append("<ul id=\"SideBarMenu\" class=\"nav nav-pills nav-stacked sidebarmenu\">");
            content = content.Append("<li><a class=\"sidebarmenuitem\" href=\"javascript:ReturntoZoneList();\"><i class=\"fa fa-arrow-left fa-fw\"></i>Cancel</a></li>");
            content = content.Append("<li><a class=\"sidebarmenuitem\" href=\"javascript:SaveZone();\"><i class=\"fa fa-save fa-fw\"></i>Save</a></li>");
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
                        controlSnippet = controlSnippet.Append("<div class=\"col-md-12 form-group\">");
                        controlSnippet = controlSnippet.Append("<label for=\"\">" + label + "</label>");
                        controlSnippet = controlSnippet.Append("<textarea class=\"form-control\" id=\"\" rows=\"10\"></textarea>");
                        controlSnippet = controlSnippet.Append("</div>");
                        controlSnippet = controlSnippet.Append("</div>");
                        break;
                    }
                case (int)FormControlTypes.RichTextBox:
                    {
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
            }
            return controlSnippet;
        }
        #endregion
    }
}
