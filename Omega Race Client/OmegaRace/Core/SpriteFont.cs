using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Xml;
using System.Diagnostics;

namespace OmegaRace
{
    public class SpriteFont
    {
        Azul.Color defaulColor = new Azul.Color(1.0f, 1.0f, 1.0f);

        SortedList<int, Glyph> charlist;
        Azul.Texture charTexture;
        Azul.Sprite RenderGlyph;
        Azul.Rect textRect;
        Azul.Rect scrnRect;
        float scale;
        int fontHeight;
        public SpriteFont(String assetName, float s = 1)
        {
            scale = s;
            textRect = new Azul.Rect();
            scrnRect = new Azul.Rect();
            charTexture = new Azul.Texture(assetName + ".tga");
            RenderGlyph = new Azul.Sprite(charTexture, textRect, scrnRect, defaulColor);

            System.Xml.XmlTextReader reader = new XmlTextReader(assetName + ".xml");
            charlist = new SortedList<int, Glyph>();

            int key = '\0';
            int x = -1;
            int y = -1;
            int width = -1;
            int height = -1;


            fontHeight = 0;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        if (reader.GetAttribute("key") != null)
                        {
                            key = Convert.ToInt32(reader.GetAttribute("key"));
                        }
                        else if (reader.Name == "x")
                        {
                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    x = Convert.ToInt32(reader.Value);
                                    break;
                                }
                            }
                        }
                        else if (reader.Name == "y")
                        {
                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    y = Convert.ToInt32(reader.Value);
                                    break;
                                }
                            }
                        }
                        else if (reader.Name == "width")
                        {
                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    width = Convert.ToInt32(reader.Value);
                                    break;
                                }
                            }
                        }
                        else if (reader.Name == "height")
                        {
                            while (reader.Read())
                            {
                                if (reader.NodeType == XmlNodeType.Text)
                                {
                                    height = Convert.ToInt32(reader.Value);
                                    break;
                                }
                            }
                        }
                        break;

                    case XmlNodeType.EndElement: 
                        if (reader.Name == "character")
                        {
                            charlist.Add(key, new Glyph(this, key, x, y, width, height));
                            fontHeight = Math.Max(fontHeight, height);
                        }
                        break;
                }
            }

        }

        public int GetFontHeight() { return (int) (fontHeight * scale); }

        public int Render(int glyph, int posx, int posy, Azul.Color c)
        {
            Glyph g = charlist[glyph];

            textRect.Set(g.x, g.y, g.width, g.height);
            scrnRect.Set(posx + g.width/2, posy, g.width * scale, g.height * scale);
            RenderGlyph.SwapTextureRect(textRect);
            RenderGlyph.SwapScreenRect(scrnRect);
            RenderGlyph.SwapColor(c);

            RenderGlyph.Update();
            RenderGlyph.Render();

            return (int) (g.width * scale);
        }

        public int Render(int glyph, int posx, int posy)
        {
            return Render(glyph, posx, posy, defaulColor);
        }

        public int Render(string txt, int posx, int posy, Azul.Color c)
        {
            float deltax;

            for (int i = 0; i < txt.Length; i++)
            {
                int key = Convert.ToByte(txt[i]);

                deltax = Render(key, posx, posy, c);
                posx += (int) deltax;

            }

            return posx;
        }

        public int Render(string txt, int posx, int posy)
        {
            return Render(txt, posx, posy, defaulColor);
        }
    }
}
