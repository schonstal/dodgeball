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

namespace Dodgeball.Engine {
  public class Sprite : GameObject {
    const string DEFAULT_ANIMATION = "__default__";

    public BlendState blend = BlendState.AlphaBlend;
    public Color color = Color.White;
    public bool visible = true;
    public Vector2 offset = new Vector2();

    protected Vector2 sheetOffset = new Vector2();

    String currentAnimation = DEFAULT_ANIMATION;
    Dictionary<String, Animation> animations;
    Texture2D atlas;
    Vector2 screenPosition = new Vector2();
    Rectangle renderSlice = new Rectangle();

    int graphicWidth;
    int graphicHeight;

    public int GraphicHeight {
      get { return graphicHeight; }
    }

    public int GraphicWidth {
      get { return graphicWidth; }
    }

    public bool finished {
      get { return animations[currentAnimation].Finished; }
    }

    public Animation animation {
      get { return animations[currentAnimation]; }
    }

    public Sprite(float x = 0f, float y = 0f, int width = 0, int height = 0) :
        base(x, y, width, height) {
      animations = new Dictionary<string, Animation>();
      addAnimation(DEFAULT_ANIMATION, new List<int>() { 0 });
    }

    public void loadGraphic(String textureName, int width = 0, int height = 0) {
      atlas = Assets.getTexture(textureName);
      if(width > 0) this.width = width;
      if(height > 0) this.height = height;
      graphicWidth = width;
      graphicHeight = height;
    }

    public void play(String animation) {
      this.animation.start();
      currentAnimation = animation;
    }

    public void stop() {
      animation.stop();
    }

    public void addAnimation(String name, List<int> frames, int fps = 15, bool looped = false) {
      animations.Add(name, new Animation(frames, fps, looped));
    }

    //callback is passed the frame index
    public void addAnimationCallback(String animationName, Action<int> callback) {
      animations[animationName].addAnimationCallback(callback);
    }

    //callback is passed the frame index
    public void addOnCompleteCallback(String animationName, Action<int> callback) {
      animations[animationName].addOnCompleteCallback(callback);
    }

    public override void Update() {
      animation.play();
      base.Update();
    }

    public override void Draw() {
      if(visible) {
        screenPosition.X = (int)(G.camera.x + offset.X + x);
        screenPosition.Y = (int)(G.camera.y + offset.Y + y);

        renderSlice.X = ((animation.getFrame() * graphicWidth) % atlas.Width) + (int)sheetOffset.X;
        renderSlice.Y = ((int)Math.Floor(
            (double)((animation.getFrame() * graphicWidth) / atlas.Width)
          ) * graphicHeight) + (int)sheetOffset.Y;
        renderSlice.Width = graphicWidth;
        renderSlice.Height = graphicHeight;

        G.camera.Render(blend, (spriteBatch) => {
          spriteBatch.Draw(atlas, screenPosition, renderSlice, color);
        });
      }
    }
  }
}
