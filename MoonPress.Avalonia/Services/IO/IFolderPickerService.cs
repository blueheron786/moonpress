using System.Threading.Tasks;

namespace MoonPress.Avalonia.Services.IO;

public interface IFolderPickerService
{
    Task<string?> ShowFolderSelectionDialogAsync();
}
