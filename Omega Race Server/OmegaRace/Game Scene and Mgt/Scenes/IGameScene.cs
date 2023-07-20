using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmegaRace
{
    public interface IGameScene
    {
        void Enter();
        void Update();
        void Draw();
        void Leave();

    }
}
