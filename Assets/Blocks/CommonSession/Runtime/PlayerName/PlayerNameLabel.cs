using System;
using Blocks.Common;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Blocks.Sessions.Common
{
    [UxmlElement]
    public partial class PlayerNameLabel : Label
    {
        [UxmlAttribute, CreateProperty]
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

        public PlayerNameLabel()
        {
            AddToClassList(BlocksTheme.Header);
            AddToClassList(BlocksTheme.HeaderSmall);
            m_DataBinding = new DataBinding()
            {
                dataSourcePath = new PropertyPath(nameof(PlayerNameViewModel.PlayerName)),
                bindingMode = BindingMode.ToTarget
            };
            SetBinding(new BindingId(nameof(text)), m_DataBinding);

            RegisterCallback<AttachToPanelEvent>(_ => UpdateBindings());
            RegisterCallback<DetachFromPanelEvent>(_ => CleanupBindings());
        }

        void UpdateBindings()
        {
            CleanupBindings();
            m_DataBinding.dataSource = new PlayerNameViewModel(m_SessionType);
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
