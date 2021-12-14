
namespace Lun.Controls
{
    using static LunEngine;
    public class Panel : Bond
    {
        /// <summary>
        /// Cor de fundo
        /// </summary>        
        public Color FillColor = new Color(96, 104, 146, 240);

        /// <summary>
        /// Arredondamento
        /// </summary>
        public int Radius = 6;

        /// <summary>
        /// Cor da borda
        /// </summary>
        public Color OutlineColor = new Color(200, 200, 200);

        /// <summary>
        /// Espessura da borda
        /// </summary>
        public int OutlineThickness = 1;

        public Panel(Bond bond) : base(bond)
        { }

        /// <summary>
        /// Desenha o painel
        /// </summary>
        public override void Draw()
        {
            DrawRoundedRectangle(GlobalPosition(), Size, FillColor, Radius, 8, OutlineThickness, OutlineColor);

            base.Draw();
        }
    }
}
