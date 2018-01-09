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
    class Bomb : Shape
    {
        public Bomb(Board board, Physics physics, float dropHeight, Random rand)
        {
            _board = board;
            _physics = physics;
            _halt = 0;
            //LOAD CONTENT FUNCTION:?
            _line = ResourceMgr.Instance.Game.Content.Load<Model>("cilindro");
            _goingUp = true;
            _alpha = MIN_ALPHA;

            _type = 0;

            _pieces = new Piece[1];
            _inactivePieces = new int[1];
            int x = rand.Next(_board.DimensionX);
            int z = rand.Next(_board.DimensionZ);
            while (!_board.ValidTile(x, z))
            {
                x = rand.Next(_board.DimensionX);
                z = rand.Next(_board.DimensionZ);
            }
            _pieces[0] = new Piece(new Vector3(_board.GetCoord(x, z).X, dropHeight, _board.GetCoord(x, z).Y),
                x, _board.GetLowestFreeY(x, z), z, 9);//9 = BOMB
            return;
        }

        public override bool isBomb
        {
            get { return true; }
        }

        public override Vector3 DiscreteCenter
        {
            get { return new Vector3(_pieces[0].IndexPositionX, _pieces[0].IndexPositionY, _pieces[0].IndexPositionZ); }
        }

        public override void RotateRight()
        {
            return;
        }

        public override void RotateLeft()
        {
            return;
        }
    }
}
