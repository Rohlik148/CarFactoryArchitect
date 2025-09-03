using MonoGameLibrary.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace CarFactoryArchitect;
public class Conveyor
{
    public AnimatedSprite ConveyorUp { get; set; }
    public AnimatedSprite ConveyorRight { get; set; }
    public AnimatedSprite ConveyorDown { get; set; }
    public AnimatedSprite ConveyorLeft { get; set; }

    public Conveyor(TextureAtlas atlas, Single scale)
    {
        ConveyorUp = atlas.CreateAnimatedSprite("conveyor-animation-up");
        ConveyorUp.Scale = new Vector2(scale, scale);

        ConveyorRight = atlas.CreateAnimatedSprite("conveyor-animation-right");
        ConveyorRight.Scale = new Vector2(scale, scale);

        ConveyorDown = atlas.CreateAnimatedSprite("conveyor-animation-down");
        ConveyorDown.Scale = new Vector2(scale, scale);

        ConveyorLeft = atlas.CreateAnimatedSprite("conveyor-animation-left");
        ConveyorLeft.Scale = new Vector2(scale, scale);
    }
}
