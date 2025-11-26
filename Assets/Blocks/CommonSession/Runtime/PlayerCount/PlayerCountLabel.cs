using System;
using Blocks.Common;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Blocks.Sessions.Common
{
    [UxmlElement]
    public partial class PlayerCountLabel : Label
    {
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

        DataBinding m_DataBinding;

        public PlayerCountLabel()
        {
            AddToClassList(BlocksTheme.Label);
            m_DataBinding = new DataBinding()
            {
                dataSourcePath = new PropertyPath(nameof(PlayerCountViewModel.DisplayText)),
                bindingMode = BindingMode.ToTarget
            };
            SetBinding(new BindingId(nameof(text)), m_DataBinding);

            RegisterCallback<AttachToPanelEvent>(_ => UpdateBindings());
            RegisterCallback<DetachFromPanelEvent>(_ => CleanupBindings());
        }

        void UpdateBindings()
        {
            CleanupBindings();
            m_DataBinding.dataSource = new PlayerCountViewModel(m_SessionType);
        }

        void CleanupBindings()
        {
            if (m_DataBinding.dataSource is IDisposable disposable)
            {
                disposable.Dispose();
            }

            m_DataBinding.dataSource = null;
        }
    }
}
