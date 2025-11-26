using System;
using Blocks.Common;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Blocks.Sessions.Common
{
    [UxmlElement]
    public partial class LeaveSessionButton : Button
    {
        const string k_LeaveSessionButtonText = "Leave Session";

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
        LeaveSessionViewModel m_ViewModel;

        public LeaveSessionButton()
        {
            text = k_LeaveSessionButtonText;

            AddToClassList(BlocksTheme.Button);
            m_DataBinding = new DataBinding() { dataSourcePath = new PropertyPath(nameof(LeaveSessionViewModel.CanLeaveSession)), bindingMode = BindingMode.ToTarget };
            SetBinding(new BindingId(nameof(enabledSelf)), m_DataBinding);
            clicked += LeaveSession;

            RegisterCallback<AttachToPanelEvent>(_ => UpdateBindings());
            RegisterCallback<DetachFromPanelEvent>(_ => CleanupBindings());
        }

        void LeaveSession()
        {
            _ = m_ViewModel.LeaveSession(m_SessionType);
        }

        void UpdateBindings()
        {
            CleanupBindings();
            m_ViewModel = new LeaveSessionViewModel(m_SessionType);
            m_DataBinding.dataSource = m_ViewModel;
        }

        void CleanupBindings()
        {
            if (m_DataBinding.dataSource is IDisposable disposable)
            {
                disposable.Dispose();
            }

            m_ViewModel = null;
            m_DataBinding.dataSource = null;
        }
    }
}
