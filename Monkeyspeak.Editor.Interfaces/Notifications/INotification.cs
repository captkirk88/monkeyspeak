using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Monkeyspeak.Editor.Interfaces.Notifications
{
    public interface INotification
    {
        object Content { get; }
    }
}