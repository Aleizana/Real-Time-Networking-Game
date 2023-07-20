using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmegaRace
{
    public class TextureCollection
    {
        // Main objects
        public static Azul.Texture shipTexture = new Azul.Texture("PlayerShip.tga");
        public static Azul.Texture missileTexture = new Azul.Texture("Missile.tga");
        public static Azul.Texture mineTexture = new Azul.Texture("Mine.tga");
        public static Azul.Texture mineTexture2 = new Azul.Texture("Mine2.tga");
        public static Azul.Texture fenceTexture = new Azul.Texture("FenceTall2.tga");
        public static Azul.Texture fencePostTexture = new Azul.Texture("FencePost.tga");

        // Particles
        public static Azul.Texture explosionText = new Azul.Texture("explosion.tga");
        public static Azul.Texture fenceText1 = new Azul.Texture("FenceTall1.tga");
        public static Azul.Texture fenceText2 = new Azul.Texture("FenceTall2.tga");
        public static Azul.Texture fenceText3 = new Azul.Texture("FenceTall3.tga");
        public static Azul.Texture fenceText4 = new Azul.Texture("FenceTall4.tga");
        public static Azul.Texture fenceText5 = new Azul.Texture("FenceTall5.tga");
        public static Azul.Texture fenceText6 = new Azul.Texture("FenceTall6.tga");
        public static Azul.Texture fenceText7 = new Azul.Texture("FenceTall7.tga");

        // Splash pages
        public static Azul.Texture titleScreenText = new Azul.Texture("MainMenu.tga");

        // Fonts
        public static SpriteFont scoreFont = new SpriteFont("Arial20pt");
        public static SpriteFont ScreenLogFont = new SpriteFont("LucidaTpwrtr16pt");

    }
}
