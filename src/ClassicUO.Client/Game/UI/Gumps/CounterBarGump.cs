// SPDX-License-Identifier: BSD-2-Clause

using System.Collections.Generic;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Assets;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Game.UI.Gumps
{
    public class CounterBarGump : Gump
    {
        private AlphaBlendControl _background;

        public static CounterBarGump CurrentCounterBarGump { get; private set; }

        private int _rows,
            _columns,
            _rectSize;

        //private bool _isVertical;

        public CounterBarGump(World world) : base(world, 0, 0)
        {
            CurrentCounterBarGump = this;
        }

        public CounterBarGump(
            World world,
            int x,
            int y,
            int rectSize = 30,
            int rows = 1,
            int columns = 1 /*, bool vertical = false*/
        ) : base(world, 0, 0)
        {
            X = x;
            Y = y;

            if (rectSize < 30)
            {
                rectSize = 30;
            }
            else if (rectSize > 80)
            {
                rectSize = 80;
            }

            if (rows < 1)
            {
                rows = 1;
            }

            if (columns < 1)
            {
                columns = 1;
            }

            _rows = rows;
            _columns = columns;
            _rectSize = rectSize;
            //_isVertical = vertical;

            BuildGump();

            CurrentCounterBarGump = this;
            IsLocked = ProfileManager.CurrentProfile.CounterGumpLocked;
            CanCloseWithRightClick = false;
        }

        public override GumpType GumpType => GumpType.CounterBar;

        private void BuildGump()
        {
            CanMove = true;
            AcceptMouseInput = true;
            AcceptKeyboardInput = false;
            WantUpdateSize = false;
            Width = _rectSize * _columns + 1;
            Height = _rectSize * _rows + 1;

            Add(_background = new AlphaBlendControl(0.7f) { Width = Width, Height = Height });

            for (int row = 0; row < _rows; row++)
            {
                for (int col = 0; col < _columns; col++)
                {
                    Add(
                        new CounterItem(
                            this,
                            col * _rectSize + 2,
                            row * _rectSize + 2,
                            _rectSize - 4,
                            _rectSize - 4
                        )
                    );
                }
            }
        }

        public void SetLayout(int size, int rows, int columns)
        {
            bool ok = false;

            //if (_isVertical != isvertical)
            //{
            //    _isVertical = isvertical;
            //    int temp = _rows;
            //    _rows = _columns;
            //    _columns = temp;
            //    ok = true;
            //}

            if (rows > 30)
            {
                rows = 30;
            }

            if (columns > 30)
            {
                columns = 30;
            }

            if (size < 30)
            {
                size = 30;
            }
            else if (size > 80)
            {
                size = 80;
            }

            if (_rectSize != size)
            {
                ok = true;
                _rectSize = size;
            }

            if (rows < 1)
            {
                rows = 1;
            }

            if (_rows != rows)
            {
                ok = true;
                _rows = rows;
            }

            if (columns < 1)
            {
                columns = 1;
            }

            if (_columns != columns)
            {
                ok = true;
                _columns = columns;
            }

            if (ok)
            {
                ApplyLayout();
            }
        }

        private void ApplyLayout()
        {
            Width = _rectSize * _columns + 1;
            Height = _rectSize * _rows + 1;

            _background.Width = Width;
            _background.Height = Height;

            CounterItem[] items = GetControls<CounterItem>();

            int[] indices = new int[items.Length];

            for (int row = 0; row < _rows; row++)
            {
                for (int col = 0; col < _columns; col++)
                {
                    int index = /*_isVertical ? col * _rows + row :*/
                        row * _columns + col;

                    if (index < items.Length)
                    {
                        CounterItem c = items[index];

                        c.X = col * _rectSize + 2;
                        c.Y = row * _rectSize + 2;
                        c.Width = _rectSize - 4;
                        c.Height = _rectSize - 4;

                        c.SetGraphic(c.Graphic, c.Hue);

                        indices[index] = -1;
                    }
                    else
                    {
                        Add(
                            new CounterItem(
                                this,
                                col * _rectSize + 2,
                                row * _rectSize + 2,
                                _rectSize - 4,
                                _rectSize - 4
                            )
                        );
                    }
                }
            }

            for (int i = 0; i < indices.Length; i++)
            {
                int index = indices[i];

                if (index >= 0 && index < items.Length)
                {
                    items[i].Parent = null;

                    items[i].Dispose();
                }
            }

            SetInScreen();
        }

        public CounterItem GetCounterItem(int index)
        {
            CounterItem[] items = GetControls<CounterItem>();

            if (items == null)
            {
                return null;
            }

            if (items.Length > index)
            {
                return items[index];
            }

            return null;
        }

        protected override void OnMouseUp(int x, int y, MouseButtonType button)
        {
            base.OnMouseUp(x, y, button);

            if (button == MouseButtonType.Left)
            {
                if (Keyboard.Alt && Keyboard.Ctrl)
                {
                    IsLocked = ProfileManager.CurrentProfile.CounterGumpLocked = !IsLocked;
                }
            }
        }

        public override void Save(XmlTextWriter writer)
        {
            base.Save(writer);

            writer.WriteAttributeString("rows", _rows.ToString());
            writer.WriteAttributeString("columns", _columns.ToString());
            writer.WriteAttributeString("rectsize", _rectSize.ToString());

            IEnumerable<CounterItem> controls = FindControls<CounterItem>();

            writer.WriteStartElement("controls");

            foreach (CounterItem control in controls)
            {
                writer.WriteStartElement("control");
                writer.WriteAttributeString("graphic", control.Graphic.ToString());
                writer.WriteAttributeString("hue", control.Hue.ToString());
                if (control.SpellID != default)
                    writer.WriteAttributeString("spellid", control.SpellID.ToString());
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        public override void Restore(XmlElement xml)
        {
            base.Restore(xml);

            _rows = int.Parse(xml.GetAttribute("rows"));
            _columns = int.Parse(xml.GetAttribute("columns"));
            _rectSize = int.Parse(xml.GetAttribute("rectsize"));

            BuildGump();

            XmlElement controlsXml = xml["controls"];

            if (controlsXml != null)
            {
                CounterItem[] items = GetControls<CounterItem>();
                int index = 0;

                foreach (XmlElement controlXml in controlsXml.GetElementsByTagName("control"))
                {
                    if (index < items.Length)
                    {
                        bool isGump = false;
                        if (controlXml.HasAttribute("spellid"))
                        {
                            items[index].SpellID = int.Parse(controlXml.GetAttribute("spellid"));
                            isGump = true;
                        }

                        items[index]?.SetGraphic(
                            ushort.Parse(controlXml.GetAttribute("graphic")),
                            ushort.Parse(controlXml.GetAttribute("hue")),
                            isGump
                        );
                        index++;
                    }
                    else
                    {
                        Log.Error(ResGumps.IndexOutOfbounds);
                    }
                }
            }

            IsEnabled = IsVisible = ProfileManager.CurrentProfile.CounterBarEnabled;
            IsLocked = ProfileManager.CurrentProfile.CounterGumpLocked;
        }

        protected override void OnLockedChanged()
        {
            base.OnLockedChanged();
            CanCloseWithRightClick = false;
        }

        public override void Dispose()
        {
            if (CurrentCounterBarGump == this)
            {
                CurrentCounterBarGump = null;
            }
            base.Dispose();
        }

        public class CounterItem : Control
        {
            private int _amount;
            private readonly ImageWithText _image;
            private uint _time;
            private const uint HIGHLIGHT_DURATION = 1000;
            private uint _endHighlight;
            private bool _highlight;
            private readonly CounterBarGump _gump;

            public CounterItem(CounterBarGump gump, int x, int y, int w, int h)
            {
                _gump = gump;
                AcceptMouseInput = true;
                WantUpdateSize = false;
                CanMove = true;
                CanCloseWithRightClick = false;

                X = x;
                Y = y;
                Width = w;
                Height = h;

                _image = new ImageWithText();
                Add(_image);

                ContextMenu = new ContextMenuControl(_gump);
                ContextMenu.Add(ResGumps.UseObject, Use);
                ContextMenu.Add(ResGumps.Remove, RemoveItem);
                ContextMenu.Add("Set spell", GenSpellList());
            }

            public ushort Graphic { get; private set; }

            public ushort Hue { get; private set; }

            public int SpellID { get; set; }

            public void SetGraphic(ushort graphic, ushort hue, bool isGumpIcon = false)
            {
                _image.ChangeGraphic(graphic, hue, isGumpIcon);

                if (graphic == 0)
                {
                    return;
                }

                Graphic = graphic;
                Hue = hue;
            }

            public void RemoveItem()
            {
                _image?.ChangeGraphic(0, 0);
                _amount = 0;
                Graphic = 0;
                SpellID = default;
            }

            public void Use()
            {
                if (Graphic == 0)
                {
                    return;
                }

                if (SpellID != default)
                {
                    GameActions.CastSpell(SpellID);
                    return;
                }

                if (SpellID != default)
                {
                    GameActions.CastSpell(SpellID);
                    return;
                }

                Item backpack = _gump.World.Player.Backpack;

                if (backpack == null)
                {
                    return;
                }

                Item item = backpack.FindItem(Graphic, Hue);

                if (item != null)
                {
                    GameActions.DoubleClick(_gump.World, item);
                }
            }

            // protected override void OnMouseOver(int x, int y)
            // {
            //     base.OnMouseOver(x, y);

            //     if (_gump.World.Player.Backpack?.FindItem(Graphic, Hue) is {} item)
            //         SetTooltip(item);
            // }

            // protected override void OnMouseExit(int x, int y)
            // {
            //     base.OnMouseExit(x, y);
            //     ClearTooltip();
            // }

            public List<ContextMenuItemEntry> GenSpellList()
            {
                List<ContextMenuItemEntry> list = new List<ContextMenuItemEntry>();

                ContextMenuItemEntry entry = new ContextMenuItemEntry("Magery");
                foreach (var spell in SpellsMagery.GetAllSpells.Values)
                    entry.Add(new ContextMenuItemEntry(spell.Name, () =>
                    {
                        SetGraphic((ushort)(spell.GumpIconSmallID), 0, true);
                        SpellID = spell.ID;
                    }));
                list.Add(entry);


                entry = new ContextMenuItemEntry("Necromancy");
                foreach (var spell in SpellsNecromancy.GetAllSpells.Values)
                    entry.Add(new ContextMenuItemEntry(spell.Name, () =>
                    {
                        SetGraphic((ushort)(spell.GumpIconSmallID), 0, true);
                        SpellID = spell.ID;
                    }));
                list.Add(entry);


                entry = new ContextMenuItemEntry("Chivalry");
                foreach (var spell in SpellsChivalry.GetAllSpells.Values)
                    entry.Add(new ContextMenuItemEntry(spell.Name, () =>
                    {
                        SetGraphic((ushort)(spell.GumpIconSmallID), 0, true);
                        SpellID = spell.ID;
                    }));
                list.Add(entry);


                entry = new ContextMenuItemEntry("Bushido");
                foreach (var spell in SpellsBushido.GetAllSpells.Values)
                    entry.Add(new ContextMenuItemEntry(spell.Name, () =>
                    {
                        SetGraphic((ushort)(spell.GumpIconSmallID), 0, true);
                        SpellID = spell.ID;
                    }));
                list.Add(entry);


                entry = new ContextMenuItemEntry("Ninjitsu");
                foreach (var spell in SpellsNinjitsu.GetAllSpells.Values)
                    entry.Add(new ContextMenuItemEntry(spell.Name, () =>
                    {
                        SetGraphic((ushort)(spell.GumpIconSmallID), 0, true);
                        SpellID = spell.ID;
                    }));
                list.Add(entry);


                entry = new ContextMenuItemEntry("Spellweaving");
                foreach (var spell in SpellsSpellweaving.GetAllSpells.Values)
                    entry.Add(new ContextMenuItemEntry(spell.Name, () =>
                    {
                        SetGraphic((ushort)(spell.GumpIconSmallID), 0, true);
                        SpellID = spell.ID;
                    }));
                list.Add(entry);


                entry = new ContextMenuItemEntry("Mysticism");
                foreach (var spell in SpellsMysticism.GetAllSpells.Values)
                    entry.Add(new ContextMenuItemEntry(spell.Name, () =>
                    {
                        SetGraphic((ushort)(spell.GumpIconSmallID), 0, true);
                        SpellID = spell.ID;
                    }));
                list.Add(entry);


                entry = new ContextMenuItemEntry("Mastery");
                foreach (var spell in SpellsMastery.GetAllSpells.Values)
                    entry.Add(new ContextMenuItemEntry(spell.Name, () =>
                    {
                        SetGraphic((ushort)(spell.GumpIconSmallID), 0, true);
                        SpellID = spell.ID;
                    }));
                list.Add(entry);
                return list;
            }
            protected override void OnMouseUp(int x, int y, MouseButtonType button)
            {
                if (button == MouseButtonType.Left)
                {
                    if (Keyboard.Alt && Keyboard.Ctrl)
                        if(Parent is Gump pg)
                        {
                            Log.Trace(pg.GetType().ToString());
                            pg.IsLocked = ProfileManager.CurrentProfile.CounterGumpLocked = !pg.IsLocked;
                        }
                    if (Client.Game.UO.GameCursor.ItemHold.Enabled)
                    {
                        SetGraphic(
                            Client.Game.UO.GameCursor.ItemHold.Graphic,
                            Client.Game.UO.GameCursor.ItemHold.Hue
                        );

                        GameActions.DropItem(
                            Client.Game.UO.GameCursor.ItemHold.Serial,
                            Client.Game.UO.GameCursor.ItemHold.X,
                            Client.Game.UO.GameCursor.ItemHold.Y,
                            0,
                            Client.Game.UO.GameCursor.ItemHold.Container
                        );
                    }
                    else if (ProfileManager.CurrentProfile.CastSpellsByOneClick)
                    {
                        Use();
                        return;
                    }
                }
                else if (button == MouseButtonType.Right && Keyboard.Alt && Graphic != 0)
                {
                    RemoveItem();

                    return;
                }

                base.OnMouseUp(x, y, button);
            }

            protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
            {
                if (
                    button == MouseButtonType.Left
                    && !ProfileManager.CurrentProfile.CastSpellsByOneClick
                )
                {
                    Use();
                }

                return true;
            }

            public override void Update()
            {
                base.Update();

                if (Parent != null && Parent.IsEnabled && _time < Time.Ticks)
                {
                    _time = Time.Ticks + 100;
                    if (SpellID != default)
                    {
                        if (Tooltip == null)
                            SetTooltip(SpellDefinition.FullIndexGetSpell(SpellID).Name);
                        return;
                    }

                    if (Graphic == 0)
                    {
                        _image.SetAmount(string.Empty);
                    }
                    else
                    {
                        _amount = 0;

                        for (
                            Item item = (Item)_gump.World.Player.Items;
                            item != null;
                            item = (Item)item.Next
                        )
                        {
                            if (
                                item.ItemData.IsContainer
                                && !item.IsEmpty
                                && item.Layer >= Layer.OneHanded
                                && item.Layer <= Layer.Legs
                            )
                            {
                                GetAmount(item, Graphic, Hue, ref _amount);
                            }
                        }

                        if (ProfileManager.CurrentProfile.CounterBarDisplayAbbreviatedAmount)
                        {
                            if (
                                _amount >= ProfileManager.CurrentProfile.CounterBarAbbreviatedAmount
                            )
                            {
                                _image.SetAmount(StringHelper.IntToAbbreviatedString(_amount));

                                return;
                            }
                        }

                        if (ProfileManager.CurrentProfile.CounterBarHighlightOnUse)
                        {
                            if (int.TryParse(_image.GetText(), out int cAmt))
                            {
                                if (cAmt > _amount)
                                {
                                    _highlight = true;
                                    _endHighlight = Time.Ticks + HIGHLIGHT_DURATION;
                                }
                            }
                        }
                        _image.SetAmount(_amount.ToString());
                    }
                }
            }

            private void GetAmount(Item parent, ushort graphic, ushort hue, ref int amount)
            {
                if (parent == null)
                {
                    return;
                }

                for (LinkedObject i = parent.Items; i != null; i = i.Next)
                {
                    Item item = (Item)i;

                    GetAmount(item, graphic, hue, ref amount);

                    if (item.Graphic == graphic && item.Hue == hue && item.Exists)
                    {
                        amount += item.Amount;
                        SetTooltip(item);
                    }
                }
            }

            public override bool Draw(UltimaBatcher2D batcher, int x, int y)
            {
                base.Draw(batcher, x, y);

                Texture2D color = SolidColorTextureCache.GetTexture(
                    MouseIsOver
                        ? Color.Yellow
                        : ProfileManager.CurrentProfile.CounterBarHighlightOnAmount
                        && _amount < ProfileManager.CurrentProfile.CounterBarHighlightAmount
                        && Graphic != 0
                            ? Color.Red
                            : Color.Gray
                );

                Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);

                if (_highlight && Time.Ticks > _endHighlight)
                {
                    _highlight = false;
                }

                if (_highlight)
                {
                    hueVector.Z = ((float)_endHighlight - (float)Time.Ticks) / (float)HIGHLIGHT_DURATION;
                    batcher.Draw(SolidColorTextureCache.GetTexture(Color.Yellow), new Rectangle(x, y, Width, Height), hueVector);
                }

                hueVector.Z = 1;

                batcher.DrawRectangle(color, x, y, Width, Height, hueVector);

                return true;
            }

            private class ImageWithText : Control
            {
                private readonly Label _label;
                private ushort _graphic;
                private ushort _hue;
                private bool _partial;
                private bool _isGumpGraphic;

                public ImageWithText()
                {
                    CanMove = true;
                    WantUpdateSize = true;
                    AcceptMouseInput = false;

                    _label = new Label("", true, 0x35, 0, 1, FontStyle.BlackBorder)
                    {
                        X = 2,
                        Y = Height - 15,
                        AcceptMouseInput = false
                    };

                    Add(_label);
                }

                public void ChangeGraphic(ushort graphic, ushort hue, bool isGumpGraphic = false)
                {
                    _isGumpGraphic = isGumpGraphic;
                    if (graphic != 0)
                    {
                        _graphic = graphic;
                        _hue = hue;
                        _partial = isGumpGraphic ? false : Client.Game.UO.FileManager.TileData.StaticData[graphic].IsPartialHue;
                        _label.Y = Parent.Height - 15;
                    }
                    else
                    {
                        _graphic = 0;
                    }
                    if (_isGumpGraphic)
                        _label.Text = string.Empty;
                }

                public override void Update()
                {
                    base.Update();

                    if (Parent != null)
                    {
                        Width = Parent.Width;
                        Height = Parent.Height;
                    }
                }

                public override bool Draw(UltimaBatcher2D batcher, int x, int y)
                {
                    if (_graphic != 0)
                    {
                        ref readonly var artInfo = ref Client.Game.UO.Arts.GetArt(_graphic);
                        if (_isGumpGraphic)
                            artInfo = ref Client.Game.UO.Gumps.GetGump(_graphic);

                        var rect = _isGumpGraphic ? artInfo.UV : Client.Game.UO.Arts.GetRealArtBounds(_graphic);

                        Vector3 hueVector = ShaderHueTranslator.GetHueVector(_hue, _partial, 1f, _isGumpGraphic);

                        Point originalSize = new Point(Width, Height);
                        Point point = new Point();

                        if (rect.Width < Width)
                        {
                            originalSize.X = rect.Width;
                            point.X = (Width >> 1) - (originalSize.X >> 1);
                        }

                        if (rect.Height < Height)
                        {
                            originalSize.Y = rect.Height;
                            point.Y = (Height >> 1) - (originalSize.Y >> 1);
                        }

                        if (_isGumpGraphic)
                            batcher.Draw(
                                artInfo.Texture,
                                new Rectangle(x + point.X, y + point.Y, originalSize.X, originalSize.Y),
                                new Rectangle(
                                    artInfo.UV.X,
                                    artInfo.UV.Y,
                                    rect.Width,
                                    rect.Height
                                ),
                                hueVector
                            );
                        else
                            batcher.Draw(
                                artInfo.Texture,
                                new Rectangle(x + point.X, y + point.Y, originalSize.X, originalSize.Y),
                                new Rectangle(
                                    artInfo.UV.X + rect.X,
                                    artInfo.UV.Y + rect.Y,
                                    rect.Width,
                                    rect.Height
                                ),
                                hueVector
                            );
                    }

                    return base.Draw(batcher, x, y);
                }

                public void SetAmount(string amount)
                {
                    _label.Text = amount;
                }

                public string GetText()
                {
                    return _label?.Text ?? "";
                }
            }
        }
    }
}
