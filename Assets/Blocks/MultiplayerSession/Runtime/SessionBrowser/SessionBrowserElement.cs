using System;
using UnityEngine;
using Blocks.Common;
using Unity.Properties;
using UnityEngine.UIElements;
using Blocks.Sessions.Common;
using Unity.Services.Multiplayer;
using System.Collections.Generic;

namespace Blocks.Sessions
{
    [UxmlElement]
    public partial class SessionBrowserElement : ListView
    {
        private const string k_JoinButtonText = "JOIN";
        private const string k_RefreshButtonText = "REFRESH LIST";
        private const string k_NoSessionFoundText = "No sessions found";

        private const string k_SessionNameLabel = "SessionNameLabel";
        private const string k_SessionPlayerCountLabel = "SessionPlayerCountLabel";

        private int m_MaxSessionsDisplayed = 20;
        private SessionSettings m_SessionSettings;
        private SessionBrowserViewModel m_ViewModel;
        private List<DataBinding> m_DataBindings;

        private Button m_RefreshButton;
        private Button m_JoinSessionButton;

        [CreateProperty, UxmlAttribute]
        public SessionSettings SessionSettings
        {
            get => m_SessionSettings;
            set
            {
                if (m_SessionSettings == value)
                    return;

                m_SessionSettings = value;
                if (panel != null)
                {
                    UpdateBindingSources();
                }
            }
        }

        [CreateProperty, UxmlAttribute]
        public int MaxSessionsDisplayed
        {
            get => m_MaxSessionsDisplayed;
            set => m_MaxSessionsDisplayed = value;
        }

