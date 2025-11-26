using System;
using Blocks.Common;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Blocks.Sessions.Common
{
    [UxmlElement]
    public partial class SessionStatusLabel : Label
    {
        [UxmlAttribute, CreateProperty]
        public string SessionType
        {
            get => m_SessiontType;
            set
            {
                if (m_SessiontType == value)
                    return;

                m_SessiontType = value;
                if (panel != null)
                    UpdateBindings();
            }
        }

        string m_SessiontType;

        DataBinding m_Binding;

        public SessionStatusLabel()
        {
            AddToClassList(BlocksTheme.Label);

            m_Binding = new DataBinding()
            {
                dataSourcePath = new PropertyPath(nameof(SessionStatusViewModel.DisplayText)),
                bindingMode = BindingMode.ToTarget,
            };

            SetBinding(new BindingId(nameof(text)), m_Binding);

            RegisterCallback<AttachToPanelEvent>(evt => UpdateBindings());
            RegisterCallback<DetachFromPanelEvent>(evt => RemoveBindings());
        }

        void RemoveBindings()
        {
            if (m_Binding.dataSource is IDisposable disposable)
                disposable.Dispose();
            m_Binding.dataSource = null;
        }

        void UpdateBindings()
        {
            RemoveBindings();
            m_Binding.dataSource = new SessionStatusViewModel(SessionType);
        }
    }
}
