using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmegaRace
{
    public class Glyph
    {
        public int key { get; }
        public int x { get; }
        public int y { get; }
        public int width { get; }
        public int height { get; }

        SpriteFont sprFont;

        public Glyph()
        {
            this.key = -1;
            this.x = -1;
            this.y = -1;
            this.width = -1;
            this.height = -1;
            this.sprFont = null;
        }
        public Glyph( SpriteFont sf , int key, int x, int y, int width, int height)
        {
            this.sprFont = sf;
            this.key = key;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }
}
