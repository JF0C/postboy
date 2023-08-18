using AntDesign;
using Microsoft.AspNetCore.Components;
using Postboy.Data;

namespace Postboy.Views
{
    public partial class FoldersView
    {
        [Parameter]
        public Folder? Folder { get; set; }

        [Parameter]
        public int Level { get; set; }

        private SubMenu? _subMenu;

        private EditFoldernameModal? _folderName;

        private AddRequestToFolderModal? _addRequest;

        private List<StoredRequest>? _requests;

        protected override async Task OnInitializedAsync()
        {
            _requests = await Storage.GetAll();
            await base.OnInitializedAsync();
        }

        private async Task EditTitle()
        {
            if (_folderName is null) return;
            _folderName.Done = new EventCallback<string>(this, async (string name) =>
            {
                if (name is null) return;
                await Storage.RenameFolder(Folder.Id, name);
                Interaction.Notify("menuchanged");
            });
            _folderName.Show(Folder.Name);
        }
        private async Task AddFolder(Guid folderId)
        {
            if (_folderName is null) return;
            _folderName.Done = new EventCallback<string>(this, async (string name) =>
            {
                if (name is null) return;
                await Storage.CreateFolder(folderId, name);
                Interaction.Notify("menuchanged");
            });
            _folderName.Show("new folder");
        }
        private async Task Delete(Guid folderId)
        {
            if (await Modal.ConfirmAsync(new ConfirmOptions
            {
                Title = "Delete Folder",
                Content = $"Are you sure, that you want to delete the folder {Folder?.Name} and all its contents?"
            }))
            {
                await Storage.DeleteFolder(Folder.Id);
                Interaction.Notify("menuchanged");
            }
        }
        private async Task AddRequest(Guid folderId)
        {
            if (_addRequest is null) return;
            _addRequest.Done = new EventCallback<Guid?>(this, async (Guid? id) =>
            {
                if (id is null) return;
                await Storage.AddRequestToFolder(folderId, id.Value);
                Interaction.Notify("menuchanged");
            });
            _addRequest.Show();
        }
        private async Task RemoveRequest(Guid folderId, Guid requestId)
        {
            await Storage.RemoveRequestFromFolder(folderId, requestId);
            Interaction.Notify("menuchanged");
        }
        private StoredRequest? GetById(Guid id)
        {
            return _requests?.FirstOrDefault(r => r.Id == id);
        }
    }
}
