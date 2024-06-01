using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace MauiDemo
{
    public abstract class IAppInfo
    {
        /// <summary>
        /// unit use pixel.
        /// </summary>
        public virtual uint Width { get; }
        /// <summary>
        /// unit use pixel.
        /// </summary>
        public virtual uint Height { get; }

        GameBase game;
        public GameBase Game
        {
            get
            {
                return game;
            }
            set
            {
                game = value;
                game.VeldridAppInfo = this;
            }
        }

        public GraphicsBackend Backend;

        public GraphicsDevice GraphicsDevice;
    }
}
