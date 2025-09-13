using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace roguishpanda.AB_Bauble_Farm
{
    public class ListSettingsTabView : View
    {
        private BaubleFarmModule _BaubleFarmModule;
        protected override void Build(Container buildPanel)
        {
            _BaubleFarmModule = BaubleFarmModule.ModuleInstance;

            Blish_HUD.Controls.Panel listSettingsPanel = new Blish_HUD.Controls.Panel
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.ContentRegion.Size.X + 500, buildPanel.ContentRegion.Size.Y + 535), // Match the panel to the content region
                Location = new Point(buildPanel.ContentRegion.Location.X, buildPanel.ContentRegion.Location.Y - 35), // Align with content region
                BackgroundTexture = BaubleFarmModule.ModuleInstance._asyncTimertexture
            };

            Blish_HUD.Controls.Label label = new Blish_HUD.Controls.Label
            {
                Text = "Under Construction",
                Font = GameService.Content.DefaultFont32,
                Size = new Point(400, 60),
                Location = new Point(425, 275),
                Parent = buildPanel
            };
        }
    }
}