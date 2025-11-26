using Unity.Services.Multiplayer;
using UnityEngine;

namespace Blocks.Sessions
{
    [CreateAssetMenu(fileName = nameof(QuickJoinSettings), menuName = "Services/Blocks/Session/" + nameof(QuickJoinSettings))]
    public class QuickJoinSettings : ScriptableObject
    {
        [Header("QuickJoinSettings")]
        [Tooltip("The timeout in seconds for the quick join to stop trying to join and either fail or create its own session is createSession is true.")]
        public float timeout = 5f;
        [Tooltip("If true, the quick join will create a session if it cannot find one to join withing the given timeout time frame.")]
        public bool createSession = true;

        public QuickJoinOptions ToQuickJoinOptions()
        {
            return new QuickJoinOptions
            {
                Timeout = System.TimeSpan.FromSeconds(timeout),
                CreateSession = createSession
            };
        }
    }
}
