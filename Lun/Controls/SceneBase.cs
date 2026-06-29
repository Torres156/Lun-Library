using SFML.Graphics;

namespace Lun.Controls
{
    using SFML.Window;
    using static LunEngine;
    public abstract class SceneBase : Bond
    {
        #region Properties
        protected string alert_message = "";
        protected long alert_timer = 0;        
        internal Form form_Priority;

        #endregion

        #region Methods
        /// <summary>
        /// Construtor
        /// </summary>
        public SceneBase() : base()
        {
            Size = (Vector2)Game.WindowSize;            
        }

        public abstract void LoadContent();

        /// <summary>
        /// Descarrega os recursos
        /// </summary>
        public virtual void UnloadContent()
        {
            if (controls.Count > 0)
                foreach (var i in controls)
                    i.Destroy();
        }

        /// <summary>
        /// Desenha a tela
        /// </summary>
        /// <param name="target"></param>
        /// <param name="states"></param>
        public override void Draw(Batcher2D batcher)
        {
            batcher.Begin();
            if (controls.Count > 0)
            {
                var count = controls.Count;
                for (int i = 0; i < count; i++)
                    if (controls[i] != null && controls[i] != priority && controls[i].Visible)
                        controls[i].Draw(batcher);
            }

            if (forms.Count > 0)
            {
                var count = forms.Count;
                for (int i = 0; i < count; i++)
                    if (forms[i] != null && forms[i] != priority  && forms[i] != form_Priority && forms[i].Visible)
                        forms[i].Draw(batcher);
            }

            if (form_Priority != null)
            {
                batcher.DrawRectangle(Vector2.Zero, Game.WindowSize, new Color(0, 0, 0, 150));
                form_Priority.Draw(batcher);
            }
            priority?.Draw(batcher);

            if (isAlert)
                Draw_Alert();

            batcher.End();
        }

        /// <summary>
        /// Atualiza a tela
        /// </summary>
        public virtual void Update()
        {
            if (isAlert && TickCount > alert_timer)
            {
                alert_message = "";
                alert_timer = 0;
            }
        }

        /// <summary>
        /// Entrada de texto
        /// </summary>
        /// <param name="e"></param>
        public virtual void TextEntered(TextEventArgs e)
        {

            if (isAlert)
                return;

            if (TextBox.Focus != null && !TextBox.Focus.Blocked)
            {
                var tbox = TextBox.Focus;
                int m = char.Parse(e.Unicode);
                if (m >= 32 && m <= 255)
                {
                    if (TextBox.Focus.MaxLength > 0 && TextBox.Focus.Text.Length >= TextBox.Focus.MaxLength)
                        return;

                    if (TextBox.Focus.isNumeric)
                    {
                        if (e.Unicode.IsNumeric())
                        {
                            tbox.Text = tbox.Text.Insert(TextBox.Focus.Character_CurrentIndex, e.Unicode);
                            tbox.RequestRight();
                        }
                    }
                    else
                    {
                        tbox.Text = tbox.Text.Insert(tbox.Text.Length > 0 ? TextBox.Focus.Character_CurrentIndex : 0, e.Unicode);
                        tbox.RequestRight();
                    }

                }
                else
                {

                    // Backspace
                    if (e.Unicode == "\b")
                    {
                        if (tbox.Character_CurrentIndex > 0)
                        {
                            tbox.Text = tbox.Text.Remove(tbox.Character_CurrentIndex - 1, 1);
                            tbox.RequestLeft();
                        }
                    }

                    // Tab
                    if (e.Unicode == "\t")
                        TextBox.Focus.TabPress();
                }
            }
        }

        /// <summary>
        /// Desenha o alerta
        /// </summary>
        /// <param name="target"></param>
        protected virtual void Draw_Alert()
        {
            // Fundo
            DrawRectangle(new Vector2(), Size, new Color(0, 0, 0, 210));

            // Texto
            DrawText(alert_message, 12, new Vector2((Size.x - GetTextWidth(alert_message)) / 2, Size.y / 2 - 7), Color.White);
        }


        /// <summary>
        /// Alerta
        /// </summary>
        /// <param name="message"></param>
        public void Alert(string message)
        {
            alert_message = message;
            alert_timer = TickCount + 3000;
        }

        /// <summary>
        /// Checa se tem alerta
        /// </summary>
        protected bool isAlert
            => alert_message.Length > 0;

        /// <summary>
        /// Mouse pressionado
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool MousePressed(MouseButtonEventArgs e)
        {
            if (isAlert)
                return false;

            if (formDragged.form != null) return true;

            if (priority != null && priority.MousePressed(e)) return true;

            if (form_Priority != null)
            {
                form_Priority.MousePressed(e);
                return true;
            }

            if (forms.Count > 0)
                for (int i = forms.Count - 1; i >= 0; i--)
                    if (forms[i] != null && forms[i].Visible && forms[i].MousePressed(e)) return true;

            if (controls.Count > 0)
                for (int i = controls.Count - 1; i >= 0; i--)
                    if (controls[i] != null && controls[i] != priority && controls[i].Visible && controls[i].MousePressed(e)) return true;

            return false;
        }

