namespace Lun.Controls
{
    using static LunEngine;
    using SFML.Graphics;
    using SFML.Window;
    using Color = Lun.Color;
    public class TextBox : ControlBase
    {
        #region Static
        /// <summary>
        /// Campo de texto com foco
        /// </summary>
        public static TextBox Focus
        {
            get
                => s_focus;
            set
            {
                s_focus?.Validate();
                s_focus = value;
            }
        }
        static TextBox s_focus = null;
        internal static bool s_animation = false;
        #endregion

        #region Properties
        /// <summary>
        /// Texto
        /// </summary>
        public string Text
        {
            get
            {
                string copyValue = "";

                copyValue = _text;
                return copyValue;
            }
            set
            {
                _text = value;
                if (Character_CurrentIndex > value.Length + 1) Character_CurrentIndex = value.Length;
                OnTextChanged?.Invoke(this);
            }
        }
        string _text = "";

        /// <summary>
        /// Pré texto
        /// </summary>
        public string pre_Text = "";

        /// <summary>
        /// Máximo de caracteres
        /// </summary>
        public int MaxLength = 0;

        /// <summary>
        /// Cor de fundo
        /// </summary>
        public Color FillColor = Color.White;

        /// <summary>
        /// Cor de fundo
        /// </summary>
        public Color TextColor = new Color(56, 61, 87);

        /// <summary>
        /// Espessura da borda
        /// </summary>
        public float OutlineThickness = 0;

        /// <summary>
        /// Cor da borda
        /// </summary>
        public Color OutlineColor = new Color(200, 200, 200);

        /// <summary>
        /// Modo senha
        /// </summary>
        public bool Password = false;

        /// <summary>
        /// Modo linhas multiplas
        /// </summary>
        public bool Multiple_Lines = false;

        /// <summary>
        /// Alinhamento do texto
        /// </summary>
        public TextAligns Align = TextAligns.Left;


        /// <summary>
        /// Escala da borda
        /// </summary>
        public float Radius = 4f;

        /// <summary>
        /// Bloqueia o campo de digitação
        /// </summary>
        public bool Blocked = false;

        /// <summary>
        /// Modo numérico
        /// </summary>
        public bool isNumeric = false;

        /// <summary>
        /// Valor mínimo
        /// </summary>
        public int Minimum = int.MinValue;

        /// <summary>
        /// Valor máximo
        /// </summary>
        public long Maximum = int.MaxValue;

        /// <summary>
        /// Atual caractere
        /// </summary>
        public int Character_CurrentIndex = 0;

        /// <summary>
        /// Sugestões
        /// </summary>
        public Dictionary<string, string[]> Suggestion;

        /// <summary>
        /// Modo arredondado
        /// </summary>
        public bool Rounded = true;

        /// <summary>
        /// Pode mudar de foco para o proximo
        /// </summary>
        public bool CanTabNext = true;

        /// <summary>
        /// Indice de tab
        /// </summary>
        public int TabIndex = 0;

        /// <summary>
        /// Valor
        /// </summary>
        public long Value
        {
            get => ToInt64();
            set
            {
                if (isNumeric) Text = value.ToString();
            }
        }

        RenderTexture render;
        #endregion

        #region Events
        public event HandleCommon OnValidate, OnEnter, OnTextChanged;
        #endregion

        #region Methods
        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="bond"></param>
        public TextBox(Bond bond) : base(bond)
        {
            Suggestion = new Dictionary<string, string[]>();            
        }

        /// <summary>
        /// Desenha o campo de texto
        /// </summary>
        /// <param name="target"></param>
        /// <param name="states"></param>
        public override void Draw()
        {
            Draw_Normal();

            base.Draw();
        }


        /// <summary>
        /// Desenha normalmente
        /// </summary>
        /// <param name="target"></param>
        void Draw_Normal()
        {
            var gp = GlobalPosition();
            float X = 0;

            // Fundo
            DrawRoundedRectangle(gp, Size, FillColor, Radius, 8, OutlineThickness, OutlineColor);


            // Pré texto
            if (!HasFocus() && Text.Trim().Length == 0)
            {
                switch (Align)
                {
                    case TextAligns.Left:
                        X = 4;
                        break;

                    case TextAligns.Center:
                        X = (Size.x - GetTextWidth(pre_Text)) / 2;
                        break;

                    case TextAligns.Right:
                        X = Size.x - 4 - GetTextWidth(pre_Text);
                        break;
                }

                DrawText(pre_Text, 11, gp + new Vector2(X, (Size.y / 2) - 7), TextColor - new Color(60, 60, 60, 0));
                return;
            }

            string display = Text;
            if (Password)
            {
                display = "";
                foreach (var i in Text)
                    display += "*";
            }

            // Calcula o texto aparecer
            bool useDiff = false;
            if (GetTextWidth(display) > Size.x - 8)
            {                
                string display2 = "";

                if (GetTextWidth( display.Substring(Character_CurrentIndex)) < Size.x - 8)
                {
                    for (int i = display.Length - 1; i > 0; i--)
                        if (GetTextWidth(display.Substring(i)) < Size.x - 8)
                            display2 = display.Substring(i);
                        else
                            break;
                    display = display2;
                }
                else
                {
                    var entered = display.Substring(Character_CurrentIndex);
                    for(int i = 0; i < entered.Length; i++)
                        if (GetTextWidth(entered.Substring(0, i)) < Size.x - 8)
                            display2 = entered.Substring(0, i);
                        else
                            break;
                    display = display2;
                    useDiff = true;
                }
            }

            // Desenha o texto
            switch (Align)
            {
                case TextAligns.Left:
                    X = 4;
                    break;

                case TextAligns.Center:
                    X = (Size.x - GetTextWidth(display)) / 2;
                    break;

                case TextAligns.Right:
                    X = Size.x - 4 - GetTextWidth(display);
                    break;
            }

            if (HasFocus() && s_animation)
            {
                if (Character_CurrentIndex > Text.Length) Character_CurrentIndex = Text.Length;
                
                int diff = Text.Length - display.Length;
                DrawText("|", 11, gp + new Vector2(X + (useDiff ? 0 : (Character_CurrentIndex == 0 ?
                    0 :
                    GetTextWidth(display.Substring(0, Character_CurrentIndex - diff)))),
                    (Size.y / 2) - 7), TextColor);
            }
            DrawText(display, 11, gp + new Vector2(X, (Size.y / 2) - 7), TextColor);
            if (HasFocus() && Text.Length > 0 && Suggestion.Count > 0) Draw_Suggestion();
        }

