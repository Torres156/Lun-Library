
namespace Lun.Controls
{
    public class CheckButton : ControlBase
    {
        // Texto
        public string Text = "";                        // Texto
        public Color TextColor = Color.White;           // Cor do Texto
        public int TextSize = 11;                       // Tamanho do texto

        // Design
        public Color FillColor = new Color(56, 61, 87);         // Cor do Botão
        public Color FillColor_Hover = new Color(82, 89, 126);  // Cor do Botão : Hover
        public Color FillColor_Press = new Color(50, 54, 76);   // Cor do Botão : Press
        public float Radius = 6f;                               // Canto arredondado
        public uint CornerPoints = 8;                           // Pontos para arrendondar
        public int OutlineThickness = 1;                        // Espessura da borda
        public Color OutlineColor = new Color(200, 200, 200);   // Cor da borda
        public Texture Texture = null;                          // Textura
        public Color OutlineColor_Checked = Color.Red;          // Cor da Outline quando checado
        public int OutlineThickness_Checked = 1;                // Espessura da borda quando checado
        public bool IgnoreAutoChecked = false;                  // Ignora os checked


        public bool Checked
        {
            get => _checked;
            set
            {
                OnCheckedChanged?.Invoke(this);
                _checked = value;
            }
        }

        bool _checked = false;
        bool _hover;

        public event HandleCommon OnCheckedChanged;



        public CheckButton(Bond bond) : base(bond)
        { }

        public override void Draw()
        {
            var gp = GlobalPosition();

            if (_hover && !Hover())
                _hover = false;

            if (!_hover)
                DrawRoundedRectangle(gp, Size, FillColor, Radius, CornerPoints,
                    Checked ? OutlineThickness_Checked : OutlineThickness,
                    Checked ? OutlineColor_Checked : OutlineColor);
            else
                DrawRoundedRectangle(gp, Size, FillColor_Hover, Radius, CornerPoints,
                    Checked ? OutlineThickness_Checked : OutlineThickness,
                    Checked ? OutlineColor_Checked : OutlineColor);

            if (Texture != null)
                DrawTexture(Texture, new Rectangle(gp + new Vector2(2), new Vector2(Size.x - 4, Size.y - 4)), Color.White);

            var x = (Size.x - GetTextWidth(Text, (uint)TextSize)) / 2;
            var y = (Size.y - GetTextHeight(Text, (uint)TextSize)) / 2;
            DrawText(Text, TextSize, gp + new Vector2(x, y - 1), TextColor);
            base.Draw();
        }

        public override bool MouseMoved(Vector2 e)
        {
            _hover = false;

            if (Hover())
                _hover = true;

            return base.MouseMoved(e);
        }

        public override bool MouseReleased(MouseButtonEventArgs e)
        {
            if (_hover && !IgnoreAutoChecked)
                Checked = !Checked;

            return base.MouseReleased(e);
        }

    }
}
