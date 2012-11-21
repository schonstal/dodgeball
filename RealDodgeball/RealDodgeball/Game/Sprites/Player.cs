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
  class Player : Sprite {
    public Sprite shadow;

    PlayerIndex playerIndex;
    Team team;
    float movementAccel = 5000.0f;

    float charge = 0;
    float chargeAmount = 1000.0f;
    float maxCharge = 2500.0f;
    float minCharge = 700.0f;

    bool hasBall = false;
    Ball ball = null;

    bool triggerHeld = false;

    public Player(PlayerIndex playerIndex, Team team) : base(0,0) {
      this.playerIndex = playerIndex;
      this.team = team;
      
      maxSpeed = 250f;
      drag = new Vector2(2500,2500);
      
      loadGraphic("player", 34, 34);
      addAnimation("idle", new List<int> { 0, 1, 2, 3 }, 15, true);

      addAnimation("runForward", new List<int> { 0, 1, 2, 3 }, 15, true);
      addAnimation("runBackward", new List<int> { 3, 2, 1, 0 }, 15, true);
      addAnimation("runUp", new List<int> { 3, 2, 1, 0 }, 15, true);
      addAnimation("runDown", new List<int> { 3, 2, 1, 0 }, 15, true);

      height = 22;
      offset.Y = -5;
      width = 18;
      offset.X = -9;

      shadow = new Sprite(0, 0);
      shadow.loadGraphic("playerShadow", 13, 12);
      shadow.color = new Color(0x1c, 0x1c, 0x1c);
      shadow.z = 0;
      G.state.add(shadow);
    }

    public override void Update() {
      if(velocity.X > 50) play("runBackward");
      else if(velocity.X < -50) play("runForward");
      else stop();

      acceleration.X = G.input.ThumbSticks(playerIndex).Left.X * movementAccel;
      //DEBUG - REMOVE
      if(Keyboard.GetState().IsKeyDown(Keys.A)) acceleration.X -= movementAccel;
      if(Keyboard.GetState().IsKeyDown(Keys.D)) acceleration.X += movementAccel;
      //END DEBUG
      if(Math.Sign(acceleration.X) != Math.Sign(velocity.X)) acceleration.X *= 15;

      acceleration.Y = G.input.ThumbSticks(playerIndex).Left.Y * -movementAccel;
      //DEBUG - REMOVE
      if(Keyboard.GetState().IsKeyDown(Keys.W)) acceleration.Y -= movementAccel;
      if(Keyboard.GetState().IsKeyDown(Keys.S)) acceleration.Y += movementAccel;
      //END DEBUG
      if(Math.Sign(acceleration.Y) != Math.Sign(velocity.Y)) acceleration.Y *= 15;

      if(hasBall) {
        ball.x = x + 5;
        ball.y = y + 10;
        if(G.input.Triggers(playerIndex).Right > 0.3 || Keyboard.GetState().IsKeyDown(Keys.Space)) {
          triggerHeld = true;
          maxSpeed = 150f;
          if(charge < maxCharge)
            charge += chargeAmount * G.elapsed;
          charge = MathHelper.Clamp(charge, minCharge, maxCharge);
        } else {
          if(triggerHeld) FlingBall();
          triggerHeld = false;
          maxSpeed = 250f;
          charge = 0;
        }
      } else {
        charge = 0;
      }

      base.Update();
    }

    public override void postUpdate() {
      if(x < 0) x = 0;
      if(y < 0) y = 0;
      if(y > PlayState.ARENA_HEIGHT - height) y = PlayState.ARENA_HEIGHT - height;
      if(x > PlayState.ARENA_WIDTH - width) x = PlayState.ARENA_WIDTH - width;
      z = shadow.y;
      shadow.y = y + 18;
      shadow.x = x + 4;

      base.postUpdate();
    }

    private void FlingBall() {
      Vector2 flingDirection = Vector2.Normalize(G.input.ThumbSticks(playerIndex).Right);
      if(float.IsNaN(flingDirection.X) || float.IsNaN(flingDirection.Y)) {
        ball.Fling(-1, 0, charge);
      } else {
        ball.Fling(flingDirection.X, -flingDirection.Y, charge);
      }
      hasBall = false;
    }

    public void PickUpBall(Ball ball) {
      if(!ball.dangerous) {
        hasBall = true;
        this.ball = ball;
        this.ball.pickedUp();
      }
    }
  }

  public enum Team {
    Left = 0x01,
    Right = 0x02
  }
}
