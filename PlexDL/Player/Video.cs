﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PlexDL.Player
{
    /// <summary>
    /// A class that is used to group together the Video methods and properties of the PVS.MediaPlayer.Player class.
    /// </summary>
    [CLSCompliant(true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Video : HideObjectMembers
    {
        #region Fields (Video Class)

        private Player _base;
        private bool _zoomBusy;
        private bool _boundsBusy;

        #endregion Fields (Video Class)

        internal Video(Player player)
        {
            _base = player;
        }

        /// <summary>
        /// Gets a value indicating whether the playing media contains video.
        /// </summary>
        public bool Present
        {
            get
            {
                _base._lastError = Player.NO_ERROR;
                return _base._hasVideo;
            }
        }

        ///// <summary>
        ///// Gets or sets a value indicating whether the display of video of the player is enabled (default: true).
        ///// </summary>
        //public bool Enabled
        //{
        //    get
        //    {
        //        _base._lastError = Player.NO_ERROR;
        //        return _base._videoEnabled;
        //    }
        //    set { _base.AV_SetVideoEnabled(value); }
        //}

        /// <summary>
        /// Gets or sets the active video track of the playing media. See also: Player.Media.GetVideoTracks.
        /// </summary>
        public int Track
        {
            get
            {
                _base._lastError = Player.NO_ERROR;
                return _base._videoTrackCurrent;
            }
            set { _base.AV_SetTrack(value, false); }
        }

        /// <summary>
        /// Gets or sets the size and location of the video image on the display of the player. When set, the display mode of the player (Player.Display.Mode) is set to Displaymode.Manual.
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                _base._lastError = Player.NO_ERROR;
                if (!_base._hasVideoBounds && _base._hasVideo)
                {
                    _base.AV_GetDisplayModeSize(_base._displayMode);
                }

                return _base._videoBounds;
            }
            set
            {
                if (_base._hasVideo)
                {
                    if (!_boundsBusy && (value.Width >= 8) && (value.Height >= 8))
                    {
                        _boundsBusy = true;
                        _base._lastError = Player.NO_ERROR;

                        _base._videoBounds = value;
                        _base._videoBoundsClip = Rectangle.Intersect(_base._display.DisplayRectangle, _base._videoBounds);
                        _base._hasVideoBounds = true;

                        if (_base._displayMode == DisplayMode.Manual) _base._display.Refresh();
                        else _base.Display.Mode = DisplayMode.Manual;

                        // TODO - image gets stuck when same size as display - is it _videoDisplay or MF
                        if (_base._videoBounds.X <= 0 || _base._videoBounds.Y <= 0)
                        {
                            _base._videoDisplay.Width--;
                            _base._videoDisplay.Width++;
                        }

                        if (_base._hasDisplayShape) _base.AV_UpdateDisplayShape();

                        if (_base._mediaVideoBoundsChanged != null) _base._mediaVideoBoundsChanged(_base, EventArgs.Empty);

                        _boundsBusy = false;
                    }
                    else _base._lastError = HResult.MF_E_OUT_OF_RANGE;
                }
                else _base._lastError = HResult.S_FALSE;
            }
        }

        /// <summary>
        /// Enlarges or reduces the size of the video image at the center location of the display of the player. The display mode of the player (Player.Display.Mode) is set to Displaymode.Manual.
        /// </summary>
        /// <param name="factor">The factor by which the video image is to be zoomed.</param>
        public int Zoom(double factor)
        {
            if (_base._hasVideo) return Zoom(factor, _base._display.Width / 2, _base._display.Height / 2);
            _base._lastError = HResult.S_FALSE;
            return (int)_base._lastError;
        }

        /// <summary>
        /// Enlarges or reduces the size of the player's video image at the specified display location. The display mode of the player (Player.Display.Mode) is set to Displaymode.Manual.
        /// </summary>
        /// <param name="factor">The factor by which the video image is to be zoomed.</param>
        /// <param name="center">The center location of the zoom on the display of the player.</param>
        public int Zoom(double factor, Point center)
        {
            return (Zoom(factor, center.X, center.Y));
        }

        /// <summary>
        /// Enlarges or reduces the size of the player's video image at the specified display location. The display mode of the player (Player.Display.Mode) is set to Displaymode.Manual.
        /// </summary>
        /// <param name="factor">The factor by which the video image is to be zoomed.</param>
        /// <param name="xCenter">The horizontal (x) center location of the zoom on the display of the player.</param>
        /// <param name="yCenter">The vertical (y) center location of the zoom on the display of the player.</param>
        public int Zoom(double factor, int xCenter, int yCenter)
        {
            if (_base._hasVideo && factor > 0)
            {
                _base._lastError = Player.NO_ERROR;

                if (factor != 1)
                {
                    if (_zoomBusy) return (int)_base._lastError;

                    _zoomBusy = true;
                    double width = 0;
                    double height = 0;
                    Rectangle r = new Rectangle(_base._videoBounds.Location, _base._videoBounds.Size);

                    if (r.Width < r.Height)
                    {
                        r.X = (int)Math.Round(-factor * (xCenter - r.X)) + xCenter;
                        width = r.Width * factor;

                        if (width >= 10)
                        {
                            r.Y = (int)Math.Round(-(width / r.Width) * (yCenter - r.Y)) + yCenter;
                            height = (width / r.Width) * r.Height;
                        }
                    }
                    else
                    {
                        r.Y = (int)Math.Round(-factor * (yCenter - r.Y)) + yCenter;
                        height = r.Height * factor;

                        if (height >= 10)
                        {
                            r.X = (int)Math.Round(-(height / r.Height) * (xCenter - r.X)) + xCenter;
                            width = (height / r.Height) * r.Width;
                        }
                    }

                    r.Width = (int)Math.Round(width);
                    r.Height = (int)Math.Round(height);
                    Bounds = r;

                    _zoomBusy = false;
                }
            }
            else _base._lastError = HResult.S_FALSE;

            return (int)_base._lastError;
        }

        /// <summary>
        /// Enlarges the specified part of the display of the player to the entire display of the player. The display mode of the player (Player.Display.Mode) is set to Displaymode.Manual.
        /// </summary>
        /// <param name="area">The area of the display of the player to enlarge.</param>
        public int Zoom(Rectangle area)
        {
            if (_base._hasVideo)
            {
                if ((area.X >= 0 && area.X <= (_base._display.Width - 8)) && (area.Y >= 0 && area.Y <= (_base._display.Height - 8)) &&
                    (area.X + area.Width <= _base._display.Width) && (area.Y + area.Height <= _base._display.Height))
                {
                    double factorX = (double)_base._display.Width / area.Width;
                    double factorY = (double)_base._display.Height / area.Height;

                    Bounds = new Rectangle(
                        (int)((_base._videoBounds.X - area.X) * factorX),
                        (int)((_base._videoBounds.Y - area.Y) * factorY),
                        (int)(_base._videoBounds.Width * factorX),
                        (int)(_base._videoBounds.Height * factorY));
                }
                else _base._lastError = HResult.MF_E_OUT_OF_RANGE;
            }
            else _base._lastError = HResult.S_FALSE;

            return (int)_base._lastError;
        }

        /// <summary>
        /// Moves the location of the video image on the display of the player by the given amount of pixels. The display mode of the player (Player.Display.Mode) is set to Displaymode.Manual.
        /// </summary>
        /// <param name="horizontal">The amount of pixels to move the video image in the horizontal (x) direction.</param>
        /// <param name="vertical">The amount of pixels to move the video image in the vertical (y) direction.</param>
        public int Move(int horizontal, int vertical)
        {
            if (_base._hasVideo)
            {
                Bounds = new Rectangle(_base._videoBounds.X + horizontal, _base._videoBounds.Y + vertical, _base._videoBounds.Width,
                    _base._videoBounds.Height);
            }
            else _base._lastError = HResult.S_FALSE;

            return (int)_base._lastError;
        }

        /// <summary>
        /// Enlarges or reduces the size of the player's video image by the given amount of pixels at the center of the video image. The display mode of the player (Player.Display.Mode) is set to Displaymode.Manual.
        /// </summary>
        /// <param name="horizontal">The amount of pixels to stretch the video image in the horizontal (x) direction.</param>
        /// <param name="vertical">The amount of pixels to stretch the video image in the vertical (y) direction.</param>
        public int Stretch(int horizontal, int vertical)
        {
            if (_base._hasVideo)
            {
                Bounds = new Rectangle(_base._videoBounds.X - (horizontal / 2), _base._videoBounds.Y - (vertical / 2), _base._videoBounds.Width + horizontal,
                    _base._videoBounds.Height + vertical);
            }
            else _base._lastError = HResult.S_FALSE;

            return (int)_base._lastError;
        }

        /// <summary>
        /// Gets or sets a value indicating the brightness of the player's video image. Values from -1.0 to 1.0 (default: 0.0).
        /// </summary>
        public double Brightness
        {
            get
            {
                _base._lastError = Player.NO_ERROR;
                return _base._brightness;
            }
            set { _base.AV_SetBrightness(value, true); }
        }

        /// <summary>
        /// Gets or sets a value indicating the contrast of the player's video image. Values from -1.0 to 1.0 (default: 0.0).
        /// </summary>
        public double Contrast
        {
            get
            {
                _base._lastError = Player.NO_ERROR;
                return _base._contrast;
            }
            set { _base.AV_SetContrast(value, true); }
        }

        /// <summary>
        /// Gets or sets a value indicating the hue of the player's video image. Values from -1.0 to 1.0 (default: 0.0).
        /// </summary>
        public double Hue
        {
            get
            {
                _base._lastError = Player.NO_ERROR;
                return _base._hue;
            }
            set { _base.AV_SetHue(value, true); }
        }

        /// <summary>
        /// Gets or sets a value indicating the saturation of the player's video image. Values from -1.0 to 1.0 (default: 0.0).
        /// </summary>
        public double Saturation
        {
            get
            {
                _base._lastError = Player.NO_ERROR;
                return _base._saturation;
            }
            set { _base.AV_SetSaturation(value, true); }
        }

        /// <summary>
        /// Returns a copy of the currently displayed video image of the player (without display overlay). See also: Player.ScreenCopy.
        /// </summary>
        public Image ToImage()
        {
            return _base.AV_DisplayCopy(true, false);
        }

        /// <summary>
        /// Copies the currently displayed video image of the player (without display overlay) to the system's clipboard. See also: Player.ScreenCopy.
        /// </summary>
        public int ToClipboard()
        {
            Image theImage = _base.AV_DisplayCopy(true, false);
            if (_base._lastError == Player.NO_ERROR)
            {
                try { Clipboard.SetImage(theImage); }
                catch (Exception e)
                {
                    _base._lastError = (HResult)Marshal.GetHRForException(e);
                }

                theImage.Dispose();
            }

            return (int)_base._lastError;
        }

        /// <summary>
        /// Saves a copy of the currently displayed video image of the player (without display overlay) to the specified file. See also: Player.ScreenCopy.
        /// </summary>
        /// <param name="fileName">The name of the file to save.</param>
        /// <param name="imageFormat">The file format of the image to save.</param>
        public int ToFile(string fileName, System.Drawing.Imaging.ImageFormat imageFormat)
        {
            if ((fileName != null) && (fileName.Length > 3))
            {
                Image theImage = _base.AV_DisplayCopy(true, false);
                if (_base._lastError == Player.NO_ERROR)
                {
                    try { theImage.Save(fileName, imageFormat); }
                    catch (Exception e)
                    {
                        _base._lastError = (HResult)Marshal.GetHRForException(e);
                    }

                    theImage.Dispose();
                }
            }
            else
            {
                _base._lastError = HResult.ERROR_INVALID_NAME;
            }

            return (int)_base._lastError;
        }
    }
}