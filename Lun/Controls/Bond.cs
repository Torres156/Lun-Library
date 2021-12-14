
namespace Lun.Controls
{
    using SFML.System;
    using SFML.Window;
    public abstract class Bond : ControlBase
    {
        #region Properties
        protected List<ControlBase> controls;
        protected List<Form> forms;
        internal ControlBase priority;
        protected FormDragged formDragged;
        #endregion

        #region Methods
        /// <summary>
        /// Construtor
        /// </summary>
        public Bond() : base()
        {
            controls = new List<ControlBase>();
            forms = new List<Form>();
        }

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="bond"></param>
        public Bond(Bond bond) : base(bond)
        {
            controls = new List<ControlBase>();
            forms = new List<Form>();
        }


        /// <summary>
        /// Encontra um controle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FindControl<T>() where T : ControlBase
            => (forms.Find(i => i is T) as T) ?? controls.Find(i => i is T) as T;

        /// <summary>
        /// Desenha os controles vinculados
        /// </summary>
        /// <param name="target"></param>
        /// <param name="states"></param>
        public override void Draw()
        {
            base.Draw();
            if (controls.Count > 0)
            {
                var count = controls.Count;
                for (int i = 0; i < count; i++)
                    if (controls[i] != null && controls[i] != priority && controls[i].Visible)
                       controls[i].Draw();
            }

            if (forms.Count > 0)
            {
                var count = forms.Count;
                for (int i = 0; i < count; i++)
                    if (forms[i] != null && forms[i].Visible)
                        forms[i].Draw();
            }

            priority?.Draw();

        }

        /// <summary>
        /// Mouse pressionado
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool MousePressed(MouseButtonEventArgs e)
        {
            var result = base.MousePressed(e);
            if (Hover())
            {
                if (formDragged.form != null) return true;

                if (priority != null && priority.MousePressed(e)) return true;

                if (forms.Count > 0)
                    for (int i = forms.Count - 1; i >= 0; i--)
                        if (forms[i] != null && forms[i].Visible && forms[i].MousePressed(e)) return true;

                if (controls.Count > 0)
                    for (int i = controls.Count - 1; i >= 0; i--)
                        if (controls[i] != null && controls[i] != priority && controls[i].Visible && controls[i].MousePressed(e)) return true;
            }

            return result;
        }

        /// <summary>
        /// Mouse quando solta
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool MouseReleased(MouseButtonEventArgs e)
        {
            var hover = base.MouseReleased(e);
            if (Hover())
            {
                if (formDragged.form != null)
                {
                    formDragged.form.Form_Dragged();
                    formDragged.form = null;
                    return true;
                }

                if (priority != null && priority.Visible && priority.MouseReleased(e)) return true;

                if (forms.Count > 0)
                    for (int i = forms.Count - 1; i >= 0; i--)
                        if (forms[i] != null && forms[i].Visible && forms[i].MouseReleased(e)) return true;

                if (controls.Count > 0)
                    for (int i = controls.Count - 1; i >= 0; i--)
                        if (controls[i] != null && controls[i] != priority && controls[i].Visible && controls[i].MouseReleased(e)) return true;

                TextBox.Focus = null;
            }

            return hover;
        }

        /// <summary>
        /// Movimento do Mouse
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool MouseMoved(Vector2 e)
        {
            var hover = base.MouseMoved(e);
            if (Hover())
            {
                if (formDragged.form != null)
                {
                    var newp = new Vector2(e.x, e.y) - formDragged.MousePosition;
                    SetPositionFormWithAnchor(formDragged, newp);
                    return true;
                }

                if (priority != null && priority.Visible && priority.MouseMoved(e)) return true;

                if (forms.Count > 0)
                    for (int i = forms.Count - 1; i >= 0; i--)
                        if (forms[i] != null && forms[i].Visible && forms[i].MouseMoved(e)) return true;

                if (controls.Count > 0)
                    for (int i = controls.Count - 1; i >= 0; i--)
                        if (controls[i] != null && controls[i] != priority && controls[i].Visible && controls[i].MouseMoved(e)) return true;

            }

            return hover;
        }

