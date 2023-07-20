using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace OmegaRace
{
    public class AnimationParticle : Particle
    {
        Stopwatch timer;
        List<Azul.Sprite> animation;

        int frame;
        float frameSpeed;

        public AnimationParticle(Azul.Rect textRect, Azul.Rect destRect, Azul.Texture text, Azul.Color color)
            : base(textRect, destRect, text, color)
        {
            timer = new Stopwatch();

            timer.Start();
            frameSpeed = 35;
            animation = new List<Azul.Sprite>();
            animation.Add(pSprite);

            //Debug.WriteLine("Particle {0} created", getID());
        }

        public void setAnimation(List<Azul.Sprite> anim)
        {
            animation = anim;
        }

        public void StartAnimation(float posx, float posy)
        {
            frame = 0;
            pSprite.x = posx;
            pSprite.y = posy;
            pSprite.Update();
            timer.Start();
            GameManager.AddGameObject(this);
        }

        public override void Update()
        {
            if (frameSpeed < timer.Elapsed.Milliseconds)
            {
                timer.Restart();
                frame++;
                if (frame < animation.Count)
                {
                    pSprite = animation[frame];
                    pSprite.Update();
                }
                else
                {
                    timer.Stop();
                    GameManager.DestroyObject(this);
                }
            }

        }
    }
}
