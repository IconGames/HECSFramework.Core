using HECSFramework.Core;
using System;

namespace Components
{
    [Serializable]
    [Documentation("GameLogic", "��������� � ������� �� ������ ������")]
    public partial class AppVersionComponent : BaseComponent
    {
        public int Version = 101;
    }
}