        /// <summary>
        /// Movimento do mouse
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool MouseMoved(Vector2 e)
        {
            if (isAlert)
                return false;

            if (formDragged.form != null)
            {
                var newp = new Vector2(e.x, e.y) - formDragged.MousePosition;
                SetPositionFormWithAnchor(formDragged, newp);
                return true;
            }

            if (priority != null && priority.Visible && priority.MouseMoved(e)) return true;

            if (form_Priority != null)
            {
                form_Priority.MouseMoved(e);
                return true;
            }

            if (forms.Count > 0)
                for (int i = forms.Count - 1; i >= 0; i--)
                    if (forms[i] != null && forms[i].Visible && forms[i].MouseMoved(e)) return true;

            if (controls.Count > 0)
                for (int i = controls.Count - 1; i >= 0; i--)
                    if (controls[i] != null && controls[i] != priority && controls[i].Visible && controls[i].MouseMoved(e)) return true;


            return false;
        }

        /// <summary>
        /// Solta o clique
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool MouseReleased(MouseButtonEventArgs e)
        {
            if (isAlert)
            {
                alert_message = "";
                alert_timer = 0;
                return false;
            }

            TextBox.Focus = null;
            if (ScrollVertical.Current != null)
            {
                ScrollVertical.Current._press = false;
                ScrollVertical.Current = null;
            }

            if (formDragged.form != null)
            {
                formDragged.form.Form_Dragged();
                formDragged.form = null;
                return true;
            }

            if (priority is { Visible: true } && priority.MouseReleased(e)) return true;

            if (priority is { Visible: true } and ComboBox { isOpen: true } priorityCombo)
            {
                priorityCombo.isOpen = false;
                priorityCombo.Size = new Vector2(priority.Size.x, 0);
                priority = null;
            }

            if (form_Priority != null)
            {
                form_Priority.MouseReleased(e);
                return true;
            }

            if (forms.Count > 0)
                for (int i = forms.Count - 1; i >= 0; i--)
                    if (forms[i] != null && forms[i].Visible && forms[i].MouseReleased(e)) return true;

            if (controls.Count > 0)
                for (int i = controls.Count - 1; i >= 0; i--)
                    if (controls[i] != null && controls[i] != priority && controls[i].Visible && controls[i].MouseReleased(e)) return true;

            return false;
        }

        public virtual void KeyPressed(KeyEventArgs e)
        {
            var tBox = TextBox.Focus;
            if (tBox != null)
            {
                if (e.Control && e.Code == Keyboard.Key.C) return;
                if (e.Control && e.Code == Keyboard.Key.V) return;
                
                if (e.Code == Keyboard.Key.Left)
                    tBox.RequestLeft();

                if (e.Code == Keyboard.Key.Right)
                    tBox.RequestRight();

                if (e.Code == Keyboard.Key.End)
                {
                    tBox.Character_CurrentIndex = tBox.Text.Length;
                   // tBox.XoffSet = tBox.Character_CurrentIndex;
                    tBox.RequestRight(false);
                }

                if (e.Code == Keyboard.Key.Home)
                {
                    tBox.Character_CurrentIndex = 0;
                    tBox.XoffSet = 0;
                }

                if (e.Code == Keyboard.Key.Delete)
                {
                    if (tBox.Character_CurrentIndex < tBox.Text.Length)
                    {
                        tBox.Text = tBox.Text.Remove(tBox.Character_CurrentIndex, 1);
                        tBox.RequestLeft(false);
                    }
                }

            }

        }

        public virtual void KeyReleased(KeyEventArgs e)
        {
            var tBox = TextBox.Focus;
            if (tBox == null) return;
            if (e.Code == Keyboard.Key.Enter)
                tBox.EnterPress();

            if (e.Control && e.Code == Keyboard.Key.C)
                Clipboard.Contents = tBox.Text;
            
            if (e.Control && e.Code == Keyboard.Key.V && Clipboard.Contents.Length > 0)
            {
                var clip = Clipboard.Contents;
                var maxLength = clip.Length;
                if (tBox.MaxLength > 0 && tBox.Text.Length + clip.Length > tBox.MaxLength)
                    maxLength = tBox.MaxLength - tBox.Text.Length;
                if (maxLength <= 0) return;
                
                tBox.Text = tBox.Text.Insert(tBox.Character_CurrentIndex, Clipboard.Contents[..maxLength]);
                tBox.Character_CurrentIndex += maxLength;
                tBox.RequestRight(false);
            }
        }

        public override void Resize()
        {
            Size = Game.WindowSize;
            base.Resize();                    
        }

        public bool HasPriority()
            => form_Priority != null;

        internal void RemovePriority()
        {
            form_Priority = null;
        }

        #endregion
    }
}
