using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EPocalipse.Json.Viewer;
using RestMan.DbUtil;
using RestMan.Entity.Db;
using RestMan.RestClient;
using RestSharp;
using RestSharp.Authenticators;

namespace EPocalipse.Json.JsonView
{
    public partial class MainForm : Form
    {
        private bool _isReName;

        private bool isAddApi;

        public MainForm()
        {
            InitializeComponent();
            treeView1.HideSelection = false;
            //自已绘制
            treeView1.DrawMode = TreeViewDrawMode.OwnerDrawText;
            treeView1.DrawNode += treeView1_DrawNode;
        }

        //在绘制节点事件中，按自已想的绘制
        private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            e.DrawDefault = true; //我这里用默认颜色即可，只需要在TreeView失去焦点时选中节点仍然突显
            return;
            if ((e.State & TreeNodeStates.Selected) != 0)
            {
                //演示为绿底白字
                e.Graphics.FillRectangle(Brushes.DarkBlue, e.Node.Bounds);

                var nodeFont = e.Node.NodeFont;
                if (nodeFont == null) nodeFont = ((TreeView)sender).Font;
                e.Graphics.DrawString(e.Node.Text, nodeFont, Brushes.White, Rectangle.Inflate(e.Bounds, 2, 0));
            }

            e.DrawDefault = true;

            if ((e.State & TreeNodeStates.Focused) != 0)
                using (var focusPen = new Pen(Color.Black))
                {
                    focusPen.DashStyle = DashStyle.Dot;
                    var focusBounds = e.Node.Bounds;
                    focusBounds.Size = new Size(focusBounds.Width - 1,
                        focusBounds.Height - 1);
                    e.Graphics.DrawRectangle(focusPen, focusBounds);
                }
        }

        private void Bind_Tv(List<FolderManage> folderManages, List<RestInfo> restInfos, TreeNodeCollection tnc,
            string pidVal)
        {
            TreeNode tn;
            //string filter = string.IsNullOrEmpty(pid_val) ? pid + " is null" : string.Format(pid + "='{0}'", pid_val);
            var folders = string.IsNullOrEmpty(pidVal)
                ? folderManages.Where(s => string.IsNullOrEmpty(s.PID?.ToString())).ToList()
                : folderManages.Where(s => s.PID.ToString() == pidVal).ToList();
            foreach (var drv in folders)
            {
                tn = new TreeNode();
                var chrestInfos = restInfos.Where(s => s.FolderId == drv.ID).ToList();
                foreach (var ch in chrestInfos)
                {
                    var tn1 = new TreeNode();
                    tn1.Name = ch.Id.ToString();
                    tn1.Tag = ch;
                    tn1.ImageIndex = 1;
                    tn1.SelectedImageIndex = 1;
                    tn1.Text = ch.Name;
                    tn.Nodes.Add(tn1);
                }

                tn.Name = drv.ID.ToString();
                tn.Tag = drv;
                tn.Text = drv.FolderName;
                tnc.Add(tn);

                Bind_Tv(folderManages, restInfos, tn.Nodes, tn.Name); //递归（反复调用这个方法，直到把数据取完为止）
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            var list = DbContext.DbClient.Queryable<FolderManage>().ToList();
            var restInfos = DbContext.DbClient.Queryable<RestInfo>().ToList();
            Bind_Tv(list, restInfos, treeView1.Nodes, "-1");
            treeView1.ExpandAll();
            var args = Environment.GetCommandLineArgs();
            for (var i = 1; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.Equals("/c", StringComparison.OrdinalIgnoreCase))
                    LoadFromClipboard();
                else if (File.Exists(arg)) LoadFromFile(arg);
            }
        }

        private void LoadFromFile(string fileName)
        {
            var json = File.ReadAllText(fileName);
            JsonViewer.ShowTab(Tabs.Viewer);
            JsonViewer.Json = json;
        }

        private void LoadFromClipboard()
        {
            var json = Clipboard.GetText();
            if (!string.IsNullOrEmpty(json))
            {
                JsonViewer.ShowTab(Tabs.Viewer);
                JsonViewer.Json = json;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            JsonViewer.ShowTab(Tabs.Text);
        }

        /// <summary>
        ///     Closes the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Menu item File > Exit</remarks>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        ///     Open File Dialog  for Yahoo! Pipe files or JSON files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Menu item File > Open</remarks>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter =
                "Yahoo! Pipe files (*.run)|*.run|json files (*.json)|*.json|All files (*.*)|*.*";
            dialog.InitialDirectory = Application.StartupPath;
            dialog.Title = "Select a JSON file";
            if (dialog.ShowDialog() == DialogResult.OK) LoadFromFile(dialog.FileName);
        }

