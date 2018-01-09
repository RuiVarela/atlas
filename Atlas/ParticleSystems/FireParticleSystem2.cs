#region File Description
//-----------------------------------------------------------------------------
// FireParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Atlas
{
    /// <summary>
    /// Custom particle system for creating a flame effect.
    /// </summary>
    class FireParticleSystem2 : ParticleSystem
    {
        public FireParticleSystem2(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "fire";

            settings.MaxParticles = 1000;

            settings.Duration = TimeSpan.FromSeconds(1);

            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 15;

            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = 10;

            // Set gravity upside down, so the flames will 'fall' upward.
            settings.Gravity = new Vector3(0, 15, 0);


            //settings.MinColor = Color.Violet;
            //settings.MaxColor = Color.Purple;

            settings.MinStartSize = 0.1f;
            settings.MaxStartSize = 0.5f;

            settings.MinEndSize = 0.3f;
            settings.MaxEndSize = 0.9f;

            // Use additive blending.
            settings.SourceBlend = Blend.SourceAlpha;
            settings.DestinationBlend = Blend.One;
        }
    }
}
