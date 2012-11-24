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
    public const float MIN_RUN_SPEED = 50;
    public const float BALL_OFFSEET_X = 5f;
    public const float BALL_OFFSEET_Y = 8f;
    public const float CATCH_THRESHOLD = 0.06f;

    public const float MAX_RUN_SPEED = 175f;
    public const float CHARGE_RUN_SPEED = 100f;

    public const float MIN_THROW_FPS = 20f;
    public const float MAX_THROW_FPS = 60f;
    public const float MIN_THROW_DELAY = 0.05f;
    public const float MAX_THROW_DELAY = 0.5f;

    public Sprite shadow;

    PlayerIndex playerIndex;
    Team team;
    Heading heading;
    float movementAccel = 5000.0f;
    Retical retical;

    float charge = 0;
    float flungAtCharge = 0;
    float chargeAmount = 1000.0f;
    float maxCharge = 2000.0f;
    float minCharge = 700.0f;

    Ball ball = null;

    bool triggerHeld = false;
    bool triggerWasHeld = false;
    bool throwing = false;

    Vector2[][] throwOffsets = new Vector2[Enum.GetNames(typeof(Heading)).Length][];

    float catchTimer = 0f;

    public bool onLeft {
      get { return team == Team.Left; }
    }

    public bool onRight {
      get { return team == Team.Right; }
    }

    public SpriteMode Mode {
      set { sheetOffset.Y = (int)value * GraphicHeight; }
    }

    public bool Stunned {
      get { return throwing; }
    }

    public Player(PlayerIndex playerIndex, Team team, float X=0f, float Y=0f) : base(X,Y) {
      this.playerIndex = playerIndex;
      this.team = team;
      heading = Heading.Forward;
      
      maxSpeed = MAX_RUN_SPEED;
      drag = new Vector2(2500,2500);
      
      loadGraphic("player", 34, 34);
      addAnimation("idle", new List<int> { 0, 1, 2, 3 }, 10, true);

      addAnimation("runForward", new List<int> { 12, 13, 14, 15 }, 15, true);
      addAnimation("runBackward", new List<int> { 16, 17, 18, 19 }, 15, true);
      addAnimation("runUpForward", new List<int> { 4, 5, 6, 7 }, 15, true);
      addAnimation("runDownForward", new List<int> { 8, 9, 10, 11 }, 15, true);
      addAnimation("runUpBackward", new List<int> { 11, 10, 9, 8 }, 15, true);
      addAnimation("runDownBackward", new List<int> { 7, 6, 5, 4 }, 15, true);
      addAnimation("throw", new List<int> { 0, 1, 2, 3 }, 10, false);
      addAnimationCallback("throw", onThrowCallback);
      addOnCompleteCallback("throw", onThrowCompleteCallback);

      throwOffsets[(int)Heading.Up] = new Vector2[3] {
          new Vector2(0, 0),
          new Vector2(0, 0),
          new Vector2(0, 0)
        };
      throwOffsets[(int)Heading.UpMid] = new Vector2[3] {
          new Vector2(0, 0),
          new Vector2(0, 0),
          new Vector2(0, 0)
        };
      throwOffsets[(int)Heading.Forward] = new Vector2[3] {
          new Vector2(1, 0),
          new Vector2(3, 0),
          new Vector2(1, 0)
        };
      throwOffsets[(int)Heading.DownMid] = new Vector2[3] {
          new Vector2(0, 0),
          new Vector2(0, 0),
          new Vector2(0, 0)
        };
      throwOffsets[(int)Heading.Down] = new Vector2[3] {
          new Vector2(0, 0),
          new Vector2(0, 0),
          new Vector2(0, 0)
        };

      //No actual hit yet, this should substitute for now
      addAnimation("hit", new List<int> { 12, 13, 14, 15 }, 60, true);

      height = 22;
      offset.Y = -5;
      width = 18;
      offset.X = -9;

      shadow = new Sprite(0, 0);
      shadow.loadGraphic("playerShadow", 13, 12);
      shadow.color = new Color(0x1c, 0x1c, 0x1c);
      shadow.z = 0;
      G.state.add(shadow);

      retical = new Retical(playerIndex, team);
      retical.visible = false;
      G.state.add(retical);
    }

    public override void Update() {
      updateAnimation();
      updatePhysics();
      updateHeading();

      triggerWasHeld = triggerHeld;
      if(G.input.Triggers(playerIndex).Right > 0.3) {
        triggerHeld = true;
      } else {
        triggerHeld = false;
      }

      if(this.ball != null) {
        if(charge > 0) {
          Mode = SpriteMode.Charge;
        } else {
          Mode = SpriteMode.Hold;
        }
        ball.x = x + BALL_OFFSEET_X;
        ball.y = y + BALL_OFFSEET_Y;
        if(triggerHeld) {
          retical.visible = true;
          triggerHeld = true;
          maxSpeed = CHARGE_RUN_SPEED;
          if(charge < maxCharge)
            charge += chargeAmount * G.elapsed;
          charge = MathHelper.Clamp(charge, minCharge, maxCharge);
        } else {
          retical.visible = false;
          if(triggerWasHeld) FlingBall();
          triggerHeld = false;
          maxSpeed = MAX_RUN_SPEED;
          charge = 0;
        }
      } else {
        Mode = SpriteMode.Neutral;
        charge = 0;
      }

      if(throwing) Mode = SpriteMode.Misc;

      retical.charge = charge / maxCharge;

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

      if(team == Team.Right) {
        retical.X = x + 5;
        retical.Y = y + 24;
      } else {
        retical.X = x + 15;
        retical.Y = y + 24;
      }
      base.postUpdate();
    }

    void FlingBall() {
      if(ball != null) {
        flungAtCharge = charge;

        Vector2 flingDirection = Vector2.Normalize(retical.Direction);
        if(float.IsNaN(flingDirection.X) || float.IsNaN(flingDirection.Y)) {
          ball.Fling(-1, 0, charge);
        } else {
          ball.Fling(flingDirection.X, flingDirection.Y, charge);
        }
        ball = null;
        play("throw");
        throwing = true;
        animation.reset();
        animation.FPS = MIN_THROW_FPS + ((charge / maxCharge) * (MAX_THROW_FPS - MIN_THROW_FPS));
        G.state.DoForSeconds(0.2f,
          () => GamePad.SetVibration(playerIndex, 0.4f, 0.2f),
          () => GamePad.SetVibration(playerIndex, 0, 0));
      }
    }

    void updateAnimation() {
      if(throwing) {
        play("throw");
      } else if(Math.Abs(velocity.X) > Math.Abs(velocity.Y)) {
        if(velocity.X > MIN_RUN_SPEED) play("runBackward");
        else if(velocity.X < -MIN_RUN_SPEED) play("runForward");
        else play("idle");
      } else {
        if(velocity.Y > MIN_RUN_SPEED) {
          play(velocity.X < 0 ? "runDownForward" : "runDownBackward");
        } else if(velocity.Y < -MIN_RUN_SPEED) {
          play(velocity.X < 0 ? "runUpForward" : "runUpBackward");
        } else play("idle");
      }

      if(currentAnimation != "throw" && currentAnimation != "idle") {
        animation.FPS = velocity.Length() / 14f;
      }
    }

    void updatePhysics() {
      if(!Stunned) {
        acceleration.X = G.input.ThumbSticks(playerIndex).Left.X * movementAccel;
        if(Math.Sign(acceleration.X) != Math.Sign(velocity.X)) acceleration.X *= 15;

        acceleration.Y = G.input.ThumbSticks(playerIndex).Left.Y * -movementAccel;
        if(Math.Sign(acceleration.Y) != Math.Sign(velocity.Y)) acceleration.Y *= 15;
      } else {
        acceleration.X = acceleration.Y = 0;
      }
    }

    void updateHeading() {
    }

    void onThrowCallback(int frameIndex) {
      int teamDirection = onLeft ? 1 : -1;
      if(frameIndex > 0) {
        x += throwOffsets[(int)heading][frameIndex - 1].X * teamDirection;
        y += throwOffsets[(int)heading][frameIndex - 1].Y * teamDirection;
      }
    }

    void onThrowCompleteCallback(int frameIndex) {
      float seconds = MIN_THROW_DELAY + 
        MathHelper.Lerp(MIN_THROW_DELAY, MAX_THROW_DELAY, flungAtCharge/maxCharge);
      G.state.DoInSeconds(seconds, () => {
        throwing = false;
      });
    }

    public void PickUpBall(Ball ball) {
      if(!ball.dangerous && this.ball == null && !ball.owned) {
        ball.owned = true;
        this.ball = ball;
        this.ball.pickedUp();
      }
    }
  }

  public enum Team {
    Left = 0x01,
    Right = 0x02
  }

  public enum Heading {
    Up = 0,
    UpMid = 1,
    Forward = 2,
    DownMid = 3,
    Down = 4
  }

  public enum SpriteMode {
    Neutral = 0,
    Hold = 1,
    Charge = 2,
    Misc = 3
  }
}