        public SessionBrowserElement()
        {
            virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
            fixedItemHeight = 56f;

            // required for making each element inheriting an indexed array item for the data source path
            // if not set, you need to specify the datasource path manually in the bindItem callback for each item
            bindingSourceSelectionMode = BindingSourceSelectionMode.AutoAssign;

            AddToClassList(BlocksTheme.ScrollView);
            AddToClassList(BlocksTheme.SpaceBottom);

            makeNoneElement = MakeNoneElement;
            makeItem = MakeDefaultItem;
            makeFooter =  MakeFooter;

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanelEvent);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanelEvent);
        }

        private VisualElement MakeFooter()
        {
            var buttonsContainer = new VisualElement();
            buttonsContainer.AddToClassList(BlocksTheme.ContainerHorizontal);
            buttonsContainer.AddToClassList(BlocksTheme.ContainerAlignedRight);

            m_JoinSessionButton = new Button { text = k_JoinButtonText };
            m_JoinSessionButton.AddToClassList(BlocksTheme.Button);
            m_JoinSessionButton.AddToClassList(BlocksTheme.SpaceRight);
            buttonsContainer.Add(m_JoinSessionButton);

            m_RefreshButton = new Button { text = k_RefreshButtonText };
            m_RefreshButton.AddToClassList(BlocksTheme.Button);
            buttonsContainer.Add(m_RefreshButton);

            return buttonsContainer;
        }

        private void OnRefreshButtonClicked()
        {
            ClearSelection();
            // fire and forget, so we don't block the UI thread
            _ = m_ViewModel?.UpdateSessionListAsync(MaxSessionsDisplayed);
        }

        private void JoinSession()
        {
            if (!m_ViewModel.SelectedAndAvailable)
            {
                Debug.LogError("Selected session is no longer selected.");
                return;
            }

            // fire and forget, so we don't block the UI thread
            _ = m_ViewModel.JoinSessionAsync(SessionSettings.ToJoinSessionOptions());
        }

        private void UpdateBindingSources()
        {
            CleanupBindings();

            m_ViewModel = new SessionBrowserViewModel(SessionSettings?.sessionType);
            foreach (var dataBinding in m_DataBindings)
            {
                dataBinding.dataSource = m_ViewModel;
            }
        }

        private void CleanupBindings()
        {
            m_ViewModel?.Dispose();
            m_ViewModel = null;
            foreach (var dataBinding in m_DataBindings)
            {
                dataBinding.dataSource = null;
            }
        }

        private void OnDetachFromPanelEvent(DetachFromPanelEvent evt)
        {
            CleanupBindings();

            m_RefreshButton.clicked -= OnRefreshButtonClicked;
            m_RefreshButton.ClearBinding(nameof(SessionBrowserViewModel.CanRefresh));
            m_JoinSessionButton.clicked -= JoinSession;
            m_JoinSessionButton.ClearBinding(nameof(enabledSelf));

            ClearBindings();
        }

        private void OnAttachToPanelEvent(AttachToPanelEvent evt)
        {
            m_DataBindings = new List<DataBinding>();

            var listBinding = new DataBinding { dataSourcePath = new PropertyPath(nameof(SessionBrowserViewModel.Sessions)), bindingMode = BindingMode.ToTarget };
            SetBinding(new BindingId(nameof(ListView.itemsSource)), listBinding);
            m_DataBindings.Add(listBinding);

            var selectionBinding = new DataBinding { dataSourcePath = new PropertyPath(nameof(SessionBrowserViewModel.SelectedSessionIndex)), bindingMode = BindingMode.TwoWay };
            SetBinding(new BindingId(nameof(ListView.selectedIndex)), selectionBinding);
            m_DataBindings.Add(selectionBinding);

            var joinSessionBinding = new DataBinding { dataSourcePath = new PropertyPath(nameof(SessionBrowserViewModel.SelectedAndAvailable)), bindingMode = BindingMode.ToTarget };

            m_JoinSessionButton.SetBinding(new BindingId(nameof(enabledSelf)), joinSessionBinding);
            m_DataBindings.Add(joinSessionBinding);
            m_JoinSessionButton.clicked += JoinSession;

            var refreshBinding = new DataBinding { dataSourcePath = new PropertyPath(nameof(SessionBrowserViewModel.CanRefresh)), bindingMode = BindingMode.ToTarget };

            m_RefreshButton.SetBinding(new BindingId(nameof(enabledSelf)), refreshBinding);
            m_DataBindings.Add(refreshBinding);
            m_RefreshButton.clicked += OnRefreshButtonClicked;

            UpdateBindingSources();
        }

        private static VisualElement MakeNoneElement()
        {
            var label = new Label(k_NoSessionFoundText);
            label.AddToClassList(BlocksTheme.Label);
            label.AddToClassList(BlocksTheme.SpaceLeft);
            return label;
        }

        private static VisualElement MakeDefaultItem()
        {
            var container = new VisualElement();
            container.AddToClassList(BlocksTheme.ContainerHorizontal);
            container.AddToClassList(BlocksTheme.ScrollViewElement);
            container.AddToClassList(BlocksTheme.ContainerSpaceBetween);

            var sessionNameLabel = new Label { name = k_SessionNameLabel };
            sessionNameLabel.AddToClassList(BlocksTheme.Label);
            sessionNameLabel.AddToClassList(BlocksTheme.SpaceLeft);
            container.Add(sessionNameLabel);

            var db = new DataBinding
            {
                dataSourcePath = PropertyPath.FromName(nameof(SessionInfoViewModel.Name)),
                bindingMode = BindingMode.ToTarget,
                updateTrigger = BindingUpdateTrigger.OnSourceChanged
            };
            sessionNameLabel.SetBinding(nameof(Label.text), db);

            var sessionPlayerCountLabel = new Label { name = k_SessionPlayerCountLabel };
            sessionPlayerCountLabel.AddToClassList(BlocksTheme.Label);
            sessionPlayerCountLabel.AddToClassList(BlocksTheme.SpaceRight);
            container.Add(sessionPlayerCountLabel);

            var sessionPlayerCountBinding = new DataBinding { bindingMode = BindingMode.ToTarget };

            // register a local converter to display relevant session properties as a formatted string
            sessionPlayerCountBinding.sourceToUiConverters
                .AddConverter((ref SessionInfoViewModel session) => $"{session.MaxPlayers - session.AvailableSlots}/{session.MaxPlayers} Players");

            sessionPlayerCountLabel.SetBinding(nameof(Label.text), sessionPlayerCountBinding);

            return container;
        }
    }
}