        public void AddSuggestion(string Name, string valueType, string desc)
            => Suggestion.Add(Name, new string[] { valueType, desc });

        /// <summary>
        /// Desenha as sugestões
        /// </summary>
        /// <param name="target"></param>
        void Draw_Suggestion()
        {
            var gp = GlobalPosition();

            var findKeys = Suggestion.ToList().FindAll(i => i.Key.Contains(Text) || (Text.Length >= i.Key.Length && i.Key.Contains(Text.Substring(0, i.Key.Length - 1))));
            int count = findKeys.Count;
            if (count > 0)
            {
                int h = 4;
                int w = 150;
                foreach (var i in findKeys)
                {
                    if (GetTextWidth(i.Key) + (i.Value[0].Length > 0 ? GetTextWidth($"({i.Value[0]})") + 8 : 4) > w) w = (int)GetTextWidth(i.Key) + (int)(i.Value[0].Length > 0 ? GetTextWidth($"({i.Value[0]})") + 8 : 4);
                    h += 14 + GetWordWrap(i.Value[1], w - 4).Length * 14 + 4;
                }

                var pos = gp + new Vector2(2, Size.y + 1);
                DrawRectangle(pos, new Vector2(w, h), TextColor, 1, OutlineColor);

                int off = 0;
                for (int i = 0; i < count; i++)
                {
                    DrawText(findKeys[i].Key, 11, pos + new Vector2(2, 2 + off), Color.White);
                    if (findKeys[i].Value[0].Length > 0)
                        DrawText($"({findKeys[i].Value[0]})", 11, pos + new Vector2(2 + GetTextWidth(findKeys[i].Key) + 4, 2 + off), FillColor);

                    var words = GetWordWrap(findKeys[i].Value[1], w - 4);
                    for (var x = 0; x < words.Length; x++)
                        DrawText(words[x], 11, pos + new Vector2(2, 2 + off + 14 + 14 * x), new Color(FillColor.R, FillColor.G, FillColor.B,(byte)150));

                    off += 14 + 14 * words.Length + 4;
                }
            }
        }

        /// <summary>
        /// Verifica se está com foco
        /// </summary>
        /// <returns></returns>
        public bool HasFocus()
            => Focus == this;

        /// <summary>
        /// Quando o Tab é pressionado
        /// </summary>
        public void TabPress()
        {
            if (CanTabNext)
                Bond?.TextBoxNext(TabIndex);
        }

        /// <summary>
        /// Quando o Enter é pressionado
        /// </summary>
        public void EnterPress()
        {
            OnEnter?.Invoke(this);
        }

        /// <summary>
        /// Coloca o foco
        /// </summary>
        public void SetFocus()
        {
            if (!Blocked)
            {
                if (Focus != this)
                {
                    Focus = this;
                    Character_CurrentIndex = Text.Length;
                }
            }
        }

        /// <summary>
        /// Valida o campo de texto
        /// </summary>
        public void Validate()
        {
            if (isNumeric)
            {
                if (!Text.IsNumeric())
                    Text = "0";

                int value = int.Parse(Text);
                if (value < Minimum) Text = Minimum.ToString();
                if (value > Maximum) Text = Maximum.ToString();
            }

            OnValidate?.Invoke(this);
        }

        /// <summary>
        /// Solta o clique
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool MouseReleased(MouseButtonEventArgs e)
        {
            var result = base.MouseReleased(e);
            if (Hover())
            {
                if (!HasFocus())
                {
                    SetFocus();
                }
            }
            return result;
        }

        public long ToInt64()
        {
            long value = _text.IsNumeric() ? long.Parse(_text) : Minimum;
            if (value < Minimum) value = Minimum;
            if (value > Maximum) value = Maximum;
            return value;
        }
        #endregion
    }

    public enum TextAligns
    {
        Left,

        Center,

        Right,
    }
}
