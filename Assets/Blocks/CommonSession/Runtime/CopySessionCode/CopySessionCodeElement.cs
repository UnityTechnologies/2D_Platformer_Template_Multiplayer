using Blocks.Common;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blocks.Sessions.Common
{
    [UxmlElement]
    public partial class CopySessionCodeElement : VisualElement
    {
        const string k_CopySessionCodeButtonText = "COPY";

        [CreateProperty, UxmlAttribute]
        public string SessionType
        {
            get => m_SessionType;
            set
            {
                if (m_SessionType == value)
                    return;

                m_SessionType = value;
                if (panel != null)
                    UpdateBindings();
            }
        }
        string m_SessionType;

        CopySessionCodeViewModel m_ViewModel;

        readonly List<DataBinding> m_Bindings = new();

        public CopySessionCodeElement()
        {
            AddToClassList(BlocksTheme.ContainerHorizontal);

            var hasSessionCode = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(CopySessionCodeViewModel.HasSessionCode)),
                bindingMode = BindingMode.ToTarget
            };
            SetBinding(new BindingId(nameof(enabledSelf)), hasSessionCode);

            var sessionCode = new TextField
            {
                textEdition =
                {
                    isReadOnly = true
                },
                selectAllOnMouseUp = false,
                selectAllOnFocus = false
            };
            sessionCode.AddToClassList(BlocksTheme.TextField);
            sessionCode.AddToClassList(BlocksTheme.SpaceRight);
            var sessionCodeBinding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(CopySessionCodeViewModel.SessionCode)),
                bindingMode = BindingMode.ToTarget
            };
            sessionCode.SetBinding("value", sessionCodeBinding);
            Add(sessionCode);
            m_Bindings.Add(sessionCodeBinding);

            var copySessionCodeButton = new Button
            {
                text = k_CopySessionCodeButtonText
            };
            copySessionCodeButton.AddToClassList(BlocksTheme.Button);
            copySessionCodeButton.clicked += CopySessionCode;
            Add(copySessionCodeButton);
            m_Bindings.Add(hasSessionCode);

            RegisterCallback<AttachToPanelEvent>(_ => UpdateBindings());
            RegisterCallback<DetachFromPanelEvent>(_ => CleanupBindings());
        }

        void CopySessionCode()
        {
            if (string.IsNullOrEmpty(m_ViewModel?.SessionCode))
                return;

            GUIUtility.systemCopyBuffer = m_ViewModel.SessionCode;
        }

        void UpdateBindings()
        {
            CleanupBindings();

            m_ViewModel = new CopySessionCodeViewModel(SessionType);
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
