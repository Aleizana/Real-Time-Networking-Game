using System;
using System.Diagnostics;
using Lidgren.Network;
using System.Net;
using System.Runtime.InteropServices;
using System.IO;


namespace OmegaRace
{
    class OmegaRaceGame : Azul.Game
    {
        public const string WindowCaption = "Omega Race (Not Networked)";
        public const int Width = 800;
        public const int Height = 500;

        //-----------------------------------------------------------------------------
        // Game::Initialize()
        //		Allows the engine to perform any initialization it needs to before 
        //      starting to run.  This is where it can query for any required services 
        //      and load any non-graphic related content. 
        //-----------------------------------------------------------------------------
        public override void Initialize()
        {
            // Game Window Device setup 
            this.SetWindowName(WindowCaption);
            this.SetWidthHeight(Width, Height);
            this.SetClearColor(0, 0, 0, 1);
        }

        //-----------------------------------------------------------------------------
        // Game::LoadContent()
        //		Allows you to load all content needed for your engine,
        //	    such as objects, graphics, etc.
        //-----------------------------------------------------------------------------
        public override void LoadContent()
        {
            TimeManager.Initialize(GetTime());
            GameManager.SetMode();
        }

        //-----------------------------------------------------------------------------
        // Game::Update()
        //      Called once per frame, update data, tranformations, etc
        //      Use this function to control process order
        //      Input, AI, Physics, Animation, and Graphicsf
        //-----------------------------------------------------------------------------

        // static int number = 0;
        public override void Update()
        {
            TimeManager.Update(GetTime());

            GameManager.Update();

            InputManager.Update();
        }

        //-----------------------------------------------------------------------------
        // Game::Draw()
        //		This function is called once per frame
        //	    Use this for draw graphics to the screen.
        //      Only do rendering here
        //-----------------------------------------------------------------------------
        public override void Draw()
        {
            GameManager.Draw();

            ScreenLog.Display(); // keep as last
        }

        //-----------------------------------------------------------------------------
        // Game::UnLoadContent()
        //       unload content (resources loaded above)
        //       unload all content that was loaded before the Engine Loop started
        //-----------------------------------------------------------------------------
        public override void UnLoadContent()
        {
        }

    }
}

