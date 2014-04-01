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
using WireGenerator.Model;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;

namespace WireGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        #region variables, enums, properties
        private XElement appConfig;
        private AppModel appModel, xAppModal;
        private MenuSubItem menuSubItem;
        private StringBuilder content = new StringBuilder();
        private int counter;
        private bool lineItemModelSnippetAdded;
        private string destinationPath;
        private string schemaAbsoluteFileName = string.Empty;
        private string schemaName = string.Empty;
        private string _previousEntity;

        readonly TreeViewItemModel rootNode = new TreeViewItemModel("Root");
        
        enum FormControlTypes : int
        {
            TextBox = 0,
            MultiLine = 1,
            DropDown = 2,
            CheckBox = 3,
            LineItems = 4,
            Content = 5
        }

        enum UserForms : int
        {
            AppMain = 1,
            AppNavigation = 2,
            AppEntities = 3
        }

        enum FieldDataTypes : int
        { 
            ShortString = 0,
            ShortInteger = 1,
            LongString = 2,
            LongInteger = 3
        }

        public string PreviousEntity
        {
            get { return _previousEntity; }
            set { _previousEntity = value; }
        }
        #endregion

        #region MainWindow
        public MainWindow()
        {
            InitializeComponent();

            appModel = new AppModel();
            appModel.Entities = new List<Entity>();
            appModel.Navigation = new List<WireGenerator.Model.MenuItem>();

            List<string> schemaNames = new List<string>();
            schemaNames.Add("New Configuration...");
            
            string assemblyPath = System.Reflection.Assembly.GetAssembly(typeof(MainWindow)).Location;
            string assemblyFolderPath = System.IO.Path.GetDirectoryName(assemblyPath);
            foreach (string file in Directory.GetFiles(assemblyFolderPath))
            {
                string fileName = System.IO.Path.GetFileName(file);
                if (file.Contains("_appconfig.xml"))
                    schemaNames.Add(fileName.Substring(0, fileName.Length - 14));
            }

            cmbSchemaNames.ItemsSource = schemaNames;
            cmbSchemaNames.SelectedIndex = 0;
        }
        #endregion

        #region GenerateOnClick(object sender, RoutedEventArgs e)
        private void GenerateOnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(schemaAbsoluteFileName))
            {
                //load app schema file
                appConfig = XElement.Load(schemaAbsoluteFileName);

                if (!appConfig.IsEmpty)
                {
                    //select folder for saving generated files
                    var dialog = new System.Windows.Forms.FolderBrowserDialog();
                    System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        destinationPath = dialog.SelectedPath + "/";

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
                        IEnumerable<XElement> entities = from el in appConfig.Elements("Entities").Elements("Entity") select el;
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
                }

                MessageBox.Show("Wire frames saved.");
            }
        }
        #endregion

        #region private StringBuilder ConstructPageNavigation()
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private StringBuilder ConstructPageNavigation()
        {
            xAppModal = new AppModel();
            xAppModal.Name = appConfig.Element("Name").Value;
            xAppModal.LoginUser = appConfig.Element("LoginUser").Value;

            //load the navigation 
            IEnumerable<XElement> navMenuItems = from el in appConfig.Elements("Navigation").Elements("MenuItem") select el;
            WireGenerator.Model.MenuItem menuItem;
            xAppModal.Navigation = new List<WireGenerator.Model.MenuItem>();

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
                xAppModal.Navigation.Add(menuItem);
            }


            #region top nav menu
            content = content.Append("<nav id=\"NavBar\" class=\"navbar navbar-default navbar-fixed-top\" role=\"navigation\">");
            content = content.Append("<div class=\"container-fluid\">");
            content = content.Append("<div class=\"wmnavbar\">");
            content = content.Append("<div class=\"navbar-header\">");
            content = content.Append("<span type=\"button\" class=\"navbar-toggle\" data-toggle=\"collapse\" data-target=\"#bs-example-navbar-collapse-1\">");
            content = content.Append("<i class=\"fa fa-bars fa-lg\"></i>");
            content = content.Append("</span>");
            content = content.Append("<span id=\"appname\" class=\"navbar-brand brandfont\">" + xAppModal.Name + "</span>");
            content = content.Append("</div>");

            content = content.Append("<div class=\"collapse navbar-collapse\" id=\"bs-example-navbar-collapse-1\">");
            content = content.Append("<ul id=\"Ul2\" class=\"nav navbar-nav\">");
            counter = 1;
            foreach (var nav in xAppModal.Navigation)
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
            content = content.Append("<a href=\"#\" class=\"dropdown-toggle\" data-hover=\"dropdown\" data-toggle=\"dropdown\">" + xAppModal.LoginUser + " <b class=\"caret\"></b></a>");
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
            XElement entity = ((IEnumerable<XElement>)from el in appConfig.Elements("Entities").Elements("Entity").Where(e => e.Attribute("name").Value == name) select el).First();
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

                Random randomInt = new Random();
             
                for (int i = 0; i <= 20; i++)
                {
                    content = content.Append("<tr>");
                    if (listPropertiesElement.Attribute("WithSelector") != null)
                        if (listPropertiesElement.Attribute("WithSelector").Value == "true")
                            content = content.Append("<td class=\"text-center\"><a href=\"javascript:void(0);\"><i id=item\"" + counter + "\" class=\"phaseselect fa fa-square-o fa-lg\"></i></a></td>");
                    foreach (XElement property in listFields.ToList())
                    {
                        if (Convert.ToInt32(property.Attribute("datatype").Value) == (int)FieldDataTypes.ShortString)
                            content = content.Append("<td> " + Utility.RandomString(20) + " </td>");
                        else if (Convert.ToInt32(property.Attribute("datatype").Value) == (int)FieldDataTypes.ShortInteger)
                            content = content.Append("<td>" + randomInt.Next(0, 1000) + "</td>");
                        if (Convert.ToInt32(property.Attribute("datatype").Value) == (int)FieldDataTypes.LongString)
                            content = content.Append("<td> " + Utility.RandomString(40) + " </td>");
                        else if (Convert.ToInt32(property.Attribute("datatype").Value) == (int)FieldDataTypes.LongInteger)
                            content = content.Append("<td>" + randomInt.Next(0, 100000000) + "</td>");
                    }
                    content = content.Append("<td><i class=\"fa fa-edit fa-fw\"></i></td>");
                    content = content.Append("<td><i class=\"fa fa-trash-o fa-lg\"></i></td>");
                    content = content.Append("</tr>");
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
            XElement entity = ((IEnumerable<XElement>)from el in appConfig.Elements("Entities").Elements("Entity").Where(e => e.Attribute("name").Value == name) select el).First();
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
                        content = content.Append(this.FormControlSnippet(Convert.ToInt32(field.Attribute("type").Value), field.Attribute("label").Value));
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
        private StringBuilder FormControlSnippet(int controlType, string label = "")
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
                        controlSnippet = controlSnippet.Append("<a class=\"btn btn-sm btn-default\" id=\"Btn\" href=\"\"><i id=\"Text\" class=\"fa fa-lg valign fa-check-square-o\"></i>&nbsp;" + label + "</a>");
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
            if (cmbSchemaNames.SelectedIndex == 0 && txtSchemaName.Text == "Enter Configuration Name")
            {
                schemaName = string.Empty;
                MessageBox.Show("Enter Configuration Name to save.");
                txtSchemaName.Focus();
            }
            else if (cmbSchemaNames.SelectedIndex == 0 && txtSchemaName.Text != "Enter Configuration Name")
            {
                schemaName = txtSchemaName.Text.Trim();
            }

            if (schemaName != string.Empty )
            {
                //root and application properties
                var root = new XElement("App");
                root.Add(new XElement("Name", txtAppName.Text));
                root.Add(new XElement("LoginUser", txtAppUserName.Text));

                if (appModel != null)
                {
                    //navigation
                    if (appModel.Navigation.Count > 0)
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
                                        xElement.SetAttributeValue("index", listField.Index);
                                        xElement.SetAttributeValue("datatype", listField.DataType);
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
                                        xElement.SetAttributeValue("linkTo", listAction.LinkTo);
                                        xListActions.Add(xElement);
                                    }
                                    xEntity.Add(xListActions);
                                }
                            }
                            //AddFields
                            if (entity.AddScreenSections != null)
                            {
                                if (entity.AddScreenSections.Count > 0)
                                {
                                    xAddFields = new XElement("AddFields");
                                    foreach (var section in entity.AddScreenSections)
                                    {
                                        xSection = new XElement("Section");
                                        xSection.SetAttributeValue("id", section.SectionId);
                                        xSection.SetAttributeValue("name", section.SectionName);
                                        xSection.SetAttributeValue("zone", section.Zone);

                                        foreach (var field in section.Fields)
                                        {
                                            xElement = new XElement("Field");
                                            xElement.SetAttributeValue("type", field.Type);
                                            xElement.SetAttributeValue("label", field.FieldName);
                                            xElement.SetAttributeValue("index", field.Index);
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
                                    entity.WorkflowAliasName = string.IsNullOrEmpty(txtPhaseAliasName.Text) ? "Phase" : txtPhaseAliasName.Text;
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
                root.Save(schemaName+"_appconfig.xml");
                MessageBox.Show(schemaName + " Configuration saved successfully!");
            }
        }
        #endregion

        #region AddNav_Click
        private void AddNav_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var treeNode = new TreeViewItem();
            WireGenerator.Model.MenuItem menuItem;
            bool isValid = true;

            if (chkIsSubMenu.IsChecked == false)
            {
                //for Menu items
                treeNode.Header = txtMenuName.Text.Trim();
                NavigationTreeView.Items.Add(treeNode);

                menuItem = new WireGenerator.Model.MenuItem();
                menuItem.Name = txtMenuName.Text.Trim();
                menuItem.LinkToUrl = (!string.IsNullOrEmpty(txtMenuLinkToEntity.Text)) ? txtMenuLinkToEntity.Text : string.Empty;
                appModel.Navigation.Add(menuItem);

                txtMenuName.Clear();
                txtMenuLinkToEntity.Clear();
            }
            else
            {
                //for Sub Menu items
                var selectedNode = NavigationTreeView.SelectedItem as TreeViewItem;
                //var parentNode = selectedNode.Parent as TreeViewItem;
                //if (parentNode == null)
                //{
                    if (selectedNode == null)
                    {
                        MessageBox.Show("Select Menu Item");
                        isValid = false;
                    }

                    if (isValid)
                    {
                        treeNode.Header = txtMenuName.Text;
                        

                        menuItem = appModel.Navigation.Where(a => a.Name.Equals(selectedNode.Header.ToString())).First();

                        if (menuItem.MenuSubItems == null)
                        {
                            menuItem.MenuSubItems = new List<MenuSubItem>();
                        }
                        MenuSubItem menuSubItem = new MenuSubItem();
                        menuSubItem.SubItem = txtMenuName.Text;
                        if (!string.IsNullOrEmpty(txtMenuLinkToEntity.Text))
                        {
                            selectedNode.Items.Add(treeNode.Header + " (" + txtMenuLinkToEntity.Text + ")");
                            menuSubItem.LinkToURL = txtMenuLinkToEntity.Text;
                        }
                        else
                        {
                            selectedNode.Items.Add(treeNode);
                        }

                        menuItem.MenuSubItems.Add(menuSubItem);

                        txtMenuName.Clear();
                        txtMenuLinkToEntity.Clear();
                    //}
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
                string selectedString;



                if (parentNode == null)
                {
                    var menu = appModel.Navigation.Where(a => a.Name == ((TreeViewItem)NavigationTreeView.SelectedItem).Header.ToString()).First();
                    appModel.Navigation.Remove(menu);
                    NavigationTreeView.Items.RemoveAt(NavigationTreeView.Items.IndexOf(NavigationTreeView.SelectedItem));
                }
                else
                {
                    if (selectedNode.Header.ToString().Contains("("))
                        selectedString = (selectedNode.Header.ToString()).Substring(0, (selectedNode.Header.ToString().IndexOf("(")) - 1);
                    else
                        selectedString = selectedNode.Header.ToString();

                    var menu = appModel.Navigation.Where(a => a.Name == parentNode.Header.ToString()).First();
                    //var subMenu = menu.MenuSubItems.Where(a => a.SubItem == ((TreeViewItem)NavigationTreeView.SelectedItem).Header.ToString()).First();
                    var subMenu = menu.MenuSubItems.Where(a => a.SubItem == selectedString).First();
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
                if (appModel.Entities == null)
                {
                    appModel.Entities = new List<Entity>();
                }
                entity.ListScreenFields = new List<Field>();
                entity.ListScreenActions = new List<WireGenerator.Model.Action>();
                entity.Workflow = new List<WorkflowPhase>();
                appModel.Entities.Add(entity);

                txtEntityName.Clear();
                txtEntityTitle.Clear();
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
                field.DataType = cmbListFieldDataType.SelectedIndex;

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
                    lstListFields.ItemsSource = entity.ListScreenFields.Select(a => a.FieldName).ToList();
                    txtListFieldName.Clear();
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
                lstListFields.ItemsSource = entity.ListScreenFields.Select(a => a.FieldName).ToList();
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
                action.LinkTo = txtListActionLinkTo.Text.Trim();

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
                    lstListActions.ItemsSource = entity.ListScreenActions.Select(a => a.ActionName).ToList();
                    txtListActionName.Clear();
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
                lstListActions.ItemsSource = entity.ListScreenActions.Select(a => a.ActionName).ToList();
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
                    section.SectionId = entity.AddScreenSections.Count + 1;
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

                    var treeNode = new TreeViewItemModel();
                    treeNode.Value = txtSectionName.Text.Trim();
                    treeNode.ParentId = section.SectionId;
                    rootNode.Items.Add(treeNode);

                    AddScreenSectionFieldsTreeView.ItemsSource = rootNode.Items;
                    txtSectionName.Clear();
                }
            }
        }
        #endregion

        #region RemoveSection_Click
        private void RemoveSection_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (AddScreenSectionFieldsTreeView.SelectedItem != null)
            {
                var selectedNode = (TreeViewItemModel)AddScreenSectionFieldsTreeView.SelectedItem;
                if (selectedNode != null)
                {
                    var entity = appModel.Entities.Where(a => a.Name.Equals(lstEntities.SelectedItem.ToString())).First();
                    var section = entity.AddScreenSections.Where(a => a.SectionId == selectedNode.ParentId).First();
                    entity.AddScreenSections.Remove(section);

                    var root = rootNode.Items.Where(s => s.ParentId == selectedNode.ParentId).Single();
                    var parentNodeIndex = rootNode.Items.IndexOf(root);
                    rootNode.Items.RemoveAt(rootNode.Items.IndexOf(selectedNode));
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

            var selectedNode = (TreeViewItemModel)AddScreenSectionFieldsTreeView.SelectedItem;
            if (selectedNode == null)
            {
                MessageBox.Show("Select Section Item");
                isValid = false;
            }

            if (isValid)
            {
                WireGenerator.Model.Section section;
                var entity = appModel.Entities.Where(a => a.Name.Equals(lstEntities.SelectedItem.ToString())).First();
                var entitySection = entity.AddScreenSections.Where(s => s.SectionName == selectedNode.Value).First();

                if (entitySection.Fields != null)
                {
                    if (entitySection.Fields.Any(a => a.FieldName.Equals(txtAddScreenFieldName.Text)))
                        isValid = false;
                }

                if (isValid)
                {

                    if (entitySection.Fields == null)
                        entitySection.Fields = new List<Field>();

                    var root = rootNode.Items.Where(s => s.ParentId == selectedNode.ParentId).Single();
                    var parentNodeIndex = rootNode.Items.IndexOf(root);
                    var fieldNode = rootNode.Items.ElementAt(parentNodeIndex).Items.LastOrDefault();
                    var tabIndex = (fieldNode != null) ? rootNode.Items.ElementAt(parentNodeIndex).Items.IndexOf(fieldNode) + 1 : default(int);

                    Field field = new Field();
                    field.FieldName = txtAddScreenFieldName.Text.Trim();
                    field.Type = cmbAddScreenFieldType.SelectedIndex;
                    field.Index = tabIndex;
                    section = new Model.Section();
                    entitySection.Fields.Add(field);

                    var treeNode = new TreeViewItemModel(field.FieldName);
                    treeNode.ParentId = entitySection.SectionId;
                    selectedNode.Items.Add(treeNode);

                    txtAddScreenFieldName.Clear();
                }
            }
        }
        #endregion

        #region AddScreenRemove_Click
        private void AddScreenRemove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (AddScreenSectionFieldsTreeView.SelectedItem != null)
            {
                var selectedNode = (TreeViewItemModel)AddScreenSectionFieldsTreeView.SelectedItem;
                if (selectedNode != null)
                {
                    var entity = appModel.Entities.Where(a => a.Name.Equals(lstEntities.SelectedItem.ToString())).First();
                    var section = entity.AddScreenSections.Where(a => a.SectionId == selectedNode.ParentId).First();
                    var field = section.Fields.Where(f => f.FieldName == selectedNode.Value).First();
                    section.Fields.Remove(field);

                    var root = rootNode.Items.Where(s => s.ParentId == selectedNode.ParentId).Single();
                    var parentNodeIndex = rootNode.Items.IndexOf(root);
                    rootNode.Items.ElementAt(parentNodeIndex).Items.RemoveAt(rootNode.Items.ElementAt(parentNodeIndex).Items.IndexOf(selectedNode));
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
                    lstPhases.ItemsSource = entity.Workflow.Select(w => w.PhaseName).ToList();
                    txtPhaseName.Clear();
                }
            }
        }
        #endregion

        #region RemovePhase_Click
        private void RemovePhase_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (lstPhases.SelectedItem != null)
            {
                var entity = appModel.Entities.Where(a => a.Name.Equals(lstEntities.SelectedItem.ToString())).First();
                var phase = entity.Workflow.Where(a => a.PhaseName == lstPhases.SelectedItem).First();
                entity.Workflow.Remove(phase);
                lstPhases.ItemsSource = entity.Workflow.Select(s => s.PhaseName).ToList();
            }
        }
        #endregion

        #region Entity_Select
        private void Entity_Select(object sender, SelectionChangedEventArgs e)
        {
            var entity = appModel.Entities.Where(a => a.Name == lstEntities.SelectedItem.ToString()).First();
            txtEntityName.Text = entity.Name;
            txtEntityTitle.Text = entity.Title;
            chkEntitySearch.IsChecked = false;
            chkListWithSelector.IsChecked = false;

            if (entity.HasSearch == true)
                chkEntitySearch.IsChecked = true;

            if (entity.ListScreenFields != null)
                if (entity.IsListScreenWithSelector == true)
                    chkListWithSelector.IsChecked = true;
            lstListFields.ItemsSource = entity.ListScreenFields.Select(f => f.FieldName).ToList();
            lstListActions.ItemsSource = entity.ListScreenActions.Select(a => a.ActionName).ToList();
            
            if (entity.AddScreenSections != null)
            {
                if (entity.AddScreenSections.Count > 0)
                {
                    // set index value for the previous selected entity
                    foreach (var root in rootNode.Items)
                    {
                        var previousEntity = appModel.Entities.Where(a => a.Name == PreviousEntity).First();
                        var parentNodeIndex = rootNode.Items.IndexOf(root);
                        foreach (var item in root.Items)
                        {
                            var field = previousEntity.AddScreenSections.ElementAt(parentNodeIndex).Fields.Where(f => f.FieldName == item.Value).Single();
                            int index = previousEntity.AddScreenSections.ElementAt(parentNodeIndex).Fields.ToList().IndexOf(field);
                            previousEntity.AddScreenSections.ElementAt(parentNodeIndex).Fields.ElementAt(index).Index = root.Items.IndexOf(item);
                        }
                    }

                    // reset root node for treeview
                    rootNode.Items.Clear();
                    AddScreenSectionFieldsTreeView.ItemsSource = null;
                    AddScreenSectionFieldsTreeView.Items.Clear();
                    AddScreenSectionFieldsTreeView.ItemsSource = rootNode.Items;

                    foreach (var section in entity.AddScreenSections)
                    {
                        TreeViewItemModel innerNode = new TreeViewItemModel();
                        innerNode.Value = section.SectionName;
                        innerNode.ParentId = section.SectionId;

                        foreach (var item in section.Fields.OrderBy(ob => ob.Index))
                        {
                            TreeViewItemModel childNode = new TreeViewItemModel(item.FieldName);
                            childNode.ParentId = section.SectionId;
                            innerNode.Items.Add(childNode);
                        }
                        
                        rootNode.Items.Add(innerNode);
                    }
                }
            }

            PreviousEntity = lstEntities.SelectedItem.ToString();

            lstPhases.ItemsSource = entity.Workflow.Select(w => w.PhaseName).ToList();
            if (!string.IsNullOrEmpty(entity.WorkflowAliasName))
                txtPhaseAliasName.Text = entity.WorkflowAliasName;
        }
        #endregion

        #region chkListWithSelector_Click
        private void chkListWithSelector_Click(object sender, RoutedEventArgs e)
        {
            if (lstEntities.SelectedItem != null)
            {
                var entity = appModel.Entities.Where(a => a.Name == lstEntities.SelectedItem.ToString()).First();
                entity.IsListScreenWithSelector = false;
                if (chkListWithSelector.IsChecked == true)
                    entity.IsListScreenWithSelector = true;
            }
        }
        #endregion

        #region chkEntitySearch_Click
        private void chkEntitySearch_Click(object sender, RoutedEventArgs e)
        {
            if (lstEntities.SelectedItem != null)
            {
                var entity = appModel.Entities.Where(a => a.Name == lstEntities.SelectedItem.ToString()).First();
                entity.HasSearch = false;
                if (chkEntitySearch.IsChecked == true)
                    entity.HasSearch = true;
            }
        }
        #endregion

        #region lstListFields_SelectionChanged
        private void lstListFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (lstEntities.SelectedItem != null)
            //{
            //    var field = appModel.Entities.Where(a => a.Name == lstEntities.SelectedItem.ToString()).First().ListScreenFields.Where(f => f.FieldName == lstListFields.SelectedItem.ToString()).First();
            //    txtListFieldName.Text = field.FieldName;
            //}
        }
        #endregion

        #region lstListActions_SelectionChanged
        private void lstListActions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (lstEntities.SelectedItem != null)
            //{
            //    var action = appModel.Entities.Where(a => a.Name == lstEntities.SelectedItem.ToString()).First().ListScreenActions.Where(f => f.ActionName == lstListActions.SelectedItem.ToString()).First();
            //    txtListActionName.Text = action.ActionName;
            //    txtListActionLinkTo.Text = action.LinkTo;
            //}
        }
        #endregion

        #region cmbSchemaNames_SelectionChanged
        private void cmbSchemaNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSchemaNames.SelectedIndex == 0)
            {
                txtSchemaName.Text = "Enter Configuration Name";
                txtSchemaName.Visibility = System.Windows.Visibility.Visible;
                txtAppName.Clear();
                txtAppUserName.Clear();
            }
            else
                txtSchemaName.Visibility = System.Windows.Visibility.Hidden;

            if (cmbSchemaNames.SelectedIndex > 0)
            {
                string executableLocation = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                appModel = new AppModel();
                appModel.Navigation = new List<WireGenerator.Model.MenuItem>();
                schemaName = cmbSchemaNames.SelectedItem.ToString();
                schemaAbsoluteFileName = System.IO.Path.Combine(executableLocation, cmbSchemaNames.SelectedItem.ToString() + "_appConfig.xml");
                if (File.Exists(schemaAbsoluteFileName))
                {
                    appConfig = XElement.Load(schemaAbsoluteFileName);
                    #region load appModel from xml configuration
                    if (!appConfig.IsEmpty)
                    {
                        txtAppName.Text = appConfig.Element("Name").Value;
                        txtAppUserName.Text = appConfig.Element("LoginUser").Value;

                        //navigation
                        var navigation = new XElement("Navigation");
                        TreeViewItem node, subnode;
                        WireGenerator.Model.MenuItem menuItem;
                        IEnumerable<XElement> navMenuItems = from el in appConfig.Elements("Navigation").Elements("MenuItem") select el;
                        if (navMenuItems.ToList().Count > 0)
                        {
                            foreach (XElement el in navMenuItems.ToList())
                            {
                                menuItem = new WireGenerator.Model.MenuItem();
                                menuItem.Name = el.Attribute("name").Value;
                                menuItem.LinkToUrl = (el.Attribute("linkTo") != null) ? el.Attribute("linkTo").Value : string.Empty;

                                node = new TreeViewItem();
                                if (string.IsNullOrEmpty(menuItem.LinkToUrl))
                                    node.Header = el.Attribute("name").Value;
                                else
                                    node.Header = el.Attribute("name").Value + "(" + menuItem.LinkToUrl + ")";
                                IEnumerable<XElement> menuSubItems = el.Descendants();
                                menuItem.MenuSubItems = new List<MenuSubItem>();
                                foreach (XElement subel in menuSubItems)
                                {
                                    menuSubItem = new MenuSubItem();
                                    menuSubItem.SubItem = subel.Value;
                                    menuSubItem.LinkToURL = (subel.Attribute("linkTo") != null) ? subel.Attribute("linkTo").Value : string.Empty;
                                    menuItem.MenuSubItems.Add(menuSubItem);

                                    subnode = new TreeViewItem();
                                    if (string.IsNullOrEmpty(menuSubItem.LinkToURL))
                                        subnode.Header = subel.Value;
                                    else
                                        if (menuSubItem.LinkToURL.Contains("_"))
                                            subnode.Header = subel.Value + " (" + (menuSubItem.LinkToURL).Substring(0, menuSubItem.LinkToURL.IndexOf("_")) + ")";
                                        else
                                            subnode.Header = subel.Value + " (" + (menuSubItem.LinkToURL) + ")";

                                    node.Items.Add(subnode);
                                }
                                NavigationTreeView.Items.Add(node);
                                appModel.Navigation.Add(menuItem);
                            }
                        }

                        //entities
                        IEnumerable<XElement> xEntities = from el in appConfig.Elements("Entities").Elements("Entity") select el;
                        appModel.Entities = new List<Entity>();
                        if (xEntities.ToList().Count > 0)
                        {
                            Entity entity;
                            Field field;
                            WorkflowPhase phase;
                            WireGenerator.Model.Action action;
                            WireGenerator.Model.Section section;
                            XElement xElement;
                            XAttribute xListFieldsHasSelector;
                            IEnumerable<XElement> xElements, xFields;
                            IEnumerable<XElement> xListFields;
                            
                            foreach (XElement xEntity in xEntities.ToList())
                            {
                                entity = new Entity();
                                entity.Name = xEntity.Attribute("name").Value;
                                entity.Title = xEntity.Attribute("title").Value;
                                entity.HasSearch = (xEntity.Attribute("hasSearch").Value == "true") ? true : false;

                                //load list screen fields
                                xListFieldsHasSelector = (from a in xEntity.Elements("ListFields").Attributes("WithSelector") select a).FirstOrDefault();
                                if (xListFieldsHasSelector != null)
                                    entity.IsListScreenWithSelector = Convert.ToBoolean(xListFieldsHasSelector.Value);

                                entity.ListScreenFields = new List<Field>();
                                if (xEntity.Elements("ListFields").Count() > 0)
                                {
                                    xListFields = from a in xEntity.Elements("ListFields").Descendants() select a;
                                    foreach (XElement xListField in xListFields.ToList())
                                    {
                                        field = new Field();
                                        field.FieldName = xListField.Value;
                                        field.Index = Convert.ToInt32(xListField.Attribute("index").Value);
                                        field.DataType = Convert.ToInt32(xListField.Attribute("datatype").Value);
                                        entity.ListScreenFields.Add(field);
                                    }
                                }

                                //load list screen actions
                                entity.ListScreenActions = new List<WireGenerator.Model.Action>();
                                if (xEntity.Elements("ListActions").Count() > 0)
                                {
                                    xElements = from a in xEntity.Elements("ListActions").Descendants() select a;
                                    foreach (XElement xListAction in xElements)
                                    {
                                        action = new WireGenerator.Model.Action();
                                        action.ActionName = xListAction.Value;
                                        action.LinkTo = xListAction.Attribute("linkTo").Value;
                                        entity.ListScreenActions.Add(action);
                                    }
                                }

                                //load add screen sections and fields
                                if (xEntity.Elements("AddFields").Count() > 0)
                                {
                                    xElements = from a in xEntity.Elements("AddFields").Elements("Section") select a;
                                    entity.AddScreenSections = new List<WireGenerator.Model.Section>();
                                    foreach (XElement xSection in xElements)
                                    {
                                        section = new Model.Section();
                                        section.SectionId = Convert.ToInt32(xSection.Attribute("id").Value);
                                        section.SectionName = xSection.Attribute("name").Value;
                                        section.Zone = xSection.Attribute("zone").Value;

                                        xFields = from a in xSection.Descendants() select a;
                                        section.Fields = new List<Field>();
                                        foreach (XElement xfield in xFields)
                                        {
                                            field = new Model.Field();
                                            field.FieldName = xfield.Attribute("label").Value;
                                            field.Index = Convert.ToInt32(xfield.Attribute("index").Value);
                                            field.Type = Convert.ToInt32(xfield.Attribute("type").Value);
                                            section.Fields.Add(field);
                                        }
                                        entity.AddScreenSections.Add(section);
                                    }
                                }

                                //load workflow
                                entity.Workflow = new List<WorkflowPhase>();
                                if (xEntity.Elements("Workflow").Count() > 0)
                                {
                                    xElement = (from a in xEntity.Elements("Workflow") select a).FirstOrDefault();
                                    if (xElement != null)
                                    {
                                        entity.WorkflowAliasName = xElement.Attribute("name").Value;
                                        xElements = from a in xEntity.Elements("Workflow").Descendants() select a;
                                        foreach (XElement xPhase in xElements)
                                        {
                                            phase = new WorkflowPhase();
                                            phase.PhaseName = xPhase.Value;
                                            entity.Workflow.Add(phase);
                                        }
                                    }
                                }
                                appModel.Entities.Add(entity);
                                lstEntities.Items.Add(entity.Name);
                            }
                        }
                    }
                    #endregion
                }
            }
        }
        #endregion

        #region txtSchemaName_GotFocus
        private void txtSchemaName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtSchemaName.Text == "Enter Configuration Name")
                txtSchemaName.Clear();
        }
        #endregion

        #region txtSchemaName_LostFocus
        private void txtSchemaName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtSchemaName.Text == "")
                txtSchemaName.Text = "Enter Configuration Name";
        }
        #endregion

        #region cbmListFieldDataType_Loaded
        private void cbmListFieldDataType_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> dataTypes = new List<string>();
            dataTypes.Add("Short String");
            dataTypes.Add("Short Integer");
            dataTypes.Add("Long String");
            dataTypes.Add("Long Integer");
            this.cmbListFieldDataType.ItemsSource = dataTypes;

            this.cmbListFieldDataType.SelectedIndex = 0;
        }
        #endregion

        #region TreeViewFieldMoveDown_Click
        private void TreeViewFieldMoveDown_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItemModel selectedChildNode = (TreeViewItemModel)AddScreenSectionFieldsTreeView.SelectedItem;
            TreeViewItemModel selectedParentNode = (TreeViewItemModel)rootNode.Items.Where(pn => pn.ParentId == selectedChildNode.ParentId).SingleOrDefault();

            if (selectedParentNode != null)
            {
                int parentNodeIndex = rootNode.Items.IndexOf(selectedParentNode);
                int childNodeIndex = selectedParentNode.Items.IndexOf(selectedChildNode);

                var element = rootNode.Items.ElementAt(parentNodeIndex);

                if (childNodeIndex >= 0 && childNodeIndex < element.Items.Count - 1)
                {
                    element.Items.RemoveAt(childNodeIndex);
                    element.Items.Insert(childNodeIndex + 1, selectedChildNode);
                }
            }
        }
        #endregion

        #region TreeViewFieldMoveUp_Click
        private void TreeViewFieldMoveUp_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItemModel selectedChildNode = (TreeViewItemModel)AddScreenSectionFieldsTreeView.SelectedItem;
            TreeViewItemModel selectedParentNode = (TreeViewItemModel)rootNode.Items.Where(pn => pn.ParentId == selectedChildNode.ParentId).SingleOrDefault();

            if (selectedParentNode != null)
            {
                int parentNodeIndex = rootNode.Items.IndexOf(selectedParentNode);
                int childNodeIndex = selectedParentNode.Items.IndexOf(selectedChildNode);

                var element = rootNode.Items.ElementAt(parentNodeIndex);

                if (childNodeIndex > 0)
                {
                    element.Items.RemoveAt(childNodeIndex);
                    element.Items.Insert(childNodeIndex - 1, selectedChildNode);
                }
            }
        }
        #endregion
    }
}
