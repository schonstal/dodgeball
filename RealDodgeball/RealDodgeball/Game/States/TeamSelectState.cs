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
  public class TeamSelectState : GameState {
    public const int MIDDLE_Y_SPACING = 50;
    public const int MIDDLE_Y_OFFSET = 80;

    public const int SIDE_Y_SPACING = 150;
    public const int SIDE_Y_OFFSET = 80;

    public const int LEFT_X = 100;
    public const int RIGHT_X = 500;

    public const float THRESHOLD = 0.8f;
    public const float RETURN_THRESHOLD = 0.7f;

    Sprite teamSelectScreen;
    Sprite pressStart;

    Dictionary<PlayerIndex, Sprite> playerSprites = new Dictionary<PlayerIndex, Sprite>();

    Dictionary<PlayerIndex, Dictionary<string, bool>> pushActive = new Dictionary<PlayerIndex, Dictionary<string, bool>>();
    Dictionary<PlayerIndex, Dictionary<string, bool>> lastPushActive = new Dictionary<PlayerIndex, Dictionary<string, bool>>();
    List<string> direcions = new List<string> { "left", "right" };

    public override void Create() {
      G.playMusic("resultsMusic");
      teamSelectScreen = new Sprite();
      teamSelectScreen.screenPositioning = ScreenPositioning.Absolute;
      teamSelectScreen.loadGraphic("teamSelect", 640, 360);
      add(teamSelectScreen);

      Input.ForEachInput((i) => {
        playerSprites.Add(i, new Sprite(G.camera.width/2 - 16, MIDDLE_Y_OFFSET + MIDDLE_Y_SPACING*(int)i));
        playerSprites[i].loadGraphic("Dot", 32, 32);
        playerSprites[i].screenPositioning = ScreenPositioning.Absolute;
        add(playerSprites[i]);
        pushActive.Add(i, new Dictionary<string, bool>());
        lastPushActive.Add(i, new Dictionary<string, bool>());
        direcions.ForEach((s) => {
          pushActive[i].Add(s, false);
          lastPushActive[i].Add(s, false);
        });
      });
    }

    public override void Update() {
      Input.ForEachInput((i) => {
        lastPushActive[i]["left"] = pushActive[i]["left"];
        lastPushActive[i]["right"] = pushActive[i]["right"];

        float X = G.input.ThumbSticks(i).Left.X;
        float Y = G.input.ThumbSticks(i).Left.Y;

        if(X > THRESHOLD) pushActive[i]["right"] = true;
        else if(X < RETURN_THRESHOLD) pushActive[i]["right"] = false;

        if(X < -THRESHOLD) pushActive[i]["left"] = true;
        else if(X > -RETURN_THRESHOLD) pushActive[i]["left"] = false;

        if((!lastPushActive[i]["left"] && pushActive[i]["left"]) || G.input.JustPressed(i, Buttons.DPadLeft)) {
          if(GameTracker.RightPlayers.Contains(i)) {
            Assets.getSound("select").Play(0.6f, -0.2f, 0);
            GameTracker.RightPlayers.Remove(i);
          } else if(!GameTracker.LeftPlayers.Contains(i) && GameTracker.LeftPlayers.Count < 2) {
            Assets.getSound("select").Play(0.6f, 0, 0);
            GameTracker.LeftPlayers.Add(i);
          }
        }

        if((!lastPushActive[i]["right"] && pushActive[i]["right"]) || G.input.JustPressed(i, Buttons.DPadRight)) {
          if(GameTracker.LeftPlayers.Contains(i)) {
            Assets.getSound("select").Play(0.6f, -0.2f, 0);
            GameTracker.LeftPlayers.Remove(i);
          } else if(!GameTracker.RightPlayers.Contains(i) && GameTracker.RightPlayers.Count < 2) {
            Assets.getSound("select").Play(0.6f, 0, 0);
            GameTracker.RightPlayers.Add(i);
          }
        }

        if(!GameTracker.LeftPlayers.Contains(i) && !GameTracker.RightPlayers.Contains(i)) {
          playerSprites[i].y = MIDDLE_Y_OFFSET + MIDDLE_Y_SPACING * (int)i;
          playerSprites[i].x = G.camera.width / 2 - 16;
        }
      });
      
      for(int i = 0; i < GameTracker.LeftPlayers.Count; i++) {
        playerSprites[GameTracker.LeftPlayers[i]].x = LEFT_X;
        playerSprites[GameTracker.LeftPlayers[i]].y = SIDE_Y_OFFSET + i * SIDE_Y_SPACING;
      }

      for(int i = 0; i < GameTracker.RightPlayers.Count; i++) {
        playerSprites[GameTracker.RightPlayers[i]].x = RIGHT_X;
        playerSprites[GameTracker.RightPlayers[i]].y = SIDE_Y_OFFSET + i * SIDE_Y_SPACING;
      }

      base.Update();
    }
  }
}