        /// <summary>
        /// Movimento do Mouse
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool MouseScrolled(MouseWheelScrollEventArgs e)
        {
            var hover = base.MouseScrolled(e);
            if (Hover())
            {
                if (formDragged.form != null) return true;

                if (priority != null && priority.Visible && priority.MouseScrolled(e)) return true;

                // Encontrar Scrolls
                var findScrolls = controls.FirstOrDefault(i => i is ScrollVertical);
                if (findScrolls != null && findScrolls.Visible && (findScrolls as ScrollVertical).CanScrollWithBond)
                {
                    (findScrolls as ScrollVertical).InternalMouseScrollWheel((int)e.Delta);
                    return true;
                }

                if (forms.Count > 0)
                    for (int i = forms.Count - 1; i >= 0; i--)
                        if (forms[i] != null && forms[i].Visible && forms[i].MouseScrolled(e)) return true;

                if (controls.Count > 0)
                    for (int i = controls.Count - 1; i >= 0; i--)
                        if (controls[i] != null && controls[i] != priority && controls[i].Visible && controls[i].MouseScrolled(e)) return true;

            }

            return hover;
        }

        /// <summary>
        /// Adiciona um controle ao vinculo
        /// </summary>
        /// <param name="control"></param>
        public void AddControl(ControlBase control)
        {
            if (control.GetType().Name == "Form" || control.GetType().BaseType.Name == "Form")
                forms.Add((Form)control);
            else
            {
                if (control is TextBox)
                    (control as TextBox).TabIndex = controls.Count(i => i is TextBox);

                controls.Add(control);                
            }
        }

        /// <summary>
        /// Prossegue para o proximo textbox
        /// </summary>
        /// <param name="index"></param>
        internal void TextBoxNext(int index)
        {
            if (controls.Any(i => (i as TextBox).CanTabNext && (i as TextBox).TabIndex > index))
            {
                (controls.Find(i => (i as TextBox).CanTabNext && (i as TextBox).TabIndex > index) as TextBox).SetFocus();
                return;
            }

            var find = (TextBox)controls.Find(i => (i as TextBox).CanTabNext);
            find?.SetFocus();
        }

        /// <summary>
        /// Seta um controle como prioridade
        /// </summary>
        /// <param name="control"></param>
        public void SetControlPriority(ControlBase control)
        {
            priority = control;
        }

        /// <summary>
        /// Remove o Foco
        /// </summary>
        /// <param name="control"></param>
        public void RemoveFocusForm(Form control)
        {
            if (forms.Count == 0) return;
            if (control == forms[0]) return;

            var lst = new List<Form>();
            lst.Add(control);
            foreach (var i in forms)
                if (i != control)
                    lst.Add(i); // Ultimo será o novo Foco

            forms = lst;
        }

        /// <summary>
        /// Define o foco de Janela
        /// </summary>
        /// <param name="control"></param>
        public void SetFocusForm(Form control)
        {
            if (forms.Count == 0) return;
            if (control == forms[forms.Count - 1]) return;

            var lst = new List<Form>();
            foreach (var i in forms)
                if (i != control) lst.Add(i);

            lst.Add(control);
            forms = lst;
        }

        /// <summary>
        /// Muda a posição conforme ancoragem
        /// </summary>
        /// <param name="formDragged"></param>
        /// <param name="value"></param>
        protected void SetPositionFormWithAnchor(FormDragged formDragged, Vector2 value)
        {
            var form = formDragged.form;
            switch (form.Anchor)
            {
                case Anchors.TopLeft: form.Position = value; break;
                case Anchors.TopCenter: form.Position = value - new Vector2((Size.x - form.Size.x) / 2, 0); break;
                case Anchors.TopRight: form.Position = new Vector2(Size.x - form.Size.x - value.x, value.y); break;
                case Anchors.Left: form.Position = new Vector2(value.x, value.y - (Size.y - form.Size.y) / 2); break;
                case Anchors.Right: form.Position = new Vector2(Size.x - form.Size.x - value.x, value.y - (Size.y - form.Size.y) / 2); break;
                case Anchors.BottomLeft: form.Position = new Vector2(value.x, Size.y - form.Size.y - value.y); break;
                case Anchors.BottomCenter: form.Position = new Vector2(value.x - (Size.x - form.Size.x) / 2, Size.y - form.Size.y - value.y); break;
                case Anchors.BottomRight: form.Position = new Vector2(Size.x - form.Size.x - value.x, Size.y - form.Size.y - value.y); break;
                case Anchors.Center: form.Position = value - (Size - form.Size) / 2; break;
            }
        }

        /// <summary>
        /// Movimenta o formulario
        /// </summary>
        /// <param name="form"></param>
        /// <param name="mousePosition"></param>
        public void SetDragForm(Form form, Vector2 mousePosition)
        {
            if (formDragged.form != null) return;
            formDragged = new FormDragged(form, mousePosition);
        }

        public override void Resize()
        {
            if (controls.Count > 0)
                foreach (var i in controls)
                    i.Resize();

            if (forms.Count > 0)
                foreach (var i in forms)
                    i.Resize();
        }

        #endregion
    }
}
