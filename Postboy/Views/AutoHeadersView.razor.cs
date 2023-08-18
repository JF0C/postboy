using Microsoft.AspNetCore.Components;
using Postboy.Data;

namespace Postboy.Views
{
    public partial class AutoHeadersView
    {
        [Parameter]
        public List<AutoHeader>? Value { get; set; }

        [Parameter]
        public EventCallback<List<AutoHeader>> ValueChanged { get; set; }

        private List<StoredRequest>? _requests;

        private AutoHeader _newItem = new();

        private List<AutoHeaderRepresentation>? _autoHeaders;

        private class AutoHeaderRepresentation
        {
            public string Name { get; set; } = string.Empty;
            public Guid Guid { get; set; } = Guid.Empty;
        }

        protected override async Task OnInitializedAsync()
        {
            _autoHeaders = Executor.AutoHeaders.Select(a => new AutoHeaderRepresentation { Name = a.Name, Guid = a.Guid }).ToList();
            _requests = await Storage.GetAll();
            await base.OnInitializedAsync();
        }

        private void Changed()
        {
            ValueChanged.InvokeAsync(Value);
        }

        private void AddAutoRequest()
        {
            var newitem = new AutoHeader
            {
                RequestId = _newItem.RequestId,
                Type = _newItem.Type
            };
            _newItem = new();
            Value?.Add(newitem);
            ValueChanged.InvokeAsync(Value);
        }
        private void RemoveAutoHeader(AutoHeader header)
        {
            Value?.Remove(header);
            ValueChanged.InvokeAsync(Value);
        }
    }
}
