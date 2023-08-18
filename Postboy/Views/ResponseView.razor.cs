using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Postboy.Helpers;

namespace Postboy.Views
{
    public partial class ResponseView
    {
        [Parameter]
        public HttpResponseMessage? Value { get; set; }

        [Parameter]
        public bool Loading { get; set; }

        private string _status = string.Empty;

        protected override Task OnInitializedAsync()
        {
            Interaction.Subscribe<string>("RequestExecutionStatus", (msg) => InvokeAsync(() =>
            {
                _status = msg;
                StateHasChanged();
            }));
            return base.OnInitializedAsync();
        }

        private string? ResponseText
        {
            get
            {
                if (Value is null) return null;
                var val = Value.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                if (IsValidJson(val))
                {
                    return JsonHelper.FormatJson(val);
                }
                return val;
            }
            set
            {
                // immutable
            }
        }

        private bool IsValidJson(string str)
        {
            try
            {
                dynamic parsedJson = JsonConvert.DeserializeObject(str);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
