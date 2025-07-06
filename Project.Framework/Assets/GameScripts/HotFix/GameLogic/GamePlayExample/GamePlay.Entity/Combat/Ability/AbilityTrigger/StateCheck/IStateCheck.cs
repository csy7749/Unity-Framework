using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLogic.Combat
{
    public interface IStateCheck
    {
        bool IsInvert { get; }
        bool CheckWith(Entity target);
    }
}
