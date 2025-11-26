using System.Collections.Generic;
using Blocks.Common;
using Blocks.Sessions.Common;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blocks.Sessions
{
    [UxmlElement]
    public partial class JoinSessionByCode : VisualElement
    {
        const string k_SessionCodeTextFieldPlaceholder = "Enter Session Code";
        const string k_JoinButtonText = "JOIN";

        [UxmlAttribute, CreateProperty]
        SessionSettings SessionSettings
        {
            get => m_SessionSettings;
            set
            {
                if (m_SessionSettings == value)
                {
                    return;
                }

                m_SessionSettings = value;
                if (panel != null)
                {
                    UpdateBindings();
                }
            }
        }
        SessionSettings m_SessionSettings;

        JoinSessionByCodeViewModel m_ViewModel;

        readonly List<DataBinding> m_Bindings = new();

        public JoinSessionByCode()
        {
            AddToClassList(BlocksTheme.ContainerHorizontal);

            var sessionCodeTextField = new TextField
            {
                textEdition =
                {
                    placeholder = k_SessionCodeTextFieldPlaceholder,
                    hidePlaceholderOnFocus = true
                }
            };
            sessionCodeTextField.AddToClassList(BlocksTheme.TextField);
            sessionCodeTextField.AddToClassList(BlocksTheme.SpaceRight);
            var sessionCodeBinding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(m_ViewModel.SessionCode)),
                bindingMode = BindingMode.ToSource
            };
            sessionCodeTextField.SetBinding("value", sessionCodeBinding);
            Add(sessionCodeTextField);
            m_Bindings.Add(sessionCodeBinding);

            var createSessionButton = new Button
            {
                text = k_JoinButtonText
            };
            createSessionButton.AddToClassList(BlocksTheme.Button);
            var createSessionBinding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(m_ViewModel.CanJoinSession)),
                bindingMode = BindingMode.ToTarget
            };
            createSessionButton.SetBinding(new BindingId(nameof(enabledSelf)), createSessionBinding);
            createSessionButton.clicked += JoinSession;
            Add(createSessionButton);
            m_Bindings.Add(createSessionBinding);

            RegisterCallback<AttachToPanelEvent>(_ => UpdateBindings());
            RegisterCallback<DetachFromPanelEvent>(_ => CleanupBindings());
        }

        void JoinSession()
        {
            if (!m_ViewModel.AreMultiplayerServicesInitialized())
            {
                Debug.LogError("Multiplayer Services are not initialized. You can initialize them with default settings by adding a Servicesinitialization and PlayerAuthentication components in your scene.");
                return;
            }

            _ = m_ViewModel.JoinSessionByCodeAsync(m_SessionSettings.ToJoinSessionOptions());
        }

        void UpdateBindings()
        {
            CleanupBindings();

            m_ViewModel = new JoinSessionByCodeViewModel(m_SessionSettings?.sessionType);
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
