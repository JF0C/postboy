using AntDesign;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components;
using Postboy.Data;
using Postboy.Views;
using Microsoft.AspNetCore.Components.Web;

namespace Postboy.Pages
{
    public partial class RequestPage
    {
        [Parameter]
        public string? Name { get; set; }

        private bool _editTitle, _deleting;

        private StoredRequest? Request { get; set; }

        private HttpResponseMessage? Response { get; set; }

        private string? Responsetext { get; set; }

        private bool _waitingForResponse, _saving;

        private AddFolderToRequestModal? _addFolder;

        private readonly List<HttpMethod> Methods = new()
    {
        HttpMethod.Get,
        HttpMethod.Post,
        HttpMethod.Put,
        HttpMethod.Delete,
        HttpMethod.Options,
        HttpMethod.Patch,
        HttpMethod.Head,
        HttpMethod.Connect,
        HttpMethod.Trace
    };

        private readonly List<StoredRequestContentType> ContentTypes = new()
    {
        StoredRequestContentType.None,
        StoredRequestContentType.Json,
        StoredRequestContentType.FormEncoded
    };

        protected override Task OnInitializedAsync()
        {
            //_autoSaveTimer = new Timer(async (obj) => await SaveChanges(), new object(), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

            Navigation.LocationChanged += async (s, a) => await AutoSave(s, a);
            _deleting = false;
            return base.OnInitializedAsync();
        }

        private async Task AutoSave(object? sender, LocationChangedEventArgs args)
        {
            if (!_deleting && !args.Location.TrimEnd('/').EndsWith("Request") && !_saving)
            {
                _saving = true;
                await SaveChanges();
                _saving = false;
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            if (Name is null)
            {
                Request = new StoredRequest();
            }
            else
            {
                Request = await Storage.GetByName(Name);
            }
            Response = null;
        }

        private async Task SaveChanges()
        {
            try
            {
                await SaveChangesUnsafe();
            }
            catch (Exception ex)
            {
                Notification.Error(ex.Message);
            }
        }

        private async Task SaveChangesUnsafe()
        {
            if (Name is null)
            {
                await Storage.Create(Request);
                Navigation.NavigateTo($"/Request/{Request.Name}");
                Notification.Success("Request created!");
            }
            else
            {
                await Storage.Update(Request);
                Notification.Success("Changes saved!");
            }
            Interaction.Notify("menuchanged");
        }

        private async Task Duplicate(Guid id)
        {
            var request = await Storage.GetById(id);
            if (request is null)
            {
                Notification.Error("Error duplicating the request");
                return;
            }
            var newRequest = new StoredRequest
            {
                Name = $"{request.Name} copy",
                AutoHeaders = request.AutoHeaders.ToList(),
                Body = request.Body,
                ContentType = StoredRequestContentType.Deserialize(request.ContentType.ToString() ?? "None"),
                ContentTypeString = request.ContentTypeString,
                Headers = request.Headers.ToDictionary(kv => kv.Key, kv => kv.Value),
                Method = request.Method,
                Url = request.Url
            };
            await Storage.Create(newRequest);
            Navigation.NavigateTo($"Request/{newRequest.Name}");
        }

        private async Task Delete()
        {
            if (!await Modal.ConfirmAsync(new ConfirmOptions
            {
                Title = "Confirm Deletion",
                Content = $"Are you sure, that you want to delete request {Request.Name}",
                OkText = "Yes",
                CancelText = "No"
            }))
            {
                return;
            }
            try
            {
                _deleting = true;
                var name = Request.Name;
                await Storage.Delete(Request.Id);
                Navigation.NavigateTo("/");
                Notification.Success($"Deleted request {name}");
                Interaction.Notify("menuchanged");
            }
            catch (Exception ex)
            {
                Notification.Error(ex.Message);
            }
        }

        private async Task AddToFolder(Guid requestId)
        {
            if (_addFolder is null) return;
            _addFolder.Done = new EventCallback<Guid?>(this, async (Guid? folderId) =>
            {
                if (folderId is null) return;
                if (Name is null)
                {
                    await SaveChanges();
                }
                await Storage.AddRequestToFolder(folderId.Value, requestId);
                Interaction.Notify("menuchanged");
            });
            _addFolder.Show();
        }

        private async Task Execute()
        {
            _waitingForResponse = true;
            Response = null;
            await SaveChanges();
            try
            {
                Response = await Executor.Execute(Request);
                _waitingForResponse = false;
            }
            catch (Exception ex)
            {
                Notification.Error(ex.Message);
                _waitingForResponse = false;
            }
            finally
            {
                _waitingForResponse = false;
            }
        }

        private async Task OnRequestNameInput(KeyboardEventArgs args)
        {

        }

        private async Task EditTitle()
        {
            _editTitle = !_editTitle;
            if (!_editTitle)
            {
                try
                {
                    await SaveChangesUnsafe();
                    Navigation.NavigateTo($"Request/{Request.Name}");
                    Interaction.Notify("menuchanged");
                }
                catch (Exception ex)
                {
                    Notification.Error(ex.Message);
                    Request.Name = Name;
                    Navigation.NavigateTo($"Request/{Name}");
                }
            }
        }
    }
}
