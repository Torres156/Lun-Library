namespace Lun.Controls
{
    public class Button : Bond
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


        bool _hover, _press;


        public Button(Bond bond) : base(bond)
        { }

        public override void Draw()
        {
            var gp = GlobalPosition();

            if (_hover && !Hover())
                _hover = false;

            if (!Hover())
                DrawRoundedRectangle(gp, Size, FillColor, Radius, CornerPoints, OutlineThickness, OutlineColor);
            else
            {
                if (!_press)
                    DrawRoundedRectangle(gp, Size, FillColor_Hover, Radius, CornerPoints, OutlineThickness, OutlineColor);
                else
                    DrawRoundedRectangle(gp, Size, FillColor_Press, Radius, CornerPoints, OutlineThickness, OutlineColor);
            }

            if (Texture != null)
                DrawTexture(Texture, new Rectangle(gp + new Vector2(2), new Vector2(Size.x - 4, Size.y - 4)), Color.White);

            var x = (Size.x - GetTextWidth(Text, (uint)TextSize)) / 2;
            var y = (Size.y - GetTextHeight(Text, (uint)TextSize)) / 2 ;
            DrawText(Text, TextSize, gp + new Vector2(x, y - 2), TextColor);
            base.Draw();
        }

        public override bool MouseMoved(Vector2 e)
        {
            _hover = false;

            if (Hover())
                _hover = true;

            return base.MouseMoved(e);
        }

        public override bool MousePressed(MouseButtonEventArgs e)
        {
            if (_hover)
                _press = true;

            return base.MousePressed(e);
        }

        public override bool MouseReleased(MouseButtonEventArgs e)
        {
            _press = false;
            return base.MouseReleased(e);
        }
    }
}