        /// <summary>
        ///     Launches About JSONViewer dialog box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Menu item Help > About JSON Viewer</remarks>
        private void aboutJSONViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutJsonViewer().ShowDialog();
        }

        /// <summary>
        ///     Selects all text in the textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Menu item Edit > Select All</remarks>
        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Control c;
            c = JsonViewer.Controls.Find("txtJson", true)[0];
            ((RichTextBox)c).SelectAll();
        }

        /// <summary>
        ///     Deletes selected text in the textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Menu item Edit > Delete</remarks>
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Control c;
            c = JsonViewer.Controls.Find("txtJson", true)[0];
            string text;
            if (((RichTextBox)c).SelectionLength > 0)
                text = ((RichTextBox)c).SelectedText;
            else
                text = ((RichTextBox)c).Text;
            ((RichTextBox)c).SelectedText = "";
        }

        /// <summary>
        ///     Pastes text in the clipboard into the textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Menu item Edit > Paste</remarks>
        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Control c;
            c = JsonViewer.Controls.Find("txtJson", true)[0];
            ((RichTextBox)c).Paste();
        }

        /// <summary>
        ///     Copies text in the textbox into the clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Menu item Edit > Copy</remarks>
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Control c;
            c = JsonViewer.Controls.Find("txtJson", true)[0];
            ((RichTextBox)c).Copy();
        }

        /// <summary>
        ///     Cuts selected text from the textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Menu item Edit > Cut</remarks>
        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Control c;
            c = JsonViewer.Controls.Find("txtJson", true)[0];
            ((RichTextBox)c).Cut();
        }

        /// <summary>
        ///     Undo the last action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Menu item Edit > Undo</remarks>
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Control c;
            c = JsonViewer.Controls.Find("txtJson", true)[0];
            ((RichTextBox)c).Undo();
        }

        /// <summary>
        ///     Displays the find prompt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Menu item Viewer > Find</remarks>
        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Control c;
            c = JsonViewer.Controls.Find("pnlFind", true)[0];
            ((Panel)c).Visible = true;
            Control t;
            t = JsonViewer.Controls.Find("txtFind", true)[0];
            ((TextBox)t).Focus();
        }

        /// <summary>
        ///     Expands all the nodes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Menu item Viewer > Expand All</remarks>
        /// <!---->
        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Control c;
            c = JsonViewer.Controls.Find("tvJson", true)[0];
            ((TreeView)c).BeginUpdate();
            try
            {
                if (((TreeView)c).SelectedNode != null)
                {
                    var topNode = ((TreeView)c).TopNode;
                    ((TreeView)c).SelectedNode.ExpandAll();
                    ((TreeView)c).TopNode = topNode;
                }
            }
            finally
            {
                ((TreeView)c).EndUpdate();
            }
        }

        /// <summary>
        ///     Copies a node
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Menu item Viewer > Copy</remarks>
        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Control c;
            c = JsonViewer.Controls.Find("tvJson", true)[0];
            var node = ((TreeView)c).SelectedNode;
            if (node != null) Clipboard.SetText(node.Text);
        }

        /// <summary>
        ///     Copies just the node's value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Menu item Viewer > Copy Value</remarks>
        /// <!-- JsonViewerTreeNode had to be made public to be accessible here -->
        private void copyValueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Control c;
            c = JsonViewer.Controls.Find("tvJson", true)[0];
            var node = (JsonViewerTreeNode)((TreeView)c).SelectedNode;
            if (node != null && node.JsonObject.Value != null) Clipboard.SetText(node.JsonObject.Value.ToString());
        }


        /// <summary>
        ///     开始请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var endPoint = txtEndPoint.Text;
                var resource = txtResource.Text;
                var reqHeader = richReqHeader.Text;
                var reqbody = richReqBody.Text;
                SaveRestInfo();

                IAuthenticator iaAuthenticator = new HttpBasicAuthenticator("", "");
                IRestSharp iRestSharp = new RestSharpClient(endPoint, iaAuthenticator);
                IRestRequest iRequest = new RestRequest(new Uri(endPoint + resource));
                iRequest.Method = (Method)Enum.Parse(typeof(Method), cbMethod.Text);

                iRequest.AddParameter("application/json; charset=utf-8", reqbody, ParameterType.RequestBody);

                var iResponse = iRestSharp.Execute(iRequest);

                var headerStr = "";
                foreach (var parameter in iResponse.Headers)
                {
                    headerStr += parameter.Name + ":" + parameter.Value + "\n";
                }
                richResHeader.Text = headerStr.Trim(':');

                JsonViewer.Json = iResponse.Content;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "请求失败，请检查配置信息");
            }
        }

        private void 新增ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                MessageBox.Show("请选择父节点");
                return;
            }

            var pFolder = treeView1.SelectedNode.Tag as FolderManage;

            var node1 = new TreeNode();
            node1.Tag = pFolder;
            node1.Text = "新建文件夹";
            treeView1.SelectedNode.Nodes.Add(node1);
            treeView1.SelectedNode.Expand();
            treeView1.LabelEdit = true;
            node1.BeginEdit();
        }

        //新增插入数据
        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            e.Node.EndEdit(false);
            var item = e.Node.Tag as FolderManage;
            var restInfo1 = e.Node.Tag as RestInfo;
            if (isAddApi)
            {
                try
                {
                    if (item != null)
                    {
                        var folder = new RestInfo
                        {
                            Name = e.Label,
                            FolderId = item.ID
                        };
                        DbContext.DbClient.Insertable(folder).ExecuteCommand();
                    }
                }
                catch (Exception)
                {
                    isAddApi = false;
                }

                isAddApi = false;
            }
            else
            {
                if (_isReName)
                {
                    if (item != null)
                    {
                        item.FolderName = e.Label;
                        DbContext.DbClient.Updateable(item).ExecuteCommand();
                    }
                    if (restInfo1 != null)
                    {
                        restInfo1.Name = e.Label;
                        DbContext.DbClient.Updateable(restInfo1).ExecuteCommand();
                    }
                    _isReName = false;
                }
                else
                {
                    if (item != null)
                    {
                        var folder = new FolderManage
                        {
                            FolderName = e.Label,
                            PID = item.ID,
                            GroupId = item.GroupId
                        };
                        DbContext.DbClient.Insertable(folder).ExecuteCommand();
                    }
                }
            }
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode;
            var pFolder = node.Tag as FolderManage;
            node.Remove();
            var deleteable = DbContext.DbClient.Deleteable(pFolder).ExecuteCommand();
        }

        private void 重命名ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                MessageBox.Show("请选中节点");
                return;
            }

            var node = treeView1.SelectedNode;
            var pFolder = node.Tag as FolderManage;
            treeView1.LabelEdit = true;
            node.BeginEdit();
            _isReName = true;
        }

        private void 新增APIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                MessageBox.Show("请选择父节点");
                return;
            }

            isAddApi = true;
            var pFolder = treeView1.SelectedNode.Tag as FolderManage;

            var node1 = new TreeNode();
            node1.Tag = pFolder;
            node1.Text = "新建API";
            treeView1.SelectedNode.Nodes.Add(node1);
            treeView1.SelectedNode.Expand();
            treeView1.LabelEdit = true;
            node1.BeginEdit();
        }

        private RestInfo _restInfo;
        //节点点击事件
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            e.Node.Checked = true;
            if (e.Node.Tag is RestInfo resInfo)
            {
                cbMethod.Text = resInfo.Method;
                txtEndPoint.Text = resInfo.EndPoint;
                txtResource.Text = resInfo.Resource;
                richReqHeader.Text = resInfo.ReqHeader;
                richReqBody.Text = resInfo.ReqBody;
                cbMediaType.Text = resInfo.MediaType;
                _restInfo = resInfo;
            }
        }

        //保存请求信息
        private void button4_Click(object sender, EventArgs e)
        {
            if (_restInfo == null) { MessageBox.Show("请选择要保存的节点"); return; }
            SaveRestInfo();
            MessageBox.Show("保存成功！");
        }

        //保存restinfo信息
        private void SaveRestInfo()
        {
            var resInfo = _restInfo;
            if (resInfo != null)
            {
                resInfo.Method = cbMethod.Text;
                resInfo.EndPoint = txtEndPoint.Text;
                resInfo.Resource = txtResource.Text;
                resInfo.ReqHeader = richReqHeader.Text;
                resInfo.ReqBody = richReqBody.Text;
                resInfo.MediaType = cbMediaType.Text;
                DbContext.DbClient.Updateable(resInfo).ExecuteCommand();
            }
        }
    }
}