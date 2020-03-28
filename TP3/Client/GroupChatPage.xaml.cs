﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace IFT585_TP3.Client
{
    public class ChatFeedListBoxItem
    {
        public Model.Message Message { get; set; }

        public string Content => Message.Content;

        public string Username => Message.Username;
    }

    public enum SexType { Male, Female };

    public class MemberListItem
    {
        public string Username { get; set; } = "";

        public int Age { get; set; }

        public string Mail { get; set; }

        public SexType Sex { get; set; }
    }



    /// <summary>
    /// Interaction logic for GroupManagementPage.xaml
    /// </summary>
    public partial class GroupChatPage : BasePage
    {
        public const string ChatFeedListBoxString = "ChatFeedListBox";
        public const string ChatFeedScrollViewerString = "ChatFeedScrollViewer";
        public const string MessageInputTextBoxString = "MessageInputTextBox";
        public const string MakeAdminQuestionString = "Please enter who you want to make admin:";
        public const string MakeAdminDefaultString = "My admin";
        public const string InviteQuestionString = "Please enter who you want to invite:";
        public const string InviteDefaultString = "My new member";
        public const string KickOutQuestionString = "Please enter who you want to kick out:";
        public const string KickoutDefaultString = "My enemy";

        private ScrollViewer _chatFeedScrollViewer;
        private ListBox _chatFeedListBox;
        private TextBox _messageInputTextBox;

        public Action OnLobbyHandler { get; set; }

        public GroupChatPage()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _chatFeedListBox =
                Utils.FindChildren(new List<ListBox>(), this)
                .FirstOrDefault(item => item.Name.Equals(ChatFeedListBoxString));
            _messageInputTextBox =
                Utils.FindChildren(new List<TextBox>(), this)
                .FirstOrDefault(item => item.Name.Equals(MessageInputTextBoxString));
            _chatFeedScrollViewer = Utils.FindChildren(new List<ScrollViewer>(), this)
                .FirstOrDefault(item => item.Name.Equals(ChatFeedScrollViewerString));

            //List<MemberListItem> items = new List<MemberListItem>();
            //items.Add(new MemberListItem() { Username = "John Doe", Age = 42, Sex = SexType.Male });
            //items.Add(new MemberListItem() { Username = "Jane Doe", Age = 39, Sex = SexType.Female });
            //items.Add(new MemberListItem() { Username = "Sammy Doe", Age = 13, Sex = SexType.Male });
            //items.Add(new MemberListItem() { Username = "Donna Doe", Age = 13, Sex = SexType.Female });
        }


        public void SendMessage(Model.Message message)
        {
            _chatFeedListBox.Items.Add(new ChatFeedListBoxItem()
            {
                Message = message
            });

            _messageInputTextBox.Text = "";
            _chatFeedScrollViewer.ScrollToVerticalOffset(int.MaxValue);
        }



        private void OnMakeAdminButtonClicked(object sender, RoutedEventArgs e)
        {
            QuestionDialog dialog = new QuestionDialog(MakeAdminQuestionString, MakeAdminDefaultString);
            if (dialog.ShowDialog() == true)
            {
                // TODO verify if group exists
                //if (!_lobbyController.GroupExists(dialog.Answer))
                //{
                //    Model.Group group = new Model.Group() { GroupName = dialog.Answer };
                //    group.AdminUsernames.Add(_connection.Username);
                //    group.MemberUsernames.Add(_connection.Username);
                //    AddGroup(group);
                //}
            }
        }

        private void OnInviteButtonClicked(object sender, RoutedEventArgs e)
        {
            QuestionDialog dialog = new QuestionDialog(InviteQuestionString, InviteDefaultString);
            if (dialog.ShowDialog() == true)
            {
                // TODO verify if group exists
                //if (!_lobbyController.GroupExists(dialog.Answer))
                //{
                //    Model.Group group = new Model.Group() { GroupName = dialog.Answer };
                //    group.AdminUsernames.Add(_connection.Username);
                //    group.MemberUsernames.Add(_connection.Username);
                //    AddGroup(group);
                //}
            }
        }

        private void OnKickOutButtonClicked(object sender, RoutedEventArgs e)
        {
            QuestionDialog dialog = new QuestionDialog(KickOutQuestionString, KickoutDefaultString);
            if (dialog.ShowDialog() == true)
            {
                // TODO verify if group exists
                //if (!_lobbyController.GroupExists(dialog.Answer))
                //{
                //    Model.Group group = new Model.Group() { GroupName = dialog.Answer };
                //    group.AdminUsernames.Add(_connection.Username);
                //    group.MemberUsernames.Add(_connection.Username);
                //    AddGroup(group);
                //}
            }
        }


        private void OnSendButtonClicked(object sender, RoutedEventArgs e)
        {
            SendMessage(new Model.Message()
            {
                Content = _messageInputTextBox.Text,
                Username = _connection.Username

            });
        }

        private void TextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter) return;

            // your event handler here
            e.Handled = true;
            SendMessage(new Model.Message()
            {
                Content = _messageInputTextBox.Text,
                Username = _connection.Username

            });
        }


        private void OnLobbyButtonClicked(object sender, RoutedEventArgs e)
        {
            OnLobbyHandler?.Invoke();
        }



        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;


        private void lvUsersColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                //lvUsers.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            //lvUsers.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }


        public class SortAdorner : Adorner
        {
            private static Geometry ascGeometry =
                Geometry.Parse("M 0 4 L 3.5 0 L 7 4 Z");

            private static Geometry descGeometry =
                Geometry.Parse("M 0 0 L 3.5 4 L 7 0 Z");

            public ListSortDirection Direction { get; private set; }

            public SortAdorner(UIElement element, ListSortDirection dir)
                : base(element)
            {
                this.Direction = dir;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if (AdornedElement.RenderSize.Width < 20)
                    return;

                TranslateTransform transform = new TranslateTransform
                    (
                        AdornedElement.RenderSize.Width - 15,
                        (AdornedElement.RenderSize.Height - 5) / 2
                    );
                drawingContext.PushTransform(transform);

                Geometry geometry = ascGeometry;
                if (this.Direction == ListSortDirection.Descending)
                    geometry = descGeometry;
                drawingContext.DrawGeometry(Brushes.Black, null, geometry);

                drawingContext.Pop();
            }
        }
    }
}
