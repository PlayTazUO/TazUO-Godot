﻿// SPDX-License-Identifier: BSD-2-Clause

using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Renderer;
using ClassicUO.Utility.Collections;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.GameObjects
{
    public class TextContainer : LinkedObject
    {
        public int Size,
            MaxSize = 5;

        public void Add(TextObject obj)
        {
            PushToBack(obj);

            if (Size >= MaxSize)
            {
                ((TextObject)Items)?.Destroy();
                Remove(Items);
            }
            else
            {
                Size++;
            }
        }

        public new void Clear()
        {
            TextObject item = (TextObject)Items;
            Items = null;

            while (item != null)
            {
                TextObject next = (TextObject)item.Next;
                item.Next = null;
                item.Destroy();
                Remove(item);

                item = next;
            }

            Size = 0;
        }
    }



    public class OverheadDamage
    {
        private const int DAMAGE_Y_MOVING_TIME = 25;
        private readonly Deque<TextObject> _messages;
        private Rectangle _rectangle;
        private readonly World _world;

        public OverheadDamage(World world, GameObject parent)
        {
            _world = world;
            Parent = parent;
            _messages = new Deque<TextObject>();
        }

        public GameObject Parent { get; private set; }
        public bool IsDestroyed { get; private set; }
        public bool IsEmpty => _messages.Count == 0;


        public void SetParent(GameObject parent)
        {
            Parent = parent;
        }


        public void Add(int damage)
        {
            Parent.AddDamage(damage);

            TextObject text_obj = TextObject.Create(_world);

            ushort hue = ProfileManager.CurrentProfile == null ? (ushort)0x0021 : ProfileManager.CurrentProfile.DamageHueOther;
            string name = string.Empty;
            if (ReferenceEquals(Parent, _world.Player))
                hue = ProfileManager.CurrentProfile == null ? (ushort)0x0034 : ProfileManager.CurrentProfile.DamageHueSelf;
            else if (Parent is Mobile)
            {
                Mobile _parent = (Mobile)Parent;
                name = _parent.Name;
                if (_parent.IsRenamable && _parent.NotorietyFlag != NotorietyFlag.Invulnerable && _parent.NotorietyFlag != NotorietyFlag.Enemy)
                    hue = ProfileManager.CurrentProfile == null ? (ushort)0x0033 : ProfileManager.CurrentProfile.DamageHuePet;
                else if (_parent.NotorietyFlag == NotorietyFlag.Ally)
                    hue = ProfileManager.CurrentProfile == null ? (ushort)0x0030 : ProfileManager.CurrentProfile.DamageHueAlly;

                if (_parent.Serial == _world.TargetManager.LastAttack)
                    hue = ProfileManager.CurrentProfile == null ? (ushort)0x1F : ProfileManager.CurrentProfile.DamageHueLastAttck;
            }
            string dps = ProfileManager.CurrentProfile.ShowDPS ? $" (DPS: {Parent.GetCurrentDPS()})" : string.Empty;

            
            text_obj.TextBox = TextBox.GetOne(damage.ToString() + dps, ProfileManager.CurrentProfile.OverheadChatFont, ProfileManager.CurrentProfile.OverheadChatFontSize, hue, TextBox.RTLOptions.DefaultCenterStroked(ProfileManager.CurrentProfile.OverheadChatWidth).MouseInput(!ProfileManager.CurrentProfile.DisableMouseInteractionOverheadText));

            _world.Journal.Add(damage.ToString() + dps, hue, name, TextType.CLIENT, messageType: MessageType.Damage);

            text_obj.Time = Time.Ticks + 1500;

            _messages.AddToFront(text_obj);

            if (_messages.Count > 10)
            {
                _messages.RemoveFromBack()?.Destroy();
            }
        }

        public void Update()
        {
            if (IsDestroyed)
            {
                return;
            }

            _rectangle.Width = 0;

            for (int i = 0; i < _messages.Count; i++)
            {
                TextObject c = _messages[i];

                float delta = c.Time - Time.Ticks;

                if (c.SecondTime < Time.Ticks)
                {
                    c.OffsetY += 1;
                    c.SecondTime = Time.Ticks + DAMAGE_Y_MOVING_TIME;
                }

                if (delta <= 0)
                {
                    _rectangle.Height -= c.TextBox?.Height ?? 0;
                    c.Destroy();
                    _messages.RemoveAt(i--);
                }
                //else if (delta < 250)
                //    c.Alpha = 1f - delta / 250;
                else if (c.TextBox != null)
                {
                    if (_rectangle.Width < c.TextBox.Width)
                    {
                        _rectangle.Width = c.TextBox.Width;
                    }
                }
            }
        }

        public void Draw(UltimaBatcher2D batcher)
        {
            if (IsDestroyed || _messages.Count == 0)
            {
                return;
            }

            int offY = -NameOverheadGump.CurrentHeight;

            Point p = new Point();

            if (Parent != null)
            {
                p.X += Parent.RealScreenPosition.X;
                p.Y += Parent.RealScreenPosition.Y;

                _rectangle.X = Parent.RealScreenPosition.X;
                _rectangle.Y = Parent.RealScreenPosition.Y;

                if (Parent is Mobile m)
                {
                    if (m.IsGargoyle && m.IsFlying)
                    {
                        offY += 22;
                    }
                    else if (!m.IsMounted)
                    {
                        offY = -22;
                    }

                    Client.Game.UO.Animations.GetAnimationDimensions(
                        m.AnimIndex,
                        m.GetGraphicForAnimation(),
                        /*(byte) m.GetDirectionForAnimation()*/
                        0,
                        /*Mobile.GetGroupForAnimation(m, isParent:true)*/
                        0,
                        m.IsMounted,
                        /*(byte) m.AnimIndex*/
                        0,
                        out int centerX,
                        out int centerY,
                        out int width,
                        out int height
                    );

                    p.X += (int)m.Offset.X + 22;
                    p.Y += (int)(m.Offset.Y - m.Offset.Z - (height + centerY + 8));
                }
                else
                {
                    ref readonly var artInfo = ref Client.Game.UO.Arts.GetArt(Parent.Graphic);

                    if (artInfo.Texture != null)
                    {
                        p.X += 22;
                        int yValue = artInfo.UV.Height >> 1;

                        if (Parent is Item it)
                        {
                            if (it.IsCorpse)
                            {
                                offY = -22;
                            }
                        }
                        else if (Parent is Static || Parent is Multi)
                        {
                            offY = -44;
                        }

                        p.Y -= yValue;
                    }
                }
            }

            p = Client.Game.Scene.Camera.WorldToScreen(p);

            foreach (TextObject item in _messages)
            {
                if (item.IsDestroyed || item.TextBox == null || item.TextBox.IsDisposed)
                {
                    continue;
                }

                item.X = p.X - (item.TextBox.Width >> 1);
                item.Y = p.Y - offY - item.TextBox.Height - item.OffsetY;
                item.TextBox.Draw(batcher, item.X, item.Y);
                offY += item.TextBox.Height;
            }
        }

        public void Destroy()
        {
            if (IsDestroyed)
            {
                return;
            }

            IsDestroyed = true;

            foreach (TextObject item in _messages)
            {
                item.Destroy();
            }

            _messages.Clear();
        }
    }
}
