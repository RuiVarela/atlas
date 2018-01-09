using System;
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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Atlas
{
    class ResourceMgr
    {
        private Game1 _game;

        static readonly ResourceMgr _instance = new ResourceMgr();

        public static ResourceMgr Instance
        {
            get { return _instance; }
        }

        private ResourceMgr()
        {
            _game = null;
        }

        public void Initialize(Game1 game)
        {
            _game = game;

        }

        public Game1 Game
        {
            get { return _game; }
        }
    }
}
