﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Dodgeball.Engine;

namespace Dodgeball.Game {
  public class PlayState : GameState {
    public const int ARENA_WIDTH = 512;
    public const int ARENA_HEIGHT = 288;
    Sprite dot;
    Ball ball;

    Group players = new Group();

    public override void Create() {
      //G.visualDebug = true;

      dot = new Sprite(0,0);
      dot.loadGraphic("Dot", ARENA_WIDTH, ARENA_HEIGHT);
      dot.color = new Color(0x2c,0x2c,0x2c);
      dot.z = -1;
      add(dot);

      dot = new Sprite(-100, ARENA_HEIGHT);
      dot.loadGraphic("Dot", 1000, ARENA_HEIGHT);
      dot.color = Color.Black;
      dot.z = 100000;
      add(dot);

      ball = new Ball();
      ball.x = 40;
      ball.y = 40;
      add(ball);

      //probably want to let people pick their team later
      players.add(new Player(PlayerIndex.One, Team.Left));
      //players.add(new Player(PlayerIndex.Two, Team.Right));
      //players.add(new Player(PlayerIndex.Three, Team.Left));
      //players.add(new Player(PlayerIndex.Four, Team.Right));
      players.Each((player) => add(player));
    }

    public override void Update() {
      members = members.OrderBy((member) => member.z).ToList();

      players.Each((player) => {
        if(Util.Overlaps(((Player)player).shadow, ((Ball)ball).shadow)) {
          ((Player)player).PickUpBall(ball);
          //ball.visible = false;
        }
      });

      base.Update();
    }

    public override void Draw() {
      base.Draw();
    }
  }
}
