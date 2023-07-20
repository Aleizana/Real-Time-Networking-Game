using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmegaRace
{
    public class GameSceneNull : IGameScene
    {
        void IGameScene.Enter() { }
        void IGameScene.Update() { }
        void IGameScene.Draw() { }
        void IGameScene.Leave() { }
    }
}
