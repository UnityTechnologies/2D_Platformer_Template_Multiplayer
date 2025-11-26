using Blocks.Common;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Blocks.Sessions.Common
{
    [UxmlElement]
    public partial class PlayerListViewElement : ListView
    {
        private const string k_PlayerListIsEmptyLabel = "No player has joined";

        private string m_SessionType;
        private DataBinding m_DataBinding;
        private PlayerListViewModel m_ViewModel;

        [CreateProperty, UxmlAttribute]
        public string SessionType
        {
            get => m_SessionType;
            set
            {
                if (m_SessionType == value)
                {
                    return;
                }

                m_SessionType = value;
                if (panel != null)
                {
                    UpdateBindingSources();
                }
            }
        }

        public PlayerListViewElement()
        {
            selectionType = SelectionType.None;
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            // required for making each element inheriting an indexed array item for the data source path
            // if not set, you need to specify the datasource path manually in the bindItem callback for each item
            bindingSourceSelectionMode = BindingSourceSelectionMode.AutoAssign;

            AddToClassList(BlocksTheme.ListViewNoHover);
            AddToClassList(BlocksTheme.ScrollView);

            makeNoneElement = MakeNoneElement;

            makeItem = MakeDefaultItem;

            BindItemSource();

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanelEvent);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanelEvent);
        }


        private void OnDetachFromPanelEvent(DetachFromPanelEvent panelEvent)
        {
            CleanupBindings();
        }

        private void OnAttachToPanelEvent(AttachToPanelEvent panelEvent)
        {
            UpdateBindingSources();
        }

        private void UpdateBindingSources()
        {
            CleanupBindings();

            m_ViewModel = new PlayerListViewModel(m_SessionType);
            m_DataBinding.dataSource = m_ViewModel;
        }

        private void CleanupBindings()
        {
            m_ViewModel?.Dispose();
            m_ViewModel = null;
            m_DataBinding.dataSource = null;
        }

        private static VisualElement MakeNoneElement()
        {
            var label = new Label(k_PlayerListIsEmptyLabel);
            label.AddToClassList(BlocksTheme.Label);
            return label;
        }

        private void BindItemSource()
        {
            m_DataBinding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(PlayerListViewModel.Players)),
                bindingMode = BindingMode.ToTarget
            };
            SetBinding(new BindingId(nameof(itemsSource)), m_DataBinding);
        }

        private static VisualElement MakeDefaultItem()
        {
            var playerNameLabel = new Label { name = nameof(PlayerNameLabel) };
            playerNameLabel.AddToClassList(BlocksTheme.Label);

            var dataBinding = new DataBinding
            {
                dataSourcePath = PropertyPath.FromName(nameof(PlayerViewModel.Name)),
                bindingMode = BindingMode.ToTarget
            };

            playerNameLabel.SetBinding(new BindingId(nameof(Label.text)), dataBinding);

            return playerNameLabel;
        }
    }
}
