using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Notifications
{
    public interface INotification
    {
        object Content { get; }
        Color ForegroundColor { get; }
        Color BackgroundColor { get; }
    }
}