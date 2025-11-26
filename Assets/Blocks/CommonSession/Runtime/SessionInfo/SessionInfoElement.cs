using System.Collections.Generic;
using Blocks.Common;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Blocks.Sessions.Common
{
    [UxmlElement]
    public partial class SessionInfoElement : VisualElement
    {

        const string k_SessionInfoHeaderName = "SessionInfoHeader";
        const string k_SessionInfoHeaderText = "SESSION INFO";

        const string k_SessionDataHeaderName = "SessionData";
        const string k_SessionDataHeaderText = "Session Data";

        const string k_ListIsEmptyLabel = "List is empty";
        const string k_KeyLabelPropertyName = "PropertyKeyLabel";
        const string k_VisibilityLabelPropertyName = "PropertyVisibilityLabel";
        const string k_ValueLabelPropertyName = "PropertyValueLabel";

        const string k_SessionPropertiesHeaderName = "SessionProperties";
        const string k_SessionPropertiesHeaderText = "Session Properties";

        const string k_PlayerDataHeaderName = "PlayerData";
        const string k_PlayerDataHeaderText = "Player Data";

        const string k_PlayerHeaderName = "PlayerHeader";
        const string k_PlayerIdLabelName = "PlayerIdLabel";
        const string k_PlayerJoinedLabelName = "PlayerJoinedLabel";
        const string k_PlayerLastUpdatedLabelName = "PlayerLastUpdatedLabel";
        const string k_PlayerPropertiesListName = "PlayerPropertiesList";

        const string k_PlayerPropertiesHeaderText = "Player Properties";

        const string k_PlayerPropertiesIdKeyName = "PlayerPropertiesIDKey";
        const string k_PlayerPropertiesIdValueName = "PlayerPropertiesIDValue";
        const string k_PlayerPropertiesIdVisibilityName = "PlayerPropertiesIDVisibility";

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
                    UpdateBindings();
                }
            }
        }
        string m_SessionType;

        SessionInfoViewModel m_ViewModel;

        readonly List<DataBinding> m_Bindings = new();

        public SessionInfoElement()
        {
            AddToClassList(BlocksTheme.FullWidth);

            var sessionInfoHeader = new Label
            {
                name = k_SessionInfoHeaderName,
                text = k_SessionInfoHeaderText
            };
            sessionInfoHeader.AddToClassList(BlocksTheme.Header);
            sessionInfoHeader.AddToClassList(BlocksTheme.HeaderSpaceBottom);
            Add(sessionInfoHeader);

            var stateLabel = new Label();
            stateLabel.AddToClassList(BlocksTheme.Header);
            stateLabel.AddToClassList(BlocksTheme.HeaderXSmall);
            stateLabel.AddToClassList(BlocksTheme.SpaceBottom);
            var stateBinding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(SessionInfoViewModel.SessionState)),
                bindingMode = BindingMode.ToTarget
            };
            stateLabel.SetBinding(new BindingId(nameof(stateLabel.text)), stateBinding);
            Add(stateLabel);
            m_Bindings.Add(stateBinding);

            var scrollViewParent = new ScrollView();
            scrollViewParent.AddToClassList(BlocksTheme.ScrollView);
            var sessionContainerBinding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(SessionInfoViewModel.SessionInfoDisplayStyle)),
                bindingMode = BindingMode.ToTarget,
            };
            scrollViewParent.SetBinding(new BindingId("style.display"), sessionContainerBinding);
            Add(scrollViewParent);
            m_Bindings.Add(sessionContainerBinding);

            var sessionDataHeader = new Label
            {
                name = k_SessionDataHeaderName,
                text = k_SessionDataHeaderText
            };
            sessionDataHeader.AddToClassList(BlocksTheme.Header);
            sessionDataHeader.AddToClassList(BlocksTheme.HeaderSmall);
            sessionDataHeader.AddToClassList(BlocksTheme.SpaceBottomSmall);
            scrollViewParent.Add(sessionDataHeader);

            var sessionIdLabel = new Label();
            sessionIdLabel.AddToClassList(BlocksTheme.Label);
            var sessionIdBinding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(SessionInfoViewModel.SessionId)),
                bindingMode = BindingMode.ToTarget
            };
            sessionIdLabel.SetBinding(new BindingId(nameof(sessionIdLabel.text)), sessionIdBinding);
            scrollViewParent.Add(sessionIdLabel);
            m_Bindings.Add(sessionIdBinding);

            var sessionNameLabel = new Label();
            sessionNameLabel.AddToClassList(BlocksTheme.Label);
            var sessionNameBinding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(SessionInfoViewModel.Name)),
                bindingMode = BindingMode.ToTarget
            };
            sessionNameLabel.SetBinding(new BindingId(nameof(sessionNameLabel.text)), sessionNameBinding);
            scrollViewParent.Add(sessionNameLabel);
            m_Bindings.Add(sessionNameBinding);

            var maxPlayersLabel = new Label();
            maxPlayersLabel.AddToClassList(BlocksTheme.Label);
            var maxPlayersBinding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(SessionInfoViewModel.MaxPlayers)),
                bindingMode = BindingMode.ToTarget
            };
            maxPlayersLabel.SetBinding(new BindingId(nameof(maxPlayersLabel.text)), maxPlayersBinding);
            scrollViewParent.Add(maxPlayersLabel);
            m_Bindings.Add(maxPlayersBinding);

            var isPrivateLabel = new Label();
            isPrivateLabel.AddToClassList(BlocksTheme.Label);
            var isPrivateBinding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(SessionInfoViewModel.IsPrivate)),
                bindingMode = BindingMode.ToTarget
            };
            isPrivateLabel.SetBinding(new BindingId(nameof(isPrivateLabel.text)), isPrivateBinding);
            scrollViewParent.Add(isPrivateLabel);
            m_Bindings.Add(isPrivateBinding);

            var isLockedLabel = new Label();
            isLockedLabel.AddToClassList(BlocksTheme.Label);
            var isLockedBinding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(SessionInfoViewModel.IsLocked)),
                bindingMode = BindingMode.ToTarget
            };
            isLockedLabel.SetBinding(new BindingId(nameof(isLockedLabel.text)), isLockedBinding);
            scrollViewParent.Add(isLockedLabel);
            m_Bindings.Add(isLockedBinding);

            var sessionCodeLabel = new Label();
            sessionCodeLabel.AddToClassList(BlocksTheme.Label);
            var sessionCodeBinding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(SessionInfoViewModel.SessionCode)),
                bindingMode = BindingMode.ToTarget
            };
            sessionCodeLabel.SetBinding(new BindingId(nameof(sessionCodeLabel.text)), sessionCodeBinding);
            scrollViewParent.Add(sessionCodeLabel);
            m_Bindings.Add(sessionCodeBinding);

            var hostIDLabel = new Label();
            hostIDLabel.AddToClassList(BlocksTheme.Label);
            var hostIDBinding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(SessionInfoViewModel.HostID)),
                bindingMode = BindingMode.ToTarget
            };
            hostIDLabel.SetBinding(new BindingId(nameof(hostIDLabel.text)), hostIDBinding);
            scrollViewParent.Add(hostIDLabel);
            m_Bindings.Add(hostIDBinding);

            var isHostLabel = new Label();
            isHostLabel.AddToClassList(BlocksTheme.Label);
            isHostLabel.AddToClassList(BlocksTheme.SpaceBottomSmall);
            var isHostBinding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(SessionInfoViewModel.IsHost)),
                bindingMode = BindingMode.ToTarget
            };
            isHostLabel.SetBinding(new BindingId(nameof(isHostLabel.text)), isHostBinding);
            scrollViewParent.Add(isHostLabel);
            m_Bindings.Add(isHostBinding);

            var propertiesListView = new ListView
            {
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                makeNoneElement = () =>
                {
                    var label = new Label(k_ListIsEmptyLabel);
                    label.AddToClassList(BlocksTheme.Label);
                    return label;
                },
                makeHeader = () =>
                {
                    var sessionPropertiesHeader = new Label
                    {
                        name = k_SessionPropertiesHeaderName,
                        text = k_SessionPropertiesHeaderText
                    };
                    sessionPropertiesHeader.AddToClassList(BlocksTheme.Header);
                    sessionPropertiesHeader.AddToClassList(BlocksTheme.HeaderXSmall);
                    sessionPropertiesHeader.AddToClassList(BlocksTheme.HeaderXSmall);
                    return sessionPropertiesHeader;
                },
                makeItem = () =>
                {
                    var container = new VisualElement();
                    container.AddToClassList(BlocksTheme.Label);

                    var keyLabel = new Label
                    {
                        name = k_KeyLabelPropertyName
                    };
                    keyLabel.AddToClassList(BlocksTheme.Label);
                    container.Add(keyLabel);

                    var visibilityLabel = new Label
                    {
                        name = k_VisibilityLabelPropertyName
                    };
                    visibilityLabel.AddToClassList(BlocksTheme.Label);
                    container.Add(visibilityLabel);

                    var valueLabel = new Label
                    {
                        name = k_ValueLabelPropertyName
                    };
                    valueLabel.AddToClassList(BlocksTheme.Label);
                    container.Add(valueLabel);
                    return container;
                },
                bindItem = (element, index) =>
                {
                    var sessionProperty = m_ViewModel.SessionProperties[index];

                    element.Q<Label>(k_KeyLabelPropertyName).text = $"Key: {sessionProperty.Key}";
                    element.Q<Label>(k_VisibilityLabelPropertyName).text = $"Visibility: {sessionProperty.Visibility}";
                    element.Q<Label>(k_ValueLabelPropertyName).text = $"Value: {sessionProperty.Value}";
                }
            };
            propertiesListView.AddToClassList(BlocksTheme.Label);
            propertiesListView.AddToClassList(BlocksTheme.SpaceBottom);
            var propertiesBinding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(SessionInfoViewModel.SessionProperties)),
                bindingMode = BindingMode.ToTarget,
            };
            propertiesListView.SetBinding(new BindingId(nameof(ListView.itemsSource)), propertiesBinding);
            scrollViewParent.Add(propertiesListView);
            m_Bindings.Add(propertiesBinding);

            var playerStatsListView = new ListView
            {
                selectionType = SelectionType.None,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                makeNoneElement = () =>
                {
                    var label = new Label(k_ListIsEmptyLabel);
                    label.AddToClassList(BlocksTheme.Label);
                    return label;
                },
                makeHeader = () =>
                {
                    var playerDataHeader = new Label
                    {
                        name = k_PlayerDataHeaderName,
                        text = k_PlayerDataHeaderText
                    };
                    playerDataHeader.AddToClassList(BlocksTheme.Header);
                    playerDataHeader.AddToClassList(BlocksTheme.HeaderSmall);
                    playerDataHeader.AddToClassList(BlocksTheme.SpaceBottomSmall);
                    return playerDataHeader;
                },
                makeItem = () =>
                {
                    var container = new VisualElement();
                    container.AddToClassList(BlocksTheme.SpaceBottom);

                    var playerHeaderLabel = new Label
                    {
                        name = k_PlayerHeaderName
                    };
                    playerHeaderLabel.AddToClassList(BlocksTheme.Header);
                    playerHeaderLabel.AddToClassList(BlocksTheme.HeaderXSmall);
                    playerHeaderLabel.AddToClassList(BlocksTheme.SpaceBottomSmall);
                    container.Add(playerHeaderLabel);

                    var playerIdLabel = new Label
                    {
                        name = k_PlayerIdLabelName
                    };
                    playerIdLabel.AddToClassList(BlocksTheme.Label);
                    container.Add(playerIdLabel);

                    var playerJoinedLabel = new Label
                    {
                        name = k_PlayerJoinedLabelName
                    };
                    playerJoinedLabel.AddToClassList(BlocksTheme.Label);
                    container.Add(playerJoinedLabel);

                    var playerLastUpdatedLabel = new Label
                    {
                        name = k_PlayerLastUpdatedLabelName
                    };
                    playerLastUpdatedLabel.AddToClassList(BlocksTheme.Label);
                    playerLastUpdatedLabel.AddToClassList(BlocksTheme.SpaceBottomSmall);
                    container.Add(playerLastUpdatedLabel);

                    var playerPropertiesList = new ListView
                    {
                        name = k_PlayerPropertiesListName,
                        headerTitle = k_PlayerPropertiesHeaderText,
                        virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                        makeNoneElement = () =>
                        {
                            var label = new Label(k_ListIsEmptyLabel);
                            label.AddToClassList(BlocksTheme.Label);
                            return label;
                        },
                        makeHeader = () =>
                        {
                            var playerPropertiesHeader = new Label
                            {
                                text = k_PlayerPropertiesHeaderText
                            };
                            playerPropertiesHeader.AddToClassList(BlocksTheme.Header);
                            playerPropertiesHeader.AddToClassList(BlocksTheme.HeaderXSmall);
                            playerPropertiesHeader.AddToClassList(BlocksTheme.SpaceBottomSmall);
                            return playerPropertiesHeader;
                        }
                    };
                    container.Add(playerPropertiesList);

                    return container;
                },
                bindItem = (element, index) =>
                {
                    var player = m_ViewModel.PlayerStats[index];
                    element.Q<Label>(k_PlayerHeaderName).text = $"Player {index + 1}";
                    element.Q<Label>(k_PlayerIdLabelName).text = $"ID: {player.Id}";
                    element.Q<Label>(k_PlayerJoinedLabelName).text = $"Joined: {player.Joined}";
                    element.Q<Label>(k_PlayerLastUpdatedLabelName).text = $"Last Updated: {player.LastUpdated}";

                    var playerPropertiesList = element.Q<ListView>(k_PlayerPropertiesListName);
                    if (playerPropertiesList == null)
                        return;

                    playerPropertiesList.itemsSource = player.PlayerPropertiesList;
                    playerPropertiesList.makeItem = () =>
                    {
                        var propertyContainer = new VisualElement();

                        var propertyKeyLabel = new Label
                        {
                            name = k_PlayerPropertiesIdKeyName
                        };
                        propertyKeyLabel.AddToClassList(BlocksTheme.Label);
                        propertyContainer.Add(propertyKeyLabel);

                        var propertyValueLabel = new Label
                        {
                            name = k_PlayerPropertiesIdValueName
                        };
                        propertyValueLabel.AddToClassList(BlocksTheme.Label);
                        propertyContainer.Add(propertyValueLabel);

                        var propertyVisibilityLabel = new Label
                        {
                            name = k_PlayerPropertiesIdVisibilityName
                        };
                        propertyVisibilityLabel.AddToClassList(BlocksTheme.Label);
                        propertyContainer.Add(propertyVisibilityLabel);
                        return propertyContainer;
                    };
                    playerPropertiesList.bindItem = (propertyElement, propertyIndex) =>
                    {
                        var sessionProperty = player.PlayerPropertiesList[propertyIndex];
                        propertyElement.Q<Label>(k_PlayerPropertiesIdKeyName).text = $"Key: {sessionProperty.Key}";
                        propertyElement.Q<Label>(k_PlayerPropertiesIdValueName).text = $"Value: {sessionProperty.Value}";
                        propertyElement.Q<Label>(k_PlayerPropertiesIdVisibilityName).text = $"Visibility: {sessionProperty.Visibility}";
                    };
                }
            };
            playerStatsListView.AddToClassList(BlocksTheme.ListViewNoHover);
            var playerStatsBinding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(SessionInfoViewModel.PlayerStats)),
                bindingMode = BindingMode.ToTarget,
            };
            playerStatsListView.SetBinding(new BindingId(nameof(ListView.itemsSource)), playerStatsBinding);
            scrollViewParent.Add(playerStatsListView);
            m_Bindings.Add(playerStatsBinding);

            RegisterCallback<AttachToPanelEvent>(_ => UpdateBindings());
            RegisterCallback<DetachFromPanelEvent>(_ => CleanupBindings());
        }

        void UpdateBindings()
        {
            CleanupBindings();

            m_ViewModel = new SessionInfoViewModel(m_SessionType);
            foreach (var binding in m_Bindings)
            {
                binding.dataSource = m_ViewModel;
            }
        }

        void CleanupBindings()
        {
            m_ViewModel?.Dispose();
            m_ViewModel = null;
            foreach (var binding in m_Bindings)
            {
                binding.dataSource = null;
            }
        }
    }
}
