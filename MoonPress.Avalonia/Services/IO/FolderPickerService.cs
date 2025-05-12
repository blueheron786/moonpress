using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace MoonPress.Avalonia.Services.IO;

public class FolderPickerService : IFolderPickerService
{
    /// <summary>
    /// Shows a folder selection dialog to the user.
    /// The dialog is modal and will block the calling thread until the user selects a folder or cancels the dialog.
    /// The selected folder path is returned as a string.
    /// If the user cancels the dialog, null is returned.
    /// </summary>
    public async Task<string?> ShowFolderSelectionDialogAsync()
    {
        var window = (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        var folder = await new OpenFolderDialog
        {
            Title = "Select Folder for New Project"
        }.ShowAsync(window);

        if (string.IsNullOrWhiteSpace(folder))
        {
            return null;
        }

        return folder;
    }
}
